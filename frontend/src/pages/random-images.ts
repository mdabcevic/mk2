import { IPlaceItem } from "../interfaces/place-item";

const imgUrls = [
    "https://www.foodandwine.com/thmb/8rtGtUmtC0KiJCDxAUXP_cfwgM8=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/GTM-Best-US-Bars-Katana-Kitten-FT-BLOG0423-fa9f2ba9925c47abb4afb0abd25d915a.jpg",
    "https://media.cnn.com/api/v1/images/stellar/prod/221004154233-01-world-best-bars-2022.jpg?c=original",
    "https://www.telegraph.co.uk/content/dam/Travel/Destinations/Europe/Spain/Barcelona/nightlife/bar_drink_14_dry_martini3.jpg?imwidth=680",
    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRPwoVSAa6odttTg4kEfFFs0VeWBQekSFwbGw&s",
    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS1KO2rnFIWT2FBjsElCCQKzYdvakJKmUOKU5SDM6tjVF34pgLBk70AKdx0fgMGaQAXOP4&usqp=CAU",
    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTf4ERnSb8biMalV5cHLRxx93JcbXpG0G2IQw&s",
    "https://www.foodandwine.com/thmb/8rtGtUmtC0KiJCDxAUXP_cfwgM8=/1500x0/filters:no_upscale():max_bytes(150000):strip_icc()/GTM-Best-US-Bars-Katana-Kitten-FT-BLOG0423-fa9f2ba9925c47abb4afb0abd25d915a.jpg",
    "https://media.cnn.com/api/v1/images/stellar/prod/221004154233-01-world-best-bars-2022.jpg?c=original",
    "https://www.telegraph.co.uk/content/dam/Travel/Destinations/Europe/Spain/Barcelona/nightlife/bar_drink_14_dry_martini3.jpg?imwidth=680",
    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRPwoVSAa6odttTg4kEfFFs0VeWBQekSFwbGw&s",
    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS1KO2rnFIWT2FBjsElCCQKzYdvakJKmUOKU5SDM6tjVF34pgLBk70AKdx0fgMGaQAXOP4&usqp=CAU",
    "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTf4ERnSb8biMalV5cHLRxx93JcbXpG0G2IQw&s"
]

export const randomImages = (places: IPlaceItem | IPlaceItem[]): any =>{
    console.log("je : ", places)
    if(Array.isArray(places))
    for(var i = 0; i < places.length; i++){
        let randomIndex = Math.floor(Math.random() * imgUrls.length);
        places[i].imageUrl = imgUrls[randomIndex]
    }
    else if(places?.images){
        for(var i = 0; i < 3; i++){
            let randomIndex = Math.floor(Math.random() * imgUrls.length);
            places.images.push(imgUrls[randomIndex])
        }
    }
    console.log("jeaaaaa : ", places)
    return places;
}