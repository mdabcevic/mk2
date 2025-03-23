using Bartender.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Bartender.Domain.DTO.MenuItems;

namespace Bartender.Domain.DTO.Products
{
    public class ProductsDTO : ProductsBaseDTO
    {
        public ProductCategoryDTO Category { get; set; }
        //public IEnumerable<MenuItemsDTO>? Menu {  get; set; }
    }
}
