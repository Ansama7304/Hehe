using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.IO;
using System.Runtime.InteropServices;
using System.Net.Http.Headers;
using System.Xml.Serialization;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace ehee;

class Program
{
    static Dictionary<string, Character> characters = new()
    {
        ["Yuki"] = new Character("Yuki"),
        ["Ai"] = new Character("Ai")
    };

    static Dictionary<string, Gift> inventory = new();
    static int eventPoints = 0;

    static void Main()
    {
        Console.Write("Enter your name: ");
        string playerName = Console.ReadLine();

        var dialogMap = DialogSystem.LoadDialog("Conversation.txt");
        int currentDialogID = 0;
        
        currentDialogID = DialogRunner.RunGame(dialogMap, playerName, characters, currentDialogID);

        while (true)
        {
            Console.WriteLine("\nWhat would you like to do?");
            Console.WriteLine("0. Continue story");
            Console.WriteLine("1. Go on adventure");
            Console.WriteLine("2. Visit the shop");
            Console.WriteLine("3. Give a gift");
            Console.WriteLine("4. View Character status");
            Console.WriteLine("5. Exit");
            Console.WriteLine("6. Save game");
            Console.WriteLine("7. Load game");
            Console.Write("Choice: ");

            string input = Console.ReadLine()!;
            switch (input)
            {
                case "0":
                    if (!dialogMap.ContainsKey(currentDialogID))
                    {
                        Console.WriteLine("[No more story left. You are free to live your life sim now!!]");
                    }
                    else
                    {
                        currentDialogID = DialogRunner.RunGame(dialogMap, playerName, characters, currentDialogID);
                    }
                    break;
                case "1":
                    DoAdventure();
                    break;
                case "2":
                    VisitShop();
                    break;
                case "3":
                    Console.WriteLine("Who do you want to gift");
                    string name = Console.ReadLine()!;
                    if (characters.ContainsKey(name))
                        GiveGift(name);
                    else
                        Console.WriteLine("Character doesn't exit");
                    break;
                case "4":
                    ListCharacter();
                    break;
                case "5":
                    Console.WriteLine("Bye bye~");
                    return;
                default:
                    Console.WriteLine("Invalid input. Try again.");
                    break;
                case "6":
                    SaveGame();
                    break;
                case "7":
                    LoadGame();
                    break;
            }
        }
    }

    static void DoAdventure()
    {
        Console.WriteLine("\n[ADVENTURE]");
        int points = new Random().Next(5, 15);
        eventPoints += points;
        Console.WriteLine($"You earned {points} event point. Total: {eventPoints}");
    }

    static void VisitShop()
    {
        Console.WriteLine("\n[SHOP]");
        var shop = GiftLoader.LoadGiftsFromFile("gift.txt");

        for (int i = 0; i < shop.Count; i++)
            Console.WriteLine($"{i}. {shop[i].Name} (Cost: {shop[i].cost})");

        Console.WriteLine("Buy which? ");
        if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 0 && choice < shop.Count)
        {
            if (eventPoints >= shop[choice].cost)
            {
                var g = shop[choice];
                if (!inventory.ContainsKey(g.Name))
                    inventory[g.Name] = g;
                inventory[g.Name].quantity++;
                eventPoints -= shop[choice].cost;
                Console.WriteLine($"Bought {g.Name}. You now have {inventory[g.Name].quantity}.");
            }
            else Console.WriteLine("Not enough points!");
        }
        else { Console.WriteLine("Invalid input. Purcharge cancelled."); }
    }

    static void GiveGift(string characterName)
    {
        Console.WriteLine("\n[GIVE GIFT]");
        int i = 0;
        foreach (var item in inventory.Where(g => g.Value.quantity > 0))
        {
            Console.WriteLine($"{i++}. {item.Key} x{item.Value.quantity}");
        }

        var giftOption = inventory.Where(g => g.Value.quantity > 0).ToList();
        if (giftOption.Count > 0)
        {
            Console.WriteLine("You have no gifts to give");
            return;
        }
        Console.WriteLine("Give Which? ");
        if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < giftOption.Count)
        {
            var gift = inventory.ElementAt(index).Value;
            int affectionGain = gift.AffectionBonus.ContainsKey(characterName)
                ? gift.AffectionBonus[characterName]
                : 0;

            characters[characterName].affection += affectionGain;
            gift.quantity--;
            Console.WriteLine($"{characterName} recieved {gift.Name}. Affection +{affectionGain}. Total: {characters[characterName].affection}");
        }
        else { Console.WriteLine("Invalid gift choice"); }
    }

    static void ListCharacter()
    {
        Console.WriteLine("\n[CHARACTER STATUS]");
        foreach (var pair in characters)
        {
            var c = pair.Value;
            Console.WriteLine($"- {c.name}: Affection = {c.affection}");
        }
    }

    static void SaveGame()
    {
        var data = new SaveData
        {
            EventPoints = eventPoints
        };

        foreach (var pair in characters)
            data.CharacterAffection[pair.Key] = pair.Value.affection;

        foreach (var item in inventory)
            data.InventoryQuantity[item.Key] = item.Value.quantity;

        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("save.json", json);
        Console.WriteLine("Game save succesfully.");
    }

    static void LoadGame()
    {
        if (!File.Exists("save.json"))
        {
            Console.WriteLine("No save file found");
            return;
        }

        string json = File.ReadAllText("save.json");
        var data = JsonSerializer.Deserialize<SaveData>(json);

        foreach (var pair in data.CharacterAffection)
        {
            if (characters.ContainsKey(pair.Key))
                characters[pair.Key].affection = pair.Value;
        }

        foreach (var pair in data.InventoryQuantity)
        {
            if (inventory.ContainsKey(pair.Key))
                inventory[pair.Key] = new Gift { Name = pair.Key, quantity = pair.Value };
            else
                inventory[pair.Key].quantity = pair.Value;
        }

        eventPoints = data.EventPoints;
        Console.WriteLine("Game load successfully");
    }
}

