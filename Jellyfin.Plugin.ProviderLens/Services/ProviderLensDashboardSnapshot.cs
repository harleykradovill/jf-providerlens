using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.ProviderLens.Services;

/// <summary>
/// Stored dashboard snapshot payload.
/// </summary>
/// <param name="UpdatedUtc">Snapshot update time in UTC.</param>
/// <param name="Matches">Persisted matches.</param>
internal sealed record ProviderLensDashboardSnapshot(
    DateTimeOffset UpdatedUtc,
    IReadOnlyList<ProviderLensMatch> Matches);