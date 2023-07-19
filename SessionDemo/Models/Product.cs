using System;
using System.Collections.Generic;

namespace SessionDemo.Models;

public partial class Product
{
    public int ProdId { get; set; }

    public string ProdName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Quantity { get; set; }

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
