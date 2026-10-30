using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrganiserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Simple check – only Organisers can access
        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("Role") == "Organiser";
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // Get all events (we will filter by organiser later if needed)
            var response = await client.GetAsync("api/events");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Could not load events.";
                return View(new List<object>());
            }

            var events = await response.Content.ReadFromJsonAsync<JsonElement>();
            return View(events);
        }
    }
}