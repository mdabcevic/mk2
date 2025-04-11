import React, { useEffect } from "react";
import { createRoot } from "react-dom/client";

export enum ToastType{
    success="success",
    error="error",
    info="info"
}

type ToastProps = {
  message: string;
  type?: "success" | "error" | "info";
  onClose?: () => void;
};

const Toast = ({ message, type = "info", onClose }: ToastProps) => {
  useEffect(() => {
    const timer = setTimeout(() => {
      onClose?.();
    }, 2000);
    return () => clearTimeout(timer);
  }, []);

  return (
    <div
      className={`fixed top-15 right-5 z-[10000] px-4 py-2 rounded shadow text-white min-w-[200px] transition-opacity duration-500 ${
        type === ToastType.success
          ? "bg-green-500"
          : type === ToastType.error
          ? "bg-red-500"
          : "bg-blue-500"
      }`}
    >
      {message}
    </div>
  );
};

export const showToast = (
  message: string,
  type: "success" | "error" | "info" = "info"
) => {
  const container = document.createElement("div");
  document.body.appendChild(container);

  const root = createRoot(container);

  const removeToast = () => {
    root.unmount();
    container.remove();
  };

  root.render(<Toast message={message} type={type} onClose={removeToast} />);
};
