public partial class test2
{
    11
    private static Globals global => ScriptCore.Instance.GlobalData;
    public test2()
    {
        global.Character.Debug(string.Format("init test {0}", global.Character.ID));
    }
}