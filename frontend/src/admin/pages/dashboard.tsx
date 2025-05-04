import { useEffect, useState } from "react";
import { placeService } from "../../utils/services/place.service";
import { authService } from "../../utils/auth/auth.service";
import { IPlaceItem } from "../../utils/interfaces/place-item";
import { Link } from "react-router-dom";
import { AppPaths } from "../../utils/routing/routes";
import { useTranslation } from "react-i18next";


function Dashboard() { 
  const { t } = useTranslation("admin");
  const [placeDetails,setPlaceDetails] = useState<IPlaceItem>();

  const fetchPlaceDetails = async() =>{
    const response = await placeService.getPlaceDetailsById(authService.placeId());
    setPlaceDetails(response);
  }
  useEffect(()=>{
    fetchPlaceDetails();
  },[])

  return (
    <div className="mt-[100px] w-full">
      <div className="neutral-latte m-0 py-10 flex items-center justify-center flex-col w-full">
        <h1>{placeDetails?.businessName}</h1>
        <p>{placeDetails?.address}, {placeDetails?.cityName}</p>
      </div>
      <div className="flex flex-wrap items-center justify-center gap-8 mt-[200px] font-bold text-white text-lg tracking-widest ">
        <div className="flex-1 min-w-[200px] max-w-[250px] bg-mocha-300  rounded-lg text-center">
          <Link to={AppPaths.admin.management} className="py-6 px-12 inline-block ">emplyoee</Link>
        </div>
        <div className="flex-1 min-w-[200px] max-w-[250px] bg-mocha-300 rounded-lg text-center">
          <Link to={AppPaths.admin.tables} className="py-6 px-12 inline-block">{t("tables")}</Link>
        </div>
        <div className="flex-1 min-w-[200px] max-w-[250px] bg-mocha-300 rounded-lg text-center">
          <Link to={AppPaths.admin.management} className="py-6 px-12 inline-block">{t("management")}</Link>
        </div>
      </div>


    </div>
  );
}

export default Dashboard;
