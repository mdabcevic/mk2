import { Table } from "../../../utils/constants";

export interface PopularProducts {
    date?: string | null;
    dayOfWeek?: string | null;
    productId: number;
    product: string;
    count: number;
    earnings: number;
}

export interface ProductsByDayOfWeek {
    dayOfWeek: string;
    popularProducts: PopularProducts[];
}

export interface ProductChartData{
    product: string;
    count: number;
    earnings: number;
};

export interface TrafficByDayOfWeek {
    dayOfWeek: string;
    count: number;
    earnings: number;
}

export interface HourlyTraffic {
    dayOfWeek: string;
    hourCounts: HourCount[];
}

export interface HourCount {
    hour: number;
    count: number;
    averageEarnings: number;
}

export interface TableTraffic {
    table: Table;
    count: number;
    averageEarnings: number;
}

export interface PlaceTraffic {
    placeId: number;
    address: string;
    cityName: string;
    lat: number,
    long: number;
    count: number;
    earnings: number;
}

export interface KeyValues {
    earnings: number;
    averageOrder: number;
    totalOrders: number;
    firstEverOrderDate: string,
    firstOrderDate: string,
    lastOrderDate: string;
}
