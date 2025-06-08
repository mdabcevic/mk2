import { useTranslation } from "react-i18next";
import { PlaceStatus } from "./models";

interface DashboardCardProps {
  placeStatus: PlaceStatus;
}

export function DashboardCard({ placeStatus }: DashboardCardProps) {
  const { t } = useTranslation("admin");
  return (
    <div className="bg-white rounded-[32px] text-black w-full text-center border border-mocha-500 relative bottom-20">
        <div className="bg-[#E3DAC9] border-b-4 border-mocha-500 w-full flex justify-center flex-col items-center pt-4 pb-4 rounded-t-[32px]">
            <img src="/assets/images/icons/key_info.svg" width={"40px"} /> 
            <span>{t("dashboard.key_info")}</span>
        </div>
        <div className="p-4 flex flex-col gap-6">
            <p className="flex justify-between w-full"><span>{t("dashboard.active_orders")}:</span><span>{placeStatus.activeOrders}</span></p>
            <p className="flex justify-between w-full"><span>{t("dashboard.closed_orders")}:</span><span>{placeStatus.closedOrders}</span></p>
            <p className="flex justify-between w-full"><span>{t("dashboard.free_tables")}:</span><span>{placeStatus.freeTablesCount}</span></p>
        </div>
      
    </div>
  );
}