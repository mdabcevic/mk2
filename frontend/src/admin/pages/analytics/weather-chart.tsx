import { ResponsiveBar } from '@nivo/bar'
import React from 'react'
import { OrdersByWeather} from "./analytics-interface";
import { useTranslation } from "react-i18next";

interface WeatherOrderChartProps {
    data: OrdersByWeather[];
}

const colorsMap: Record<string, string> = {
    dry: '#8C694C',
    rainy: '#60A5FA',
    snowy: '#E3DAC9',
    severe_weather: '#D97706',
};

const WeekdayWeatherChart: React.FC<WeatherOrderChartProps> = ({ data }) => {
    const weekdayData = data.filter(item => item.weekGroup === 'Weekday');

    const transformedData = weekdayData.map(item => ({
        weatherType: item.weatherType,
        averageOrdersPerHour: item.averageOrdersPerHour,
    }));
    const { t } = useTranslation("admin");

    return (
    <ResponsiveBar
        data={transformedData}
        keys={['averageOrdersPerHour']}
        indexBy="weatherType"
        margin={{ top: 50, right: 130, bottom: 50, left: 60 }}
        padding={0.3}
        layout="vertical"
        colors={({ data }) => colorsMap[data.weatherType]}
        axisBottom={{
            legend: t("analytics.weather_type"),
            legendPosition: 'middle',
            legendOffset: 40,
            format: (value) => value === 'dry' ? 'dry (sunny/cloudy)' : value,
        }}
        axisLeft={{
            legend: t("analytics.avg_orders_hour"),
            legendPosition: 'middle',
            legendOffset: -40,
        }}
        labelSkipWidth={12}
        labelSkipHeight={12}
        enableGridX={false}
        enableGridY={true}
        />
    )
}

export default WeekdayWeatherChart;
