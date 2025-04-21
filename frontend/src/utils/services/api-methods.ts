

export const ApiMethods ={

    getPlaces: "/api/places",
    getPlaceById: "/api/places/{id}",
    getPlaceTablesByPlaceId: "/api/tables/{id}/all",

    getMenuByPlaceId: "/api/menu/{id}",
    saveProductsToPlace: "/api/menu",
    updateMenuItem:"/api/menu",

    getAllProducts: "/api/product",
    getProductCategories: "/api/product/categories",

    getGuestToken: "/api/tables/lookup",
    login:"api/auth",
    joinTable:"api/tables/join",

    createOrder:"/api/order",
    getMyOrders:"/api/order/my-orders",
    getActiveOrders:"/api/order/active/{placeId}", 
    getClosedOrders:"/api/order/closed/{placeId}",
    updateOrderStatus:"/api/order/status/{orderId}",

    saveOrUpdateTables:"/api/tables/bulk-upsert",

    getPlaceTablesByCurrentUser:"/api/tables",
    changeTableStatus:"/api/tables/{salt}/status",

    callBartender:"/api/places/notify-staff/{salt}",

    regenerateQrCode:"/api/tables/{label}/rotate-token"



}