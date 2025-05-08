
using Microsoft.Data.Sqlite;
using System.Globalization;

internal class Program
{
    static readonly string connectionString = "Data Source=habit-Tracker.db";
    private static void Main(string[] args)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();

            tableCmd.CommandText =
                $@"CREATE TABLE IF NOT EXISTS habits (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Date TEXT,
            Quantity REAL
            )";

            tableCmd.ExecuteNonQuery();

            connection.Close();
        }

        GetUserInput();

        static void GetUserInput()
        {
            bool closeApp = false;

            while (closeApp == false)
            {
                Console.Clear();

                Console.WriteLine("MAIN MENU");
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
                        Environment.Exit(0);
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

            Console.Clear();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText =
                    $"SELECT * FROM habits";

                List<Habits> tableData = new();

                SqliteDataReader reader = tableCmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tableData.Add(
                        new Habits
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Date = DateTime.ParseExact(reader.GetString(2), "dd-MM-yy", new CultureInfo("en-US")),
                            Measurement = reader.GetDouble(3)
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
                Console.ReadKey();
            }
        }

        static void Insert()
        {

            string date = GetDateInput();

            int quantity = GetNumberInput("Please insert a number of glasses or other measure of your choice (no decimals allowed)\n");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText =
                    $"INSERT INTO drinking_water(date, quantity) VALUES (@Date, @Quantity)";

                tableCmd.Parameters.Add("@Date", SqliteType.Text).Value = date;
                tableCmd.Parameters.Add("@Quantity", SqliteType.Integer).Value = quantity;

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }

            Console.WriteLine("Record inserted sucessfully.");
            Console.ReadKey();
        }

        static void Update()
        {
            Console.Clear();
            GetAllRecords();

            var recordId = GetNumberInput("\n\nPlease type Id of the record would like to update. Type 0 to return to main menu.");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var checkCmd = connection.CreateCommand();
                
                checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id = @Id)";
                checkCmd.Parameters.Add("@Id",SqliteType.Real).Value = recordId;

                int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (checkQuery == 0)
                {
                    Console.WriteLine($"\nRecord with Id {recordId} doesn't exist.\n");
                    Console.ReadKey();
                    connection.Close();
                    Update();
                    return;
                }

                string date = GetDateInput();

                int quantity = GetNumberInput("\nInsert the number of glasses or other measure of your choice");

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"UPDATE drinking_water SET date = @Date, quantity = @Quantity WHERE Id = @Id";

                tableCmd.Parameters.Add("@Date", SqliteType.Text).Value = date;
                tableCmd.Parameters.Add("@Id", SqliteType.Real).Value = recordId;
                tableCmd.Parameters.Add("@Quantity", SqliteType.Real).Value = quantity;

                tableCmd.ExecuteNonQuery();

                Console.WriteLine("\nRecord updated successfully.\n");
                Console.ReadKey();
            }
        }

        static void Delete()
        {
            Console.Clear();
            GetAllRecords();

            var recordId = GetNumberInput("\n\nType the Id of the record you want to delete or press 0 to go back to the Main Menu");

            if (recordId == 0)
            {
                return;
            }

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText = $"DELETE from drinking_water WHERE Id = @Id";
                tableCmd.Parameters.Add("@Id", SqliteType.Integer).Value = recordId;

                int rowCount = tableCmd.ExecuteNonQuery();

                if (rowCount == 0)
                {
                    Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist. \n\n");
                    Console.ReadKey();
                    Delete();
                    return;
                }
            }

            Console.WriteLine($"\n\nRecord with Id {recordId} was deleted. \n\n");
            Console.ReadKey();
        }

        static string GetDateInput()
        {
            Console.WriteLine("Please insert the date: (Format: dd-mm-yy). Type t to enter today's date or 0 to return to main menu");

            string? dateInput = Console.ReadLine().Trim().ToLower();

            if (dateInput == "0") GetUserInput();
            if (dateInput == "t") return DateTime.Now.ToString("dd-MM-yy");

            while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
            {
                Console.WriteLine("\nInvalid date. (Format: dd-mm-yy). Type 0 to return to main menu or try again:\n");
                dateInput = Console.ReadLine();
            }

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

    public class Habits
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public double Measurement { get; set; }
    }
    public class DrinkingWater
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
    }
}

