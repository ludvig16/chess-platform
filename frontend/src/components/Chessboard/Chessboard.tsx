import "./chessboard.css";
import Tile from "../Tile/Tile";
import { useRef, useState, useEffect } from "react";
import { Chess, Square, Color, PieceSymbol } from "chess.js";
import { useNavigate, useParams } from "react-router-dom";
const moveAudio = new Audio("/audio/move.mp3");
const captureAudio = new Audio("/audio/capture.mp3");
import { useFlippedBoard } from "../../hooks/useFlippedBoard";
import { GameState } from "../../interfaces";

import { connection } from "../../connection";

interface ChessboardProps {
  board: ({
    square: Square;
    type: PieceSymbol;
    color: Color;
  } | null)[];
  chess: Chess;
  gameState: GameState;
  playerColor: "white" | "black";
}

export default function Chessboard({
  board,
  chess,
  gameState,
  playerColor,
}: ChessboardProps) {
  const chessboardRef = useRef<HTMLDivElement>(null);

  const { squares, boardArr, transformCoords, getPosition } = useFlippedBoard(
    playerColor,
    board,
  );

  const [activePiece, setActivePiece] = useState<HTMLElement | null>(null);
  const [grabSquare, setGrabSquare] = useState<string>("");

  const { roomId } = useParams();

  const navigate = useNavigate();

  useEffect(() => {
    const disableDrag = (e: DragEvent) => e.preventDefault();
    document.addEventListener("dragstart", disableDrag);
    return () => document.removeEventListener("dragstart", disableDrag);
  }, []);

  useEffect(() => {
    connection.start();
  }, [roomId]);

  useEffect(() => {
    const handleMouseUp = () => {
      if (activePiece) {
        activePiece.style.removeProperty("position");
        activePiece.style.removeProperty("top");
        activePiece.style.removeProperty("left");
        setActivePiece(null);
      }
    };
    document.addEventListener("mouseup", handleMouseUp);

    return () => {
      document.removeEventListener("mouseup", handleMouseUp);
    };
  }, [activePiece]);

  function grabPiece(e: React.MouseEvent<HTMLDivElement, MouseEvent>): void {
    const element = e.target as HTMLElement;
    const chessboard = chessboardRef.current;

    const isGameOver =
      gameState.checkmate ||
      gameState.stalemate ||
      gameState.draw ||
      gameState.noTime;

    if (
      !isGameOver &&
      chessboard &&
      element.classList.contains(`chess-piece-${playerColor}`)
    ) {
      if (activePiece) return;

      let x = Math.floor((e.clientX - chessboard.offsetLeft) / 75);
      let y = Math.floor((e.clientY - chessboard.offsetTop) / 75);

      ({ x, y } = transformCoords(x, y));

      setGrabSquare(getPosition(x, y));
      setActivePiece(element);
    }
  }

  function movePiece(e: React.MouseEvent<HTMLDivElement, MouseEvent>): void {
    if (activePiece && chessboardRef.current) {
      const size_of_half_piece = 37.5;
      const min_x = chessboardRef.current.offsetLeft - size_of_half_piece;
      const min_y = chessboardRef.current.offsetTop - size_of_half_piece;
      const max_x =
        chessboardRef.current.offsetLeft +
        chessboardRef.current.clientWidth -
        size_of_half_piece;
      const max_y =
        chessboardRef.current.offsetTop +
        chessboardRef.current.clientHeight -
        size_of_half_piece;

      const x = e.clientX - 40;
      const y = e.clientY - 50;

      activePiece.style.zIndex = "10000";

      if (x < min_x) {
        activePiece.style.left = `${min_x}px`;
      } else if (x > max_x) {
        activePiece.style.left = `${max_x}px`;
      } else {
        activePiece.style.left = `${x}px`;
      }

      if (y < min_y) {
        activePiece.style.top = `${min_y}px`;
      } else if (y > max_y) {
        activePiece.style.top = `${max_y}px`;
      } else {
        activePiece.style.top = `${y}px`;
      }
    }
  }

  function dropPiece(e: React.MouseEvent<HTMLDivElement, MouseEvent>): void {
    const chessboard = chessboardRef.current;
    if (activePiece && chessboard) {
      let x = Math.floor((e.clientX - chessboard.offsetLeft) / 75);
      let y = Math.floor((e.clientY - chessboard.offsetTop) / 75);

      ({ x, y } = transformCoords(x, y));

      const targetSquare = getPosition(x, y);

      try {
        const move = chess.move({ from: grabSquare, to: targetSquare });

        connection.invoke("MakeMove", Number(roomId), grabSquare, targetSquare);

        move.captured ? captureAudio.play() : moveAudio.play();
      } catch (error) {
        activePiece.style.removeProperty("position");
        activePiece.style.removeProperty("top");
        activePiece.style.removeProperty("left");
        setActivePiece(null);
      }
      setActivePiece(null);
      setGrabSquare("");
    }
  }

  function isHighlighted(square: string): boolean {
    if (grabSquare) {
      const validMoves = chess.moves({
        square: grabSquare as Square,
        verbose: true,
      });
      return validMoves.some((move) => move.to === square);
    }
    return false;
  }

  return (
    <div
      className="chessboard"
      onMouseMove={(e) => movePiece(e)}
      onMouseDown={(e) => grabPiece(e)}
      onMouseUp={(e) => dropPiece(e)}
      ref={chessboardRef}
    >
      <div
        className="game-over-screen"
        style={{
          display:
            gameState.checkmate ||
            gameState.stalemate ||
            gameState.draw ||
            gameState.noTime
              ? "block"
              : "none",
        }}
      >
        {gameState.checkmate && (
          <h1>
            {gameState.currentTurn === "black"
              ? "White wins by checkmate"
              : "Black wins by checkmate"}
          </h1>
        )}
        {gameState.stalemate && <h1>Stalemate!</h1>}
        {gameState.draw && <h1>Draw!</h1>}
        {gameState.noTime && (
          <h1>
            {gameState.currentTurn === "black"
              ? "White wins on time"
              : "Black wins on time"}
          </h1>
        )}
        <button className="game-over-btn" onClick={() => navigate("/")}>
          Home
        </button>
      </div>

      {squares.map((square, i) => (
        <Tile
          key={i}
          number={(i + Math.floor(i / 8)) % 2}
          image={
            boardArr[i]
              ? `/pieces/${boardArr[i].color}${boardArr[i].type}.png`
              : undefined
          }
          highlight={isHighlighted(square)}
          check={chess.inCheck()}
          currentTurn={gameState.currentTurn}
          color={boardArr[i] ? boardArr[i].color : null}
        />
      ))}
    </div>
  );
}
