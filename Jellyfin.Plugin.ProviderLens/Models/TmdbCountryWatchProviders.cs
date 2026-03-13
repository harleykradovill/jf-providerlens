using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.ProviderLens.Models;

/// <summary>
/// TMDB provider data for a single country.
/// </summary>
internal sealed class TmdbCountryWatchProviders
{
    /// <summary>
    /// Gets provider category payloads keyed by category name
    /// (for example "flatrate", "rent", "buy", "free", "ads").
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> ProviderBuckets { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);
}