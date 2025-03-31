import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useParams } from "react-router-dom";
import ImageSlider from "../components/image-slider";
import Menu from "../components/menu";

const BarDetails = () => {
    
  const { id } = useParams();
  const { t } = useTranslation("public");
  const [bar, setBar] = useState<any>(null);
  const [showMenu,setShowMenu] = useState<boolean>(false);
  console.log("id is: ",id)
  useEffect(() => {
    console.log("useeffect")
    getBarDetails();
  }, []);

  const getBarDetails = async () => {
    // let response = await barService.getBarById();
    // setBar(response.data);
    setBar({
      name: "The Chill Bar",
      address: "123 Main St, City",
      hours: "8:00-22:00",
      images: [
        "https://after5.hr/wp-content/uploads/2023/02/Dezman-Bar.jpeg",
        "https://miss7.24sata.hr/media/img/0f/96/b86cd03846ffef241ef0.jpeg",
        "https://croatia-hotspots.com/wp-content/uploads/2020/08/bar2.1.jpg",
      ],
    });
  };

  return (
    <div className="max-w-4xl mx-auto p-4 bg-white">

      <div className="bg-white text-black p-4 rounded-lg">
        <h3 className="text-2xl font-semibold">{bar?.name || "Loading..."}</h3>

        <div className="flex flex-col sm:flex-col sm:justify-between mt-2">
          <p className="text-gray-400">{bar?.address || "Loading..."}</p>
          <p className="text-gray-300">{bar?.hours}</p>
        </div>

        <button className="mt-4 bg-orange-600 text-white py-2 px-4 rounded hover:bg-orange-500 transition" onClick={() => setShowMenu(!showMenu)}>
          {t("menu_text")}
        </button>
      </div>

        {bar?.images?.length > 0 && !showMenu && (
            <ImageSlider images={bar.images} />
        )}

        {showMenu && id && ( <Menu placeId={Number(id)}/> )}

    </div>
  );
};

export default BarDetails;
