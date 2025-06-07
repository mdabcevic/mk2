import { Constants } from "../constants";
import { AppPaths } from "../routing/routes";
import { ApiMethods } from "../services/api-methods";
import api from "../services/client";
import { GuestToken, Payload } from "./guest-token";

let checkSessionLastSent: Date | null = null;
export const authService = {
    
    login: async (username: string, password: string): Promise<any> => {
        const response = await api.post(ApiMethods.login, { username: username, password: password });
        setToken(response);
        return response;
    },

    logout: () => {
        removePreviousState();
        window.location.href = AppPaths.admin.dashboard;
    },

    getLastSessionCheckTime: (): Date | null => checkSessionLastSent,

    getGuestToken: async (salt: string, checkSession: boolean): Promise<GuestToken> => {
        const params = { salt: salt }
        if(!checkSession){
            removePreviousState();
            setSalt(salt);  
        }     
        checkSessionLastSent = new Date();
        const response = await api.get(ApiMethods.getGuestToken, params);
        return response;
    },

    setGuestToken: (token: string,placeId:string) => {
        setToken(token);
        const passcode = getTokenPayload()?.passphrase;
        localStorage.setItem(Constants.place_id,placeId);
        if (passcode)
            setPassCode(passcode);
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

    tokenValid: (): boolean => {
        const token = authService.token();
        if (!token) return false;
    
        try {
            const payload = JSON.parse(atob(token.split(".")[1]));
            const exp = payload.exp;
            if (!exp) return false;
            
            const currentTime = Math.floor(Date.now() / 1000);
            if(exp < currentTime){
                removePreviousState();
                return false
            }
            return true;
        } catch (error) {
            console.error("Invalid token format", error);
            return false;
        }
    },

    userRole: (): string => {
        return getTokenPayload()?.role || "";
    },

    placeId: (): number => {
        const tokenPlaceId = Number(getTokenPayload()?.place_id);
        if (!isNaN(tokenPlaceId)) {
            return tokenPlaceId;
        }
        return Number(localStorage.getItem(Constants.place_id));
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
    // const existingtoken = localStorage.getItem(Constants.tokenKey);
    // if (existingtoken)
    //     localStorage.removeItem(Constants.tokenKey);
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

export function removePreviousState(){
    localStorage.removeItem(Constants.tokenKey);
    localStorage.removeItem(Constants.salt);
    localStorage.removeItem(Constants.passcode);
    localStorage.removeItem(Constants.cartKey);
}

