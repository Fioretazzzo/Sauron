using System;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.VisualBasic;
using RestSharp;
using GeoCoordinatePortable;
using System.Runtime.InteropServices;

namespace SauronV1
{
    public class SauronV1{
        public static Notifications notificador = new Notifications();
        //public static List<Internos> internos = null;

        public static async Task StartProcess(){

            //var notificador = new Notifications();
            bool primeraDataCreada = false;
            _ = notificador.SendNotificationAsync("Primera vuelta de coles", "Testeando la primera vuelta de los coles", "1");
            List<Internos>? listOficialInternos = null;

            while(true){

                int pages = 1;
                
                List<ColesData>? listCompleto = null;
                List<Internos>? listInternos = null;

                while (pages < 4){

                    
                    RestResponse response = await ScrapeData15(pages);
                    
                    //pregunta si el GET fue exitoso con un codigo 200 OK
                    var CheckResult = CheckAddData15(response, pages, listCompleto);

                    if(CheckResult.Estado){

                        try{
                            listCompleto = CheckResult.Data;

                           
                                listInternos = createInternosData(listCompleto);
                          
                            
                        }
                        catch(Exception ex){
                            Console.WriteLine($"ERROR CREANDO LA LISTA DE INTERNOS: {ex}");
                        }

                        pages++;

                    }else{
                        Console.WriteLine("Esperando 15 segundos para volver a preguntar");
                        System.Threading.Thread.Sleep(15000);
                    }

                }
                //si es la primera ejecucion se ejecuta esto para definir la lista en donde se va a trabajar
                if(!primeraDataCreada){
                    listOficialInternos = listInternos;
                    primeraDataCreada = true;
                }
                else{

                    try{

                        listOficialInternos = UpdateData(listOficialInternos, listInternos);

                        listOficialInternos = await CheckToSleep(listOficialInternos);

                        listOficialInternos = await CheckCercaSalida(listOficialInternos);

                    }catch(Exception ex){

                        Console.WriteLine($"ERROR/EXCEPCION AL HACER UPDATE: {ex}");
                        _ = await notificador.SendNotificationAsync("Error/Excepcion data update",
                        $"Excepcion:{ex}",
                        "1");

                    }

                }

                System.Threading.Thread.Sleep(120000);
                
            }
        }

        /*public static List<Internos> CheckWakeup(List<Internos> ListOficialInternos){

        }*/
        public static async Task<List<Internos>?> CheckCercaSalida(List<Internos> ListOficialInternos){
            GeoCoordinate SalidaNorte = new GeoCoordinate(-27.749717255283777, -64.29106858973618);
            GeoCoordinate SalidaSur = new GeoCoordinate(-27.842789938245154, -64.2478033899487);

            foreach(Internos interno in ListOficialInternos){
                if(interno.Coord[4] != null && !interno.OnBase && interno.Oculto && !interno.OnStartLine){
                    var Coord = ParseToGeoCoord(interno.Coord[4].Lat, interno.Coord[4].Long);
                    if(Coord != null){
                        if(SalidaSur.GetDistanceTo(Coord) < 750 || SalidaNorte.GetDistanceTo(Coord) < 750){
                            interno.OnStartLine = true;
                            interno.Oculto = false;
                            var ResCambio = await CambiarMostrarEnApp(interno.Id, true);
                            if(ResCambio.IsSuccessStatusCode){
                                _ = notificador.SendNotificationAsync($"SE MOSTRO EL INTERNO: {interno.Numero}",
                                 "Se mostro el interno xq anda en algun punto de salida valido", "1");
                            }
                            else{

                                Console.WriteLine("Error al cambiar el estado del cole a dormido fuera de base!!!");
                                _ = notificador.SendNotificationAsync($"ERROR ESCONDIENDO EL COLE LPM: {interno.Numero}",
                             $"tiro error cuando se trataba de esconder el cole, error: {ResCambio.StatusCode}", "1");
                            }
                        }
                        else{
                            interno.OnStartLine = false;
                        }
                    }
                }
            }
            return ListOficialInternos;
        }
        public static List<Internos>? createInternosData(List<ColesData>? listCompleto){

            //List<ColesData>? listCompleto = null;
            List<Internos>? listInternos = new List<Internos>();
            bool bandera = false;

            if(listCompleto != null && listCompleto.Count() > 0){


                foreach(ColesData colectivos in listCompleto){
                
                    //Console.WriteLine("Interno "+colectivos.Number+" : \tlat: "+colectivos.Lat+"\tlong: "+colectivos.Lng);
                    Internos auxInter = new Internos(colectivos.Number, colectivos.Name, !colectivos.ActivoEnApp, colectivos.Status, colectivos.Lat, colectivos.Lng, colectivos.Id);

                    listInternos.Add(auxInter);

                }

                
            }else{
                Console.WriteLine("NO HAY DATA PARSEADA");
            }

            return listInternos;

        }

        public static List<Internos>? UpdateData(List<Internos> OldData, List<Internos> NewData){
            //check OldData cantidad de coordenadas q tiene en el array Coord
            foreach ( Internos interno in OldData ){
                int i = 0;

                //fijarme si posta es asi la pregunta esta
                while(i < interno.Coord.GetLength(0)){
                    if(interno.Coord[i] == null){
                        break;
                    }
                    i++;
                }

                //busca los internos q se correspondan segun su numero para actualizar
                foreach ( Internos newInterno in NewData ){
                    if(interno.Numero == newInterno.Numero){

                        interno.Linea = newInterno.Linea;
                        interno.Oculto = newInterno.Oculto;
                        interno.OnBase = newInterno.OnBase;
                        interno.ServiceStatus = newInterno.ServiceStatus;

                        if(newInterno.Coord[0] != null){

                            if( i < 5 ){
                                interno.Coord[i] = newInterno.Coord[0];
                            }

                            else{
                                //shift a la izquierda
                                interno.Coord[0] = interno.Coord[1];
                                interno.Coord[1] = interno.Coord[2];
                                interno.Coord[2] = interno.Coord[3];
                                interno.Coord[3] = interno.Coord[4];
                                interno.Coord[4] = newInterno.Coord[0];
                            }

                        }
                        else{
                            Console.WriteLine("ERROR AL HACER UPDATE DE LA DATA, LA NUEVA COORDINADA ES NULA");
                        }
                        
                        break;
                    }

                }
            }
            Console.WriteLine("DATA DE LOS COLES UPDATEADA");
            
            /*_ =  notificador.SendNotificationAsync("DATA UPDATEADA",
                        $"Se hizo el update de la data de los internos correctamente",
                        "1");*/

            return OldData;
        }


        public static async Task<List<Internos>> CheckToSleep(List<Internos> ListOficialInternos){
            //check para saber si esta en la base el cole
            GeoCoordinate BaseUriarte = new GeoCoordinate(-27.814322487516797, -64.25720838995561);

            foreach(Internos interno in ListOficialInternos){
                if(interno.Coord[4] != null && !interno.Oculto){
                    //aqui tengo q hacer algunas verificaciones y parseos xq GeoCoordinate no acepte los nulls asi q tengo q sanitizar las coordenadas primero
                    double Dist0 = -1;
                    double Dist1 = -1;
                    double Dist2 = -1;
                    double Dist3 = -1;
                    double Dist4 = -1;

                    #region obtener Coordenadas y Distancias

                    //primera coordenada del array 
                    var Coord0 = ParseToGeoCoord(interno.Coord[0].Lat, interno.Coord[0].Long);
                    
                    if(Coord0 != null){
                        Dist0 = BaseUriarte.GetDistanceTo(Coord0);
                    }

                    //segunda coordenada del array
                    var Coord1 = ParseToGeoCoord(interno.Coord[1].Lat, interno.Coord[1].Long);
                    
                    if(Coord1 != null){
                        Dist1 = BaseUriarte.GetDistanceTo(Coord1);
                    }

                    //tercera coordenada del array
                    var Coord2 = ParseToGeoCoord(interno.Coord[2].Lat, interno.Coord[2].Long);

                    if(Coord2 != null){
                        Dist2 = BaseUriarte.GetDistanceTo(Coord2);
                    }

                    //cuarta coordenada del array
                    var Coord3 = ParseToGeoCoord(interno.Coord[3].Lat, interno.Coord[3].Long);

                    if(Coord3 != null){
                        Dist3 = BaseUriarte.GetDistanceTo(Coord3);
                    }

                    //quinta coordenada del array
                    var Coord4 = ParseToGeoCoord(interno.Coord[4].Lat, interno.Coord[4].Long);

                    if(Coord4 != null){
                        Dist4 = BaseUriarte.GetDistanceTo(Coord4);
                    }

                    #endregion

                    #region Region para contar cuantas veces se detecto al cole en la base Uriarte
                    int CountChecker = 0;

                    if(Dist0 < 70 && Dist0 != -1){
                        CountChecker++;
                    }
                    if(Dist1 < 70 && Dist1 != -1){
                        CountChecker++;
                    }
                    if(Dist2 < 70 && Dist2 != -1){
                        CountChecker++;
                    }
                    if(Dist3 < 70 && Dist4 != -1){
                        CountChecker++;
                    }
                    if(Dist4 < 70 && Dist4 != -1){
                        CountChecker++;
                    }

                    #endregion

                    
                    #region Ocultar interno cuando esta en la base
                    if(CountChecker >= 4){
                        interno.OnBase = true;
                        interno.Oculto = true;
                        var ResCambio = await CambiarMostrarEnApp(interno.Id, false);
                        if(ResCambio.IsSuccessStatusCode){
                            _ = notificador.SendNotificationAsync($"SE OCULTO EL INTERNO: {interno.Numero}",
                             "Se oculto el interno en la app xq marca que esta en la base de la pija", "1");
                        }
                        else{

                            Console.WriteLine("Error al cambiar el estado del cole a dormido fuera de base!!!");
                            _ = notificador.SendNotificationAsync($"ERROR ESCONDIENDO EL COLE LPM: {interno.Numero}",
                         $"tiro error cuando se trataba de esconder el cole, error: {ResCambio.StatusCode}", "1");
                        }
                    }
                    //si no estuvo ninguna de las veces cerca de la base entonces se lo pase a OnBase = false
                    else if(CountChecker == 0){
                        interno.OnBase = false;
                    }
                    #endregion

                    //esto es asi un if dentro de otro xq no los puedo meter juntos ya que altera el resultado del valor de la llamada a la func OcultarColeFueraBase()
                    //los cambios se hacen dentro de la func y dsp de eso tira true para actualizar la data q tengo del interno
                    if(!interno.OnStartLine){
                        if(await OcultarColeFueraBase(Coord0, Coord1, Coord2, Coord3, Coord4, interno)){
                        interno.Oculto = true;
                        }
                    }
                    else if(interno.Oculto){
                        CambiarMostrarEnApp(interno.Id, true);
                    }

                    

                    // 11/12/24 08:17 revisar esto en q valor inicia Oculto cuando se crea el objeto
                    if(interno.Oculto){
                        interno.TimeSleep = DateTime.Now;
                    }

                }   
            }
            return ListOficialInternos;
        }

        public static async Task<bool> OcultarColeFueraBase(GeoCoordinate? Coord0, GeoCoordinate? Coord1, GeoCoordinate? Coord2, GeoCoordinate? Coord3, GeoCoordinate? Coord4, Internos interno){
            
            //Arragle de las combinaciones de coordenadas para preguntar si ya paso el suficiente tiempo y se tiene la data pertinente para poner el cole como Oculto y q este fuera de la base
            List<GeoCoordinate?[]> ArrayCombinaciones3 = new List<GeoCoordinate?[]>(){new GeoCoordinate?[] {Coord1, Coord3, Coord4},
                                                                                      new GeoCoordinate?[] {Coord1, Coord2, Coord4},
                                                                                      new GeoCoordinate?[] {Coord0, Coord2, Coord3},
                                                                                      new GeoCoordinate?[] {Coord0, Coord3, Coord4},
                                                                                      new GeoCoordinate?[] {Coord0, Coord2, Coord4},
                                                                                      new GeoCoordinate?[] {Coord0, Coord1, Coord4},
                                                                                      new GeoCoordinate?[] {Coord0, Coord1, Coord3}};

            if((Coord0 ?? Coord1 ?? Coord2 ?? Coord3 ?? Coord4) != null){

                double aux1 = Coord0.GetDistanceTo(Coord1);
                double aux2 = Coord1.GetDistanceTo(Coord2);
                double aux3 = Coord2.GetDistanceTo(Coord3);
                double aux4 = Coord3.GetDistanceTo(Coord4);

                double SumaDist = Coord0.GetDistanceTo(Coord1) + Coord1.GetDistanceTo(Coord2) + Coord2.GetDistanceTo(Coord3) + Coord3.GetDistanceTo(Coord4);
                //pregunta si la suma de las 4 distancias es menos de 100 metras, si no se movio ni 100 metros en 8 minutos entonces esta dormido en algun lado el cole
                if(SumaDist < 100.0){
                    var ResCambio = await CambiarMostrarEnApp(interno.Id, false);
                    if(ResCambio != null){

                        if(ResCambio.IsSuccessStatusCode){
                        _ = notificador.SendNotificationAsync($"SE OCULTO EL INTERNO: {interno.Numero}",
                         "Se oculto el interno en la app xq anda sin moverse hace rato", "1");

                         return true;
                        }
                        else{

                            Console.WriteLine("Error al cambiar el estado del cole a dormido fuera de base!!!");
                            _ = notificador.SendNotificationAsync($"ERROR ESCONDIENDO EL COLE LPM: {interno.Numero}",
                         $"tiro error cuando se trataba de esconder el cole, error: {ResCambio.StatusCode}", "1");
                         return false;
                        }
                    }
                }
            }
            else{
                //aqui mando el ciclo para iterar por la lista de arreglos y empezar a comparar cada una de las posibles combinaciones
                //asi con esta forma me ahorro una re cade llena de else if
                foreach (GeoCoordinate?[] Combinacion in ArrayCombinaciones3){
                    if(await EsconderColeDormido(Combinacion[0], Combinacion[1], Combinacion[2], interno)){
                        return true;
                    }
                }                
            }
            return false;
            //en teoria nunca tendria q entrar aqui por el checkeo q se le hizo antes
            //lpm q ganas de unas empanadas caprese q tengo

        }

        public static double GetDistancia(double lat, double lng, GeoCoordinate CoordenadaFija){

            double Dist = -1;
            var Coord = ParseToGeoCoord(lat, lng);
            
            if(Coord != null){
                Dist = CoordenadaFija.GetDistanceTo(Coord);
            }

            return Dist;
        }
        //toma 3 coordenadas y calcula si el cole no se movio lo suficiente como para ser escondido
        public static async Task<bool> EsconderColeDormido(GeoCoordinate? C1, GeoCoordinate? C2, GeoCoordinate? C3, Internos interno){
            //fuck it en vez de hacer las 6 combinaciones correspondientes voy a hacer 3 nomas, con eso tendria q bastar y prevenir ciertos casos especiales
            if(( C1 ?? C2 ?? C3 ) != null){

                if(C1.GetDistanceTo(C2) + C2.GetDistanceTo(C3) < 40.0){
                    var ResCambio = await CambiarMostrarEnApp(interno.Id, false);
                    if(ResCambio != null){
                        if(ResCambio.IsSuccessStatusCode){
                        _ = notificador.SendNotificationAsync($"SE OCULTO EL INTERNO: {interno.Numero}",
                         "Se oculto el interno en la app xq anda sin moverse hace rato", "1");
                         return true;
                        }
                        else{
                            Console.WriteLine("Error al cambiar el estado del cole a dormido fuera de base!!!");
                            _ = notificador.SendNotificationAsync($"ERROR ESCONDIENDO EL COLE LPM: {interno.Numero}",
                            $"tiro error cuando se trataba de esconder el cole, error: {ResCambio.StatusCode}", "1");
                        }
                    }
                }
            }
            return false;
        }

        
        public static GeoCoordinate? ParseToGeoCoord(double? lat, double? lng){
            try{

                if(lat != null && lng != null){
                double CoordLat = (double)lat;
                double CoordLong = (double)lng;
                GeoCoordinate Coordenada = new GeoCoordinate(CoordLat, CoordLong);

                return Coordenada;
                }
            return null;
            }catch(Exception ex){
                Console.WriteLine($"EXCEPCION AL CONVERTIR COORDENADA A GEOCOORDINATE TYPE {ex}");
                return null;
            }
            
        }

        public static async Task<RestResponse>? CambiarMostrarEnApp(int Id, bool mostrar) {

            int modo;

            //modo = 1/true para MOSTRAR modo = 0/false para OCULTAR
            if(mostrar){
                modo = 1;
            }else{
                modo = 0;
            }

            try{
                var options = new RestClientOptions("https://api.santiagobus.com.ar")
                {
                  MaxTimeout = -1,
                  UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:133.0) Gecko/20100101 Firefox/133.0",
                };
                var client = new RestClient(options);
                var request = new RestRequest($"/api/internos/{Id}", Method.Put);
                request.AddHeader("Accept", "application/json, text/plain, */*");
                request.AddHeader("Accept-Language", "en-US,en;q=0.5");
                request.AddHeader("Accept-Encoding", "gzip, deflate, br, zstd");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer 267|EmDMZcnQUAf6qU0x930kjiArjzZcQRLKOGxj3278a71b26f4");
                request.AddHeader("Origin", "https://panel.santiagobus.com.ar");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Referer", "https://panel.santiagobus.com.ar/");
                request.AddHeader("Sec-Fetch-Dest", "empty");
                request.AddHeader("Sec-Fetch-Mode", "cors");
                request.AddHeader("Sec-Fetch-Site", "same-site");
                request.AddHeader("Priority", "u=0");
                request.AddHeader("Pragma", "no-cache");
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("TE", "trailers");
                var body = @"{""activo_en_app"":"+modo+"}";
                request.AddStringBody(body, DataFormat.Json);
                RestResponse response = await client.ExecuteAsync(request);
                Console.WriteLine("ESTADO CAMBIADO EN LA APP"+response.StatusCode);

                return response;
            }catch(Exception ex){
                if(mostrar){
                    Console.WriteLine("ERROR CAMBIANDO A OCULTO DEL INTERNO");
                _ = notificador.SendNotificationAsync($"ERROR OCULTANDO INTERNO",
                $"SE PRODUJO UN ERROR AL OCULTAR UN INTERNO {ex}",
                "1");                

                }else{
                    Console.WriteLine("ERROR CAMBIANDO A MOSTRAR DEL INTERNO");
                _ = notificador.SendNotificationAsync($"ERROR MOSTRANDO INTERNO",
                $"SE PRODUJO UN ERROR AL MOSTRAR UN INTERNO {ex}",
                "1");
                }
                
            }
            
            return null;
            
        }

        public static double calcularDistancia(double lat1, double lng1, double lat2, double lng2){
            var coordenada1 = new GeoCoordinate(lat1, lng1);
            var coordenada2 = new GeoCoordinate(lat2, lng2);

            return coordenada1.GetDistanceTo(coordenada2);
        }

        public static async Task<RestResponse> ScrapeData15(int pages){

            var options = new RestClientOptions("https://api.santiagobus.com.ar")
                {
                MaxTimeout = -1,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:132.0) Gecko/20100101 Firefox/132.0",
                };

                var client = new RestClient(options);
                var request = new RestRequest("/api/internos?include=ramal.linea.empresa,trackers,recorrido&filter%5Bstatus_service%5D=&filter%5Blinea_id%5D=12&filter%5Bactivo_en_app%5D=&filter%5Bfuera_recorrido%5D=&sort=&page%5Bnumber%5D="+pages+"&page%5Bnumber%5D="+pages, Method.Get);
                request.AddHeader("Accept", "application/json, text/plain, */*");
                request.AddHeader("Accept-Language", "en-US,en;q=0.5");
                request.AddHeader("Accept-Encoding", "gzip, deflate, br, zstd");
                request.AddHeader("Authorization", "Bearer 267|EmDMZcnQUAf6qU0x930kjiArjzZcQRLKOGxj3278a71b26f4");
                request.AddHeader("Origin", "https://panel.santiagobus.com.ar");
                request.AddHeader("Connection", "keep-alive");
                request.AddHeader("Referer", "https://panel.santiagobus.com.ar/");
                request.AddHeader("Sec-Fetch-Dest", "empty");
                request.AddHeader("Sec-Fetch-Mode", "cors");
                request.AddHeader("Sec-Fetch-Site", "same-site");
                request.AddHeader("Priority", "u=0");
                request.AddHeader("Pragma", "no-cache");
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("TE", "trailers");
                RestResponse response = await client.ExecuteAsync(request);

                return response;
        }

       
       public static DataResult<List<ColesData>> CheckAddData15(RestResponse response,int pages, List<ColesData>? listCompleto){

            //List<ColesData>? listCompleto = null;
            //List<Internos>? listInternos = null;
        
            if(response.IsSuccessStatusCode){
            //trata de deserializar el JSON
            try{
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(response.Content);
                //controla que el contenido del JSON no este vacio
                if(apiResponse?.Data != null && apiResponse.Data.Count > 0){

                    if(listCompleto?.Count > 0 && listCompleto != null){
                        listCompleto.AddRange(apiResponse.Data);
                    }else{
                        listCompleto = apiResponse.Data;
                    }

                    //return 200 OK regresa la lista checkeada y va con un codigo true para verificarla en la funcion que lo llame
                    return DataResult<List<ColesData>>.Success(listCompleto);
                    
                }else{
                    Console.WriteLine("SIN DATA PARA MOSTRAR DE LOS COLES");
                }
            }catch(JsonException ex){
                Console.WriteLine($"ERROR DESERIALIZANDO EL JSON, PAGINA {pages}: {ex.Message}");

                //Notifications

                //return error y muestra lo agarrado por el Try Catch
                return DataResult<List<ColesData>>.Failure(ex.Message);
                }
            }
            else{
            Console.WriteLine("ERROR EN LA PAGINA/REQUEST");
            }

            return DataResult<List<ColesData>>.Failure("ERROR NO SE PUDO OBTENER INFO VALIDA");
        }

        static async Task Main(){
            await StartProcess();
        }
    }
}

//clase hecha para poder pasar bien los parametros, si esta todo bien devulve el estado= true y la data, sino devuelve estado = false y la excepcion o sino un string con algun msj
public class DataResult<T>
{
    public T? Data { get; set; }
    public bool Estado { get; set; }
    public string? ErrorMessage { get; set; }

    public static DataResult<T> Success(T data)
    {
        return new DataResult<T> { Data = data, Estado = true };
    }

    public static DataResult<T> Failure(string errorMessage)
    {
        return new DataResult<T> { Estado = false, ErrorMessage = errorMessage };
    }
}