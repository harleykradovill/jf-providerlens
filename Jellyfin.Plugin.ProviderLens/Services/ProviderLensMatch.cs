using System.Collections.Generic;

namespace Jellyfin.Plugin.ProviderLens.Services;

/// <summary>
/// Matched media item and its selected-country providers.
/// </summary>
/// <param name="JellyfinItemId">Jellyfin item identifier.</param>
/// <param name="Name">Item title.</param>
/// <param name="TmdbId">TMDB item identifier.</param>
/// <param name="Country">Country code used for provider filtering.</param>
/// <param name="Providers">Matched providers.</param>
public sealed record ProviderLensMatch(
    string JellyfinItemId,
    string Name,
    string TmdbId,
    string Country,
    IReadOnlyList<ProviderLensProviderMatch> Providers);