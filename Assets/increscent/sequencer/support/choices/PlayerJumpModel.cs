public class PlayerJumpModel
{
    public int sectionJumpedFrom;
    public int sectionJumpedTo;
    public int commandJumpedFrom;
    public int commandJumpedTo;

    public PlayerJumpModel(int secFrom, int secTo, int cmdFrom, int cmdTo)
    {
        sectionJumpedFrom = secFrom;
        sectionJumpedTo = secTo;
        commandJumpedFrom = cmdFrom;
        commandJumpedTo = cmdTo;
    }
}