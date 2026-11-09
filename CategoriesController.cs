using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CategoriesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsOrganiser() =>
            HttpContext.Session.GetString("Role") == "Organiser";

        // GET: /Categories?eventId=5 / index method 
        public async Task<IActionResult> Index(int eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (eventId == 0)
                return RedirectToAction("Index", "Events");

            ViewBag.EventId = eventId;

            var client = _httpClientFactory.CreateClient("RaceDayAPI");
            var linkedCategories = new List<JsonElement>();

            try
            {
                var response = await client.GetAsync($"api/events/{eventId}/categories");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                    linkedCategories = json.EnumerateArray().ToList();
                }
            }
            catch { }

            ViewBag.LinkedCategories = linkedCategories;
            return View();
        }

        // GET: /Categories/Create?eventId=5
        [HttpGet]
        public IActionResult Create(int eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            return View(new CategoryViewModel { EventId = eventId });
        }

        // POST: /Categories/Create
        [HttpPost]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            // 1. Create the global category
            var createResponse = await client.PostAsJsonAsync("api/categories", new
            {
                categoryName = model.CategoryName,
                minAge = model.MinAge,
                maxAge = model.MaxAge
            });

            if (!createResponse.IsSuccessStatusCode)
            {
                var error = await createResponse.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Failed to create category: {error}");
                return View(model);
            }

            var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            int categoryId = created.GetProperty("categoryId").GetInt32();

            // 2. Link it to the event with entry fee
            var linkResponse = await client.PostAsJsonAsync($"api/events/{model.EventId}/categories", new
            {
                categoryId = categoryId,
                entryFee = model.EntryFee
            });

            if (!linkResponse.IsSuccessStatusCode)
            {
                var error = await linkResponse.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Category created but failed to link to event: {error}");
                return View(model);
            }

            TempData["Success"] = "Category added successfully!";
            return RedirectToAction("Index", new { eventId = model.EventId });
        }
    }
}