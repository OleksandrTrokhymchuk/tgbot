using System;
namespace KURSOVA
{
    public class UserParams
    {
        public string operation { get; set; }
        public string requestByUser { get; set; }
        public int numberOfFirstMonth { get; set; }
        public int numberOfLastMonth { get; set; }
        public string monthOnConcreteDateOnTrip { get; set; }
        public string dayOnConcreteDateOnTrip { get; set; }
        public string monthOnFirstDateOnTrip { get; set; }
        public string dayOnFirstDateOnTrip { get; set; }
        public string monthOnLastDateOnTrip { get; set; }
        public string dayOnLastDateOnTrip { get; set; }
        public string dataForSavingTrip { get; set; }
        public string StartCity { get; set; }
        public string FinishCity { get; set; }
        public string favotireTrip { get; set; }
        public List<string> numberOfTrip = new List<string>();

        public string coor1 { get; set; }
        public string coor2 { get; set; }
        public string DateTimeOnConcreteDateOnTrip { get; set; }

        /// <summary>
        /// ////////////////////////////
        /// </summary>
        public string DateTimeOnFirstDateOnTrip { get; set; }
        public string DateTimeOnLastDateOnTrip { get; set; }
        public string CityName { get; set; }
        public string Сoordinates { get; set; }
    }
}
