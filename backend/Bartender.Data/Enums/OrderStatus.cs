using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartender.Data.Enums;

public enum OrderStatus
{
    created,
    approved,
    delivered,
    paid,
    closed,
    cancelled
}
