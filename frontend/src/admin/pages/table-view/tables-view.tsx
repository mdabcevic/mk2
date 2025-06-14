import { useEffect, useState } from "react";
import QRCode from "qrcode";
import jsPDF from "jspdf";
import { useTranslation } from "react-i18next";
import { Constants, Table, TableStatusString } from "../../../utils/constants";
import { tableService } from "../../../utils/services/tables.service";
import { getBgColorByNotificationStatus, getTableColor, getTableIcon, NotificationType, orderStatusIndex } from "../../../utils/table-color";
import OrdersTable from "./orders-table";
import { NotificationScreen } from "./notifications";
import { subscribeToNotifications,Notification } from "../../../utils/notification-store";
import TableActionModal from "../../../utils/components/table-actions-modal";
import { orderService } from "../../../pages/places/menu/order.service";
import OrdersByTableModal, { Order } from "../../../utils/components/orders-by-table-modal";
import { placeOrderService } from "./place-orders.service";
import { placeService } from "../../../utils/services/place.service";
import { authService } from "../../../utils/auth/auth.service";
import { ImageType } from "../../../utils/interfaces/place-item";

const initial_div_width = Constants.create_tables_container_width;
const initial_div_height = Constants.create_tables_container_height;
const URL_QR = Constants.url_qr;
const blueprintDefault = `/${Constants.template_image}`;

const TablesView = () => {
  const placeId = authService.placeId();
  const [tables, setTables] = useState<Table[]>([]);
  const [selectedTable, setSelectedTable] = useState<Table | null>(null);
  const { t } = useTranslation("admin");
  const [rerenderOrdersFlag, setRerenderOrdersFlag] = useState<number>(1);
  const [ordersByTable, setOrdersByTables] = useState<Order[] |null>(null);
  const [manageTables, setManageTables] = useState<boolean>(false);
  const [blueprint, setBlueprint] = useState<string | null>(null);

  const fetchTables = async (notification?:Notification) => {
    setTables([]);
    const response = await tableService.getPlaceTablesByCurrentUser();
    const result = response.map(table => {
      if (notification && table.label === notification.tableLabel) {
      if(notification.type === NotificationType.PaymentRequested)
          return { ...table, requestType: notification.type};
        else if(notification.type === NotificationType.GuestJoinedTable)
          return {...table, requestType: NotificationType.StaffNeeded}
        else if(notification.type === NotificationType.GuestLeftTable)
          return {...table}
        else if(notification.pending) return { ...table, requestType: notification.type};
        else return {...table}
      }
      return table;
    });
    setTables(result);
  };

  const getPlaceDetails = async () => {
    let place = await placeService.getPlaceDetailsById(Number(placeId));
    let blueprintsList = place?.images?.find(img => img.imageType === ImageType.blueprint)?.urls ?? [];
    setBlueprint(blueprintsList?.length > 0 ? blueprintsList[0] : blueprintDefault);
  };

  useEffect(() => {
    fetchTables();
    getPlaceDetails();
  }, []);

  useEffect(() => {
      const unsubscribe = subscribeToNotifications((n) => {
        fetchTables(n);
        setRerenderOrdersFlag(prev => prev+1);
      });
  
      return () => unsubscribe();
    }, []);
    
  const setNewStatus = async (status: TableStatusString) => {
    await tableService.changeStatus(status,selectedTable?.token!);
    setTables((prevTables:any) => {
      return prevTables.map((table:any) =>
        table.label === selectedTable?.label ? { ...table, status } : table
      );
    });
    setSelectedTable(null);
  };

  const fetchOrdersByTable = async (tableLabel:string) =>{
    const response = await orderService.getOrdersByTable(tableLabel);
    setOrdersByTables(response);
  }

  const generateQrCode = async() =>{
    const newSalt = await tableService.regenrateQrCode(selectedTable?.label!);
    const qrCodeValue = URL_QR.replace("{placeId}",placeId.toString()).replace("{salt}",newSalt);
    try {
      const qrDataUrl = await QRCode.toDataURL(qrCodeValue);
  
      const doc = new jsPDF();
      const tableLabel = selectedTable?.label || "table";
  
      doc.setFontSize(16);
      doc.text(`${t("qr_code_message")}: ${tableLabel}`, 20, 20);
      doc.addImage(qrDataUrl, "PNG", 20, 30, 50, 50);
  
      doc.save(`qr_${tableLabel}.pdf`);
      fetchTables();
    } catch (err) {
      console.error("Error generating QR code PDF:", err);
      alert("Error")
    }
  }

  const disableTable = async (tableLabel: string) => {
    await tableService.disableTable(tableLabel,true);
    fetchTables();
  };

  const enableTable = async (tableLabel: string) => {
    await tableService.disableTable(tableLabel,false);
    fetchTables();
  };

  const onClose = async (label: string, setOrdersAsPaid:boolean) => {
    setTables(prev =>
      prev.map(table => {
        if (table.label === label) {
          const { requestType, ...rest } = table;
          return rest;
        }
        return table;
      })
    );
    if(!setOrdersAsPaid) return;

    const response = await orderService.getOrdersByTable(label);
    if(response?.length > 0)
    response.forEach(async order => {
      await placeOrderService.updateOrderStatus(order.id!, orderStatusIndex.paid);
    })   
  };

  return (
    <div className="relative">
      <section className="hidden lg:flex justify-center items-start  w-full h-full p-[16px] pt-[80px]">
      <div className="flex flex-col items-center space-x-4 absolute right-0 top-0">
        <span>{t("manage_tables")}:</span>
        <div
          role="button"
          onClick={() => setManageTables(!manageTables)}
          className="relative mt-2 w-14 h-6 bg-[#DFD8CD] rounded-full cursor-pointer transition-colors duration-300"
          style={{ backgroundColor: manageTables ? "#7E5E44" : "#DFD8CD" }}
        >
          <div
            className={`absolute w-6 h-6 bg-white rounded-full border shadow-md transition-all duration-300 ${ manageTables ? "translate-x-8" : "" }`}
          ></div>
        </div>
      </div>
        <NotificationScreen onClose={onClose} />
        <div
          style={{
            width: `${initial_div_width}px`,
            height: `${initial_div_height}px`,
            backgroundImage: `url(${blueprint})`,
            backgroundSize: "contain",
            backgroundRepeat: "no-repeat",
            position: "relative",
            zIndex:"1"
          }}
        >
          {tables?.length > 0  && tables.map((table, index) => (
            
            <div
              key={index}
              className={'absolute cursor-pointer flex items-center justify-center font-bold border border-black box-border'}
              style={{
                left: table.x,
                top: table.y,
                width: table.width,
                height: table.height,
                backgroundColor: ((table?.requestType ?? -1) >=0) ? getBgColorByNotificationStatus(table.requestType!): getTableColor(table.status, "admin")  ,
                borderRadius: `${Math.min(table.width, table.height) / 2}px`,             
              }}
              onClick={() => {setSelectedTable(table); fetchOrdersByTable(table.label); }}
            >
              {table.label}
              {selectedTable?.label === table.label && manageTables &&  (
              <TableActionModal
                tableLabel={table.label}
                isDisabled={table.isDisabled!}
                onClose={() => setSelectedTable(null)}
                onSetStatus={setNewStatus}
                onGenerateQR={() => generateQrCode()}
                disable={() => disableTable(table.label)}
                enable={()=> enableTable(table.label)}
              />
              )}

            {ordersByTable && !manageTables && selectedTable?.label === table.label &&(
              <OrdersByTableModal orders={ordersByTable} onClose={() => {setOrdersByTables(null); setSelectedTable(null);}} />
            )}
            {typeof table?.requestType === "number" && (
              <div className="absolute bottom-4">
                <img src={getTableIcon(table?.requestType)} width="30px" height="30px" className="rounded" style={{animation: 'floatUpDown 1s ease-in-out infinite'}} alt="icon" />
              </div>
            )}
            </div>
          ))}
        </div>
      </section>

      <section>
        <OrdersTable key={rerenderOrdersFlag} rerender={rerenderOrdersFlag} showStatus={true} />
      </section>
    </div>
    
  );
};

export default TablesView;
