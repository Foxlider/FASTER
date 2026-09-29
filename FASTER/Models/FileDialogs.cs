using Microsoft.Win32;

namespace FASTER.Models
{
    internal static class FileDialogs
    {
        public static string? SelectFile(string filter)
        {
            OpenFileDialog openFileDialog = new() { Filter = filter };
            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }
    }
}
