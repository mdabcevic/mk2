import { ProductsByDayOfWeek, PopularProducts} from "./analytics-interface";

export const getDataForChart = (
    data: ProductsByDayOfWeek[],
    selectedDay: string
): PopularProducts[] => {
    if (!data.length) return [];

    const dayData = data.find(
        (day) => day.weekGroup.toLowerCase() === selectedDay.toLowerCase()
    ) || data[0];

    return dayData.popularProducts;
};

export const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat("en-GB", {
        day: "numeric",
        month: "numeric",
        year: "numeric",
    }).format(date);
};

export const formatDateToMonthYear = (dateString: string): string => {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat("en-US", {
        month: "long",
        year: "numeric",
    }).format(date);
};

type MonthYearOption = { value: number; label: string };

export const getMonthAndYearOptions = (
    firstOrderDate: string,
    lastOrderDate: string,
    selectedYear?: number
): { monthOptions: MonthYearOption[]; yearOptions: MonthYearOption[] } => {
    const startDate = new Date(firstOrderDate);
    const endDate = new Date(lastOrderDate);

    const years = Array.from(
        { length: endDate.getFullYear() - startDate.getFullYear() + 1 },
        (_, i) => startDate.getFullYear() + i
    );

    let months: number[] = [];

    if (selectedYear !== undefined) {
        if (selectedYear === startDate.getFullYear() && selectedYear === endDate.getFullYear()) {
            const monthLength = endDate.getMonth() - startDate.getMonth() + 1;
            months = Array.from({ length: monthLength }, (_, i) => startDate.getMonth() + 1 + i);
        } 
        else if (selectedYear === startDate.getFullYear()) {
            const monthLength = 12 - startDate.getMonth();
            months = Array.from({ length: monthLength }, (_, i) => startDate.getMonth() + 1 + i);
        } 
        else if (selectedYear === endDate.getFullYear()) {
            const monthLength = endDate.getMonth() + 1;
            months = Array.from({ length: monthLength }, (_, i) => i + 1);
        }
    } 
    else if (startDate.getFullYear() === endDate.getFullYear()) {
        const monthLength = endDate.getMonth() - startDate.getMonth() + 1;
        months = Array.from({ length: monthLength }, (_, i) => startDate.getMonth() + 1 + i);
    } 
    else {
        months = Array.from({ length: 12 }, (_, i) => i + 1);
    }

    const monthOptions = months.map((m) => ({
        value: m,
        label: m.toString(),
    }));

    const yearOptions = years.map((y) => ({
        value: y,
        label: y.toString(),
    }));

    return { monthOptions, yearOptions };
};
