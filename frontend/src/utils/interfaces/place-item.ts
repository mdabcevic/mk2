export interface IPlaceItem{
    id:number,
    businessName:string,
    address:string,
    imageUrl:string,
    cityName:string,
    workHours?:string,
    images:string[];
    freeTablesCount:number
}