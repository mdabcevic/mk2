import { useEffect, useState } from "react";
import { subscribeToNotifications,Notification } from "../../../utils/notification-store";
import { getNotificationColor, NotificationType, orderStatusIndex } from "../../../utils/table-color";
import { placeOrderService } from "./place-orders.service";


export function NotificationScreen({ onClose }:{onClose?: (label: string) => void}) {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  useEffect(() => {
    const unsubscribe = subscribeToNotifications((n) => {
      setNotifications((prev) => [...prev, n]);
    });

    return () => unsubscribe();
  }, []);

  const removeNotification = async(notification:Notification) => {
    setNotifications((prev) => prev.filter((n) => n.id !== notification.id));
    const label = notifications.find(not => not.id === notification.id)?.tableLabel;
    if (onClose && label) {
      onClose(label);
      if(notification.type == NotificationType.OrderCreated){
        await placeOrderService.updateOrderStatus(notification.orderId!, orderStatusIndex.delivered);
      }
    }
  };

  return (
    <section id="notifications" className="flex flex-col pb-8 pt-[100px] md:pt-0 border flex-start rounded-[30px] min-h-[400px] items-start mr-4 w-full md:w-[350px] md:max-h-[450px] overflow-hidden">
      <h3 className="text-lg font-bold mb-2 text-center bg-latte w-full border-b-3"><img className="m-auto" src="/assets/images/icons/notificationBell.svg" alt="notification bell"/></h3>
      <div className="flex flex-col gap-2 max-w-sm p-2 w-full">
      {notifications.map((n) => (
        <div
          key={n.id}
          className={`p-4 rounded shadow border text-black w-full  flex justify-between items-center ${getNotificationColor(n.type)}`}
        >
          <span className="text-black">{n.message}</span>
          <button
            className="ml-4 text-sm"
            onClick={() => removeNotification(n)}
          >
            <img src="/assets/images/icons/checkMark.svg"/>
          </button>
        </div>
      ))}
    </div>
    </section>
    
  );
}
