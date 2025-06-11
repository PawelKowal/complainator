import type { FC } from "react";
import { useNavigate } from "react-router";
import { AppBar, Box, Button, Container, Toolbar, Typography } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import LogoutIcon from "@mui/icons-material/Logout";
import { useAuth } from "../contexts/AuthContext";
import { useCreateRetrospective } from "../hooks/useCreateRetrospective";

export const Header: FC = () => {
  const navigate = useNavigate();
  const { setUser } = useAuth();
  const createRetrospectiveMutation = useCreateRetrospective();

  const handleCreateRetrospective = async () => {
    try {
      const result = await createRetrospectiveMutation.mutateAsync();
      navigate(`/retrospectives/${result.id}`);
    } catch (error) {
      // Error will be handled by the global error handler
      console.error("Failed to create retrospective:", error);
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    setUser(null);
    navigate("/login");
  };

  return (
    <AppBar position="sticky" elevation={1} color="default" sx={{ width: "100%" }}>
      <Container maxWidth="lg">
        <Toolbar disableGutters>
          <Typography variant="h6" component="h1" sx={{ flexGrow: 1, minWidth: 200 }}>
            Complainator
          </Typography>
          <Box sx={{ display: "flex", gap: 2, flexShrink: 0 }}>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={handleCreateRetrospective}
              disabled={createRetrospectiveMutation.isPending}
              sx={{
                whiteSpace: "nowrap",
                minWidth: 180,
                px: 3,
              }}
            >
              {createRetrospectiveMutation.isPending ? "Tworzenie..." : "Nowa retrospektywa"}
            </Button>
            <Button
              variant="outlined"
              color="inherit"
              startIcon={<LogoutIcon />}
              onClick={handleLogout}
              sx={{
                whiteSpace: "nowrap",
                minWidth: 140,
                px: 3,
              }}
            >
              Wyloguj się
            </Button>
          </Box>
        </Toolbar>
      </Container>
    </AppBar>
  );
};
