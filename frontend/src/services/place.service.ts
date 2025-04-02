import { ApiUrl } from "../client/api-urls"
import api from "../client/client";

export const placeService = {

    getPlaces: async () => {
        const response = await api.get(ApiUrl.GetPlaces);
        return response;
    },

    getPlaceDetailsById: async (id:number) =>{

        const response = await api.get(ApiUrl.GetPlaceById.replace("{id}",id.toString()));
        return response;
    }
}