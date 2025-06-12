import { useMutation } from "@tanstack/react-query";
import type { AxiosError } from "axios";
import type { RegisterRequest, RegisterResponse } from "../dto/AuthDto";
import axiosInstance from "../api/axios";
import { useAuth } from "../hooks/useAuth";
import { useNavigate } from "react-router";

export const useRegister = () => {
  const navigate = useNavigate();
  const { setUser } = useAuth();

  return useMutation<RegisterResponse, AxiosError<{ message: string }>, RegisterRequest>({
    mutationFn: async (data) => {
      const response = await axiosInstance.post<RegisterResponse>("/auth/register", data);
      return response.data;
    },
    onSuccess: (data) => {
      localStorage.setItem("token", data.token);
      setUser(data.user);
      navigate("/dashboard");
    },
  });
};
