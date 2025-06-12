import { createContext } from "react";
import type { UserDto } from "../dto/AuthDto";

export interface AuthContextType {
  user: UserDto | null;
  setUser: (user: UserDto | null) => void;
  isAuthenticated: boolean;
}

export const AuthContext = createContext<AuthContextType | null>(null);
