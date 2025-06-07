import { useEffect, useState } from "react";
import { subscribeToNotifications,Notification } from "../../../utils/notification-store";
import { getNotificationColor, NotificationType, orderStatusIndex } from "../../../utils/table-color";
import { placeOrderService } from "./place-orders.service";
import { authService } from "../../../utils/auth/auth.service";
import { UserRole } from "../../../utils/constants";
import { useTranslation } from "react-i18next";


export function NotificationScreen({ onClose }:{onClose?: (label: string, setOrdersAsPaid:boolean) => void}) {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const { i18n } = useTranslation();
  useEffect(() => {
    const unsubscribe = subscribeToNotifications((n) => {
        const regexStaff = /^Staff updated Order \d+ status to payment_requested\.$/;
        const regexGuest = /^Guest updated Order \d+ status to payment_requested\.$/;
        console.log(n);
        if(n.type === NotificationType.OrderStatusUpdated && (regexStaff.test(n.message) || regexGuest.test(n.message))){
          n.type = NotificationType.PaymentRequested;
        } 
        if(i18n.language === "hr"){
          n.message = n.message.replace("Waiter requested at table", "Potreban konobar na stolu");
          n.message = n.message.replace("Guest updated Order","Zatražen račun").replace("status to payment_requested","");
          n.message = n.message.replace("Guest have left table","Gosti napustili stol");
          n.message = n.message.replace("New guest at table","Novi gost na stolu");
        }
        console.log(n);
        if(i18n.language === "hr" && n.type === NotificationType.GuestJoinedTable)
          n.message = "Novi gost na stolu " + n.tableLabel;
      setNotifications((prev) => [...prev, n]);
    });

    return () => unsubscribe();
  }, []);

  const removeNotification = async(notification:Notification) => {
    setNotifications((prev) => prev.filter((n) => n.id !== notification.id));
    const label = notifications.find(not => not.id === notification.id)?.tableLabel;
    if (onClose && label) {
      if(notification.type == NotificationType.OrderCreated){
        onClose(label, false);
        await placeOrderService.updateOrderStatus(notification.orderId!, orderStatusIndex.delivered);
      }
      if(notification.type == NotificationType.PaymentRequested){
        onClose(label, true);
      }
      else
        onClose(label, false);
    }
  };

  return (
    <section id="notifications" className={`flex flex-col pb-8 md:pt-0  flex-start items-start w-full md:w-[350px]  ${authService.userRole() === UserRole.staff ? "pt-[100px] min-h-[700px] ":"md:max-h-[450px] min-h-[400px] border overflow-hidden mr-4  rounded-[30px]"}`}>
      <h3 className="text-lg font-bold mb-2 text-center bg-latte w-full border-b-3"><img className="m-auto" src="/assets/images/icons/notificationBell.svg" alt="notification bell"/></h3>
      <div className="flex flex-col gap-2 max-w-sm p-2 w-full">
      {notifications.map((n) => { if(n.pending && n.type != NotificationType.GuestLeftTable) return (
        <div
          key={n.id}
          className={`p-4 rounded shadow border text-black w-full  flex justify-between items-center ${getNotificationColor(n.type)}`}
        >
          <span className="text-black">{n.message}</span>
          <button
            className="ml-4 text-sm"
            onClick={() => removeNotification(n)}
          >
            <img width={"32px"} src="/assets/images/icons/checkMark.svg"/>
          </button>
        </div>
      )})}
    </div>
    </section>
    
  );
}
