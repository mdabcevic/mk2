import  { forwardRef } from "react";
import AnalyticsSection from "./analytics-section";

const Analytics = forwardRef(() => {

    return (
    <div className="p-4 max-w-[1500px] m-auto">
        <AnalyticsSection />
    </div>
    );
});

export default Analytics;