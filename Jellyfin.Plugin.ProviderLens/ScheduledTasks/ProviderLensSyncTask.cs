using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.ProviderLens.Services;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ProviderLens.ScheduledTasks;

/// <summary>
/// Scheduled task that runs ProviderLens provider matching.
/// </summary>
public class ProviderLensSyncTask : IScheduledTask
{
    private readonly IProviderLensScanService _scanService;
    private readonly ILogger<ProviderLensSyncTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderLensSyncTask"/> class.
    /// </summary>
    /// <param name="scanService">ProviderLens scan service.</param>
    /// <param name="logger">Logger.</param>
    public ProviderLensSyncTask(
        IProviderLensScanService scanService,
        ILogger<ProviderLensSyncTask> logger)
    {
        _scanService = scanService;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "ProviderLens Sync";

    /// <inheritdoc />
    public string Key => "ProviderLensSync";

    /// <inheritdoc />
    public string Description => "Fetches TMDB watch providers and stores country-filtered matches.";

    /// <inheritdoc />
    public string Category => "ProviderLens";

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var matches = await _scanService.ScanAsync(cancellationToken, progress).ConfigureAwait(false);
        _logger.LogInformation("ProviderLens sync task finished with {MatchCount} matches.", matches.Count);
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return
        [
            new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerInterval,
                IntervalTicks = TimeSpan.FromHours(24).Ticks
            }
        ];
    }
}