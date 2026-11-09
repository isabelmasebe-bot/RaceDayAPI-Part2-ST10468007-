using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

using System.Net.Http.Headers;
using System.Net.Http.Json;


namespace RaceDayMVC.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProfileController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsLoggedIn() =>
            HttpContext.Session.GetInt32("UserId") != null;

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");
            var response = await client.GetAsync("api/users/me");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Could not load profile.";
                return View();
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return View(json);
        }

        // POST: /Profile/Update
        [HttpPost]
        public async Task<IActionResult> Update(string? firstName, string? lastName,
            string? organisationName, string? phone, string? emergencyContact)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var role = HttpContext.Session.GetString("Role");
            object dto;

            if (role == "Organiser")
            {
                dto = new { organisationName, phone };
            }
            else
            {
                dto = new { firstName, lastName, emergencyContact };
            }

            var response = await client.PutAsJsonAsync("api/users/me", dto);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Profile updated successfully.";
            else
                TempData["Error"] = "Failed to update profile.";

            return RedirectToAction("Index");
        }

        // POST: /Profile/UploadPicture
        [HttpPost]
        public async Task<IActionResult> UploadPicture(IFormFile file)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file.";
                return RedirectToAction("Index");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            var response = await client.PostAsync("api/users/me/profile-picture", content);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Profile picture uploaded.";
            else
                TempData["Error"] = "Failed to upload picture.";

            return RedirectToAction("Index");
        }
    }
} 
