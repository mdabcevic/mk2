import { useState } from "react";
import BarCard from "../components/bar-card";
import { IBarItem } from "../interfaces/bar-item";
import BarDetails from "./bar-details";

function Home() {

  const bars = [
    {
      id: 1,
      name: "Bar 1",
      imgUrl: "https://www.foodandwine.com/thmb/8rtGtUmtC0KiJCDxAUXP_cfwgM8=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/GTM-Best-US-Bars-Katana-Kitten-FT-BLOG0423-fa9f2ba9925c47abb4afb0abd25d915a.jpg",
      address: "123 Main St, City, Country",
    },
    {
      id: 2,
      name: "Bar 2",
      imgUrl: "https://media.cnn.com/api/v1/images/stellar/prod/221004154233-01-world-best-bars-2022.jpg?c=original",
      address: "456 Side St, City, Country",
    },
    {
      id: 3,
      name: "Bar 3",
      imgUrl: "https://www.telegraph.co.uk/content/dam/Travel/Destinations/Europe/Spain/Barcelona/nightlife/bar_drink_14_dry_martini3.jpg?imwidth=680",
      address: "789 High St, City, Country",
    },
    {
      id: 4, // ✅ Added missing ID
      name: "Bar 4",
      imgUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRPwoVSAa6odttTg4kEfFFs0VeWBQekSFwbGw&s",
      address: "1010 Low St, City, Country",
    },
    {
      id: 5, // ✅ Added missing ID
      name: "Bar 5",
      imgUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS1KO2rnFIWT2FBjsElCCQKzYdvakJKmUOKU5SDM6tjVF34pgLBk70AKdx0fgMGaQAXOP4&usqp=CAU",
      address: "2020 Park Ave, City, Country",
    },
    {
      id: 6, // ✅ Added missing ID
      name: "Bar 6",
      imgUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTf4ERnSb8biMalV5cHLRxx93JcbXpG0G2IQw&s",
      address: "3030 River Rd, City, Country",
    },
    {
      id: 7, 
      name: "Bar 1",
      imgUrl: "https://www.foodandwine.com/thmb/8rtGtUmtC0KiJCDxAUXP_cfwgM8=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/GTM-Best-US-Bars-Katana-Kitten-FT-BLOG0423-fa9f2ba9925c47abb4afb0abd25d915a.jpg",
      address: "123 Main St, City, Country",
    },
    {
      id: 8, 
      name: "Bar 2",
      imgUrl: "https://media.cnn.com/api/v1/images/stellar/prod/221004154233-01-world-best-bars-2022.jpg?c=original",
      address: "456 Side St, City, Country",
    },
    {
      id: 9, 
      name: "Bar 3",
      imgUrl: "https://www.telegraph.co.uk/content/dam/Travel/Destinations/Europe/Spain/Barcelona/nightlife/bar_drink_14_dry_martini3.jpg?imwidth=680",
      address: "789 High St, City, Country",
    },
    {
      id: 10, 
      name: "Bar 4",
      imgUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRPwoVSAa6odttTg4kEfFFs0VeWBQekSFwbGw&s",
      address: "1010 Low St, City, Country",
    },
    {
      id: 11, 
      name: "Bar 5",
      imgUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS1KO2rnFIWT2FBjsElCCQKzYdvakJKmUOKU5SDM6tjVF34pgLBk70AKdx0fgMGaQAXOP4&usqp=CAU",
      address: "2020 Park Ave, City, Country",
    },
    {
      id: 12, 
      name: "Bar 6",
      imgUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTf4ERnSb8biMalV5cHLRxx93JcbXpG0G2IQw&s",
      address: "3030 River Rd, City, Country",
    },
  ] as IBarItem[];
  
  const [selectedOption, setSelectedOption] = useState("Zagreb");
  const [isOpen, setIsOpen] = useState(false);
  const options = ["Zagreb", "Rijeka", "Karlovac", "Osijek"];
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
            {options.map((option) => (
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

      {/* Search Input */}
      <input
        type="text"
        placeholder="Pretraži..."
        className="bg-white rounded p-2 text-black border border-gray-300 focus:outline-none focus:ring focus:ring-gray-400"
      />
    </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2 mt-3">
        {bars.map((bar, index) => (
          <BarCard key={index} bar={bar} index={index} 
          />
        ))}
      </div>
    </div>
  );
}

export default Home;
