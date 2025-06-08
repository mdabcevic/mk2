import { Constants } from "../../../utils/constants";
import { TableTraffic } from "./analytics-interface";
import { interpolateRgbBasis } from 'd3-interpolate';

const colorInterpolator = interpolateRgbBasis([
    '#F6F2EC', 
    '#D4C0B0',
    '#624935' 
]);

const getInterpolatedColor = (value: number, min: number, max: number): string => {
    if (max === min) return colorInterpolator(0.5); 
    const normalized = (value - min) / (max - min);
    return colorInterpolator(normalized);
};

interface TableAnalyticsProp {
    data: TableTraffic[];
    count: boolean;
}

const initial_div_width = Constants.create_tables_container_width;
const initial_div_height = Constants.create_tables_container_height;

const TableAnalytics: React.FC<TableAnalyticsProp> = ({ data, count }) => {
  const values = count ? data.map(d => d.count) : data.map(d => d.averageRevenue);
  const minValue = Math.min(...values);
  const maxValue = Math.max(...values);

    return (
      <div className="relative">
        
        <div className="flex flex-col items-center space-x-4 absolute right-0 top-0">
        </div>
          <div
            style={{
              width: `${initial_div_width}px`,
              height: `${initial_div_height}px`,
              backgroundImage: `url(/${Constants.template_image})`,
              backgroundSize: "contain",
              backgroundRepeat: "no-repeat",
              position: "relative",
              zIndex:"1"
            }}
          >
            {data?.length > 0 && data.map((tableData, index) => {
                const color = getInterpolatedColor(count ? tableData.count : tableData.averageRevenue, minValue, maxValue);
                return (
                    <div
                        key={index}
                        className="absolute cursor-pointer flex items-center justify-center font-bold border border-black box-border text-xs text-black"
                        style={{
                            left: tableData.table.x,
                            top: tableData.table.y,
                            width: tableData.table.width,
                            height: tableData.table.height,
                            backgroundColor: color,
                            borderRadius: `${Math.min(tableData.table.width, tableData.table.height) / 2}px`,
                            zIndex: 2,
                            whiteSpace: "normal",
                            ...(count ? {} : { wordBreak: "break-word" }),
                            textAlign: "center",
                            overflow: "visible",
                        }}
                        title={`Table: ${tableData.table.label}, Traffic: ${count ? tableData.count : tableData.averageRevenue}`}
                    >
                        {count ? tableData.count : "~ " + tableData.averageRevenue + " €"}
                    </div>
                );
            })}
          </div>
        
      </div>
      
    );
  };

export default TableAnalytics;