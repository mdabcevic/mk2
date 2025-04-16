import { ApiMethods } from "./api-methods"
import api from "./client";

export const tableService = {

    getPlaceTablesByCurrent: async() =>{
        const response = await api.get(ApiMethods.getPlaceTablesByCurrentUser);
        return response;
    }
}