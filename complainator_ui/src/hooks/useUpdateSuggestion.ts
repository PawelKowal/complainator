import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { UpdateSuggestionRequest } from "../dto/SuggestionDto";
import axiosInstance from "../api/axios";

export const useUpdateSuggestion = (retrospectiveId: string) => {
  const queryClient = useQueryClient();
  const queryKey = ["retrospective", retrospectiveId];

  const mutation = useMutation({
    mutationFn: async ({ suggestionId, request }: { suggestionId: string; request: UpdateSuggestionRequest }) => {
      const response = await axiosInstance.patch(`/suggestions/${suggestionId}`, request);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey });
    },
  });

  return {
    updateSuggestion: mutation.mutate,
    isUpdatingSuggestion: mutation.isPending,
    updateSuggestionError: mutation.error,
  };
};
