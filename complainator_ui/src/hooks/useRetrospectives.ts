import { useInfiniteQuery } from "@tanstack/react-query";
import type { RetrospectiveListRequest, RetrospectiveListResponse } from "../dto/RetrospectiveDto";
import axiosInstance from "../api/axios";

export const useRetrospectives = (perPage: number = 10, sort: "dateDesc" | "dateAsc" = "dateDesc") => {
  return useInfiniteQuery({
    queryKey: ["retrospectives", { perPage, sort }],
    queryFn: async ({ pageParam = 1 }) => {
      const response = await axiosInstance.get<RetrospectiveListResponse>("/retrospectives", {
        params: {
          page: pageParam,
          perPage,
          sort,
        } satisfies RetrospectiveListRequest,
      });
      return response.data;
    },
    getNextPageParam: (lastPage) => {
      const hasNextPage = lastPage.page * lastPage.perPage < lastPage.total;
      return hasNextPage ? lastPage.page + 1 : undefined;
    },
    initialPageParam: 1,
  });
};
