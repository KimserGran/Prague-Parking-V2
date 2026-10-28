using Prague_Parking_V2.Classes;
using Spectre.Console;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
namespace Prague_Parking_V2
{
    partial class Program
    {
        public static Dictionary<int, ParkingEntry> EntryID = new();
        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = true };
        static void Main(string[] args)
        {
            StartUp();
            MainMenu();
        }
        static void MainMenu()
        {
            while (true)
            {
                Console.Clear();
                string choice = PromptSelectionWithChoices("Welcome to the garage!", ["Park your vehicle", "Take out your vehicle", "Parking Grid", "Admin menu","Exit"]);
                switch (choice)
                {
                    case "Park your vehicle":
                        ParkVehicleAndCreateEntry();
                        break;
                    case "Take out your vehicle":
                        TakeVehicleAndEntryOut();
                        break;
                    case "Parking Grid":
                        ParkingGarage.ParkingGrid();
                        Console.ReadKey(true);
                        break;
                    case "Admin menu":
                        AdminMenu();
                        break;
                    case "Exit":
                        CloseApplication();
                        break;
                }
            }
        }
        static void ParkVehicleAndCreateEntry()
        {
            var options = new[] { "Go Back" }.Concat(GarageConfig.GetVehicleTypes()).ToArray();
            string vehicleType = PromptSelectionWithChoices("Please select the type of vehicle you want to park", options);

            if (vehicleType == "Go Back")
                return;

            string vehicleReg = TextPromptWithValidationAndReturn<string>("Please type you vehicle registration with no spaces or special symbols",
                r => r.Length is > 0 and <= 10 && r.All(char.IsLetterOrDigit), "Please enter a registration with at most 10 characters and no special characters");

            Vehicle vehicle = vehicleType switch
            {
                "Car" => new Car(vehicleReg),
                "MC" => new MC(vehicleReg),
                _ => throw new ArgumentException("Error: Unknown vehicle type")
            };
            var spot = ChooseParkingSpot(vehicle);
            if (spot == null)
            {
                Console.WriteLine("Something went wrong please try again");
                return;
            }
            ParkingGarage.CreateEntryAndParkVehicle(vehicle, spot);
            return;
        }
        static void TakeVehicleAndEntryOut()
        {
            string result = TextPromptWithValidationAndReturn<string>("Please enter your registry to take out your vehicle, ",
                r => r == string.Empty || ParkingGarage.RemoveMatchingVehicle(r) == true, "Either wrong register or vehicle does not exist.");
        }
        static void AdminMenu()
        {
            string val = PromptSelectionWithChoices("Admin menu", "How to change config guide", "Close the garage", "Back");
            switch (val)
            {
                case "How to change config guide":
                    ConfigChoice();
                    break;
                case "Close the garage":
                    ParkingGarage.ClearAll();
                    break;
                case "Back":
                    return;
                    break;
            }
        }
        static void ConfigChoice()
        {
            HowToConfig howTo = new();
            string choice = PromptSelectionWithChoices(howTo.SendOpeningTitle(), "How to change prices and other settings", "Open config file", "Go back");

            switch (choice)
            {
                case "How to change prices and other settings":
                    Console.WriteLine(howTo.SendHowTo());
                    Console.ReadKey(true);
                    break;
                case "Open config file":
                    JsonManager.OpenConfig();
                    Console.ReadKey(true);
                    break;
                case "Go back":
                    return;
            }
        }
        /// <summary>
        /// Prompts the user to select a vehicle type and enter a registration, then parks the vehicle and creates a
        /// parking entry.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when an unknown vehicle type is selected.</exception>
        static ParkingSpot? ChooseParkingSpot(Vehicle v)
        {
            ParkingGarage.ParkingGrid();

            string choice = PromptSelectionWithChoices(
                "Do you want to choose a spot or have one auto-selected?",
                "Choose my own spot", "Auto-Select a spot for me");

            if (choice == "Choose my own spot")
            {
                var spot = ChooseOwnSpot(v);
                if (spot == null)
                    return null;
                return spot;
            }
            if (choice == "Auto-Select a spot for me")
            {
                var spot = AutoSelectSpot(v);
                if (spot == null)
                    return null;
                return spot;
            }
            return null;
        }
        /// <summary>
        /// Selects an available parking spot for the specified vehicle, if one exists.
        /// </summary>
        /// <param name="v">The vehicle for which to find an available parking spot.</param>
        /// <returns>A suitable ParkingSpot if available; otherwise, null.</returns>
        static ParkingSpot? AutoSelectSpot(Vehicle v)
        {
            var spot = ParkingGarage.FindAvailableSpot(v);
            if (spot == null)
                return null;
            return spot;
        }
        /// <summary>
        /// Prompts the user to select a parking spot for the specified vehicle, validating the selection and returning
        /// the chosen spot or null if the user exits.
        /// </summary>
        /// <param name="v">The vehicle for which to choose a parking spot.</param>
        /// <returns>The selected ParkingSpot if a valid spot is chosen; otherwise, null.</returns>
        static ParkingSpot? ChooseOwnSpot(Vehicle v)
        {
            int spot = TextPromptWithValidationAndReturn<int>("Please type a desired spot or type 0 to exit",
             spot => spot == 0 || (ParkingGarage.IsValidNumberSpot(spot) &&
             ParkingGarage.IsValidSpot(ParkingGarage.ParkingSpots[spot]) &&
             ParkingGarage.ParkingSpots[spot].CanVehicleFit(v)),
             "That spot will not work, try another one.");
            if (spot == 0)
                return null;
            return ParkingGarage.ParkingSpots[spot];
        }

        //////////////// Extra methods
        static void StartUp()
        {
            bool AlreadyStarted = false;
            while (AlreadyStarted == false)
            {
                ChangeCultureInfo();
                ParkingGarage.LoadParkingSpots(GarageConfig.GetMaxAllowedParkingSpots());
                JsonManager.StartUp();
                AlreadyStarted = true;
            }
        }
        static void ChangeCultureInfo()
        {
            var culture = new CultureInfo("en-GB");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.CurrentCulture = culture;
        }

        /// <summary>
        /// Selection prompt that allows a titel string and any amount of string choices to be displayed
        /// </summary>
        /// <param name="titel">param>titel of the prompt.</param>
        /// <param name="choices">selection of choices, choice will be returned as string</param>
        /// <returns></returns>
        public static string PromptSelectionWithChoices(string titel, params string[] choices)
        {
            return AnsiConsole.Prompt(new SelectionPrompt<string>()
                 .Title(titel)
                 .AddChoices(choices));
        }
        /// <summary>
        /// Prompts the user for input, validates the input using a specified condition, and returns the validated
        /// value.
        /// </summary>
        /// <typeparam name="T">The type of the value to prompt for and validate.</typeparam>
        /// <param name="titel">The prompt message displayed to the user.</param>
        /// <param name="checkCondition">A function that determines whether the input value is valid.</param>
        /// <param name="errorMessage">The error message displayed when validation fails.</param>
        /// <returns>The validated value entered by the user.</returns>
        public static T TextPromptWithValidationAndReturn<T>(string titel, Func<T, bool> checkCondition, string errorMessage)
        {
            int attempts = 0;
            int maxAttempts = 3;
            return AnsiConsole.Prompt(new TextPrompt<T>(titel + Environment.NewLine + "> ")
                .Validate(value =>
                {
                    if (checkCondition(value))
                        return ValidationResult.Success();

                    attempts++;

                    if (attempts >= maxAttempts)
                    {
                        Console.WriteLine("You entered the wrong registry too many times application will now close in 5 seconds.");
                        DateTime endTime = DateTime.Now.AddSeconds(5);

                        while (DateTime.Now < endTime)
                        {
                            while (Console.KeyAvailable)
                            {
                                Console.ReadKey(true);
                            }
                            Thread.Sleep(50);
                        }
                        Program.CloseApplication();
                    }
                    Console.Clear();
                    return ValidationResult.Error(errorMessage);
                }));
        }
        public static void CloseApplication()
        {
            Environment.Exit(0);
        }
        public static void CloseApplicationAndRestart()
        {
            Process.Start(Environment.ProcessPath!);
            Environment.Exit(0);
        }
    }
}
