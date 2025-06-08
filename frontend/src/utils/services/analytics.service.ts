
import { ApiMethods } from "./api-methods"
import api from "./client";
import { AllAnayticsData, TableTraffic, } from "../../admin/pages/analytics/analytics-interface";

export const analyticsService = {
    getTableTraffic: async (id: number, month?: number, year?: number): Promise<TableTraffic[]> =>{
        const params = {
            month: month,
            year: year
        }

        const response = await api.get(ApiMethods.getTableTraffic.replace("{placeId}", id.toString()), params);
        return response;
    },

    getAll: async (id?: number, month?: number, year?: number): Promise<AllAnayticsData> => {
        const params = {
            month: month,
            year: year
        }

        const response = await api.get(ApiMethods.getAllAnalyticsData.replace("{placeId}", id ? id.toString() : ""), params);
        return response;
    },
}