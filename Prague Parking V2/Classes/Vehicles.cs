using System.Text.Json.Serialization;

namespace Prague_Parking_V2.Classes
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Car), "Car")]
    [JsonDerivedType(typeof(MC), "MC")]
    public abstract class Vehicle
    {
        public string VehicleType { get; }
        private string _registry;
        public string Registry
        {
            get => _registry;
            set
            {
                if (value != null && !RegistryCheck(value))
                    throw new ArgumentException("Invalid Registry");
                _registry = value;
            }
        }
        public int VehicleSize { get; }
        public decimal VehiclePrice { get; }

        public Vehicle(string vehicleType, string registry)
        {
            this.VehicleType = vehicleType;
            this.Registry = registry;
            this.VehicleSize = GarageConfig.GetVehicleSize(vehicleType);
            this.VehiclePrice = GarageConfig.GetVehiclePrice(vehicleType);
        }
        private bool RegistryCheck(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 10;
    }
    class Car : Vehicle
    {
        public Car(string registry)
            : base("Car", registry)
        {
        }
    }
    class MC : Vehicle
    {
        public MC(string registry)
            : base("MC", registry)
        {
        }
    }
}