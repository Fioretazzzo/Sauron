using System;

public static class CoordinateUtils
{
    private const double EarthRadiusKm = 6371;

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    public static double CalculateDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        lat1 = DegreesToRadians(lat1);
        lat2 = DegreesToRadians(lat2);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c * 1000; // Convert to meters
    }

    public static bool IsNearCoordinate(double lat1, double lon1, double lat2, double lon2, double toleranceMeters = 50)
    {
        return CalculateDistanceInMeters(lat1, lon1, lat2, lon2) <= toleranceMeters;
    }
}