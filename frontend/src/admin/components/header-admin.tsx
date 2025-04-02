import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useLocation } from "react-router-dom";
import { Menu } from "lucide-react";

const HeaderAdminComponent = () => {
  const { t, i18n } = useTranslation("admin");
  const location = useLocation();
  console.log(location.pathname);
  const [open, setOpen] = useState(false);
  const openNavbar = () => {
    setOpen(!open);
  };

  return (
    <header>

      <nav className="flex justify-between items-center text-[1.2rem] bg-black p-5 w-full fixed top-0 left-1/2 transform -translate-x-1/2
">
      <h1 className="">Mk2</h1>
      <div className="hamburger-cross-icons" onClick={openNavbar}>
        <Menu size={24} />
      </div>
      <ul className={open ? "menu-items active" : "menu-items"}>
      <Link
          to="/admin"
          className={`nav-links ${
            location.pathname === "/admin" ? "text-orange-500" : ""
          }`}
        >
          {t("home_link_text")}
        </Link>
        <Link
          to="menu"
          className={`nav-links  ${
            location.pathname === "/admin/menu" ? "text-orange-500" : ""
          }`}
        >
          {t("menu_link_text")}
        </Link>
        <Link
          to="products"
          className={`nav-links  ${
            location.pathname === "/admin/products" ? "text-orange-500" : ""
          }`}
        >
          {t("products_link_text")}
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
          <option value="en"><img src="../assets/images/uk_flag.png" width="50px" height="30px" /></option>
          <option value="hr"><img src="../assets/images/cro_flag.png" width="50px" height="30px" /></option>
        </select>
      </ul>
      
    </nav>
    
    </header>
  );
};

export default HeaderAdminComponent;
