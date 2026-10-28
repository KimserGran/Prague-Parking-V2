using System.Text.Json.Serialization;

namespace Prague_Parking_V2.Classes
{

    /// <summary>
    /// Creates a parkingspot object containing DesignatedSpot, 
    /// </summary>
    public class ParkingSpot
    {
        public int DesignatedSpot { get; init; }
        [JsonIgnore]
        public List<Vehicle> Parked_Vehicles { get; } = new();
        [JsonIgnore]
        public int MaxCapacity { get; init; } = GarageConfig.CurrentValues.ParkingSpotsSize;
        [JsonIgnore]
        public int CurrentCapacity => Parked_Vehicles.Sum(v => v.VehicleSize);

        public ParkingSpot(int DesignatedSpot)
        {
            this.DesignatedSpot = DesignatedSpot;
        }
        /// <summary>
        /// Returns a string array of parked vehicles.
        /// </summary>
        /// <returns></returns>
        public string[]? ReturnParkedVehicles() => !IsEmpty()
            ? Parked_Vehicles.Select(v => $"{v.VehicleType}").ToArray()
            : null;
        public bool IsEmpty() => CurrentCapacity == 0;
        public bool IsFull() => CurrentCapacity == MaxCapacity;
        public void ClearVehicles() => Parked_Vehicles.Clear();
        /// <summary>
        /// Checks if vehicle can fit, if true adds it to parked_vehicles list
        /// </summary>
        public int? ParkVehicle(Vehicle V)
        {
            if(CanVehicleFit(V))
            {
                Parked_Vehicles.Add(V);
                return DesignatedSpot;
            }
            return null;
        }
        public bool CanVehicleFit(Vehicle vehicle) => CurrentCapacity + vehicle.VehicleSize <= MaxCapacity;
    }
}
