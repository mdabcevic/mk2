using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartender.Domain.DTO.Products
{
    public class UpsertProductDTO
    {
        public string Name { get; set; }
        public string? Volume {  get; set; }
        public int CategoryId { get; set; }
    }
}
