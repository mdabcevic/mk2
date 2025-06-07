type BenefitRow = {
    text: string;
    values: (string | boolean)[];
};

type SubscriptionTableBodyProps = {
    t: (key: string) => string;
    benefits: BenefitRow[];
};

const bgColors = ["bg-neutral-200", "bg-[#D4C0B0]", "bg-[#60483D] text-white"];

const SubscriptionTableBody = ({ t, benefits }: SubscriptionTableBodyProps) => {
    return (
        <tbody>
            {benefits.map((benefit, rowIndex) => (
                <tr key={rowIndex} className="border border-neutral-300">
                    <td className="px-4 py-2">{t(benefit.text)}</td>
                    {benefit.values.map((value, colIndex) => {
                        const bg = bgColors[colIndex];
                        return (
                            <td key={colIndex} className={`px-4 py-2 text-center ${bg}`}>
                                {value === true ? (
                                    <img src={`/assets/images/${colIndex === 2 ? "checkmark_white.svg" : "checkmark.svg"}`} className="w-5 h-5 md:w-6 md:h-6 mx-auto" alt="✓" />
                                ) : value === false || value === "" ? null : isNaN(Number(value as string)) ? (
                                    t(value as string)
                                ) : (
                                    `${t("up_to")} ${value}`
                                )}
                            </td>
                        );
                    })}
                </tr>
            ))}
        </tbody>
    );
};

export default SubscriptionTableBody;
