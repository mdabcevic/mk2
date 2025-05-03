import { useTranslation } from "react-i18next";
import { AppPaths } from "../../utils/routing/routes";
import { Button } from "../../utils/components/button";

function HomePage() {
  const { t } = useTranslation("public");

  return (
    <div>

    <section className="w-full flex-row items-center justify-center gap-10 mt-5 bg-[#DFD8CD] border-[#7E5E44] border-b-2">
        <div className="flex flex-col w-full m-auto max-w-[1200px] lg:flex-row items-center justify-center gap-10 mt-5">
            <div className="md:max-w-[500px] sm:max-w-[300px] w-full lg:w-1/2">
                <img src="/assets/images/homepage/explore.png" alt="Home intro" className="w-full h-auto" />
            </div>
            <div className="w-full lg:w-1/2 flex flex-col text-center pb-4">
                <h1 className="text-1xl lg:text-3xl font-bold text-center text-black">{t('home.explore')}</h1>
                <p className="mt-5 text-lg text-center">{t('home.explore_desc')}</p>
                <Button 
                    textValue={t("discover").toUpperCase()}
                    size="medium"
                    type="brown"
                    navigateTo={AppPaths.public.places}
                />
            </div>
        </div>
    </section>

    <div className="flex flex-col max-w-[1200px] mx-auto pt-10 pb-20 px-4 sm:px-6 lg:px-8">
      
      <section className="flex flex-col-reverse lg:flex-row items-center gap-10 mt-5">
        <div className="w-full lg:w-1/2 flex flex-col text-center lg:text-left">
          <h1 className="text-1xl lg:text-4xl font-bold text-black">{t('home.title')}</h1>
          <p className="mt-5 text-lg">{t('home.subtitle')}</p>
        </div>
        <div className="md:max-w-[500px] sm:max-w-[300px] w-full lg:w-1/2">
          <img src="/assets/images/homepage/home1.png" alt="Home intro" className="w-full h-auto" />
        </div>
      </section>


      <section className="mt-20">
        <h2 className="font-bold text-3xl text-black mb-10">{t('home.how_it_works')}</h2>
        <div className="flex flex-col sm:flex-row justify-center gap-10 text-black">
          

          <div className="flex flex-col items-center max-w-[220px] mx-auto text-center">
            <img src="/assets/images/scan_qr.png" className="w-[180px] h-[180px] rounded-full border border-mocha mb-4 object-cover" alt="Scan QR" />
            <p>
              <span className="font-bold">{t('home.step1_title')}</span> – {t('home.step1_desc')}
            </p>
          </div>

          <div className="flex flex-col items-center max-w-[220px] mx-auto text-center">
            <img src="/assets/images/order.jpg" className="w-[180px] h-[180px] rounded-full border border-mocha mb-4 object-cover" alt="Order" />
            <p>
              <span className="font-bold">{t('home.step2_title')}</span> – {t('home.step2_desc')}
            </p>
          </div>

  
          <div className="flex flex-col items-center max-w-[220px] mx-auto text-center">
            <img src="/assets/images/relax.jpg" className="w-[180px] h-[180px] rounded-full border border-mocha mb-4 object-cover" alt="Relax" />
            <p>
              <span className="font-bold">{t('home.step3_title')}</span> – {t('home.step3_desc')}
            </p>
          </div>
        </div>
      </section>


      <section className="mt-20 flex flex-col-reverse lg:flex-row items-center gap-10">
        <div className="w-full lg:w-1/2">
          <img src="/assets/images/homepage/home2.png" alt="Operations" className="w-full max-w-[400px] h-auto mx-auto" />
        </div>
        <div className="w-full lg:w-1/2 flex flex-col text-center lg:text-left">
          <h3 className="text-black text-2xl font-bold">{t('home.streamlined_operations_title')}</h3>
          <p className="mt-5">{t('home.streamlined_operations_desc')}</p>

          <h3 className="text-black text-2xl font-bold mt-10">{t('home.improved_efficiency_title')}</h3>
          <p className="mt-5">{t('home.improved_efficiency_desc')}</p>
        </div>
      </section>


      <section className="mt-20 flex flex-col lg:flex-row items-center gap-10">
        <div className="w-full lg:w-1/2 flex flex-col text-center lg:text-left">
          <h3 className="text-2xl font-bold text-black">{t('home.multilingual_support_title')}</h3>
          <p className="mt-5">{t('home.multilingual_support_desc')}</p>
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
          <h3 className="text-black text-2xl font-bold">{t('home.analytics_title')}</h3>
          <p className="mt-5">{t('home.analytics_desc')}</p>
          <Button 
            textValue={t('home.view_pricing').toUpperCase()}
            size="medium"
            type="brown"
            navigateTo={AppPaths.public.subsciption}
        />
        </div>
      </section>

      <div className="pt-20 text-center">
        <p className="font-bold text-3xl text-mocha-600 text-black">
          {t('home.upgrade_text')}
        </p>
        <Button 
            textValue={t('home.get_started').toUpperCase()}
            size="medium"
            type="brown"
            navigateTo={AppPaths.public.contactUs}
        />
      </div>
    </div>
    </div>
  );
}

export default HomePage;
