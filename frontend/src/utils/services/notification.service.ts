import { ApiMethods } from "./api-methods";
import api from "./client";


export const notificationService = {

    callBartender: async (salt:string) => {
        const response = await api.get(ApiMethods.callBartender.replace("{salt}",salt));
        return response;
    },
}