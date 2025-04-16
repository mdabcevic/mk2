import { useState } from "react";

function Home() {
  return (
    <div className="flex flex-col max-w-[800px] mx-auto pt-10 pb-50 px-4">
      {/* Hero Section */}
      <div className="flex flex-col lg:flex-row items-center gap-6">
        <div className="w-full lg:w-1/2 flex flex-col">
          <h1 className="text-2xl font-bold text-white">
            Boost Your Café’s Service with QR Ordering
          </h1>
          <p className="mt-5 text-white">
            Reduce wait times, streamline orders, and keep customers happy—all with a simple scan.
          </p>
          <div className="flex flex-col sm:flex-row gap-3 mt-4">
            <button className="bg-mocha-300 rounded-full w-full sm:w-[150px] h-[50px] text-white">
              Contact Us
            </button>
            <button className="border border-mocha rounded-full w-full sm:w-[150px] h-[50px] text-mocha">
              Discover
            </button>
          </div>
        </div>
        <div className="w-full lg:w-1/2">
          <img src="../assets/images/home_img1.png" alt="coverPhoto" className="w-full h-auto" />
        </div>
      </div>


      <div className="mt-20">
        <h2 className="font-bold text-black text-[2rem] mb-5">How it works</h2>
        <div className="flex flex-wrap justify-center gap-6 text-black">

          <div className="flex flex-col items-center max-w-[220px] text-center">
            <img src="../assets/images/scan_qr.png" className="w-[150px] h-[150px] rounded-full border border-mocha mb-4" alt="Scan QR" />
            <p>
              <span className="font-bold">Scan the QR Code</span> – Customers simply scan the code at their table.
            </p>
          </div>

          <div className="flex flex-col items-center max-w-[220px] text-center">
            <img src="../assets/images/order.jpg" className="w-[150px] h-[150px] rounded-full border border-mocha mb-4" alt="Order" />
            <p>
              <span className="font-bold">Browse & Order</span> – No app needed. See the full menu and order instantly.
            </p>
          </div>

          <div className="flex flex-col items-center max-w-[220px] text-center">
            <img src="../assets/images/relax.jpg" className="w-[150px] h-[150px] rounded-full border border-mocha mb-4" alt="Relax" />
            <p>
              <span className="font-bold">Relax & Enjoy</span> – Orders go directly to staff – no delays, no confusion.
            </p>
          </div>
        </div>
      </div>


      <div className="mt-25 w-full flex flex-col lg:flex-row items-center gap-6">
        <div className="w-full lg:w-1/2">
          <img src="../assets/images/home_img2.png" alt="Analytics" className="w-full h-auto" />
        </div>
        <div className="w-full lg:w-1/2 flex flex-col">
          <h3 className="text-black text-2xl font-bold">
            Built-in Analytics. Smarter Café Management
          </h3>
          <p className="mt-5 text-gray-700">
            From order trends and best-selling items to staff performance insights — our powerful dashboard gives you the data you need to optimize your business and serve smarter.
          </p>
          <button className="bg-mocha-300 rounded-full w-full sm:w-[200px] h-[50px] mt-4 text-white">
            View our pricing
          </button>
        </div>
      </div>


      <div className="pt-30 text-center">
        <p className="font-bold text-mocha-600 text-[2rem] text-black">
          Upgrade your café experience in minutes
        </p>
        <button className="bg-mocha-300 rounded-full mt-4 w-[150px] h-[50px] text-white">
          Get Started
        </button>
      </div>
    </div>
  );
}

export default Home;
