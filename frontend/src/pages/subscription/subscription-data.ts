export const subscriptionPlans = [
    {
        title: "Basic",
        price: "€5.99",
        textColor: "text-neutral-500",
        bgColor: "bg-neutral-500",
        features: ["basic_description", "subscription_cancel"],
    },
    {
        title: "Pro",
        price: "€15.99",
        textColor: "text-blue-500",
        bgColor: "bg-blue-500",
        features: ["pro_description", "subscription_cancel"],
    },
    {
        title: "Premium",
        price: "€34.99",
        textColor: "text-orange-400",
        bgColor: "bg-orange-400",
        features: ["premium_description", "subscription_cancel"],
    },
];

export const benefitsData = [
    { text: "qr_code_ordering", values: [true, true, true] },
    { text: "basic_menu_management", values: [true, true, true] },
    { text: "custom_products_limit", values: ["15", "50", "unlimited"] },
    { text: "real_time_order_alerts", values: [true, true, true] },
    { text: "support", values: ["email_support", "priority_email_support", "priority_live_chat_support"] },
    { text: "custom_table_layout", values: [false, true, true] },
    { text: "advanced_analytics", values: [false, false, true] },
    { text: "menu_performance_tracking", values: [false, false, true] },
];