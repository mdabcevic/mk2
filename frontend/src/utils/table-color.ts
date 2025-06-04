import {  TableStatusString } from "./constants";

export enum TableColor {
  empty = "#ffffff",
  occupied = "#A3A3A3",
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
    created: "#FCD34D",
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
    3: "bg-[#FCD34D] text-black", // OrderCreated
    4: "bg-[#3B82F6] text-white", // OrderStatusUpdated
    5: "bg-[#3B82F6] text-white", // OrderContentUpdated
    6: "bg-[#FCD34D] text-black", // PaymentRequested
  };

export function getBgColorByNotificationStatus(type: number){
  return notificationColors[type].split(" ")[0].split("[")[1].split("]")[0];

}

  export function getNotificationColor(type: number):string{
    return notificationColors[type] || "#D4D4D4";
  }

  export enum NotificationType{
    StaffNeeded=0,
    GuestJoinedTable=1,
    GuestLeftTable=2,
    OrderCreated=3,
    OrderStatusUpdated=4,
    OrderContentUpdated=5,
    PaymentRequested=6,
  }

  const tableIcon: Record<number, string> = {
    [NotificationType.StaffNeeded]: "../assets/images/icons/notificationBell.svg", // StaffNeeded
    [NotificationType.OrderCreated]: "../assets/images/icons/cup.svg", // OrderCreated
    [NotificationType.PaymentRequested]: "../assets/images/icons/dollar.svg" // payment requested
  };
  export function getTableIcon(type: number):string | undefined{
    return tableIcon[type] || undefined;
  }