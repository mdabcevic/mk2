import React, { useEffect, useRef, useState } from "react";
import { UserRole } from "../../../utils/constants";
import { cartStorage } from "../../../utils/storage";
import { MenuGroupedItemDto } from "../../../admin/pages/products/product";
import { useTranslation } from "react-i18next";
import { AnimatePresence, motion } from "framer-motion";
import { showToast, ToastType } from "../../../utils/toast";

const ITEMS_VISIBLE = 10;
const EMPTY_DIV_HEIGHT = 20;
const ITEM_HEIGHT = 60; 

export function MenuItemsList({ items,userRole }: {items:MenuGroupedItemDto[],userRole:string}) {
    const containerRef = useRef<HTMLDivElement>(null);
    const [visibleStart, setVisibleStart] = useState(0);
    const [scrollDirection, setScrollDirection] = useState<"up" | "down">("down");
    const [lastScrollTop, setLastScrollTop] = useState(0);
    const { t } = useTranslation("public");
    const handleScroll = () => {
      const container = containerRef.current;
      if (!container) return;
  
      const scrollTop = container.scrollTop;
      const newStart = Math.floor(scrollTop / ITEM_HEIGHT);
  
      setScrollDirection(scrollTop > lastScrollTop ? "down" : "up");
      setLastScrollTop(scrollTop);
      setVisibleStart(newStart);
    };
  
    useEffect(() => {
      const el = containerRef.current;
      if (el) el.addEventListener("scroll", handleScroll);
      return () => el?.removeEventListener("scroll", handleScroll);
    }, []);
  
  
    return (
      <div
        ref={containerRef}
        className="overflow-y-auto h-[calc(10*60px)] relative overflow-x-hidden"
      >
        <div className="relative" style={{ height: `${items.length * (ITEM_HEIGHT)}px` }}>
        <AnimatePresence mode="popLayout">
            {items.map((item, index) => {
              const isVisible = index >= visibleStart && index < visibleStart + ITEMS_VISIBLE;

              return (
                  <div key={index} className="relative">
                      <motion.div
                  
                  layout
                  initial={{
                    opacity: 0,
                    x: scrollDirection === "down" ? 100 : -100,
                  }}
                  animate={{
                    opacity: isVisible ? 1 : 0,
                    x: isVisible ? 0 : scrollDirection === "down" ? -100 : 100,
                    pointerEvents: isVisible ? "auto" : "none",
                  }}
                  transition={{ duration: 0.3 }}
                  className={`p-3 border rounded-lg shadow-sm flex justify-between items-start absolute w-full ${
                    !item.isAvailable ? "bg-gray-200" : "bg-white"
                  }`}
                  style={{
                    height: ITEM_HEIGHT,
                    top: index * (ITEM_HEIGHT),
                  }}
                >
                  <div className="max-w-[250px]">
                    <h4 className="text-sm font-bold text-black">{item.product.name}</h4>
                    <p className="text-gray-600 text-sm">{item.description}</p>
                  </div>
                
                  <div className="text-right">
                    <p className="text-[#03af3a] font-bold">€{item.price}</p>
                
                    {!item.isAvailable ? (
                      <span className="px-2 mt-1 bg-gray-200 rounded text-gray-500 inline-block text-sm">
                        {t("unavailable_text") ?? "Trenutno nedostupno"}
                      </span>
                    ) : (
                      userRole === UserRole.guest && (
                        <div className="flex gap-1 justify-end">
                          <button
                            className="px-2 py-1 rounded text-black"
                            onClick={() => {cartStorage.removeItem(item); showToast(`${item.product.name} ${t("removed")}`,ToastType.info)}}
                          >
                            -
                          </button>
                          <button
                            className="px-2 py-1 rounded text-black"
                            onClick={() => {cartStorage.addItem(item); showToast(`${item.product.name} ${t("added")}`,ToastType.info)}}
                          >
                            +
                          </button>
                        </div>
                      )
                    )}
                  </div>
                </motion.div>
                <div className="bottom-0 left-0 right-0 text-black text-center"
                  style={{ height: EMPTY_DIV_HEIGHT }}></div>
                </div>
                  
              );
            })}
          </AnimatePresence>
        </div>
      </div>
    );
}
