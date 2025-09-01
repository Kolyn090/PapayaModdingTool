using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine; // remove if you just want System.Drawing.Color

namespace PapayaModdingTool.Assets.Script.Misc.ColorGen
{
    public static class ColorGenerator
    {
        /// <summary>
        /// Generates a deterministic random color from a string key.
        /// </summary>
        public static Color GetColorFromString(string key)
        {
            if (string.IsNullOrEmpty(key))
                return Color.white;

            // Create a hash of the string
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(key));

                // Use first three bytes for RGB
                float r = hash[0] / 255f;
                float g = hash[1] / 255f;
                float b = hash[2] / 255f;

                return new Color(r, g, b);
            }
        }
    }
}