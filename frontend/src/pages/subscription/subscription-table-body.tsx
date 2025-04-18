type BenefitRow = {
    text: string;
    values: (string | boolean)[];
};

type SubscriptionTableBodyProps = {
    t: (key: string) => string;
    benefits: BenefitRow[];
};

const bgColors = ["bg-gray-100", "bg-blue-100", "bg-orange-100"];

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
                                    <img src="../../../public/assets/images/checkmark.png" className="w-4 h-4 mx-auto" alt="✓" />
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
