using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Arcmage.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Arcmage.Client
{
    public class ApiClient : IDisposable
    {
        public string BaseAddress { get; private set; }
        
        private HttpClient _httpClient { get; set; }

        private string _login { get; set; }
        private string _password { get; set; }

        private static Dictionary<Type,string> RouteMapping = new Dictionary<Type, string>();

        private static JsonMediaTypeFormatter JsonMediaTypeFormatter { get; set; }

        static ApiClient()
        {

            foreach (var routemap in Routes.RouteMapping)
            {
                RouteMapping.Add(routemap.Key,routemap.Value);
            }

            JsonMediaTypeFormatter = new JsonMediaTypeFormatter();

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,

            };
            settings.Converters.Add(new StringEnumConverter());
            JsonMediaTypeFormatter.SerializerSettings = settings;
        
        }

        public ApiClient(string baseAddress, string login, string password)
        {
            _login = login;
            _password = GetMd5Hash(password);
            BaseAddress = baseAddress;
        }

        public async Task Login()
        {
            await GetHttpClient();
        }

        public async Task<T> Create<T>(T item) where T : class
        {
            return await AddAsync(RouteMapping[typeof(T)], item);
        }

        public async Task<T> GetByGuid<T>(string guid) where T : class
        {
            return await ReadAsync<T>(RouteMapping[typeof(T)], guid);
        }

        public async Task<T> Get<T>() where T : class
        {
            return await ReadAsync<T>(RouteMapping[typeof(T)]);
        }

        public async Task<T> GetByGuid<T>(Guid guid) where T : class
        {
            return await ReadAsync<T>(RouteMapping[typeof(T)], guid.ToString());
        }

        public async Task<ResultList<T>> GetAll<T>() where T : class
        {
            return await ListAsync<T>(RouteMapping[typeof(T)]);
        }

        public async Task Update<T>(T item) where T : Base
        {
            await UpdateAsync(RouteMapping[typeof(T)], item);
        }

        public async Task Delete<T>(T item) where T : Base
        {
            await DeleteAsync(RouteMapping[typeof(T)], item);
        }

      

      

     

        #region helpers

        private async Task<HttpClient> GetHttpClient()
        {
            if (_httpClient == null)
            {
              

                _httpClient = new HttpClient(new HttpClientHandler { UseCookies = false })
                {
                    BaseAddress = new Uri(BaseAddress),
                    Timeout = new TimeSpan(0, 10, 0)
                };
                var login = new Login()
                {
                    Email = _login,
                    Password = _password
                };

                var response = await _httpClient.PostAsync(RouteMapping[typeof(Login)], login, JsonMediaTypeFormatter);
                response.EnsureSuccessStatusCode();
                var token = await response.Content.ReadAsStringAsync();
                token = token.Trim('"');
                //_httpClient.DefaultRequestHeaders.Add("Cookie", "Authorization=" + HttpUtility.UrlEncode(token));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            }

         


            return _httpClient;
        }

        static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
          
        }

        private async Task<ResultList<T>> ListAsync<T>(string route) where T : class
        {


            var httpClient = await GetHttpClient();

            // build uri
            var uri = $"{route}";

            var response = await httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode) return null;

            return await HttpContentExtensions.ReadAsAsync<ResultList<T>>(response.Content);
        }



        private async Task<T> ReadAsync<T>(string route, string guid) where T : class
        {
            

            var httpClient = await GetHttpClient();

            // build uri
            var uri = $"{route}/{guid}";

            var response = await httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode) return null;

            return await HttpContentExtensions.ReadAsAsync<T>(response.Content);
        }

        private async Task<T> ReadAsync<T>(string route) where T : class
        {


            var httpClient = await GetHttpClient();

            // build uri
            var uri = $"{route}";

            var response = await httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode) return null;

            return await HttpContentExtensions.ReadAsAsync<T>(response.Content);
        }

        private async Task<T> AddAsync<T>(string route, T entity)
        {
            var httpClient = await GetHttpClient();

            // build uri
            var response = await httpClient.PostAsync(route, entity, JsonMediaTypeFormatter);

            response.EnsureSuccessStatusCode();

            return await HttpContentExtensions.ReadAsAsync<T>(response.Content);
        }

        private async Task UpdateAsync<T>(string route, T entity) where T : Base
        {
            var httpClient = await GetHttpClient();

            var uri = $"{route}/{entity.Guid.ToString()}";


            var method = new HttpMethod("PATCH");

            var request = new HttpRequestMessage(method, uri)
            {
                Content = new ObjectContent<T>(entity, JsonMediaTypeFormatter)
            };

          
            var response = await httpClient.SendAsync(request);

            // build uri
            // var response = await httpClient.PatchAsync(route, entity, JsonMediaTypeFormatter);

            response.EnsureSuccessStatusCode();
        }

        private async Task DeleteAsync<T>(string route, T entity) where T : Base
        {
            var httpClient = await GetHttpClient();

            // build uri
            var uri = $"{route}/{entity.Guid}";
            var response = await httpClient.DeleteAsync(uri);

            response.EnsureSuccessStatusCode();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        #endregion


        public async Task DownloadFile(string route, string filename)
        {
            if(File.Exists(filename)) File.Delete(filename);

            var httpClient = await GetHttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(route);

            if (response.IsSuccessStatusCode)
            {
                System.Net.Http.HttpContent content = response.Content;
                var contentStream = await content.ReadAsStreamAsync(); 
                using (var stream = new FileStream(filename, FileMode.Create))
                {
                    await contentStream.CopyToAsync(stream);
                }
            }
            else
            {
                throw new FileNotFoundException();
            }
        }


        public async Task<HttpStatusCode> UploadFile(string guid, string filename)
        {
            byte[] image = File.ReadAllBytes(filename);
            var httpClient = await GetHttpClient();
            using (var content =
                new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
            {
                content.Add(new StreamContent(new MemoryStream(image)), "artork", "artwork.png");

                using (var message = await httpClient.PostAsync($"/api/FileUpload/{guid}", content))
                {
                    message.EnsureSuccessStatusCode();
                    return message.StatusCode;
                }
            }
        }

        public async Task<ResultList<T>> Search<T,TS>(TS searchOptions)
        {
            return await SearchAsync<T,TS>(RouteMapping[typeof(TS)], searchOptions);
        }

        private async Task<ResultList<T>> SearchAsync<T,TS>(string route, TS entity)
        {
            var httpClient = await GetHttpClient();

            // build uri
            var response = await httpClient.PostAsync(route, entity, JsonMediaTypeFormatter);

            response.EnsureSuccessStatusCode();

            return await HttpContentExtensions.ReadAsAsync<ResultList<T>>(response.Content);
        }


      


    }
}
