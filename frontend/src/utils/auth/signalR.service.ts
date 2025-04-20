import * as signalR from "@microsoft/signalr";
import { authService } from "./auth.service";
import { showToast, ToastType } from "../toast";
import { notifyListeners } from "../notification-store";

let connection: signalR.HubConnection | null = null;

export const startConnection = async (placeId:number) => {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl("https://localhost:7281/hubs/place", {
        accessTokenFactory: () => authService.token() ?? "",
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    connection.on("ReceiveNotification", (message: string) => {
      showToast(message, ToastType.info);

      notifyListeners({
        id: "Bartender" + Date.now() ,
        message,
        type: ToastType.info,
      });
    });
    connection.onreconnecting(error => {
      console.warn("Reconnecting to SignalR hub...", error);
    });

    connection.onreconnected(connectionId => {
      console.log("Reconnected to SignalR hub with ID:", connectionId);
    });

    connection.onclose(error => {
      console.error("SignalR connection closed.", error);
    });

    try {
      await connection.start();
      await connection.invoke("JoinPlaceGroup", placeId);
      console.log("Connected to SignalR hub");
    } catch (err) {
      console.error("Error connecting to SignalR hub:", err);
    }
  }
};

export const getConnection = () => connection;