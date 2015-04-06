using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeltonikaParser
{
   public partial class Position
    {
        public int Id { get; set; }
        public string GpsTime { get; set; }
        public double La { get; set; }
        public double Lo { get; set; }
        public double Speed { get; set; }
        public string Status { get; set; }
        public double AlarmHandle { get; set; }
        public bool IsPointMsg { get; set; }
        public double Pointed { get; set; }
        public bool IsGetSetMsg { get; set; }
        public string SettingStr { get; set; }
        public double Direction { get; set; }
        public bool StopFlag { get; set; }
        public bool IsStop { get; set; }
        public double Mileage { get; set; }
        public double Temperature { get; set; }
        public double Fuel { get; set; }
        public string Input1 { get; set; }
        public string Input2 { get; set; }
        public string Input3 { get; set; }
        public string Input4 { get; set; }
        public string Input5 { get; set; }
        public string Output1 { get; set; }
        public string Output2 { get; set; }
        public string Output3 { get; set; }
        public string Output4 { get; set; }
        public string Output5 { get; set; }
        public string MNO { get; set; }
        public string Alarm { get; set; }
        public int CarId { get; set; }
        public Nullable<int> AlarmStatus { get; set; }
        public Nullable<System.DateTime> Time { get; set; }
    }
}
