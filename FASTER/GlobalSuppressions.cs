// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1075")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug", "S4143:Collection elements should not be replaced unconditionally", Justification = "False positive", Scope = "member", Target = "~M:FASTER.ServerProfile.WriteConfigFiles(System.String)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Vulnerability", "S2278:Neither DES (Data Encryption Standard) nor DESede (3DES) should be used", Justification = "Wontfix", Scope = "member", Target = "~F:FASTER.Models.Encryption.TripleDes")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null", Justification = "Supposed to be null")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1643:Strings should not be concatenated using '+' in a loop", Justification = "False positive : using interpolation")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Might cause errors", Scope = "member", Target = "~F:FASTER.ServerStatus.processes")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Might cause errors", Scope = "member", Target = "~F:FASTER.ServerStatus.td")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3010:Static fields should not be updated in constructors", Justification = "Needed", Scope = "member", Target = "~M:FASTER.MainWindow.#ctor")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Wontfix : instance data is used")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "Serialization handled manually")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Bug", "S2930:\"IDisposables\" should be disposed", Justification = "False Positive : client is in using statement", Scope = "member", Target = "~M:FASTER.MainWindow.InstallSteam")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug", "S3168:\"async\" methods should not return \"void\"", Justification = "Call is not awaited. Task is not used")]