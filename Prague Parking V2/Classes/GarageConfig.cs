namespace Prague_Parking_V2.Classes
{
    public record GarageConfig
    {
        public int MaxAllowedParkingSpots { get; init; }
        public int ParkingSpotsSize { get; init; }
        public List<VehicleConfig> VehicleConfigList { get; init; } = new();

        public static GarageConfig CurrentValues = CreateDefaultConfig();

        public static GarageConfig CreateDefaultConfig() => new()
        {
            MaxAllowedParkingSpots = 100,
            ParkingSpotsSize = 10,

            VehicleConfigList = new List<VehicleConfig>
            {
                new() { VehicleType = "Car", VehicleSize = 10, VehiclePrice = 20m },
                new() { VehicleType = "MC", VehicleSize = 5, VehiclePrice = 10 },
            }
        };
        public static void LoadExistingConfig(GarageConfig currentValues)
        {
            if (currentValues != null)
                CurrentValues = currentValues;
        }
        public static int GetMaxAllowedParkingSpots() => CurrentValues.MaxAllowedParkingSpots;
        public static string[] GetVehicleTypes() => CurrentValues.VehicleConfigList.Select(x => x.VehicleType).ToArray();
        public static decimal GetVehiclePrice(string v) => CurrentValues.VehicleConfigList.Find(x => x.VehicleType == v)?.VehiclePrice ?? -1;
        public static int GetVehicleSize(string v) => CurrentValues.VehicleConfigList.Find(x => x.VehicleType == v)?.VehicleSize ?? -1;
    }
}
