using System;

namespace KURSOVA
{
    public class TripMain
    {
        public string InfoAboutTrip { get; set; }

        public string StatusOfRequest { get; set; }
        public int Price { get; set; }
    }


    public class CityMain
    {
        public string CityName { get; set; }
        public string Coordinates { get; set; }


    }

    public class FavoriteTripMain
    {
        public string Trip { get; set; }
    }
}
