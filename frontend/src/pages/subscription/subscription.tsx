import { useTranslation } from "react-i18next";
import SubscriptionCard from './subscription-card';
import SubscriptionTableBody from './subscription-table-body';
import { subscriptionPlans, benefitsData } from './subscription-data';
import { useState, useEffect } from "react";
import ContactModal from "./contact-modal";
import Footer from "../../containers/footer";

function Subscription() {
    const { t } = useTranslation("public");
    const [modalOpen, setModalOpen] = useState(false);
    const [subject, setSubject] = useState("");
    const [message, setMessage] = useState("");

    useEffect(() => {
        if (modalOpen) {
            document.body.style.overflow = "hidden";
        } else {
            document.body.style.overflow = "";
        }

        return () => {
            document.body.style.overflow = "";
        };
    }, [modalOpen]);


    const handleOpenModal = (sub: string) => {
        setSubject(sub + " " + t("subscription")); 
        setMessage(t('inquire_plan', { sub }));
        setModalOpen(true);
    };

    return (
        <>
        <div className="flex flex-col max-w-[1200px] mx-auto pt-50 pb-50 px-4">
            <h2 className="text-3xl text-center font-semibold text-black mt-5">
                {t("choose_subscription")}
            </h2>
            <div className="flex flex-col sm:flex-row items-stretch gap-2 lg:gap-4 mt-15 ">
                {subscriptionPlans.map((subscription, index) => (
                    <SubscriptionCard
                        key={index}
                        title={subscription.title}
                        price={subscription.price}
                        textColor={subscription.textColor}
                        bgColor={subscription.bgColor}
                        hoverColor={subscription.hoverColor}
                        features={subscription.features}
                        t = {t}
                        onSubscribe={() => handleOpenModal(subscription.title)}
                    />
                ))}
            </div>

            <div className="w-full mx-auto mt-20">
                <div className="overflow-x-auto rounded-2xl border-[#A3A3A3] border-[2px]">
                    <table className="table-auto w-full text-left bg-white rounded-3xl text-xs sm:text-sm md:text-lg">
                        <thead>
                        <tr className="border border-neutral-300">
                            <th className="px-4 py-2 rounded-tl-2xl  w-1/3">{t("benefits")}</th>
                            <th className="px-4 py-2 text-center bg-neutral-300 w-2/9">Basic {t("subscription")}</th>
                            <th className="px-4 py-2 text-center bg-[#AE8768] w-2/9">Pro {t("subscription")}</th>
                            <th className="px-4 py-2 text-center bg-brown-500 text-white rounded-tr-2xl w-2/9">Premium {t("subscription")}</th>
                        </tr>
                        </thead>
                        <SubscriptionTableBody t={t} benefits={benefitsData} /> 
                    </table>
                </div>
            </div>
            <ContactModal
                isOpen={modalOpen}
                onClose={() => setModalOpen(false)}
                subject={subject}
                message={message}
                t = {t}
            />

        </div>
        <Footer />
        </>
    );
}
export default Subscription;