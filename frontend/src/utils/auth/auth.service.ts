import { Constants } from "../constants";
import { AppPaths } from "../routing/routes";
import { ApiMethods } from "../services/api-methods";
import api from "../services/client";


export const authService = {

    login: async (username:string, password:string):Promise<any> => {
        const response = await api.post(ApiMethods.login,{username:username,password:password});
        const token = localStorage.getItem(Constants.tokenKey);
        if(token)
            localStorage.removeItem(token);
        console.log(response)
        localStorage.setItem(Constants.tokenKey,response);
        return response;
    },

    userRole: ():string =>{

        const token = localStorage.getItem(Constants.tokenKey);
        if(!token) return "";
        
        const payload = token!.split(".")[1];
        if (!payload) throw new Error("Invalid token");

        return JSON.parse(atob(payload)).role || "";
    },

    placeId: (): number =>{
        const token = localStorage.getItem(Constants.tokenKey);
        if(!token){
            throw Error("Token not found!");
        } 
        const payload = token!.split(".")[1];
        if (!payload) throw new Error("Invalid token");

        const decodedPayload = JSON.parse(atob(payload));
        return decodedPayload?.place_id;
    }
}