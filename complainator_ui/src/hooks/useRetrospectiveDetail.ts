import { useQuery } from "@tanstack/react-query";
import type { RetrospectiveDetailResponse } from "../dto/RetrospectiveDto";
import axiosInstance from "../api/axios";

export const useRetrospectiveDetail = (retrospectiveId: string) => {
  return useQuery<RetrospectiveDetailResponse>({
    queryKey: ["retrospective", retrospectiveId],
    queryFn: async () => {
      try {
        const response = await axiosInstance.get<RetrospectiveDetailResponse>(`/retrospectives/${retrospectiveId}`);
        return response.data;
      } catch (error) {
        console.error("Error fetching retrospective details:", error);
        throw error;
      }
    },
    retry: 1,
    refetchOnWindowFocus: true,
    staleTime: 30000, // Consider data fresh for 30 seconds
  });
};
