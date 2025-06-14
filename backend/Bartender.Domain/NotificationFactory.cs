﻿using Bartender.Data.Models;
using Bartender.Data;

namespace Bartender.Domain;

public static class NotificationFactory
{
    public static TableNotification ForTableStatus(Table table, string message, NotificationType type, bool isPending = true)
    {
        return new TableNotification
        {
            Type = type,
            TableLabel = table.Label,
            Message = message,
            Pending = isPending
        };
    }

    public static TableNotification ForOrder(Table table, int orderId, string message, NotificationType type, bool isPending = true)
    {
        return new TableNotification
        {
            Type = type,
            TableLabel = table.Label,
            OrderId = orderId,
            Message = message,
            Pending = isPending
        };
    }
}

