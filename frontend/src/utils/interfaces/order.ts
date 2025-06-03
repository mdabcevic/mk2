import { OrderStatusValue } from "../table-color";

export interface OrderItem {
  menuItem: string;
  price: number;
  discount: number;
  count: number;
}

export interface Order {
  id: number;
  items: OrderItem[];
  table: string;
  note: string;
  paymentType: string;
  totalPrice: number; 
  status: OrderStatusValue;
  customer: string | null;
  createdAt: string;
}