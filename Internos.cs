using System;
using System.Timers;
using System.Diagnostics;
using System.Collections.Generic;


public class Internos
{
    public string Numero {get; set; }

    public string Linea {get; set; }

    public Coodinates[] Coord {get; set; }

    public bool OnBase {get; set; }

    public DateTime? TimeSleep {get; set; }

    public bool? OnStartLine {get; set; }

    public bool Oculto {get; set; }

    public string ServiceStatus {get; set; }

    public DateTime? TimeOutOfTrack {get; set; }

    public int Id {get; set; }

    public Internos(string numero, string linea, bool oculto, string ServiceStatus, double? lat, double? lng, int Id){
        this.Numero = numero;
        this.Linea = linea;
        this.Oculto = oculto;
        this.ServiceStatus = ServiceStatus;
        this.Id = Id;
        Coord = new Coodinates[3];
        
        Coord[0] = new Coodinates
        {
            Lat = lat,
            Long = lng
        };
    }
}


public class Coodinates {
    public double? Lat {get; set; }

    public double? Long {get; set; }
    
}

