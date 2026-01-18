using Microsoft.CodeAnalysis;

namespace RegionRuler;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor NoConfig = new(
        "RR1999",
        "No .editorconfig configuration",
        "Region name '{0}' does not match default rules. Configure .editorconfig to customize allowed region names",
        "Structure",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "No custom configuration found in .editorconfig. Region name must match default naming rules or configure region_ruler.allowed_region_name or region_ruler.allowed_regex_pattern in .editorconfig."
    );

    public static readonly DiagnosticDescriptor InvalidName = new(
        "RR2001",
        "Invalid #region name",
        "Region name '{0}' is not in the allowed names list",
        "Structure",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Region name must be in the configured allowed names list."
    );

    public static readonly DiagnosticDescriptor InvalidPattern = new(
        "RR2002",
        "Invalid #region pattern",
        "Region name '{0}' does not match any allowed regex pattern",
        "Structure",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Region name must match at least one of the configured regex patterns."
    );

    public static readonly DiagnosticDescriptor Empty = new(
        "RR2003",
        "Empty #region name",
        "Region name cannot be empty",
        "Structure",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Region must have a name when empty names are not allowed."
    );
}