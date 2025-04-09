import  { useState } from "react";
import MenuTable from "./menu-table";
import ProductsSection from "./products-section";
import { authService } from "../../../utils/auth/auth.service";
import { useTranslation } from "react-i18next";

const menu = "menu";
const products = "products";
const placeId = authService.placeId();
const Products = () => {
  const { t } = useTranslation("admin");
  const [activeTab, setActiveTab] = useState<string>(menu);
  
  return (
    <div className="p-4 bg-gray-50 rounded-lg shadow-md">
      <div className="flex gap-4 mb-4 border-b border-gray-200">
        <button
          onClick={() => setActiveTab("menu")}
          className={`py-2 px-4 font-semibold ${
            activeTab === "menu"
              ? "border-b-2 border-orange-500 text-orange-600"
              : "text-gray-500"
          }`}
        >
          {t("menu")}
        </button>
        <button
          onClick={() => setActiveTab("products")}
          className={`py-2 px-4 font-semibold ${
            activeTab === "products"
              ? "border-b-2 border-orange-500 text-orange-600"
              : "text-gray-500"
          }`}
        >
          {t("products")}
        </button>
      </div>

      <div className="mt-4">
        {activeTab === menu && <MenuTable placeId={placeId} />}
        {activeTab === products && <ProductsSection />}
      </div>
    </div>
    
  );
};

export default Products;