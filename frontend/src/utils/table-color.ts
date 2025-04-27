import {  TableStatusString } from "./constants";

export enum TableColor {
  empty = "#5ea077",
  occupied = "#fb302d",
  reserved = "#c8c8c8",
  bartenderRequired = "#eebd66",
  billRequested = "#7e96c2"
}
export function getTableColor(status: string){
    switch (status) {
          case TableStatusString.occupied:
            return TableColor.occupied;
          case TableStatusString.reserved:
            return TableColor.reserved;
          default:
            return TableColor.empty;
        }
}

export enum OrderStatusValue {
  created = "created",
  approved = "approved",
  delivered = "delivered",
  payment_requested = "payment_requested",
  paid = "paid",
  closed = "closed",
  cancelled = "cancelled",
}




  const statusColors: Record<OrderStatusValue, string> = {
    created: "#3B82F6",
    approved: "#3B82F6",
    delivered: "#10B981",
    paid: "#10B981",
    payment_requested: "#FCD34D",
    closed: "#D4D4D4",
    cancelled: "#D4D4D4",
  };

  export const orderStatusIndex: Record<OrderStatusValue, number> = {
    created: 0,
    approved: 1,
    delivered: 2,
    payment_requested: 3,
    paid: 4,
    closed: 5,
    cancelled: 6,
  };
  
export function getStatusColor(status: OrderStatusValue): string {
    return statusColors[status] || "#ffffff";
  }

  const notificationColors: Record<number, string> = {
    0: "bg-[#FCD34D] text-black", // StaffNeeded
    1: "bg-[#FCD34D] text-white", // GuestJoinedTable
    2: "bg-[#10B981] text-white", // GuestLeftTable
    3: "bg-[#10B981] text-black", // OrderCreated
    4: "bg-[#3B82F6] text-white", // OrderStatusUpdated
    5: "bg-[#3B82F6] text-white", // OrderContentUpdated
  };
  

  export function getNotificationColor(type: number):string{
    return notificationColors[type] || "#D4D4D4";
  }

  export enum NotificationType{
    StaffNeeded=0,
    GuestJoinedTable=1,
    GuestLeftTable=2,
    OrderCreated=3,
    OrderStatusUpdated=4,
    OrderContentUpdated=5
  }

  const tableIcon: Record<number, string> = {
    0: "../assets/images/icons/staff.png", // StaffNeeded
    3: "../assets/images/icons/newOrder.webp", // OrderCreated
    4: "../assets/images/icons/euro.svg" // payment requested
  };
  export function getTableIcon(type: number):string{
    return tableIcon[type] || "";
  }