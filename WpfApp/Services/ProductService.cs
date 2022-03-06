using System;
using System.Collections.ObjectModel;
using WpfApp.Models;
using WpfApp.Stores;

namespace WpfApp.Services;

public interface IProductService
{
    void LoadProducts();
}

public class ProductService : IProductService
{
    private readonly Store _store;

    public ProductService(Store store)
    {
        _store = store;
    }

    public void LoadProducts()
    {
        _store.Products ??= new ObservableCollection<Product>();
        _store.Products.Add(new() {Name = $"Product {Random.Shared.Next()}", Price = Random.Shared.Next()});
    }
}