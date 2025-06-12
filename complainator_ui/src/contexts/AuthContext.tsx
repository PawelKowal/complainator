import { useState, type FC, type PropsWithChildren } from "react";
import type { UserDto } from "../dto/AuthDto";
import { AuthContext } from "./auth.context";

export const AuthProvider: FC<PropsWithChildren> = ({ children }) => {
  const [user, setUser] = useState<UserDto | null>(() => {
    const token = localStorage.getItem("token");
    const storedUser = localStorage.getItem("user");
    if (token && storedUser) {
      try {
        return JSON.parse(storedUser);
      } catch {
        localStorage.removeItem("token");
        localStorage.removeItem("user");
      }
    }
    return null;
  });

  const value = {
    user,
    setUser: (newUser: UserDto | null) => {
      setUser(newUser);
      if (newUser) {
        localStorage.setItem("user", JSON.stringify(newUser));
      } else {
        localStorage.removeItem("user");
        localStorage.removeItem("token");
      }
    },
    isAuthenticated: !!user,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
