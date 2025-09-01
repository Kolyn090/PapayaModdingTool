using System;
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

        public static string GetRawPath(string shortPath)
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
                Debug.LogWarning("GetRawPathName failed for: " + shortPath);
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

        public static string ToLongPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));

            // Normalize to full absolute path
            path = Path.GetFullPath(path);

            // Already long path?
            if (path.StartsWith(@"\\?\") || path.StartsWith(@"\\.\"))
                return path;

            if (path.StartsWith(@"\\"))
            {
                // UNC path (e.g. \\server\share)
                return @"\\?\UNC\" + path[2..];
            }
            else
            {
                // Local path (e.g. C:\folder\file)
                return @"\\?\" + path;
            }
        }

        // List of extensions Unity commonly supports
        private static readonly string[] extensions =
        {
        ".png", ".jpg", ".jpeg", ".tga", ".bmp", ".gif", ".psd", ".tif", ".tiff", ".exr", ".hdr"
        };

        /// <summary>
        /// Returns the full path of the existing image if found, otherwise null.
        /// </summary>
        public static string FindImagePath(string pathWithoutExtension)
        {
            foreach (var ext in extensions)
            {
                string fullPath = pathWithoutExtension + ext;
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }

        /// <summary>
        /// Safely duplicate a file multiple times in the same folder.
        /// Names will be suffixed with _copy, _copy1, _copy2, etc.
        /// </summary>
        /// <param name="filePath">Path of the file to duplicate</param>
        /// <param name="count">How many duplicates to make</param>
        /// <returns>Array of paths of the duplicates</returns>
        public static string[] DuplicateFile(string filePath, int count)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found: " + filePath);

            string folder = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            string[] createdPaths = new string[count];

            for (int i = 0; i < count; i++)
            {
                string newPath;

                if (i == 0)
                    newPath = Path.Combine(folder, fileName + "_copy" + extension);
                else
                    newPath = Path.Combine(folder, fileName + "_copy" + i + extension);

                // Ensure uniqueness if file already exists
                newPath = GetUniquePath(newPath);

                File.Copy(filePath, newPath);
                createdPaths[i] = newPath;
            }

            return createdPaths;
        }

        /// <summary>
        /// Ensures the file path is unique by appending a counter if needed.
        /// </summary>
        private static string GetUniquePath(string path)
        {
            int counter = 1;
            string folder = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);

            string newPath = path;

            while (File.Exists(newPath))
            {
                newPath = Path.Combine(folder, $"{fileName}_{counter}{extension}");
                counter++;
            }

            return newPath;
        }
        
        /// <summary>
        /// Moves a file to a new location safely.
        /// If the destination file already exists, it appends (1), (2), etc.
        /// </summary>
        /// <param name="sourcePath">The file to move</param>
        /// <param name="destPath">The desired destination path</param>
        /// <returns>The final path where the file was moved</returns>
        public static string MoveFileSafe(string sourcePath, string destPath)
        {
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Source file not found", sourcePath);

            string folder = Path.GetDirectoryName(destPath);
            string fileName = Path.GetFileNameWithoutExtension(destPath);
            string extension = Path.GetExtension(destPath);

            string finalPath = destPath;
            int counter = 1;

            while (File.Exists(finalPath))
            {
                finalPath = Path.Combine(folder, $"{fileName} ({counter}){extension}");
                counter++;
            }

            Directory.CreateDirectory(folder); // ensure destination folder exists
            File.Move(sourcePath, finalPath);

            return finalPath;
        }
    }
}