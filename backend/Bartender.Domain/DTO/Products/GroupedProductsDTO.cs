using Bartender.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartender.Domain.DTO.Products
{
    public class GroupedProductsDto
    {
        public string Category { get; set; }
        public IEnumerable<ProductBaseDto>? Products { get; set; }
    }
}
