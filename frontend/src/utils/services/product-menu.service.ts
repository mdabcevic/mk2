import { Product, MenuItem, UpsertMenuItemDto, MenuItemDto, CategoryGroup, MenuGroupedItemDto } from "../../admin/pages/products/product";
import { ApiMethods } from "./api-methods"
import api from "./client";

export const productMenuService = {

    getMenuByPlaceId: async (placeId: string, includeCategory: boolean): Promise<MenuItemDto[] | CategoryGroup[]> => {
        const params = includeCategory ? {
            groupByCategory:true
        } : {}
        const response = await api.get(ApiMethods.getMenuByPlaceId.replace("{id}",placeId),params);
        return response;
    },

    getAllProducts: async (id: number): Promise<Product[]> => {
        const response = await api.get(ApiMethods.getAllProducts);
        return response;
    },

    getProductCategories: async(id: number): Promise<any> => {
        const response = await api.get(ApiMethods.getProductCategories);
        return response;
    },

    saveProductsToPlace: async(menuList: MenuItem[]): Promise<any> => {
        return await api.post(ApiMethods.saveProductsToPlace,menuList);
    },

    updateMenuItem: async(item: UpsertMenuItemDto): Promise<any> =>{
        return await api.put(ApiMethods.updateMenuItem,item);
    }
}