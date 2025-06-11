import type { FC } from "react";
import { useState } from "react";
import { Box } from "@mui/material";
import { Header } from "../components/Header";
import { InfiniteScrollList } from "../components/InfiniteScrollList";
import { ErrorAlert } from "../components/ErrorAlert";
import { useRetrospectives } from "../hooks/useRetrospectives";

export const DashboardPage: FC = () => {
  const [errorMessage, setErrorMessage] = useState<string | undefined>();
  const { data, isLoading, isFetchingNextPage, hasNextPage, fetchNextPage, error } = useRetrospectives();

  // Show error alert when query fails
  if (error) {
    setErrorMessage("Nie udało się pobrać listy retrospektyw");
  }

  const allItems = data?.pages.flatMap((page) => page.items) ?? [];

  return (
    <Box sx={{ display: "flex", flexDirection: "column", minHeight: "100vh" }}>
      <Header />
      <Box component="main" sx={{ flex: 1, p: 3 }}>
        <InfiniteScrollList
          items={allItems}
          isLoading={isLoading}
          isFetchingNextPage={isFetchingNextPage}
          hasNextPage={hasNextPage}
          fetchNextPage={fetchNextPage}
        />
      </Box>
      <ErrorAlert open={!!errorMessage} message={errorMessage} onClose={() => setErrorMessage(undefined)} />
    </Box>
  );
};
