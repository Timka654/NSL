namespace NSLLibMetaCleaner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var path = args.FirstOrDefault();

            ProcessDirectory(path);

            Console.WriteLine("Hello, World!");
        }

        static void ProcessDirectory(string path)
        {
            var f =
            Directory.GetFiles(path);

            if (f.Any(x => x.EndsWith(".csproj")))
            {
                var d = Path.Combine(path,"obj");

                if (Directory.Exists(d))
                    Directory.Delete(d, true);
                
                d = Path.Combine(path,"bin");

                if (Directory.Exists(d))
                    Directory.Delete(d, true);
            }

            foreach (var item in Directory.GetDirectories(path))
            {
                ProcessDirectory(item);
            }
        }
    }
}
