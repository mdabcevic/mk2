import * as signalR from "@microsoft/signalr";
import { authService } from "./auth.service";
import { notifyListeners } from "../notification-store";
import { Constants } from "../constants";

let connection: signalR.HubConnection | null = null;

export const startConnection = async (placeId:number) => {
  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(Constants.signalR_hub_url + `?access_token=${authService.token()}`, {
        accessTokenFactory: () => authService.token() ?? "testQuery",
        skipNegotiation:true,
        transport: signalR.HttpTransportType.WebSockets,
        withCredentials: true,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    connection.on("ReceiveNotification", (notf) => {
      notifyListeners(notf);
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