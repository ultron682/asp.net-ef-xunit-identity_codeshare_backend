using Newtonsoft.Json;

public class ChangeSet
{
    public int Start { get; set; }
    public int Length { get; set; }
    public string Text { get; set; }

    public ChangeSet(int start, int length, string text)
    {
        Start = start;
        Length = length;
        Text = text;
    }

    public static ChangeSet? FromJSON(string json)
    {
        return JsonConvert.DeserializeObject<ChangeSet>(json);
    }

    public string Apply(string document)
    {
        if (Start < 0 || Start > document.Length)
        {
            throw new ArgumentOutOfRangeException("Invalid ChangeSet range");
        }

        Console.WriteLine($"Applying ChangeSet: Start={Start}, Length={Length}, Text='{Text}', DocumentLength={document.Length}");

        return document.Substring(0, Start) + Text + document.Substring(Start + Length);
    }
}
