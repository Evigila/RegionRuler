using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace RegionRuler.Rules;

public sealed class RuleContext(
    string regionName,
    bool hasCustomRegionConfig,
    bool hasCustomPatternConfig,
    ImmutableHashSet<string> allowedRegions,
    ImmutableArray<Regex> allowedPatterns,
    StringComparer comparer,
    bool allowEmpty,
    Location location)
{
    public string RegionName { get; } = regionName;
    public bool HasCustomRegionConfig { get; } = hasCustomRegionConfig;
    public bool HasCustomPatternConfig { get; } = hasCustomPatternConfig;
    public ImmutableHashSet<string> AllowedRegions { get; } = allowedRegions;
    public ImmutableArray<Regex> AllowedPatterns { get; } = allowedPatterns;
    public StringComparer Comparer { get; } = comparer;
    public bool AllowEmpty { get; } = allowEmpty;
    public Location Location { get; } = location;

    public bool IsEmpty => string.IsNullOrWhiteSpace(RegionName);
    public bool HasAnyCustomConfig => HasCustomRegionConfig || HasCustomPatternConfig;
}