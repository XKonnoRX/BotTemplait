
public class MessageContainer
{
    public Dictionary<string, string> messages { get; set; }
    public Dictionary<string, string[]> menues { get; set; }
    public Dictionary<string, Button[]> inlines { get; set; }
}
public class Button
{
    public string? type { get; set; }
    public string? text { get; set; }
    public string? back { get; set; }
}
