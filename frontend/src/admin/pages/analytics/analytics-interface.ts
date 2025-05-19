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

export interface ChartData {
    day: string;
    [productName: string]: number | string;
};

export interface TrafficByDayOfWeek {
    dayOfWeek: string;
    count: number;
    averageEarnings: number;
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
    count: number;
    earnings: number;
}
