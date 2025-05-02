import { useState } from "react";

export interface Order {
  id: number;
  totalPrice: number;
  createdAt: string;
  paymentType: string;
  status:string;
  note: string;
  items:OrderMenuItem[];
}
interface OrderMenuItem{
  menuItem:string;
  count:number;
}
interface OrdersByTable {
  orders: Order[];
  onClose: () => void;
}

const OrdersByTableModal = ({ orders, onClose }: OrdersByTable) => {
  const [currentOrderIndex, setCurrentOrderIndex] = useState(0);

  if (orders.length === 0) return null;

  const currentOrder = orders[currentOrderIndex];

  const next = () => {
    setCurrentOrderIndex((prev) => (prev + 1) % orders.length);
  };

  const prev = () => {
    if(currentOrderIndex > 0)
      setCurrentOrderIndex((prev) => (prev - 1) % orders.length);
    else
    setCurrentOrderIndex(orders.length - 1);
  };

  return (
    <div className="absolute z-50 text-sm rounded-[40px] shadow p-3 right-20 bottom-50 w-[375px]">
      <div className="absolute flex items-center justify-center z-50 w-full" onClick={onClose}>
      <div
        className="bg-white rounded-[20px] shadow-lg w-full border border-[#A3A3A3]"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="relative bg-[#FAFAFA] rounded-t-[20px] text-[#A3A3A3] w-full flex justify-between items-start p-4">
          <div className="flex flex-col w-full">
            <h3 className="text-lg font-bold">
              Order #{currentOrder.id}
            </h3>
            <div className="text-sm flex items-center gap-2 justify-center">
            <button
                onClick={(e) => {
                  e.stopPropagation();
                  prev();
                }}
                className="text-xs mr-6"
              >
                prev
              </button>
              <span>{currentOrderIndex + 1}/{orders.length}</span>
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  next();
                }}
                className="text-xs ml-6"
              >
                next
              </button>
            </div>
          </div>
          <button onClick={onClose} className="">
            <img src="/assets/images/icons/close_icon.svg" alt="close" />
          </button>
        </div>

        <div className="px-7 pb-6 pt-4 text-black font-normal">
          <p className="mt-2 ">
            <span className="font-bold">Total price:</span> €{currentOrder.totalPrice.toFixed(2)}
          </p>
          <p className="mt-2">
            <span className="font-bold">Payment type:</span> {currentOrder.paymentType}
          </p>
          <p className="mt-2">
            <span className="font-bold">Status:</span> {currentOrder.status}
          </p>
          <p className="mt-2">
            <span className="font-bold">Created at:</span> {currentOrder.createdAt}
          </p>
          <div>
            {currentOrder.items.map((item,index) => <p key={index}>{item.count}x {item.menuItem}</p>)}
          </div>
          <p className="mt-2 mb-4">
            <span className="font-bold">Note:</span> {currentOrder.note || ""}
          </p>
        </div>
      </div>
    </div>
    </div>
    
  );
};


export default OrdersByTableModal;
