import type { FC } from "react";
import { Box, Typography, Stack } from "@mui/material";
import type { SuggestionDto } from "../dto/SuggestionDto";
import { SuggestionItem } from "./SuggestionItem";

interface SuggestionsListProps {
  suggestions: SuggestionDto[];
  retrospectiveId: string;
  title?: string;
  showActions?: boolean;
}

export const SuggestionsList: FC<SuggestionsListProps> = ({
  suggestions,
  retrospectiveId,
  title = "Sugerowane wnioski",
}) => {
  if (!suggestions.length) {
    return null;
  }

  return (
    <Box sx={{ mt: 4 }}>
      <Typography variant="h6" sx={{ mb: 2 }}>
        {title}
      </Typography>
      <Stack spacing={2}>
        {suggestions.map(
          (suggestion) =>
            suggestion.status !== "Rejected" && (
              <SuggestionItem
                key={suggestion.id}
                suggestion={suggestion}
                retrospectiveId={retrospectiveId}
                showActions={suggestion.status === "Pending"}
              />
            )
        )}
      </Stack>
    </Box>
  );
};
