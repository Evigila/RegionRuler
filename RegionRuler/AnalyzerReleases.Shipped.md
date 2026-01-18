; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.2

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RR1999 | Structure | Warning | No .editorconfig configuration
RR2001 | Structure | Warning | Invalid #region name
RR2002 | Structure | Warning | Invalid #region pattern
RR2003 | Structure | Warning | Empty #region name

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RR1001 | Structure | Warning | Invalid #region name - Replaced by RR2001
RR1002 | Structure | Warning | Invalid #region pattern - Replaced by RR2002
RR1003 | Structure | Warning | Empty #region name - Replaced by RR2003

## Release 1.1

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RR1002 | Structure | Warning | Invalid #region pattern - Region name does not match any allowed regex pattern
RR1003 | Structure | Warning | Empty #region name - Region name cannot be empty

## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
RR1001 | Structure | Warning | Invalid #region name - Enforces region naming conventions