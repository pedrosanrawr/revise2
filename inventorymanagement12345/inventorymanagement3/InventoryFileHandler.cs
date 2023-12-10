using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace inventorymanagement3
{
    public class InventoryItem
    {
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public double ManufacturingCost { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string UserIdentifier { get; set; }
    }

    public static class InventoryFileHandler
    {
        private static string filePath;

        public static string FilePath
        {
            get => filePath;
            set
            {
                string directoryPath = Path.GetDirectoryName(value);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                filePath = value;
            }
        }

        public static void SaveInventoryItems(List<InventoryItem> inventoryItems, string userIdentifier)
        {
            string directoryPath = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            List<InventoryItem> existingItems = new List<InventoryItem>();
            if (File.Exists(FilePath))
            {
                string existingJson = File.ReadAllText(FilePath);
                existingItems = JsonConvert.DeserializeObject<List<InventoryItem>>(existingJson);
            }

            existingItems.AddRange(inventoryItems);

            var userItems = existingItems.FindAll(item => item.UserIdentifier == userIdentifier);

            string json = JsonConvert.SerializeObject(userItems, Formatting.Indented);

            File.WriteAllText(FilePath, json);
        }

        public static List<InventoryItem> LoadInventoryItems(string userIdentifier)
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                List<InventoryItem> loadedItems = JsonConvert.DeserializeObject<List<InventoryItem>>(json);
                return loadedItems.FindAll(item => item.UserIdentifier == userIdentifier);
            }

            return new List<InventoryItem>();
        }
    }
}
