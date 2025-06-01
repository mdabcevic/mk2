import { useEffect, useState } from "react";
import { placeService } from "../../../utils/services/place.service";
import { authService } from "../../../utils/auth/auth.service";
import { IPlaceItem } from "../../../utils/interfaces/place-item";
import { PlaceStatus } from "./models";
import { DashboardCard } from "./dashboard-card";


function Dashboard() { 
  const [placeDetails,setPlaceDetails] = useState<IPlaceItem>();
  const [placeStatus, setPlaceStatus] = useState<PlaceStatus | null>(null);
  const fetchPlaceDetails = async() =>{
    const response = await placeService.getPlaceDetailsById(authService.placeId());
    setPlaceDetails(response);
  }

  const fetchPlaceInfo = async() => {
    const response = await placeService.getPlaceStatus(authService.placeId());
    setPlaceStatus(response);

  }
  useEffect(()=>{
    fetchPlaceDetails();
    fetchPlaceInfo();
  },[])

  return (
    <div className="mt-[100px] w-full min-h-screen">
      <div className="bg-whitem-0 py-10 flex items-center justify-center flex-col w-full">
        <h1>{placeDetails?.businessName}</h1>
        <p>{placeDetails?.address}, {placeDetails?.cityName}</p>
      </div>
      <div className="flex flex-wrap items-center justify-center gap-8 mt-[200px] font-bold text-white text-lg tracking-widest ">
        {placeStatus && (
          <div className="flex-1 min-w-[200px] max-w-[350px]">
            <DashboardCard placeStatus={placeStatus} />
          </div>
        )}      
      </div>
    </div>
  );
}

export default Dashboard;
