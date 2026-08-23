import { SQUARES, Square } from "chess.js";

export function useFlippedBoard(playerColor: "white" | "black", board: any[]) {
  const flipped = playerColor === "black";

  const squares = flipped ? [...SQUARES].reverse() : SQUARES;
  const boardArr = flipped ? [...board].reverse() : board;

  function transformCoords(x: number, y: number) {
    return flipped ? { x: 7 - x, y: 7 - y } : { x, y };
  }

  function getPosition(x: number, y: number): Square {
    const file = ["a", "b", "c", "d", "e", "f", "g", "h"][x];
    return `${file}${8 - y}` as Square;
  }

  return { squares, boardArr, transformCoords, getPosition };
}
