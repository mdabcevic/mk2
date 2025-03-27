using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartender.Domain.DTO.Products
{
    public class ProductBaseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Volume { get; set; }
        public string Category { get; set; }
    }
}
