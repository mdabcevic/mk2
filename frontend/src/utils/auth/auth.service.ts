import { Constants } from "../constants";
import { AppPaths } from "../routing/routes";
import { ApiMethods } from "../services/api-methods";
import api from "../services/client";
import { GuestToken, Payload } from "./guest-token";


export const authService = {

    login: async (username: string, password: string): Promise<any> => {
        const response = await api.post(ApiMethods.login, { username: username, password: password });
        const token = localStorage.getItem(Constants.tokenKey);
        if (token)
            localStorage.removeItem(Constants.tokenKey);
        localStorage.setItem(Constants.tokenKey, response);
        return response;
    },

    logout: () => {
        localStorage.removeItem(Constants.tokenKey);
        window.location.href = AppPaths.admin.dashboard;
    },


    getGuestToken: async (salt: string): Promise<GuestToken> => {
        const params = { salt: salt }
        return await api.get(ApiMethods.getGuestToken, params);
    },

    setGuestToken: (token: string,placeId:string) => {
        const existingtoken = localStorage.getItem(Constants.tokenKey);
        if (existingtoken)
            localStorage.removeItem(Constants.tokenKey);
        localStorage.setItem(Constants.tokenKey, token);

        const passcode = decodePayload(token)?.passphrase;
        if (passcode)
            setPassCode(passcode);
        setTimeout(()=>{window.location.href = AppPaths.public.placeDetails.replace(":id", placeId);},5000)
        
    },

    userRole: (): string => {
        const token = localStorage.getItem(Constants.tokenKey);
        if (!token) return "";

        return decodePayload(token)?.role || "";
    },

    placeId: (): number => {
        const token = localStorage.getItem(Constants.tokenKey);
        if (!token) {
            throw Error("Token not found!");
        }
        return decodePayload(token)?.place_id;
    },

    tableId: (): number => {
        const token = localStorage.getItem(Constants.tokenKey);
        if (!token) {
            throw Error("Token not found!");
        }
        return decodePayload(token)?.table_id;
    }

}
function decodePayload(token: string): Payload {
    const payload = token!.split(".")[1];
    if (!payload) throw new Error("Invalid token");

    const decodedPayload = JSON.parse(atob(payload));
    return decodedPayload;
}

function setPassCode(passcode: string) {
    const existingPasscode = localStorage.getItem(Constants.passcode);
    if (existingPasscode)
        localStorage.removeItem(Constants.passcode);
    localStorage.setItem(Constants.passcode, passcode);
}
