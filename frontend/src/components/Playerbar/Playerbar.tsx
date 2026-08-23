import "./playerbar.css";
import Timer from "./Timer";
import { GameState, PlayerData } from "../../interfaces";

interface Props {
  className: string;
  playerData: PlayerData;
  gameState: GameState;
}

export default function Playerbar({ className, playerData, gameState }: Props) {
  return (
    <div className={`${className}`}>
      <div className="user-info">
        <img src={`${playerData.image}`}></img>
        <h2>{`${playerData.username}`}</h2>
        <Timer
          active={
            gameState.ongoingGame && playerData.turnStatus === "Your turn"
          }
          playerData={playerData}
          gameState={gameState}
        />
      </div>
    </div>
  );
}
