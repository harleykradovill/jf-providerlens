using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.ProviderLens.Services;

/// <summary>
/// Persists and loads the latest ProviderLens scan result for dashboard reads.
/// </summary>
public interface IProviderLensResultStore
{
    /// <summary>
    /// Saves scan matches to disk.
    /// </summary>
    /// <param name="matches">Matches to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the write operation finishes.</returns>
    Task SaveAsync(IReadOnlyList<ProviderLensMatch> matches, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the latest dashboard snapshot from disk.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest snapshot, or an empty snapshot when none exists.</returns>
    Task<ProviderLensDashboardSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);
}