import { forwardRef, useEffect, useState, Ref } from "react";
import { ProductsByDayOfWeek, TrafficByDayOfWeek, HourlyTraffic, TableTraffic, PlaceTraffic, ChartData } from "./analytics-interface";
import { analyticsService } from "../../../utils/services/analytics.service";
import { authService } from "../../../utils/auth/auth.service";
import { useTranslation } from "react-i18next";
import { ResponsiveBar } from '@nivo/bar'
import PopularProductsChart from "./popularProductsChart";
import Dropdown, { DropdownItem } from "../../../utils/components/dropdown";

const placeIdFromAuth  = authService.placeId();

const transformDataForChart = (rawData: ProductsByDayOfWeek[]): ChartData[] => {
    return rawData.map((day) => {
        const row: ChartData = { day: day.dayOfWeek };
        day.popularProducts.forEach((product) => {
        row[product.product] = product.count;
        });
        return row;
    });
};

const AnalyticsSection = () => {
    const [filterType, setFilterType] = useState<"day" | "month" | "year">("day");
    const [placeId, setPlaceId] = useState(null);
    const [month, setMonth] = useState(null);
    const [year, setYear] = useState(null);
    const [popularProducts, setProducts] = useState<ProductsByDayOfWeek[]>([]);
    const [chartData, setChartData] = useState<ChartData[]>([]);
    const [dailyTraffic, setDailyTraffic] = useState<TrafficByDayOfWeek[]>([]);
    const [hourlyTraffic, setHourlyTraffic] = useState<HourlyTraffic[]>([]);
    const [tableTraffic, setTableTraffic] = useState<TableTraffic[]>([]);
    const [placeTraffic, setPlaceTraffic] = useState<PlaceTraffic[]>([]);
    const { t } = useTranslation("admin");

    const fetchProducts = async (placeId?: number, month?: number, year?: number) => {
        const products = await analyticsService.getPopularProducts(placeId, month, year);
        const transformed = transformDataForChart(products);
        setChartData(transformed);
    };

    const fetchDailyTraffic = async (placeId?: number, month?: number, year?: number) => {
        const traffic = await analyticsService.getWeeklyTraffic(placeId, month, year);
        setDailyTraffic(traffic);
    };
    const fetchHourlyTraffic = async (placeId?: number, month?: number, year?: number) => {
        const traffic = await analyticsService.getHourlyTraffic(placeId, month, year);
        setHourlyTraffic(traffic);
    };
    const fetchTableTraffic = async (placeId: number, month?: number, year?: number) => {
        const traffic = await analyticsService.getTableTraffic(placeId, month, year);
        setTableTraffic(traffic);
    };
    const fetchPlaceTraffic = async (month?: number, year?: number) => {
        const traffic = await analyticsService.getAllPlacesTraffic(month, year);
        setPlaceTraffic(traffic);
    };

    /*const fetchTotalEarnings = async (placeId?: number, month?: number, year?: number) => {
        const earnings = await analyticsService.getTotalEarnings(placeId, month, year);
        setTotalEarnings(earnings);
    };*/

    useEffect(() => {
        fetchProducts();
        fetchDailyTraffic();
        fetchHourlyTraffic();
        //fetchTableTraffic();
        fetchPlaceTraffic();
        //fetchTotalEarnings();
    }, []);
    
    return (
    <div className="w-full lg:w-1/2 lg:h-[500px] h-[700px]">
        <h2>{t("popular_products")}</h2>
        <div className="h-full">
        <PopularProductsChart data={chartData} />
        </div>
    </div>
    );
};

export default AnalyticsSection;