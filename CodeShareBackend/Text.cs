using Newtonsoft.Json;

public class Text
{
    public List<string> Lines { get; private set; }

    public Text(List<string> lines)
    {
        Lines = lines;
    }

    public Text(string text)
    {
        Lines = text.Split('\n').ToList();
    }

    public override string ToString()
    {
        return string.Join("\n", Lines);
    }

    public Text ApplyChangeSet(ChangeSet changeSet)
    {
        string currentText = this.ToString();
        string updatedText = changeSet.Apply(currentText);
        return new Text(updatedText.Split('\n').ToList());
    }
}
