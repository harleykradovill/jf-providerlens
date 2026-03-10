using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.ProviderLens.Models;

/// <summary>
/// A provider entry in a TMDB category list.
/// </summary>
internal sealed class TmdbWatchProvider
{
    /// <summary>
    /// Gets the TMDB provider id.
    /// </summary>
    [JsonPropertyName("provider_id")]
    public int ProviderId { get; init; }

    /// <summary>
    /// Gets the display name of the provider.
    /// </summary>
    [JsonPropertyName("provider_name")]
    public string ProviderName { get; init; } = string.Empty;
}