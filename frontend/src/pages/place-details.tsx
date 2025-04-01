import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useParams } from "react-router-dom";
import ImageSlider from "../components/image-slider";
import Menu from "../components/menu";
import { IPlaceItem } from "../interfaces/place-item";
import { PlaceService } from "../services/place.service";

const PlaceDetails = () => {
    
  const { id } = useParams();
  const { t } = useTranslation("public");
  const [place, setPlace] = useState<IPlaceItem | null>(null);

  useEffect(() => {
    getPlaceDetails();
  }, []);

  const getPlaceDetails = async () => {
    let place = await PlaceService.getPlaceDetailsById(Number(id));
    setPlace(place);
  };

  return (
    <div className="max-w-4xl mx-auto p-4 bg-white">

      <div className="bg-white text-black p-4 rounded-lg">
        <h3 className="text-2xl font-semibold">{place?.name || "Loading..."}</h3>

        <div className="flex flex-col sm:flex-col sm:justify-between mt-2">
          <p className="text-gray-400">{place?.address || "Loading..."}</p>
          <p className="text-gray-300">{place?.workingHours}</p>
        </div>

        <Link to={`/place/${id}/menu`}  className="mt-4 bg-orange-600 text-white py-2 px-4 rounded hover:bg-orange-500 transition">
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
