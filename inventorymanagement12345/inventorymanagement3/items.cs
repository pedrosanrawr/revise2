using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace inventorymanagement3
{
    public partial class items : Form
    {
        private DataGridView dataGridView1;
        private List<InventoryItem> inventoryItems;
        private User currentUser;

        public items(User user)
        {
            InitializeComponent();
            currentUser = user;
            InventoryFileHandler.FilePath = "path/to/file.json";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.dataGridView1 = new DataGridView();
            InitializeDataGridView();
            LoadData();
            PopulateDataGridView();
        }

        private void InitializeDataGridView()
        {
            dataGridView1 = new DataGridView();
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.MultiSelect = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView1.Columns.Add("ItemNumber", "Item Number");
            dataGridView1.Columns.Add("Item", "Item");
            dataGridView1.Columns.Add("Status", "Status");
            dataGridView1.Columns.Add("Quantity", "Quantity");

            Controls.Add(dataGridView1);
        }

        private void LoadData()
        {
            inventoryItems = InventoryFileHandler.LoadInventoryItems(currentUser.UserIdentifier);
        }

        private void PopulateDataGridView()
        {
            // Clear existing rows
            dataGridView1.Rows.Clear();

            // Populate DataGridView with data from inventoryItems
            foreach (var item in inventoryItems)
            {
                int quantity = item.Quantity;

                // Determine the status based on quantity
                string status = GetStatus(quantity);

                // Add a new row to the DataGridView
                dataGridView1.Rows.Add(item.ItemNumber, item.ItemName, status, quantity);
            }
        }

        private string GetStatus(int quantity)
        {
            if (quantity > 10)
            {
                return "In Stock";
            }
            else if (quantity > 0)
            {
                return "Running Low";
            }
            else
            {
                return "Out of Stock";
            }
        }



        private void button3_Click(object sender, EventArgs e)
        {
            inventory inventory = new inventory(currentUser);
            inventory.Show();
            this.Hide();
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
    }
}
