import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useLocation } from "react-router-dom";
import { Menu } from "lucide-react";
import { languages } from "../utils/languages";
import { AppPaths } from "../utils/routing/routes";



const HeaderComponent = () => {
  const { t, i18n } = useTranslation("public");
  const location = useLocation();

  const [open, setOpen] = useState(false);
  const openNavbar = () => {
    setOpen(!open);
  };
  const [showLanguages,setShowLanguages] = useState(false);
  const [selectedLang, setSelectedLang] = useState(i18n.language);

  const changeLanguage = (lang:string) => {
    i18n.changeLanguage(lang);
    setSelectedLang(lang);
    setShowLanguages(false);
  };

  return (
    <header>

      <nav className="flex justify-between items-center text-[1.2rem] bg-white text-black p-5 w-full fixed top-0 left-1/2 transform -translate-x-1/2
">
      <h1 className="">Mk2</h1>
      <div className="hamburger-cross-icons" onClick={openNavbar}>
        <Menu size={24} />
      </div>
      <ul className={open ? "menu-items active" : "menu-items"}>
      <Link
          to={AppPaths.public.home}
          className={`nav-links ${
            location.pathname === AppPaths.public.home ? "text-orange-500" : ""
          }`}
        >
          {t("home_link_text")}
        </Link>
        <button
            className={`nav-links`}
           // onClick={() => {showPasscode } }
        >
          {t("passcode")}
        </button>
        <div className="relative">
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

export default HeaderComponent;
