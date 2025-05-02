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
import { PlaceMainInfo } from "../../utils/components/place-main-info";
import Footer from "../../containers/footer";


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
    <>
      <div className="max-w-4xl mx-auto p-4  pt-[100px]  relative">

      <div className={`${userRole !== UserRole.guest ? "p-4 mb-5" : "p-0 mb-0 max-h-[98vh] relative top-0"}`}>
        
        <PlaceMainInfo placeid={Number(id)}></PlaceMainInfo>
        

      {userRole !== UserRole.guest && (place?.images?.length ?? 0) > 0 && (
        <section className="px-4 flex flex-col">
          <ImageSlider images={place?.images!}/>
          <div className="flex flex-col items-center mt-8 w-full">
            <p className="text-black font-bold">{t("free_tables")}:{place?.freeTablesCount}</p>
            <button className="w-full px-5 py-3 rounded-[16px] max-w-[350px] mt-3 bg-mocha-600 text-white">
              <Link to={AppPaths.public.placeTables.replace(":placeId", id!.toString())} className="w-full block text-center">
                {t("view_tables").toUpperCase()}
              </Link>
            </button>
            <button className="w-full px-5 py-3 rounded-[16px] max-w-[350px] mt-6 bg-mocha-600 text-white">
              <Link to={AppPaths.public.menu.replace(":placeId", id!.toString())} className="w-full block text-center">
                {t("menu").toUpperCase()}
              </Link>
            </button>           
          </div>            

          <article className="mt-20">
            <p className="text-justify">
            Welcome to {place?.businessName}, where timeless flavors meet modern elegance. Nestled in the heart of {place?.cityName}, we offer a carefully crafted menu of seasonal dishes, premium ingredients, and warm hospitality. Whether you're joining us for a romantic dinner, a family celebration, or a casual lunch, our inviting atmosphere and dedicated team ensure a memorable dining experience.
            </p>
            <p className="mt-4">At {place?.businessName}, we believe in honest food made from the freshest, locally sourced ingredients. Our farm-to-table menu changes with the seasons, featuring nourishing dishes inspired by nature. Come enjoy vibrant flavors, thoughtful preparation, and a welcoming space that feels like home.</p>
          </article>

          <iframe className="mt-30 rounded-[16px]" src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d177883.6367772199!2d15.799556012676435!3d45.84265628595503!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x4765d692c902cc39%3A0x3a45249628fbc28a!2sZagreb!5e0!3m2!1sen!2shr!4v1745679320079!5m2!1sen!2shr" width="100%" height="200"  loading="lazy" referrerPolicy={"no-referrer-when-downgrade"}></iframe>
        </section>
      )}
      {userRole === UserRole.guest && (<>
        <MyOrders placeId={id!}/>
      </>)}
    </div>

    </div>
    {userRole !== UserRole.guest && userRole !== UserRole.manager && userRole !== UserRole.admin && <Footer />}
  </>
    
  );
};

export default PlaceDetails;
