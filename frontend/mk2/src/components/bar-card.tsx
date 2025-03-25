import { IBarItem } from "../interfaces/bar-item";


function BarCard( { bar }: { bar: IBarItem }) {
    return (
      <div className="bg-gray-800 p-4 rounded-lg shadow-md">
        <img src={bar.imgUrl} alt={bar.name} className="w-full h-32 object-cover rounded-lg mb-4" />
        
        <h3 className="text-xl text-white font-semibold">{bar.name}</h3>
        
        <p className="text-gray-400">{bar.address}</p>
        
        <div className="flex justify-end mt-4">
          <button className="bg-orange-800 text-white py-2 px-4 rounded hover:bg-orange-600">
            More
          </button>
        </div>
      </div>
    );
  };
  
  export default BarCard;
  
  