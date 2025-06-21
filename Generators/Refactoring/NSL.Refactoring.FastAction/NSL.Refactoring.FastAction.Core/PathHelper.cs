using System;
using System.IO;

namespace NSL.Refactoring.FastAction.Core
{
    public static class PathHelper
    {
        public static string GetRelativePath(string basePath, string targetPath)
        {
            if (string.IsNullOrEmpty(basePath)) throw new ArgumentNullException(nameof(basePath));
            if (string.IsNullOrEmpty(targetPath)) throw new ArgumentNullException(nameof(targetPath));

            basePath = Path.GetFullPath(basePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
            targetPath = Path.GetFullPath(targetPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));

            // On Windows, ignore case
            bool isWindows = Path.DirectorySeparatorChar == '\\';
            StringComparison comparison = isWindows ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            // Find common root
            string[] baseParts = basePath.Split(new char[] { Path.DirectorySeparatorChar }, options: StringSplitOptions.RemoveEmptyEntries);
            string[] targetParts = targetPath.Split(new char[] { Path.DirectorySeparatorChar }, options: StringSplitOptions.RemoveEmptyEntries);

            int commonLength = 0;
            int maxLength = Math.Min(baseParts.Length, targetParts.Length);

            while (commonLength < maxLength &&
                   string.Equals(baseParts[commonLength], targetParts[commonLength], comparison))
            {
                commonLength++;
            }

            // If drive letters (on Windows) are different, return full target path
            if (isWindows && commonLength == 0 && baseParts.Length > 0 && targetParts.Length > 0 &&
                !string.Equals(baseParts[0], targetParts[0], comparison))
            {
                return targetPath;
            }

            // Add ".." for each remaining base part
            var relativeParts = new string[baseParts.Length - commonLength + targetParts.Length - commonLength];
            for (int i = 0; i < baseParts.Length - commonLength; i++)
            {
                relativeParts[i] = "..";
            }
            for (int i = commonLength; i < targetParts.Length; i++)
            {
                relativeParts[baseParts.Length - commonLength + i - commonLength] = targetParts[i];
            }

            return relativeParts.Length > 0 ? string.Join(Path.DirectorySeparatorChar.ToString(), relativeParts) : ".";
        }
    }
}
