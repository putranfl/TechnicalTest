using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TechnicalTest.Models;

public class OrderListViewModel
{

    public List<SoOrder> Orders { get; set; }


    public string? Keyword { get; set; }
    public DateTime? OrderDate { get; set; }

    public string OrderNo { get; set; }
    public int ComCustomerId { get; set; }
    public string Address { get; set; }

    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}



