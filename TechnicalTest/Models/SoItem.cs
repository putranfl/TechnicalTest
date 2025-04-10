using System;
using System.Collections.Generic;

namespace TechnicalTest.Models;

using System.Text.Json.Serialization;

public partial class SoItem
{
    public long SoItemId { get; set; }
    public long SoOrderId { get; set; }

    [JsonPropertyName("itemName")]
    public string ItemName { get; set; } = null!;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("price")]
    public double Price { get; set; }
}



