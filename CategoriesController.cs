using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    /// <summary>
    /// Manage global categories that can be linked to events.
    /// </summary>
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string? GetRole() => HttpContext.Session.GetString("Role");

        /// <summary>
        /// Get a list of all available categories.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
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

        /// <summary>
        /// Create a new global category. Only Organisers can perform this action.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            if (GetRole() != "Organiser")
                return StatusCode(403, "Only Organisers can create categories.");

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

        /// <summary>
        /// Update an existing category. Only Organisers can perform this action.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryDto dto)
        {
            if (GetRole() != "Organiser")
                return StatusCode(403, "Only Organisers can update categories.");

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

        /// <summary>
        /// Delete a category. Only allowed if the category is not linked to any event.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (GetRole() != "Organiser")
                return StatusCode(403, "Only Organisers can delete categories.");

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