
const AdminRoot="/admin";

export const AppPaths = {
    public: {
        home:"/",
        aboutUs:"aboutus",
        placeDetails:"bar/:id",
        menu:"/place/:placeId/menu",
        login:"login"
    },
    
    admin:{
        dashboard:AdminRoot,
        products:`${AdminRoot}/products`,
        tables:`${AdminRoot}/tables`,
        scanner:`${AdminRoot}/scanner`,
    }


}