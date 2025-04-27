
const AdminRoot="/admin";

export const AppPaths = {
    public: {
        home:"/",
        aboutUs:"/aboutus",
        places:"/places",
        placeDetails:"/place/:id",
        menu: "/place/:placeId/menu",
        placeTables:"/place/tables/:id",
        redirectPage: "/table-lookup/:placeId/:salt",
        login:"/login",
        subsciption:"/subscription",
    },
    
    admin:{
        dashboard:AdminRoot,
        products:`${AdminRoot}/products`,
        tables:`${AdminRoot}/tables`,
        notifications:`${AdminRoot}/notifications`,
        scanner:`${AdminRoot}/scanner`,
        
    },
}