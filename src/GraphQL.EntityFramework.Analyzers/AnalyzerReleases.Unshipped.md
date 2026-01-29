; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
GQLEF002 | Usage | Warning | Use projection-based Resolve extension methods when accessing navigation properties
GQLEF003 | Usage | Error | Identity projection is not allowed in projection-based Resolve methods
GQLEF004 | Usage | Info | Suggests using simplified filter API when identity projection only accesses keys
GQLEF005 | Usage | Error | Prevents accessing non-key properties with simplified filter API
GQLEF006 | Usage | Error | Prevents accessing non-key properties with identity projection
