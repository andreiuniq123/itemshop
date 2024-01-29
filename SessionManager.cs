using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace ItemShop_Metin
{
    public static class SessionManager
    {
        private static MySqlConnection connection;
        private static string connectionString = "Server=127.0.0.1;Database=biliard;UserId=root;SslMode=none;";


        // Informații de sesiune
        private static bool isUserLoggedIn = false;
        private static string currentUsername = "";

        // Proprietăți pentru acces la informațiile de sesiune
        public static bool IsUserLoggedIn
        {
            get { return isUserLoggedIn; }
        }
        public static string ConnectionString
        {
            get { return connectionString; }
        }
        public static string CurrentUsername
        {
            get { return currentUsername; }
        }

        // Proprietate pentru acces la conexiunea la baza de date
        public static MySqlConnection Connection
        {
            get { return connection; }
        }

        // Metodă pentru a seta informațiile de sesiune la autentificare
        public static void SetLoggedInUser(string username)
        {
            isUserLoggedIn = true;
            currentUsername = username;
        }

        // Metodă pentru a seta informațiile de sesiune la deconectare
        public static void SetLoggedOutUser()
        {
            isUserLoggedIn = false;
            currentUsername = "";
        }

        // Metodă pentru a deschide conexiunea la baza de date
        public static void OpenConnection()
        {
            if (connection == null)
            {
                connection = new MySqlConnection(connectionString);
            }

            if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
            {
                connection.Open();
            }
        }

        public static void CloseConnection()
        {
            if (connection != null && (connection.State == ConnectionState.Open || connection.State == ConnectionState.Connecting))
            {
                connection.Close();
            }
        }

        // Metodă pentru a obține suma de bani pentru utilizatorul curent

        public static void SetMoneyForCurrentUser(decimal newMoney)
        {
            try
            {
                OpenConnection();

                string query = "UPDATE user SET md = @newMoney WHERE user = @username";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@newMoney", newMoney);
                    command.Parameters.AddWithValue("@username", currentUsername);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la actualizarea sumei de bani: {ex.Message}");
            }
            finally
            {
                CloseConnection();
            }
        }
        public static decimal GetMoneyForCurrentUser()
        {
            try
            {
                OpenConnection();


                string query = "SELECT md FROM user WHERE user = @username";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", currentUsername);

                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToDecimal(result);
                    }

                    return 0; // Sau o altă valoare implicită, în funcție de cerințe
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la obținerea sumei de bani: {ex.Message}");
                return 0;
            }
            finally
            {
                CloseConnection();
            }
        }
    }
}
