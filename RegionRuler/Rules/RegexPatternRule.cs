using System.Text.RegularExpressions;

namespace RegionRuler.Rules;

public sealed class RegexPatternRule : IRegionRule
{
    public int Priority => 5;

    public bool CanHandle(RuleContext context)
        => !context.IsEmpty
           && context.HasCustomPatternConfig
           && !context.AllowedPatterns.IsDefaultOrEmpty;

    public RuleResult Evaluate(RuleContext context)
    {
        foreach (var pattern in context.AllowedPatterns)
        {
            try
            {
                if (pattern.IsMatch(context.RegionName))
                    return RuleResult.Valid;
            }
            catch (RegexMatchTimeoutException)
            {
                // Timeout
                continue;
            }
        }

        return RuleResult.Invalid(DiagnosticDescriptors.InvalidPattern, context.RegionName);
    }
}