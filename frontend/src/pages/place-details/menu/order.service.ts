import { authService } from "../../../utils/auth/auth.service";
import { ApiMethods } from "../../../utils/services/api-methods";
import api from "../../../utils/services/client";

export interface CreateOrderRequest{
    tableId:number,
    paymentType:number,
    note:string,
    items:CreateOrderRequestItem[]
}

export interface CreateOrderRequestItem{
    menuItemId:number,
    count:number,
    discount:number,
}

export const orderService = {

    createOrder: async (orderItems: CreateOrderRequestItem[],paymentType:number,note:string): Promise<any> => {

        const tableId = authService.tableId();
        const createOrderRequest: CreateOrderRequest = {
            tableId:tableId,
            paymentType: paymentType,
            note:note,
            items:orderItems,
        } 
        const response = await api.post(ApiMethods.createOrder,createOrderRequest);
        return response;
    },

    getMyOrders: async (): Promise<any> =>{
        return await api.get(ApiMethods.getMyOrders);
    }

}