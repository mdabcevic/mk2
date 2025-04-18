type SubscriptionCardProps = {
    title: string;
    price: string;
    textColor: string;
    bgColor: string;
    hoverColor: string;
    features: string[];
    t: (key: string) => string;
    onSubscribe: () => void;
};

const SubscriptionCard = ({ title, price, textColor, bgColor, hoverColor, features, t, onSubscribe }: SubscriptionCardProps) => {
    return (
        <div className="w-full lg:w-1/3 flex flex-col border border-neutral-400 rounded-3xl bg-neutral-100 px-5">
            <div className="flex flex-col flex-grow">
                <h2 className={`${textColor} p-5 pb-10 text-center font-semibold text-3xl`}>
                    {title}
                </h2>
                <p className="text-3xl pb-5 border-b border-neutral-400">
                    {price}
                    <span className="text-base">/{t("month").toLowerCase()}</span>
                </p>
                <ul className="p-5 list-disc text-base mb-15 min-h-[80px]">
                    {features.map((feature, i) => (
                        <li key={i}>{t(feature)}</li>
                    ))}
                </ul>
            </div>
            <button
                onClick={onSubscribe}
                className={`${bgColor} text-white rounded-full w-full h-[50px] mb-4 font-semibold tracking-widest ${hoverColor}`}>
                {t("get_subscription").toUpperCase()} {title.toUpperCase()}
            </button>
        </div>
    );
};

export default SubscriptionCard;
