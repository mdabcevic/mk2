import { useEffect, useState } from "react";
import { placeService } from "../services/place.service";
import { IPlaceItem } from "../interfaces/place-item";


export function PlaceMainInfo({ placeid }: { placeid: number }){
    const [place, setPlace] = useState<IPlaceItem | null>(null);

    const getPlaceDetails = async () => {
        let place = await placeService.getPlaceDetailsById(Number(placeid));
        setPlace(place);
      };
    useEffect(() => {
        getPlaceDetails();
      }, []);
    return(
        <div className="flex flex-row justify-around">
            <div className="flex flex-col sm:flex-col">
            <h3 className="text-2xl font-semibold">{place?.businessName}</h3>
            <p className="flex flex-row"><img src="/assets/images/icons/location.svg" alt={place?.address} />{place?.address}</p>
            </div>
            <div className="flex flex-col sm:flex-col sm:justify-between mt-2 mb-3 text-center">
            <span className="font-bold">Working hours:</span>
            <span>{place?.workHours}</span>
            </div>
        </div>
    )
}