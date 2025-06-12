import { useMutation } from "@tanstack/react-query";
import type { AxiosError } from "axios";
import type { LoginRequest, LoginResponse } from "../dto/AuthDto";
import axiosInstance from "../api/axios";
import { useAuth } from "./useAuth";
import { useNavigate } from "react-router";

export const useLogin = () => {
  const navigate = useNavigate();
  const { setUser } = useAuth();

  return useMutation<LoginResponse, AxiosError<{ message: string }>, LoginRequest>({
    mutationFn: async (data) => {
      const response = await axiosInstance.post<LoginResponse>("/auth/login", data);
      return response.data;
    },
    onSuccess: (data) => {
      localStorage.setItem("token", data.token);
      setUser(data.user);
      navigate("/dashboard");
    },
  });
};
