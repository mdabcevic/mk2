using Bartender.Domain.DTO.Places;
using Bartender.Domain.DTO.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartender.Domain.DTO.MenuItems
{
    public class MenuItemsBaseDTO
    {
        public ProductsBaseDTO Product { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }
    }
}
