export const subscriptionPlans = [
    {
        title: "Basic",
        price: "€19.99",
        textColor: "text-neutral-500",
        bgColor: "bg-neutral-500",
        hoverColor: "hover:bg-neutral-700",
        features: ["basic_description", "subscription_cancel"],
    },
    {
        title: "Standard",
        price: "€39.99",
        textColor: "text-[#AE8768]",
        bgColor: "bg-[#AE8768]",
        hoverColor: "hover:bg-brown-400",
        features: ["pro_description", "subscription_cancel"],
    },
    {
        title: "Premium",
        price: "€59.99",
        textColor: "text-brown-500",
        bgColor: "bg-brown-500",
        hoverColor: "hover:bg-brown-400",
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