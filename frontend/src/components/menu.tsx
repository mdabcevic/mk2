import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useParams } from "react-router-dom";
import { menuService } from "../services/menu.service";
import { IMenuItem } from "../interfaces/menu";


function Menu() {
  const { t } = useTranslation("public");
  const [editingId, setEditingId] = useState<Number | null>(null);
  const [viewType, setViewType] = useState<string>("guest");
  const { placeId } = useParams<{ placeId: string }>();
  const [loading, setLoading] = useState<boolean>(false);

  const [cart, setCart] = useState<Record<string, { name: string; quantity: number; price: number }>>({});

  const [menuItems,setMenuItems]= useState<IMenuItem[]>([]);


  useEffect(()=>{
    fetchMenu();
  },[placeId]);

  const fetchMenu = async () =>{
    if(placeId){
      let _menuItems = await menuService.getMenuByPlaceId(placeId!) as IMenuItem[];
      setMenuItems(_menuItems);
    }
  }
  const addItem = (item: IMenuItem) => {
    setCart((prevCart) => {
      const existingItem = prevCart[item.product.name];
      const itemPrice = parseFloat(item.price);
  
      if (existingItem) {
        return {
          ...prevCart,
          [item.product.name]: {
            ...existingItem,
            quantity: existingItem.quantity + 1,
          },
        };
      } else {
        return {
          ...prevCart,
          [item.product.name]: {
            name: item.product.name,
            quantity: 1,
            price: itemPrice,
          },
        };
      }
    });
  };

  const decreaseItem = (item: IMenuItem) => {
    setCart((prevCart) => {
      const existingItem = prevCart[item.product.name];
      if (!existingItem || existingItem.quantity <= 0) return prevCart;
  
      return {
        ...prevCart,
        [item.product.name]: {
          ...existingItem,
          quantity: existingItem.quantity - 1,
        },
      };
    });
  };

  const calculateTotal = () => {
    let totalQuantity = 0;
    let totalPrice = 0;
  
    Object.values(cart).forEach((item) => {
      totalQuantity += item.quantity;
      totalPrice += item.quantity * item.price;
    });
  
    return { totalQuantity, totalPrice };
  };

  const { totalQuantity, totalPrice } = calculateTotal();

  return (
    <>
      <div className="p-2">
        <h2 className="text-xl font-bold mb-4 text-black text-center mt-2">
          {t("menu_text")}
        </h2>
        <button className="p-2 bg-black m-2 hidden" onClick={() => setViewType("admin")}>admin</button>
        <button className="p-2 bg-black hidden" onClick={() => setViewType("quest")}>gost</button>
        {/* Display Cart Summary for Guest */}
        {viewType === "guest" && (
          <div className="mb-4">
            {Object.values(cart).length > 0 && (
              <div>
                <h3 className="text-black">{t("selected_items")}</h3>
                <div className="mt-2">
                  {Object.values(cart).map((item, index) => (
                    <div key={index} className="m-0 text-black flex flex-row pl-2 ">
                      <p>{item.name}</p>
                      <p className="mr-2 ml-2">{t("quantity_text")}: {item.quantity}</p>
                      <p className="ml-2">/ ${(item.quantity * item.price).toFixed(2)}</p>
                    </div>
                  ))}
                </div>
              </div>
            )}
            <div className="mb-4 mt-2 text-black flex flex-row">
              <p className="mr-2">{t("quantity_text")}: {totalQuantity}</p>
              <p>{t("total_price_text")}: ${totalPrice.toFixed(2)}</p>
            </div>
          </div>
        )}

        {/* Display Menu Items */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-1">
          {menuItems.map((item, index) => (
            <div key={index} className="p-1 border border-gray-300 rounded-lg shadow-md flex justify-between bg-white">
              <div className="max-w-[250px]">
                <h4 className="text-sm font-semibold text-black">{item.product.name}</h4>
                <p className="text-gray-600">{item.description}</p>
              </div>
              <div>
                <p className="text-[#03af3a] font-bold text-right">${item.price}</p>

                {viewType === "admin" && (
                  <div className="text-black">
                    <button onClick={() => setEditingId(index)}>Actions</button>
                    {editingId === index && (
                      <div>
                        <span>{t("edit_text")}</span>
                        <span>{t("delete_text")}</span>
                      </div>
                    )}
                  </div>
                )}

                {viewType === "guest" && (
                  <div className="text-black">
                    <button className="px-2 bg-white ml-1 rounded" onClick={() => decreaseItem(item)}>-</button>
                    <button className="px-2 bg-white rounded" onClick={() => addItem(item)}>+</button>
                  </div>
                )}
              </div>
              
              
            </div>
          ))}
        </div>
      </div>
    </>
  );
}

export default Menu;
