import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";
import { AppPaths } from "../utils/routing/routes";

function AboutUs() {
  const { t } = useTranslation("public");

  return (
    <div className="flex flex-col max-w-[1200px] mx-auto pt-10 pb-20 px-4 sm:px-6 lg:px-8">
      
      <section className="flex flex-col-reverse lg:flex-row items-center gap-10 mt-5">
        <div className="w-full lg:w-1/2 flex flex-col text-center lg:text-left">
          <h1 className="text-1xl lg:text-4xl font-bold text-black">{t('aboutUs.title')}</h1>
          <p className="mt-5 text-lg">{t('aboutUs.subtitle')}</p>
        </div>
        <div className="md:max-w-[500px] sm:max-w-[300px] w-full lg:w-1/2">
          <img src="/assets/images/homepage/home1.png" alt="Home intro" className="w-full h-auto" />
        </div>
      </section>


      <section className="mt-20">
        <h2 className="font-bold text-3xl text-black mb-10">{t('aboutUs.how_it_works')}</h2>
        <div className="flex flex-col sm:flex-row justify-center gap-10 text-black">
          

          <div className="flex flex-col items-center max-w-[220px] mx-auto text-center">
            <img src="/assets/images/scan_qr.png" className="w-[180px] h-[180px] rounded-full border border-mocha mb-4 object-cover" alt="Scan QR" />
            <p>
              <span className="font-bold">{t('aboutUs.step1_title')}</span> – {t('aboutUs.step1_desc')}
            </p>
          </div>

          <div className="flex flex-col items-center max-w-[220px] mx-auto text-center">
            <img src="/assets/images/order.jpg" className="w-[180px] h-[180px] rounded-full border border-mocha mb-4 object-cover" alt="Order" />
            <p>
              <span className="font-bold">{t('aboutUs.step2_title')}</span> – {t('aboutUs.step2_desc')}
            </p>
          </div>

  
          <div className="flex flex-col items-center max-w-[220px] mx-auto text-center">
            <img src="/assets/images/relax.jpg" className="w-[180px] h-[180px] rounded-full border border-mocha mb-4 object-cover" alt="Relax" />
            <p>
              <span className="font-bold">{t('aboutUs.step3_title')}</span> – {t('aboutUs.step3_desc')}
            </p>
          </div>
        </div>
      </section>


      <section className="mt-20 flex flex-col-reverse lg:flex-row items-center gap-10">
        <div className="w-full lg:w-1/2">
          <img src="/assets/images/homepage/home2.png" alt="Operations" className="w-full max-w-[400px] h-auto mx-auto" />
        </div>
        <div className="w-full lg:w-1/2 flex flex-col text-center lg:text-left">
          <h3 className="text-black text-2xl font-bold">{t('aboutUs.streamlined_operations_title')}</h3>
          <p className="mt-5">{t('aboutUs.streamlined_operations_desc')}</p>

          <h3 className="text-black text-2xl font-bold mt-10">{t('aboutUs.improved_efficiency_title')}</h3>
          <p className="mt-5">{t('aboutUs.improved_efficiency_desc')}</p>
        </div>
      </section>


      <section className="mt-20 flex flex-col lg:flex-row items-center gap-10">
        <div className="w-full lg:w-1/2 flex flex-col text-center lg:text-left">
          <h3 className="text-2xl font-bold text-black">{t('aboutUs.multilingual_support_title')}</h3>
          <p className="mt-5">{t('aboutUs.multilingual_support_desc')}</p>
        </div>
        <div className="w-full lg:w-1/2">
          <img src="/assets/images/homepage/home3.jpg" alt="Multilingual" className="w-full max-w-[400px] h-auto mx-auto" />
        </div>
      </section>


      <section className="mt-20 flex flex-col-reverse lg:flex-row items-center gap-10">
        <div className="w-full lg:w-1/2">
          <img src="/assets/images/home_img2.png" alt="Analytics" className="w-full h-auto max-w-[400px] " />
        </div>
        <div className="w-full lg:w-1/2 flex flex-col text-center lg:text-left">
          <h3 className="text-black text-2xl font-bold">{t('aboutUs.analytics_title')}</h3>
          <p className="mt-5">{t('aboutUs.analytics_desc')}</p>
          <Link to={AppPaths.public.subsciption}>
            <button className="bg-mocha-300 hover:bg-mocha-400 transition-colors rounded-full w-full sm:w-[200px] h-[50px] mt-6 text-white font-bold">
              {t('aboutUs.view_pricing').toUpperCase()}
            </button>
          </Link>
        </div>
      </section>

      <div className="pt-20 text-center">
        <p className="font-bold text-3xl text-mocha-600 text-black">
          {t('aboutUs.upgrade_text')}
        </p>
        <button className="bg-mocha-300 hover:bg-mocha-400 transition-colors rounded-full mt-6 w-[180px] h-[50px] text-white font-bold">
          {t('aboutUs.get_started').toUpperCase()}
        </button>
      </div>
    </div>
  );
}

export default AboutUs;
