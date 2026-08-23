import "./tile.css";

interface Props {
  image: string | undefined;
  number: number;
  highlight: boolean;
  check: boolean;
  currentTurn: "white" | "black";
  color: "w" | "b" | null;
}

export default function Tile({
  number,
  image,
  highlight,
  check,
  currentTurn,
  color,
}: Props) {
  let king;
  currentTurn === "white"
    ? (king = image === `/pieces/wk.png`)
    : (king = image === `/pieces/bk.png`);

  const className = [
    "tile",
    number % 2 === 0 && "white-tile",
    number % 2 !== 0 && "black-tile",
    highlight && "highlight",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div
      className={className}
      style={{ backgroundColor: king && check ? "red" : "" }}
    >
      {image && (
        <div
          className={color === "w" ? "chess-piece-white" : "chess-piece-black"}
          style={{ backgroundImage: `url(${image})` }}
          draggable={false}
        ></div>
      )}
    </div>
  );
}
