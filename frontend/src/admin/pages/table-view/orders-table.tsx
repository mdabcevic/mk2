import { useEffect, useState } from "react";
import { placeOrderService } from "./place-orders.service";
import { OrderStatusValue, getStatusColor, orderStatusIndex } from "../../../utils/table-color";
import Dropdown from "../../../utils/components/dropdown";
import { Order } from "../../../utils/interfaces/order";
import OrderDetailsModal from "../../../utils/components/order-details-modal";
import PaginationControls from "../../../utils/components/pagination-controlls";

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

const tablePageSize = 15;

const OrdersTable:React.FC<{rerender:number,showStatus:boolean}> = ({rerender,showStatus}) => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [modalOpen, setModalOpen] = useState(false);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [activeTab, setActiveTab] = useState<OrderTabs>(OrderTabs.activeOrders);
  const [total,setTotal] = useState<number>(0);
  const [page, setPage] = useState(1);

  useEffect(() => {
    fetchOrders();
  }, [activeTab,rerender,page]);

  const fetchOrders = async () => {
    const response = await placeOrderService.getOrders(activeTab == OrderTabs.activeOrders,page,tablePageSize);
    let allOrders: Order[] = [];
  
    if (activeTab === OrderTabs.activeOrders) {
      allOrders = response?.items?.flatMap((group: { orders: Order[] }) => group.orders ?? []) ?? [];
    } else {
      allOrders = response?.items ?? [];
    }
    setOrders(allOrders);
    setTotal(response?.total ?? 0);
  };

  const updateStatus = async (id: number, newStatus: OrderStatusValue) => {
    await placeOrderService.updateOrderStatus(id, orderStatusIndex[newStatus]);
    setOrders((prev) => prev.map((order) => order.id === id ? { ...order, status: newStatus } : order));
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
        <button onClick={() => setActiveTab(OrderTabs.activeOrders)}
          className={`pb-2 px-4  ${activeTab === OrderTabs.activeOrders ? " text-brown-500 font-semibold" : "text-brown-500 font-thin"}`}        
        >
          Active orders
        </button>
        <button onClick={() => setActiveTab(OrderTabs.inactiveOrders)}
          className={`pb-2 px-4 ${activeTab === OrderTabs.inactiveOrders ? "text-brown-500 font-semibold" : "text-brown-500 font-thin"}`}       
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
            <th className="text-center">Table</th>  
            <th className="text-center">{showStatus ? (<span>Status</span>) : (<span>Payment Type</span>) }</th>
            <th className="text-center">Total</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          { orders?.length > 0 && (orders?.map((order,index) => (
            <tr
              key={index}
              className="text-sm hover:bg-gray-50 py-4"
            >
              <td>{order?.createdAt}</td>
              <td className="text-center">{order?.table}</td>
              <td className="text-center">
                {showStatus ? (
                  <Dropdown
                    items={statusOptions.map((s) => ({
                      id: s,
                      value: s,
                      label: s.replace("_", " "),
                    }))}
                    value={order.status}
                    onChange={(item) =>
                      updateStatus(order.id, item.value as OrderStatusValue)
                    }
                    type="custom"
                    className={`w-[200px]`}
                    buttonClassName={`rounded-[30px] py-[10px] pl-[40px] bg-[${getStatusColor(order?.status)}] text-black ${activeTab === OrderTabs.activeOrders ? "border-none" : "border" }`}
                  />
                ) : (
                  <span>{order.paymentType}</span>
                )}
                
              </td>
              <td className="text-center">{order?.totalPrice.toFixed(2)}€</td>
              <td className="text-center">
                <button onClick={() => openModal(order)} className="cursor-pointer ">
                  <img src="/assets/images/icons/search_showmore_icon.svg" alt="show"/>
                </button>
              </td>
            </tr>)
          ))}
        </tbody>
      </table>
      <PaginationControls
        currentPage={page}
        totalItems={total}
        itemsPerPage={tablePageSize}
        onPageChange={setPage}
      />

      {modalOpen && selectedOrder && (
        <OrderDetailsModal
          open={modalOpen}
          order={selectedOrder}
          onClose={closeModal}
        />
      )}
    </div>
  );
};

export default OrdersTable;
