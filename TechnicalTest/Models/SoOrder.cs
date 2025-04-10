using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TechnicalTest.Models;

public partial class SoOrder
{
    public long SoOrderId { get; set; }
    public string? OrderNo { get; set; }
    public DateTime OrderDate { get; set; }
    public string? Address { get; set; }
    public int ComCustomerId { get; set; }

    public List<SoItem> Items { get; set; } = new List<SoItem>();

    [ValidateNever]
     public ComCustomer ComCustomer { get; set; } 

}

