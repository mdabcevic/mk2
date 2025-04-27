import { useEffect, useState } from "react";
import QRCode from "qrcode";
import jsPDF from "jspdf";
import { useTranslation } from "react-i18next";
import { Constants, Table, TableStatusString } from "../../../utils/constants";
import { tableService } from "../../../utils/services/tables.service";
import { getTableColor, getTableIcon, NotificationType } from "../../../utils/table-color";
import TableActionModal from "../../../utils/table-actions-modal";
import OrdersTable from "./orders-table";
import { NotificationScreen } from "./notifications";
import { subscribeToNotifications,Notification } from "../../../utils/notification-store";

const initial_div_width = Constants.create_tables_container_width;
const initial_div_height = Constants.create_tables_container_height;
const URL_QR = Constants.url_qr;

const TablesView = () => {
  const placeId = 1;
  const [tables, setTables] = useState<Table[]>([]);
  const [selectedTable, setSelectedTable] = useState<Table | null>(null);
  const { t } = useTranslation("admin");
  const [rerenderOrdersFlag, setRerenderOrdersFlag] = useState<boolean>(false);

  const fetchTables = async (notification?:Notification) => {
    const response = await tableService.getPlaceTablesByCurrent();
    const tables = response.map(table => {
      if (notification &&(notification.type === NotificationType.OrderCreated || notification.type === NotificationType.StaffNeeded || notification.type === NotificationType.OrderStatusUpdated) && table.label === notification.tableLabel) {
        const regex = /^Staff updated Order \d+ status to payment_requested\.$/;
        if(notification.type === NotificationType.OrderStatusUpdated && regex.test(notification.message))
        return { ...table, requestType: notification.type };
      }
      return table;
    });
    setTables(tables);
  };

  useEffect(() => {
    fetchTables();
  }, []);

  useEffect(() => {
      console.log("cc")
      const unsubscribe = subscribeToNotifications((n) => {
        console.log("dosla notf")
        fetchTables(n);
        setRerenderOrdersFlag(!rerenderOrdersFlag);
      });
  
      return () => unsubscribe();
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

  const onClose = (label: string) => {
    setTables(prev =>
      prev.map(table => {
        if (table.label === label) {
          const { requestType, ...rest } = table;
          return rest;
        }
        return table;
      })
    );
  };

  return (
    <div>
      <section className="hidden lg:flex justify-center  w-full h-full p-[16px]">
        <NotificationScreen onClose={onClose} />
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
            {typeof table?.requestType === "number" && (
              <div className="absolute top-2">
                <img src={getTableIcon(table?.requestType)} width="40px" height="40px" className="bg-white rounded"
                     style={{animation: 'floatUpDown 1s ease-in-out infinite'}}/>
              </div>
            )}
            </div>
          ))}
        </div>
      </section>

      <section>
        <OrdersTable rerender={rerenderOrdersFlag} />
      </section>
    </div>
    
  );
};

export default TablesView;
