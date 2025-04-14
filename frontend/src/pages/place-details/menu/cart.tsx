import { useEffect, useState } from "react";
import { Minus, Plus } from "lucide-react";
import { useTranslation } from "react-i18next";
import { cartStorage, CartItem } from "../../../utils/storage";
import { PaymentType } from "../../../utils/constants";
import { orderService } from "./order.service";

const Cart = () => {
  const [cart, setCart] = useState<Record<string, CartItem>>(cartStorage.getCart());
  const [paymentType, setPaymentType] = useState<number>(PaymentType.cash);
  const [note, setNote] = useState<string>("");
  const { t } = useTranslation("public");

  useEffect(() => {
    const unsubscribe = cartStorage.subscribe(setCart);
    return unsubscribe;
  }, []);

  const handleAdd = (item: CartItem) => {
    cartStorage.addItem(item);
  };

  const handleRemove = (item: CartItem) => {
    cartStorage.removeItem(item);
  };

  const createOrder = async () => {
    const orderItems = Object.values(cart).map((item) => ({
      menuItemId: item.menuId,
      count: item.quantity,
      discount: 0,
    }));

    const response = await orderService.createOrder(orderItems, paymentType, note);
    if (response) {
      console.log("Order created successfully", response);
    }
  };

  const paymentTypeChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    setPaymentType(Number(event.target.value));
  };

  const noteChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    setNote(event.target.value);
  };

  return (
    <div className="space-y-4 px-4">

      {Object.values(cart).map((item) => (
        <div
          key={item.name}
          className="flex justify-between items-center rounded-2xl shadow-md p-4"
        >
          <div className="flex flex-col">
            <p className="font-semibold text-black text-[14px]">{item.name}</p>
            <p className="text-gray-500 text-sm">
              €{(item.quantity * item.price).toFixed(2)}
            </p>
          </div>

          <div className="flex items-center gap-2">
            <button
              onClick={() => handleRemove(item)}
              className="bg-red-100 text-red-600 hover:bg-red-200 px-2 py-1 rounded-full"
            >
              <Minus size={16} />
            </button>
            <span className="min-w-[24px] text-center text-black text-[14px]">{item.quantity}</span>
            <button
              onClick={() => handleAdd(item)}
              className="bg-green-100 text-green-600 hover:bg-green-200 px-2 py-1 rounded-full"
            >
              <Plus size={16} />
            </button>
          </div>
        </div>
      ))}

<div className="space-y-4 text-black">
        <div>
          <label className="block text-lg font-semibold mb-2" htmlFor="payment-method">{t("payment_type")}</label>
          <select
            id="payment-method"
            value={paymentType}
            onChange={paymentTypeChange}
            className="block w-[150px] p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value={PaymentType.cash}>{t("cash")}</option>
            <option value={PaymentType.creditcard}>{t("credit_card")}</option>
          </select>
        </div>

        <div>
          <label className="block text-lg font-semibold mb-2" htmlFor="note">{t("note")}</label>
          <textarea
            id="note"
            maxLength={100}
            value={note}
            onChange={noteChange}
            className="block w-full p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder=""
          ></textarea>
        </div>
      </div>


      <div className="fixed bottom-10 left-0 w-full p-4 text-center z-50">
        <button
          onClick={() => createOrder()}
          className="bg-black font-semibold max-w-[250px] py-2 px-10 rounded"
        >
          Order {cartStorage.getTotalPrice().toFixed(2)}€
        </button>
      </div>
    </div>
  );
};

export default Cart;
