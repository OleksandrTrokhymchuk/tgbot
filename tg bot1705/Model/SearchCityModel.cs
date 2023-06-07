using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KURSOVA
{
    public class Response
    {
        public List<Result> Results { get; set; }
        public string Status { get; set; }
    }

    public class AddressComponent
    {
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public List<string> Types { get; set; }
    }

    public class GeometryBoundsLocation
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class GeometryBounds
    {
        public GeometryBoundsLocation Northeast { get; set; }
        public GeometryBoundsLocation Southwest { get; set; }
    }

    public class Geometry
    {
        public GeometryBounds Bounds { get; set; }
        public GeometryBoundsLocation Location { get; set; }
        public string LocationType { get; set; }
        public GeometryBounds Viewport { get; set; }
    }

    public class Result
    {
        public List<AddressComponent> AddressComponents { get; set; }
        public string FormattedAddress { get; set; }
        public Geometry Geometry { get; set; }
        public string PlaceId { get; set; }
        public List<string> Types { get; set; }
    }
}
