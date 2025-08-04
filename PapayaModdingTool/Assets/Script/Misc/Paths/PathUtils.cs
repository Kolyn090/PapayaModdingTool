using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Misc.Paths
{
    public static class PathUtils
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetLongPathName(string shortPath, StringBuilder longPath, uint bufferLength);

        public static string GetLongPath(string shortPath)
        {
            if (!Directory.Exists(shortPath) && !File.Exists(shortPath))
            {
                Debug.LogWarning("Path does not exist: " + shortPath);
                return shortPath;
            }

            StringBuilder sb = new(1024);
            uint result = GetLongPathName(shortPath, sb, (uint)sb.Capacity);

            if (result == 0)
            {
                Debug.LogWarning("GetLongPathName failed for: " + shortPath);
                return shortPath;
            }

            return sb.ToString();
        }

        public static bool ArePathsEqual(string path1, string path2)
        {
            string fullPath1 = Path.GetFullPath(path1).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToLowerInvariant();
            string fullPath2 = Path.GetFullPath(path2).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToLowerInvariant();

            return fullPath1 == fullPath2;
        }

        public static bool PathStartsWith(string pathA, string pathB)
        {
            string fullPathA = Path.GetFullPath(pathA).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToLowerInvariant();
            string fullPathB = Path.GetFullPath(pathB).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToLowerInvariant();

            return fullPathA.StartsWith(fullPathB);
        }

        public static void DeleteAllContents(string targetDir)
        {
            if (!Directory.Exists(targetDir))
                return;

            // Delete files
            foreach (string file in Directory.GetFiles(targetDir))
            {
                File.Delete(file);
            }

            // Delete subdirectories
            foreach (string dir in Directory.GetDirectories(targetDir))
            {
                Directory.Delete(dir, true); // true = recursive delete
            }
        }

        public static (string, string) SplitByLastRegex(string input, string pattern)
        {
            MatchCollection matches = Regex.Matches(input, pattern);
            if (matches.Count == 0)
                return (input, ""); // No match, return full string and empty

            Match lastMatch = matches[^1];
            int splitIndex = lastMatch.Index;

            return (input[..splitIndex], input[splitIndex..]);
        }
    }
}