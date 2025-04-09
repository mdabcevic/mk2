// components/Cart.tsx
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { cartStorage } from "../../../utils/storage";

export const Cart = () => {
  const { t } = useTranslation("public");
  const [cart, setCart] = useState(cartStorage.getCart);

  useEffect(() => {
    const unsubscribe = cartStorage.subscribe(setCart);
    return () => unsubscribe();
  }, []);

  const { totalQuantity, totalPrice } = cartStorage.getTotal();

  if (Object.values(cart).length === 0) return null;

  return (
    <div className="mb-4">
      <h3 className="text-black">{t("selected_items")}</h3>
      <div className="mt-2">
        {Object.values(cart).map((item, index) => (
          <div key={index} className="m-0 text-black flex flex-row pl-2">
            <p>{item.name}</p>
            <p className="mr-2 ml-2">{t("quantity_text")}: {item.quantity}</p>
            <p className="ml-2">/ €{(item.quantity * item.price).toFixed(2)}</p>
          </div>
        ))}
      </div>
      <div className="mb-4 mt-2 text-black flex flex-row">
        <p className="mr-2">{t("quantity_text")}: {totalQuantity}</p>
        <p>{t("total_price_text")}: €{totalPrice.toFixed(2)}</p>
      </div>
    </div>
  );
};
