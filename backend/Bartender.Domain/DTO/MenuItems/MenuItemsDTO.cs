using Bartender.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartender.Domain.DTO.MenuItems
{
    public class MenuItemsDTO
    {
        public int Id { get; set; }
        public string Place { get; set; }
        public bool IsAvailable { get; set; }
        public string? Description { get; set; }
    }
}
