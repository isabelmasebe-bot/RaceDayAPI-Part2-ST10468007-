using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class EnrolmentsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EnrolmentsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsParticipant() =>
            HttpContext.Session.GetString("Role") == "Participant";

        // POST: /Enrolments/Create
        [HttpPost]
        public async Task<IActionResult> Create(int eventId, int categoryId)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.PostAsJsonAsync("api/enrolments", new
            {
                eventId,
                categoryId
            });

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "You have successfully enrolled!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Enrolment failed: {error}";
            }

            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        // GET: /Enrolments/MyEnrolments
        public async Task<IActionResult> MyEnrolments()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");
            var response = await client.GetAsync("api/enrolments/me");

            var enrolments = new List<JsonElement>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                enrolments = json.EnumerateArray().ToList();
            }

            return View(enrolments);
        }

        // GET: /Enrolments/MyResults
        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");
            var response = await client.GetAsync("api/users/me/results");

            var results = new List<JsonElement>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                results = json.EnumerateArray().ToList();
            }

            return View(results);
        }
    }
}