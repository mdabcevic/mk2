
const AdminRoot="/admin";

export const AppPaths = {
    public: {
        home:"/",
        aboutUs:"/aboutus",
        contactUs: "/contactus",
        places:"/places",
        placeDetails:"/place/:id",
        menu: "/place/:placeId/menu",
        placeTables:"/place/:placeId/tables",
        redirectPage: "/table-lookup/:placeId/:salt",
        login:"/login",
        subsciption:"/subscription",
        myOrders:"/:placeId/my-orders"
    },
    
    admin:{
        dashboard:AdminRoot,
        management:`${AdminRoot}/management`,
        products:`${AdminRoot}/products`,
        tables:`${AdminRoot}/tables`,
        notifications:`${AdminRoot}/notifications`,
        scanner:`${AdminRoot}/scanner`,
        analytics:`${AdminRoot}/analytics`,
    },
}