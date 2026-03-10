using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ProviderLens.Services;

/// <summary>
/// JSON file-backed result store.
/// </summary>
internal sealed class ProviderLensResultStore : IProviderLensResultStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly string _outputPath;
    private readonly ILogger<ProviderLensResultStore> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderLensResultStore"/> class.
    /// </summary>
    /// <param name="applicationPaths">Jellyfin application paths.</param>
    /// <param name="logger">Logger.</param>
    public ProviderLensResultStore(
        IApplicationPaths applicationPaths,
        ILogger<ProviderLensResultStore> logger)
    {
        _logger = logger;

        var pluginDataDirectory = Path.Combine(
            applicationPaths.PluginConfigurationsPath,
            "ProviderLens");

        Directory.CreateDirectory(pluginDataDirectory);
        _outputPath = Path.Combine(pluginDataDirectory, "dashboard-data.json");
    }

    /// <inheritdoc />
    public async Task SaveAsync(IReadOnlyList<ProviderLensMatch> matches, CancellationToken cancellationToken)
    {
        var payload = new ProviderLensDashboardSnapshot(
            DateTimeOffset.UtcNow,
            matches);

        var json = JsonSerializer.Serialize(payload, SerializerOptions);
        await File.WriteAllTextAsync(_outputPath, json, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "ProviderLens wrote {MatchCount} matches to {OutputPath}.",
            matches.Count,
            _outputPath);
    }
}