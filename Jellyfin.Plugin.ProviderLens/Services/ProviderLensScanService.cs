using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.ProviderLens.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ProviderLens.Services;

/// <summary>
/// Default scan service implementation.
/// </summary>
internal sealed class ProviderLensScanService : IProviderLensScanService
{
    private readonly ILibraryManager _libraryManager;
    private readonly ITmdbWatchProviderClient _tmdbClient;
    private readonly ILogger<ProviderLensScanService> _logger;
    private readonly IProviderLensResultStore _resultStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderLensScanService"/> class.
    /// </summary>
    /// <param name="libraryManager">Jellyfin library manager.</param>
    /// <param name="tmdbClient">TMDB provider client.</param>
    /// <param name="resultStore">Result persistence store.</param>
    /// <param name="logger">Logger.</param>
    public ProviderLensScanService(
        ILibraryManager libraryManager,
        ITmdbWatchProviderClient tmdbClient,
        IProviderLensResultStore resultStore,
        ILogger<ProviderLensScanService> logger)
    {
        _libraryManager = libraryManager;
        _tmdbClient = tmdbClient;
        _resultStore = resultStore;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProviderLensMatch>> ScanAsync(
    CancellationToken cancellationToken,
    IProgress<double>? progress = null)
    {
        var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
        var selectedProviders = new HashSet<string>(
            config.SelectedProviders ?? new Collection<string>(),
            StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(config.TmdbApiKey) ||
            string.IsNullOrWhiteSpace(config.Country) ||
            selectedProviders.Count == 0 ||
            config.MonitoredLibraryIds.Count == 0)
        {
            _logger.LogInformation("ProviderLens scan skipped due to incomplete configuration.");
            progress?.Report(100);
            return Array.Empty<ProviderLensMatch>();
        }

        var matches = new List<ProviderLensMatch>();
        var country = config.Country.Trim().ToUpperInvariant();
        var apiKey = config.TmdbApiKey.Trim();

        var scanItems = new List<BaseItem>();
        foreach (var libraryIdText in config.MonitoredLibraryIds)
        {
            if (!Guid.TryParse(libraryIdText, out var libraryId))
            {
                _logger.LogWarning("ProviderLens ignored invalid library id '{LibraryIdText}'.", libraryIdText);
                continue;
            }

            var query = new InternalItemsQuery
            {
                AncestorIds = [libraryId],
                IncludeItemTypes = [BaseItemKind.Movie, BaseItemKind.Series]
            };

            scanItems.AddRange(_libraryManager.QueryItems(query).Items);
        }

        var totalItems = scanItems.Count;
        var processedItems = 0;
        var lastReportedPercent = -1;

        void ReportProgress()
        {
            if (progress is null)
            {
                return;
            }

            var percent = totalItems == 0
                ? 100
                : (int)Math.Floor((processedItems * 100d) / totalItems);

            if (percent > 99 && processedItems < totalItems)
            {
                percent = 99;
            }

            if (percent == lastReportedPercent)
            {
                return;
            }

            lastReportedPercent = percent;
            progress.Report(percent);
        }

        ReportProgress();

        foreach (var item in scanItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (item is not Movie && item is not Series)
                {
                    continue;
                }

                var tmdbId = item.GetProviderId(MetadataProvider.Tmdb);
                if (string.IsNullOrWhiteSpace(tmdbId))
                {
                    continue;
                }

                IReadOnlyDictionary<string, string> providers =
                    item is Movie
                        ? await _tmdbClient.GetMovieProvidersAsync(tmdbId, apiKey, country, cancellationToken).ConfigureAwait(false)
                        : await _tmdbClient.GetSeriesProvidersAsync(tmdbId, apiKey, country, cancellationToken).ConfigureAwait(false);

                if (providers.Count == 0)
                {
                    continue;
                }

                var matchedProviders = providers.Keys
                    .Where(selectedProviders.Contains)
                    .Select(id => new ProviderLensProviderMatch(id, providers[id]))
                    .ToArray();

                if (matchedProviders.Length == 0)
                {
                    continue;
                }

                matches.Add(new ProviderLensMatch(
                    item.Id.ToString("N", CultureInfo.InvariantCulture),
                    item.Name,
                    tmdbId,
                    country,
                    matchedProviders));
            }
            finally
            {
                processedItems++;
                ReportProgress();
            }
        }

        await _resultStore.SaveAsync(matches, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("ProviderLens scan completed with {MatchCount} matches.", matches.Count);

        progress?.Report(100);
        return matches;
    }
}