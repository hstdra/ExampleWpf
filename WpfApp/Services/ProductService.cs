using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WpfApp.Helpers;
using WpfApp.Models;
using WpfApp.Stores;

namespace WpfApp.Services;

public interface IProductService
{
    void LoadProducts();
}

public class ProductService : IProductService
{
    private readonly ProductStore _productStore;

    public ProductService(ProductStore productStore)
    {
        _productStore = productStore;
    }

    public void LoadProducts()
    {
        _productStore.Products ??= new ObservableCollection<Product>();
        _productStore.Products.Add(new() {Name = $"Product {Random.Shared.Next()}", Price = Random.Shared.Next()});
    }
}