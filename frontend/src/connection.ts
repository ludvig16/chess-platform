import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

export const connection = new HubConnectionBuilder()
  .withUrl("http://localhost:5038/hubs/game", {
    accessTokenFactory: () => {
      const token = sessionStorage.getItem("accessToken");

      if (!token) {
        throw new Error("No access token available");
      }

      return token;
    },
  })
  .withAutomaticReconnect()
  .configureLogging(LogLevel.Trace)
  .build();
