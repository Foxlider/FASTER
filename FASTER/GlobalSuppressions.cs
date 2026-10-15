// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1075")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1643:Strings should not be concatenated using '+' in a loop", Justification = "False positive : using interpolation")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3010:Static fields should not be updated in constructors", Justification = "Needed", Scope = "member", Target = "~M:FASTER.MainWindow.#ctor")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Wontfix : instance data is used")]
