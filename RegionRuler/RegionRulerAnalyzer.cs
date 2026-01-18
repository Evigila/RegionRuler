using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace RegionRuler;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RegionRulerAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "REGION_RULE_001";

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

    private static readonly DiagnosticDescriptor Rule =
        new(
            DiagnosticId,
            "Invalid #region name",
            "Region name '{0}' is not allowed!",
            "Structure",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Region names should follow the configured naming rules."
        );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

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

        var allowedRegions = GetAllowedRegions(options);
        var allowedPatterns = GetAllowedPatterns(options);
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
                        Rule,
                        region.GetLocation(),
                        "[empty]"
                    );
                    context.ReportDiagnostic(diagnostic);
                }
                continue;
            }

            if (allowedRegions.Contains(name, comparer))
                continue;

            if (IsMatchedByPattern(name, allowedPatterns))
                continue;

            var reportDiagnostic = Diagnostic.Create(
                Rule,
                region.GetLocation(),
                name
            );

            context.ReportDiagnostic(reportDiagnostic);
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

    private static ImmutableHashSet<string> GetAllowedRegions(AnalyzerConfigOptions options)
    {
        if (options.TryGetValue(AllowedRegionName, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            var regions = value
                .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r));

            return ImmutableHashSet.CreateRange(regions);
        }

        return DefaultRegions;
    }

    private static ImmutableArray<Regex> GetAllowedPatterns(AnalyzerConfigOptions options)
    {
        if (!options.TryGetValue(AllowedRegexPattern, out var value) || string.IsNullOrWhiteSpace(value))
            return ImmutableArray<Regex>.Empty;

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

        return ImmutableArray.CreateRange(patterns);
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