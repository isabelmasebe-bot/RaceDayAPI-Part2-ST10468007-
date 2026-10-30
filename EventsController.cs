using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class EventsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EventsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsOrganiser() =>
            HttpContext.Session.GetString("Role") == "Organiser";

        // GET: /Events
        public async Task<IActionResult> Index()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");
            var response = await client.GetAsync("api/events");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Could not load events.";
                return View(new List<EventViewModel>());
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var events = new List<EventViewModel>();

            foreach (var item in json.EnumerateArray())
            {
                events.Add(new EventViewModel
                {
                    EventId = item.GetProperty("eventId").GetInt32(),
                    EventName = item.GetProperty("eventName").GetString() ?? "",
                    EventDate = item.GetProperty("eventDate").GetDateTime(),
                    Location = item.GetProperty("location").GetString() ?? "",
                    Status = item.TryGetProperty("status", out var s) ? s.GetString() ?? "Upcoming" : "Upcoming"
                });
            }

            return View(events);
        }

        // GET: /Events/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            return View(new EventViewModel());
        }

        // POST: /Events/Create
        [HttpPost]
        public async Task<IActionResult> Create(EventViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var dto = new
            {
                eventName = model.EventName,
                eventDate = model.EventDate,
                location = model.Location,
                distanceKm = model.DistanceKm,
                routeUrl = model.RouteUrl,
                weatherEnabled = model.WeatherEnabled,
                status = "Upcoming"
            };

            var response = await client.PostAsJsonAsync("api/events", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Failed to create event: {error}");
                return View(model);
            }

            // Optional: upload banner if a file was selected
            if (model.BannerFile != null && model.BannerFile.Length > 0)
            {
                var created = await response.Content.ReadFromJsonAsync<JsonElement>();
                int eventId = created.GetProperty("eventId").GetInt32();

                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(model.BannerFile.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(model.BannerFile.ContentType);
                content.Add(streamContent, "file", model.BannerFile.FileName);

                await client.PostAsync($"api/events/{eventId}/banner", content);
            }

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction("Index");
        }
        // GET: /Events/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");
            var response = await client.GetAsync($"api/events/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return View(json);
        }

    }
    
}