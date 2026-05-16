import { HUB_URL } from "../constants/api";
import * as signalR from "@microsoft/signalr";

let connection: signalR.HubConnection | null = null;

export const getConnection = (): signalR.HubConnection => {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(`${HUB_URL}/feedback`)
      .withAutomaticReconnect()
      .build();
  }
  return connection;
};

export const startConnection = async (): Promise<void> => {
  const conn = getConnection();
  if (conn.state === signalR.HubConnectionState.Disconnected) {
    try {
      await conn.start();
      console.log("SignalR connected");
    } catch (err) {
      console.error("SignalR connection failed:", err);
      setTimeout(() => startConnection(), 5000); // Retry after 5s
    }
  }
};

export const stopConnection = async (): Promise<void> => {
  if (connection?.state === signalR.HubConnectionState.Connected) {
    await connection.stop();
    console.log("SignalR disconnected");
  }
};
