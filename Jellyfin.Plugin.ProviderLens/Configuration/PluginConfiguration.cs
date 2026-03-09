using System.Collections.ObjectModel;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.ProviderLens.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        TmdbApiKey = string.Empty;
        SelectedProviders = new Collection<string>();
        MonitoredLibraryIds = new Collection<string>();
    }

    /// <summary>
    /// Gets or sets the TMDB API key.
    /// </summary>
    public string TmdbApiKey { get; set; }

    /// <summary>
    /// Gets the selected streaming provider ids.
    /// </summary>
    public Collection<string> SelectedProviders { get; }

    /// <summary>
    /// Gets the Jellyfin library ids to monitor.
    /// </summary>
    public Collection<string> MonitoredLibraryIds { get; }
}