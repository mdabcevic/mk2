import { useEffect, useState } from "react";
import { placeOrderService } from "./place-orders";

const statusPriority: Record<string, number> = {
  "Receipe needed": 1,
  "Payment": 2,
  "Paid": 3,
};

type Order = {
  id: string;
  dateTime: string;
  tableName: string;
  status: "Receipe needed" | "Payment" | "Paid";
  price: number;
};

const OrdersTable = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [modalOpen, setModalOpen] = useState(false);

  useEffect(() => {
    fetchOrders(currentPage);
  }, [currentPage]);

  const fetchOrders = async (page: number) => {

    const response = await placeOrderService.getActiveOrders();
    console.log(response);
    // const sorted = data.sort((a: Order, b: Order) =>
    //   statusPriority[a.status] - statusPriority[b.status]
    // );
    // setOrders(sorted);
  };

  const updateStatus = (id: string, newStatus: Order["status"]) => {
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
    setSelectedOrder(null);
    setModalOpen(false);
  };

  return (
    <div className="bg-white rounded-md shadow-md p-4 overflow-x-auto">
      <table className="w-full table-auto border-separate border-spacing-y-2">
        <thead>
          <tr className="text-left border-b border-gray-200">
            <th>Date & Time</th>
            <th>Table Name</th>
            <th>Status</th>
            <th>Price</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {orders.map((order) => (
            <tr
              key={order.id}
              className="border-b border-gray-200 text-sm hover:bg-gray-50"
            >
              <td>{order.dateTime}</td>
              <td>{order.tableName}</td>
              <td>
                <select
                  value={order.status}
                  onChange={(e) =>
                    updateStatus(order.id, e.target.value as Order["status"])
                  }
                  className="border border-gray-300 rounded px-2 py-1"
                >
                  <option value="Receipe needed">Receipe needed</option>
                  <option value="Payment">Payment</option>
                  <option value="Paid">Paid</option>
                </select>
              </td>
              <td>${order.price.toFixed(2)}</td>
              <td>
                <button
                  onClick={() => openModal(order)}
                  className="text-blue-500 hover:text-blue-700"
                >
                  <span>View</span>
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="mt-4 flex justify-end gap-2">
        <button
          onClick={() => setCurrentPage((p) => Math.max(p - 1, 1))}
          className="px-3 py-1 border rounded text-sm"
        >
          Prev
        </button>
        <button
          onClick={() => setCurrentPage((p) => p + 1)}
          className="px-3 py-1 border rounded text-sm"
        >
          Next
        </button>
      </div>

      {modalOpen && selectedOrder && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-md p-6 shadow-lg w-full max-w-md">
            <h2 className="text-lg font-bold mb-4">Order Details</h2>
            <p><strong>Date & Time:</strong> {selectedOrder.dateTime}</p>
            <p><strong>Table Name:</strong> {selectedOrder.tableName}</p>
            <p><strong>Status:</strong> {selectedOrder.status}</p>
            <p><strong>Price:</strong> ${selectedOrder.price.toFixed(2)}</p>
            <button
              onClick={closeModal}
              className="mt-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
            >
              Close
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default OrdersTable;
