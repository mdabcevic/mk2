import { Table, TableStatusString } from "../constants";
import { ApiMethods } from "./api-methods"
import api from "./client";

export const tableService = {

    getPlaceTablesByCurrentUser: async():Promise<Table[]>  => {
        return await api.get(ApiMethods.getPlaceTablesByCurrentUser);
    },

    getPlaceTablesByPlaceId: async(placeId:number):Promise<Table[]>  => {
        return await api.get(ApiMethods.getPlaceTablesByPlaceId.replace("{id}",placeId.toString()));
    },

    saveOrUpdateTables: async(tables: Table[]) => {
        return await api.post(ApiMethods.saveOrUpdateTables,tables);
    },

    changeStatus: async (status:TableStatusString, salt:string) =>{
        return await api.patch(ApiMethods.changeTableStatus.replace("{salt}",salt),status);
    },

    regenrateQrCode:async (label:string) =>{
        return await api.post(ApiMethods.regenerateQrCode.replace("{label}",label),label)
    },

    disableTable: async(tableLabel:string) => {
        return await api.patch(ApiMethods.disableTable.replace("{tableLabel}",tableLabel),true);
    },


}