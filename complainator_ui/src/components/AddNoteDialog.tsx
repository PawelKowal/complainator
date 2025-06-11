import type { FC } from "react";
import { useState } from "react";
import { Dialog, DialogTitle, DialogContent, DialogActions, Button, TextField, Typography } from "@mui/material";
import type { NoteCategory } from "../dto/NoteDto";
import { useAddNote } from "../hooks/useAddNote";
import { categoryTitles } from "../constants/categoryTitles";

interface AddNoteDialogProps {
  open: boolean;
  onClose: () => void;
  category: NoteCategory;
  retrospectiveId: string;
}

export const AddNoteDialog: FC<AddNoteDialogProps> = ({ open, onClose, category, retrospectiveId }) => {
  const [content, setContent] = useState("");
  const { addNote, isAddingNote } = useAddNote(retrospectiveId);

  const handleSubmit = async () => {
    if (!content.trim()) return;

    await addNote(
      { category, content },
      {
        onSuccess: () => {
          setContent("");
          onClose();
        },
      }
    );
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Dodaj notatkę</DialogTitle>
      <DialogContent>
        <Typography variant="subtitle1" sx={{ mb: 2 }}>
          Kategoria: {categoryTitles[category]}
        </Typography>
        <TextField
          autoFocus
          multiline
          rows={4}
          fullWidth
          label="Treść notatki"
          value={content}
          onChange={(e) => setContent(e.target.value)}
          disabled={isAddingNote}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={isAddingNote}>
          Anuluj
        </Button>
        <Button onClick={handleSubmit} variant="contained" disabled={!content.trim() || isAddingNote}>
          {isAddingNote ? "Zapisywanie..." : "Zapisz"}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
