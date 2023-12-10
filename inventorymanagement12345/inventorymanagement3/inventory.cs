using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace inventorymanagement3
{
    public partial class inventory : Form
    {
        private int currentRowIndex = 1;
        private string currentUserIdentifier;
        private DataGridView dataGridView1;
        private List<InventoryItem> inventoryItems = new List<InventoryItem>();
        private int nextItemNumber = 1;
        private User currentUser;

        public inventory(User user)
        {
            InitializeComponent();
            currentUser = user;
            InventoryFileHandler.FilePath = "path/to/file.json";
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeDataGridView();
            LoadData();
            PopulateNumbers();
        }

        private void PopulateNumbers()
        {
            const int initialRowCount = 50;
            for (int i = 1; i <= initialRowCount; i++)
            {
                AddNumberRow();
            }

            dataGridView1.Scroll += DataGridView_Scroll;
        }

        private void DataGridView_Scroll(object sender, ScrollEventArgs e)
        {
            if ((e.Type == ScrollEventType.SmallIncrement || e.Type == ScrollEventType.LargeIncrement) &&
                e.NewValue + dataGridView1.DisplayedRowCount(true) >= dataGridView1.Rows.Count - 10)
            {
                for (int i = 0; i < 10; i++)
                {
                    AddNumberRow();
                }
            }
        }

        private void AddNumberRow()
        {
            string formattedNumber = currentRowIndex++.ToString();
            dataGridView1.Rows.Add(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        private void InitializeDataGridView()
        {
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.MultiSelect = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView1.Columns.Add("ItemNumber", "Item Number");
            dataGridView1.Columns.Add("ItemName", "Item");
            dataGridView1.Columns.Add("ManufacturingCost", "Manufacturing Cost");
            dataGridView1.Columns.Add("Quantity", "Quantity");
            dataGridView1.Columns.Add("TotalPrice", "Total Price");

            dataGridView1.Columns["ItemNumber"].ReadOnly = false;
            dataGridView1.Columns["ItemNumber"].Frozen = false;
            dataGridView1.Columns["TotalPrice"].ReadOnly = true;

            dataGridView1.CellEndEdit += DataGridView1_CellEndEdit;
        }

        private void DataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["ManufacturingCost"].Index ||
                e.ColumnIndex == dataGridView1.Columns["Quantity"].Index)
            {
                CalculateTotalPrice(e.RowIndex);
            }
        }

        private void CalculateTotalPrice(int rowIndex)
        {
            string manufacturingCostValue = dataGridView1.Rows[rowIndex].Cells["ManufacturingCost"].Value?.ToString();
            string quantityValue = dataGridView1.Rows[rowIndex].Cells["Quantity"].Value?.ToString();

            if (double.TryParse(manufacturingCostValue, out double manufacturingCost) &&
                int.TryParse(quantityValue, out int quantity))
            {
                decimal totalPrice = (decimal)(manufacturingCost * quantity);
                dataGridView1.Rows[rowIndex].Cells["TotalPrice"].Value = totalPrice;
            }
            else
            {
                dataGridView1.Rows[rowIndex].Cells["TotalPrice"].Value = "Invalid";
            }
        }

        private void LoadData()
        {
            inventoryItems = InventoryFileHandler.LoadInventoryItems(currentUserIdentifier);

            if (inventoryItems.Any())
            {
                nextItemNumber = Convert.ToInt32(inventoryItems.Max(item => item.ItemNumber)) + 1;
            }

            PopulateDataGridView();
        }

        private void PopulateDataGridView()
        {
            dataGridView1.Rows.Clear();

            foreach (var item in inventoryItems)
            {
                dataGridView1.Rows.Add(item.ItemNumber, item.ItemName, item.ManufacturingCost, item.Quantity, item.TotalPrice);
            }
        }

        private void inventory_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    string itemNumber = dataGridView1.Rows[i].Cells["ItemNumber"].Value.ToString();
                    string itemName = dataGridView1.Rows[i].Cells["ItemName"].Value.ToString();

                    if (double.TryParse(dataGridView1.Rows[i].Cells["ManufacturingCost"].Value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double manufacturingCost) &&
                        int.TryParse(dataGridView1.Rows[i].Cells["Quantity"].Value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out int quantity))
                    {
                        var existingItem = inventoryItems.FirstOrDefault(item => item.ItemNumber == itemNumber);

                        if (existingItem != null)
                        {
                            if (existingItem.UserIdentifier == currentUserIdentifier)
                            {
                                existingItem.ItemName = itemName;
                                existingItem.ManufacturingCost = manufacturingCost;
                                existingItem.Quantity = quantity;
                                existingItem.TotalPrice = (decimal)(manufacturingCost * quantity);
                            }
                        }
                        else
                        {
                            var newItem = new InventoryItem
                            {
                                ItemNumber = itemNumber,
                                ItemName = itemName,
                                ManufacturingCost = manufacturingCost,
                                Quantity = quantity,
                                UserIdentifier = currentUserIdentifier,
                                TotalPrice = (decimal)(manufacturingCost * quantity)
                            };
                            inventoryItems.Add(newItem);
                        }
                    }
                }

                InventoryFileHandler.SaveInventoryItems(inventoryItems, currentUserIdentifier);

                MessageBox.Show("Data saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string searchTerm = textBox1.Text.ToLower();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null && cell.Value.ToString().ToLower().Contains(searchTerm))
                    {
                        dataGridView1.ClearSelection();
                        row.Selected = true;
                        return;
                    }
                }
            }

            MessageBox.Show("Item not found.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            items items = new items(currentUser);
            items.Show();
            this.Hide();
        }
    }
}
