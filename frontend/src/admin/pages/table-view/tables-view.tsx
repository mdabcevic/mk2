import { useEffect, useState } from "react";
import QRCode from "qrcode";
import jsPDF from "jspdf";
import { useTranslation } from "react-i18next";
import { Constants, Table, TableStatusString } from "../../../utils/constants";
import { tableService } from "../../../utils/services/tables.service";
import { getTableColor } from "../../../utils/table-color";
import TableActionModal from "../../../utils/table-actions-modal";
import OrdersTable from "./orders-table";
import { NotificationScreen } from "./notifications";

const initial_div_width = Constants.create_tables_container_width;
const initial_div_height = Constants.create_tables_container_height;
const URL_QR = Constants.url_qr;

const TablesView = () => {
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
    const screenWidth = window.innerWidth; 
    const screenHeight = window.innerHeight;
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
    <div>
      <section className="flex justify-center items-start w-full h-full p-[16px]">
        <NotificationScreen />
        <div
          style={{
            width: `${initial_div_width}px`,
            height: `${initial_div_height}px`,
            backgroundImage: `url(/${Constants.template_image})`,
            backgroundSize: "contain",
            backgroundRepeat: "no-repeat",
            position: "relative",
          }}
        >
          {tables.map((table, index) => (
            <div
              key={index}
              className="absolute cursor-pointer flex items-center justify-center text-white font-bold border border-black box-border"
              style={{
                left: table.x,
                top: table.y,
                width: table.width,
                height: table.height,
                backgroundColor: getTableColor(table.status),
                borderRadius: `${Math.min(table.width, table.height) / 2}px`,
                
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
      </section>

      <section>
        <OrdersTable />
      </section>
    </div>
    
  );
};

export default TablesView;
