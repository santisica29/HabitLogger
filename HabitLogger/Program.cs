
using Microsoft.Data.Sqlite;

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
            //case 1:
            //    GetUserInput();
            //    break;
            case "2":
                Insert();
                break;
            //case 3:
            //    Delete();
            //    break;
            //case 4:
            //    Update();
            //    break;
            //default:
            //    Console.WriteLine("Invalid Command. Please type a number from 0 to 4.\n";
            //    break;
        }


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

static string GetDateInput()
{
    Console.WriteLine("Please insert the date: (Format: dd-mm-yy). Type 0 to return to main menu");

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