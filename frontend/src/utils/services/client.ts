import axios from "axios";
import { Constants } from "../constants";

const httpClient = axios.create({
  baseURL: Constants.api_base_url.toString(),
  headers: {
    "Content-Type": "application/json",
  },
});

httpClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem(Constants.tokenKey);
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

httpClient.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    return;
    if (error.response) {
      //alert(`Error: ${error.response.data.message || "Something went wrong!"}`);
    } else if (error.request) {
      //alert("Network error! Please check your internet connection.");
    } else {
      //alert("An unexpected error occurred.");
    }
    
    return Promise.reject(error);
  }
);


const sendRequest = async (method:string, url:string, data = null, params = {}) => {
  try {
    const response = await httpClient({ method, url, data, params });
    return response?.data ?? "";
  } catch (error:any) {
    // console.error("API Error:", error.response?.data || error.message);
    throw error.response?.data || error.message;
  }
};

const api = {
  get: (url:string, params?:{}) => sendRequest("get", url, null, params),
  post: (url:string, data:any) => sendRequest("post", url, data),
  put: (url:string, data:any) => sendRequest("put", url, data),
  patch: (url:string, data:any) => sendRequest("patch",url,data),
  delete: (url:string) => sendRequest("delete", url),
};

export default api;