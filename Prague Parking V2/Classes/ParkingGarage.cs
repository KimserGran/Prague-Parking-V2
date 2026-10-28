using Spectre.Console;

namespace Prague_Parking_V2.Classes
{
    static class ParkingGarage
    {
        public static List<ParkingEntry> ParkedVehicles { get; set; } = new();
        public static List<ParkingSpot> ParkingSpots { get; set; } = new();

        public static void ParkingGrid()
        {

            foreach (var spot in ParkingSpots)
            {
                if (spot == null) continue;
                string text = spot.IsEmpty() ? "FREE"
                    : spot.IsFull() ? string.Join("/", spot.ReturnParkedVehicles())
                    : string.Join("/", spot.ReturnParkedVehicles());
                string color = spot.IsEmpty() ? "green" : spot.IsFull() ? "red" : "yellow";
                AnsiConsole.Markup($"[{color}]|{Markup.Escape(text),-5}|[/]");


                if (spot.DesignatedSpot % 10 == 0) AnsiConsole.WriteLine();
                if (spot.DesignatedSpot % 20 == 0) AnsiConsole.WriteLine("\n");
            }
        }
        /// <summary>
        /// makes sure the selected spot fits into the size of the garage
        /// </summary>
        public static bool IsValidSpot(ParkingSpot p) => p is not null && ParkingSpots.Contains(p);
        public static bool IsValidNumberSpot(int n) => n > 0 && n <= GarageConfig.GetMaxAllowedParkingSpots();
        /// <summary>
        /// Auto-finds the first spot it can find, if nothing can be found then returns null;
        /// </summary>
        public static ParkingSpot? FindAvailableSpot(Vehicle vehicle)
        {
            if(vehicle.VehicleType== "MC")
                return ParkingSpots.Find(s => s != null && s.CanVehicleFit(vehicle) && (!s.IsEmpty() && !s.IsFull())) ?? ParkingSpots.Find(s => s != null && s.CanVehicleFit(vehicle)) ?? null;
            return ParkingSpots.Find(s => s != null && s.CanVehicleFit(vehicle)) ?? null;
        }
        public static bool RemoveMatchingVehicle(string reg)
        {
            var entry = ParkedVehicles.FirstOrDefault(e => string.Equals(e.Vehicle.Registry, reg, StringComparison.OrdinalIgnoreCase));

            if (entry == null) return false;
            PayParkingFee(entry);

            entry.ParkingSpot.Parked_Vehicles.Remove(entry.Vehicle);
            ParkedVehicles.Remove(entry);
            JsonManager.Serialize(ParkedVehicles, JsonManager.ParkingEntriesFileName());
            return true;
        }
        static void PayParkingFee(ParkingEntry entry)
        {
            DateTime departure = DateTime.Now;
            TimeSpan difference = departure - DateTime.Parse(entry.Time);
            decimal diff = (decimal)difference.TotalMinutes;
            decimal price = diff switch
            {
                < 10 => 0m,
                >= 10 and <= 60 => entry.Vehicle.VehiclePrice,
                > 60 => (decimal)difference.TotalHours * entry.Vehicle.VehiclePrice,
            };
            Console.WriteLine($"Your Price todauy will be {price} CZK");
            string payMethod = Program.PromptSelectionWithChoices("Please select method of paying", "Card", "Cash");
            Console.Clear();
            Console.WriteLine("Processing");
            for (int i = 0; i <= 5; i++)
            {
                Thread.Sleep(1000);
                Console.WriteLine("...");
            }
            Console.WriteLine("Payment method accepted, Thank you for using our garage!");
            Console.ReadKey(true);
            Program.CloseApplication();

        }
        /// <summary>
        /// Creates max parking spots where max = max allowed parkingspots
        /// </summary>
        public static void LoadParkingSpots(int max)
        {
            ParkingSpots.Add(null);
            for (int x = 1; x <= max; x++)
                ParkingSpots.Add(new ParkingSpot(x));
        }
        public static void CreateEntryAndParkVehicle(Vehicle vehicle, ParkingSpot parkingSpot)
        {
            parkingSpot.ParkVehicle(vehicle);
            ParkedVehicles.Add(new(parkingSpot, vehicle));
            JsonManager.Serialize(ParkedVehicles, JsonManager.ParkingEntriesFileName());
        }
        public static void ClearGarage()
        {
            Console.Clear();
            string choice = Program.PromptSelectionWithChoices("Warning: this can not be undone, continue?", "Yes", "No go back");
            if (choice == "Yes")
            {
                foreach (var v in ParkedVehicles)
                {
                    v.ParkingSpot.ClearVehicles();
                }
                ParkedVehicles.Clear();

            }
            if (choice == "No go back")
            {
                return;
            }
        }
        public static void ClearAll()
        {
            Console.Clear();
            string val = Program.PromptSelectionWithChoices("Here you can clear both the garage and the file holding all parked vehicles", "Clear Garage", "Clear everything");
            switch (val)
            {
                case "Clear Garage":
                    ParkingGarage.ClearGarage();
                    break;
                case "Clear everything and restart":
                    ClearFiles();
                    Program.CloseApplicationAndRestart();
                    break;
            }
        }
        public static void ClearFiles()
        {
            Console.Clear();
            string choice = Program.PromptSelectionWithChoices("Warning: this can not be undone, continue?", "Yes", "No go back");
            if (choice == "Yes")
            {
                foreach (string file in Directory.GetFiles(JsonManager.Filepath()))
                {
                    File.Delete(file);
                }
                foreach (var v in ParkedVehicles)
                {
                    v.ParkingSpot.ClearVehicles();
                }
                ParkedVehicles.Clear();

            }
            if (choice == "No go back")
            {
                return;
            }

        }
    }

}