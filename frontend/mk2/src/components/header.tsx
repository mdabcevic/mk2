import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Link, useLocation } from "react-router-dom";

const HeaderComponent = () => {
  const { t, i18n } = useTranslation("public");
  const location = useLocation();

  useEffect(() => {
    i18n.reloadResources();
  }, []);
  const changeLanguage = (e:any) => {
    i18n.changeLanguage(e.target.value);
  };
  return (
    <header className="bg-gray-900 text-white p-4 shadow-md">
      <h1 className="text-2xl font-bold">My Website</h1>
      
      <nav className="mt-2 flex gap-4">
        <Link 
          to="/home" 
          className={`hover:text-orange-400 ${location.pathname === "/home" ? "text-orange-500" : ""}`}
        >
          {t("home_link_text")}
        </Link>
        <Link 
          to="/app" 
          className={`hover:text-orange-400 ${location.pathname === "/app" ? "text-orange-500" : ""}`}
        >
          {t("bars_link_text")}
        </Link>
        <Link 
          to="/aboutus" 
          className={`hover:text-orange-400 ${location.pathname === "/aboutus" ? "text-orange-500" : ""}`}
        >
          {t("aboutus_link_text")}
        </Link>
        <Link 
          to="/NotFound" 
          className={`hover:text-orange-400 ${location.pathname === "/NotFound" ? "text-orange-500" : ""}`}
        >
          Not Found
        </Link>
      </nav>

      <select 
        onChange={(e) => i18n.changeLanguage(e.target.value)} 
        value={i18n.language}
        className="mt-2 bg-gray-800 text-white border border-gray-700 p-2 rounded"
      >
        <option value="en">English</option>
        <option value="hr">Hrvatski</option>
      </select>
    </header>
  );
};

export default HeaderComponent;
