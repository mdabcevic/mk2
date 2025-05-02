export interface Category {
    id: number;
    name: string;
  }
  
export interface Product {
    id: number;
    name: string;
    volume: string;
    exclusive: boolean;
    category: Category;
  }

  export interface MenuItemDto{
    productId?: number;
    price: string,
    isAvailable: boolean,
    description?: string | null,
    product: Product;
  }


  export interface MenuGroupedItemDto {
    id: number;
    price: string;
    isAvailable: boolean;
    description?: string | null;
    product: {
      id: number;
      name: string;
      volume: string;
      category: string;
    };
  }
  
  export interface CategoryGroup {
    category: string;
    items: MenuGroupedItemDto[];
  }

  export interface cartItem{
    menuId:number,
    name:string,
    price:number,
    quantity:number
  }



  export interface MenuItem{
    placeId: number,
    productId: number,
    price: number,
    isAvailable: boolean,
    description?: string | null,
  }

  export interface MenuProduct {
    id?: number |null;
    name: string;
    volume: string;
    category: string;
  }

  

  export interface UpsertMenuItemDto{
    placeId:number;
    productId: number;
    price:number;
    isAvailable: boolean;
    description: string;

  }