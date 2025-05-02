import { useEffect, useState } from "react";
import { Constants, Table, UserRole } from "../../utils/constants";
import { tableService } from "../../utils/services/tables.service";
import { useTranslation } from "react-i18next";
import { getTableColor } from "../../utils/table-color";
import { AppPaths } from "../../utils/routing/routes";
import { Link, useParams } from "react-router-dom";
import { authService } from "../../utils/auth/auth.service";
import { placeService } from "../../utils/services/place.service";
import { IPlaceItem } from "../../utils/interfaces/place-item";
import { PlaceMainInfo } from "../../utils/components/place-main-info";
import Footer from "../../containers/footer";


const initial_div_width = Constants.create_tables_container_width;
const initial_div_height = Constants.create_tables_container_height;
const userRole = authService.userRole();
const PlaceTablesViewPublic = () => {
  const { placeId } = useParams();
  const [tables, setTables] = useState<Table[]>([]);
  const [scale, setScale] = useState<number>();
  const [place, setPlace] = useState<IPlaceItem | null>(null);
  
  const fetchTables = async () => {
    const response = authService.userRole() === UserRole.admin || 
                     authService.userRole() === UserRole.manager ? 
                     await tableService.getPlaceTablesByCurrentUser() : await tableService.getPlaceTablesByPlaceId(Number(placeId));
    setTables(response);
  };
  
  const calculateScale = () => {
    const screenWidth = window.innerWidth; 
    const screenHeight = window.innerHeight;
    const scaleX = screenWidth / initial_div_width;
    const scaleY = screenHeight / initial_div_height;
    if(screenWidth < 500){
      const finalScale = Math.max(Math.min(scaleX, scaleY), 0.2);
      setScale(finalScale);
    }
    else{
      const minScale = Math.min(scaleX, scaleY);
      const finalScale = Math.min(minScale, 1);
      setScale(Math.max(finalScale, 0.2));
    }
    
  };
  const getPlaceDetails = async () => {
      let place = await placeService.getPlaceDetailsById(Number(placeId));
    };
  useEffect(() => {
    getPlaceDetails();
    fetchTables();
    calculateScale();
    window.addEventListener("resize", calculateScale);
    return () => window.removeEventListener("resize", calculateScale);
  }, []);

  return (
    <div
      className="flex flex-col w-full h-full min-h-screen items-center"
    >
      <div className="max-w-[1500px] w-full mt-30">
        <Link to={AppPaths.public.placeDetails.replace(":id",placeId!)} className="ml-4 mt-4 mb-4 w-full">Go back</Link>
      </div>
      <div className="max-w-[1500px] w-full mt-8 px-8">
        <PlaceMainInfo placeid={Number(placeId)}></PlaceMainInfo>
      </div>
      
      <div className="flex flex-row w-full justify-center mt-8">
        <div className="flex fle-row"><div className="w-[30px] h-[30px] bg-white mr-4"></div><span>Empty</span></div>
        <div className="flex fle-row"><div className="w-[30px] h-[30px] bg-[#A3A3A3] ml-4 mr-4"></div><span>Occupied</span></div>
      </div>
      {scale && (
        <div
          style={{
            width: `${initial_div_width * scale!}px`,
            height: `${initial_div_height * scale!}px`,
            backgroundImage: "url(/assets/images/place_view.png)",
            backgroundSize: "contain",
            backgroundRepeat: "no-repeat",
            position: "relative",
            marginTop:"50px"
          }}
        >
          {tables.map((table, index) => (
            <div
              key={index}
              className="absolute flex items-center justify-center text-white font-bold border border-black box-border"
              style={{
                left: table.x * scale!,
                top: table.y * scale!,
                width: table.width * scale!,
                height: table.height * scale!,
                backgroundColor: getTableColor(table.status),
                borderRadius: `${(Math.min(table.width, table.height) * scale!) / 2}px`,
              }}
              >
            </div>
          ))}
        </div>
      )}
      {userRole !== UserRole.guest && userRole !== UserRole.manager && userRole !== UserRole.admin && <Footer />}
    </div>
  );
};

export default PlaceTablesViewPublic;
