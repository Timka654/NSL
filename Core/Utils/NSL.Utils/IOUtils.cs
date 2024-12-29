using System;
using System.IO;
using System.Linq;

namespace NSL.Utils
{
    public static class IOUtils
    {
        public static void CreateDirectoryIfNoExists(this DirectoryInfo dir)
        {
            CreateDirectoryIfNoExists(dir.FullName);
        }

        public static void CreateDirectoryIfNoExists(string path)
        {
            var splited = path.Split(Path.DirectorySeparatorChar);

            var firstElement = splited.First();

            if (splited.Any(x => (string.IsNullOrWhiteSpace(x) && firstElement != x) || x.Trim() != x))
                throw new Exception($"invalid path \"{path}\"");

            path = string.Join("" + Path.DirectorySeparatorChar, splited.Select(x => x.Trim()));

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public delegate bool FileConfirmDelegate(string destinationPath, FileInfo sourceFile);
        public delegate void FileDelegate(string destinationPath, FileInfo sourceFile);

        public static void CopyDirectory(string sourceDir
            , string destinationDir
            , bool recursive
            , bool overwrite = true
            , FileDelegate onCopy = null
            , FileConfirmDelegate filter = null)
        {
            onCopy = onCopy ?? ((destinationPath, sourceFile) => { });
            filter = filter ?? ((destinationPath, relativePath) => true);

            var _sourceDir = new DirectoryInfo(sourceDir);

            if (!_sourceDir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {_sourceDir.FullName}");

            int srcDirSubStrOffset = _sourceDir.FullName.Length + 1;

            Directory.CreateDirectory(destinationDir);

            if (recursive)
            {
                var dirs = _sourceDir.GetDirectories("*", SearchOption.AllDirectories);

                foreach (DirectoryInfo subDir in dirs)
                {
                    var destPath = Path.Combine(destinationDir, subDir.FullName.Substring(srcDirSubStrOffset - 1));

                    Directory.CreateDirectory(destPath);
                }
            }


            var copyFiles = _sourceDir.GetFiles("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foreach (FileInfo file in copyFiles)
            {
                string targetFilePath = Path.Combine(destinationDir, file.FullName.Substring(srcDirSubStrOffset - 1));

                if (!filter(targetFilePath, file))
                    continue;

                file.CopyTo(targetFilePath, overwrite);

                onCopy(targetFilePath, file);
            }
        }
    }
}
