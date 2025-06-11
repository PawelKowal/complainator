import type { FC, PropsWithChildren } from "react";
import { Box, Container } from "@mui/material";

export const PublicLayout: FC<PropsWithChildren> = ({ children }) => {
  return (
    <Box
      sx={{
        display: "flex",
        flexDirection: "column",
        minHeight: "100vh",
        bgcolor: "background.default",
      }}
    >
      <Container
        maxWidth="sm"
        sx={{
          flex: 1,
          display: "flex",
          flexDirection: "column",
          justifyContent: "center",
          py: 4,
        }}
      >
        <Box
          sx={{
            bgcolor: "background.paper",
            borderRadius: 1,
            boxShadow: 1,
            p: 4,
          }}
        >
          {children}
        </Box>
      </Container>
    </Box>
  );
};
