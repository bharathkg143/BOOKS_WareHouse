using System;
using System.Collections.Generic;

namespace SessionDemo.Models;

public partial class Article
{
    public int ArticleId { get; set; }

    public string ArticleTitle { get; set; } = null!;

    public string ArticleContent { get; set; } = null!;

    public int ProdId { get; set; }

    public int? ProductProdId { get; set; }

    public virtual Product? ProductProd { get; set; }
}
