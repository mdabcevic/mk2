
export enum Constants{
    tokenKey = "token",
    cartKey = "cart",
    passcode = "passcode",
}

export enum UserRole{
    admin = "admin",
    manager  ="manager",
    guest = "guest"
}


export enum PaymentType{
    cash=0,
    creditcard=1
}


export interface Table {
  label: string;
  positionX: number;
  positionY: number;
  width: number;
  height: number;
  seats: number;
  status: number;
  token?: string;
}

export enum TableStatus {
  empty = 0,
  occupied = 1,
  reserved = 2,
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