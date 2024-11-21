using System;
using System.Timers;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

public class Internos
{
    public int Numero {get; set; }

    public string Linea {get; set; }

    public double? Lat {get; set; }

    public double? Lng {get; set; }

    public bool isSleep {get; set; }

    public TimeSpan? timeSleep {get; set; }

    public bool onStartLine {get; set; }

    public bool Oculto {get; set; }

    public TimeSpan? timeOutOfTrack {get; set; }

    public Internos(int numero, string linea, double lat, double lng, bool oculto){
        
    }
}

