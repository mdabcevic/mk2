using Bartender.Domain.DTO.Places;
using Bartender.Domain.DTO.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Bartender.Domain.DTO.MenuItems
{
    public class MenuItemBaseDto
    {
        public ProductBaseDto Product { get; set; }
        [JsonIgnore]
        public decimal Price { get; set; }
        [JsonPropertyName("price")]
        public string FormattedPrice => Price.ToString("0.00");
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }
    }
}
