using System.Xml.Serialization;

public class Dialognote
{
    public int ID;
    public string Character = "";
    public string Text = "";
    public int AffectionChange;
    public List<Choice> Choices = new();
    public int? MinAffection;
    public int? MaxAffection;
}

public class Choice
{
    public string Text;
    public int NextDialogID;
}

static class DialogSystem
{
    public static Dictionary<int, Dialognote> LoadDialog(string path)
    {
        var dialogMap = new Dictionary<int, Dialognote>();
        Dialognote current = null;

        foreach (var line in File.ReadLines(path))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("[DialogID]"))
            {
                if (current != null)
                    dialogMap[current.ID] = current;
                current = new Dialognote { ID = int.Parse(trimmed.Split(']')[1].Trim()) };
            }
            else if (trimmed.StartsWith("Character:"))
                current.Character = trimmed.Substring("Character:".Length).Trim();
            else if (trimmed.StartsWith("Text:"))
                current.Text = trimmed.Substring("Text:".Length).Trim();
            else if (trimmed.StartsWith("Affection"))
                current.AffectionChange = int.Parse(trimmed.Substring("Affection:".Length).Trim());
            else if (trimmed.StartsWith("[Answer]")) {/* skip */}
            else if (trimmed.Contains(".") && trimmed.Contains("|"))
            {
                var segments = trimmed.Split('.', 2);
                if (segments.Length < 2) continue;

                var parts = segments[1].Split('|');
                if (parts.Length < 2) continue;

                current.Choices.Add(new Choice
                {
                    Text = parts[0].Trim(),
                    NextDialogID = int.Parse(parts[1].Trim())
                });
            }
        }

        if (current != null)
            dialogMap[current.ID] = current;

        return dialogMap;
    }
}