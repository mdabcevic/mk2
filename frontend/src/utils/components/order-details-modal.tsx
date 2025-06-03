import React from "react";
import { Order } from "../interfaces/order";
import { useTranslation } from "react-i18next";

type Props = {
  open: boolean;
  order: Order | null;
  onClose: () => void;
};

const OrderDetailsModal: React.FC<Props> = ({ open, order, onClose }) => {
  const { t } = useTranslation("admin");
  if (!open || !order) return null;

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white rounded-[20px] shadow-lg w-full max-w-[375px] border border-[#A3A3A3]">
        <div className="relative bg-[#FAFAFA] rounded-[20px] text-[#A3A3A3] flex justify-between items-start w-full pl-4 pt-4 pb-0 pr-4">
          <h3 className="text-lg font-bold mb-4 flex flex-col">
            <span>{t("orderStatus.order")} #{order.id}</span>
            <span className="text-sm">{t("orderStatus.table")}: {order.table}</span>
          </h3>
          <button onClick={onClose}>
            <img src="/assets/images/icons/close_icon.svg" alt="close" />
          </button>
        </div>

        <div className="pl-7 pb-3 mt-8">
            <p className="mt-2">
              <span className="font-bold">{t("orderStatus.total_price")}:</span> €{order.totalPrice.toFixed(2)}
            </p>
            <p className="mt-2">
              <span className="font-bold">{t("orderStatus.payment_type")}:</span> {order.paymentType}
            </p>
            <p className="mt-2 mb-2">
              <span className="font-bold">{t("orderStatus.status")}:</span> {order.status.replace("_", " ")}
            </p>

            <div>
              <h4 className="font-semibold">{t("orderStatus.items")}:</h4>
              <ul className="list-disc list-inside text-[14px] pl-2">
                {order.items.map((item, i) => (
                  <li key={i}>
                    {item.count} × {item.menuItem} - €{(item.price * item.count).toFixed(2)}
                  </li>
                ))}
              </ul>
            </div>

            <p className="mt-2">
              <span className="font-bold">{t("orderStatus.note")}:</span> {order.note}
            </p>
          </div>
      </div>
    </div>
  );
};

export default OrderDetailsModal;
