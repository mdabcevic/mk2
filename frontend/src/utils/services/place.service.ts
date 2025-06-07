import { PlaceStatus } from "../../admin/pages/dashboard/models";
import { TablePublic } from "../constants";
import { IPlaceItem } from "../interfaces/place-item";
import { ApiMethods } from "./api-methods"
import api from "./client";

export const placeService = {

    getPlaces: async (): Promise<IPlaceItem[]> => {
        return await api.get(ApiMethods.getPlaces);
    },

    getPlaceDetailsById: async (id:number): Promise<IPlaceItem> =>{
        return await api.get(ApiMethods.getPlaceById.replace("{id}",id.toString()));
    },

    getPlaceTables: async (id:number): Promise<TablePublic[]> => {
        return await api.get(ApiMethods.getPlaceTablesByPlaceId.replace("{id}",id.toString()));
    },

    getPlaceStatus: async (id:number): Promise<PlaceStatus> => {
        return await api.get(ApiMethods.getPlaceStatus.replace("{id}",id.toString()));
    }
}