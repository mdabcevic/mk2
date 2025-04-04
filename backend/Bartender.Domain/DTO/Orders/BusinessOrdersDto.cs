using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartender.Domain.DTO.Orders;

public class BusinessOrdersDto
{
    public PlaceDto Place { get; set; }
    public List<OrderBaseDto> Orders { get; set; }
}
