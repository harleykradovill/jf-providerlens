using Jellyfin.Plugin.ProviderLens.ScheduledTasks;
using Jellyfin.Plugin.ProviderLens.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.ProviderLens.Extensions;

/// <summary>
/// Registers ProviderLens services in Jellyfin DI.
/// </summary>
public sealed class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(
        IServiceCollection serviceCollection,
        IServerApplicationHost applicationHost)
    {
        serviceCollection.AddHttpClient<ITmdbWatchProviderClient, TmdbWatchProviderClient>();
        serviceCollection.AddSingleton<IProviderLensResultStore, ProviderLensResultStore>();
        serviceCollection.AddSingleton<IProviderLensScanService, ProviderLensScanService>();
        serviceCollection.AddSingleton<IScheduledTask, ProviderLensSyncTask>();
    }
}