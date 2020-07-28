using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace JsonSerialize
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "https://jsonplaceholder.typicode.com/posts";
            //string result = GetPost(url);
            Convert();
        }
        public static string Convert()
        {
            var today = DateTime.Today;
            var month = new DateTime(today.Year, today.Month, 1);
            var first = month.AddMonths(-1);
            var values = new Dictionary<string, string>
                {
                    { "month",first.Month.ToString() },
                    { "year", first.Year.ToString() }
                };
            string strPayload = JsonConvert.SerializeObject(values);
            return strPayload;
        }
        public static string GetPost(string url)
        {
            Persona persona = new Persona() { Nombre = "Juanito", Edad = 18 };
            string result = "";
            WebRequest request = WebRequest.Create(url);
            request.Method = "post";
            request.ContentType = "application/json;charset=UTF-8";

            using (var sw = new StreamWriter(request.GetRequestStream()))
            {
                //string json = "{\"nombre\":\"juanito\",\"edad\":\"18\"}";
                string json = JsonConvert.SerializeObject(persona);
                sw.Write(json);
                sw.Flush();
                sw.Close();
            }
            WebResponse response = request.GetResponse();
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                result = sr.ReadToEnd().Trim();
            }

            return result;
        }

        public class Persona
        {
            public string Nombre { get; set; }
            public int Edad { get; set; }
        }
    }
}
