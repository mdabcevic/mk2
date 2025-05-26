export interface OrderItem {
  menuItem: string;
  price: number;
  discount: number;
  count: number;
}

export interface MyOrder {
  id: number;
  items: OrderItem[];
  table: string;
  note: string;
  paymentType: string;
  totalPrice: number; 
  status: string;
  customer: string | null;
  createdAt: string;
}