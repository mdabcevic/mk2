import { Constants } from "../constants";
import { AppPaths } from "../routing/routes";
import { ApiMethods } from "../services/api-methods";
import api from "../services/client";
import { GuestToken, Payload } from "./guest-token";

export const authService = {

    login: async (username: string, password: string): Promise<any> => {
        const response = await api.post(ApiMethods.login, { username: username, password: password });
        setToken(response);
        return response;
    },

    logout: () => {
        localStorage.removeItem(Constants.tokenKey);
        window.location.href = AppPaths.admin.dashboard;
    },


    getGuestToken: async (salt: string): Promise<GuestToken> => {
        const params = { salt: salt }
        setSalt(salt);
        localStorage.removeItem(Constants.tokenKey);
        
        return await api.get(ApiMethods.getGuestToken, params);
    },

    setGuestToken: (token: string,placeId:string) => {
        setToken(token);
        const passcode = getTokenPayload()?.passphrase;
        if (passcode)
            setPassCode(passcode);
        setTimeout(()=>{window.location.href = AppPaths.public.placeDetails.replace(":id", placeId);},5000)
        
    },

    joinTable: async (passcode:string, salt: string): Promise<GuestToken> => {   
        const data ={
            salt:salt,
            passphrase:passcode
        }
        return await api.get(ApiMethods.getGuestToken, data);
    },

    token:():string | null =>{
        return localStorage.getItem(Constants.tokenKey);
    },

    userRole: (): string => {
        return getTokenPayload()?.role || "";
    },

    placeId: (): number => {
        return Number(getTokenPayload()?.place_id) ?? null;
    },

    tableId: (): number => {
        return getTokenPayload()?.table_id ?? null;
    },

    passCode: (): string => {
        return getTokenPayload()?.passphrase ?? null;
    },

    salt: (): string | null=> {
        return localStorage.getItem(Constants.salt);
    },

}

function setSalt (salt:string){
    const existingSalt = localStorage.getItem(Constants.salt);
    if (existingSalt) // Remove if already present because a guest may scan multiple tables
        localStorage.removeItem(Constants.salt);
    localStorage.setItem(Constants.salt, salt);
}

function setToken (token:string){
    const existingtoken = localStorage.getItem(Constants.tokenKey);
    if (existingtoken)
        localStorage.removeItem(Constants.tokenKey);
    localStorage.setItem(Constants.tokenKey, token);
}

function getTokenPayload(): Payload{
    const token = localStorage.getItem(Constants.tokenKey);
    const payload = token?.split(".")[1];
    return payload ? JSON.parse(atob(payload)) : {} as Payload;
}

function setPassCode(passcode: string) {
    const existingPasscode = localStorage.getItem(Constants.passcode);
    if (existingPasscode)
        localStorage.removeItem(Constants.passcode);
    localStorage.setItem(Constants.passcode, passcode);
}
