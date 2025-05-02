import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useLocation } from "react-router-dom";
import { Menu } from "lucide-react";
import { AppPaths } from "../../utils/routing/routes";
import { languages } from "../../utils/languages";
import { authService } from "../../utils/auth/auth.service";
import { startConnection } from "../../utils/auth/signalR.service";

const HeaderAdminComponent = () => {
  const { t, i18n } = useTranslation("admin");
  const location = useLocation();
  const [showLanguages,setShowLanguages] = useState(false);
  const [selectedLang, setSelectedLang] = useState(i18n.language);
  
  const changeLanguage = (lang:string) => {
    i18n.changeLanguage(lang);
    setSelectedLang(lang);
    setShowLanguages(false);
  };

  const [open, setOpen] = useState(false);
  const openNavbar = () => {
    setOpen(!open);
  };
  const openConnection = async () => {
    await startConnection(authService.placeId());
  };
  useEffect(()=>{
    openConnection();
    },[]);
  return (
    <header>

      <nav className="flex justify-between items-center bg-brown-500 text-light text-[1.2rem] p-5 w-full fixed top-0 left-1/2 transform -translate-x-1/2 z-100000">
      <h1 className=""><img src="/assets/images/icons/logo.svg" width={"100px"} height={"100px"} className="rounded-[30px]" /></h1>
      <div className="hamburger-cross-icons" onClick={openNavbar}>
        <Menu size={24} />
      </div>
      <ul className={open ? "menu-items flex active" : "menu-items flex"}>
        <Link
          to={AppPaths.admin.dashboard}
          className={`nav-links transition-all duration-500 ease-in-out ${
              location.pathname === AppPaths.admin.dashboard ? "order-3 text-orange-500" : "order-1"
          }`}
        >
          {t("home_link_text")}
        </Link>
        
        <Link
          to={AppPaths.admin.products}
          className={`nav-links transition-all duration-500 ease-in-out ${location.pathname === AppPaths.admin.products ? "text-orange-500" : ""}
                                 ${location.pathname !== AppPaths.admin.dashboard ?  "opacity-100 order-2 cursor-pointer" : "opacity-0 order-1 cursor-default"}`}
        >
          {t("products")}
        </Link>
        <Link
          to={AppPaths.admin.management}
          className={`nav-links transition-all duration-500 ease-in-out ${location.pathname === AppPaths.admin.management ? "text-orange-500" : ""}
                                 ${location.pathname !== AppPaths.admin.dashboard ? "opacity-100 order-3 cursor-pointer" : "opacity-0 order-2 cursor-default"}`}
        >
          {t("management")}
        </Link>
        <Link
          to={AppPaths.admin.tables}
          className={`nav-links transition-all duration-500 ease-in-out ${location.pathname === AppPaths.admin.tables ? "text-orange-500" : ""}
                                 ${location.pathname !== AppPaths.admin.dashboard ? "opacity-100 order-3 cursor-pointer" : "opacity-0 order-2 cursor-default"}`}
        >
          {t("tables")}
        </Link>
        <button
            className={`nav-links transition-all duration-500 ease-in-out order-4`}
            onClick={()=>authService.logout()}
        >
            {t("logout")}
        </button>
        <div className="relative transition-all duration-500 ease-in-out order-5">
        <button className="m-0 p-0"
                onClick={() => setShowLanguages(!showLanguages)}>
          <img src={languages!.find((l) => l.code === selectedLang)!.flag} alt="flag" className="w-7 h-5" /> 
        </button>
        <ul className="absolute mt-2 w-[50px] bg-white">
          {showLanguages && languages.map((lang) => (
            <li
              key={lang.code}
              className="p-2 flex items-center gap-2 cursor-pointer hover:bg-gray-200"
              onClick={() => changeLanguage(lang.code)}
            >
              <img src={lang.flag} alt={lang.label} className="w-7 h-5" />
            </li>
          ))}
        </ul>
        </div>
      </ul>
      
    </nav>
    
    </header>
  );
};

export default HeaderAdminComponent;


