import { useEffect, useState } from "react";
import { orderService } from "../menu/order.service";
import { AppPaths } from "../../../utils/routing/routes";
import { Link, useNavigate, useParams } from "react-router-dom";
import { notificationService } from "../../../utils/services/notification.service";
import { authService, removePreviousState } from "../../../utils/auth/auth.service";
import { placeOrderService } from "../../../admin/pages/table-view/place-orders.service";
import { orderStatusIndex } from "../../../utils/table-color";
import { useTranslation } from "react-i18next";
import { Order } from "../../../utils/interfaces/order";


// function MyOrders({ placeId }: { placeId: string }) {
function MyOrders() {
  const { placeId } = useParams();
  const [myOrders, setMyOrders] = useState<Order[]>([]);
  const [showOrders, setShowOrders] = useState(false);
  const { t } = useTranslation("public");
  const passCode = authService.passCode();
  const navigate = useNavigate();
  
  const fetchMyOrders = async () => {
    const response = await orderService.getMyOrders();
    setMyOrders(response);
  };

  const callBartender = async () => {
    await notificationService.callBartender(authService.salt()!);
  };

  const requestPayment = async () => {
    const lastIndex = myOrders.length - 1;
    await placeOrderService.updateOrderStatus(
      myOrders[lastIndex].id,
      orderStatusIndex.payment_requested
    );
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
      navigate(AppPaths.public.myOrders);
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

  return (
    <section className={`relative w-full min-h-[80vh] mt-[100px]  h-content ${showOrders ? "overflowY-scroll": "overflow-hidden"} p-4`}>

      <div
        className={`absolute top-0 left-0 w-full h-full flex flex-col justify-start items-center transition-all duration-700 transform ${
          showOrders ? "opacity-0 scale-90 pointer-events-none" : "opacity-100 scale-100"
        }`}
      >
        <p className="text-center mb-8 mt-20  font-bold text-[16px]">{t("sm_message").toUpperCase()}</p>

        <button
          className="px-6 py-3 rounded-[40px] bg-white color-mocha-600 font-bold border-mocha mb-4 w-64"
          onClick={() => callBartender()}
        >
          {t("call_bartender").toUpperCase()}
        </button>

        <Link
          to={AppPaths.public.menu.replace(":placeId", placeId!)}
          className="px-6 py-3 rounded-[40px] bg-mocha-600 font-bold text-white mb-4 w-64 text-center"
        >
          {t("order").toUpperCase()}
        </Link>
        {passCode && (<p className=" mt-16 flex flex-col items-center">MY PASSCODE:<span className="font-bold">{passCode}</span></p>)}
        {myOrders.length > 0 && (
          <button
            className=" absolute bottom-25 px-6 py-3 rounded-[40px] bg-white font-bold color-mocha-600 border-mocha w-64"
            onClick={() => setShowOrders(true)}
          >
            {t("my_orders").toUpperCase()}
          </button>
        )}
      </div>

      <div
        className={`relative top-0 left-0 w-full h-full flex flex-col p-4 pb-28 transition-all duration-700 transform ${
          showOrders ? "opacity-100 scale-100" : "opacity-0 scale-90 pointer-events-none"
        }`}
      >

        <button
          className="self-start mb-4 text-mocha-600 font-semibold underline flex items-center"
          onClick={() => setShowOrders(false)}
        >
            {t("back").toUpperCase()}
        </button>

        <h4 className="text-md font-bold border-b pb-2 mb-4">
          {t("my_orders")}
        </h4>

        {showOrders && myOrders.length > 0 ? (
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
                <div
                  key={idx}
                  className="flex-column pt-2 pb-2 pl-6 neutral-latte border b-white rounded-[30px] text-[14px] mb-6"
                >
                  <p className="color-mocha-600 font-semibold">
                    {item.menuItem} (x{item.count})
                  </p>
                  <span className="font-normal">
                    {(item.price * item.count).toFixed(2)}€
                  </span>
                </div>
              ))}
            </div>
          ))
        ) : (
          <p className="text-center">{t("sm_message")}</p>
        )}

        {myOrders.length > 0 && (
          <div className="fixed bottom-2 left-0 w-full flex flex-col items-center">
            <button
              className="px-6 py-3 rounded-[40px] bg-mocha-600 text-white mb-4 w-64"
              onClick={() => requestPayment()}
            >
              {t("request_payment").toUpperCase()}
            </button>
          </div>
        )}
      </div>
    </section>
  );
}

export default MyOrders;
