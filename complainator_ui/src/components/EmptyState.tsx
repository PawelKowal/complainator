import type { FC } from "react";
import { Box, Typography } from "@mui/material";
import InboxIcon from "@mui/icons-material/Inbox";

interface EmptyStateProps {
  message?: string;
}

export const EmptyState: FC<EmptyStateProps> = ({ message = "Brak retrospektyw do wyświetlenia" }) => {
  return (
    <Box
      sx={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        gap: 2,
        py: 8,
      }}
    >
      <InboxIcon sx={{ fontSize: 64, color: "text.secondary" }} />
      <Typography variant="h6" color="text.secondary">
        {message}
      </Typography>
    </Box>
  );
};
