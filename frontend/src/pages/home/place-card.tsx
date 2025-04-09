import { useNavigate } from "react-router-dom";
import { IPlaceItem } from "../../utils/interfaces/place-item";
import { AppPaths } from "../../utils/routing/routes";

function PlaceCard( { place, index }: { place: IPlaceItem, index: number }) {
  const navigate = useNavigate();

    return (
      <div 
        className={`flex md:block items-center max-h-[60px] md:max-h-none w-full bg-white p-2 md:p-4 rounded shadow-[0px_4px_10px_rgb(0,0,0,0)] `}
        onClick={() => navigate(AppPaths.public.placeDetails.replace(":id",place.id.toString())) }>
        
        <img 
          src={place.imageUrl} 
          alt={place.businessName} 
          className="w-10 h-10 md:w-[90%] md:h-45 md:mx-auto rounded object-cover"
        />

        <div className="flex flex-col justify-center ml-3 md:ml-0 flex-1 overflow-hidden">
          <h3 className="text-sm md:text-xl text-black font-semibold truncate">{place.businessName}</h3>
          <p className="text-xs text-black truncate">{place.address}</p>
        </div>
        <div className="w-full text-right">
        <button className="bg-orange-300 py-1 px-3 cursor-pointer rounded hover:bg-orange-100 ml-auto md:ml-0 md:mt-4">
          <img src="https://cdn2.iconfinder.com/data/icons/game-center-mixed-icons/512/arrow5.png" width="30px" height="30px" />
        </button>
        </div>
        
      </div>


    );
  };
  
  export default PlaceCard;
  
  