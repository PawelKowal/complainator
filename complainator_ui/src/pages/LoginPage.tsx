import type { FC } from "react";
import { useState, useCallback } from "react";
import { Link as RouterLink } from "react-router";
import { Alert, Box, Button, TextField, Typography, Link } from "@mui/material";
import { useLogin } from "../hooks/useLogin";

interface LoginFormState {
  email: string;
  password: string;
  errors: {
    email?: string;
    password?: string;
    submit?: string;
  };
}

const initialState: LoginFormState = {
  email: "",
  password: "",
  errors: {},
};

export const LoginPage: FC = () => {
  const [formState, setFormState] = useState<LoginFormState>(initialState);
  const loginMutation = useLogin();

  const validateEmail = useCallback((email: string): string | undefined => {
    if (!email) return "Email jest wymagany";
    const emailRegex = /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i;
    if (!emailRegex.test(email)) return "Niepoprawny format email";
    return undefined;
  }, []);

  const validatePassword = useCallback((password: string): string | undefined => {
    if (!password) return "Hasło jest wymagane";
    return undefined;
  }, []);

  const isFormValid = useCallback((): boolean => {
    return !validateEmail(formState.email) && !validatePassword(formState.password);
  }, [formState.email, formState.password, validateEmail, validatePassword]);

  const handleEmailChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const email = e.target.value;
    const emailError = validateEmail(email);
    setFormState((prev) => ({
      ...prev,
      email,
      errors: { ...prev.errors, email: emailError },
    }));
  };

  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const password = e.target.value;
    const passwordError = validatePassword(password);
    setFormState((prev) => ({
      ...prev,
      password,
      errors: { ...prev.errors, password: passwordError },
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!isFormValid()) {
      return;
    }

    loginMutation.mutate(
      {
        email: formState.email,
        password: formState.password,
      },
      {
        onError: (error) => {
          setFormState((prev) => ({
            ...prev,
            errors: {
              ...prev.errors,
              submit: error.response?.data?.message || "Nieprawidłowy email lub hasło",
            },
          }));
        },
      }
    );
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
      <Typography variant="h5" align="center">
        Logowanie
      </Typography>

      {formState.errors.submit && <Alert severity="error">{formState.errors.submit}</Alert>}

      <TextField
        required
        fullWidth
        id="email"
        label="Email"
        name="email"
        autoComplete="email"
        autoFocus
        value={formState.email}
        onChange={handleEmailChange}
        error={!!formState.errors.email}
        helperText={formState.errors.email}
      />

      <TextField
        required
        fullWidth
        name="password"
        label="Hasło"
        type="password"
        id="password"
        autoComplete="current-password"
        value={formState.password}
        onChange={handlePasswordChange}
        error={!!formState.errors.password}
        helperText={formState.errors.password}
      />

      <Button type="submit" fullWidth variant="contained" disabled={loginMutation.isPending || !isFormValid()}>
        {loginMutation.isPending ? "Logowanie..." : "Zaloguj się"}
      </Button>

      <Box sx={{ display: "flex", justifyContent: "center", mt: 2 }}>
        <Typography variant="body2">
          Nie masz jeszcze konta?{" "}
          <Link component={RouterLink} to="/register" sx={{ textDecoration: "none" }}>
            Zarejestruj się
          </Link>
        </Typography>
      </Box>
    </Box>
  );
};
