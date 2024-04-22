namespace NSLLibMetaCleaner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var path = args.FirstOrDefault();

            ProcessDirectory(path);

            Console.WriteLine("finished");
        }

        static void ProcessDirectory(string path)
        {
            if (path.Contains("NSLLibMetaCleaner"))
                return;

            var f = Directory.GetFiles(path);

            var fpath = f.FirstOrDefault(x => x.EndsWith(".csproj"));

            if (fpath != null)
            {
                Console.WriteLine($"Clear - {Path.GetFullPath(fpath)}");

                var d = Path.Combine(path, "obj");

                if (Directory.Exists(d))
                {
                    Directory.Delete(d, true);
                    Console.WriteLine($"- obj");

                }
                d = Path.Combine(path, "bin");

                if (Directory.Exists(d))
                {
                    Directory.Delete(d, true);
                    Console.WriteLine($"- bin");
                }
            }

            foreach (var item in Directory.GetDirectories(path))
            {
                ProcessDirectory(item);
            }
        }
    }
}
