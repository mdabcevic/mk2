import { useTranslation } from "react-i18next";

function AboutUs() {
  const { t } = useTranslation("public");
  return (
    <div className="flex flex-col max-w-[800px] mx-auto pt-10 pb-50 px-4">
      <div className="flex flex-col lg:flex-row items-center gap-6">
        <div className="w-full lg:w-1/2 flex flex-col">
          <h1 className="text-2xl font-bold text-white">{t('aboutUs.title')}</h1>
          <p className="mt-5 text-white">{t('aboutUs.subtitle')}</p>
          <div className="flex flex-col sm:flex-row gap-3 mt-4">
            <button className="bg-mocha-300 rounded-full w-full sm:w-[150px] h-[50px] text-white cursor-pointer">
              {t('aboutUs.contact_us').toUpperCase()}
            </button>
            <button className="border border-mocha rounded-full w-full sm:w-[150px] h-[50px] text-mocha cursor-pointer">
              {t('aboutUs.discover').toUpperCase() }
            </button>
          </div>
        </div>
        <div className="w-full lg:w-1/2">
          <img src="../assets/images/home_img1.png" alt="coverPhoto" className="w-full h-auto" />
        </div>
      </div>

      <div className="mt-20">
        <h2 className="font-bold text-black text-[2rem] mb-5">{t('aboutUs.how_it_works')}</h2>
        <div className="flex flex-wrap justify-center gap-6 text-black">

          <div className="flex flex-col items-center max-w-[220px] text-center">
            <img src="../assets/images/scan_qr.png" className="w-[150px] h-[150px] rounded-full border border-mocha mb-4" alt="Scan QR" />
            <p><span className="font-bold">{t('aboutUs.step1_title')}</span> – {t('aboutUs.step1_desc')}</p>
          </div>

          <div className="flex flex-col items-center max-w-[220px] text-center">
            <img src="../assets/images/order.jpg" className="w-[150px] h-[150px] rounded-full border border-mocha mb-4" alt="Order" />
            <p><span className="font-bold">{t('aboutUs.step2_title')}</span> – {t('aboutUs.step2_desc')}</p>
          </div>

          <div className="flex flex-col items-center max-w-[220px] text-center">
            <img src="../assets/images/relax.jpg" className="w-[150px] h-[150px] rounded-full border border-mocha mb-4" alt="Relax" />
            <p><span className="font-bold">{t('aboutUs.step3_title')}</span> – {t('aboutUs.step3_desc')}</p>
          </div>
        </div>
      </div>

      <div className="mt-25 w-full flex flex-col lg:flex-row items-center gap-6">
        <div className="w-full lg:w-1/2">
          <img src="../assets/images/home_img2.png" alt="Analytics" className="w-full h-auto" />
        </div>
        <div className="w-full lg:w-1/2 flex flex-col">
          <h3 className="text-black text-2xl font-bold">{t('aboutUs.analytics_title')}</h3>
          <p className="mt-5 text-gray-700">{t('aboutUs.analytics_desc')}</p>
          <button className="bg-mocha-300 rounded-full w-full sm:w-[200px] h-[50px] mt-4 text-white cursor-pointer">
            {t('aboutUs.view_pricing').toUpperCase()}
          </button>
        </div>
      </div>

      <div className="pt-30 text-center">
        <p className="font-bold text-mocha-600 text-[2rem] text-black">
          {t('aboutUs.upgrade_text')}
        </p>
        <button className="bg-mocha-300 rounded-full mt-4 w-[150px] h-[50px] text-white cursor-pointer">
          {t('aboutUs.get_started').toUpperCase()}
        </button>
      </div>
    </div>
  );
}

export default AboutUs;
