import { ToastType } from "./toast";

export type Notification = {
  id: string;
  message: string;
  type: ToastType;
};

type Listener = (n: Notification) => void;

let listeners: Listener[] = [];

export const notifyListeners = (notification: Notification) => {
  listeners.forEach((l) => l(notification));
};

export const subscribeToNotifications = (listener: Listener) => {
  listeners.push(listener);
  return () => {
    listeners = listeners.filter((l) => l !== listener);
  };
};
