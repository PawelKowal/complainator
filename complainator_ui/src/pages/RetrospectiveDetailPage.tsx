import type { FC } from "react";
import { useState, useEffect } from "react";
import { useParams, Navigate } from "react-router";
import { Box, CircularProgress } from "@mui/material";
import { Header } from "../components/Header";
import { ErrorAlert } from "../components/ErrorAlert";
import { NoteColumn } from "../components/NoteColumn";
import { AddNoteDialog } from "../components/AddNoteDialog";
import { GenerateSuggestionsButton } from "../components/GenerateSuggestionsButton";
import { SuggestionsList } from "../components/SuggestionsList";
import { useRetrospectiveDetail } from "../hooks/useRetrospectiveDetail";
import type { NoteCategory } from "../dto/NoteDto";

export const RetrospectiveDetailPage: FC = () => {
  const { id } = useParams<{ id: string }>();
  const [errorMessage, setErrorMessage] = useState<string | undefined>();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState<NoteCategory | null>(null);
  const { data, isLoading, error } = useRetrospectiveDetail(id || "");

  useEffect(() => {
    if (error) {
      setErrorMessage("Wystąpił błąd podczas pobierania danych retrospektywy");
    }
  }, [error]);

  const handleAddNoteClick = (category: NoteCategory) => {
    setSelectedCategory(category);
    setDialogOpen(true);
  };

  const handleDialogClose = () => {
    setDialogOpen(false);
    setSelectedCategory(null);
  };

  if (!id) {
    return <Navigate to="/dashboard" replace />;
  }

  if (isLoading) {
    return (
      <Box
        sx={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          minHeight: "100vh",
        }}
      >
        <CircularProgress />
      </Box>
    );
  }

  if (!data) {
    return null;
  }

  const hasAnyNotes =
    data.notes &&
    (data.notes.improvementArea.length > 0 || data.notes.observation.length > 0 || data.notes.success.length > 0);

  return (
    <Box sx={{ display: "flex", flexDirection: "column", minHeight: "100vh" }}>
      <Header />
      <Box
        component="main"
        sx={{
          flex: 1,
          p: 3,
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
        }}
      >
        <Box
          sx={{
            width: "100%",
            maxWidth: "1400px",
          }}
        >
          <Box sx={{ display: "grid", gridTemplateColumns: { xs: "1fr", md: "repeat(3, 1fr)" }, gap: 3 }}>
            <Box>
              <NoteColumn
                category="improvementArea"
                notes={data.notes.improvementArea}
                onAddClick={handleAddNoteClick}
                hasSuggestions={data.suggestions?.length > 0}
              />
            </Box>
            <Box>
              <NoteColumn
                category="observation"
                notes={data.notes.observation}
                onAddClick={handleAddNoteClick}
                hasSuggestions={data.suggestions?.length > 0}
              />
            </Box>
            <Box>
              <NoteColumn
                category="success"
                notes={data.notes.success}
                onAddClick={handleAddNoteClick}
                hasSuggestions={data.suggestions?.length > 0}
              />
            </Box>
          </Box>

          <Box sx={{ display: "flex", justifyContent: "center", mt: 4 }}>
            <GenerateSuggestionsButton retrospectiveId={id} disabled={!hasAnyNotes || data.suggestions?.length > 0} />
          </Box>

          {data.suggestions?.length > 0 && (
            <Box sx={{ mt: 4 }}>
              <SuggestionsList suggestions={data.suggestions} retrospectiveId={id} title="Sugestie" />
            </Box>
          )}
        </Box>
      </Box>

      {selectedCategory && (
        <AddNoteDialog open={dialogOpen} onClose={handleDialogClose} category={selectedCategory} retrospectiveId={id} />
      )}

      <ErrorAlert open={!!errorMessage} message={errorMessage} onClose={() => setErrorMessage(undefined)} />
    </Box>
  );
};
