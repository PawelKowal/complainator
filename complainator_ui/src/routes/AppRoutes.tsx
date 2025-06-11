import type { FC } from "react";
import { Routes, Route, Navigate } from "react-router";
import { RegisterPage } from "../pages/RegisterPage";

export const AppRoutes: FC = () => {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/register" replace />} />
      <Route path="/register" element={<RegisterPage />} />
    </Routes>
  );
};
