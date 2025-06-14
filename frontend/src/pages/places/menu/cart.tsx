import { useEffect, useState } from "react";
import { Minus, Plus } from "lucide-react";
import { useTranslation } from "react-i18next";
import { cartStorage, CartItem } from "../../../utils/storage";
import { PaymentType } from "../../../utils/constants";
import { orderService } from "../../places/menu/order.service";
import { AppPaths } from "../../../utils/routing/routes";
import { authService } from "../../../utils/auth/auth.service";
import Dropdown, { DropdownItem } from "../../../utils/components/dropdown";

const Cart = () => {
  const [cart, setCart] = useState<Record<string, CartItem>>(cartStorage.getCart());
  const [paymentType, setPaymentType] = useState<number>(PaymentType.cash);
  const [note, setNote] = useState<string>("");
  const { t } = useTranslation("public");

  useEffect(() => {
    const unsubscribe = cartStorage.subscribe(setCart);
    return unsubscribe;
  }, []);

  const addItem = (item: CartItem) => {
    cartStorage.addItem(item);
  };

  const removeItem = (item: CartItem) => {
    cartStorage.removeItem(item);
  };

  const createOrder = async () => {
    const orderItems = Object.values(cart).map((item) => ({
      menuItemId: item.menuId,
      count: item.quantity,
      discount: 0,
    }));

    await orderService.createOrder(orderItems, paymentType, note);
    cartStorage.deleteCart();
    window.location.href = AppPaths.public.myOrders.replace(":placeId",authService.placeId().toString());
  };

  const paymentTypeChange = (item: DropdownItem<number>) => {
    setPaymentType(item.value);
  };

  const noteChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    setNote(event.target.value);
  };

  return (
    <div className="space-y-4 px-4 relative">
      <div className="max-h-[30vh] overflow-scroll relative border-b">
        {Object.values(cart).map((item) => (
          <div
            key={item.name}
            className="py-2 pl-8 mb-4 border rounded-[40px] shadow-sm flex justify-between items-center w-full bg-latte"
          >
            <div className="flex flex-col">
              <p className="font-semibold text-[14px]">{item.name}</p>
              <p className=" text-sm">
                €{(item.quantity * item.price).toFixed(2)}
              </p>
            </div>

            <div className="flex items-center gap-2 pr-3">
              <button onClick={() => removeItem(item)} className="color-mocha-600 hover:bg-red-200 px-2 py-1 rounded-full">
                <Minus size={16} />
              </button>
              <span className="text-[14px]">{item.quantity}</span>
              <button onClick={() => addItem(item)} className="color-mocha-600 hover:bg-green-200 px-2 py-1 rounded-full">
                <Plus size={16} />
              </button>
            </div>
          </div>
        ))}
      </div>
      

      <div className="space-y-2 text-black m-0 pt-4">
        <div className="">
          <label className="block text-lg font-semibold mb-2" htmlFor="payment-method">{t("payment_type")}</label>
          <Dropdown
            items={[{id:1,value:PaymentType.cash,label:t("cash")},{id:2,value:PaymentType.creditcard,label:t("credit_card")}]}
            value={paymentType}
            type="custom"
            onChange={paymentTypeChange}
            className="w-[300px]"
            buttonClassName="block w-[250px] py-1 px-4 border rounded-[40px] bg-neutral-latte-light focus:outline-none"
          />
        </div>

        <div>
          <label className="block text-lg font-semibold mb-2" htmlFor="note">{t("note")}</label>
          <textarea
            id="note"
            maxLength={100}
            value={note}
            onChange={noteChange}
            className="block w-full p-2 rounded-md bg-neutral-latte-light border"
            placeholder=""
          ></textarea>
        </div>
      </div>


      <div className="fixed bottom-0 left-0 w-full p-4 text-center z-50">
        <button
          onClick={() => createOrder()}
          className="text-white bg-mocha-600 font-semibold w-full max-w-[450px] py-2 px-10 rounded-[16px] cursor-pointer"
        >
          {t("cart.order")} {cartStorage.getTotalPrice().toFixed(2)}€
        </button>
      </div>
    </div>
  );
};

export default Cart;
