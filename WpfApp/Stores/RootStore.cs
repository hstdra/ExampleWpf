using System;
using CommonServiceLocator;

namespace WpfApp.Stores;

public class RootStore
{
    public Action<RootStore>? OnChanged;

    public ProductStore ProductStore => ServiceLocator.Current.GetInstance<ProductStore>();
}

public class SerializableRootStore
{
    public ProductStore ProductStore { get; set; }
}