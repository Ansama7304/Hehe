public static class DialogRunner
{
    public static int RunGame(Dictionary<int, Dialognote> dialogMap, string playerName, Dictionary<string, Character> characters, int startID)
    {
        int currentID = startID;

        while (dialogMap.ContainsKey(currentID))
        {
            var node = dialogMap[currentID];

            if (!characters.ContainsKey(node.Character))
            {
                characters[node.Character] = new Character(node.Character);
            }

            // Check for affection-based conditions
            if (node.MinAffection.HasValue && characters[node.Character].affection < node.MinAffection.Value)
            {
                currentID++;
                continue;
            }
            if (node.MaxAffection.HasValue && characters[node.Character].affection > node.MaxAffection.Value)
            {
                currentID++;
                continue;
            }

            // Apply affection change
            characters[node.Character].affection += node.AffectionChange;

            // Replace player name in dialogue
            string dialogue = node.Text != null
                ? node.Text.Replace("[player name]", playerName)
                : "[Missing dialog Text]";
            Console.WriteLine($"\n{node.Character}: {dialogue}");
            Console.WriteLine($"[Affection: {node.Character} = {characters[node.Character].affection}]");

            // Check for affection-based fail state
            if (characters[node.Character].affection <= -10 && node.Character == "Ai")
            {
                Console.WriteLine("\n[BAD END] Ai's affection collapsed. You've unlocked the yandere spiral.");
                return currentID;
            }

            if (characters[node.Character].affection <= 0 && node.Character == "Yuki" && currentID == 999)
            {
                Console.WriteLine("\n[BREAKUP END] Yuki has given up on you.");
                return currentID;
            }

            // End if no choices
            if (node.Choices.Count == 0)
            {
                Console.WriteLine("\n[End of this branch]");
                break;
            }

            // Display choices with player name replacement
            for (int i = 0; i < node.Choices.Count; i++)
            {
                string choiceText = node.Choices[i].Text.Replace("[player name]", playerName);
                Console.WriteLine($"{i}. {choiceText}");
            }

            Console.Write("Choose: ");
            if (int.TryParse(Console.ReadLine(), out int choiceIndex) &&
                choiceIndex >= 0 && choiceIndex < node.Choices.Count)
            {
                currentID = node.Choices[choiceIndex].NextDialogID;
            }
            else
            {
                Console.WriteLine("Invalid input. Exiting.");
                break;
            }
        }
        return currentID;
    }
}