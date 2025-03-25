import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useLocation } from "react-router-dom";
import { Menu } from "lucide-react";

const HeaderComponent = () => {
  const { t, i18n } = useTranslation("public");
  const location = useLocation();
  console.log(location.pathname);
  const [open, setOpen] = useState(false);
  const openNavbar = () => {
    setOpen(!open);
  };

  return (
    <header>

      <nav className="flex justify-between items-center text-[1.2rem] bg-gray-900 p-5 w-full fixed top-0 left-1/2 transform -translate-x-1/2
">
      <h1 className="text-2xl">Logo</h1>
      <div className="Hamburger-Cross-Icons" onClick={openNavbar}>
        <Menu size={24} />
      </div>
      <ul className={open ? "menu-items active" : "menu-items"}>
      <Link
          to="/home"
          className={`nav-links ${
            location.pathname === "/home" ? "text-orange-500" : ""
          }`}
        >
          {t("home_link_text")}
        </Link>
        <Link
          to="/app"
          className={`nav-links  ${
            location.pathname === "/app" ? "text-orange-500" : ""
          }`}
        >
          {t("bars_link_text")}
        </Link>
        <Link
          to="/aboutus"
          className={`nav-links  ${
            location.pathname === "/aboutus" ? "text-orange-500" : ""
          }`}
        >
          {t("aboutus_link_text")}
        </Link>
        <Link
          to="/NotFound"
          className={`nav-links  ${
            location.pathname === "/NotFound" ? "text-orange-500" : ""
          }`}
        >
          Not Found
        </Link>
        <select
          onChange={(e) => i18n.changeLanguage(e.target.value)}
          value={i18n.language}
          className="bg-gray-800 text-white border border-gray-700 p-2 rounded"
        >
          <option value="en">🇬🇧</option>
          <option value="hr">🇭🇷</option>
        </select>
      </ul>
      
    </nav>
    
    </header>
  );
};

export default HeaderComponent;
