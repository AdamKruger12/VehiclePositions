using System.Diagnostics;

class VehiclePosition
{
    public int VehicleId { get; set; }
    public string VehicleRegistration { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime RecordedTimeUTC { get; set; }
}

class LatitudeLongitude
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

class Program
{
    private const string FILE_NAME = "..\\..\\..\\..\\VehiclePositions.dat";

    public static List<LatitudeLongitude> coordinateToCompare = new List<LatitudeLongitude>() {
      new LatitudeLongitude { Latitude = 34.544909 , Longitude = -102.10084},   //1
      new LatitudeLongitude { Latitude = 32.345544 , Longitude = -99.123124},   //2
      new LatitudeLongitude { Latitude = 33.234235 , Longitude = -100.214124},  //3
      new LatitudeLongitude { Latitude = 35.195739 , Longitude = -95.348899},   //4
      new LatitudeLongitude { Latitude = 31.895839 , Longitude = -97.789573},   //5
      new LatitudeLongitude { Latitude = 32.895839 , Longitude = -101.789573},  //6
      new LatitudeLongitude { Latitude = 34.115839 , Longitude = -100.225732},  //7
      new LatitudeLongitude { Latitude = 32.335839 , Longitude = -99.992232},   //8
      new LatitudeLongitude { Latitude = 33.535339 , Longitude = -94.792232},   //9
      new LatitudeLongitude { Latitude = 32.234235 , Longitude = -100.222222},  //10
    };

    static double minDistance = double.MaxValue;
    static List<VehiclePosition> closestVehicleList = new List<VehiclePosition>();
    static List<VehiclePosition> twoMilVehicleList = new List<VehiclePosition>();

    public static void Main()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        using (FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
        using (BinaryReader r = new BinaryReader(fs))
        {

            long lenght = fs.Length;

            for (; fs.Position + 16 <= fs.Length;)
            {
                int vehicleId = r.ReadInt32();
                string vehicleRegistration = ReadNullTerminatedString(r);
                float latitude = r.ReadSingle();
                float longitude = r.ReadSingle();
                ulong recordedTimeSeconds = r.ReadUInt64();
                DateTime referenceDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime recordedTimeUtc = referenceDateTime.AddSeconds(recordedTimeSeconds);
                VehiclePosition vehiclePosition = new VehiclePosition
                {
                    VehicleId = vehicleId,
                    VehicleRegistration = vehicleRegistration,
                    Latitude = latitude,
                    Longitude = longitude,
                    RecordedTimeUTC = recordedTimeUtc
                };
                for (int i = 0; i < coordinateToCompare.Count; i++)
                {
                    double distance = CalculateHaversineDistance(latitude, longitude, coordinateToCompare[i].Latitude, coordinateToCompare[i].Longitude);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestVehicleList.Add(vehiclePosition);
                    }
                }

            }

        }

        stopwatch.Stop();
        Console.WriteLine($"Reading time: {stopwatch.Elapsed}");
        Console.WriteLine($"");

        closestVehicleList.ForEach(x => Console.WriteLine($"Vehicle id: {x.VehicleId}  Vehicle registration: {x.VehicleRegistration}"));
    }

    private static string ReadNullTerminatedString(BinaryReader reader)
    {
        string result = string.Empty;
        char nextChar;
        while ((nextChar = reader.ReadChar()) != '\0')
        {
            result += nextChar;
        }
        return result;
    }

    //first attempt HaversineDistance
    public static double CalculateHaversineDistance(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        //according to nasa
        const double EarthRadiusKm = 6378;

        double degLat = DegreesToRadians(latitude2 - latitude1);
        double degLon = DegreesToRadians(longitude2 - longitude1);

        latitude1 = DegreesToRadians(latitude1);
        latitude2 = DegreesToRadians(latitude2);

        double a = Math.Sin(degLat / 2) * Math.Sin(degLat / 2) + Math.Sin(degLon / 2) * Math.Sin(degLon / 2) * Math.Cos(latitude1) * Math.Cos(latitude2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;

    }
    //simple maths
    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}