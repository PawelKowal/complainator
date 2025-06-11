import type { FC } from "react";
import { Box, Typography, Button, Stack } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import type { NoteDto, NoteCategory } from "../dto/NoteDto";
import { categoryTitles } from "../constants/categoryTitles";

interface NoteColumnProps {
  category: NoteCategory;
  notes: NoteDto[];
  onAddClick: (category: NoteCategory) => void;
  isGeneratingSuggestions?: boolean;
}

export const NoteColumn: FC<NoteColumnProps> = ({ category, notes, onAddClick, isGeneratingSuggestions = false }) => {
  return (
    <Box
      sx={{
        display: "flex",
        flexDirection: "column",
        height: "100%",
        minHeight: "300px",
        maxHeight: "600px",
        bgcolor: "background.paper",
        borderRadius: 1,
        boxShadow: 1,
        p: 3,
      }}
    >
      <Typography variant="h6" sx={{ mb: 3 }}>
        {categoryTitles[category]}
      </Typography>

      <Stack spacing={2} sx={{ flex: 1, mb: 3, overflowY: "auto" }}>
        {notes.map((note) => (
          <Box
            key={note.id}
            sx={{
              p: 2.5,
              bgcolor: "grey.50",
              borderRadius: 1,
              wordBreak: "break-word",
            }}
          >
            <Typography>{note.content}</Typography>
          </Box>
        ))}
      </Stack>

      <Button
        variant="outlined"
        startIcon={<AddIcon />}
        onClick={() => onAddClick(category)}
        disabled={isGeneratingSuggestions}
        fullWidth
        sx={{ py: 1 }}
      >
        Dodaj notatkę
      </Button>
    </Box>
  );
};
