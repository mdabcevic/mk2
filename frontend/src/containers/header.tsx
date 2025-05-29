import {  useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useLocation, } from "react-router-dom";
import { languages } from "../utils/languages";
import { AppPaths } from "../utils/routing/routes";
import { authService } from "../utils/auth/auth.service";
import { UserRole } from "../utils/constants";


const HeaderComponent = () => {
  const { t, i18n } = useTranslation("public");
  const location = useLocation();
  const [open, setOpen] = useState(false);
  const [showLanguages,setShowLanguages] = useState(false);
  const [selectedLang, setSelectedLang] = useState(i18n.language);

  const changeLanguage = (lang:string) => {
    i18n.changeLanguage(lang);
    setSelectedLang(lang);
    setShowLanguages(false);
  };
  


  return (
    <header className="">

      <nav className="flex justify-between items-center bg-brown-500 text-light text-[1.2rem] p-5 w-full fixed top-0 left-1/2 transform -translate-x-1/2 z-100000">
      <h1 className="cursor-pointer" onClick={()=> window.location.href = AppPaths.public.home}><img src="/assets/images/icons/logo.svg" width={"100px"} height={"100px"} className="rounded-[30px]" /></h1>

      <ul className={open ? "menu-items active" : "menu-items"}>
      <Link
        to={AppPaths.public.login}
        onClick={()=> setOpen(!open)}
        className={`nav-links text-light ${
          location.pathname === AppPaths.public.login ? "text-orange-500" : "text-light"
        } ${authService.tokenValid() ? "hidden" : "block"}`}
      >
        {t("header.login")}
      </Link>

      {authService.userRole() === UserRole.guest && (
        <Link
          to={AppPaths.public.myOrders.replace(":placeId",authService.placeId().toString())}
          onClick={()=> setOpen(!open)}
          className={`nav-links text-light ${(authService.placeId() > 0) ? "block" : "hidden"}`}
        >
          {t("header.my_orders")}
        </Link>
      )}
      
        
        <div className="relative">
        <button className="m-0 p-0"
                onClick={() => setShowLanguages(!showLanguages)}>
          <img src={languages!.find((l) => l.code === selectedLang)!.flag} alt="flag" className="w-7 h-5" /> 
        </button>
        <ul className="absolute mt-2 w-[50px] bg-white">
          {showLanguages && languages.map((lang) => (
            <li
              key={lang.code}
              className="p-2 flex items-center gap-2 cursor-pointer"
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

export default HeaderComponent;
