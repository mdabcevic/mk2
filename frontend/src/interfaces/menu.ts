export interface IProduct {
    name: string;
    volume: string;
    category: string;
  }
  
export interface IMenuItem {
    product: IProduct;
    price: string;
    description: string | null;
    isAvailable: boolean;
  }