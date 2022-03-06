using System.Collections.Generic;
using System.Windows.Input;
using CommonServiceLocator;
using PropertyChanged.SourceGenerator;
using WpfApp.Commands;
using WpfApp.Models;
using WpfApp.Services;
using WpfApp.Stores;

namespace WpfApp.ViewModels;

public partial class DashboardWindowViewModel
{
    private readonly Store _store = ServiceLocator.Current.GetInstance<Store>();

    [Notify] private ICollection<Product>? _products;

    public LoadProductCommand LoadProductCommand { get; } = new();

    public DashboardWindowViewModel()
    {
        Products = _store.Products;
        
        _store.OnProductsChanged += products => Products = products;
    }
}