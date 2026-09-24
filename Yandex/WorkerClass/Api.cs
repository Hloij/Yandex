using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK.Compute.OpenCL;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Yandex.Model;
namespace Yandex.WorkerClass
{
    internal class Api<T>
    {
        public static async Task<T> PostApiRespons(string Url,  (string, string)[] Params, string Auth)
        {
          
            RestClient restClient = new RestClient();
            var request = new RestRequest(Url, Method.Post);
            request.AddHeader("Authorization", Auth);
            for (int i = 0; i < Params.Length; i++)
            {
                request.AddParameter(Params[i].Item1, Params[i].Item2);
            }
            int h = 0;
            m1: h++;
            RestResponse response = restClient.ExecuteAsync(request).Result;
            try
            {
               // if (!response.IsSuccessStatusCode && h < 10) { await Task.Delay(50000); goto m1; }


                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            catch (Exception ex)
            {
                return default(T);
            }

          
        }
        public static async Task<T> PostApiRespons(string Url, string obj, string Auth)
        {

            RestClient restClient = new RestClient();
            var request = new RestRequest(Url, Method.Post);
            request.AddHeader("Authorization", Auth);
            request.AddStringBody(obj, DataFormat.Json);
            request.AddHeader("Content-Type", "application/json");
            int i = 0;
            m1: i++;
            RestResponse response = restClient.ExecuteAsync(request).Result;
            try
            {
             if (!response.IsSuccessStatusCode && i < 10) { await Task.Delay(50000); goto m1; }


                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            catch (Exception ex)
            {
                return default(T);
            }


        }
        public static async Task<bool> PutApiRespons(string Url, string obj, string Auth)
        {

            var client = new RestClient();
            var request = new RestRequest(Url, Method.Put);
             request.AddHeader("Authorization", Auth);
            request.AddStringBody(obj, DataFormat.Json);
            int i = 0;
            request.AddHeader("Content-Type", "application/json");
            m1: i++;
            RestResponse response = await client.ExecuteAsync(request);
            if (!response.IsSuccessStatusCode && i < 10) { await Task.Delay(50000); goto m1; }

            return response.IsSuccessStatusCode;
            //RestClient restClient = new RestClient();
            //var request = new RestRequest(Url, Method.Put);
            //request.AddHeader("Authorization", Auth);
            //request.AddStringBody(obj, DataFormat.Json);
            //int i = 0;
            //request.AddHeader("Content-Type", "application/json");
            //m1: i++;

            //var vs= restClient.ExecuteAsync(request);
            //vs.Wait();
            //RestResponse response = vs.Result;
            //try
            //{  

            //    return response.IsSuccessStatusCode;
            //}
            //catch (Exception ex)
            //{
            //    return false;
            //}


        }
        public static async Task<T> GetApiRespons(string Url,   string Auth)
        {
             T  api  ;
            RestClient restClient = new RestClient();
            var request = new RestRequest(Url, Method.Get);
            request.AddHeader("Authorization", Auth);
            int i = 0;
            m1: i++;
            RestResponse response = restClient.GetAsync(request).Result;
            try
            {
               if (!response.IsSuccessStatusCode && i < 10) { await Task.Delay(50000); goto m1; }

            
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            catch (Exception ex)
            {
                return default(T);
            }


        }
        public static async Task<string> GetApiResponsString(string Url, string Auth)
        {
            T api;
            RestClient restClient = new RestClient();
            var request = new RestRequest(Url, Method.Get);
            request.AddHeader("Authorization", Auth);
            int i = 0;
            m1: i++;
            RestResponse response = restClient.GetAsync(request).Result;
            try
            {
                if (!response.IsSuccessStatusCode && i < 10) { await Task.Delay(50000); goto m1; }


                return (response.Content);
            }
            catch (Exception ex)
            {
                return "";
            }


        }
    }
}
