﻿using System;
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

    }
}
