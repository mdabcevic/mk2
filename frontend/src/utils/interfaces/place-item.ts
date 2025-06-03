export interface IPlaceItem{
    id:number,
    businessName:string,
    address:string,
    imageUrl:string,
    cityName:string,
    workHours?:string,
    banner:string;
    description:string,
    freeTablesCount:number;
    images: PlaceImages[]
}

export enum ImageType{
    banner="banner", 
    gallery= "gallery",
    logo= "logo",
    blueprint = "blueprints"
} 

export interface PlaceImages {
  imageType: ImageType;
  urls: string[];
}