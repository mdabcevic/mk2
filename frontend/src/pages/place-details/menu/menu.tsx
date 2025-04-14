import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useParams } from "react-router-dom";
import { productMenuService } from '../../../utils/services/product-menu.service';
import { CategoryGroup, MenuGroupedItemDto, MenuItemDto } from "../../../admin/pages/products/product";
import { cartStorage } from "../../../utils/storage";
import { authService } from "../../../utils/auth/auth.service";
import { UserRole } from "../../../utils/constants";
import React from "react";
import { MenuItemsList } from "./menu-items-list";
import Cart from "./cart";


export function Menu() {
  const { t } = useTranslation("public");
  const { placeId } = useParams<{ placeId: string }>();
  const [menu, setMenu] = useState<CategoryGroup[]>([]);
  const [showCart, setShowCart] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);
  const [itemsOfSelectedCategory, setItemsOfSelectedCategory] = useState<MenuGroupedItemDto[]>([]);
  const userRole = authService.userRole();

  const fetchMenu = async () => {
    if (placeId) {
        const response = await productMenuService.getMenuByPlaceId(placeId, true) as CategoryGroup[];
        setMenu(response);
        if (response.length > 0) {
            setSelectedCategory(response[0].category); // Set default category to first one
            setItemsOfSelectedCategory(response[0].items);
        }
    }
  };

    const changeCategory = (category: string) => {
        setSelectedCategory(category);
        setItemsOfSelectedCategory(menu.find(group => group.category === category)?.items ?? []);
    }
  useEffect(() => {
      fetchMenu();
  }, [placeId]);

  return (
    <div className="relative h-screen overflow-hidden">
      <h2 className="text-xl font-bold mb-4 text-black text-center mt-2">
        {t("menu_text")}
      </h2>


      <div className="relative w-full h-full">

        <section
          id="menu"
          className={`absolute top-0 left-0 w-full h-full p-4 overflow-y-auto transition-transform duration-1000 ease-in-out ${
            showCart ? "-translate-x-full" : "translate-x-0"
          }`}
        >
          <div className="flex space-x-2 overflow-x-auto mb-4">
            {menu.map((group) => (
              <button
                key={group.category}
                onClick={() => changeCategory(group.category)}
                className={`px-4 py-2 rounded-full text-sm font-medium ${
                  selectedCategory === group.category
                    ? "bg-black text-white"
                    : "bg-gray-200 text-gray-700"
                }`}
              >
                {group.category}
              </button>
            ))}
          </div>

          <div className="mb-6">
          <MenuItemsList
            items={itemsOfSelectedCategory}
            userRole={userRole}
            />
          </div>
        </section>


        <section
          id="cart"
          className={`absolute top-0 left-0 w-full h-full p-4 overflow-y-auto transition-transform duration-1000 ease-in-out ${
            showCart ? "translate-x-0" : "translate-x-full"
          }`}
        >
          <Cart />

          <div className="mt-4">
            <button
              onClick={() => setShowCart(false)}
              className="px-4 py-2 bg-black text-white rounded"
            >
              ← {t("menu_text")}
            </button>
          </div>
        </section>
      </div>
      {
        !showCart && (
          <div className="fixed bottom-0 left-0 w-full p-4 text-center z-50">
            <button
              onClick={() => setShowCart(true)}
              className="bg-black font-semibold max-w-[250px] py-2 px-10 rounded"
            >
              Total {cartStorage.getTotalPrice().toFixed(2)}€ next
            </button>
          </div>
        ) 
      }
      
    </div>
  );
}
