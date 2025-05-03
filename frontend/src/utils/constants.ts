

export enum Constants{
    backend_port = 7281,
    tokenKey = "token",
    cartKey = "cart",
    passcode = "passcode",
    salt = "salt",
    place_id = "place_id",
    api_base_url = import.meta.env.VITE_API_BASE_URL || `https://localhost:${backend_port}`,
    url_qr = import.meta.env.VITE_FRONTEND_QR_URL || "http://localhost:5173/table-lookup/{placeId}/{salt}",
    create_tables_container_width = 550, // Matches the width of the 'place_view.png' template image
    create_tables_container_height = 471 , // Matches the height of the 'place_view.png' template image
    template_image = "assets/images/place_view.png",
    signalR_hub_url = import.meta.env.VITE_SIGNALR_HUB_URL || `https://localhost:${backend_port}/hubs/place`
}

export enum UserRole{
    admin = "admin",
    manager  ="manager",
    guest = "guest",
    staff = "regular"
}


export enum PaymentType{
    cash=0,
    creditcard=1
}

export interface TablePublic{
  label:string,
  seats:number,
  width:number,
  height:number,
  x:number,
  y:number,
  status:string,
}

export interface Table {
  label: string;
  x: number;
  y: number;
  width: number;
  height: number;
  seats: number;
  status: string;
  token?: string;
  requestType?:number;
}

export enum TableStatus {
  empty = 0,
  occupied = 1,
  reserved = 2,
}

export enum TableStatusString {
  empty = "empty",
  occupied = "occupied",
  reserved = "reserved",
}

export enum BtnVisibility {
  visible = "visible",
  invisible = "invisible",
}
