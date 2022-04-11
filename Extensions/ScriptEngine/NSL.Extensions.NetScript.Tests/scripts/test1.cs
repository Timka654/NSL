public class test1
{
    private static Globals global => ScriptCore.Instance.GlobalData;

    public static void test1v()
    {
        global.Character.Debug(string.Format("test static method {0}", global.Character.ID));
    }

    public static void test2v()
    {
        global.Character.Debug(string.Format("test static method {0} from static Globals", global.Character.ID));
    }
}