using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ItemShop_Metin
{
    public partial class Form2 : Form
    {
        private Timer welcomeTimer;
        private Timer moneyUpdateTimer;
        private Label userInfoLabel;

        public Form2()
        {
            InitializeComponent();

            welcomeTimer = new Timer();
            welcomeTimer.Interval = 3000;
            welcomeTimer.Tick += WelcomeTimer_Tick;
            welcomeTimer.Start();

            moneyUpdateTimer = new Timer();
            moneyUpdateTimer.Interval = 100;
            moneyUpdateTimer.Tick += MoneyUpdateTimer_Tick;
            moneyUpdateTimer.Start();

            userInfoLabel = new Label();
            userInfoLabel.AutoSize = true;
            userInfoLabel.Location = new Point(this.Width - 200, 10);
            userInfoLabel.Text = "Nume: - Banii: -";
            this.Controls.Add(userInfoLabel);

            dataGridView1.CellFormatting += dataGridView1_CellFormatting;

            DisplayDataForCategory1And2();
        }

        private void DisplayDataForCategory1And2()
        {
            try
            {
                string connectionString = "Server=127.0.0.1;Database=biliard;UserId=root;SslMode=none;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Interogare SQL pentru a selecta produsele pentru categoria 1 și 2
                    string query = "SELECT ID, itemvnum, itemname, pret, category FROM itemshop WHERE category IN (1, 2)";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Transpune datele pentru categoria 1 și categoria 2
                        DataTable transposedTable1 = TransposeDataTable(dataTable.Select("category = 1").CopyToDataTable(), "ProductDetails_Category1");
                        DataTable transposedTable2 = TransposeDataTable(dataTable.Select("category = 2").CopyToDataTable(), "Descriere_Category2");

                        // Combinați cele două tabele transpuse într-un singur set de date
                        DataTable finalTable = new DataTable();
                        finalTable.Columns.Add(transposedTable1.Columns[0].ColumnName, typeof(string));
                        finalTable.Columns.Add(transposedTable2.Columns[0].ColumnName, typeof(string));

                        for (int i = 0; i < Math.Max(transposedTable1.Rows.Count, transposedTable2.Rows.Count); i++)
                        {
                            DataRow newRow = finalTable.NewRow();
                            if (i < transposedTable1.Rows.Count)
                                newRow[0] = transposedTable1.Rows[i][0];
                            if (i < transposedTable2.Rows.Count)
                                newRow[1] = transposedTable2.Rows[i][0];
                            finalTable.Rows.Add(newRow);
                        }

                        dataGridView1.DataSource = finalTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la afișarea datelor: " + ex.Message, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable TransposeDataTable(DataTable inputTable, string columnName)
        {
            DataTable outputTable = new DataTable();

            outputTable.Columns.Add(columnName, typeof(string));

            foreach (DataRow inputRow in inputTable.Rows)
            {
                StringBuilder categoryDetails = new StringBuilder();

                for (int col = 0; col < inputTable.Columns.Count; col++)
                {
                    categoryDetails.Append($"{inputTable.Columns[col].ColumnName}: {inputRow[col]} ");
                }

                DataRow newRow = outputTable.NewRow();
                newRow[columnName] = categoryDetails.ToString().TrimEnd();
                outputTable.Rows.Add(newRow);
            }

            return outputTable;
        }




        private void Form2_Load(object sender, EventArgs e)
        {
            if (SessionManager.IsUserLoggedIn)
            {
                string currentUsername = SessionManager.CurrentUsername;
                decimal currentMoney = SessionManager.GetMoneyForCurrentUser();

                userInfoLabel.Text = $"Nume: {currentUsername}   MD: {currentMoney}";

                MessageBox.Show("Bun venit!", "Informație", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (dataGridView1.Columns.Contains("ID"))
                {
                    dataGridView1.Columns["ID"].Visible = false;
                }
            }
            else
            {
                MessageBox.Show("Nu ești autentificat. Redirecționare către Form1...", "Informație", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void MoneyUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (SessionManager.IsUserLoggedIn)
            {
                decimal currentMoney = SessionManager.GetMoneyForCurrentUser();
                userInfoLabel.Text = $"Nume: {SessionManager.CurrentUsername}   MD: {currentMoney}";
            }
        }

        private void WelcomeTimer_Tick(object sender, EventArgs e)
        {
            welcomeTimer.Stop();

            if (SessionManager.IsUserLoggedIn)
            {
                MessageBox.Show($"Bun venit!", "Informație", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Verifică dacă s-a făcut clic pe conținutul unei celule și dacă indicele rândului este valid
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count && e.ColumnIndex >= 0 && e.ColumnIndex < dataGridView1.Columns.Count)
            {
                DataGridViewColumn columnID = dataGridView1.Columns[0]; // ID
                DataGridViewColumn columnName = dataGridView1.Columns[1]; // itemname
                DataGridViewColumn columnItemVnum = dataGridView1.Columns[2]; // itemvnum
                DataGridViewColumn columnPret = dataGridView1.Columns[3]; // pret


                if (columnID != null && columnName != null && columnItemVnum != null && columnPret != null)
                {
                    object valueID = dataGridView1[columnID.Index, e.RowIndex].Value;

                    object valueName = dataGridView1[columnName.Index, e.RowIndex].Value;
                    object valueItemVnum = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    object valuePret = dataGridView1[columnPret.Index, e.RowIndex].Value;

                    if (valueID != null && valueName != null && valueItemVnum != null && valuePret != null)
                    {
                        if (e.ColumnIndex == columnID.Index)
                        {
                            e.Value = $"ID: {valueID}";
                        }
                        else
                        {
                            string combinedText = $"Nume: {valueName}\nItemVNum: {valueItemVnum}\nPret: {valuePret}";
                            e.Value = combinedText;
                            e.CellStyle.WrapMode = DataGridViewTriState.True;
                        }

                        e.FormattingApplied = true;
                    }
                }
            }
        }
        public class InventoryManager
        {
            // Exemplu de conexiune la bază de date
            private static string connectionString = "Server=127.0.0.1;Database=YourDatabaseName;User Id=YourUsername;Password=YourPassword;SslMode=none;";

            public static void AddItemToInventory(string username, int itemvnum)
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Verifică dacă utilizatorul există în inventar
                    string checkUserQuery = "SELECT COUNT(*) FROM Inventory WHERE Username = @Username";
                    using (MySqlCommand checkUserCommand = new MySqlCommand(checkUserQuery, connection))
                    {
                        checkUserCommand.Parameters.AddWithValue("@Username", username);
                        int userCount = Convert.ToInt32(checkUserCommand.ExecuteScalar());

                        if (userCount == 0)
                        {
                            // Dacă nu există, adaugă un nou utilizator în inventar
                            string insertUserQuery = "INSERT INTO Inventory (Username) VALUES (@Username)";
                            using (MySqlCommand insertUserCommand = new MySqlCommand(insertUserQuery, connection))
                            {
                                insertUserCommand.Parameters.AddWithValue("@Username", username);
                                insertUserCommand.ExecuteNonQuery();
                            }
                        }
                    }

                    // Adaugă itemvnum în inventarul utilizatorului
                    string insertItemQuery = "INSERT INTO Inventory (Username, ItemVnum) VALUES (@Username, @ItemVnum)";
                    using (MySqlCommand insertItemCommand = new MySqlCommand(insertItemQuery, connection))
                    {
                        insertItemCommand.Parameters.AddWithValue("@Username", username);
                        insertItemCommand.Parameters.AddWithValue("@ItemVnum", itemvnum);
                        insertItemCommand.ExecuteNonQuery();
                    }
                }

                Console.WriteLine($"Adăugat itemvnum {itemvnum} în inventarul utilizatorului {username}.");
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DialogResult result = MessageBox.Show("Ești sigur că vrei să cumperi acest obiect?", "Confirmare cumpărare", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Obține valoarea din coloana "vnum" din DataGridView folosind indicele coloanei
                    object valueItemVnum = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    Console.WriteLine($"Index coloană: {e.ColumnIndex}, Nume coloană: {dataGridView1.Columns[e.ColumnIndex].Name}");


                    if (valueItemVnum != null && !string.IsNullOrWhiteSpace(valueItemVnum.ToString()))
                    {
                        if (int.TryParse(valueItemVnum.ToString(), out int selectedItemId))
                        {
                            // Obține numele utilizatorului curent
                            string currentUsername = SessionManager.CurrentUsername;

                            // Adaugă itemvnum în inventarul utilizatorului
                            InventoryManager.AddItemToInventory(currentUsername, selectedItemId);

                            MessageBox.Show($"Ai adăugat obiectul cu ID-ul {selectedItemId} în inventarul utilizatorului {currentUsername}.", "Informație", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Valoarea nu poate fi convertită la un număr întreg.", "Eroare de conversie", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Valoarea este nulă sau un șir de caractere gol.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }




    }