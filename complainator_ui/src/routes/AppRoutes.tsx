import type { FC } from "react";
import { Routes, Route, Navigate } from "react-router";
import { RegisterPage } from "../pages/RegisterPage";
import { LoginPage } from "../pages/LoginPage";
import { DashboardPage } from "../pages/DashboardPage";
import { RetrospectiveDetailPage } from "../pages/RetrospectiveDetailPage";
import { PublicRoute } from "../components/PublicRoute";
import { PrivateRoute } from "../components/PrivateRoute";

export const AppRoutes: FC = () => {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route
        path="/register"
        element={
          <PublicRoute>
            <RegisterPage />
          </PublicRoute>
        }
      />
      <Route
        path="/login"
        element={
          <PublicRoute>
            <LoginPage />
          </PublicRoute>
        }
      />
      <Route
        path="/dashboard"
        element={
          <PrivateRoute>
            <DashboardPage />
          </PrivateRoute>
        }
      />
      <Route
        path="/retrospectives/:id"
        element={
          <PrivateRoute>
            <RetrospectiveDetailPage />
          </PrivateRoute>
        }
      />
    </Routes>
  );
};
