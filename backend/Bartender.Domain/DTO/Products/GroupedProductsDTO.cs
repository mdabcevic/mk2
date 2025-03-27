using Bartender.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartender.Domain.DTO.Products
{
    public class GroupedProductsDTO
    {
        public string Category { get; set; }
        public IEnumerable<ProductBaseDTO>? Products { get; set; }
    }
}
