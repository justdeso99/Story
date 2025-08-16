
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace Exam_Desislav_Lechev
{
    [TestFixture]
    public class StorySpoilerTests
    {
        private RestClient _client;
        private static string createStorySpoilerId;
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("justdeso", "deso123");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            _client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }

        [Test, Order(1)]
        public void CreateStory_ShouldReturnCreated()
        {
            var story = new
            {
                Title = "New Story Title",
                Description = "Test story description",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Successfully created!"));

            createStorySpoilerId = json.GetProperty("storyId").GetString();
        }

        [Test, Order(2)]
        public void EditStoryTitle_ShouldReturnOk()
        {
            var changes = new
            {
                Title = "Edited Title",
                Description = "Edited story description",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{createStorySpoilerId}", Method.Put);
            request.AddJsonBody(changes);

            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllStorySpoilers_ShouldReturnNonEmptyArray()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);

            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var stories = JsonSerializer.Deserialize<List<object>>(response.Content);

            Assert.That(stories, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteStorySpoiler_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Story/Delete/{createStorySpoilerId}", Method.Delete);

            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateStoryWithoutRequiredFields_ShouldReturnBadRequest()
        {
            var incompleteStory = new
            {
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(incompleteStory);

            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            var changes = new
            {
                Title = "Edited Title",
                Description = "Edited description",
                Url = ""
            };

            var fakeId = "123";

            var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put);
            request.AddJsonBody(changes);

            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            var fakeId = "123";

            var request = new RestRequest($"/api/Story/Delete/{fakeId}", Method.Delete);

            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Unable to delete this story spoiler!"));
        }



        [OneTimeTearDown]
        public void CleanUp()
        {
            _client.Dispose();
        }
    }
}