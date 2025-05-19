import React, { useState, useEffect } from "react";
import { ResponsiveBar, BarDatum, BarCustomLayerProps, BarLegendProps  } from '@nivo/bar';
import { ChartData } from "../../../interfaces/analytics-interface";

interface PopularProductsChartProps {
    data: ChartData[];
}

const PopularProductsChart: React.FC<PopularProductsChartProps> = ({ data }) => {
    const [isSmallScreen, setIsSmallScreen] = useState(false);

    useEffect(() => {
        const handleResize = () => {
            setIsSmallScreen(window.innerWidth < 600);
        };
    
        handleResize();
        window.addEventListener('resize', handleResize);
    
        return () => window.removeEventListener('resize', handleResize);
    }, []);

    const keys = Array.from(
        new Set(data.flatMap(item => Object.keys(item).filter(key => key !== "day")))
    );
    const reversedKeys = [...keys].reverse();

    const margin = isSmallScreen
    ? { top: 50, right: 20, bottom: 350, left: 80 }
    : { top: 50, right: 150, bottom: 80, left: 80 };

    const legendConfig: BarLegendProps = {
        dataFrom: "keys" as const,
        anchor: isSmallScreen ? "bottom-left" : "top-right",
        direction: "column",
        translateX: isSmallScreen ? 0 : 150,
        translateY: isSmallScreen ? 300 : 0,
        itemWidth: isSmallScreen ? 90 : 150,
        itemHeight: 15,
        itemsSpacing: 5,
        justify: false,
        symbolSize: 12,
        symbolShape: "circle",
    };
    
    return (
    <ResponsiveBar
        data={data}
        keys={reversedKeys}
        indexBy="day"
        margin={margin}
        padding={0.3}
        groupMode="stacked"
        colors={{ scheme: "pastel2" }}
        axisBottom={{
            tickRotation: -20,
            legend: "Dan u tjednu",
            legendPosition: "middle",
            legendOffset: 50,
            tickPadding: 10
        }}
        axisLeft={{
            legend: "Broj prodanih",
            legendPosition: "middle",
            legendOffset: -60,
            tickPadding: 10
        }}
        legends={[legendConfig]}
        animate
        //label={d => `${d.value}\n${d.id}`}
        />
    );
};

export default PopularProductsChart;