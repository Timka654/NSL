namespace NSL.Generators.FillTypeGenerator.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if !DEVELOP
            Test1Model tmodel1 = new Test1Model() { TestValue1 = "abcxaca", TestValue2 = "35t2452" };

            Test2Model tmodel2 = new Test2Model() { };

            tmodel1.FillTo(tmodel2);
#endif

            Test6Model model5 = new Test6Model();
            Test6Model model6 = new Test6Model();
            Test7Model model7 = new Test7Model();

            model7.Filla1To(model6);
            model7.FillTo(model6);
        }
    }
}