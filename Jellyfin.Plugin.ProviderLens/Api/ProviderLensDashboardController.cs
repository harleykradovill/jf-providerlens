using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.ProviderLens.Services;
using MediaBrowser.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.ProviderLens.Api;

/// <summary>
/// Provides dashboard data for the ProviderLens configuration page.
/// </summary>
[ApiController]
[Authorize(Policy = Policies.RequiresElevation)]
[Produces(MediaTypeNames.Application.Json)]
public sealed class ProviderLensDashboardController : ControllerBase
{
    private readonly IProviderLensResultStore _resultStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderLensDashboardController"/> class.
    /// </summary>
    /// <param name="resultStore">Dashboard snapshot result store.</param>
    public ProviderLensDashboardController(IProviderLensResultStore resultStore)
    {
        _resultStore = resultStore;
    }

    /// <summary>
    /// Gets the latest ProviderLens dashboard snapshot.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest dashboard snapshot payload.</returns>
    [HttpGet("ProviderLens/Dashboard")]
    [ProducesResponseType(typeof(ProviderLensDashboardSnapshot), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProviderLensDashboardSnapshot>> GetDashboard(CancellationToken cancellationToken)
    {
        var snapshot = await _resultStore.GetSnapshotAsync(cancellationToken).ConfigureAwait(false);
        return Ok(snapshot);
    }
}