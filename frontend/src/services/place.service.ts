import { ApiUrl } from "../client/api-urls"
import api from "../client/client";

export const PlaceService = {

    getPlaces: async () => {
        const response = await api.get(ApiUrl.GetPlaces);
        return response;
    },

    getPlaceDetailsById: async (id:number) =>{

        const response = await api.get(ApiUrl.GetPlaceById);
        return response;
    }
}