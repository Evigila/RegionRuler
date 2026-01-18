; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.1

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RR1002 | Structure | Warning | Invalid #region pattern - Region name does not match any allowed regex pattern
RR1003 | Structure | Warning | Empty #region name - Region name cannot be empty

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
RR1001 | Structure | Warning | Structure | Warning | Updated message to be more specific - now reports when region name is not in the allowed names list

## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RR1001 | Structure | Warning | Invalid #region name - Enforces region naming conventions
