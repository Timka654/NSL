namespace NSL.Generators.SelectTypeCycleGenerator.Shared
{
    public enum SqlCycleFilterTarget
    {
        Both,
        Anchor,    // Только для Root (якоря)
        Recursion  // Только для дочерних элементов
    }
}
