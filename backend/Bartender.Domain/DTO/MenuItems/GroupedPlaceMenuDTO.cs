using Bartender.Data.Models;
using Bartender.Domain.DTO.Places;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartender.Domain.DTO.MenuItems
{
    public class GroupedPlaceMenuDTO
    {
        public PlaceDTO Place {  get; set; }
        public IEnumerable<MenuItemsBaseDTO> Items { get; set; }
    }
}
