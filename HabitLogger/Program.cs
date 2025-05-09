
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
            Name TEXT,
            MeasurementUnit TEXT,
            MeasurementValue REAL
            )";

            tableCmd.ExecuteNonQuery();

            var countCmd = connection.CreateCommand();
            countCmd.CommandText = $"SELECT COUNT(*) FROM habits";

            int rowCount = Convert.ToInt32(countCmd.ExecuteScalar());

            if (rowCount == 0)
            {
                countCmd.CommandText = PreseedDB();
                countCmd.ExecuteNonQuery();
            }
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
                            Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                            Name = reader.GetString(2),
                            MeasurementUnit = reader.GetString(3),
                            MeasurementValue = reader.GetDouble(4),
                        });
                    }
                }
                else
                {
                    Console.WriteLine("No rows found");
                }

                connection.Close();

                Console.WriteLine("---------------------------------------------");
                foreach (var habit in tableData)
                {
                    Console.WriteLine($"{habit.Id} - {habit.Date.ToString("dd-MM-yyyy")} - {habit.Name}: {habit.MeasurementUnit} {habit.MeasurementValue}");
                }
                Console.WriteLine("---------------------------------------------\n");
                Console.ReadKey();
            }
        }

        static void Insert()
        {

            string date = GetDateInput();

            string name = GetStringInput("Enter the name of your habit:");

            string measurementUnit = GetStringInput("Enter the measurement unit (km, minutes, pages");

            double measurementValue = GetNumberInput("Enter the measurement value");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText =
                    $"INSERT INTO habits (date, name, measurementUnit, measurementValue) VALUES (@Date, @Name, @MeasurementUnit, @MeasurementValue)";

                tableCmd.Parameters.Add("@Date", SqliteType.Text).Value = date;
                tableCmd.Parameters.Add("@Name", SqliteType.Text).Value = name;
                tableCmd.Parameters.Add("@MeasurementUnit", SqliteType.Text).Value = measurementUnit;
                tableCmd.Parameters.Add("@MeasurementValue", SqliteType.Real).Value = measurementValue;

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

            var habitId = GetNumberInput("\n\nPlease type Id of the habit you would like to update. Type 0 to return to main menu.");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var checkCmd = connection.CreateCommand();

                checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM habits WHERE Id = @Id)";
                checkCmd.Parameters.Add("@Id", SqliteType.Real).Value = habitId;

                int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (checkQuery == 0)
                {
                    Console.WriteLine($"\nHabit with Id {habitId} doesn't exist.\n");
                    Console.ReadKey();
                    connection.Close();
                    Update();
                    return;
                }

                string date = GetDateInput();

                string name = GetStringInput("Enter the name of your habit:");

                string measurementUnit = GetStringInput("Enter the measurement unit (km, minutes, pages)");

                double measurementValue = GetNumberInput("Enter the measurement value");

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"UPDATE habits SET date = @Date, name = @Name, measurementUnit = @MeasurementUnit, measurementValue = @MeasurementValue WHERE Id = @Id";

                tableCmd.Parameters.Add("@Id", SqliteType.Real).Value = habitId;
                tableCmd.Parameters.Add("@Date", SqliteType.Text).Value = date;
                tableCmd.Parameters.Add("@Name", SqliteType.Text).Value = name;
                tableCmd.Parameters.Add("@MeasurementUnit", SqliteType.Text).Value = measurementUnit;
                tableCmd.Parameters.Add("@MeasurementValue", SqliteType.Real).Value = measurementValue;

                tableCmd.ExecuteNonQuery();

                Console.WriteLine("\nRecord updated successfully.\n");
                Console.ReadKey();
            }
        }

        static void Delete()
        {
            Console.Clear();
            GetAllRecords();

            var habitId = GetNumberInput("\n\nType the Id of the habit you want to delete or press 0 to go back to the Main Menu");

            if (habitId == 0)
            {
                return;
            }

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText = $"DELETE from habits WHERE Id = @Id";
                tableCmd.Parameters.Add("@Id", SqliteType.Integer).Value = habitId;

                int rowCount = tableCmd.ExecuteNonQuery();

                if (rowCount == 0)
                {
                    Console.WriteLine($"\n\nHabit with Id {habitId} doesn't exist. \n\n");
                    Console.ReadKey();
                    Delete();
                    return;
                }
            }

            Console.WriteLine($"\n\nHabit with Id {habitId} was deleted. \n\n");
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

        static double GetNumberInput(string message)
        {
            Console.WriteLine(message);

            string numberInput = Console.ReadLine();

            if (numberInput == "0") GetUserInput();

            while (!double.TryParse(numberInput, out _))
            {
                Console.WriteLine("Invalid input. It needs to be a number");
                numberInput = Console.ReadLine();
            }

            double finalInput = Convert.ToDouble(numberInput);

            return finalInput;
        }

        static string GetStringInput(string message)
        {
            Console.WriteLine(message);

            string stringInput = Console.ReadLine();

            while (String.IsNullOrWhiteSpace(stringInput))
            {
                Console.WriteLine("Invalid string. Try again.");
                stringInput = Console.ReadLine();
            }

            return stringInput;
        }

        static string PreseedDB()
        {
            Random random = new Random();

            string date = "22-05-18";
            string[] name = { "Running", "Studying", "Reading", "Playing piano", "Boxing" };
            string unit = "minutes";

            List<string> values = new List<string>();

            for (int i = 1; i <= 100; i++)
            {
                int numBetween0And4 = random.Next(5);
                int numBetween1And60 = random.Next(1, 61);

                values.Add($"('{date}', '{name[numBetween0And4]}', '{unit}', {numBetween1And60})");
            }

            string str = "INSERT INTO habits (date, name, measurementUnit, measurementValue) VALUES " +
                string.Join(", ", values) + ";";

            return str;
        }
    }



    public class Habits
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string MeasurementUnit { get; set; }
        public double MeasurementValue { get; set; }

    }
    public class DrinkingWater
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
    }
}

