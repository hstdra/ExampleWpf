using System.Collections.ObjectModel;
using WpfApp.Models;
using WpfGenerator;

namespace WpfApp.Stores;

[ChangedListener]
public partial class ProductStore
{
    private ObservableCollection<Product>? _products;

    // Using for serialize/desialize
    public ProductStore() {}
    
    public ProductStore(RootStore rootStore)
    {
        OnChanged += (_) => rootStore.OnChanged?.Invoke(rootStore);
        OnProductsChanged += (products) =>
        {
            products!.CollectionChanged += (sender, args) =>
            {
                rootStore.OnChanged?.Invoke(rootStore);
            };
        };
    }
}