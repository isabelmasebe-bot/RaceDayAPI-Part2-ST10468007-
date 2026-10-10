using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");
        private string? GetRole() => HttpContext.Session.GetString("Role");

        // GET: /api/categories
        // Public – list all available categories
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    c.CategoryId,
                    c.CategoryName,
                    c.MinAge,
                    c.MaxAge
                })
                .ToListAsync();

            return Ok(categories);
        }

        // POST: /api/categories
        // Organiser only – create a new global category
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            if (GetRole() != "Organiser")
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.CategoryName))
                return BadRequest("Category name is required.");

            var category = new Category
            {
                CategoryName = dto.CategoryName,
                MinAge = dto.MinAge,
                MaxAge = dto.MaxAge
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllCategories), new { id = category.CategoryId }, category);
        }

        // PUT: /api/categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryDto dto)
        {
            if (GetRole() != "Organiser")
                return Forbid();

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("Category not found.");

            if (!string.IsNullOrWhiteSpace(dto.CategoryName))
                category.CategoryName = dto.CategoryName;

            category.MinAge = dto.MinAge;
            category.MaxAge = dto.MaxAge;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Category updated successfully" });
        }

        // DELETE: /api/categories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (GetRole() != "Organiser")
                return Forbid();

            var category = await _context.Categories
                .Include(c => c.EventCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound("Category not found.");

            if (category.EventCategories.Any())
                return Conflict("Cannot delete category because it is linked to one or more events.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}