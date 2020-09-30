using Microsoft.AppCenter.Crashes;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global

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
