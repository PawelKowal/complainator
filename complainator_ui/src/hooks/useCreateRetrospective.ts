import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { CreateRetrospectiveResponse } from "../dto/RetrospectiveDto";
import axiosInstance from "../api/axios";

export const useCreateRetrospective = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      const response = await axiosInstance.post<CreateRetrospectiveResponse>("/retrospectives");
      return response.data;
    },
    onSuccess: () => {
      // Invalidate retrospectives query to trigger a refetch
      queryClient.invalidateQueries({ queryKey: ["retrospectives"] });
    },
  });
};
