import { ApiMethods } from "./api-methods"
import api from "./client";

export const placeService = {

    getPlaces: async () => {
        const response = await api.get(ApiMethods.getPlaces);
        return response;
    },

    getPlaceDetailsById: async (id:number) =>{

        const response = await api.get(ApiMethods.getPlaceById.replace("{id}",id.toString()));
        return response;
    }
}