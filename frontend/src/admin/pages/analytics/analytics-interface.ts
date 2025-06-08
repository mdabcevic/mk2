import { Table } from "../../../utils/constants";

export interface PopularProducts {
    date?: string | null;
    weekGroup?: string | null;
    productId: number;
    product: string;
    count: number;
    revenue: number;
}

export interface ProductsByDayOfWeek {
    weekGroup: string;
    popularProducts: PopularProducts[];
}

export interface ProductChartData{
    product: string;
    count: number;
    revenue: number;
};

export interface TrafficByDayOfWeek {
    dayOfWeek: string;
    count: number;
    revenue: number;
}

export interface HourlyTraffic {
    dayOfWeek: string;
    hourCounts: HourCount[];
}

export interface HourCount {
    hour: number;
    count: number;
    averageRevenue: number;
}

export interface TableTraffic {
    table: Table;
    count: number;
    averageRevenue: number;
}

export interface PlaceTraffic {
    placeId: number;
    address: string;
    cityName: string;
    lat: number,
    long: number;
    count: number;
    revenue: number;
}

export interface KeyValues {
    revenue: number;
    averageOrder: number;
    totalOrders: number;
    firstEverOrderDate: string,
    firstOrderDate: string,
    lastOrderDate: string;
    mostPopularProduct: string;
}

export interface OrdersByWeather {
    weekGroup: string;
    weatherType: string;
    averageOrdersPerHour: number;
}

export interface AllAnayticsData {
    popularProducts: ProductsByDayOfWeek[];
    dailyTraffic: TrafficByDayOfWeek[];
    hourlyTraffic: HourlyTraffic[];
    placeTraffic: PlaceTraffic[];
    weatherAnalytics: OrdersByWeather[];
    keyValues: KeyValues;
}
