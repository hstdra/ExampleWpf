using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommonServiceLocator;
using WpfApp.Models;
using WpfGenerator;

namespace WpfApp.Stores;

[ChangedListener]
public partial class TestStore
{
    private string? _name;
    private short _age;
}