import React from "react";
import { ResponsiveLine } from '@nivo/line';
import { TrafficByDayOfWeek } from "./analytics-interface";
import { useTranslation } from "react-i18next";

interface DailyTraffic {
    data: TrafficByDayOfWeek[];
    count: boolean;
}

const LineChart: React.FC<DailyTraffic> = ({ data,  count}) => {
    const { t } = useTranslation("admin");

    const formattedData = [
        {
            id: count ? t("analytics.count") : t("analytics.revenue"),
            data: data.map(d => ({
                x: d.dayOfWeek,
                y: count ? d.count : d.revenue
            }))
        }
    ];

    return (
    <ResponsiveLine
        data={formattedData}
        margin={{ top: 50, right: 110, bottom: 70, left: 80 }}
        yScale={{ type: 'linear', min: 'auto', max: 'auto', stacked: true, reverse: false }}
        axisBottom={{ legend: t("analytics.dayOfWeek"), legendOffset: 50 }}
        axisLeft={{ legend: count ? t("analytics.count") : t("analytics.revenue"), legendOffset: -60 }}
        colors={{ scheme: 'brown_blueGreen' }}
        lineWidth={3}
        pointColor={{ theme: 'background' }}
        pointBorderWidth={2}
        pointBorderColor={{ from: 'seriesColor' }}
        pointLabelYOffset={-12}
        enableArea={false}
        areaOpacity={0.15}
        enableTouchCrosshair={true}
        useMesh={true}
    />
    )
}

export default LineChart
