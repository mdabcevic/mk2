import { useEffect, useState } from "react";
import { orderService } from "../../menu/order.service";
import { AppPaths } from "../../../../utils/routing/routes";
import { Link, useParams } from "react-router-dom";
import { notificationService } from "../../../../utils/services/notification.service";
import { authService, removePreviousState } from "../../../../utils/auth/auth.service";
import { placeOrderService } from "../../../../admin/pages/table-view/place-orders.service";
import { orderStatusIndex } from "../../../../utils/table-color";
import { useTranslation } from "react-i18next";
import { Order } from "../../../../utils/interfaces/order";
import { showToast, ToastType } from "../../../../utils/components/toast";
import MyOrdersList from "./my-orders-list";

const myOrdersKey = "showMyOrders";
export function updateShowOrders(status:boolean | null){
  let _status = localStorage.getItem(myOrdersKey) === "false" ? false : true; 
  sessionStorage.setItem(myOrdersKey, status !== null ? status.toString() :  (!_status).toString());
  window.dispatchEvent(new Event("showOrdersUpdated"));
}

function MyOrders() {
  const { placeId } = useParams();
  const [myOrders, setMyOrders] = useState<Order[]>([]);
  const { t } = useTranslation("public");
  const passCode = authService.passCode();
  const [showOrders, setShowOrders] = useState(false);

  
  const fetchMyOrders = async () => {
    const response = await orderService.getMyOrders();
    setMyOrders(response);
  };

  const callBartender = async () => {
    await notificationService.callBartender(authService.salt()!);
    showToast(t("call_bartender_message"),ToastType.info);
  };

  const requestPayment = async () => {
    const lastIndex = myOrders.length - 1;
    await placeOrderService.updateOrderStatus(myOrders[lastIndex].id, orderStatusIndex.payment_requested);
    showToast(t("request_payment_message"),ToastType.info);
  };

  const checkSession = async () => {
    if(authService.getLastSessionCheckTime()){
      const secondsSinceLastCall = ((new Date()).getTime() - authService.getLastSessionCheckTime()!.getTime()) / 1000;
      if(secondsSinceLastCall < 5) return;
    }
    const response = await authService.getGuestToken(authService.salt()!,true);
    if (!response.isSessionEstablished) {
        removePreviousState();
    }
    else{
      authService.setGuestToken(response.guestToken,placeId!);
    }
      
  }
  
  useEffect(() => {
    if(authService.salt()){
      checkSession();
      fetchMyOrders();  
    }
    else
      window.location.href = AppPaths.public.placeDetails.replace(":id",placeId!); 
  }, [placeId]);


  useEffect(() => {
    const syncWithStorage = () => {
      const value = sessionStorage.getItem(myOrdersKey) === "true";
      setShowOrders(value);
    };

    window.addEventListener("showOrdersUpdated", syncWithStorage);

    return () => {
      window.removeEventListener("showOrdersUpdated", syncWithStorage);
    };
  }, []);

  return (
    <section className={`relative w-full min-h-[80vh] mt-[100px]  h-content ${showOrders ? "overflowY-scroll": "overflow-hidden"} p-4`}>

      <div
        className={`absolute top-0 left-0 w-full h-full flex flex-col justify-start items-center transition-all duration-700 transform ${
          showOrders ? "opacity-0 scale-90 pointer-events-none" : "opacity-100 scale-100"
        }`}
      >
        <p className="text-center mb-8 mt-20  font-bold text-[16px] px-4">{t("sm_message").toUpperCase()}</p>

        <button className="px-6 py-3 rounded-[40px] bg-white color-mocha-600 font-bold border-mocha mb-4 w-64" onClick={() => callBartender()}>
          {t("call_bartender").toUpperCase()}
        </button>

        <Link to={AppPaths.public.menu.replace(":placeId", placeId!)} className="px-6 py-3 rounded-[40px] bg-mocha-600 font-bold text-white mb-4 w-64 text-center" >
          {t("order").toUpperCase()}
        </Link>
        {passCode && (<p className=" mt-16 flex flex-col items-center">MY PASSCODE:<span className="font-bold">{passCode}</span></p>)}
        {myOrders.length > 0 && (
          <button className=" absolute bottom-25 px-6 py-3 rounded-[40px] bg-white font-bold color-mocha-600 border-mocha w-64"  onClick={() => setShowOrders(true)}>
            {t("my_orders").toUpperCase()}
          </button>
        )}
      </div>

      <div
        className={`relative top-0 left-0 w-full h-full flex flex-col p-4 pb-28 transition-all duration-700 transform ${
          showOrders ? "opacity-100 scale-100" : "opacity-0 scale-90 pointer-events-none"
        }`}
      >

        <button onClick={() => setShowOrders(false)} className="self-start mb-4 text-mocha-600 font-semibold underline flex items-center" >
          {t("back").toUpperCase()}
        </button>

        <h4 className="text-md font-bold border-b pb-2 mb-4">
          {t("my_orders")}
        </h4>

        {showOrders && myOrders.length > 0 ? (
          <MyOrdersList
            visible={showOrders}
            myOrders={myOrders}
            onClose={() => setShowOrders(false)}
            onRequestPayment={requestPayment}
          />
        ) : (
          <p className="text-center px-4">{t("sm_message")}</p>
        )}

        {myOrders.length > 0 && (
          <div className="fixed bottom-2 left-0 w-full flex flex-col items-center">
            <button onClick={() => requestPayment()} className="px-6 py-3 rounded-[40px] bg-mocha-600 text-white mb-4 w-64">
              {t("request_payment").toUpperCase()}
            </button>
          </div>
        )}
      </div>
    </section>
  );
}

export default MyOrders;
