; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
GQLEF002 | Usage | Warning | Use projection-based Resolve extension methods when accessing navigation properties
GQLEF003 | Usage | Error | Identity projection is not allowed in projection-based Resolve methods
GQLEF004 | Usage | Info | Suggests using 4-parameter filter syntax instead of explicit identity projection
GQLEF005 | Usage | Error | Prevents accessing non-key properties with 4-parameter filter syntax
GQLEF006 | Usage | Error | Prevents accessing non-key properties with explicit identity projection
