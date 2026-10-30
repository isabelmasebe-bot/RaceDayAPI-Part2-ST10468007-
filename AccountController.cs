using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.PostAsJsonAsync("api/auth/login", new
            {
                email = model.Email,
                password = model.Password
            });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            // Save user info in MVC session
            HttpContext.Session.SetInt32("UserId", json.GetProperty("userId").GetInt32());
            HttpContext.Session.SetString("Email", json.GetProperty("email").GetString()!);
            HttpContext.Session.SetString("Role", json.GetProperty("role").GetString()!);

            var role = json.GetProperty("role").GetString();

            // Redirect based on role
            if (role == "Organiser")
                return RedirectToAction("Dashboard", "Organiser");
            else
                return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            object registerDto;

            if (model.Role == "Organiser")
            {
                registerDto = new
                {
                    email = model.Email,
                    password = model.Password,
                    role = "Organiser",
                    organisationName = model.OrganisationName ?? "My Organisation",
                    phone = model.Phone
                };
            }
            else
            {
                registerDto = new
                {
                    email = model.Email,
                    password = model.Password,
                    role = "Participant",
                    firstName = model.FirstName ?? "",
                    lastName = model.LastName ?? ""
                };
            }

            var response = await client.PostAsJsonAsync("api/auth/register", registerDto);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                ModelState.AddModelError("", "Email already exists.");
                return View(model);
            }

            if (!response.IsSuccessStatusCode)
            {
                // Show the real error from the API
                ModelState.AddModelError("", $"Registration failed: {response.StatusCode} - {responseBody}");
                return View(model);
            }

            TempData["Success"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}