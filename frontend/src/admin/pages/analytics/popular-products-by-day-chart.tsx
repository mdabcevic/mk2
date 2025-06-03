import React, { useState, useEffect  } from "react";
import { ResponsiveBar, BarLegendProps } from '@nivo/bar';

interface PopularProductsByDayChart2Props {
    data: any;
}

const PopularProductsByDayChart: React.FC<PopularProductsByDayChart2Props> = ({ data }) => {
    const [isSmallScreen, setIsSmallScreen] = useState(false);
    
    useEffect(() => {
        const handleResize = () => {
            setIsSmallScreen(window.innerWidth < 600);
        };
    
        handleResize();
        window.addEventListener('resize', handleResize);
    
        return () => window.removeEventListener('resize', handleResize);
    }, []);

    const margin = isSmallScreen
    ? { top: 10, right: 20, bottom: 350, left: 80 }
    : { top: 10, right: 90, bottom: 100, left: 80 };

    const legendConfig: BarLegendProps = {
            dataFrom: "keys" as const,
            anchor: isSmallScreen ? "bottom-left" : "top-right",
            direction: "column",
            translateX: isSmallScreen ? 0 : 160,
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
        keys={["count", "earnings"]}
        indexBy="product"
        margin={margin}
        padding={0.3}
        groupMode="grouped"
        colors={['#AE8768', '#60483D']}
        labelTextColor="white"
        axisBottom={{
            tickRotation: -20,
            legend: "Proizvodi",
            legendPosition: "middle",
            legendOffset: 80,
            tickPadding: 10
        }}
        legends={[legendConfig]}
        animate
        />
    );
};

export default PopularProductsByDayChart;