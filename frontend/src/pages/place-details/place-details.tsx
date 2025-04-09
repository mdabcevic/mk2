import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useParams } from "react-router-dom";
import ImageSlider from "./image-slider";
import { IPlaceItem } from "../../utils/interfaces/place-item";
import { placeService } from "../../utils/services/place.service";
import { randomImages } from "../../utils/random-images";
import { AppPaths } from "../../utils/routing/routes";


const PlaceDetails = () => {
    
  const { id } = useParams();
  const { t } = useTranslation("public");
  const [place, setPlace] = useState<IPlaceItem | null>(null);

  useEffect(() => {
    getPlaceDetails();
  }, []);

  const getPlaceDetails = async () => {
    let place = await placeService.getPlaceDetailsById(Number(id));
    setPlace(randomImages(place));
  };

  return (
    <div className="max-w-4xl mx-auto p-4">

      <div className="bg-white text-black p-4 mb-5">
        <h3 className="text-2xl font-semibold">{place?.businessName || "Loading..."}</h3>

        <div className="flex flex-col sm:flex-col sm:justify-between mt-2">
          <p className="text-gray-700">{place?.address || "Loading..."}</p>
          <p className="text-gray-700">{place?.workHours}</p>
        </div>

        <Link to={AppPaths.public.menu.replace(":placeId",id!.toString())}  className="mt-4 bg-orange-600 text-white  px-4 rounded hover:bg-orange-500 transition">
          {t("menu_text")}
        </Link>
      </div>

        {(place?.images?.length ?? 0) > 0 && (
            <ImageSlider images={place?.images!} />
        )}

    </div>
  );
};

export default PlaceDetails;
