import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useParams } from "react-router-dom";
import ImageSlider from "./image-slider";
import { ImageType, IPlaceItem } from "../../../utils/interfaces/place-item";
import { placeService } from "../../../utils/services/place.service";
import { AppPaths } from "../../../utils/routing/routes";
import { authService } from "../../../utils/auth/auth.service";
import { UserRole } from "../../../utils/constants";
import { PlaceMainInfo } from "../../../utils/components/place-main-info";
import Footer from "../../../containers/footer";
import { Button } from "../../../utils/components/button";


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
    setPlace(place);
  };

  return (
    <>
      <div className="max-w-4xl mx-auto p-4  pt-[100px]  relative">

      <div className={`${userRole !== UserRole.guest ? "p-4 mb-5" : "p-0 mb-0 max-h-[98vh] relative top-0"}`}>
        
        <PlaceMainInfo placeid={Number(id)}></PlaceMainInfo>
        

      {(place?.images?.length ?? 0) > 0 && (
        <section className="px-4 flex flex-col">
          <ImageSlider images={place?.images!.find(i => i.imageType == ImageType.gallery)?.urls!}/>
          <div className="flex flex-col items-center mt-8 w-full">
            <p className="text-black font-bold">{t("free_tables")}: {place?.freeTablesCount}</p>
            <Button 
              textValue={t("view_tables").toUpperCase()} 
              type={"brown-dark"}
              size={"large"}
              navigateTo={AppPaths.public.placeTables.replace(":placeId", id!.toString())}
              className={"w-full max-w-[350px] rounded-[16px] "}
              />
              <Button 
                textValue={t("menu").toUpperCase()} 
                type={"brown-dark"}
                size={"large"}
                navigateTo={AppPaths.public.menu.replace(":placeId", id!.toString())}
                className={"w-full max-w-[350px] rounded-[16px] "}
              />        
          </div>            

          <article className="mt-20">
            <p className="text-justify">
              {place?.description}
            </p>
          </article>

          <iframe className="mt-20 rounded-[16px] mb-10" src={place?.googleMapIframeLink} width="100%" height="200"  loading="lazy" referrerPolicy={"no-referrer-when-downgrade"}></iframe>
        </section>
      )}
    </div>

    </div>
    {userRole !== UserRole.guest && userRole !== UserRole.manager && userRole !== UserRole.admin && <Footer />}
  </>
    
  );
};

export default PlaceDetails;
