import { ApiUrl } from "../client/api-urls"
import api from "../client/client";

export const MenuService = () => {

    const getMenuById = async () => {
        const response = await api.get(ApiUrl.GetMenuById);
        return response;
    }

}