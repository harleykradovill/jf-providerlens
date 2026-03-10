using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.ProviderLens.Services;

/// <summary>
/// Scans monitored libraries and finds items available on selected providers.
/// </summary>
public interface IProviderLensScanService
{
    /// <summary>
    /// Runs a provider scan using current plugin settings.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matched items and providers.</returns>
    Task<IReadOnlyList<ProviderLensMatch>> ScanAsync(CancellationToken cancellationToken);
}