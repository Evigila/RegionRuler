using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace RegionRuler;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RegionRulerAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticIdInvalidName = "RR1001";
    public const string DiagnosticIdInvalidPattern = "RR1002";
    public const string DiagnosticIdEmpty = "RR1003";

    // Allowed region name
    // 允许的Region名字
    private const string AllowedRegionName = "region_ruler.allowed_region_name";

    // Allowed Regex patterns
    // 允许的正则表达式
    private const string AllowedRegexPattern = "region_ruler.allowed_regex_pattern";

    // Allow empty names
    // 允许空名字
    private const string AllowEmpty = "region_ruler.allow_empty";

    // Case sensitivity
    // 区分大小写
    private const string CaseSensitive = "region_ruler.case_sensitive";

    // Regex timeout
    // 正则超时
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);

    private static readonly DiagnosticDescriptor RuleInvalidName =
        new(
            DiagnosticIdInvalidName,
            "Invalid #region name",
            "Region name '{0}' is not in the allowed names list",
            "Structure",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Region name must be in the configured allowed names list."
        );

    private static readonly DiagnosticDescriptor RuleInvalidPattern =
        new(
            DiagnosticIdInvalidPattern,
            "Invalid #region pattern",
            "Region name '{0}' does not match any allowed regex pattern",
            "Structure",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Region name must match at least one of the configured regex patterns."
        );

    private static readonly DiagnosticDescriptor RuleEmpty =
        new(
            DiagnosticIdEmpty,
            "Empty #region name",
            "Region name cannot be empty",
            "Structure",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Region must have a name when empty names are not allowed."
        );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(RuleInvalidName, RuleInvalidPattern, RuleEmpty);

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

        var (allowedRegions, hasAllowedRegionsConfig) = GetAllowedRegions(options);
        var (allowedPatterns, hasAllowedPatternsConfig) = GetAllowedPatterns(options);
        var caseSensitive = GetCaseSensitive(options);
        var allowEmpty = GetAllowEmpty(options);

        var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

        foreach (var trivia in root.DescendantTrivia())
        {
            if (trivia.GetStructure() is not RegionDirectiveTriviaSyntax region)
                continue;

            var regionText = trivia.ToString();
            var name = regionText
                .Substring("#region".Length)
                .Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                if (!allowEmpty)
                {
                    var diagnostic = Diagnostic.Create(
                        RuleEmpty,
                        region.GetLocation(),
                        "[empty]"
                    );
                    context.ReportDiagnostic(diagnostic);
                }
                continue;
            }

            bool matchedInAllowedNames = allowedRegions.Contains(name, comparer);
            bool matchedInPatterns = IsMatchedByPattern(name, allowedPatterns);

            // pass if any matched
            if (matchedInAllowedNames || matchedInPatterns)
                continue;

            // report all diagnostics if both not passed
            if (hasAllowedRegionsConfig && hasAllowedPatternsConfig)
            {
                var diagnostic1 = Diagnostic.Create(
                    RuleInvalidName,
                    region.GetLocation(),
                    name
                );
                context.ReportDiagnostic(diagnostic1);

                var diagnostic2 = Diagnostic.Create(
                    RuleInvalidPattern,
                    region.GetLocation(),
                    name
                );
                context.ReportDiagnostic(diagnostic2);
            }
            // only region name configured
            else if (hasAllowedRegionsConfig)
            {
                var diagnostic = Diagnostic.Create(
                    RuleInvalidName,
                    region.GetLocation(),
                    name
                );
                context.ReportDiagnostic(diagnostic);
            }
            // only regex pattern configured
            else if (hasAllowedPatternsConfig)
            {
                var diagnostic = Diagnostic.Create(
                    RuleInvalidPattern,
                    region.GetLocation(),
                    name
                );
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsMatchedByPattern(string name, ImmutableArray<Regex> patterns)
    {
        if (patterns.IsDefaultOrEmpty)
            return false;

        foreach (var pattern in patterns)
        {
            try
            {
                if (pattern.IsMatch(name))
                    return true;
            }
            catch (RegexMatchTimeoutException)
            {
                // Timeout
                continue;
            }
        }

        return false;
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