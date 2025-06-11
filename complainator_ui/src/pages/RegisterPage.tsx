import type { FC } from "react";
import { useState, useCallback } from "react";
import { useNavigate } from "react-router";
import { useMutation } from "@tanstack/react-query";
import { Alert, Box, Button, TextField, Typography } from "@mui/material";
import type { AxiosError } from "axios";
import type { RegisterRequest, RegisterResponse } from "../dto/AuthDto";
import { useAuth } from "../contexts/AuthContext";
import axiosInstance from "../api/axios";

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
  const navigate = useNavigate();
  const { setUser } = useAuth();
  const [formState, setFormState] = useState<RegisterFormState>(initialState);

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

  const registerMutation = useMutation<RegisterResponse, AxiosError<{ message: string }>, RegisterRequest>({
    mutationFn: async (data) => {
      const response = await axiosInstance.post<RegisterResponse>("/auth/register", data);
      return response.data;
    },
    onSuccess: (data) => {
      localStorage.setItem("token", data.token);
      setUser(data.user);
      navigate("/dashboard");
    },
    onError: (error) => {
      setFormState((prev) => ({
        ...prev,
        errors: {
          ...prev.errors,
          submit: error.response?.data?.message || "Wystąpił błąd",
        },
      }));
    },
  });

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

    registerMutation.mutate({
      email: formState.email,
      password: formState.password,
    });
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
    </Box>
  );
};
