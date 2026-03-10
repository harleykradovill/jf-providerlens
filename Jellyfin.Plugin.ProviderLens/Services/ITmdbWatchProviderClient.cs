using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.ProviderLens.Services;

/// <summary>
/// TMDB watch provider API client.
/// </summary>
internal interface ITmdbWatchProviderClient
{
    /// <summary>
    /// Gets provider ids and names for a movie in the selected country.
    /// </summary>
    /// <param name="tmdbId">TMDB id.</param>
    /// <param name="apiKey">TMDB API key.</param>
    /// <param name="country">2-letter ISO country code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Provider ids mapped to provider names.</returns>
    Task<IReadOnlyDictionary<string, string>> GetMovieProvidersAsync(
        string tmdbId,
        string apiKey,
        string country,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets provider ids and names for a tv series in the selected country.
    /// </summary>
    /// <param name="tmdbId">TMDB id.</param>
    /// <param name="apiKey">TMDB API key.</param>
    /// <param name="country">2-letter ISO country code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Provider ids mapped to provider names.</returns>
    Task<IReadOnlyDictionary<string, string>> GetSeriesProvidersAsync(
        string tmdbId,
        string apiKey,
        string country,
        CancellationToken cancellationToken);
}