using System;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace PriTraConsoleApp
{
    internal class Program
    {
        private MySqlConnection con;

        private static void Main(string[] args)
        {
            Program program = new Program();
            program.RunProgram();
        }

        public void RunProgram()
        {
            string[] art = new string[]
            {
        "██████╗ ██████╗ ██╗████████╗██████╗  █████╗ ",
        "██╔══██╗██╔══██╗██║╚══██╔══╝██╔══██╗██╔══██╗",
        "██████╔╝██████╔╝██║   ██║   ██████╔╝███████║",
        "██╔═══╝ ██╔══██╗██║   ██║   ██╔══██╗██╔══██║",
        "██║     ██║  ██║██║   ██║   ██║  ██║██║  ██║",
        "╚═╝     ╚═╝  ╚═╝╚═╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝",
        "                                            "
            };
            string border = new string('*', art[0].Length + 4);

            Console.WriteLine(border);
            foreach (string line in art)
            {
                Console.WriteLine("* " + line + " *");
            }
            Console.WriteLine(border);
            Console.WriteLine();

            while (true)
            {
                // Ensure the user connects to the database first
                bool isConnected = false;
                while (!isConnected)
                {
                    Console.WriteLine("******** PriTra APP *********\n");
                    Console.WriteLine("Press [C] Connect to Database");
                    Console.WriteLine("Press [X] Exit the Program");
                    Console.WriteLine("\n******************************\n");

                    string initialInput = Console.ReadLine()?.Trim().ToUpper();
                    if (string.IsNullOrEmpty(initialInput))
                    {
                        continue;
                    }

                    switch (initialInput)
                    {
                        case "C":
                            ConnectDB();
                            if (con.State == ConnectionState.Open)
                            {
                                isConnected = true;
                                Console.WriteLine("Successfully connected to the database.");
                            }
                            else
                            {
                                Console.WriteLine("Failed to connect to the database. Please try again.");
                            }
                            break;
                        case "X":
                            return; // Exit the program
                        default:
                            Console.WriteLine("Invalid option, please try again.");
                            break;
                    }
                }

                // Main menu loop
                while (con.State == ConnectionState.Open)
                {
                    Console.WriteLine("******** PriTra APP *********\n");
                    Console.WriteLine("Press [D] Disconnect from Database");
                    Console.WriteLine("Press [I] Insert Data Manually");
                    Console.WriteLine("Press [E] Execute Custom Queries");
                    Console.WriteLine("Press [P] Run Predefined Queries (A-T)");
                    Console.WriteLine("Press [T] Create Tables");
                    Console.WriteLine("Press [A] Alter Tables");
                    Console.WriteLine("Press [X] Exit the Program");
                    Console.WriteLine("\n******************************\n");

                    string userInput = Console.ReadLine()?.Trim().ToUpper();
                    if (string.IsNullOrEmpty(userInput))
                    {
                        continue;
                    }

                    switch (userInput)
                    {
                        case "D":
                            CloseDB();
                            Console.WriteLine("Disconnected from the database.");
                            isConnected = false;
                            break;
                        case "X":
                            return; // Exit the program
                        case "I":
                            InsertData();
                            break;
                        case "E":
                            Console.WriteLine("Enter the SQL query to execute:\n");
                            string query = Console.ReadLine();
                            if (!string.IsNullOrEmpty(query))
                            {
                                FetchData(query);
                            }
                            break;
                        case "P":
                            ExecuteTransactions();
                            break;
                        case "T":
                            Console.WriteLine("Enter the SQL query to create a table:\n");
                            string createTableQuery = Console.ReadLine()?.Trim();
                            CreateTables(createTableQuery);
                            break;
                        case "A":
                            AlterTable();
                            break;
                        default:
                            Console.WriteLine("Invalid option, please try again.\n");
                            break;
                    }
                }
            }
        }


        private void ConnectDB()
        {
            string conString = ConfigurationManager.AppSettings["conString"];
            con = new MySqlConnection(conString);
            try
            {
                con.Open();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"MySQL Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void CloseDB()
        {
            if (con != null && con.State == ConnectionState.Open)
            {
                con.Close();
            }
        }

        private void InsertData()
        {
            Console.WriteLine("Write custom Insert Queries statement:\n");
            string stringInsert = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(stringInsert))
            {
                Console.WriteLine("Invalid input. Please try again.");
                return;
            }

            try
            {
                // this will Ensure the connection is open before executing the command
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                }

                // Use a using statement to properly manage the resources
                using (MySqlCommand com = con.CreateCommand())
                {
                    com.CommandText = stringInsert;

                    com.ExecuteNonQuery();
                    Console.WriteLine("\nData inserted successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting data: {ex.Message}");
            }
       
        }

        // fethchData function for paramterized queries

        private void FetchData(MySqlCommand com)
        {
            try
            {
                Console.WriteLine("Executing query...");
                using (MySqlDataReader reader = com.ExecuteReader())
                {
                    Console.WriteLine("Query executed, processing results...");

                    // Determine the maximum width for each column for better padding
                    int[] columnWidths = new int[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        columnWidths[i] = reader.GetName(i).Length;
                    }

                    // Read through the data once to determine the maximum column widths
                    var rows = new List<string[]>();
                    while (reader.Read())
                    {
                        string[] row = new string[reader.FieldCount];
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[i] = reader[i].ToString();
                            if (row[i].Length > columnWidths[i])
                            {
                                columnWidths[i] = row[i].Length;
                            }
                        }
                        rows.Add(row);
                    }

                    // Print the column headers with padding
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Console.Write(reader.GetName(i).PadRight(columnWidths[i] + 2) + " ");
                    }
                    Console.WriteLine();
                    Console.WriteLine(new string('-', columnWidths.Sum() + (3 * reader.FieldCount)));

                    // Print the rows with padding
                    foreach (var row in rows)
                    {
                        for (int i = 0; i < row.Length; i++)
                        {
                            Console.Write(row[i].PadRight(columnWidths[i] + 2) + " ");
                        }
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data: {ex.Message}");
                Console.WriteLine($"Query: {com.CommandText}");
                foreach (MySqlParameter param in com.Parameters)
                {
                    Console.WriteLine($"{param.ParameterName} = {param.Value}");
                }
            }
        }

        // fethchData function for non-paramterized querie
        private void FetchData(string query)
        {
            MySqlCommand com = con.CreateCommand();
            com.CommandText = query;

            FetchData(com); // Reuse the parameterized FetchData method
        }


        private void CreateTables(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                Console.WriteLine("Invalid input. Please provide a valid SQL query.");
                return;
            }

            try
            {
                // Ensure the connection is open before executing the command
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                }

                // Extract the table name from the query
                string tableName = GetTableNameFromQuery(query);

                if (TableExists(tableName))
                {
                    Console.WriteLine($"Table '{tableName}' already exists.");
                    return;
                }

                // Use a using statement to properly manage the resources
                using (MySqlCommand com = con.CreateCommand())
                {
                    com.CommandText = query;
                    com.ExecuteNonQuery();
                    Console.WriteLine("Table created successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating table: {ex.Message}");
            }
        
        }

        private bool TableExists(string tableName)
        {
            string checkTableQuery = $"SHOW TABLES LIKE '{tableName}'";
            using (MySqlCommand cmd = new MySqlCommand(checkTableQuery, con))
            {
                object result = cmd.ExecuteScalar();
                return result != null;
            }
        }

        private string GetTableNameFromQuery(string query)
        {
            // This table name comes after "CREATE TABLE IF NOT EXISTS"
            // and is followed by whitespace or parentheses.
            string[] tokens = query.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int index = Array.IndexOf(tokens, "TABLE") + 1;
            if (index < tokens.Length)
            {
                return tokens[index].Trim('`', ' ', '\t', '\n', '\r');
            }
            return null;
        }

        private void AlterTable()
        {
            Console.WriteLine("Enter the SQL command to alter the table:\n");
            string alterTableQuery = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(alterTableQuery))
            {
                Console.WriteLine("Invalid input. Please provide a valid SQL query.");
                return;
            }

            try
            {
                // this will insure the connection is open before executing the command
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                }

                // Use a using statement to properly manage the resources
                using (MySqlCommand com = con.CreateCommand())
                {
                    com.CommandText = alterTableQuery;
                    com.ExecuteNonQuery();
                    Console.WriteLine("Table altered successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error altering table: {ex.Message}");
            }
      
        }


        private void ExecuteTransactions()
        {
            bool continueTransactions = true;
            while (continueTransactions)
            {
                Console.WriteLine("\n*** Choose an option (A - T) for execution: ***\n");
                Console.WriteLine("A. The names and phone numbers of the Managers at each office.");
                Console.WriteLine("B. The names of all female drivers based in the Grimstad office.");
                Console.WriteLine("C. The total number of staff at each office.");
                Console.WriteLine("D. The details of all cars at the Grimstad office.");
                Console.WriteLine("E. The total number of PD registered taxis.");
                Console.WriteLine("F. The number of drivers allocated to each car.");
                Console.WriteLine("G. The name and number of owners with more than one car.");
                Console.WriteLine("H. The full address of all business clients in Grimstad.");
                Console.WriteLine("I. The details of the current contracts with business clients in Grimstad.");
                Console.WriteLine("J. The total number of private clients in each city.");
                Console.WriteLine("K. The details ofjobs undertaken by a driver on a given day.");
                Console.WriteLine("L. The names of drivers who are over 55 years old.");
                Console.WriteLine("M. The names and numbers of private clients who hired a taxi in November 2020.");
                Console.WriteLine("N. The names and addresses of private clients who have hired a car more than three times.");
                Console.WriteLine("O. The average number of miles driven during a job.");
                Console.WriteLine("P. The total number ofjobs allocated to each car.");
                Console.WriteLine("Q. The total number ofjobs allocated to each driver.");
                Console.WriteLine("R. The total amount charged for each car in November 2020.");
                Console.WriteLine("S. The names of drivers who are over 55 years old.");
                Console.WriteLine("T. list ofjobs a particular driver has had for a particular owner, driving a particular business client.");
                Console.WriteLine("X. Exit\n");

                string userInput = Console.ReadLine()?.Trim().ToUpper();
                Console.WriteLine("\n");
                if (string.IsNullOrEmpty(userInput))
                {
                    continue;
                }
                // switch cases from A - T excute quries 
                switch (userInput)
                {
                    case "A":
                        string queryA = "SELECT o.Location, m.ManagerFName, m.ManagerTlf FROM Office o JOIN Manager m ON o.ManagerId = m.ManagerId;";
                        FetchData(queryA);
                        Console.WriteLine("\nQuery A executed");
                        break;
                    case "B":
                        string queryB = "SELECT DriverName\r\nFROM Driver\r\nWHERE DriverSex = 'Female' AND OfficeId = (\r\n    SELECT OfficeId\r\n    FROM Office\r\n    WHERE Location = 'Grimstad'\r\n);\r\n";
                        FetchData(queryB);
                        Console.WriteLine("\nQuery B executed");
                        break;
                    case "C":
                        string queryC = "SELECT o.Location, COUNT(s.StaffId) AS TotalStaff\r\nFROM Office o\r\nLEFT JOIN Staff s ON o.OfficeId = s.OfficeId\r\nGROUP BY o.OfficeId, o.Location\r\nHAVING TotalStaff > 0;";
                        FetchData(queryC);
                        Console.WriteLine("\nQuery C executed");
                        break;
                    case "D":
                        string queryD = "SELECT c.*\r\nFROM Cars c\r\nJOIN Driver d ON c.DriverId = d.DriverId\r\nJOIN Office o ON d.OfficeId = o.OfficeId\r\nWHERE o.Location = 'Grimstad';";
                        FetchData(queryD);
                        Console.WriteLine("\nQuery D executed");
                        break;
                    case "E":
                        string queryE = "SELECT COUNT(*) AS TotalPrivateDriverTaxis\r\nFROM Cars c\r\nJOIN Driver d ON c.DriverId = d.DriverId\r\nWHERE c.OwnershipType = 'Private';";
                        FetchData(queryE);
                        Console.WriteLine("\nQuery E executed");
                        break;
                    case "F":
                        string queryF = "SELECT \r\n    c.CarId,\r\n    c.LicencePlate,\r\n    c.Model,\r\n    COUNT(dcr.DriverId) AS NumberOfDrivers\r\nFROM \r\n    Cars c\r\nJOIN \r\n    DriverCarRelation dcr ON c.CarId = dcr.CarId\r\nGROUP BY \r\n    c.CarId, c.LicencePlate, c.Model;";
                        FetchData(queryF);
                        Console.WriteLine("\nQuery F executed");
                        break;
                    case "G":
                        string queryG = "SELECT OwnerName, COUNT(*) AS NumberOfCars\r\nFROM CarOwner\r\nJOIN Cars ON CarOwner.OwnerId = Cars.OwnerId\r\nGROUP BY OwnerName\r\nHAVING COUNT(*) > 1;";
                        FetchData(queryG);
                        Console.WriteLine("\nQuery G executed");
                        break;
                    case "H":  
                        string queryH = "SELECT \r\n    BuisnessClientsName,\r\n    BuisnessClientsGateNum,\r\n    BuisnessClientsPostNum,\r\n    BuisnessClientsPostSted\r\nFROM \r\n    BuisnessClients\r\nWHERE \r\n    BuisnessClientsPostSted = 'Grimstad';";
                        FetchData(queryH);
                        Console.WriteLine("\nQuery H executed");
                        break; 
                    case "I":
                        string queryI = "SELECT * \r\nFROM Contract\r\nWHERE OfficeId = 6 -- Grimstad office\r\nAND ContractStatus = 'Active' -- Only current contracts\r\nAND ContractType = 'Formal'; -- Only contracts with business client";
                        FetchData(queryI);
                        Console.WriteLine("\nQuery I executed");
                        break;
                    case "J":
                        string queryJ = "SELECT PrivateClientsClientsPostSted AS City, COUNT(*) AS TotalPrivateClients\r\nFROM PrivateClients\r\nGROUP BY PrivateClientsClientsPostSted;";
                        FetchData(queryJ);
                        Console.WriteLine("\nQuery J executed");
                        break;
                    case "K":
                        string queryK = "SELECT \r\n    j.*,\r\n    D.DriverName\r\nFROM \r\n    Job j\r\nJOIN \r\n    Cars c ON j.CarId = c.CarId\r\nJOIN \r\n    Driver D ON c.DriverId = D.DriverId\r\nORDER BY \r\n    D.DriverName, j.JobDate;";
                        FetchData(queryK);
                        Console.WriteLine("\nQuery K executed");
                        break;
                    case "L":
                        string queryL = "SELECT \r\n    DriverName,\r\n    DriverAge\r\nFROM \r\n    Driver\r\nWHERE \r\n    DriverAge > 55;";
                        FetchData(queryL);
                        Console.WriteLine("\nQuery L executed");
                        break;
                    case "M":
                        string queryM = "SELECT\r\n    PrivateClients.PrivateClientsName,\r\n    PrivateClients.PrivateClientsClientsGateNum\r\nFROM\r\n    Job\r\nINNER JOIN\r\n    PrivateClients ON Job.PrivateClientsId = PrivateClients.PrivateClientsId\r\nWHERE\r\n    Job.JobDate BETWEEN '2020-11-01' AND '2020-11-30';";
                        FetchData(queryM);
                        Console.WriteLine("\nQuery M executed");
                        break;
                    case "N":
                        string queryN = "SELECT\r\n    PrivateClients.PrivateClientsName,\r\n    PrivateClients.PrivateClientsClientsGateNum,\r\n    PrivateClients.PrivateClientsClientsPostNum,\r\n    PrivateClients.PrivateClientsClientsPostSted\r\nFROM\r\n    Job\r\nINNER JOIN\r\n    PrivateClients ON Job.PrivateClientsId = PrivateClients.PrivateClientsId\r\nGROUP BY\r\n    Job.PrivateClientsId,\r\n    PrivateClients.PrivateClientsName,\r\n    PrivateClients.PrivateClientsClientsGateNum,\r\n    PrivateClients.PrivateClientsClientsPostNum,\r\n    PrivateClients.PrivateClientsClientsPostSted\r\nHAVING\r\n    COUNT(Job.JobId) > 3;";
                        FetchData(queryN);
                        Console.WriteLine("\nQuery N executed");
                        break;
                    case "O":
                        string queryO = " SELECT AVG(MilesDriven) AS AverageMilesDriven\r\nFROM Job;";
                        FetchData(queryO);
                        Console.WriteLine("\nQuery O executed");
                        break;
                    case "P":
                        string queryP = "SELECT CarId, COUNT(JobId) AS TotalJobs\r\nFROM Job\r\nGROUP BY CarId;";
                        FetchData(queryP);
                        Console.WriteLine("\nQuery P executed");
                        break;
                    case "Q":
                        string queryQ = "SELECT d.DriverName, COUNT(j.JobId) AS TotalJobsAllocated\r\nFROM Driver d\r\nJOIN Cars c ON d.DriverId = c.DriverId\r\nJOIN Job j ON c.CarId = j.CarId\r\nGROUP BY d.DriverId;";
                        FetchData(queryQ);
                        Console.WriteLine("\nQuery Q executed");
                        break;
                    case "R":
                        string queryR = "SELECT c.LicencePlate, SUM(j.Price) AS TotalAmountCharged\r\nFROM Cars c\r\nJOIN Job j ON c.CarId = j.CarId\r\nWHERE YEAR(j.JobDate) = 2020 AND MONTH(j.JobDate) = 11\r\nGROUP BY c.CarId;";
                        FetchData(queryR);
                        Console.WriteLine("\nQuery R executed");
                        break;
                    case "S":
                        string queryS = "SELECT c.ContractId, \r\n       COUNT(j.JobId) AS TotalJobs,\r\n       SUM(j.MilesDriven) AS TotalMilesDriven\r\nFROM Contract c\r\nLEFT JOIN Job j ON c.ContractId = j.ContractId\r\n\tGROUP BY c.ContractId;";
                        FetchData(queryS);
                        Console.WriteLine("\nQuery S executed");
                        break;
                    case "T":
                        bool cont = true;
                        while (cont)
                        {
                            Console.WriteLine("********************************");
                            Console.WriteLine("Press [A] excute predifined qurie for Option T ");
                            Console.WriteLine("Press [B] to manully search ");
                            Console.WriteLine("Press [X] Exit the Program");
                            Console.WriteLine("******************************\n");

                            string initialInput = Console.ReadLine()?.Trim().ToUpper();
                            if (string.IsNullOrEmpty(initialInput))
                            {
                                continue;
                            }


                            switch (initialInput)
                            {

                                case "A":
                                    string queryAT = "SELECT \r\n    d.DriverName,\r\n    d.DriverId,\r\n    co.OwnerName,\r\n    co.OwnerId,\r\n    bc.BuisnessClientsName,\r\n    bc.BuisnessClientsId,\r\n    j.JobId,\r\n    c.LicencePlate,\r\n    c.Model\r\nFROM \r\n    Driver d\r\nJOIN \r\n    DriverCarRelation dcr ON d.DriverId = dcr.DriverId\r\nJOIN \r\n    Cars c ON dcr.CarId = c.CarId\r\nJOIN \r\n    CarOwner co ON c.OwnerId = co.OwnerId\r\nJOIN \r\n    Job j ON c.CarId = j.CarId\r\nJOIN \r\n    Contract ct ON j.ContractId = ct.ContractId\r\nJOIN \r\n    BuisnessClients bc ON ct.BuisnessClientsId = bc.BuisnessClientsId\r\nWHERE \r\n    bc.BuisnessClientsName IS NOT NULL\r\nORDER BY \r\n    d.DriverName, co.OwnerName, bc.BuisnessClientsName, j.JobDate;\r\n";
                                    FetchData(queryAT);
                                    Console.WriteLine("Query T executed");
                                    break;
                                case "B":
                                    Console.WriteLine("Enter driver id: ");
                                    string strDriverID = Console.ReadLine()?.Trim();
                                    Console.WriteLine("Enter Car owner id: ");
                                    string strOwnerID = Console.ReadLine()?.Trim();
                                    Console.WriteLine("Enter BusinessClient id: ");
                                    string strBusinessClientID = Console.ReadLine()?.Trim();

                                    string queryBT = "SELECT d.DriverId, d.DriverName, o.OwnerId, o.OwnerName, b.BuisnessClientsId, b.BuisnessClientsName, j.JobId, j.JobDate, c.LicencePlate, c.Model FROM Job j JOIN Cars c ON j.CarId = c.CarId JOIN Driver d ON c.DriverId = d.DriverId JOIN CarOwner o ON c.OwnerId = o.OwnerId JOIN Contract co ON j.ContractId = co.ContractId JOIN BuisnessClients b ON co.BuisnessClientsId = b.BuisnessClientsId WHERE d.DriverId = @DriverId AND o.OwnerId = @OwnerId AND b.BuisnessClientsId = @BusinessClientId;";

                                    // Create a MySqlCommand object
                                    MySqlCommand com = con.CreateCommand();
                                    com.CommandText = queryBT;

                                    // Add parameters to the command
                                    com.Parameters.AddWithValue("@DriverId", strDriverID);
                                    com.Parameters.AddWithValue("@OwnerId", strOwnerID);
                                    com.Parameters.AddWithValue("@BusinessClientId", strBusinessClientID);

                                    FetchData(com);
                                    Console.WriteLine("Query T executed");
                                    break;
                                case "X":
                                    cont = false;
                                    break;
                            }
                        }                      
                        break;
                    case "X":
                        continueTransactions = false;
                        Console.WriteLine("Exited Option menu");
                        break;
                    default:
                        Console.WriteLine("Invalid option, please try again.");
                        break;
                }
            }
        }
    }
}
