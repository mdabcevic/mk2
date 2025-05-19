
import { ApiMethods } from "./api-methods"
import api from "./client";

export const analyticsService = {

    getPopularProducts: async (id?: number, month?: number, year?: number) => {
        const params = {
            month: month,
            year: year
        }

        const response = await api.get(ApiMethods.getPopularProducts.replace("{placeId}", id ? id.toString() : ""), params);
        return response;
    },

    getWeeklyTraffic: async (id?: number, month?: number, year?: number) =>{
        const params = {
            month: month,
            year: year
        }

        const response = await api.get(ApiMethods.getWeeklyTraffic.replace("{placeId}", id ? id.toString() : ""), params);
        return response;
    },

    getHourlyTraffic: async (id?: number, month?: number, year?: number) =>{
        const params = {
            month: month,
            year: year
        }

        const response = await api.get(ApiMethods.getHourlyTraffic.replace("{placeId}", id ? id.toString() : ""), params);
        return response;
    },
    getTableTraffic: async (id: number, month?: number, year?: number) =>{
        const params = {
            month: month,
            year: year
        }

        const response = await api.get(ApiMethods.getTableTraffic.replace("{placeId}", id.toString()), params);
        return response;
    },
    getAllPlacesTraffic: async (month?: number, year?: number) =>{
        const params = {
            month: month,
            year: year
        }

        const response = await api.get(ApiMethods.getAllPlacesTraffic, params);
        return response;
    },

    getTotalEarnings: async (id?: number, month?: number, year?: number) => {
        const params = {
            month: month,
            year: year
        }

        const response = await api.get(ApiMethods.getTotalEarnings.replace("{placeId}", id ? id.toString() : ""), params);
        return response;
    },
}