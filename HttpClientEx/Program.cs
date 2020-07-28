using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace HttpClientEx
{
    class Program
    {
        static void Main(string[] args)
        {
            #region [Routes]
            //  Api Servicios Imputacion
            string url1 = "https://localhost:44323/api/imputador/v1/imputador/estado_imputador";
            string url2 = "https://localhost:44323/api/fechacontable/v1/fechacontable/get_fecha_contable";
            string url3 = "https://localhost:44323/api/imputador/v1/imputador/post_cola_proceso";
            //  Api Solicitudes
            string url4 = "https://localhost:44323/api/v1/filtrossolicitudes";
            string url5 = "https://localhost:44323/api/v1/filtrossolicitudes/filtrarfallecidos";
            string url6 = "https://localhost:44323/api/v1/filtrossolicitudes/filtrarSaldoCero";
            #endregion

            #region [Prueba GetEstado]
            //string result = GetEstado(url);
            #endregion

            #region [Prueba GetFechaContable]
            /*string result = GetFechaContable($"{url2}/1");
            DateTime fechaContable = Convert.ToDateTime(result);
            var mensaje = fechaContable.Date.Equals(DateTime.Today) ? "es la fecha actual" : "no es la fecha actual";*/
            #endregion

            #region [Prueba PostColaProceso]
            /*var tipoProceso = 80;
            var codUsuario = "S761226";
            var llaveOrigen = "1";
            var values = new Filtros()
            {
                TipoProceso = tipoProceso,
                CodUsuario = codUsuario,
                LlaveOrigen = llaveOrigen
            };
            string reqValues = JsonConvert.SerializeObject(values);
            var reqContent = new StringContent(reqValues, Encoding.UTF8, "application/json");
            PostColaProceso(url3, reqContent);*/
            #endregion

            #region [Prueba GetLista]
            int tipoSolicitud = 170;
            int estado = 459;
            string fecha = Convert.ToString(DateTime.Today);
            string usuario = "s761226";

            var url = $"{url4}?Solicitud={tipoSolicitud}&Fecha={fecha}&Usuario={usuario}&Estado={estado}";
            List<SolicitudDto> ListaSolicitudes = GetLista(url);
            #endregion

            #region [Prueba GetListaFiltrada]

            //  Validar que la lista de solicitudes no este vacia.
            if(ListaSolicitudes.Count != 0)
            {
                var existeFallecido = false;
                var countFallecidos = 0;
                foreach (var solicitud in ListaSolicitudes)
                {
                    if (solicitud.FechaFallecimiento != "")
                    {
                        existeFallecido = true; countFallecidos++;
                    }
                    else continue;
                }

                //  Validar si existen clientes fallecidos.
                if (existeFallecido == true)
                {
                    //  Mensaje de consola
                    var mensaje = countFallecidos > 1 ? $"Se han encontrado {countFallecidos} clientes fallecidos." : "Se ha encontrado un cliente fallecido.";
                    Console.WriteLine(mensaje);
                    Console.WriteLine("Filtrando las solicitudes de clientes fallecidos de la lista . . .");
                    
                    // Contenido de la request
                    string reqValues = JsonConvert.SerializeObject(ListaSolicitudes);
                    var reqContent = new StringContent(reqValues, Encoding.UTF8, "application/json");
                    ListaSolicitudes = FiltrarFallecidos(url5, usuario, reqContent);
                }
                var saldo0 = false;
                var countSaldo0 = 0;
                foreach (var solicitud in ListaSolicitudes)
                {
                    int count = 0;
                    
                    SolicitudDto SolicitudSaldosCero = new SolicitudDto();
                    foreach (var ObjDetalle in solicitud.Saldos)
                    {
                        if (ObjDetalle.SaldoCuota == "0") count++;
                    }
                    if (count == solicitud.Saldos.Count || solicitud.Detalle.Count == 0)
                    {
                        saldo0 = true; countSaldo0++;
                    }
                }

                // Validar si exiten clientes con saldos en 0.
                if (saldo0)
                {
                    //  Mensaje de consola
                    var mensaje = countSaldo0 > 1 ? $"Se han encontrado {countSaldo0} solicitudes de clientes con saldos en 0." : "Se ha encontrado una solicitud de cliente con saldos en 0.";
                    Console.WriteLine(mensaje);
                    Console.WriteLine("Filtrando solicitudes de clientes con saldos en 0 . . .");

                    //  Contenido de la request
                    string reqValues = JsonConvert.SerializeObject(ListaSolicitudes);
                    var reqContent = new StringContent(reqValues, Encoding.UTF8, "application/json");
                    ListaSolicitudes = FiltrarSaldosCero(url6, usuario, reqContent);
                }
            }
            else
            {
                Console.WriteLine("No se pudo obtener la lista de solucitudes...");
            }
            #endregion
        }

        public static string GetEstado(string url)
        {
            string result = "";
            WebRequest request = WebRequest.Create(url);
            request.Method = "get";
            request.ContentType = "application/json;charset=UTF-8";

            WebResponse response = request.GetResponse();
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                result = sr.ReadToEnd().Trim();
            }
            JObject o = JObject.Parse("{\"nombre\":\"siri\"}");
            string name = (string)o.SelectToken("nombre");

            return result;
        }
        public static string GetFechaContable(string url)
        {
            string result = "";
            WebRequest request = WebRequest.Create(url);
            request.Method = "get";
            request.ContentType = "application/json;charset=UTF-8";

            WebResponse response = request.GetResponse();
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                result = sr.ReadToEnd().Trim();
            }
            JObject o = JObject.Parse(result);
            string fechaContable = o.SelectToken("fechaContable").ToString();

            return fechaContable;
        }
        public static ColaProceso PostColaProceso(string url, StringContent content)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = client.PostAsync(url, content).Result)
                {
                    HttpContent resContent = res.Content;
                    var resData = resContent.ReadAsStringAsync().Result;
                    ColaProceso result = JsonConvert.DeserializeObject<ColaProceso>(resData);
                    Console.WriteLine($"el Id del proceso es: {result.Id}");
                    return result;
                }
            }
        }
        public static List<SolicitudDto> GetLista(string url)
        {
            List<SolicitudDto> result = new List<SolicitudDto>();
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = client.GetAsync(url).Result)
                {
                    try
                    {
                        HttpContent resContent = res.Content;
                        if (res.IsSuccessStatusCode)
                        {
                            res.EnsureSuccessStatusCode();
                            var resData = resContent.ReadAsStringAsync().Result;
                            result = JsonConvert.DeserializeObject<List<SolicitudDto>>(resData);
                            var mensaje = result.Count > 1 ? $"Se han  encontrado {result.Count} solicitudes [170] pendientes." : "Se ha encontrado una solicitud [170] pendiente.";
                            Console.WriteLine($"{mensaje}{{0}}",Environment.NewLine);
                        }
                        else
                        {
                            var resData = resContent.ReadAsStringAsync().Result;
                            Console.WriteLine($"Error [{res.StatusCode}] al recibir la respuesta");
                            Console.WriteLine($"Mensaje: {resData}{{0}}", Environment.NewLine);
                        }
                        return result;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Se ha producido una excepcion al recibir la respuesta:{ex}, {ex.Message}");
                        throw;
                    }
                }
            }
        }
        public static List<SolicitudDto> FiltrarFallecidos(string url, string usuario, StringContent content)
        {
            List<SolicitudDto> result = new List<SolicitudDto>();
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = client.PostAsync($"{url}/{usuario}", content).Result)
                {
                    try
                    {
                        HttpContent resContent = res.Content;
                        if (res.IsSuccessStatusCode)
                        {
                            res.EnsureSuccessStatusCode();
                            var resData = resContent.ReadAsStringAsync().Result;
                            result = JsonConvert.DeserializeObject<List<SolicitudDto>>(resData);
                            Console.WriteLine($"La lista filtrada contiene ahora {result.Count} solicitudes.{{0}}", Environment.NewLine);
                        }
                        else
                        {
                            var resData = resContent.ReadAsStringAsync().Result;
                            Console.WriteLine($"Error [{res.StatusCode}] al recibir la respuesta");
                            Console.WriteLine($"Mensaje: {resData}{{0}}", Environment.NewLine);
                        }
                        return result;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Se ha producido una excepcion al recibir la respuesta:{ex}, {ex.Message}");
                        throw;
                    }
                }
            }
        }
        public static List<SolicitudDto> FiltrarSaldosCero(string url, string usuario, StringContent content)
        {
            List<SolicitudDto> result = new List<SolicitudDto>();
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage res = client.PostAsync($"{url}/{usuario}", content).Result)
                {
                    try
                    {
                        HttpContent resContent = res.Content;
                        if (res.IsSuccessStatusCode)
                        {
                            res.EnsureSuccessStatusCode();
                            var resData = resContent.ReadAsStringAsync().Result;
                            result = JsonConvert.DeserializeObject<List<SolicitudDto>>(resData);
                            Console.WriteLine($"La lista filtrada contiene ahora {result.Count} solicitudes.{{0}}", Environment.NewLine);
                        }
                        else
                        {
                            var resData = resContent.ReadAsStringAsync().Result;
                            Console.WriteLine($"No se pudo obtener la lista de solicitudes filtradas, statusCode: {res.StatusCode}");
                            Console.WriteLine($"Mensaje: {resData}");
                        }
                        return result;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Se ha producido una excepcion al recibir la respuesta:{ex}, {ex.Message}");
                        throw;
                    }
                }
            }
        }
    }
    public class Filtros
    {
        public int TipoProceso { get; set; }
        public string CodUsuario { get; set; }
        public string LlaveOrigen { get; set; }
    }

    public class ColaProceso
    {
        public int Id { get; set; }
        public string Error { get; set; }
        public int Retorno { get; set; }
    }

    public class SolicitudDto
    {
        //Propiedades
        public long IdSolicitud { get; set; }
        public string TipoRutAfectado { get; set; }
        public int RutAfectado { get; set; }
        public string DvAfectado { get; set; }
        public string IdNumCliente { get; set; }
        public string FechaFallecimiento { get; set; }
        public int TipoSolicitud { get; set; }
        public string FechaSolicitud { get; set; }
        public int Folio { get; set; }
        public string FechaMaterializacion { get; set; }
        public string TipoRutSolicitante { get; set; }
        public string RutSolicitante { get; set; }
        public string DvSolicitante { get; set; }
        public int IdEstado { get; set; }
        public int TipoCodSucursal { get; set; }
        public int RutVendedor { get; set; }
        public string DvVendedor { get; set; }
        public List<DetalleSolicitudDto> Detalle { get; set; }
        public List<DetalleSaldoDto> Saldos { get; set; }
        public string IdWorkFlow { get; set; }
        public string FechaRecepcion { get; set; }
        public string Observaciones { get; set; }
        public string FechaDigitacion { get; set; }
        public string IdRegistro { get; set; }
    }
    public class DetalleSolicitudDto
    {
        public long IdSolicitud { get; set; }
        public int TipoCuentaCliente { get; set; }
        public string TipoCuentaFondo { get; set; }
        public int TipoRegimenTributario { get; set; }
        public int IdSaldo { get; set; }
        public string TipoSubSaldo { get; set; }
        public int IdContrato { get; set; }
        public string MontoDestinadoCuotas { get; set; }
        public int TipoAbono { get; set; }
    }
    public class DetalleSaldoDto
    {
        public string NumCliente { get; set; }
        public string TipoCuentaCliente { get; set; }
        public string TipoCuentaFondo { get; set; }
        public string TipoSubSaldo { get; set; }
        public string TipoRegimen { get; set; }
        public string IdContrato { get; set; }
        public string IdSaldo { get; set; }
        public string IdRegistro { get; set; }
        public string SaldoCuota { get; set; }
        public string SaldoUtm { get; set; }
        public string SaldoUf { get; set; }
    }
}
