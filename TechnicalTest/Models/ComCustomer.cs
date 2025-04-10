using System;
using System.Collections.Generic;

namespace TechnicalTest.Models;

public partial class ComCustomer
{
    public int ComCustomerId { get; set; }

    public string? CustomerName { get; set; }

    public ICollection<SoOrder> SoOrders { get; set; } // optional
}
