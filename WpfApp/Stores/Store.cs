using System.Collections.Generic;
using WpfApp.Models;
using WpfGenerator;

namespace WpfApp.Stores;

[Store]
public partial class Store
{
    private ICollection<Product>? _products;
}