import type { FC } from "react";
import { Alert, Snackbar } from "@mui/material";

interface ErrorAlertProps {
  message?: string;
  onClose: () => void;
  open: boolean;
}

export const ErrorAlert: FC<ErrorAlertProps> = ({
  message = "Wystąpił błąd, spróbuj ponownie później",
  onClose,
  open,
}) => {
  return (
    <Snackbar
      open={open}
      autoHideDuration={6000}
      onClose={onClose}
      anchorOrigin={{ vertical: "top", horizontal: "center" }}
    >
      <Alert onClose={onClose} severity="error" variant="filled" sx={{ width: "100%" }}>
        {message}
      </Alert>
    </Snackbar>
  );
};
