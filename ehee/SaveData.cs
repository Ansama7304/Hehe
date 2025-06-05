using System.Dynamic;

public class SaveData
{
    public Dictionary<string, int> CharacterAffection { get; set; } = new();
    public Dictionary<string, int> InventoryQuantity { get; set; } = new();
    public int EventPoints { get; set; }
}