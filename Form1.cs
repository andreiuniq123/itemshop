using System;
using System.Data;
using System.Windows.Forms;
using BCrypt;
using MySql.Data.MySqlClient;

namespace ItemShop_Metin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            if (CheckDatabaseConnection())
            {
                MessageBox.Show("Ai fost conectat la baza de date!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Eroare la conectarea la baza de date TEST.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool CheckDatabaseConnection()
        {
            try
            {
                SessionManager.OpenConnection(); // Deschide conexiunea utilizând SessionManager

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la conectarea la baza de date: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                SessionManager.CloseConnection(); // Închide conexiunea utilizând SessionManager
            }
        }

        private bool VerifyCredentialsFromDatabase(string username, string password)
        {
            try
            {
                SessionManager.OpenConnection();

                // Obține parola criptată din baza de date pentru utilizatorul dat
                string query = "SELECT password FROM user WHERE user = @username";
                using (MySqlCommand command = new MySqlCommand(query, SessionManager.Connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string hashedPasswordFromDatabase = reader["password"].ToString();

                            // Adaugă mesaje de consolă pentru a urmări fluxul execuției și valorile variabilelor
                            Console.WriteLine($"Parola introdusă: {password}");
                            Console.WriteLine($"Parola criptată din baza de date: {hashedPasswordFromDatabase}");

                            // Verifică parola criptată utilizând BCrypt
                            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, hashedPasswordFromDatabase);
                            Console.WriteLine($"Rezultatul verificării parolei: {isPasswordCorrect}");

                            return isPasswordCorrect;
                        }
                    }

                    return false; // Utilizatorul nu a fost găsit în baza de date
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la conectarea la baza de date: {ex.Message}");
                return false;
            }
            finally
            {
                SessionManager.CloseConnection();
            }
        }

        private void BtnLogin_Click_1(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (VerifyCredentialsFromDatabase(username, password))
            {
                MessageBox.Show("Autentificare reușită!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Setează informațiile de sesiune la autentificare
                SessionManager.SetLoggedInUser(username);

                // Aici poți naviga către alt formular sau executa alte acțiuni după autentificare
                Form2 form2 = new Form2();
                form2.Show();

                // Ascunde Form1 (formularul curent)
                this.Hide();
            }
            else
            {
                MessageBox.Show("Autentificare eșuată. Verificați numele de utilizator și parola.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

    }
}
