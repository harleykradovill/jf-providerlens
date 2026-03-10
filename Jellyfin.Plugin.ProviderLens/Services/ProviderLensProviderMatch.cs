namespace Jellyfin.Plugin.ProviderLens.Services;

/// <summary>
/// Matched provider.
/// </summary>
/// <param name="ProviderId">Provider id.</param>
/// <param name="ProviderName">Provider display name.</param>
internal sealed record ProviderLensProviderMatch(string ProviderId, string ProviderName);