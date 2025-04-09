import { MenuGroupedItemDto } from "../admin/pages/products/product";
import { Constants } from "./constants";



type CartItem = {
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

const addItem = (item: MenuGroupedItemDto) => {
  const cart = getCart();
  const existing = cart[item.product.name];

  const updated: Cart = {
    ...cart,
    [item.product.name]: {
      name: item.product.name,
      quantity: existing ? existing.quantity + 1 : 1,
      price: parseFloat(item.price),
    },
  };

  saveCart(updated);
};

const removeItem = (item: MenuGroupedItemDto) => {
  const cart = getCart();
  const existing = cart[item.product.name];
  if (!existing) return;

  const updated = { ...cart };

  if (existing.quantity <= 1) {
    delete updated[item.product.name];
  } else {
    updated[item.product.name].quantity -= 1;
  }

  saveCart(updated);
};

const subscribe = (callback: (cart: Cart) => void) => {
  subscribers.push(callback);
  callback(getCart());

  return () => {
    subscribers = subscribers.filter((cb) => cb !== callback);
  };
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

export const cartStorage = {
  getCart,
  addItem,
  removeItem,
  subscribe,
  getTotal,
};
