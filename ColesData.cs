using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

// Root object
public class ColesData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("ramal_id")]
    public int RamalId { get; set; }

    [JsonPropertyName("ramal")]
    public Ramal Ramal { get; set; }

    [JsonPropertyName("trackers")]
    public List<Tracker> Trackers { get; set; }

    [JsonPropertyName("recorrido")]
    public Recorrido Recorrido { get; set; }

    [JsonPropertyName("recorrido_id")]
    public int? RecorridoId { get; set; }

    [JsonPropertyName("active_trackers")]
    public int ActiveTrackers { get; set; }

    [JsonPropertyName("number")]
    public string Number { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("lat")]
    public double? Lat { get; set; }

    [JsonPropertyName("lng")]
    public double? Lng { get; set; }

    [JsonPropertyName("fixed_position")]
    public List<string> FixedPosition { get; set; }

    [JsonPropertyName("estimated_time")]
    public object EstimatedTime { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("speed")]
    public int? Speed { get; set; }

    [JsonPropertyName("course")]
    public int? Course { get; set; }

    [JsonPropertyName("contador_fuera_recorrido")]
    public int ContadorFueraRecorrido { get; set; }

    [JsonPropertyName("cant_min_detenido")]
    public int CantMinDetenido { get; set; }

    [JsonPropertyName("activo_en_app")]
    public bool ActivoEnApp { get; set; }

    [JsonPropertyName("distance")]
    public object Distance { get; set; }

    [JsonPropertyName("status_updated_at")]
    public string StatusUpdatedAt { get; set; }

    [JsonPropertyName("started_at")]
    public string StartedAt { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; }

    [JsonPropertyName("deleted_at")]
    public object DeletedAt { get; set; }
}

// Ramal object
public class Ramal
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("linea_id")]
    public int LineaId { get; set; }

    [JsonPropertyName("recorrido_id")]
    public object RecorridoId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("short_name")]
    public string ShortName { get; set; }

    [JsonPropertyName("description")]
    public object Description { get; set; }

    [JsonPropertyName("color")]
    public object Color { get; set; }

    [JsonPropertyName("active")]
    public int Active { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; }

    [JsonPropertyName("deleted_at")]
    public object DeletedAt { get; set; }

    [JsonPropertyName("linea")]
    public Linea Linea { get; set; }
}

// Linea object
public class Linea
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("empresa_id")]
    public int EmpresaId { get; set; }

    [JsonPropertyName("empresa")]
    public Empresa Empresa { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("short_name")]
    public string ShortName { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; }

    [JsonPropertyName("deleted_at")]
    public object DeletedAt { get; set; }
}

// Empresa object
public class Empresa
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("short_name")]
    public string ShortName { get; set; }

    [JsonPropertyName("color")]
    public object Color { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; }

    [JsonPropertyName("deleted_at")]
    public object DeletedAt { get; set; }
}

// Tracker object
public class Tracker
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("tracker_model")]
    public string TrackerModel { get; set; }

    [JsonPropertyName("tracker_id")]
    public string TrackerId { get; set; }

    [JsonPropertyName("procesar_en_server")]
    public bool ProcesarEnServer { get; set; }

    [JsonPropertyName("es_active_tracker")]
    public bool EsActiveTracker { get; set; }

    [JsonPropertyName("interno_id")]
    public int InternoId { get; set; }

    [JsonPropertyName("interno")]
    public Interno Interno { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; }

    [JsonPropertyName("deleted_at")]
    public object DeletedAt { get; set; }
}

// Interno object
public class Interno
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("ramal_id")]
    public int RamalId { get; set; }

    [JsonPropertyName("recorrido_id")]
    public int? RecorridoId { get; set; }

    [JsonPropertyName("active_trackers")]
    public int ActiveTrackers { get; set; }

    [JsonPropertyName("number")]
    public string Number { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("lat")]
    public double? Lat { get; set; }

    [JsonPropertyName("lng")]
    public double? Lng { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("speed")]
    public int? Speed { get; set; }

    [JsonPropertyName("status_updated_at")]
    public string StatusUpdatedAt { get; set; }

    [JsonPropertyName("started_at")]
    public string StartedAt { get; set; }
}

// Recorrido object
public class Recorrido
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("ramal_id")]
    public int RamalId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; }

    [JsonPropertyName("deleted_at")]
    public object DeletedAt { get; set; }
}
