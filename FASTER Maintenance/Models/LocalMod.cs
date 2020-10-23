using System;

// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable once CheckNamespace
namespace FASTER.Models
{
    [Serializable]
    internal class LocalMod
    {
        public string Name    { get; set; } = string.Empty;
        public string Path    { get; set; } = string.Empty;
        public string Author  { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
    }
}
