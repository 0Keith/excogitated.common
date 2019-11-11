using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Excogitated.Common
{
    public class SimpleHttpClient : IDisposable
    {
        private readonly AtomicStack<IDisposable> _resources = new AtomicStack<IDisposable>();
        private readonly HttpClientHandler _handler;
        private readonly HttpClient _client;

        public void Dispose() => _resources.Dispose();

        public SimpleHttpClient(Uri baseAddress = null)
        {
            _resources.Add(_handler = new HttpClientHandler());
            _resources.Add(_client = new HttpClient(_handler));
            _client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            if (baseAddress is null == false)
                _client.BaseAddress = baseAddress;
        }

        public void AddCertificate(string fileName) =>
            _handler.ClientCertificates.Add(_resources.AddAndGet(new X509Certificate2(fileName)));

        public void SetAccessToken(string tokenType, string accessToken) =>
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);

        public async Task<T> Post<T>(Uri uri, object data)
        {
            var response = await Post(uri, data);
            return Deserialize<T>(response);
        }

        public async Task<string> Post(Uri uri, object data)
        {
            var w = Stopwatch.StartNew();
            var json = Jsonizer.Serialize(data);
            while (true)
                using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                using (var response = await _client.PostAsync(uri, content))
                {
                    var message = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                        return message;
                    if (response.StatusCode.EqualsAny(HttpStatusCode.BadGateway, HttpStatusCode.InternalServerError))
                        if (w.Elapsed.TotalMinutes < 15)
                        {
                            await Task.Delay(5000);
                            continue;
                        }
                    Loggers.Error(message);
                    response.EnsureSuccessStatusCode();
                    return null;
                }
        }

        public async Task<string> Get(Uri uri, bool ensureSuccess = true)
        {
            var w = Stopwatch.StartNew();
            while (true)
                using (var response = await _client.GetAsync(uri))
                {
                    var message = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                        return message;
                    if (ensureSuccess)
                    {
                        if (response.StatusCode.EqualsAny(HttpStatusCode.BadGateway, HttpStatusCode.InternalServerError))
                            if (w.Elapsed.TotalMinutes < 15)
                            {
                                await Task.Delay(5000);
                                continue;
                            }
                        Loggers.Error(message);
                        response.EnsureSuccessStatusCode();
                    }
                    return message;
                }
        }

        public Task<string> Get(Uri uri, object queryObj) => Get(uri.Append(queryObj));

        public async Task<T> Get<T>(Uri uri)
        {
            var response = await Get(uri);
            return Deserialize<T>(response);
        }

        public async Task<T> Get<T>(Uri uri, object data)
        {
            var response = await Get(uri, data);
            return Deserialize<T>(response);
        }

        public T Deserialize<T>(string json)
        {
            try
            {
                //still need to find a way to detect if properties are missing
                return Jsonizer.Deserialize<T>(json);
            }
            catch (Exception e)
            {
                Loggers.Warn(e.Message);
            }
            return Jsonizer.Deserialize<T>(json);
        }
    }

    public static class HttpExtensions
    {
        public static Uri Append(this Uri uri, object queryObj)
        {
            var query = string.Join("&", GetValues(queryObj));
            if (!uri.Query.IsNullOrWhiteSpace())
                query = $"{uri.Query}&{query}";
            return new UriBuilder(uri) { Query = query }.Uri;
        }

        private static IEnumerable<string> GetValues(object queryObj) => queryObj.GetType().GetProperties()
            .Select(p => $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.GetValue(queryObj)?.ToString())}");
    }
}