import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { GenerateSuggestionsResponse } from "../dto/SuggestionDto";
import axiosInstance from "../api/axios";

export const useGenerateSuggestions = (retrospectiveId: string) => {
  const queryClient = useQueryClient();
  const queryKey = ["retrospective", retrospectiveId];

  const mutation = useMutation({
    mutationFn: async () => {
      const response = await axiosInstance.post<GenerateSuggestionsResponse>(
        `/retrospectives/${retrospectiveId}/generate-suggestions`
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey });
    },
  });

  return {
    generateSuggestions: () => mutation.mutate(),
    isGeneratingSuggestions: mutation.isPending,
    generateSuggestionsError: mutation.error,
  };
};
