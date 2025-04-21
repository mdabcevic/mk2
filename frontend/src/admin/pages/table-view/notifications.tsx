import { useEffect, useState } from "react";
import { subscribeToNotifications,Notification } from "../../../utils/notification-store";
import { ToastType } from "../../../utils/toast";


export function NotificationScreen() {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  useEffect(() => {
    const unsubscribe = subscribeToNotifications((n) => {
      setNotifications((prev) => [...prev, n]);
    });

    return () => unsubscribe();
  }, []);

  const removeNotification = (id: string) => {
    setNotifications((prev) => prev.filter((n) => n.id !== id));
  };

  return (
    <div className="flex flex-col gap-2 max-w-sm">
      {notifications.map((n) => (
        <div
          key={n.id}
          className={`p-4 rounded shadow text-white flex justify-between items-center ${
            n.type === ToastType.success
              ? "bg-green-600"
              : n.type === ToastType.error
              ? "bg-red-600"
              : "bg-blue-600"
          }`}
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
  );
}
