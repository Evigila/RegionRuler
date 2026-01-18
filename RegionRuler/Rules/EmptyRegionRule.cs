namespace RegionRuler.Rules;

public sealed class EmptyRegionRule : IRegionRule
{
    // Greatest priority to handle empty regions first
    public int Priority => 0;

    public bool CanHandle(RuleContext context)
        => context.IsEmpty && !context.AllowEmpty;

    public RuleResult Evaluate(RuleContext context)
        => RuleResult.Invalid(DiagnosticDescriptors.Empty, "[empty]");
}