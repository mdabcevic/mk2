import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useParams } from "react-router-dom";
import { productMenuService } from '../../../utils/services/product-menu.service';
import { CategoryGroup, MenuGroupedItemDto } from "../../../admin/pages/products/product";
import { cartStorage } from "../../../utils/storage";
import { authService } from "../../../utils/auth/auth.service";
import { UserRole } from "../../../utils/constants";
import { MenuItemsList } from "./menu-items-list";
import Cart from "./cart";
import { AppPaths } from "../../../utils/routing/routes";
import { CategoryTabs } from "../../../utils/components/menu-category-tabs";
import Footer from "../../../containers/footer";


export function Menu() {
  const { t } = useTranslation("public");
  const { placeId } = useParams<{ placeId: string }>();
  const [menu, setMenu] = useState<CategoryGroup[]>([]);
  const [showCart, setShowCart] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);
  const [itemsOfSelectedCategory, setItemsOfSelectedCategory] = useState<MenuGroupedItemDto[]>([]);
  const userRole = authService.userRole();
  const [totalPrice, setTotalPrice] = useState(0);

  useEffect(() => {
    const unsubscribe = cartStorage.subscribe(() => {
      setTotalPrice(cartStorage.getTotalPrice());
    });

    setTotalPrice(cartStorage.getTotalPrice());

    return () => {
      unsubscribe();
    };
  }, []);
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
    <>
      <div className="relative flex flex-col min-h-screen overflow-hidden px-2 pt-[100px]">
      
      <h2 className={`text-xl font-bold mb-4 text-center mt-2 ${!showCart ? "block":"hidden"}`} >
        {t("menu_text")}
      </h2>
      <Link className={`ml-4 ${!showCart ? "block":"hidden"}`} to={AppPaths.public.placeDetails.replace(":id",placeId!)} >Go Back</Link>

      
      <h2 className={`text-xl font-bold mb-4 text-left pl-2 pb-2 border-b mt-2 ${showCart ? "block":"hidden"}`}>
          My orders
      </h2>

      <div className="relative w-full h-full">

        <section
          id="menu"
          className={`flex flex-col flex-grow overflow-y-auto p-4 transition-transform duration-1000 ease-in-out ${
            showCart ? "-translate-x-full" : "translate-x-0"
          }`}
        >
          <CategoryTabs 
            menu={menu} 
            selectedCategory={selectedCategory} 
            changeCategory={changeCategory} 
          />

          <div className={`${showCart ? "hidden":"block mb-6"}`}>
          <MenuItemsList
            items={itemsOfSelectedCategory}
            userRole={userRole}
            />
          </div>
        </section>


        <section
          id="cart"
          className={`absolute top-0 left-0 w-full h-content min-h-[80vh] p-4 overflow-y-auto transition-transform duration-1000 ease-in-out ${
            showCart ? "translate-x-0" : "translate-x-full"
          }`}
        >
          <Cart />

          <div className="mt-4">
            <button
              onClick={() => setShowCart(false)}
              className="text-white bg-mocha-600 font-semibold flex gap-3 flex-row max-w-[250px] py-2 px-10 rounded-[50px] cursor-pointer"
            >
              <img  src="/assets/images/arrow.svg" alt="back_arrow"/>{t("menu_text")}
            </button>
          </div>
        </section>
      </div>
      {
        !showCart && userRole === UserRole.guest && (
          <div className="fixed bottom-0 left-0 w-full p-4 text-center z-50">
            <button
              onClick={() => {if(totalPrice > 0) setShowCart(true);}}
              className="text-white bg-mocha-600 font-semibold max-w-[250px] py-2 px-10 rounded-[50px] cursor-pointer"
            >
              Total {totalPrice.toFixed(2)}€ next
            </button>
          </div>
        ) 
      }
    </div>
    {userRole !== UserRole.guest && userRole !== UserRole.manager && userRole !== UserRole.admin && <Footer />}
    </>
    
  );
}
