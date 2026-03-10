using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.ProviderLens.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ProviderLens.Services;

/// <summary>
/// Default TMDB watch provider client implementation.
/// </summary>
internal sealed class TmdbWatchProviderClient : ITmdbWatchProviderClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<TmdbWatchProviderClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TmdbWatchProviderClient"/> class.
    /// </summary>
    /// <param name="httpClient">Configured HTTP client.</param>
    /// <param name="logger">Logger.</param>
    public TmdbWatchProviderClient(
        HttpClient httpClient,
        ILogger<TmdbWatchProviderClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress ??= new Uri("https://api.themoviedb.org/", UriKind.Absolute);
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<string, string>> GetMovieProvidersAsync(
        string tmdbId,
        string apiKey,
        string country,
        CancellationToken cancellationToken)
        => GetProvidersAsync("movie", tmdbId, apiKey, country, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<string, string>> GetSeriesProvidersAsync(
        string tmdbId,
        string apiKey,
        string country,
        CancellationToken cancellationToken)
        => GetProvidersAsync("tv", tmdbId, apiKey, country, cancellationToken);

    private async Task<IReadOnlyDictionary<string, string>> GetProvidersAsync(
        string mediaType,
        string tmdbId,
        string apiKey,
        string country,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tmdbId) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(country))
        {
            return new Dictionary<string, string>();
        }

        var normalizedCountry = country.Trim().ToUpperInvariant();
        var requestPath = string.Format(
            CultureInfo.InvariantCulture,
            "3/{0}/{1}/watch/providers?api_key={2}",
            mediaType,
            Uri.EscapeDataString(tmdbId.Trim()),
            Uri.EscapeDataString(apiKey.Trim()));

        using var response = await SendWithRateLimitRetryAsync(requestPath, cancellationToken).ConfigureAwait(false);
        if (response is null)
        {
            return new Dictionary<string, string>();
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "TMDB request failed for {MediaType} {TmdbId}. Status: {StatusCode}",
                mediaType,
                tmdbId,
                (int)response.StatusCode);

            return new Dictionary<string, string>();
        }

        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var payload = await JsonSerializer.DeserializeAsync<TmdbWatchProviderResponse>(
            responseStream,
            SerializerOptions,
            cancellationToken).ConfigureAwait(false);

        if (payload?.Results is null ||
            !payload.Results.TryGetValue(normalizedCountry, out var countryPayload) ||
            countryPayload is null)
        {
            return new Dictionary<string, string>();
        }

        return ExtractProviders(countryPayload);
    }

    private async Task<HttpResponseMessage?> SendWithRateLimitRetryAsync(
        string requestPath,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(requestPath, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.TooManyRequests)
        {
            return response;
        }

        var retryDelay = response.Headers.RetryAfter?.Delta;
        if (!retryDelay.HasValue || retryDelay.Value <= TimeSpan.Zero)
        {
            return response;
        }

        response.Dispose();

        _logger.LogInformation(
            "TMDB rate limit hit. Retrying once in {DelaySeconds} seconds.",
            retryDelay.Value.TotalSeconds);

        await Task.Delay(retryDelay.Value, cancellationToken).ConfigureAwait(false);
        return await _httpClient.GetAsync(requestPath, cancellationToken).ConfigureAwait(false);
    }

    private static Dictionary<string, string> ExtractProviders(TmdbCountryWatchProviders countryPayload)
    {
        var providers = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var bucket in countryPayload.ProviderBuckets)
        {
            if (string.Equals(bucket.Key, "link", StringComparison.OrdinalIgnoreCase) ||
                bucket.Value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var providerElement in bucket.Value.EnumerateArray())
            {
                if (!providerElement.TryGetProperty("provider_id", out var idElement) ||
                    !idElement.TryGetInt32(out var providerId))
                {
                    continue;
                }

                var providerIdText = providerId.ToString(CultureInfo.InvariantCulture);
                var providerName = providerIdText;

                if (providerElement.TryGetProperty("provider_name", out var nameElement))
                {
                    var parsedName = nameElement.GetString();
                    if (!string.IsNullOrWhiteSpace(parsedName))
                    {
                        providerName = parsedName;
                    }
                }

                providers[providerIdText] = providerName;
            }
        }

        return providers;
    }
}