import { create } from "zustand";
import axios from "axios";

type User = {
  id: number;
  username: string;
};

type AuthState = {
  isAuthenticated: boolean;
  user: User | null;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
  checkAuth: () => Promise<void>;
  createAccount: (username: string, password: string) => Promise<void>;
};

export const useAuthStore = create<AuthState>((set) => ({
  isAuthenticated: false,
  user: null,

  login: async (username, password) => {
    const response = await axios.post(
      `http://localhost:5038/api/auth/login`,
      { username, password },
      { withCredentials: true },
    );

    sessionStorage.setItem("accessToken", response.data.token);

    set({ isAuthenticated: true });
  },

  createAccount: async (username, password) => {
    const response = await axios.post(
      `http://localhost:5038/api/auth/register`,
      { username, password },
      { withCredentials: true },
    );

    set({ isAuthenticated: true, user: response.data });
  },

  logout: async () => {
    /*
    await axios.post(
      `${import.meta.env.VITE_API_BASE_URL}/api/auth/logout`,
      {},
      { withCredentials: true },
    );
    */
    sessionStorage.removeItem("accessToken");
    set({ isAuthenticated: false, user: null });
  },

  checkAuth: async () => {
    try {
      const token = sessionStorage.getItem("accessToken");

      const response = await axios.get("http://localhost:5038/api/auth/me", {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      set({ isAuthenticated: true, user: response.data });
    } catch (error) {
      set({ isAuthenticated: false, user: null });
    }
  },
}));
