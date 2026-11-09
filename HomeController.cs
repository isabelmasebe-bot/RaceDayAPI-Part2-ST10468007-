using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(string? status = "Upcoming")
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var url = string.IsNullOrEmpty(status)
                ? "api/events"
                : $"api/events?status={status}";

            var response = await client.GetAsync(url);
            var events = new List<JsonElement>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                events = json.EnumerateArray().ToList();
            }

            ViewBag.CurrentStatus = status ?? "Upcoming";
            return View(events);
        }
    }
}