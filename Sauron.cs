using System;
using System.Linq.Expressions;
using System.Text.Json;
using RestSharp;

namespace SauronV1
{
    public class SauronV1{
        //public static List<Internos> internos = null;

        public static async Task StartProcess(){

            int pages = 1;
            bool primeraData = false;
            List<ColesData>? listCompleto = null;
            List<Internos>? listInternos;
            

            while (pages < 4){

                
                RestResponse response = await ScrapeData15(pages);
                
                //pregunta si el GET fue exitoso con un codigo 200 OK
                var CheckResult = CheckAddData15(response, pages, listCompleto);

                if(CheckResult.Estado){

                    try{
                        listCompleto = CheckResult.Data;

                        if(!primeraData){

                            
                            listInternos = createInternosData(listCompleto);
                            
                        }
                        
                    }
                    catch(Exception ex){
                        Console.WriteLine($"ERROR CREANDO LA LISTA DE INTERNOS: {ex}");
                    }

                }else{
                    Console.WriteLine("Esperando un rato para volver a preguntar");
                }

             
                pages++;
            }
            primeraData = true;
            listCompleto = null;
            



        }

        public static List<Internos>? createInternosData(List<ColesData>? listCompleto){

            //List<ColesData>? listCompleto = null;
            List<Internos>? listInternos = new List<Internos>();
            bool bandera = false;

            if(listCompleto != null && listCompleto.Count() > 0){


                foreach(ColesData colectivos in listCompleto){
                
                    //Console.WriteLine("Interno "+colectivos.Number+" : \tlat: "+colectivos.Lat+"\tlong: "+colectivos.Lng);
                    Internos auxInter = new Internos(colectivos.Number, colectivos.Name, colectivos.ActivoEnApp, colectivos.Status, colectivos.Lat, colectivos.Lng);

                    listInternos.Add(auxInter);

                    //listInternos = auxInter;
                }

                
            }else{
                Console.WriteLine("NO HAY DATA PARSEADA");
            }

            return listInternos;

        }

        /*public static List<Internos>? UpdateData(List<Internos> ListInternos){

        }

        public static List<Internos> CheckToSleep(List<Internos> ListInternos){

        }*/

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