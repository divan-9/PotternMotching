; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
PM0001 | PotternMotching.SourceGen | Error | Type must be a record
PM0002 | PotternMotching.SourceGen | Error | Inheritance is not supported
PM0003 | PotternMotching.SourceGen | Error | No primary constructor found
PM0004 | PotternMotching.SourceGen | Warning | Nested type pattern not found
PM0005 | PotternMotching.SourceGen | Error | Union type must be partial
PM0006 | PotternMotching.SourceGen | Error | Union type must have variant types
PM0007 | PotternMotching.SourceGen | Error | External target type could not be resolved
PM0008 | PotternMotching.SourceGen | Error | External target type must be a class or record
PM0009 | PotternMotching.SourceGen | Error | Generated pattern name collision
PM0010 | PotternMotching.SourceGen | Error | Unsupported external target type
PM9999 | PotternMotching.SourceGen | Warning | Debug property type detected
