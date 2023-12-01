
public class MessageContainer
{
    public MessageJSon[] messages { get; set; }
    public Menue[] menues { get; set; }
    public Inline[] inlines { get; set; }
    public Dictionary<string, string> messageDict { get; set; }
    public void CreateDictionary()
    {

        messageDict = new Dictionary<string, string>();
        foreach (var mess in messages)
            messageDict.Add(mess.name, mess.value);
    }
}

public class Messages
{
    public string start_message { get; set; }
    
}
public class MessageJSon
{
    public string name { get; set; }
    public string value { get; set; }
}
public class Menue
{
    public string name { get; set; }
    public string[] buttons { get; set; }
}

public class Inline
{
    public string name { get; set; }
    public Button[] buttons { get; set; }
}

public class Button
{
    public string? type { get; set; }
    public string? text { get; set; }
    public string? back { get; set; }
}
