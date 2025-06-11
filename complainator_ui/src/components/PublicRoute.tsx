import type { FC, PropsWithChildren } from "react";
import { Navigate, useLocation } from "react-router";
import { useAuth } from "../contexts/AuthContext";

export const PublicRoute: FC<PropsWithChildren> = ({ children }) => {
  const { isAuthenticated } = useAuth();
  const location = useLocation();
  const from = location.state?.from?.pathname || "/dashboard";

  if (isAuthenticated) {
    // Redirect to the page user came from or dashboard
    return <Navigate to={from} replace />;
  }

  return <>{children}</>;
};
