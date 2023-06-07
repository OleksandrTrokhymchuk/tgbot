using System;
using System.Collections.Generic;

namespace KURSOVA
{
    public class BlaBlaCarResponse
    {
        public string link { get; set; }
        public SearchInfo search_info { get; set; }
        public List<Trip> trips { get; set; }
        public string next_cursor { get; set; }
    }

    public class SearchInfo
    {
        public int count { get; set; }
        public int full_trip_count { get; set; }
    }

    public class Trip
    {
        public string link { get; set; }
        public List<Waypoint> waypoints { get; set; }
        public Price price { get; set; }
        public Vehicle vehicle { get; set; }
        public int distance_in_meters { get; set; }
        public int duration_in_seconds { get; set; }
    }

    public class Waypoint
    {
        public Place place { get; set; }
        public DateTime date_time { get; set; }
    }

    public class Place
    {
        public string city { get; set; }
        public string address { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string country_code { get; set; }
    }

    public class Price
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }

    public class Vehicle
    {
        public string make { get; set; }
        public string model { get; set; }
    }
}