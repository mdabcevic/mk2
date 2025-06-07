
import { ApiMethods } from "./api-methods"
import api from "./client";

export const analyticsService = {
    getTableTraffic: async (id: number, month?: number, year?: number) =>{
        const params = {
            month: month,
            year: year
        }

        const response = await api.get(ApiMethods.getTableTraffic.replace("{placeId}", id.toString()), params);
        return response;
    },

    getAll: async (id?: number, month?: number, year?: number) => {
        const params = {
            month: month,
            year: year
        }

        const response = await api.get(ApiMethods.getAllAnalyticsData.replace("{placeId}", id ? id.toString() : ""), params);
        return response;
    },
}