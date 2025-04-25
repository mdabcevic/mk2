import { useEffect, useState } from "react";
import { placeOrderService } from "./place-orders.service";
import { OrderStatusValue, getStatusColor, orderStatusIndex } from "../../../utils/table-color";

type OrderItem = {
  menuItem: string;
  price: number;
  discount: number;
  count: number;
};

type Order = {
  id: number;
  items: OrderItem[];
  table: string;
  note: string;
  paymentType: string;
  totalPrice: number;
  status: OrderStatusValue;
  customer: string | null;
  createdAt: string;
};


const statusOptions: OrderStatusValue[] = [
  OrderStatusValue.created,
  OrderStatusValue.approved,
  OrderStatusValue.delivered,
  OrderStatusValue.payment_requested,
  OrderStatusValue.paid,
  OrderStatusValue.closed,
  OrderStatusValue.cancelled,
];

enum OrderTabs{
  activeOrders=0,
  inactiveOrders=1,
}
const page = 1;
const tablePageSize = 30;

const OrdersTable:React.FC<{rerender:boolean}> = ({rerender}) => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [modalOpen, setModalOpen] = useState(false);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [activeTab, setActiveTab] = useState<OrderTabs>(OrderTabs.activeOrders);
  const [total,setTotal] = useState<number>(0);
  useEffect(() => {
    fetchOrders();
  }, [activeTab,rerender]);

  const fetchOrders = async () => {
    const response = await placeOrderService.getOrders(activeTab == OrderTabs.activeOrders ? true : false,page,tablePageSize);
    const allOrders = response?.items?.flatMap((group: { orders: Order[] }) => group.orders);
    setOrders(allOrders);
    console.log(response)
    setTotal(response?.total);
  };

  const updateStatus = async (id: number, newStatus: OrderStatusValue) => {
    
    await placeOrderService.updateOrderStatus(id, orderStatusIndex[newStatus]);
    setOrders((prev) =>
      prev.map((order) =>
        order.id === id ? { ...order, status: newStatus } : order
      )
    );
  };

  const openModal = (order: Order) => {
    setSelectedOrder(order);
    setModalOpen(true);
  };

  const closeModal = () => {
    setModalOpen(false);
    setSelectedOrder(null);
  };

  return (
    <div className="bg-white rounded-md shadow-md p-4 overflow-x-auto">
      <div className="flex gap-4 border-b mb-4 border-white">
        <button
          className={`pb-2 px-4  ${activeTab === OrderTabs.activeOrders ? " text-brown-500 font-semibold" : "text-brown-500 font-thin"}`}
          onClick={() => setActiveTab(OrderTabs.activeOrders)}
        >
          Active orders
        </button>
        <button
          className={`pb-2 px-4 ${activeTab === OrderTabs.inactiveOrders ? "text-brown-500 font-semibold" : "text-brown-500 font-thin"}`}
          onClick={() => setActiveTab(OrderTabs.inactiveOrders)}
        >
          Closed orders
        </button>
      </div>
      <div>
      </div>
      <span>Total:{total}</span>
      <table className="w-full table-auto border-separate border-spacing-y-2">
        <thead>
          <tr className="text-left border-b border-gray-200">
            <th>Date & Time</th>
            <th>Table</th>
            <th className="text-center">Status</th>
            <th>Total</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          { orders?.length > 0 && (orders?.map((order) => (
            <tr
              key={order.id}
              className="text-sm hover:bg-gray-50"
            >
              <td>{order?.createdAt}</td>
              <td>{order?.table}</td>
              <td className="text-center">
                <select
                  value={order?.status}
                  onChange={(e) =>
                    updateStatus(order?.id, e.target.value as OrderStatusValue)
                  }
                  style={{ backgroundColor: getStatusColor(order?.status), color: order?.status == OrderStatusValue.payment_requested ? "black" : "white" }}
                  className="border border-gray-300 rounded-[30px] py-[10px] pl-[40px]"
                >
                  {statusOptions.map((status) => (
                    <option key={status} value={status} className="bg-white text-black">
                      {status.replace("_", " ")}
                    </option>
                  ))}
                </select>
              </td>
              <td>{order?.totalPrice.toFixed(2)}€</td>
              <td>
                <button
                  onClick={() => openModal(order)}
                  className="cursor-pointer"
                >
                  <img src="/assets/images/icons/search_showmore_icon.svg" alt="show"/>
                </button>
              </td>
            </tr>)
          ))}
        </tbody>
      </table>

      {modalOpen && selectedOrder && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-[20px] shadow-lg w-full max-w-[375px] border border-[#A3A3A3]">
            <div className="relative bg-[#FAFAFA] rounded-[20px] text-[#A3A3A3] flex justify-between items-start flex-start flew-row w-full pl-4 pt-4 pb-0 pr-4">
              <h3 className="text-lg font-bold mb-4 flex flex-col"><span>Order #{selectedOrder.id}</span><span className="text-sm">Table: {selectedOrder.table}</span></h3>
              
              <button onClick={closeModal} className="">
                <img src="/assets/images/icons/close_icon.svg" alt="close" />
              </button>
            </div>
            <div className="pl-7 pb-3 mt-8">
            <p className="mt-2 "><span className="font-bold">Total price:</span> €{selectedOrder.totalPrice.toFixed(2)}</p>
            <p className="mt-2 "><span className="font-bold">Payment type:</span> {selectedOrder.paymentType}</p>
            <p className=" mt-2 mb-2"><span className="font-bold">Status:</span> {selectedOrder.status.replace("_", " ")}</p>
              <div>
                <h4 className="font-semibold">Items:</h4>
                <ul className="list-disc list-inside text-[14px] pl-2">
                  {selectedOrder.items.map((item, i) => (
                    <li key={i}>
                      {item.count} × {item.menuItem} - €{(item.price * item.count).toFixed(2)}
                    </li>
                  ))}
                </ul>
              </div>
              <p className=" mt-2"><span className="font-bold">Note:</span> {selectedOrder.note}</p>
            </div>
            
          </div>
        </div>
      )}
    </div>
  );
};

export default OrdersTable;
