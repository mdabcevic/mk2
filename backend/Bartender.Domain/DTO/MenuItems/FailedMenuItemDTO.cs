using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartender.Domain.DTO.MenuItems
{
    public class FailedMenuItemDTO
    {
        public UpsertMenuItemDTO MenuItem { get; set; }
        public string ErrorMessage { get; set; }
    }
}
