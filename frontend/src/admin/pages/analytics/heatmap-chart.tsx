import { ResponsiveHeatMap } from '@nivo/heatmap'
import { HourlyTraffic } from "./analytics-interface";
import { interpolateRgbBasis  } from 'd3-interpolate';

interface HourlyTrafficProp {
    data: HourlyTraffic[];
}

const HeatMapChart: React.FC<HourlyTrafficProp> = ({ data }) => {
    const formattedData = data.map(day => ({
        id: day.dayOfWeek,
        data: day.hourCounts.map(hourCount => ({
            x: hourCount.hour.toString() + " h",
            y: hourCount.count ?? 0
        })),
    }));

    const counts = data.flatMap(day => day.hourCounts.map(hc => hc.count));
    const minCount = Math.min(...counts);
    const maxCount = Math.max(...counts);

    const colorInterpolator = interpolateRgbBasis([
        '#F6F2EC', 
        '#D4C0B0', 
        '#624935'
    ]);

    return(
    <ResponsiveHeatMap
        data={formattedData}
        margin={{ top: 60, right: 90, bottom: 60, left: 100 }}
        valueFormat=">-.2s"
        axisLeft={{ legend: 'count', legendOffset: -90 }}
        colors={(node) => {
            if (typeof node.value !== 'number') {
                return '#000';
            }
    
            const normalized = (node.value - minCount) / (maxCount - minCount || 1);
            const clamped = Math.max(0, Math.min(1, normalized));
            
            const color = colorInterpolator(clamped);            
            return color;
        }}
        emptyColor="#555555"
        legends={[
            {
                anchor: 'bottom',
                translateX: 0,
                translateY: 30,
                length: 400,
                thickness: 8,
                direction: 'row',
                tickPosition: 'after',
                tickSize: 3,
                tickSpacing: 4,
                tickOverlap: false,
                tickFormat: '>-.2s',
                title: 'Value →',
                titleAlign: 'start',
                titleOffset: 4
            }
        ]}
    />
    )
}

export default HeatMapChart