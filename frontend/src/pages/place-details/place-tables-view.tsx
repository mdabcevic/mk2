import { useEffect, useState } from "react";
import { placeService } from "../../utils/services/place.service";
import { Table, TablePublic, TableStatus, TableStatusString } from "../../utils/constants";
import TableActionModal from "../../utils/table-actions-modal";
import { tableService } from "../../utils/services/tables.service";
import QRCode from "qrcode";
import jsPDF from "jspdf";
import { useTranslation } from "react-i18next";
const initial_div_width = 750;
const initial_div_height = 550;

const URL_QR = "http://localhost:5173/table-lookup/{placeId}/{salt}"

const PlaceTablesViewPublic = () => {
  const placeId = 1;
  const [tables, setTables] = useState<Table[]>([]);
  const [scale, setScale] = useState(1);
  const [selectedTable, setSelectedTable] = useState<Table | null>(null);
  const { t } = useTranslation("admin");


  const fetchTables = async () => {
    const response = await tableService.getPlaceTablesByCurrent();
    setTables(response);
  };


  const calculateScale = () => {
    const screenWidth = window.innerWidth * 0.95; 
    const screenHeight = window.innerHeight * 0.85;
    const scaleX = screenWidth / initial_div_width;
    const scaleY = screenHeight / initial_div_height;
    const finalScale = Math.max(Math.min(scaleX, scaleY), 0.7);
    setScale(finalScale);
  };

  useEffect(() => {
    fetchTables();
    calculateScale();
    window.addEventListener("resize", calculateScale);
    return () => window.removeEventListener("resize", calculateScale);
  }, []);

  const getBackgroundColor = (status: string) => {
    if (status === TableStatusString.empty) return "green";
    if (status === TableStatusString.occupied) return "red";
    return "gray";
  };

  const handleSetStatus = async (status: TableStatusString) => {
    const response = await tableService.changeStatus(status,selectedTable?.token!);
    setTables((prevTables:any) => {
      return prevTables.map((table:any) =>
        table.label === selectedTable?.label ? { ...table, status } : table
      );
    });
    setSelectedTable(null);
  };

  const generateQrCode = async() =>{
    const newSalt = await tableService.regenrateQrCode(selectedTable?.label!);
    const qrCodeValue = URL_QR.replace("{placeId}",placeId.toString()).replace("{salt}",newSalt);
    try {
      const qrDataUrl = await QRCode.toDataURL(qrCodeValue);
  
      const doc = new jsPDF();
      const tableLabel = selectedTable?.label || "table";
  
      doc.setFontSize(16);
      doc.text(`${t("qr_code_message")}: ${tableLabel}`, 20, 20);
      doc.addImage(qrDataUrl, "PNG", 20, 30, 100, 100);
  
      doc.save(`qr_${tableLabel}.pdf`);
    } catch (err) {
      console.error("Error generating QR code PDF:", err);
      alert("Error")
    }
  }

  return (
    <div
      className="flex justify-center items-center w-full h-full"
      style={{ padding: "16px" }}
    >
      <div
        style={{
          width: `${initial_div_width}px`,
          height: `${initial_div_height}px`,
          backgroundImage: "url(/assets/images/place_view.png)",
          backgroundSize: "contain",
          backgroundRepeat: "no-repeat",
          position: "relative",
          transform: `scale(${scale})`,
          transformOrigin: "top left",
        }}
      >
        {tables.map((table, index) => (
          <div
            key={index}
            style={{
              position: "absolute",
              left: table.x,
              top: table.y,
              width: table.width,
              height: table.height,
              backgroundColor: getBackgroundColor(table.status),
              borderRadius: `${Math.min(table.width, table.height) / 2}px`,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              color: "white",
              fontWeight: "bold",
              border: "1px solid black",
              boxSizing: "border-box",
            }}
            onClick={() => setSelectedTable(table)}
          >
            {table.label}
            {selectedTable?.label === table.label && (
            <TableActionModal
            tableLabel={table.label}
            onClose={() => setSelectedTable(null)}
            onSetStatus={handleSetStatus}
            onGenerateQR={() => generateQrCode()}
          />
          )}
          </div>
        ))}
      </div>
    </div>
  );
};

export default PlaceTablesViewPublic;
