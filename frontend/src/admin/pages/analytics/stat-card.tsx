import React from 'react';

interface StatCardProps {
    title: string;
    value: string | number;
    suffix?: string;
}

const StatCard: React.FC<StatCardProps> = ({ title, value, suffix }) => {
    return (
        <div className="text-center w-full p-5 bg-white border border-gray-200 rounded-lg shadow-sm dark:bg-gray-800 dark:border-gray-700">      
            <p className="mb-3 font-normal text-md text-gray-700 dark:text-gray-400">{title}</p>   
            <h5 className="mb-2 text-xl font-bold tracking-tight text-gray-900 dark:text-white">
                {value}{suffix ? ` ${suffix}` : ""}
            </h5>     
        </div>
    );
};

export default StatCard;
