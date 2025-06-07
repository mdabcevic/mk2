import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { AppPaths } from "../../utils/routing/routes";
import { languages } from "../../utils/languages";
import { authService } from "../../utils/auth/auth.service";
import { startConnection } from "../../utils/auth/signalR.service";
import { UserRole } from "../../utils/constants";

const HeaderAdminComponent = () => {
  const { t, i18n } = useTranslation("admin");
  const location = useLocation();
  const navigate = useNavigate();
  const [showLanguages,setShowLanguages] = useState(false);
  const [selectedLang, setSelectedLang] = useState(i18n.language);
  
  const changeLanguage = (lang:string) => {
    i18n.changeLanguage(lang);
    setSelectedLang(lang);
    setShowLanguages(false);
  };

  const openConnection = async () => {
    await startConnection(authService.placeId());
  };
  useEffect(()=>{
    openConnection();
    const isNotOnNotifications = !location.pathname.includes(AppPaths.admin.notifications);
;
    if (isNotOnNotifications && authService.userRole() == UserRole.staff) {
      navigate(AppPaths.admin.notifications, { replace: true });
    }
    },[location.pathname]);
  
  
  return (
    <header className="transition-all duration-1000 ease-in-out">

      <nav className="transition-all duration-1000 ease-in-out flex justify-between items-center bg-brown-500 text-light text-[1.2rem] p-5 w-full fixed top-0 left-1/2 transform -translate-x-1/2 z-100000">
      <h1 onClick={()=> window.location.href = AppPaths.admin.dashboard}><img src="/assets/images/icons/logo.svg" width={"100px"} height={"100px"} className="rounded-[30px]" /></h1>

      <ul className={"menu-items flex"}>
      {authService.userRole() != UserRole.staff && (
        <>
          <Link
            to={AppPaths.admin.management}
            className={`nav-links transition-all duration-500 ease-in-out cursor-pointer ${location.pathname === AppPaths.admin.management ? "text-orange-500" : ""}`}
          >
            {t("management")}
          </Link>
          <Link
            to={AppPaths.admin.tables}
            className={`nav-links transition-all duration-500 cursor-pointer ease-in-out ${location.pathname === AppPaths.admin.tables ? "text-orange-500" : ""}`}
          >
            {t("tables")}
          </Link>
        </>
      )}
        
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


