using Bartender.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bartender.Domain.DTO.Products;
using Bartender.Domain.DTO.Places;

namespace Bartender.Domain.DTO.MenuItems
{
    public class MenuItemDto : MenuItemBaseDto
    {
        public Places.PlaceDto Place { get; set; }
    }
}
