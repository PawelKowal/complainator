import type { FC } from "react";
import { Button } from "@mui/material";
import AutoFixHighIcon from "@mui/icons-material/AutoFixHigh";
import { useGenerateSuggestions } from "../hooks/useGenerateSuggestions";

interface GenerateSuggestionsButtonProps {
  retrospectiveId: string;
  disabled?: boolean;
}

export const GenerateSuggestionsButton: FC<GenerateSuggestionsButtonProps> = ({
  retrospectiveId,
  disabled = false,
}) => {
  const { generateSuggestions, isGeneratingSuggestions } = useGenerateSuggestions(retrospectiveId);

  return (
    <Button
      variant="contained"
      color="primary"
      startIcon={<AutoFixHighIcon />}
      onClick={() => generateSuggestions()}
      disabled={disabled || isGeneratingSuggestions}
      sx={{ mt: 3 }}
    >
      {isGeneratingSuggestions ? "Generowanie wniosków..." : "Generuj wnioski AI"}
    </Button>
  );
};
