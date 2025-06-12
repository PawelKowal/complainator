import type { FC } from "react";
import { useNavigate, useLocation } from "react-router";
import { AppBar, Box, Button, Container, Toolbar, Typography } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import LogoutIcon from "@mui/icons-material/Logout";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { useAuth } from "../hooks/useAuth";
import { useCreateRetrospective } from "../hooks/useCreateRetrospective";

export const Header: FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { setUser } = useAuth();
  const createRetrospectiveMutation = useCreateRetrospective();

  const isDetailPage = location.pathname.includes("/retrospectives/");

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

  const handleLogoClick = () => {
    navigate("/dashboard");
  };

  return (
    <AppBar position="sticky" elevation={1} color="default" sx={{ width: "100%" }}>
      <Container maxWidth={false} sx={{ px: 3 }}>
        <Toolbar disableGutters>
          <Typography
            variant="h6"
            component="h1"
            onClick={handleLogoClick}
            sx={{
              flexGrow: 1,
              minWidth: 200,
              cursor: "pointer",
              "&:hover": {
                color: "primary.main",
              },
            }}
          >
            Complainator
          </Typography>
          <Box sx={{ display: "flex", gap: 2, flexShrink: 0 }}>
            {isDetailPage ? (
              <Button
                variant="contained"
                startIcon={<ArrowBackIcon />}
                onClick={() => navigate("/dashboard")}
                sx={{
                  whiteSpace: "nowrap",
                  minWidth: 180,
                  px: 3,
                }}
              >
                Powrót do listy
              </Button>
            ) : (
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
            )}
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
