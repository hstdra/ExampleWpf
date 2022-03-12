using System;
using CommonServiceLocator;
using WpfGenerator;

namespace WpfApp.Stores;

[ChangedListener]
public partial class RootStore
{
    public ProductStore ProductStore => ServiceLocator.Current.GetInstance<ProductStore>();
}

public class SerializableRootStore
{
    public ProductStore ProductStore { get; set; }
}