import  { forwardRef } from "react";
import AnalyticsSection from "./analytics-section";

const Analytics = forwardRef((_, ref) => {

    return (
    <div className="p-4">
        <AnalyticsSection ref={ref} />
    </div>
    );
});

export default Analytics;