import type { FC } from "react";
import { Box, Typography, Button, Stack } from "@mui/material";
import CheckIcon from "@mui/icons-material/Check";
import CloseIcon from "@mui/icons-material/Close";
import type { SuggestionDto } from "../dto/SuggestionDto";
import { useUpdateSuggestion } from "../hooks/useUpdateSuggestion";

interface SuggestionItemProps {
  suggestion: SuggestionDto;
  retrospectiveId: string;
  showActions?: boolean;
}

export const SuggestionItem: FC<SuggestionItemProps> = ({ suggestion, retrospectiveId, showActions = true }) => {
  const { updateSuggestion, isUpdatingSuggestion } = useUpdateSuggestion(retrospectiveId);

  const handleAction = (status: "Accepted" | "Rejected") => {
    updateSuggestion({
      suggestionId: suggestion.id,
      request: { status },
    });
  };

  return (
    <Box
      sx={{
        p: 2,
        bgcolor: "background.paper",
        borderRadius: 1,
        boxShadow: 1,
      }}
    >
      <Typography sx={{ mb: showActions ? 2 : 0 }}>{suggestion.suggestionText}</Typography>
      {showActions && (
        <Stack direction="row" spacing={1} justifyContent="flex-end">
          <Button
            variant="outlined"
            color="error"
            startIcon={<CloseIcon />}
            onClick={() => handleAction("Rejected")}
            disabled={isUpdatingSuggestion}
          >
            Odrzuć
          </Button>
          <Button
            variant="contained"
            color="success"
            startIcon={<CheckIcon />}
            onClick={() => handleAction("Accepted")}
            disabled={isUpdatingSuggestion}
          >
            Zaakceptuj
          </Button>
        </Stack>
      )}
    </Box>
  );
};
