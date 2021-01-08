public class FindRoomsOptions
{
    public bool Debug { get; }
    public static readonly FindRoomsOptions Default = new FindRoomsOptions();

    public FindRoomsOptions(bool debug = false)
    {
        Debug = debug;
    }
    
}