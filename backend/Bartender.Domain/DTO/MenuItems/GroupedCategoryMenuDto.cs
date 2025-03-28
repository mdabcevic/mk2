using Bartender.Domain.DTO.Places;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartender.Domain.DTO.MenuItems
{
    public class GroupedCategoryMenuDto
    {
        public string Category { get; set; }
        public IEnumerable<MenuItemBaseDto> Items { get; set; }
    }
}
