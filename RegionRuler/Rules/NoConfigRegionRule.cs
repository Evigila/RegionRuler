namespace RegionRuler.Rules;

public sealed class NoConfigRegionRule : IRegionRule
{
    public int Priority => 1;

    public bool CanHandle(RuleContext context)
        => !context.IsEmpty && !context.HasAnyCustomConfig;

    public RuleResult Evaluate(RuleContext context)
    {
        bool matchedInDefaultRules = context.AllowedRegions.Contains(context.RegionName, context.Comparer);

        return matchedInDefaultRules
            ? RuleResult.Valid
            : RuleResult.Invalid(DiagnosticDescriptors.NoConfig, context.RegionName);
    }
}