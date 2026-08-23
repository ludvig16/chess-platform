import "./game.css";
import Chessboard from "../../components/Chessboard/Chessboard";
import Playerbar from "../../components/Playerbar/Playerbar";
import { useNavigate, useParams } from "react-router-dom";
import { useEffect, useRef, useState } from "react";
import { Chess, Color, PieceSymbol, Square } from "chess.js";
import { PlayerData, GameState, Time, User } from "../../interfaces";
import axios from "axios";
import { useAuthStore } from "../../stores/authStore";

import { connection } from "../../connection";

type Game = {
  board: ({
    square: Square;
    type: PieceSymbol;
    color: Color;
  } | null)[];
  fen: string;
  whiteTimeMillis: string;
  blackTimeMillis: string;
  turn: "black" | "white";
  status: string;
  whitePlayerId: string;
  blackPlayerId: string;
};

async function getUser(id: string) {
  const token = sessionStorage.getItem("accessToken");

  const response = await axios.get(`http://localhost:5038/api/users/${id}`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  return response.data;
}

export default function Game() {
  const navigate = useNavigate();
  const { roomId } = useParams();
  const chess = useRef(new Chess());

  const [isConnectedToSocket, setIsConnectedToSocket] =
    useState<boolean>(false);

  const { isAuthenticated, user, checkAuth } = useAuthStore();
  const [players, setPlayers] = useState<{
    white: User;
    black: User;
  }>();

  const [currentTurn, setCurrentTurn] = useState<"white" | "black">("white");
  const [chessboardState, setChessboardState] = useState(
    chess.current.board().flat(),
  );

  const [whiteTimeMillis, setWhiteTimeMillis] = useState<Time>({
    minutes: 5,
    seconds: 0,
  });
  const [blackTimeMillis, setBlackTimeMillis] = useState<Time>({
    minutes: 5,
    seconds: 0,
  });

  const [gameState, setGameState] = useState<GameState>({
    checkmate: false,
    stalemate: false,
    draw: false,
    noTime: false,
    ongoingGame: false,
    currentTurn: "white",
    whitePlayerId: "",
    blackPlayerId: "",
    status: "Waiting",
  });

  useEffect(() => {
    checkAuth();
    console.log("YOOO", user);
  }, [isAuthenticated]);

  async function connectedToSocket() {
    await connection.start();
    setIsConnectedToSocket(true);
  }

  useEffect(() => {
    async function fetchPlayers() {
      if (!gameState.whitePlayerId || !gameState.blackPlayerId) {
        return;
      }

      try {
        const [white, black] = await Promise.all([
          getUser(gameState.whitePlayerId),
          getUser(gameState.blackPlayerId),
        ]);

        setPlayers({ white, black });
      } catch (error) {
        console.error("Failed to fetch players:", error);
      }
    }

    fetchPlayers();
  }, [gameState.whitePlayerId, gameState.blackPlayerId]);

  useEffect(() => {
    connectedToSocket();

    connection.on("ReceiveError", (error) => {
      console.log(error);
    });

    connection.on("GameState", (gameState) => {
      console.log("Game state:", gameState);

      chess.current.load(gameState.currentFen);
      setChessboardState(chess.current.board().flat());

      const whiteDate = new Date(Number(gameState.whiteTimeRemainingMs));
      const blackDate = new Date(Number(gameState.blackTimeRemainingMs));

      setWhiteTimeMillis({
        minutes: whiteDate.getMinutes(),
        seconds: whiteDate.getSeconds(),
      });

      setBlackTimeMillis({
        minutes: blackDate.getMinutes(),
        seconds: blackDate.getSeconds(),
      });

      setCurrentTurn(gameState.sideToMove.toLowerCase());

      setGameState({
        checkmate: chess.current.isCheckmate(),
        stalemate: chess.current.isStalemate(),
        draw: chess.current.isDraw(),
        noTime: false,
        ongoingGame: gameState.status === "ReadyToStart",
        currentTurn: gameState.sideToMove.toLowerCase(),
        whitePlayerId: gameState.whitePlayerId,
        blackPlayerId: gameState.blackPlayerId,
        status: gameState.status,
      });
    });

    if (isConnectedToSocket) {
      connection.invoke("FetchJoinedGame");
    }
  }, [isConnectedToSocket]);

  const whitePlayerData: PlayerData = {
    image: "/player-icons/player.jpg",
    username: players?.white.username!,
    color: "white",
    turnStatus: currentTurn === "white" ? "Your turn" : "",
    time: whiteTimeMillis,
    setTime: setWhiteTimeMillis,
  };

  const blackPlayerData: PlayerData = {
    image: "/player-icons/opponent.png",
    username: players?.black.username!,
    color: "black",
    turnStatus: currentTurn === "black" ? "Your turn" : "",
    time: blackTimeMillis,
    setTime: setBlackTimeMillis,
  };

  if (
    gameState.status == "Waiting" &&
    !gameState.ongoingGame &&
    !gameState.noTime
  ) {
    return (
      <div className="main-wrapper">
        <div className="waiting-for-opponent-div">
          <h2 className="waiting-for-opponent-text">Game Code: {roomId}</h2>
          <h2 className="waiting-for-opponent-text">Waiting for opponent</h2>
        </div>
      </div>
    );
  }

  return (
    <div className="main-wrapper">
      <h2 className="game-code">Game Code: {roomId}</h2>
      <Playerbar
        className="opponent-player-bar"
        playerData={
          String(user?.id) === gameState.whitePlayerId
            ? blackPlayerData
            : whitePlayerData
        }
        gameState={gameState}
      />
      <Chessboard
        board={chessboardState}
        chess={chess.current}
        gameState={gameState}
        playerColor={players?.white.id === user?.id ? "white" : "black"}
      />
      <Playerbar
        className="player-bar"
        playerData={
          String(user?.id) === gameState.whitePlayerId
            ? whitePlayerData
            : blackPlayerData
        }
        gameState={gameState}
      />
    </div>
  );
}
