using System.Threading.Tasks;
using NUnit.Framework;
using RestSharp;
using TechTalk.SpecFlow;

namespace WebApiTesting.SpecFlow.StepDefinitions
{
    // Обмежуємо застосування цього класу лише для сценаріїв з Joke API
    [Binding, Scope(Feature = "Joke API Testing")]
    public class JokeApiSteps
    {
        private RestClient _client;
        private RestResponse _response;

        [Given(@"I have the Joke API endpoint ""(.*)""")]
        public void GivenIHaveTheJokeAPIEndpoint(string endpoint)
        {
            _client = new RestClient(endpoint);
        }

        [When(@"I send a GET request to ""(.*)""")]
        public async Task WhenISendAGETRequestTo(string resource)
        {
            var request = new RestRequest(resource, Method.Get);
            _response = await _client.ExecuteAsync(request);

        }

        [Then(@"the response status code should be (\d+)")]
        public void ThenTheResponseStatusCodeShouldBe(int expectedStatusCode)
        {
            Assert.AreEqual(expectedStatusCode, (int)_response.StatusCode);
        }
    }
}
