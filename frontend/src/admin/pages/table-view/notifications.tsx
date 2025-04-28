import { useEffect, useState } from "react";
import { subscribeToNotifications,Notification } from "../../../utils/notification-store";
import { getNotificationColor } from "../../../utils/table-color";


export function NotificationScreen({ onClose }:{onClose?: (label: string) => void}) {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  useEffect(() => {
    const unsubscribe = subscribeToNotifications((n) => {
      setNotifications((prev) => [...prev, n]);
    });

    return () => unsubscribe();
  }, []);

  const removeNotification = (id: string) => {
    setNotifications((prev) => prev.filter((n) => n.id !== id));
    const label = notifications.find(not => not.id === id)?.tableLabel;
    if (onClose && label) {
      onClose(label);
    }
  };

  return (
    <section id="notifications" className="flex flex-col flex-start items-start mr-4 w-full md:w-[350px] ">
      <h3 className="text-lg font-bold mb-2">Notifications</h3>
      <div className="flex flex-col gap-2 max-w-sm">
      {notifications.map((n) => (
        <div
          key={n.id}
          className={`p-4 rounded shadow  flex justify-between items-center ${getNotificationColor(n.type)}`}
        >
          <span>{n.message}</span>
          <button
            className="ml-4 text-sm text-white hover:text-gray-200"
            onClick={() => removeNotification(n.id)}
          >
            Close
          </button>
        </div>
      ))}
    </div>
    </section>
    
  );
}
