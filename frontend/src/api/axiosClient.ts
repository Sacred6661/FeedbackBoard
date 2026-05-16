import axios, { AxiosError } from "axios";
import { API_URL } from "../constants/api";

const axiosClient = axios.create({
  baseURL: API_URL,
  withCredentials: true,
});

axiosClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    // Return the rejected token to stop the chain
    return Promise.reject(error);
  },
);

export default axiosClient;
