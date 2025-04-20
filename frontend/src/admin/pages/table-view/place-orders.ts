import { authService } from "../../../utils/auth/auth.service";
import { ApiMethods } from "../../../utils/services/api-methods";
import api from "../../../utils/services/client";

export const placeOrderService = {

    getActiveOrders: async (): Promise<any> => {
        const params= {grouped:false}
        const placeId = authService.placeId();
        return await api.get(ApiMethods.getActiveOrders.replace("{placeId}",placeId.toString()),params);
    },

    getMyOrders: async (): Promise<any> =>{
        return await api.get(ApiMethods.getMyOrders);
    }

}