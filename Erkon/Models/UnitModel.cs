using System;

namespace Erkon.Models
{
    public class UnitModel
    {
        public string Code { get; set; }
        public bool Status { get; set; }
        public string RoomNumber { get; set; }
        public string RoomLocation { get; set; }
        public float Temperature { get; set; }
        public float TemperatureAssigned { get; set; }
        public float Humidity { get; set; }
        public Int16 State { get; set; }
    }
}
