import { useNavigate } from "react-router-dom";
import { IPlaceItem } from "../../utils/interfaces/place-item";
import { AppPaths } from "../../utils/routing/routes";

function PlaceCard( { place, index }: { place: IPlaceItem, index: number }) {
  const navigate = useNavigate();

    return (
      <div 
        className="relative flex md:block items-center w-full h-[200px] bg-white rounded-[12px] py-1 shadow-[0px_4px_10px_rgb(0,0,0,0.1)] overflow-hidden cursor-pointer"
        onClick={() => navigate(AppPaths.public.placeDetails.replace(":id", place.id.toString()))}
      >
        <img 
          src={place.imageUrl} 
          alt={place.businessName} 
          className="absolute inset-0 w-full h-full object-cover rounded"
        />
    
        <div className="absolute bottom-0 w-full bg-black/60 text-white flex flex-col items-center justify-center z-10">
          <h3 className="text-sm md:text-xl font-semibold truncate w-full text-center">
            {place.businessName},
            <span className="text-xs w-full text-center ml-2">{place.address}</span>
          </h3>
          
        </div>        
      </div>
    );
  
  };
  
  export default PlaceCard;
  
  