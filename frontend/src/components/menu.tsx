import { useState } from "react";
import { useTranslation } from "react-i18next";

interface MenuItem {
  name: string;
  price: number;
  description: string;
}

function Menu({ placeId }: { placeId: number }) {
  const { t } = useTranslation("public");

  const [editingId, setEditingId] = useState<Number | null>(null);
  const [viewType, setViewType] = useState<string>("guest");

  // Store the cart items with their quantities and prices
  const [cart, setCart] = useState<Record<string, { name: string; quantity: number; price: number }>>({});

  const menuItems: MenuItem[] = [
    { name: "Classic Burger", price: 8.99, description: "Juicy beef patty with lettuce, tomato, and cheese." },
    { name: "Margherita Pizza", price: 12.5, description: "Tomato sauce, fresh mozzarella, and basil." },
    { name: "Grilled Chicken Salad", price: 9.99, description: "Grilled chicken on fresh greens with vinaigrette." },
    { name: "Spaghetti Carbonara", price: 13.99, description: "Creamy sauce with bacon and Parmesan." },
    { name: "BBQ Ribs", price: 18.99, description: "Slow-cooked ribs glazed in BBQ sauce." },
    { name: "Fish and Chips", price: 14.5, description: "Crispy battered fish served with fries." },
    { name: "Caesar Salad", price: 7.99, description: "Romaine lettuce, croutons, and Caesar dressing." },
    { name: "Buffalo Wings", price: 11.99, description: "Spicy wings served with blue cheese dip." },
    { name: "Veggie Wrap", price: 8.49, description: "Grilled vegetables in a whole wheat wrap." },
    { name: "Cheese Nachos", price: 10.99, description: "Crispy nachos topped with melted cheese and jalapeños." },
    { name: "Pasta Alfredo", price: 12.99, description: "Creamy Alfredo sauce with fettuccine pasta." },
    { name: "Sushi Platter", price: 24.99, description: "Assorted sushi rolls with soy sauce." },
    { name: "Steak and Fries", price: 19.99, description: "Juicy steak served with crispy fries." },
    { name: "Mushroom Risotto", price: 16.49, description: "Creamy risotto with wild mushrooms." },
    { name: "Tuna Tartare", price: 14.99, description: "Fresh tuna with avocado and soy dressing." },
    { name: "Chocolate Lava Cake", price: 8.99, description: "Warm chocolate cake with a gooey center." },
    { name: "Strawberry Cheesecake", price: 7.5, description: "Creamy cheesecake topped with fresh strawberries." },
    { name: "Espresso", price: 3.99, description: "Strong and bold Italian espresso." },
    { name: "Mojito", price: 9.49, description: "Refreshing cocktail with mint, lime, and rum." },
    { name: "Pina Colada", price: 10.99, description: "Tropical drink with coconut, pineapple, and rum." },
  ];

  // Add item to cart
  const addItem = (item: MenuItem) => {
    setCart((prevCart) => {
      const existingItem = prevCart[item.name];
      if (existingItem) {
        // If item already exists, increase quantity
        return {
          ...prevCart,
          [item.name]: {
            ...existingItem,
            quantity: existingItem.quantity + 1,
          },
        };
      } else {
        // If item doesn't exist, add it to the cart
        return {
          ...prevCart,
          [item.name]: {
            name: item.name,
            quantity: 1,
            price: item.price,
          },
        };
      }
    });
  };

  // Decrease item quantity from cart
  const decreaseItem = (item: MenuItem) => {
    setCart((prevCart) => {
      const existingItem = prevCart[item.name];
      if (!existingItem || existingItem.quantity <= 0) return prevCart;
      
      // Decrease item quantity
      return {
        ...prevCart,
        [item.name]: {
          ...existingItem,
          quantity: existingItem.quantity - 1,
        },
      };
    });
  };

  // Calculate total quantity and price
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
      <div>
        <h2 className="text-xl font-bold mb-4">
          {t("menu_link_text")} {placeId}
        </h2>
        <button className="p-2 bg-black m-2" onClick={() => setViewType("admin")}>admin</button>
        <button className="p-2 bg-black" onClick={() => setViewType("quest")}>gost</button>
        {/* Display Cart Summary for Guest */}
        {viewType === "guest" && (
          <div className="mb-4">
            {Object.values(cart).length > 0 && (
              <div>
                <h3>Your Cart</h3>
                <div>
                  {Object.values(cart).map((item, index) => (
                    <div key={index} className="mb-2 text-black flex flex-row">
                      <p>{item.name}</p>
                      <p className="mr-2 ml-2">Quantity: {item.quantity}</p>
                      <p>Price: ${(item.quantity * item.price).toFixed(2)}</p>
                    </div>
                  ))}
                </div>
              </div>
            )}
            <div className="mb-4 text-black flex flex-row">
              <p className="mr-2">Total Items: {totalQuantity}</p>
              <p>Total Price: ${totalPrice.toFixed(2)}</p>
            </div>
          </div>
        )}

        {/* Display Menu Items */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {menuItems.map((item, index) => (
            <div key={index} className="p-4 border border-gray-300 rounded-lg shadow-md">
              <h3 className="text-lg font-semibold text-black">{item.name}</h3>
              <p className="text-gray-600">{item.description}</p>
              <p className="text-green-600 font-bold text-right">${item.price.toFixed(2)}</p>

              {viewType === "admin" && (
                <div className="text-black">
                  <button onClick={() => setEditingId(index)}>Actions</button>
                  {editingId === index && (
                    <div>
                      <span>Edit</span>
                      <span>Delete</span>
                    </div>
                  )}
                </div>
              )}

              {viewType === "guest" && (
                <div className="text-black">
                  <button className="p-2 bg-white" onClick={() => addItem(item)}>+</button>
                  <button className="p-2 bg-white" onClick={() => decreaseItem(item)}>-</button>
                </div>
              )}
            </div>
          ))}
        </div>
      </div>
    </>
  );
}

export default Menu;
