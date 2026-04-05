using NSL.Generators.FillTypeGenerator.Shared;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop
{
    public class IgnoreModel
    {
        public int Id { get; set; }
    }

    [FillTypeGenerate(typeof(Ignore2Model))]
    public partial class Ignore2Model
    {
        public int Id { get; set; }

        public List<string> List { get; set; }
    }
#if DEVELOP

    public partial class Ignore2Model
    {
        [NSL.Generators.FillTypeGenerator.Attributes.FillTypeGenerateIgnore]
        public List<Ignore3Model> List2 { get; set; }
    }

    public abstract class Ignore3Model
    {
        public int Abc { get; set; }
    }
#endif
}
