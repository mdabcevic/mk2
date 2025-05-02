import { useState } from "react";
import CreateTables from "./create-tables";
import TablesView from "./tables-view";
import { useTranslation } from "react-i18next";


const TablesPage = () => {
  const [activeTab, setActiveTab] = useState<"view" | "create">("view");
  const { t } = useTranslation("admin");
  return (
    <div className="p-4 mt-[105px]">
      <h2 className="font-semibold text-center text-2xl">{t("table_info").toUpperCase()}</h2>
      <div className="flex gap-4 border-b mb-4 border-white">
        <button
          className={`pb-2 px-4 flex-1 max-w-[150px]  ${activeTab === "view" ? " font-semibold border-b-3" : " font-thin"}`}
          onClick={() => setActiveTab("view")}
        >
          View
        </button>
        <button
          className={`pb-2 px-4 flex-1 max-w-[150px] ${activeTab === "create" ? " font-semibold border-b-3" : " font-thin"}`}
          onClick={() => setActiveTab("create")}
        >
          Edit
        </button>
      </div>

      <div>
        {activeTab === "view" ? <TablesView /> : <CreateTables />}
      </div>
    </div>
  );
};

export default TablesPage;
