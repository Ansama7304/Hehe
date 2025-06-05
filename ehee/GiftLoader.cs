using System;
using System.Collections.Generic;
using System.IO;

public static class GiftLoader
{
    public static List<Gift> LoadGiftsFromFile(string filePath)
    {
        var gifts = new List<Gift>();
        Gift currentGift = null;

        foreach (var line in File.ReadLines(filePath))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            if (trimmed == "[Gift]")
            {
                if (currentGift != null)
                    gifts.Add(currentGift);
                currentGift = new Gift();
            }
            else if (trimmed.StartsWith("Name:"))
            {
                currentGift.Name = trimmed.Substring("Name:".Length).Trim();
            }
            else if (trimmed.StartsWith("Cost:"))
            {
                currentGift.cost = int.Parse(trimmed.Substring("Cost:".Length).Trim());
            }
            else if (trimmed == "Affects:")
            {
                // Affects block begins — keep reading in the next loop
            }
            else if (trimmed.Contains('='))
            {
                var parts = trimmed.Split('=');
                string charName = parts[0].Trim();
                int affection = int.Parse(parts[1].Trim());
                currentGift.AffectionBonus[charName] = affection;
            }
        }

        if (currentGift != null)
            gifts.Add(currentGift);

        return gifts;
    }
}