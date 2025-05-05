
using Microsoft.Data.Sqlite;
using System.Globalization;

internal class Program
{
    private static void Main(string[] args)
    {
        string connectionString = "Data Source=habit-Tracker.db";

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();

            tableCmd.CommandText =
                @"CREATE TABLE IF NOT EXISTS drinking_water (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Date TEXT,
            Quantity INTEGER
            )";

            tableCmd.ExecuteNonQuery();

            connection.Close();
        }

        GetUserInput();

        static void GetUserInput()
        {
            Console.Clear();
            bool closeApp = false;

            while (closeApp == false)
            {
                Console.WriteLine("\n\nMAIN MENU");
                Console.WriteLine("\nWhat would you like to do?");
                Console.WriteLine("\nType 0 to Close Application.");
                Console.WriteLine("Type 1 to View All Records.");
                Console.WriteLine("Type 2 to Insert Record.");
                Console.WriteLine("Type 3 to Delete Record.");
                Console.WriteLine("Type 4 to Update Record.");
                Console.WriteLine("------------------------------------------\n");

                string? commandInput = Console.ReadLine();

                switch (commandInput)
                {
                    case "0":
                        Console.WriteLine("Goodbye\n");
                        closeApp = true;
                        break;
                    case "1":
                        GetAllRecords();
                        break;
                    case "2":
                        Insert();
                        break;
                    case "3":
                        Delete();
                        break;
                    case "4":
                        Update();
                        break;
                    default:
                        Console.WriteLine("Invalid Command. Please type a number from 0 to 4.\n");
                        break;
                }


            }
        }

        static void GetAllRecords()
        {
            string connectionString = "Data Source=habit-Tracker.db";

            Console.Clear();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText =
                    $"SELECT * FROM drinking_water";

                List<DrinkingWater> tableData = new();

                SqliteDataReader reader = tableCmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tableData.Add(
                        new DrinkingWater
                        {
                            Id = reader.GetInt32(0),
                            Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yyyy", new CultureInfo("en-US")),
                            Quantity = reader.GetInt32(2)
                        });
                    }
                }
                else
                {
                    Console.WriteLine("No rows found");
                }

                connection.Close();

                Console.WriteLine("---------------------------------------------");
                foreach (var dw in tableData)
                {
                    Console.WriteLine($"{dw.Id} - {dw.Date.ToString("dd-MM-yyyy")} - Quantity: {dw.Quantity}");
                }
                Console.WriteLine("---------------------------------------------\n");
            }
        }

        static void Insert()
        {
            string connectionString = "Data Source=habit-Tracker.db";

            string date = GetDateInput();

            int quantity = GetNumberInput("Please insert a number of glasses or other measure of your choice (no decimals allowed)\n");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText =
                    $"INSERT INTO drinking_water(date, quantity) VALUES ('{date}', {quantity})";

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }

            GetUserInput();
        }

        static void Update()
        {
            string connectionString = "Data Source=habit-Tracker.db";

            Console.Clear();
            GetAllRecords();

            var recordId = GetNumberInput("\n\nPlease type Id of the record would like to update. Type 0 to return to main menu.");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id = {recordId})";
                int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (checkQuery == 0)
                {
                    Console.WriteLine($"\nRecord with Id {recordId} doesn't exist.\n");
                    Console.ReadKey();
                    connection.Close();
                    Update();
                }

                string date = GetDateInput();

                int quantity = GetNumberInput("\nInsert the number of glasses or other measure of your choice");

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"UPDATE drinking_water SET date = '{date}', quantity = {quantity} WHERE Id = {recordId}";

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }
        }

        static void Delete()
        {
            Console.Clear();
            GetAllRecords();

            var recordId = GetNumberInput("\n\nType the Id of the record you want to delete or press 0 to go back to the Main Menu");

            string connectionString = "Data Source=habit-Tracker.db";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText = $"DELETE from drinking_water WHERE Id = '{recordId}'";

                int rowCount = tableCmd.ExecuteNonQuery();

                if (rowCount == 0)
                {
                    Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist. \n\n");
                    Console.ReadKey();
                    Delete();
                }
            }

            Console.WriteLine($"\n\nRecord with Id {recordId} was deleted. \n\n");
            Console.ReadKey();

            GetUserInput();
        }

        static string GetDateInput()
        {
            Console.WriteLine("Please insert the date: (Format: dd-mm-yyyy). Type 0 to return to main menu");

            string? dateInput = Console.ReadLine();

            if (dateInput == "0") GetUserInput();

            return dateInput;
        }

        static int GetNumberInput(string message)
        {
            Console.WriteLine(message);

            string numberInput = Console.ReadLine();

            if (numberInput == "0") GetUserInput();

            int finalInput = Convert.ToInt32(numberInput);

            return finalInput;
        }
    }

    public class DrinkingWater
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
    }
}

