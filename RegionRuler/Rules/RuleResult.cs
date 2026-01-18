using Microsoft.CodeAnalysis;

namespace RegionRuler.Rules;

public sealed class RuleResult
{
    private RuleResult(bool isValid, DiagnosticDescriptor? descriptor, string? regionName)
    {
        IsValid = isValid;
        Descriptor = descriptor;
        RegionName = regionName;
    }

    public DiagnosticDescriptor? Descriptor { get; }
    public string? RegionName { get; }
    public bool IsValid { get; }

    public static readonly RuleResult Valid = new(true, null, null);

    public static RuleResult Invalid(DiagnosticDescriptor descriptor, string regionName)
        => new(false, descriptor, regionName);

    public Diagnostic? ToDiagnostic(Location location)
        => IsValid ? null : Diagnostic.Create(Descriptor!, location, RegionName ?? "[empty]");
}