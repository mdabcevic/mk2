import { Order } from "../../../../utils/interfaces/order";

interface MyOrdersModalProps {
  myOrders: Order[];
  onRequestPayment: () => void;
  onClose: () => void;
  visible: boolean;
}

const MyOrdersList: React.FC<MyOrdersModalProps> = ({
  myOrders,
}) => {
  return (
    <div>
      {
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
       }
    </div>
  );
};

export default MyOrdersList;
