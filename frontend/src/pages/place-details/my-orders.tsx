import { useEffect, useState } from "react";
import { orderService } from "./menu/order.service";

export interface OrderItem {
    menuItem: string;
    price: number;
    discount: number;
    count: number;
  }
  
  export interface Order {
    id: number;
    items: OrderItem[];
    table: string;
    note: string;
    paymentType: string;
    totalPrice: number;
    status: string;
    customer: string | null;
    createdAt: string;
  }


function MyOrders() {
    const [myOrders, setMyOrders] = useState<Order[]>([]);

    const fetchMyOrders = async () => {
        const response = await orderService.getMyOrders();
        setMyOrders(response);
    };

    useEffect(() => {
        fetchMyOrders();
    }, []);

    return (
        <section className="p-0">
        <h4 className="text-md font-bold border-b pb-2 mb-4">My Orders</h4>

        {myOrders.length > 0 && (
            myOrders.map((order, index) => (
            <div key={order.id} className="mb-6">
                <p className="text-[16px] font-semibold mb-0">Order {index+1} - {order.totalPrice.toFixed(2)}€</p>
                {order.note && (
                <p className="text-sm mb-2 whitespace-pre-line text-[14px]">
                    <span className="font-bold">Note:</span> {order.note}
                </p>
                )}

                {order.items.map((item, idx) => (
                    <div key={idx} className="flex-column mt-2 mr-5 ml-5 pt-2 pb-2 pl-6 neutral-latte border b-white rounded-[30px] text-[14px] mb-2">
                        <p className="color-mocha-600 font-semibold">{item.menuItem} (x{item.count})</p>
                        <span className="font-normal">{(item.price * item.count).toFixed(2)}€</span>
                    </div>
                    
                ))}
            </div>
            ))
        )}
        </section>
    );
}
  

export default MyOrders;