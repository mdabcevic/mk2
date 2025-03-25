import { useState } from "react";
import BarCard from "../bar-card";
import { IBarItem } from "../../interfaces/bar-item";

function Home() {

  const bars = [
    {
      name: "Bar 1",
      imgUrl: "https://www.foodandwine.com/thmb/8rtGtUmtC0KiJCDxAUXP_cfwgM8=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/GTM-Best-US-Bars-Katana-Kitten-FT-BLOG0423-fa9f2ba9925c47abb4afb0abd25d915a.jpg",
      address: "123 Main St, City, Country",
    },
    {
      name: "Bar 2",
      imgUrl: "https://media.cnn.com/api/v1/images/stellar/prod/221004154233-01-world-best-bars-2022.jpg?c=original",
      address: "456 Side St, City, Country",
    },
    {
      name: "Bar 3",
      imgUrl: "https://www.telegraph.co.uk/content/dam/Travel/Destinations/Europe/Spain/Barcelona/nightlife/bar_drink_14_dry_martini3.jpg?imwidth=680",
      address: "789 High St, City, Country",
    },
    {
      name: "Bar 4",
      imgUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRPwoVSAa6odttTg4kEfFFs0VeWBQekSFwbGw&s",
      address: "1010 Low St, City, Country",
    },
    {
      name: "Bar 5",
      imgUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS1KO2rnFIWT2FBjsElCCQKzYdvakJKmUOKU5SDM6tjVF34pgLBk70AKdx0fgMGaQAXOP4&usqp=CAU",
      address: "2020 Park Ave, City, Country",
    },
    {
      name: "Bar 6",
      imgUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTf4ERnSb8biMalV5cHLRxx93JcbXpG0G2IQw&s",
      address: "3030 River Rd, City, Country",
    },
  ] as IBarItem[];

  return (
    <div className="p-4">
      <h2 className="text-2xl font-bold text-white mb-6">Bars Near You</h2>
      
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        {bars.map((bar, index) => (
          <BarCard key={index} bar={bar} />
        ))}
      </div>
    </div>
  );
}

export default Home;
