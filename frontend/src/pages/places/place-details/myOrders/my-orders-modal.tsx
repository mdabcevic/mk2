import { Order } from "../../../../utils/interfaces/order";
import { useTranslation } from "react-i18next";

interface MyOrdersModalProps {
  myOrders: Order[];
  onRequestPayment: () => void;
  onClose: () => void;
  visible: boolean;
}

const MyOrdersModal: React.FC<MyOrdersModalProps> = ({
  myOrders,
  onRequestPayment,
  onClose,
  visible,
}) => {
  const { t } = useTranslation("public");

  return (
    <div
      className={`fixed inset-0 bg-white z-50 p-4 transition-all duration-500 overflow-y-auto ${
        visible ? "opacity-100 scale-100" : "opacity-0 scale-90 pointer-events-none"
      }`}
    >
      <button onClick={onClose} className="mb-4 text-mocha-600 font-semibold underline flex items-center">
        {t("back").toUpperCase()}
      </button>

      <h4 className="text-md font-bold border-b pb-2 mb-4">{t("my_orders")}</h4>

      {myOrders.length > 0 ? (
        myOrders.map((order, index) => (
          <div key={order.id} className="mb-6 border-b w-full">
            <p className="text-[16px] font-semibold mb-4 text-black">
              Order {index + 1} ({order.status.toUpperCase()}) - {order.totalPrice.toFixed(2)}€
            </p>
            {order.note && (
              <p className="text-sm mb-2 whitespace-pre-line text-[14px]">
                <span className="font-bold">Note:</span> {order.note}
              </p>
            )}
            {order.items.map((item, idx) => (
              <div key={idx} className="flex-column pt-2 pb-2 pl-6 neutral-latte border b-white rounded-[30px] text-[14px] mb-6" >
                <p className="color-mocha-600 font-semibold">{item.menuItem} (x{item.count})</p>
                <span className="font-normal">{(item.price * item.count).toFixed(2)}€</span>
              </div>
            ))}
          </div>
        ))
      ) : (
        <p className="text-center">{t("sm_message")}</p>
      )}

      {myOrders.length > 0 && (
        <div className="fixed bottom-2 left-0 w-full flex flex-col items-center">
          <button onClick={onRequestPayment} className="px-6 py-3 rounded-[40px] bg-mocha-600 text-white mb-4 w-64">
            {t("request_payment").toUpperCase()}
          </button>
        </div>
      )}
    </div>
  );
};

export default MyOrdersModal;
