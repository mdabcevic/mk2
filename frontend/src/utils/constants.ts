
export enum Constants{
    tokenKey = "token",
    cartKey = "cart",
    passcode = "passcode",
    salt = "salt",
    url_qr = "http://localhost:5173/table-lookup/{placeId}/{salt}",
    create_tables_container_width = 550, // Matches the width of the 'place_view.png' template image
    create_tables_container_height = 471 , // Matches the height of the 'place_view.png' template image
    template_image = "assets/images/place_view.png",
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
export enum TableColor {
  empty = "#5ea077",
  occupied = "#fb302d",
  reserved = "#c8c8c8",
  bartenderRequired = "#eebd66",
  billRequested = "#7e96c2"
}