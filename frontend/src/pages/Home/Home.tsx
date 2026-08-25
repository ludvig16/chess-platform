import "./home.css";
import { Link, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { useAuthStore } from "../../stores/authStore";
import toast, { Toaster } from "react-hot-toast";
import axios from "axios";
import { connection } from "../../connection";

export default function Home() {
  const [code, setCode] = useState("");
  const navigate = useNavigate();
  const { isAuthenticated, user, checkAuth, logout } = useAuthStore();

  const [ongoingGameCode, setOngoingGameCode] = useState<string>();

  useEffect(() => {
    checkAuth();
  }, []);

  useEffect(() => {
    async function initializeSocket() {
      try {
        if (connection.state === "Disconnected") {
          await connection.start();
        }

        await connection.invoke("FetchJoinedGame");
      } catch (error) {
        console.error(error);
      }
    }

    initializeSocket();
  }, []);

  useEffect(() => {
    const handleGameState = (gameState: any) => {
      setOngoingGameCode(gameState.id);
    };

    const handleError = (error: any) => {
      toast.error(error.description);
    };

    const handleJoinedGame = (game: any) => {
      navigate(`/play/${game.id}`);
      location.reload();
    };

    connection.on("GameState", handleGameState);
    connection.on("ReceiveError", handleError);
    connection.on("JoinedGame", handleJoinedGame);

    return () => {
      connection.off("GameState", handleGameState);
      connection.off("ReceiveError", handleError);
      connection.off("JoinedGame", handleJoinedGame);
    };
  }, [navigate]);

  async function handleCreateGame() {
    if (ongoingGameCode) {
      return toast.error("You have an ongoing game");
    }

    const token = sessionStorage.getItem("accessToken");

    try {
      const response = await axios.post(
        "http://localhost:5038/api/games",
        {
          ChosenColor: "White",
          TimeLimitMs: 60000,
        },
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        },
      );

      navigate(`/play/${response.data.id}`);
      location.reload();
    } catch (error: any) {
      console.log(error.response.data);
    }
  }

  function handleJoinGame(gameId: string) {
    connection.invoke("JoinGame", Number(gameId));
  }

  return (
    <div className="home-container">
      <Toaster />
      <h1 className="home-title">Chess Online</h1>

      {!isAuthenticated && (
        <div className="home-links">
          <Link className="home-link" to="/login">
            Login
          </Link>
          <Link className="home-link" to="/register">
            Create Account
          </Link>
        </div>
      )}

      {isAuthenticated && (
        <>
          <button className="home-btn" onClick={handleCreateGame}>
            Create Game
          </button>

          <div className="input-btn-container">
            <input
              className="home-input"
              placeholder="Enter game code"
              onChange={(e) => setCode(e.target.value)}
            />

            <button className="home-btn" onClick={() => handleJoinGame(code)}>
              Join Game
            </button>
          </div>
        </>
      )}

      {isAuthenticated && (
        <button className="home-btn" onClick={() => logout()}>
          Log out
        </button>
      )}

      {isAuthenticated ? (
        <div className="username-container">
          <p>You are playing as</p>
          <Link className="home-link" to={`/profile/${user?.username}`}>
            {`${user?.username}`}
          </Link>
        </div>
      ) : (
        <p>Log in or create an account to play</p>
      )}

      {ongoingGameCode && (
        <div className="username-container">
          <p>You have an ongoing game at</p>
          <Link
            className="home-link"
            to={`/play/${ongoingGameCode}`}
            reloadDocument
          >
            {`${ongoingGameCode}`}
          </Link>
        </div>
      )}
    </div>
  );
}
