public class ScriptCore
{
    public const string globalMember = "GlobalData";
    public const string initMethod = "Initialize";

    public Globals GlobalData { get; set; }

    public static ScriptCore Instance;

    public void Initialize()
    {
        Instance = this;
    }
}