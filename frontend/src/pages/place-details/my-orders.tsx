import { useEffect, useState } from "react";
import { orderService } from "./menu/order.service";
import { AppPaths } from "../../utils/routing/routes";
import { Link } from "react-router-dom";
import { t } from "i18next";
import { notificationService } from "../../utils/services/notification.service";
import { authService } from "../../utils/auth/auth.service";
import { placeOrderService } from "../../admin/pages/table-view/place-orders.service";
import { orderStatusIndex } from "../../utils/table-color";

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


function MyOrders({placeId}:{placeId:string}) {
    const [myOrders, setMyOrders] = useState<Order[]>([]);

    const fetchMyOrders = async () => {
        const response = await orderService.getMyOrders();
        setMyOrders(response);
    };

    const callBartender = async () =>{
        await notificationService.callBartender(authService.salt()!); 
    }

    const requestPayment = async () =>{
        const lastIndex = myOrders.length-1;
        await placeOrderService.updateOrderStatus(myOrders[lastIndex].id,orderStatusIndex.payment_requested);
    }

    useEffect(() => {
        fetchMyOrders();
    }, []);

    return (
        <section className="p-0">
        <h4 className="text-md text-white font-bold border-b pb-2 mb-4">My Orders</h4>

        {myOrders.length > 0 && (
            myOrders.map((order, index) => (
            <div key={order.id} className="mb-6 ">
                <p className="text-[16px] font-semibold mb-0 text-black">Order {index+1} - {order.totalPrice.toFixed(2)}€</p>
                {order.note && (
                <p className="text-sm mb-2 whitespace-pre-line text-[14px]">
                    <span className="font-bold">Note:</span> {order.note}
                </p>
                )}

                {order.items.map((item, idx) => (
                    <div key={idx} className="flex-column mt-2 mr-5 ml-5 pt-2 pb-2 pl-6 bg-neutral-latte-light border b-white rounded-[30px] text-[14px] mb-2">
                        <p className="color-mocha-600 font-semibold">{item.menuItem} (x{item.count})</p>
                        <span className="font-normal">{(item.price * item.count).toFixed(2)}€</span>
                    </div>
                    
                ))}
            </div>
            ))
        )}
        {myOrders?.length == 0 && <p>{t("sm_message")}</p>}
        <div className={`flex items-center justify-center w-full pl-2 pr-2
            ${myOrders.length > 0 ? 'fixed bottom-0 left-0 pb-4 z-10 flex-row' : 'flex-col'}`}>
            
            <button className={`px-5 py-1 border-mocha rounded-[40px] ${myOrders?.length >= 0 ? 'mt-3 bg-mocha-600 text-white' : 'bg-neutral-latte-light text-black'}`}
                    onClick={()=> callBartender()}
                    >{t("call_bartender").toUpperCase()}</button>
            <button className={` px-5 py-1 rounded-[40px] border-mocha ml-1 mr-1 ${myOrders?.length == 0 ? 'mt-3 bg-mocha-600 text-white' : 'bg-neutral-latte-light text-black'}`}>
                <Link to={AppPaths.public.menu.replace(":placeId",placeId)}>
                {t("order").toUpperCase()}
                </Link>
            </button>
            {myOrders?.length > 0 && (<button onClick={()=>requestPayment()} className="bg-mocha-600 px-5 py-1 rounded-[40px] text-white"> {t("request_payment").toUpperCase()}</button>)}
        </div>
        </section>
    );
}
  

export default MyOrders;