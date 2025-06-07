import { Link } from "react-router-dom";
import { AppPaths } from "../utils/routing/routes";
import { useTranslation } from "react-i18next";


function Footer(){
    const { t } = useTranslation("public");

    return (
        <footer className="flex justify-center flex-col pt-5 pb-15 text-white bg-brown-500 w-full">
            <div className="flex justify-center flex-row">
                <img src="/assets/images/facebook.svg" alt="facebook icon"/>
                <img src="/assets/images/ig.svg" alt="instagram icon" className=" ml-5 mr-5"/>
                <img src="/assets/images/ln.svg" alt="ln icon"/>
            </div>
            <div className="flex flex-wrap sm:flex-row sm:flex-nowrap justify-center gap-4 mt-4 sm-gap:6">
                <span>© bartender.com</span>
                <Link to ="#">{t("terms")}</Link>
                <Link to={AppPaths.public.home}>{t("home_link_text")}</Link>
                <Link to={AppPaths.public.subsciption}>{t("pricing")}</Link>
                <Link to={AppPaths.public.contactUs}>{t("home.contact_us")}</Link>
            </div>
            
            
        </footer>
    )
}

export default Footer;