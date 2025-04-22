export interface Notification {
  id: string;
  message: string;
  orderId: number | null;
  pending: boolean;
  tableLabel: string;
  timestamp: string;
  type: number;
}

type NotificationListener = (notification: Notification) => void;

const listeners: NotificationListener[] = [];

export const notifyListeners = (notification: Notification) => {
  listeners.forEach((listener) => listener(notification));
};

export const subscribeToNotifications = (listener: NotificationListener) => {
  listeners.push(listener);
  return () => {
    const index = listeners.indexOf(listener);
    if (index > -1) listeners.splice(index, 1);
  };
};
