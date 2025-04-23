import { useEffect, useRef, useState } from "react";
import { UserRole } from "../../../utils/constants";
import { CartItem, cartStorage } from "../../../utils/storage";
import { MenuGroupedItemDto } from "../../../admin/pages/products/product";
import { useTranslation } from "react-i18next";

import { showToast, ToastType } from "../../../utils/toast";

const EMPTY_DIV_HEIGHT = 20;

export function MenuItemsList({ items,userRole }: {items:MenuGroupedItemDto[],userRole:string}) {
    const containerRef = useRef<HTMLDivElement>(null);

    const [cart, setCart] = useState<Record<string, CartItem>>(cartStorage.getCart());
    const { t } = useTranslation("public");
    const handleScroll = () => {
      const container = containerRef.current;
      if (!container) return;
  
    };
  
    useEffect(() => {
      const el = containerRef.current;
      if (el) el.addEventListener("scroll", handleScroll);
      return () => el?.removeEventListener("scroll", handleScroll);
    }, []);
  
    useEffect(() => {
      const unsubscribe = cartStorage.subscribe(setCart);
      return unsubscribe;
    }, []);
  
    return (
      <div
        ref={containerRef}
        className="overflow-y-auto h-[calc(10*60px)] relative overflow-x-hidden"
      >
        <div className="relative pt-3" >
        <div >

            {items.map((item, index) => {
              const quantity = cart[item.product.name]?.quantity || 0;
              return (
                  <div key={index} className={`relative px-3`}>
                  <div className={`pl-5 border rounded-[40px] shadow-sm flex justify-between items-center w-full ${
                    !item.isAvailable && UserRole.guest == userRole ? "bg-gray-200" : "bg-neutral-latte-light"
                  } ${userRole !== UserRole.guest ? "py-3 pr-5" : "py-0"}`}>
                  <div className="max-w-[250px]">
                    <h4 className="text-sm font-medium">{item.product.name}</h4>
                    <p className="text-gray-600 text-sm">{item.description}</p>
                  </div>
                
                  <div className="text-right flex flex-row items-center">
                    <p className=" font-normal">€{item.price}</p>
                
                    {!item.isAvailable && UserRole.guest == userRole ? (
                      <span className="px-2 mt-1 bg-gray-200 rounded text-gray-500 inline-block text-sm">
                        {t("unavailable") ?? "Trenutno nedostupno"}
                      </span>
                    ) : (
                      userRole === UserRole.guest && (
                        <div className="">
                          <button
                            className="p-0 rounded ml-5 relative left-1 top-1 cursor-pointer"
                            onClick={() => {cartStorage.addItem(item); showToast(`${item.product.name} ${t("added")}`,ToastType.info)}}
                          >
                            <img src="/assets/images/plus.svg" width={"35px"} />
                          </button>
                        </div>
                      )
                    )}


                    {userRole === UserRole.guest && quantity > 0 && (
                      <div className="absolute bottom-2 left-0 text-white flex items-center mb-8 p-1 px-4 bg-mocha-600 rounded-full shadow-md test">
                        <span>{quantity} x</span>
                        <button className="px-2 bg-white ml-1 cursor-pointer rounded ml-5" onClick={() => {cartStorage.removeItem(item); showToast(`${item.product.name} ${t("removed")}`,ToastType.info)}}>
                          <img src="/assets/images/minus.svg" width="5px"/>
                        </button>
                      </div>
                    )}
                  </div>
                  </div>

                <div className="bottom-0 left-0 right-0 text-black text-center"
                  style={{ height: EMPTY_DIV_HEIGHT }}></div>
                </div>
                  
              );
            })}
        </div>
        </div>
      </div>
    );
}
