import { TableColor, TableStatusString } from "./constants";


export function getTableColor(status: string){
    switch (status) {
          case TableStatusString.occupied:
            return TableColor.occupied;
          case TableStatusString.reserved:
            return TableColor.reserved;
          default:
            return TableColor.empty;
        }

}