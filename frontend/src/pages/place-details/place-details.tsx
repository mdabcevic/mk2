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
          {userRole !== UserRole.guest && (<p className="text-white">{place?.workHours}</p>)}
        </div>

        {userRole === UserRole.guest && (<>
          <MyOrders placeId={id!}/>
        </>)}
        {userRole !== UserRole.guest && (
          <section>
            
            <article className="neutral-latte rounded-sm p-5 mb-4">
              <p className="text-justify">
              Welcome to {place?.businessName}, where timeless flavors meet modern elegance. Nestled in the heart of {place?.cityName}, we offer a carefully crafted menu of seasonal dishes, premium ingredients, and warm hospitality. Whether you're joining us for a romantic dinner, a family celebration, or a casual lunch, our inviting atmosphere and dedicated team ensure a memorable dining experience.
              </p>
              <p className="mt-4">At {place?.businessName}, we believe in honest food made from the freshest, locally sourced ingredients. Our farm-to-table menu changes with the seasons, featuring nourishing dishes inspired by nature. Come enjoy vibrant flavors, thoughtful preparation, and a welcoming space that feels like home.</p>
            </article>
            <button className={` px-5 py-1 rounded-[40px] border-mocha ml-1 mr-1 mt-3 bg-mocha-600 text-white`}>
              <Link to={AppPaths.public.menu.replace(":placeId",id!.toString()!)}>
              {t("menu").toUpperCase()}
              </Link>
            </button>
          </section>
          
        )}
      </div>

      {userRole !== UserRole.guest && (place?.images?.length ?? 0) > 0 && (
        <div className="px-4">
          <ImageSlider images={place?.images!}/>
          </div>
      )}

    </div>
  );
};

export default PlaceDetails;
