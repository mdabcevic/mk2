import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useParams } from "react-router-dom";
import ImageSlider from "./image-slider";
import { IPlaceItem } from "../../utils/interfaces/place-item";
import { placeService } from "../../utils/services/place.service";
import { randomImages } from "../../utils/random-images";
import { AppPaths } from "../../utils/routing/routes";
import { authService } from "../../utils/auth/auth.service";
import { UserRole } from "../../utils/constants";
import MyOrders from "./my-orders";


const PlaceDetails = () => {
    
  const { id } = useParams();
  const { t } = useTranslation("public");
  const [place, setPlace] = useState<IPlaceItem | null>(null);
  const userRole = authService.userRole();

  useEffect(() => {
    getPlaceDetails();
  }, []);

  const getPlaceDetails = async () => {
    let place = await placeService.getPlaceDetailsById(Number(id));
    setPlace(randomImages(place));
  };

  return (
    <div className="max-w-4xl mx-auto p-4">

      <div className="text-black p-4 mb-5">
        <h3 className="text-2xl font-semibold">{place?.businessName}</h3>

        <div className="flex flex-col sm:flex-col sm:justify-between mt-2 mb-3">
          <p className="">{place?.address}</p>
          {userRole !== UserRole.guest && (<p className="text-gray-700">{place?.workHours}</p>)}
        </div>

        {userRole === UserRole.guest && (<MyOrders />)}
        

        {userRole === UserRole.guest && (
          <div className="fixed bottom-20 left-2 w-full center">
            <button className="bg-mocha-600 px-5 py-1 rounded-[10px] text-white">Call bartender</button>
            <button className="bg-mocha-600 px-5 py-1 rounded-[10px] text-white ml-1 mr-1"><Link to={AppPaths.public.menu.replace(":placeId",id!.toString())}>
          {t("menu_text")}
        </Link></button>
            <button className="bg-mocha-600 px-5 py-1 rounded-[10px] text-white">Request payment</button>
          </div>
        )}

      </div>

      {userRole !== UserRole.guest && (place?.images?.length ?? 0) > 0 && (
          <ImageSlider images={place?.images!} />
      )}

    </div>
  );
};

export default PlaceDetails;
