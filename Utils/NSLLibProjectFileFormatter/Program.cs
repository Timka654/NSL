namespace NSLLibProjectFileFormatter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
                throw new Exception();

            var path = args.FirstOrDefault();

            new Formatter(path).Run();
        }

    }
}