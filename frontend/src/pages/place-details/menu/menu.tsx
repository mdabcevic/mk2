import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useParams } from "react-router-dom";
import { productMenuService } from '../../../utils/services/product-menu.service';
import { CategoryGroup, MenuGroupedItemDto, MenuItemDto } from "../../../admin/pages/products/product";
import { cartStorage } from "../../../utils/storage";
import { Cart } from "./cart";


export function Menu() {
  const { t } = useTranslation("public");
  const { placeId } = useParams<{ placeId: string }>();
  const [menu, setMenu] = useState<any[]>([]);
  const [showCart, setShowCart] = useState(false);

  useEffect(() => {
    if (placeId) {
      productMenuService.getMenuByPlaceId(placeId,true).then(setMenu);
    }
  }, [placeId]);

  return (
    <div className="p-2">
      <h2 className="text-xl font-bold mb-4 text-black text-center mt-2">
        {t("menu_text")}
      </h2>


      <div className="flex justify-center mb-4">
        <button
          onClick={() => setShowCart((prev) => !prev)}
          className="px-4 py-2 bg-blue-500 text-white rounded-lg shadow hover:bg-blue-600 transition"
        >
          Košarica
        </button>
      </div>


      <div
        className={`grid transition-all duration-500 ease-in-out ${
          showCart ? "max-h-[1000px] opacity-100 mb-4" : "max-h-0 opacity-0"
        } overflow-hidden`}
      >
        <Cart />
      </div>


      {menu.map((group, idx) => (
        <div key={idx} className="mb-6">
          <h3 className="text-lg font-semibold text-black mb-2">{group.category}</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
            {group.items.map((item:MenuGroupedItemDto) => (
              <div
                key={item.id}
                className={`p-3 border rounded-lg shadow-sm flex justify-between items-start ${
                  !item.isAvailable ? "bg-gray-200" : "bg-white"
                }`}
              >
                <div className="max-w-[250px]">
                  <h4 className="text-sm font-bold text-black">
                    {item.product.name}
                  </h4>
                  <p className="text-gray-600 text-sm">{item.description}</p>
                </div>
                <div className="text-right">
                  <p className="text-[#03af3a] font-bold">€{item.price}</p>
                  {!item.isAvailable ? (
                    <span className="px-2 mt-1 bg-gray-200 rounded text-gray-500 inline-block text-sm">
                      {t("unavailable_text") ?? "Trenutno nedostupno"}
                    </span>
                  ) : (
                    <div className="flex gap-1 justify-end mt-1">
                      <button
                        className="px-2 py-1  rounded text-black"
                        onClick={() => cartStorage.removeItem(item)}
                      >
                        -
                      </button>
                      <button
                        className="px-2 py-1   rounded text-black"
                        onClick={() => cartStorage.addItem(item)}
                      >
                        +
                      </button>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}
