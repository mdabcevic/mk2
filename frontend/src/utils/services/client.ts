import axios from "axios";

const API_BASE_URL = "https://localhost:7281";

const httpClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

httpClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");
    
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
    if (error.response) {
      alert(`Error: ${error.response.data.message || "Something went wrong!"}`);
    } else if (error.request) {
      alert("Network error! Please check your internet connection.");
    } else {
      alert("An unexpected error occurred.");
    }
    
    return Promise.reject(error);
  }
);


const sendRequest = async (method:string, url:string, data = null, params = {}) => {
  try {
    const response = await httpClient({ method, url, data, params });
    return response.data;
  } catch (error:any) {
    console.error("API Error:", error.response?.data || error.message);
    throw error.response?.data || error.message;
  }
};

const api = {
  get: (url:string, params?:{}) => sendRequest("get", url, null, params),
  post: (url:string, data:any) => sendRequest("post", url, data),
  put: (url:string, data:any) => sendRequest("put", url, data),
  delete: (url:string) => sendRequest("delete", url),
};

export default api;