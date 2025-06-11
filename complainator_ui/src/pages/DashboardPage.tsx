import type { FC } from "react";
import { useNavigate } from "react-router";
import { Box, Button, Typography } from "@mui/material";
import { useAuth } from "../contexts/AuthContext";

export const DashboardPage: FC = () => {
  const navigate = useNavigate();
  const { setUser } = useAuth();

  const handleLogout = () => {
    // Wyczyść token i dane użytkownika
    localStorage.removeItem("token");
    setUser(null);
    navigate("/login");
  };

  return (
    <Box sx={{ display: "flex", flexDirection: "column", gap: 3, alignItems: "center" }}>
      <Typography variant="h4" component="h1">
        Dashboard
      </Typography>

      <Button variant="contained" color="error" onClick={handleLogout} sx={{ mt: 2 }}>
        Wyloguj się
      </Button>
    </Box>
  );
};
