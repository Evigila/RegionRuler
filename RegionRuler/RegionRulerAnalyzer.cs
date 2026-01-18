using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using RegionRuler.Rules;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace RegionRuler;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RegionRulerAnalyzer : DiagnosticAnalyzer
{
    private const string AllowedRegionName = "region_ruler.allowed_region_name";
    private const string AllowedRegexPattern = "region_ruler.allowed_regex_pattern";
    private const string AllowEmpty = "region_ruler.allow_empty";
    private const string CaseSensitive = "region_ruler.case_sensitive";

    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);
    private static readonly RuleEngine RuleEngine = new();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(
            DiagnosticDescriptors.NoConfig,
            DiagnosticDescriptors.InvalidName,
            DiagnosticDescriptors.InvalidPattern,
            DiagnosticDescriptors.Empty
        );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeTree);
    }

    private static void AnalyzeTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);

        // build configuration
        var config = BuildConfiguration(options);

        // scan all #region directives
        foreach (var trivia in root.DescendantTrivia())
        {
            if (trivia.GetStructure() is not RegionDirectiveTriviaSyntax region)
                continue;

            var regionName = ExtractRegionName(trivia);

            // build context
            var ruleContext = new RuleContext(
                regionName,
                config.HasCustomRegionConfig,
                config.HasCustomPatternConfig,
                config.AllowedRegions,
                config.AllowedPatterns,
                config.Comparer,
                config.AllowEmpty,
                region.GetLocation());

            // evaluate rules
            var results = RuleEngine.Evaluate(ruleContext);

            // report diagnostics
            foreach (var result in results)
            {
                var diagnostic = result.ToDiagnostic(region.GetLocation());
                if (diagnostic != null)
                {
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static string ExtractRegionName(SyntaxTrivia trivia)
    {
        var regionText = trivia.ToString();
        return regionText.Substring("#region".Length).Trim();
    }

    private static Configuration BuildConfiguration(AnalyzerConfigOptions options)
    {
        var (allowedRegions, hasRegionConfig) = GetAllowedRegions(options);
        var (allowedPatterns, hasPatternConfig) = GetAllowedPatterns(options);
        var caseSensitive = GetCaseSensitive(options);
        var allowEmpty = GetAllowEmpty(options);
        var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

        return new Configuration(
            allowedRegions,
            allowedPatterns,
            hasRegionConfig,
            hasPatternConfig,
            comparer,
            allowEmpty);
    }

    // configuration
    private sealed class Configuration(
        ImmutableHashSet<string> allowedRegions,
        ImmutableArray<Regex> allowedPatterns,
        bool hasCustomRegionConfig,
        bool hasCustomPatternConfig,
        StringComparer comparer,
        bool allowEmpty)
    {
        public ImmutableHashSet<string> AllowedRegions { get; } = allowedRegions;
        public ImmutableArray<Regex> AllowedPatterns { get; } = allowedPatterns;
        public bool HasCustomRegionConfig { get; } = hasCustomRegionConfig;
        public bool HasCustomPatternConfig { get; } = hasCustomPatternConfig;
        public StringComparer Comparer { get; } = comparer;
        public bool AllowEmpty { get; } = allowEmpty;
    }

    private static (ImmutableHashSet<string> regions, bool hasConfig) GetAllowedRegions(AnalyzerConfigOptions options)
    {
        if (options.TryGetValue(AllowedRegionName, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            var regions = value
                .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r));

            return (ImmutableHashSet.CreateRange(regions), true);
        }

        return (DefaultRegions, false);
    }

    private static (ImmutableArray<Regex> patterns, bool hasConfig) GetAllowedPatterns(AnalyzerConfigOptions options)
    {
        if (!options.TryGetValue(AllowedRegexPattern, out var value) || string.IsNullOrWhiteSpace(value))
            return (ImmutableArray<Regex>.Empty, false);

        var caseSensitive = GetCaseSensitive(options);
        var regexOptions = RegexOptions.Compiled | (caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);

        var patterns = value
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p =>
            {
                try
                {
                    return new Regex(p, regexOptions, RegexTimeout);
                }
                catch (ArgumentException)
                {
                    // Invalid regex
                    return null;
                }
            })
            .Where(r => r != null)
            .Cast<Regex>();

        return (ImmutableArray.CreateRange(patterns), true);
    }

    private static bool GetCaseSensitive(AnalyzerConfigOptions options)
    {
        if (options.TryGetValue(CaseSensitive, out var value)
            && bool.TryParse(value, out var result))
        {
            return result;
        }

        return false;
    }

    private static bool GetAllowEmpty(AnalyzerConfigOptions options)
    {
        if (options.TryGetValue(AllowEmpty, out var value)
            && bool.TryParse(value, out var result))
        {
            return result;
        }

        return true;
    }

    private static readonly ImmutableHashSet<string> DefaultRegions =
        ImmutableHashSet.Create(
            StringComparer.OrdinalIgnoreCase,

            "PUBLIC_MEMBERS",
            "PUBLIC_PROPERTIES",
            "PUBLIC_FIELDS",

            "INTERNAL_MEMBERS",
            "INTERNAL_PROPERTIES",
            "INTERNAL_FIELDS",

            "PRIVATE_MEMBERS",
            "PRIVATE_PROPERTIES",
            "PRIVATE_FIELDS",

            "PROTECTED_MEMBERS",
            "PROTECTED_PROPERTIES",
            "PROTECTED_FIELDS",

            "CONST_MEMBERS",
            "CONST_PROPERTIES",
            "CONST_FIELDS",

            "STATIC_MEMBERS",
            "STATIC_PROPERTIES",
            "STATIC_FIELDS",

            "CONSTRUCTOR",
            "DESTRUCTOR",

            "INITIALIZATION",

            "PUBLIC_METHODS",
            "INTERNAL_METHODS",
            "PRIVATE_METHODS",
            "PROTECTED_METHODS",
            "OVERRIDE_METHODS",
            "ABSTRACT_METHODS",
            "STATIC_METHODS",

            "EVENT_CALLBACKS",

            "MAIN",
            "HELPERS",
            "UTILITIES"
        );
}