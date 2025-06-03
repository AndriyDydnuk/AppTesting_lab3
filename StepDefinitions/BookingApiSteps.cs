using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using TechTalk.SpecFlow;
using System.Diagnostics; // Для логування

namespace WebApiTesting.SpecFlow.StepDefinitions
{
    // Обмежуємо застосування кроків цього класу для сценаріїв з Booking API
    [Binding, Scope(Feature = "Booking API CRUD Operations")]
    public class BookingApiSteps
    {
        private RestClient _client;
        private RestResponse _response;
        private readonly ScenarioContext _scenarioContext;
        private int _bookingId;

        public BookingApiSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given(@"I have the Restful Booker API endpoint ""(.*)""")]
        public void GivenIHaveTheRestfulBookerAPIEndpoint(string endpoint)
        {
            _client = new RestClient(endpoint);
        }

        [When(@"I send a GET request to ""(.*)""")]
        public async Task WhenISendAGETRequestTo(string resource)
        {
            var request = new RestRequest(resource, Method.Get);
            _response = await _client.ExecuteAsync(request);

            Debug.WriteLine("=== GET Raw Response ===");
            Debug.WriteLine(_response.Content);
        }

        [When(@"I send a POST request to ""(.*)"" with body:")]
        public async Task WhenISendAPOSTRequestToWithBody(string resource, string body)
        {
            var request = new RestRequest(resource, Method.Post);
            request.AddHeader("Content-Type", "application/json");

            // Відправка сирого JSON як Request Body (як вимагається API)
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            _response = await _client.ExecuteAsync(request);

            Debug.WriteLine("=== POST Raw Response ===");
            Debug.WriteLine(_response.Content);

            // Якщо відповідь є JSON (повинно містити поле bookingid)
            if (_response.Content.Trim().StartsWith("{"))
            {
                var settings = new JsonSerializerSettings { FloatParseHandling = FloatParseHandling.Decimal };
                dynamic jsonResponse = SafeDeserialize(_response.Content, settings);
                _bookingId = jsonResponse.bookingid;
                _scenarioContext["bookingId"] = _bookingId;
            }
            else
            {
                Debug.WriteLine("POST response is not valid JSON: " + _response.Content);
            }
        }

        [Given(@"I have an existing booking with id created earlier POST operation")]
        public async Task GivenIHaveAnExistingBookingWithIdCreatedEarlierPOSTOperation()
        {
            if (!_scenarioContext.ContainsKey("bookingId"))
            {
                await WhenISendAPOSTRequestToWithBody("booking", @"
                {
                     ""firstname"": ""John"",
                     ""lastname"": ""Doe"",
                     ""totalprice"": 150,
                     ""depositpaid"": true,
                     ""bookingdates"": {
                        ""checkin"": ""2025-06-01"",
                        ""checkout"": ""2025-06-10""
                     },
                     ""additionalneeds"": ""Breakfast""
                }
                ");
            }
        }

        [When(@"I send a PUT request to ""(.*)"" with body:")]
        public async Task WhenISendAPUTRequestToWithBody(string resource, string body)
        {
            if (resource.Contains("{id}"))
            {
                int id = _scenarioContext.ContainsKey("bookingId") ? (int)_scenarioContext["bookingId"] : _bookingId;
                resource = resource.Replace("{id}", id.ToString());
            }

            var request = new RestRequest(resource, Method.Put);
            request.AddHeader("Content-Type", "application/json");

            // Отримання токена автентифікації, згідно з документацією
            string token = await GetAuthToken();
            request.AddHeader("Cookie", "token=" + token);

            // Відправляємо дані на оновлення бронювання
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            _response = await _client.ExecuteAsync(request);

            Debug.WriteLine("=== PUT Raw Response ===");
            Debug.WriteLine(_response.Content);
        }

        [When(@"I send a DELETE request to ""(.*)""")]
        public async Task WhenISendADELETERequestTo(string resource)
        {
            if (resource.Contains("{id}"))
            {
                int id = _scenarioContext.ContainsKey("bookingId") ? (int)_scenarioContext["bookingId"] : _bookingId;
                resource = resource.Replace("{id}", id.ToString());
            }

            var request = new RestRequest(resource, Method.Delete);
            request.AddHeader("Content-Type", "application/json");

            // Отримання токена автентифікації для DELETE-запиту
            string token = await GetAuthToken();
            request.AddHeader("Cookie", "token=" + token);

            _response = await _client.ExecuteAsync(request);

            Debug.WriteLine("=== DELETE Raw Response ===");
            Debug.WriteLine(_response.Content);
        }

        [Then(@"the response status code should be (\d+)")]
        public void ThenTheResponseStatusCodeShouldBe(int expectedStatusCode)
        {
            Assert.AreEqual(expectedStatusCode, (int)_response.StatusCode);
        }

        // Допоміжний метод для безпечної десеріалізації JSON-відповіді
        private dynamic SafeDeserialize(string content, JsonSerializerSettings settings)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            Debug.WriteLine("Raw JSON content before processing: " + content);

            string trimmedContent = content.Trim();
            if (trimmedContent == "Infinity" || trimmedContent == "-Infinity")
            {
                Debug.WriteLine("Response content equals 'Infinity' or '-Infinity'; returning 0 instead.");
                return 0;
            }

            string pattern = @"(?<=:\s?)(-?Infinity)(?=[,\}])";
            string safeContent = Regex.Replace(content, pattern, "0");

            Debug.WriteLine("Safe JSON content after processing: " + safeContent);

            return JsonConvert.DeserializeObject(safeContent, settings);
        }

        // Отримання автентифікаційного токена через /auth, згідно з документацією API
        private async Task<string> GetAuthToken()
        {
            var request = new RestRequest("auth", Method.Post);
            request.AddHeader("Content-Type", "application/json");

            // Дані автентифікації згідно з документацією
            string authBody = @"{""username"":""admin"",""password"":""password123""}";
            request.AddParameter("application/json", authBody, ParameterType.RequestBody);

            var response = await _client.ExecuteAsync(request);

            Debug.WriteLine("=== Auth Response ===");
            Debug.WriteLine(response.Content);

            dynamic result = JsonConvert.DeserializeObject(response.Content);
            return (string)result.token;
        }
    }
}
