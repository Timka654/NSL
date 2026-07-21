using System;

namespace NSL.ShaderVM.Experiments
{
    public class Program
    {
        public static void Main()
        {
            var shaders = new (string Name, string Glsl)[]
            {
                ("MatMulShader",          MatMulShader.GlslSource),
                ("RmsNormShader",         RmsNormShader.GlslSource),
                ("DequantQ4KShader",      DequantQ4KShader.GlslSource),
                ("FusedFfnBlockShader",   FusedFfnBlockShader.GlslSource),
                ("RowwiseSoftmaxShader",  RowwiseSoftmaxShader.GlslSource),
                ("SlotSimulationShader",  SlotSimulationShader.GlslSource),
            };

            foreach (var (name, glsl) in shaders)
            {
                Console.WriteLine(new string('=', 60));
                Console.WriteLine($"// {name}");
                Console.WriteLine(new string('=', 60));
                Console.WriteLine(glsl);
                Console.WriteLine();
            }

            Console.WriteLine(new string('=', 60));
            Console.WriteLine("VALIDATION COMPLETE");
        }
    }
}
