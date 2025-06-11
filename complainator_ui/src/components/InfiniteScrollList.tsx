import type { FC } from "react";
import { useEffect, useRef } from "react";
import { Box, Stack } from "@mui/material";
import type { RetrospectiveListItem } from "../dto/RetrospectiveDto";
import { RetrospectiveCard } from "./RetrospectiveCard";
import { LoadingSkeleton } from "./LoadingSkeleton";
import { EmptyState } from "./EmptyState";

interface InfiniteScrollListProps {
  items: RetrospectiveListItem[];
  isLoading: boolean;
  isFetchingNextPage: boolean;
  hasNextPage?: boolean;
  fetchNextPage: () => void;
}

export const InfiniteScrollList: FC<InfiniteScrollListProps> = ({
  items,
  isLoading,
  isFetchingNextPage,
  hasNextPage,
  fetchNextPage,
}) => {
  const observerTarget = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
          fetchNextPage();
        }
      },
      { threshold: 0.1 }
    );

    if (observerTarget.current) {
      observer.observe(observerTarget.current);
    }

    return () => observer.disconnect();
  }, [fetchNextPage, hasNextPage, isFetchingNextPage]);

  if (isLoading) {
    return <LoadingSkeleton />;
  }

  if (!items.length) {
    return <EmptyState />;
  }

  return (
    <Box sx={{ width: "100%", maxWidth: 800, mx: "auto" }}>
      <Stack spacing={2}>
        {items.map((item) => (
          <RetrospectiveCard key={item.id} item={item} />
        ))}
      </Stack>
      <Box ref={observerTarget} sx={{ height: 20, mt: 2 }} />
      {isFetchingNextPage && (
        <Box sx={{ mt: 2 }}>
          <LoadingSkeleton />
        </Box>
      )}
    </Box>
  );
};
