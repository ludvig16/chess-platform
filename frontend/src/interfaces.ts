export interface GameState {
  checkmate: boolean;
  stalemate: boolean;
  draw: boolean;
  noTime: boolean;
  ongoingGame: boolean;
  currentTurn: "white" | "black";
  whitePlayerId: string;
  blackPlayerId: string;
  status: "Waiting" | "ReadyToStart" | "InProgress" | "Finished";
}

export interface Time {
  minutes: number;
  seconds: number;
}

export interface PlayerData {
  image: string;
  username: string;
  turnStatus: string;
  color: "white" | "black";
  time: Time;
  setTime: React.Dispatch<React.SetStateAction<Time>>;
}

export interface User {
  id: number;
  username: string;
  createdAt: string;
}
