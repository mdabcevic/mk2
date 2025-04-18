import { useTranslation } from "react-i18next";
import SubscriptionCard from './subscription-card';
import SubscriptionTableBody from './subscription-table-body';
import { subscriptionPlans, benefitsData } from './subscription-data';

function Subscription() {
    const { t } = useTranslation("public");

    return (
        <div className="flex flex-col max-w-[1200px] mx-auto pt-10 pb-50 px-4">
            <h2 className="text-3xl text-center font-semibold text-white mt-5">
                {t("choose_subscription")}
            </h2>
            <div className="flex flex-col sm:flex-row items-stretch gap-2 lg:gap-4 mt-15 lg:px-10">
                {subscriptionPlans.map((subscription, index) => (
                    <SubscriptionCard
                        key={index}
                        title={subscription.title}
                        price={subscription.price}
                        textColor={subscription.textColor}
                        bgColor={subscription.bgColor}
                        features={subscription.features}
                        t={t}
                    />
                ))}
            </div>

            <div className="w-full mx-auto mt-20">
                <div className="overflow-x-auto rounded-tl-3xl rounded-tr-3xl">
                    <table className="table-auto w-full text-left bg-white rounded-tl-3xl rounded-tr-3xl text-xs sm:text-sm md:text-lg">
                        <thead>
                        <tr className="border border-neutral-300">
                            <th className="px-4 py-2 rounded-tl-2xl  w-1/3">{t("benefits")}</th>
                            <th className="px-4 py-2 text-center bg-gray-200 w-2/9">Basic {t("subscription")}</th>
                            <th className="px-4 py-2 text-center bg-blue-200 w-2/9">Pro {t("subscription")}</th>
                            <th className="px-4 py-2 text-center bg-orange-200 rounded-tr-2xl w-2/9">Premium {t("subscription")}</th>
                        </tr>
                        </thead>
                        <SubscriptionTableBody t={t} benefits={benefitsData} /> 
                    </table>
                </div>
            </div>
        </div>
    );
}
export default Subscription;