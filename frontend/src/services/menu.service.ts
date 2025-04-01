import { ApiUrl } from "../client/api-urls"
import api from "../client/client";

export const menuService = {

    getMenuByPlaceId: async (placeId:string) => {
        const response = await api.get(ApiUrl.GetMenuByPlaceId.replace("{placeId}",placeId));
        return response;
    }

}