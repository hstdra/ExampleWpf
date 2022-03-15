using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Binaron.Serializer;
using Microsoft.Extensions.Hosting;
using WpfApp.Stores;

namespace WpfApp.Bootstrap;

public class StoreManager : IHostedService
{
    private static readonly string FilePath = $"{AppDomain.CurrentDomain.BaseDirectory}/data.json";

    private readonly RootStore _rootStore;

    public StoreManager(RootStore rootStore)
    {
        _rootStore = rootStore;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _rootStore.OnChanged += (state) =>
        {
            try
            {
                File.WriteAllText(FilePath, JsonSerializer.Serialize(state));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        };
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public static SerializableRootStore GetSaveRootStore()
    {
        try
        {
            return JsonSerializer.Deserialize<SerializableRootStore>(File.ReadAllText(FilePath)) ?? new();
        }
        catch (Exception)
        {
            return new();
        }
    }
}