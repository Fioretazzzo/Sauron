using System;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.VisualBasic;
using RestSharp;
using GeoCoordinatePortable;

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

                    }catch(Exception ex){

                        Console.WriteLine($"ERROR/EXCEPCION AL HACER UPDATE: {ex}");
                        _ = await notificador.SendNotificationAsync("Error/Excepcion data update",
                        $"Excepcion:{ex}",
                        "1");

                    }

                }

                System.Threading.Thread.Sleep(15000);
                
            }
        }

        /*public static List<Internos> CheckWakeup(List<Internos> ListOficialInternos){

        }*/

        public static List<Internos>? createInternosData(List<ColesData>? listCompleto){

            //List<ColesData>? listCompleto = null;
            List<Internos>? listInternos = new List<Internos>();
            bool bandera = false;

            if(listCompleto != null && listCompleto.Count() > 0){


                foreach(ColesData colectivos in listCompleto){
                
                    //Console.WriteLine("Interno "+colectivos.Number+" : \tlat: "+colectivos.Lat+"\tlong: "+colectivos.Lng);
                    Internos auxInter = new Internos(colectivos.Number, colectivos.Name, colectivos.ActivoEnApp, colectivos.Status, colectivos.Lat, colectivos.Lng, colectivos.Id);

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

                while(i < interno.Coord.GetLength(0)){
                    if(interno.Coord[i] == null){
                        break;
                    }
                    i++;
                }

                //busca los internos q se correspondan segun su numero
                foreach ( Internos newInterno in OldData ){
                    if(interno.Numero == newInterno.Numero){

                        interno.Linea = newInterno.Linea;
                        interno.Oculto = newInterno.Oculto;
                        interno.OnBase = newInterno.OnBase;
                        interno.ServiceStatus = newInterno.ServiceStatus;

                        if(newInterno.Coord[0] != null){
                            //VOLVER A FIJARME AQUI QUE ONDA CON ESTO

                            if( i < 3 ){
                                interno.Coord[i] = newInterno.Coord[0];
                            }

                            else{
                                //shift a la izquierda
                                interno.Coord[0] = interno.Coord[1];
                                interno.Coord[1] = interno.Coord[2];
                                interno.Coord[2] = newInterno.Coord[0];
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
            
            _ =  notificador.SendNotificationAsync("DATA UPDATEADA",
                        $"Se hizo el update de la data de los internos correctamente",
                        "1");

            return OldData;
        }


        public static async Task<List<Internos>> CheckToSleep(List<Internos> ListOficialInternos){
            //check para saber si esta en la base el cole
            GeoCoordinate BaseUriarte = new GeoCoordinate(-27.814322487516797, -64.25720838995561);

            foreach(Internos interno in ListOficialInternos){
                if(interno.Coord[2] != null && interno.Oculto != false){
                    //aqui tengo q hacer algunas verificaciones y parseos xq GeoCoordinate no acepte los nulls asi q tengo q sanitizar las coordenadas primero
                    double Dist0 = -1;
                    double Dist1 = -1;
                    double Dist2 = -1;

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

                    if(CountChecker >= 2){
                        interno.OnBase = true;
                        var ResCambio = await CambiarMostrarEnApp(interno.Id, false);
                        if(ResCambio.IsSuccessStatusCode){
                            _ = notificador.SendNotificationAsync($"SE OCULTO EL INTERNO: {interno.Numero}",
                             "Se oculto el interno en la app xq marca que esta en la base de la pija", "1");
                        }
                    }

                    if(Coord0 != null && Coord1 != null && Coord2 != null){
                        if(Coord0.GetDistanceTo(Coord1) + Coord1.GetDistanceTo(Coord2) < 100.0){
                            var ResCambio = await CambiarMostrarEnApp(interno.Id, false);
                            if(ResCambio != null){
                                if(ResCambio.IsSuccessStatusCode){
                                    interno.Oculto = true;
                                _ = notificador.SendNotificationAsync($"SE OCULTO EL INTERNO: {interno.Numero}",
                                 "Se oculto el interno en la app xq anda sin moverse hace rato", "1");
                                }
                            }
                        }
                    }
                    else if(await EsconderColeDormido(Coord0, Coord1, interno)){
                        interno.Oculto = true;
                    }
                    //en teoria nunca tendria q entrar aqui por el checkeo q se le hizo antes
                    //lpm q ganas de unas empanadas caprese q tengo
                    else if(await EsconderColeDormido(Coord1, Coord2, interno)){
                        interno.Oculto = true;
                    }
                    else if(await EsconderColeDormido(Coord0, Coord2, interno)){
                        interno.Oculto = true;
                    }

                    if(interno.Oculto){
                        interno.TimeSleep = DateTime.Now;
                    }

                }   
            }
            return ListOficialInternos;
        }

        public static async Task<bool> EsconderColeDormido(GeoCoordinate? C1, GeoCoordinate? C2, Internos interno){
            if( C1 != null && C2 != null){
                if(C1.GetDistanceTo(C2) < 100.0){
                    var ResCambio = await CambiarMostrarEnApp(interno.Id, false);
                    if(ResCambio != null){
                        if(ResCambio.IsSuccessStatusCode){
                        _ = notificador.SendNotificationAsync($"SE OCULTO EL INTERNO: {interno.Numero}",
                         "Se oculto el interno en la app xq anda sin moverse hace rato", "1");
                        }
                        return true;
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
                Console.WriteLine(response.Content);

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