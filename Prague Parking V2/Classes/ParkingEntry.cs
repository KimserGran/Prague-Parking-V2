using System.Text.Json.Serialization;

namespace Prague_Parking_V2.Classes
{

    /// <summary>
    /// Represents an entry recording a vehicle parked in a specific parking spot.
    /// </summary>
    public record ParkingEntry
    {
        public ParkingSpot ParkingSpot { get; init; }
        public Vehicle Vehicle { get; init; }
        public string Time { get; init; } = DateTime.Now.ToString("g");
        public ParkingEntry(ParkingSpot parkingSpot, Vehicle vehicle)
        {
            this.ParkingSpot = parkingSpot;
            this.Vehicle = vehicle;
        }

    }
}
