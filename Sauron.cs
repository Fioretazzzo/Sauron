using System;
using System.Linq.Expressions;
using System.Text.Json;
using RestSharp;

namespace SauronV1
{
    public class SauronV1{
        public static List<Internos> internos = null;

        public static async Task Get15Data(){
            int pages = 1;
            List<ColesData> listCompleto = null;

            while (pages < 4){
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
                //Console.WriteLine(response.Content);
                if(response.IsSuccessStatusCode){

                    try{
                        var apiResponse = JsonSerializer.Deserialize<ApiResponse>(response.Content);
                        if(apiResponse?.Data != null && apiResponse.Data.Count > 0){
                            
                            if(listCompleto?.Count > 0 && listCompleto != null){
                                listCompleto.AddRange(apiResponse.Data);
                            }else{
                                listCompleto = apiResponse.Data;
                            }
                            

                        }else{

                            Console.WriteLine("SIN DATA PARA MOSTRAR DE LOS COLES");

                        }
                    }catch(JsonException ex){
                        Console.WriteLine($"ERROR DESERIALIZANDO EL JSON, PAGINA {pages}: {ex.Message}");
                    }

                }else{
                    Console.WriteLine("ERROR EN LA PAGINA/REQUEST");
                }
             
                pages++;
            }
            if(listCompleto != null && listCompleto.Count() > 0){
                foreach(ColesData colectivos in listCompleto){
                                        
                    Console.WriteLine("Interno "+colectivos.Number+" : \tlat: "+colectivos.Lat+"\tlong: "+colectivos.Lng);

                    
                }                
                
            }else{
                Console.WriteLine("NO HAY DATA PARSEADA");
            }

        }

        static async Task Main(){
            await Get15Data();
        }
    }
}