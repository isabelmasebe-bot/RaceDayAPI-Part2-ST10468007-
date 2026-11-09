using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace RaceDayMVC.Controllers
{
    public class ResultsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ResultsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsOrganiser() =>
            HttpContext.Session.GetString("Role") == "Organiser";

        // GET: /Results/Create?enrollmentId=1&eventId=1
        [HttpGet]
        public IActionResult Create(int enrollmentId, int eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            ViewBag.EnrollmentId = enrollmentId;
            ViewBag.EventId = eventId;
            return View();
        }

        // POST: /Results/Create
        [HttpPost]
        public async Task<IActionResult> Create(int enrollmentId, int eventId,
            string? finishTime, int? finishPosition, string? performanceStatus)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.PostAsJsonAsync("api/results", new
            {
                enrollmentId,
                finishTime,
                finishPosition,
                performanceStatus
            });

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Result saved successfully.";
                return RedirectToAction("Enrolments", "Events", new { id = eventId });
            }

            var error = await response.Content.ReadAsStringAsync();
            ViewBag.Error = error;
            ViewBag.EnrollmentId = enrollmentId;
            ViewBag.EventId = eventId;
            return View();
        }
    }
}