import { authService } from "../../../utils/auth/auth.service";
import { ApiMethods } from "../../../utils/services/api-methods";
import api from "../../../utils/services/client";

export const placeOrderService = {

    getOrders: async (isActive:boolean, page:number,size:number): Promise<any> => {
        const params= {
            grouped:true,
            page:page,
            size:size
        }
        const placeId = authService.placeId();
        return isActive ? await api.get(ApiMethods.getActiveOrders.replace("{placeId}",placeId.toString()),params) : 
                          await api.get(ApiMethods.getClosedOrders.replace("{placeId}",placeId.toString()),params);
    },

    updateOrderStatus: async (orderId:number,status:number) : Promise<any> => {
        const data= {
            status:status
        }
        return await api.put(ApiMethods.updateOrderStatus.replace("{orderId}",orderId.toString()),data);
    }

}