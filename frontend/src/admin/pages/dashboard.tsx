import { useState } from "react";


function Dashboard() {

  return (
    <div className="p-4">
      <div className="flex items-center space-x-2">

      <input
        type="text"
        placeholder="Pretraži..."
        className="bg-white rounded p-2 text-black border border-gray-300 focus:outline-none focus:ring focus:ring-gray-400"
      />
    </div>

    </div>
  );
}

export default Dashboard;
