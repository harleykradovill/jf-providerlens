using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.ProviderLens.Models;

/// <summary>
/// Root TMDB watch provider response.
/// </summary>
internal sealed class TmdbWatchProviderResponse
{
    /// <summary>
    /// Gets country-keyed provider data (for example "US", "GB").
    /// </summary>
    [JsonPropertyName("results")]
    public Dictionary<string, TmdbCountryWatchProviders> Results { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}