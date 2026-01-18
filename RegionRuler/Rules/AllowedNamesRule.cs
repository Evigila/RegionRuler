namespace RegionRuler.Rules;

public sealed class AllowedNamesRule : IRegionRule
{
    public int Priority => 2;

    public bool CanHandle(RuleContext context)
        => !context.IsEmpty
           && context.HasCustomRegionConfig
           && !context.AllowedRegions.IsEmpty;

    public RuleResult Evaluate(RuleContext context)
    {
        bool matched = context.AllowedRegions.Contains(context.RegionName, context.Comparer);

        return matched
            ? RuleResult.Valid
            : RuleResult.Invalid(DiagnosticDescriptors.InvalidName, context.RegionName);
    }
}