using System.Collections.Immutable;

namespace RegionRuler.Rules;

public sealed class RuleEngine
{
    private readonly ImmutableArray<IRegionRule> _rules;

    public RuleEngine()
    {
        _rules = ImmutableArray.Create<IRegionRule>(
            new EmptyRegionRule(),
            new NoConfigRegionRule(),
            new AllowedNamesRule(),
            new RegexPatternRule()
        ).Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    public ImmutableArray<RuleResult> Evaluate(RuleContext context)
    {
        var results = ImmutableArray.CreateBuilder<RuleResult>();
        bool hasValidResult = false;

        foreach (var rule in _rules)
        {
            if (!rule.CanHandle(context))
                continue;

            var result = rule.Evaluate(context);

            // IsValid if any rule passes
            if (result.IsValid)
            {
                hasValidResult = true;
                break; // break early since we found a valid result
            }

            // collect invalid results
            results.Add(result);
        }

        // return emptu if any rule valid
        return hasValidResult ? ImmutableArray<RuleResult>.Empty : results.ToImmutable();
    }
}