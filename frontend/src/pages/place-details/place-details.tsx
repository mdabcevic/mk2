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
import { cartStorage } from "../../utils/storage";


const PlaceDetails = () => {
    
  const { id } = useParams();
  const { t } = useTranslation("public");
  const [place, setPlace] = useState<IPlaceItem | null>(null);
  const userRole = authService.userRole();

  const cart = cartStorage.getCart();

  useEffect(() => {
    getPlaceDetails();
  }, []);

  const getPlaceDetails = async () => {
    let place = await placeService.getPlaceDetailsById(Number(id));
    setPlace(randomImages(place));
  };

  return (
    <div className="max-w-4xl mx-auto p-4">

      <div className="p-4 mb-5">
        <h3 className="text-2xl font-semibold text-white">{place?.businessName}</h3>

        <div className="flex flex-col sm:flex-col sm:justify-between mt-2 mb-3 text-white">
          <p className="">{place?.address}</p>
          {userRole !== UserRole.guest && (<p className="text-gray-700">{place?.workHours}</p>)}
        </div>

        {userRole === UserRole.guest && (<>
          <MyOrders placeId={id!}/>
        </>)}

      </div>

      {userRole !== UserRole.guest && (place?.images?.length ?? 0) > 0 && (
          <ImageSlider images={place?.images!} />
      )}

    </div>
  );
};

export default PlaceDetails;
