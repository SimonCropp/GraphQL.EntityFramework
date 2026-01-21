; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
GQLEF002 | Usage | Warning | Use projection-based Resolve extension methods when accessing navigation properties
GQLEF003 | Usage | Error | Identity projection is not allowed in projection-based Resolve methods
GQLEF004 | Usage | Error | Projection to scalar types is not allowed in projection-based Resolve methods
