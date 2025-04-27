import { MenuGroupedItemDto } from "../admin/pages/products/product";
import { Constants } from "./constants";



export type CartItem = {
  menuId: number,
  name: string;
  quantity: number;
  price: number;
};

type Cart = Record<string, CartItem>;

let subscribers: ((cart: Cart) => void)[] = [];

const getCart = (): Cart => {
  const stored = localStorage.getItem(Constants.cartKey);
  try {
    return stored ? JSON.parse(stored) : {};
  } catch {
    return {};
  }
};

const saveCart = (cart: Cart) => {
  localStorage.setItem(Constants.cartKey, JSON.stringify(cart));
  subscribers.forEach((cb) => cb(cart));
};

const addItem = (item: MenuGroupedItemDto | CartItem) => {
  const cart = getCart();
  const name = isMenuGroupedItemDto(item) ? item.product.name : item.name;
  const price = isMenuGroupedItemDto(item) ? parseFloat(item.price) : item.price;
  const id = isMenuGroupedItemDto(item) ? item.id : item.menuId;

  const existing = cart[name];

  const updated: Cart = {
    ...cart,
    [name]: {
      menuId: id,
      name,
      quantity: existing ? existing.quantity + 1 : 1,
      price,
    },
  };

  saveCart(updated);
};


const removeItem = (item: MenuGroupedItemDto | CartItem) => {
  const cart = getCart();
  const name = "product" in item ? item.product.name : item.name;

  const existing = cart[name];
  if (!existing) return;

  const updated = { ...cart };
  if (existing.quantity <= 1) {
    delete updated[name];
  } else {
    updated[name].quantity -= 1;
  }

  saveCart(updated);
};

const deleteCart = () => {
  localStorage.removeItem(Constants.cartKey);
  saveCart({});
}

const subscribe = (callback: (cart: Cart) => void) => {
  subscribers.push(callback);
  callback(getCart());

  return () => {
    subscribers = subscribers.filter((cb) => cb !== callback);
  };
};

const getTotalPrice = (): number =>{
  return getTotal().totalPrice;
};

const getTotal = () => {
  const cart = getCart();
  let totalQuantity = 0;
  let totalPrice = 0;

  Object.values(cart).forEach((item) => {
    totalQuantity += item.quantity;
    totalPrice += item.quantity * item.price;
  });

  return { totalQuantity, totalPrice };
};

function isMenuGroupedItemDto(item: any): item is MenuGroupedItemDto {
  return typeof item === "object" && item !== null && "product" in item;
}

export const cartStorage = {
  getCart,
  addItem,
  removeItem,
  subscribe,
  getTotal,
  getTotalPrice,
  deleteCart,
  saveCart,
};
