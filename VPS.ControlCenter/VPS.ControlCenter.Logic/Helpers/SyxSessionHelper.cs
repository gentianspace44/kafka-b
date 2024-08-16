using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using VPS.ControlCenter.Logic.Models;

namespace VPS.ControlCenter.Logic.Helpers
{
    public class SyxSessionHelper
    {
        private static IConfiguration _configuration;
        public static string _syxEndPoint { get; set; }
        public static string _syxUserName { get; set; }
        public static string _syxPassword { get; set; }
        private static string _sessionToken;
        private static long _syxUserId;
        private static string TokenKeepAliveTimerInMinutes { get; set; }
        private static HttpClient _httpClient = new HttpClient();
        private static ILogger<SyxSessionHelper> _logger;


        public static void Initialize(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<SyxSessionHelper> logger)
        {
            _configuration = configuration;
            _syxEndPoint = _configuration["SyxSettings:SyXEndpoint"];
            _syxUserName = _configuration["SyxSettings:SyXUsername"];
            _syxPassword = _configuration["SyxSettings:SyXPassword"];
            TokenKeepAliveTimerInMinutes = _configuration["SyxSettings:TokenKeepAliveTimerInMinutes"];
            _logger = logger;

        }

        public static async Task ApiLogin()
        {
            try
            {
                ApiLoginRequest login = new ApiLoginRequest()
                {
                    username = _syxUserName,
                    password = _syxPassword
                };

                if(string.IsNullOrWhiteSpace(login.username) || string.IsNullOrWhiteSpace(login.password))
                {
                    _logger.LogError("SyxLoginHelper", "Failed to get SyxSessionToken on SyxLoginHelper with exception empty credentials");
                    throw new Exception($"Error: Missing _syxUserName or _syxPassword", new Exception("Empty Syx Credentials"));
                }

                var json = JsonConvert.SerializeObject(login);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogInformation("Sending LoginUser request for user: {user} to {postUrl}", args: new object[] { _syxUserName, $"{_syxEndPoint}/Account/LoginUser" });

                var response = await _httpClient.PostAsync($"{_syxEndPoint}/Account/LoginUser", content);

                if (response == null)
                {
                    _logger.LogError("SyxLoginHelper", "Failed to get SyxSessionToken on SyxLoginHelper with exception empty response");
                    throw new Exception($"Error: {response?.ReasonPhrase}", new Exception("Empty Syx Reponse"));
                }

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error: {response.ReasonPhrase}", new Exception(responseContent));
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonConvert.DeserializeObject<ApiLoginResponse>(responseData);

                _sessionToken = loginResponse?.SessionToken?.Trim();
                _syxUserId = loginResponse.ResponseObject.UserID;
                _logger.LogInformation("SyxApiLogin success. {UserName} successfully logged in ({_sessionToken}).", args: new object[] { _syxUserName, _sessionToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get SyxSessionToken on SyxLoginHelper with exception: {exception}", args: new object[] { ex.ToString() });
            }
        }

        public static async Task KeepSessionAlive()
        {
            try
            {
                // Set up the HttpClient request
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_syxEndPoint}/Account/KeepAlive");
                request.Headers.Add("SessionToken", _sessionToken);

                // Send the request
                _logger.LogInformation("Sending KeepAlive request for user: {user} to {postUrl}", args: new object[] { _syxUserName, $"{_syxEndPoint}/Account/KeepAlive" });
                var response = await _httpClient.SendAsync(request);

                if (response == null)
                {
                    _logger.LogError("SyxLoginHelper", "Failed to get KeepAlive on SyxLoginHelper with exception empty response");
                    throw new Exception($"Error: {response?.ReasonPhrase}", new Exception("Empty Syx Reponse"));
                }

                // Check the response
                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error: {response.ReasonPhrase}", new Exception(responseContent));
                }

                // Read and process the response
                var responseData = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonConvert.DeserializeObject<ApiLoginResponse>(responseData);

                _sessionToken = loginResponse.SessionToken;
                _logger.LogInformation("KeepAlive success. {userName} ({sessionToken}).", args: new object[] { _syxUserName, _sessionToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "KeepAlive failed for user: {userName} {sessionToken}. Exception: {exception}", args: new object[] { _syxUserName, _sessionToken, ex.ToString() });
                await ApiLogin();
            }
        }

        public static void StartKeepAliveTimer()
        {
            int minutes;
            Int32.TryParse(TokenKeepAliveTimerInMinutes, out minutes);
            if (minutes < 1)
            {
                minutes = 8;
            };
            var timer = new System.Timers.Timer();
            timer.Interval = TimeSpan.FromMinutes(minutes).TotalMilliseconds;
            timer.Enabled = true;

            // Add handler for Elapsed event
            timer.Elapsed += Timer_Elapsed;
            timer.Stop();
            timer.Start();
        }

        private static async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SyxSessionHelper.GetSyXSessionToken()))
            {
                await SyxSessionHelper.KeepSessionAlive();
            }
            else
            {
                await ApiLogin();
            }
        }

        public static string GetSyXSessionToken()
        {
            return _sessionToken;
        }
        public static long GetSyXUserId()
        {
            return _syxUserId;
        }
        public static SyxSessionModel GetSyxSession()
        {
            return new SyxSessionModel { SyxSessionToken = _sessionToken, SyxUserId = _syxUserId };
        }

        public static async Task<SyxSessionModel> ForceRefresh()
        {
            await ApiLogin();
            return GetSyxSession();
        }
    }
}
