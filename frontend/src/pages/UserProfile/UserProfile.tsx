import { useEffect, useState } from "react";
import axios from "axios";
import "./userprofile.css";
import { useParams } from "react-router-dom";
import { User } from "../../interfaces";

interface Game {
  id: number;
  white_player_id: number;
  black_player_id: number;
  winner: string;
  result: string;
  fen_history: string;
  created_at: string;
  finished_at: string;
}

async function getUser(username: string) {
  const response = await axios.get(
    `${import.meta.env.VITE_API_BASE_URL}/api/users/${username}`
  );
  return response.data;
}

async function getGames(username: string) {
  const response = await axios.get(
    `${import.meta.env.VITE_API_BASE_URL}/api/users/${username}/games`
  );
  return response.data;
}

export default function UserProfile() {
  const { username } = useParams();
  const [games, setGames] = useState<Game[]>([]);
  const [user, setUser] = useState<User>();

  useEffect(() => {
    if (!username) return;

    async function fetchGames() {
      try {
        const data = await getGames(username!);
        setGames(data.reverse());
      } catch (e) {}
    }

    async function fetchUser() {
      try {
        const data = await getUser(username!);
        setUser(data);
      } catch (e) {}
    }

    fetchGames();
    fetchUser();
  }, [username]);

  return (
    <div className="profile-container">
      <h2 className="profile-title">{`Past games for ${username}`}</h2>

      {games.length === 0 && (
        <div className="no-games">{`user ${username} has not played any games yet.`}</div>
      )}

      <div className="game-list">
        {games.map((game) => {
          const playedAsWhite = game.white_player_id === Number(user?.id);

          let isWinner;

          if (game.winner === "white") {
            isWinner = game.white_player_id === user?.id;
          }

          if (game.winner === "black") {
            isWinner = game.black_player_id === user?.id;
          }

          return (
            <div key={game.id} className="game-card">
              <div className="game-header">
                <span
                  className={`result-badge ${
                    isWinner ? "result-win" : "result-loss"
                  }`}
                >
                  {isWinner ? "Win" : "Loss"}
                </span>

                <span className="game-date">
                  {new Date(game.created_at).toLocaleDateString()}{" "}
                  {new Date(game.created_at).toLocaleTimeString()}
                </span>
              </div>

              <div className="game-info">
                <div>
                  <strong>They played:</strong>{" "}
                  {playedAsWhite ? "White" : "Black"}
                </div>
                <div>
                  <strong>Result:</strong> {game.result}
                </div>
                <div>
                  <strong>Moves:</strong> {game.fen_history.length}
                </div>
                <div>
                  <strong>Duration:</strong>{" "}
                  {(() => {
                    const durationInSeconds =
                      (new Date(game.finished_at).getTime() -
                        new Date(game.created_at).getTime()) /
                      1000;
                    const minutes = Math.floor(durationInSeconds / 60);
                    const seconds = Math.round(durationInSeconds % 60);

                    if (minutes > 0) {
                      return `${minutes} minutes ${seconds} seconds`;
                    } else {
                      return `${seconds} seconds`;
                    }
                  })()}
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
