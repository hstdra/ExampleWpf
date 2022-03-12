using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommonServiceLocator;
using WpfApp.Models;
using WpfGenerator;

namespace WpfApp.Stores;

[ChangedListener]
public partial class ProductStore
{
    private ObservableCollection<Product>? _products;

    public ProductStore()
    {
    }
    
    public ProductStore(RootStore rootStore)
    {
        OnProductsChanged += (products) =>
        {
            products!.CollectionChanged += (sender, args) =>
            {
                rootStore.OnChanged?.Invoke(rootStore);
            };
        };
        OnChanged += (_) => rootStore.OnChanged?.Invoke(rootStore);
    }
}