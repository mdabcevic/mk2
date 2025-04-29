import { useState } from "react";
import CreateTables from "./create-tables";
import TablesView from "./tables-view";


const TablesPage = () => {
  const [activeTab, setActiveTab] = useState<"view" | "create">("view");

  return (
    <div className="p-4 mt-[105px]">
      <div className="flex gap-4 border-b mb-4 border-white">
        <button
          className={`pb-2 px-4  ${activeTab === "view" ? " font-semibold" : " font-thin"}`}
          onClick={() => setActiveTab("view")}
        >
          View Tables
        </button>
        <button
          className={`pb-2 px-4 ${activeTab === "create" ? " font-semibold" : " font-thin"}`}
          onClick={() => setActiveTab("create")}
        >
          Create Tables
        </button>
      </div>

      <div>
        {activeTab === "view" ? <TablesView /> : <CreateTables />}
      </div>
    </div>
  );
};

export default TablesPage;
