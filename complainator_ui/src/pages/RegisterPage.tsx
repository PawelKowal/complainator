import type { FC } from "react";
import { useState, useCallback } from "react";
import { Link as RouterLink } from "react-router";
import { Alert, Box, Button, TextField, Typography, Link } from "@mui/material";
import { useRegister } from "../hooks/useRegister";

interface RegisterFormState {
  email: string;
  password: string;
  errors: {
    email?: string;
    password?: string;
    submit?: string;
  };
}

const initialState: RegisterFormState = {
  email: "",
  password: "",
  errors: {},
};

export const RegisterPage: FC = () => {
  const [formState, setFormState] = useState<RegisterFormState>(initialState);
  const registerMutation = useRegister();

  const validateEmail = useCallback((email: string): string | undefined => {
    if (!email) return "Email jest wymagany";
    const emailRegex = /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i;
    if (!emailRegex.test(email)) return "Niepoprawny format email";
    return undefined;
  }, []);

  const validatePassword = useCallback((password: string): string | undefined => {
    if (!password) return "Hasło jest wymagane";
    if (password.length < 8) return "Hasło musi mieć minimum 8 znaków";
    if (!/[A-Z]/.test(password)) return "Hasło musi zawierać przynajmniej jedną dużą literę";
    if (!/[0-9]/.test(password)) return "Hasło musi zawierać przynajmniej jedną cyfrę";
    if (!/[^A-Za-z0-9]/.test(password)) return "Hasło musi zawierać przynajmniej jeden znak specjalny";
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

    registerMutation.mutate(
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
              submit: error.response?.data?.message || "Wystąpił błąd",
            },
          }));
        },
      }
    );
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
      <Typography variant="h5" align="center">
        Rejestracja
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
        autoComplete="new-password"
        value={formState.password}
        onChange={handlePasswordChange}
        error={!!formState.errors.password}
        helperText={formState.errors.password}
      />

      <Button type="submit" fullWidth variant="contained" disabled={registerMutation.isPending || !isFormValid()}>
        {registerMutation.isPending ? "Rejestracja..." : "Zarejestruj się"}
      </Button>

      <Box sx={{ display: "flex", justifyContent: "center", mt: 2 }}>
        <Typography variant="body2">
          Masz już konto?{" "}
          <Link component={RouterLink} to="/login" sx={{ textDecoration: "none" }}>
            Zaloguj się
          </Link>
        </Typography>
      </Box>
    </Box>
  );
};
