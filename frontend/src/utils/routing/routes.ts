
const AdminRoot="/admin";

export const AppPaths = {
    public: {
        home:"/",
        aboutUs:"/aboutus",
        places:"/places",
        placeDetails:"/bar/:id",
        menu: "/place/:placeId/menu",
        redirectPage: "/table-lookup/:placeId/:salt",
        login:"/login",
        subsciption:"/subscription",
    },
    
    admin:{
        dashboard:AdminRoot,
        products:`${AdminRoot}/products`,
        tables:`${AdminRoot}/tables`,
        scanner:`${AdminRoot}/scanner`,
    }


}