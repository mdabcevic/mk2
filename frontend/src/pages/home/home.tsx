import { useEffect, useState } from "react";
import PlaceCard from "./place-card";
import { IPlaceItem } from "../../utils/interfaces/place-item";
import { placeService } from "../../utils/services/place.service";
import { randomImages } from "../../utils/random-images";

const availableOptions = ["Zagreb", "Rijeka", "Karlovac", "Osijek"];
function Home() {
  
  const [selectedOption, setSelectedOption] = useState(availableOptions[0]);
  const [isOpen, setIsOpen] = useState(false);
  const [loading,setLoading] = useState<boolean>(false);
  const [places,setPlaces] = useState<IPlaceItem[]>([]);

  const fetchPlaces = async () =>{
    let _places = await placeService.getPlaces() as IPlaceItem[];
    setPlaces(randomImages(_places));
  }

  useEffect(()=>{
    fetchPlaces();
  },[]);
  return (
    <div className="p-4">
      <div className="flex items-center space-x-2">

      <div className="">
        <button
          onClick={() => setIsOpen(!isOpen)}
          className="bg-gray-800 text-white px-3 py-1 rounded flex items-center space-x-1"
        >
          <span>{selectedOption}</span>
          <svg
            className="w-4 h-4 transform transition-transform"
            style={{ transform: isOpen ? "rotate(180deg)" : "rotate(0deg)" }}
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M19 9l-7 7-7-7"
            />
          </svg>
        </button>

        {isOpen && (
          <div className="absolute left-0 mt-1 w-32 bg-white text-black border rounded shadow-lg z-0">
            {availableOptions.map((option) => (
              <div
                key={option}
                onClick={() => {
                  setSelectedOption(option);
                  setIsOpen(false);
                }}
                className="px-3 py-2 hover:bg-gray-200 cursor-pointer"
              >
                {option}
              </div>
            ))}
          </div>
        )}
      </div>

      <input
        type="text"
        placeholder="Pretraži..."
        className="bg-white rounded p-2 text-black border border-gray-300 focus:outline-none focus:ring focus:ring-gray-400"
      />
    </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2 mt-3">
        {places.length > 0 && (
          places.map((place, index) => (
            <PlaceCard key={index} place={place} index={index} 
            />
        ) ))}
      </div>
    </div>
  );
}

export default Home;
