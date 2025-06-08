import { useEffect, useState } from "react";
import { ProductsByDayOfWeek, TrafficByDayOfWeek, HourlyTraffic, 
    TableTraffic, PlaceTraffic, ProductChartData, KeyValues} from "./analytics-interface";
import { getDataForChart, formatDate, formatDateToMonthYear, getMonthAndYearOptions } from "./analytics-helpers";
import { analyticsService } from "../../../utils/services/analytics.service";
import { placeService } from "../../../utils/services/place.service";
import { authService } from "../../../utils/auth/auth.service";
import { useTranslation } from "react-i18next";
import PopularProductsByDayChart from "./popular-products-by-day-chart";
import LineChart from "./line-chart";
import HeatMapChart from "./heatmap-chart";
import TableAnalytics from "./table-analytics";
import PlaceMap from "./place-analytics";
import Dropdown, { DropdownItem } from "../../../utils/components/dropdown";
import "react-datepicker/dist/react-datepicker.css";
import CustomSelect from "./custom-select";
import StatCard from "./stat-card";

const placeIdFromAuth  = authService.placeId();

const AnalyticsSection = () => {
    const [filterDayType, setFilterDayType] = useState<DropdownItem["id"]>("all");
    const [placeId, setPlaceId] = useState<number | undefined>(undefined);
    const [address, setAddress] = useState<string>("");
    const [month, setMonth] = useState<number | undefined>(undefined);
    const [year, setYear] = useState<number | undefined>(undefined);
    const [popularProducts, setProducts] = useState<ProductsByDayOfWeek[]>([]);
    const [productChartData, setProductChartData] = useState<ProductChartData[]>([]);
    const [dailyTraffic, setDailyTraffic] = useState<TrafficByDayOfWeek[]>([]);
    const [hourlyTraffic, setHourlyTraffic] = useState<HourlyTraffic[]>([]);
    const [tableTraffic, setTableTraffic] = useState<TableTraffic[]>([]);
    const [placeTraffic, setPlaceTraffic] = useState<PlaceTraffic[]>([]);
    const [loading, setLoading] = useState(true);
    const [keyValues, setKeyValues] = useState<KeyValues>({
        revenue: 0,
        averageOrder: 0,
        totalOrders: 0,
        firstEverOrderDate: new Date().toISOString(),
        firstOrderDate: new Date().toISOString(),
        lastOrderDate: new Date().toISOString(),
        mostPopularProduct: ""
    })
    const [monthOptions, setAvailableMonths] = useState<{ value: number; label: string }[]>([]);
    const [yearsOption, setAvailableYears] = useState<{ value: number; label: string }[]>([]);

    const { t } = useTranslation("admin");
    const [activeTab, setActiveTab] = useState('All');

    const tabs = ['All','Weekday', 'Weekend'];

    const placeOptions: DropdownItem[] = [
        { id: -1, value: "All" }, 
        ...placeTraffic.map(place => ({
            id: place.placeId,
            value: place.address + ", " + place.cityName
        }))
    ];

    const fetchAll = async (placeId?: number, month?: number, year?: number) => {
        const data = await analyticsService.getAll(placeId, month, year);
        setProducts(data.popularProducts);
        setDailyTraffic(data.dailyTraffic);
        setHourlyTraffic(data.hourlyTraffic);
        setPlaceTraffic(data.placeTraffic);
        setKeyValues(data.keyValues);
    };

    const fetchTableTraffic = async (placeId: number, month?: number, year?: number) => {
        const traffic = await analyticsService.getTableTraffic(placeId, month, year);
        setTableTraffic(traffic);
    };

    const fetchPlaceAddress = async (placeId: number) => {
        const place = await placeService.getPlaceDetailsById(placeId);
        setAddress(place.address + ", " + place.cityName);
    };

    const dropdownChange = (item: DropdownItem) => {
        setPlaceId(item.id === -1 ? undefined : Number(item.id));
        console.log(placeId)
    };

    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            await Promise.all([
                fetchAll(placeId, month, year),
                fetchTableTraffic(placeId ?? placeIdFromAuth, month, year),
                fetchPlaceAddress(placeId !== undefined ? placeId : placeIdFromAuth),
            ]);
            setLoading(false);
        };  
        fetchData();
    }, [month, year, placeId]);

    useEffect(() => {
        if (keyValues.firstOrderDate && keyValues.lastOrderDate) {
            const { monthOptions, yearOptions } = getMonthAndYearOptions(
                keyValues.firstOrderDate,
                keyValues.lastOrderDate,
                year
            );
            setAvailableMonths(monthOptions);
            setAvailableYears(yearOptions);
        }
        
    }, [keyValues, month, year]);

    useEffect(() => {
        setFilterDayType(activeTab.toLowerCase());
    }, [activeTab]);

    useEffect(() => {
        const chartData = getDataForChart(popularProducts, String(filterDayType).toLowerCase());
        setProductChartData(chartData);
    }, [popularProducts, filterDayType]);

    return (
        <div className="w-full min-h-screen pt-30 px-4 space-y-4">   
            <p className="text-center w-full text-[36px] font-bold">{t("analytics.title")}</p>
            {/* Sidebar */}
            <div className="pt-10">
                <div id="sidebar" className="w-[450px] sticky top-23 self-start bg-white shadow-md rounded-lg p-4 z-50 mb-8">
                    <div className ="pb-3 text-center text-gray-500">
                        <p>{t("analytics.time_frame")}</p>
                        <p>
                        {month !== undefined && year === undefined
                            ? `${formatDateToMonthYear(keyValues.firstOrderDate)}, ${formatDateToMonthYear(keyValues.lastOrderDate)}`
                            : `${formatDate(keyValues.firstOrderDate)} - ${formatDate(keyValues.lastOrderDate)}`}
                        </p>
                    </div>

                    <div className="flex space-x-2 items-center">
                        <Dropdown
                            items={placeOptions}
                            onChange={dropdownChange}
                            type="light"
                            className="bg-white rounded-[16px] w-[200px]"
                        />
                            <CustomSelect
                                options={monthOptions}
                                value={month}
                                onChange={setMonth}
                                placeholder={t("month")}
                            />
                            <CustomSelect
                                options={yearsOption}
                                value={year}
                                onChange={setYear}
                                placeholder={t("year")}
                            />
                    </div>
                </div>
            {loading ? (
                <div className="w-full h-[600px] flex justify-center items-center">
                    <div role="status">
                        <svg aria-hidden="true" className="w-10 h-10 text-gray-200 animate-spin dark:text-gray-600 fill-[#7E5E44]" viewBox="0 0 100 101" fill="none" xmlns="http://www.w3.org/2000/svg">
                            <path d="M100 50.5908C100 78.2051 77.6142 100.591 50 100.591C22.3858 100.591 0 78.2051 0 50.5908C0 22.9766 22.3858 0.59082 50 0.59082C77.6142 0.59082 100 22.9766 100 50.5908ZM9.08144 50.5908C9.08144 73.1895 27.4013 91.5094 50 91.5094C72.5987 91.5094 90.9186 73.1895 90.9186 50.5908C90.9186 27.9921 72.5987 9.67226 50 9.67226C27.4013 9.67226 9.08144 27.9921 9.08144 50.5908Z" fill="currentColor"/>
                            <path d="M93.9676 39.0409C96.393 38.4038 97.8624 35.9116 97.0079 33.5539C95.2932 28.8227 92.871 24.3692 89.8167 20.348C85.8452 15.1192 80.8826 10.7238 75.2124 7.41289C69.5422 4.10194 63.2754 1.94025 56.7698 1.05124C51.7666 0.367541 46.6976 0.446843 41.7345 1.27873C39.2613 1.69328 37.813 4.19778 38.4501 6.62326C39.0873 9.04874 41.5694 10.4717 44.0505 10.1071C47.8511 9.54855 51.7191 9.52689 55.5402 10.0491C60.8642 10.7766 65.9928 12.5457 70.6331 15.2552C75.2735 17.9648 79.3347 21.5619 82.5849 25.841C84.9175 28.9121 86.7997 32.2913 88.1811 35.8758C89.083 38.2158 91.5421 39.6781 93.9676 39.0409Z" fill="currentFill"/>
                        </svg>
                        <span className="sr-only">Loading...</span>
                    </div>
                </div>
            ) : (
            <>
            {/* Main Content */}
            {/* Daily Traffic & Revenue side by side */}
            <div className="space-y-16">
            <div className="flex space-x-6">
                    <div className="flex-1">
                        <h2 className="text-xl font-semibold mb-2 text-center">{t("analytics.daily_traffic")}</h2>
                        <div className="w-full h-[350px]">
                            <LineChart data={dailyTraffic} count={true} />
                        </div>
                    </div>
            
                    <div className="flex-1">
                        <h2 className="text-xl font-semibold mb-2 text-center">{t("analytics.daily_earnings")}</h2>
                        <div className="w-full h-[350px]">
                            <LineChart data={dailyTraffic} count={false} />
                        </div>
                    </div>
            </div>

            <div className="flex-1 flex space-x-5 items-center">
                    <StatCard title={t("analytics.total_revenue")} value={keyValues.revenue} suffix="€" />
                    <StatCard title={t("analytics.avg_order")} value={keyValues.averageOrder} suffix="€" />
                    <StatCard title={t("analytics.total_orders")} value={keyValues.totalOrders} />
                    <StatCard title={t("analytics.first_order")} value={keyValues.firstEverOrderDate} />
                    <StatCard title={t("analytics.popular_product")} value={keyValues.mostPopularProduct} />
            </div>

            <div className="flex flex-col space-y-10">
                <div className="w-full">
                    <h2 className="text-xl font-semibold mb-4 text-center">{t("analytics.popular_products")}</h2>
            
                    {/* Tabs */}
                    <div className="flex space-x-4 mb-4 pt-5">
                    {tabs.map((tab) => (
                        <button
                        key={tab}
                        onClick={() => setActiveTab(tab)}
                        className={`pb-2 text-md px-5 font-medium border-b-2 transition-colors ${
                            activeTab === tab
                            ? "text-[#7E5E44] border-[#7E5E44] font-extrabold"
                            : "text-[#737373] border-transparent"
                        }`}
                        >
                        {tab}
                        </button>
                    ))}
                    </div>
            
                    {/* Chart */}
                    <div className="w-full h-[400px]">
                    <PopularProductsByDayChart data={productChartData} />
                    </div>
                </div>
            
                

                <div className="flex space-x-6">
                    <div className="flex-1">
                    <h2 className="text-xl font-semibold mb-2 text-center">{t("analytics.hourly_traffic")}</h2>
                    <div className="w-full h-[400px]">
                        <HeatMapChart data={hourlyTraffic}/>
                    </div>
                    </div>
                </div>

                <div className="flex flex-col md:flex-row space-y-6 md:space-y-0 md:space-x-10">
                    <div className="flex-1 flex flex-col">
                        <h2 className="text-xl font-semibold mb-2 text-center mb-12">{t("analytics.place_traffic")}</h2>
                        <div className="flex-1 flex justify-center h-[400px] w-full z-1">
                        <PlaceMap data={placeTraffic}/>
                        </div>
                    </div>

                    <div className="flex-1">
                        <h2 className="text-xl font-semibold mb-2 text-center">{t("analytics.table_traffic")}</h2>
                        <p className="text-center mb-5">({address})</p>
                        <div className="w-full h-[600px]">
                        <TableAnalytics data={tableTraffic} count={true}/>
                        </div>
                    </div>
                </div>      
            </div>
        </div>
        </>
        
        )}
        
        </div>
    </div>
    );
};

export default AnalyticsSection;