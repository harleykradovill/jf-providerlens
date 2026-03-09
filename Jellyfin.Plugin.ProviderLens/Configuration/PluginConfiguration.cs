using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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
        Country = string.Empty;
        SelectedProviders = new Collection<string>();
        MonitoredLibraryIds = new Collection<string>();
    }

    /// <summary>
    /// Gets or sets the TMDB API key.
    /// </summary>
    public string TmdbApiKey { get; set; }

    /// <summary>
    /// Gets or sets the 2-letter ISO country code used to filter TMDB provider results.
    /// </summary>
    public string Country { get; set; }

    /// <summary>
    /// Gets or sets the selected streaming provider ids.
    /// </summary>
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Must be settable for plugin configuration deserialization.")]
    public Collection<string> SelectedProviders { get; set; }

    /// <summary>
    /// Gets or sets the Jellyfin library ids to monitor.
    /// </summary>
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Must be settable for plugin configuration deserialization.")]
    public Collection<string> MonitoredLibraryIds { get; set; }
}