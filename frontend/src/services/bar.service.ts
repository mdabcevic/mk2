import { ApiUrl } from "../client/api-urls"
import api from "../client/client";

export const BarService = () => {

    const getBars = async () => {
        const response = await api.get(ApiUrl.GetBars);
        return response;
    }

    const getBarDetailsById = async () =>{

        const response = await api.get(ApiUrl.GetBarById);
        return response;
    }
}