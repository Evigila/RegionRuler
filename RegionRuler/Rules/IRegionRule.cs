namespace RegionRuler.Rules;

public interface IRegionRule
{
    // Lower values indicate higher priority
    int Priority { get; }

    // Determines if this rule can handle the given context
    bool CanHandle(RuleContext context);

    // Evaluates the rule against the given context and returns the result
    RuleResult Evaluate(RuleContext context);
}