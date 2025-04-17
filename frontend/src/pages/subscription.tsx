import { useTranslation } from "react-i18next";

function Subscription() {
    const { t } = useTranslation("public");

    return (
        <div className="flex flex-col max-w-[1200px] mx-auto pt-10 pb-50 px-4">
            <h2 className="text-3xl text-center font-semibold text-white mt-5">
                {t("choose_subscription")}
            </h2>
            <div className="flex flex-col sm:flex-row items-stretch gap-2 lg:gap-4 mt-15 lg:px-10">
            {[
                {
                    title: "Basic",
                    price: "€5.99",
                    "text-color": "text-neutral-500",
                    "bg-color": "bg-neutral-500",
                    features: [
                        t("basic_description"),
                        t("subscription_cancel")
                    ],
                },
                {
                    title: "Pro",
                    price: "€15.99",
                    "text-color": "text-blue-500",
                    "bg-color": "bg-blue-500",
                    features: [
                        t("pro_description"),
                        t("subscription_cancel")
                    ],
                },
                {
                    title: "Premium",
                    price: "€34.99",
                    "text-color": "text-orange-400",
                    "bg-color": "bg-orange-400",
                    features: [
                        t("premium_description"),
                        t("subscription_cancel")
                    ],
                },
            ].map((subscription, index) => (
                <div
                    key={index}
                    className="w-full lg:w-1/3 flex flex-col border border-neutral-400 rounded-3xl bg-neutral-100 px-5">
                    <div className="flex flex-col flex-grow">
                        <h2 className={`${subscription["text-color"]} p-5 pb-10 text-center font-semibold text-3xl`}>
                            {subscription.title}
                        </h2>
                        <p className="text-3xl pb-5 border-b border-neutral-400">
                            {subscription.price}
                            <span className="text-base">/{t("month").toLowerCase()}</span>
                        </p>
                        <ul className="p-5 list-disc text-base mb-15 min-h-[80px]">
                            {subscription.features.map((feature, i) => (
                                <li key={i}>{feature}</li>
                            ))}
                        </ul>
                    </div>
                    <button
                        className={`${subscription["bg-color"]} text-white rounded-full w-full h-[50px] mb-4 font-semibold tracking-widest`}>
                        {t("get_subscription").toUpperCase()} {subscription.title.toUpperCase()}
                    </button>
                </div>
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
                        <tbody>
                        <tr className="border border-neutral-300">
                            <td className="px-4 py-2">{t("qr_code_ordering")}</td>
                            <td className="px-4 py-2 text-center bg-gray-100"><img src="../assets/images/checkmark.png" className="w-4 h-4 mx-auto"/></td>
                            <td className="px-4 py-2 text-center bg-blue-100"><img src="../assets/images/checkmark.png" className="w-4 h-4 mx-auto"/></td>
                            <td className="px-4 py-2 text-center bg-orange-100"><img src="../assets/images/checkmark.png" className="w-4 h-4 mx-auto"/></td>
                        </tr>
                        <tr className="border border-neutral-300">
                            <td className="px-4 py-2">{t("basic_menu_management")}</td>
                            <td className="px-4 py-2 text-center bg-gray-100"><img src="../assets/images/checkmark.png" className="w-4 h-4 mx-auto"/></td>
                            <td className="px-4 py-2 text-center bg-blue-100"><img src="../assets/images/checkmark.png" className="w-4 h-4 mx-auto"/></td>
                            <td className="px-4 py-2 text-center bg-orange-100"><img src="../assets/images/checkmark.png" className="w-4 h-4 mx-auto"/></td>
                        </tr>
                        <tr className="border border-neutral-300">
                            <td className="px-4 py-2">{t("custom_products_limit")}</td>
                            <td className="px-4 py-2 text-center bg-gray-100">{t("up_to")} 15</td>
                            <td className="px-4 py-2 text-center bg-blue-100">{t("up_to")} 50</td>
                            <td className="px-4 py-2 text-center bg-orange-100">{t("unlimited")}</td>
                        </tr>
                        <tr className="border border-neutral-300">
                            <td className="px-4 py-2">{t("real_time_order_alerts")}</td>
                            <td className="px-4 py-2 text-center bg-gray-100"><img src="../assets/images/checkmark.png" className="w-4 h-4 mx-auto"/></td>
                            <td className="px-4 py-2 text-center bg-blue-100"><img src="../assets/images/checkmark.png" className="w-4 h-4 mx-auto"/></td>
                            <td className="px-4 py-2 text-center bg-orange-100"><img src="../assets/images/checkmark.png" className="w-4 h-4 mx-auto"/></td>
                        </tr>
                        <tr className="border border-neutral-300">
                            <td className="px-4 py-2">{t("support")}</td>
                            <td className="px-4 py-2 text-center bg-gray-100">{t("email_support")}</td>
                            <td className="px-4 py-2 text-center bg-blue-100">{t("priority_email_support")}</td>
                            <td className="px-4 py-2 text-center bg-orange-100">{t("priority_live_chat_support")}</td>
                        </tr>
                        <tr className="border border-neutral-300">
                            <td className="px-4 py-2">{t("custom_table_layout")}</td>
                            <td className="px-4 py-2 text-center bg-gray-100"></td>
                            <td className="px-4 py-2 text-center bg-blue-100"><img src="../assets/images/checkmark.png" className="w-4 h-4 mx-auto"/></td>
                            <td className="px-4 py-2 text-center bg-orange-100"><img src="../assets/images/checkmark.png" className="w-4 h-4 mx-auto"/></td>
                        </tr>
                        <tr className="border border-neutral-300">
                            <td className="px-4 py-2">{t("advanced_analytics")}</td>
                            <td className="px-4 py-2 text-center bg-gray-100"></td>
                            <td className="px-4 py-2 text-center bg-blue-100"></td>
                            <td className="px-4 py-2 text-center bg-orange-100"><img src="../assets/images/checkmark.png" className="w-4 h-4 mx-auto"/></td>
                        </tr>
                        <tr className="border border-neutral-300">
                            <td className="px-4 py-2">{t("menu_performance_tracking")}</td>
                            <td className="px-4 py-2 text-center bg-gray-100"></td>
                            <td className="px-4 py-2 text-center bg-blue-100"></td>
                            <td className="px-4 py-2 text-center bg-orange-100"><img src="../assets/images/checkmark.png" className="w-4 h-4 mx-auto"/></td>
                        </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
}
export default Subscription;