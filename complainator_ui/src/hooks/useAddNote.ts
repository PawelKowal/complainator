import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { CreateNoteRequest, CreateNoteResponse } from "../dto/NoteDto";
import axiosInstance from "../api/axios";

export const useAddNote = (retrospectiveId: string) => {
  const queryClient = useQueryClient();
  const queryKey = ["retrospective", retrospectiveId];

  const mutation = useMutation({
    mutationFn: async (request: CreateNoteRequest) => {
      const response = await axiosInstance.post<CreateNoteResponse>(
        `/retrospectives/${retrospectiveId}/notes`,
        request
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey });
    },
  });

  return {
    addNote: mutation.mutate,
    isAddingNote: mutation.isPending,
    addNoteError: mutation.error,
  };
};
