using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Minimum Age")]
        public int? MinAge { get; set; }

        [Display(Name = "Maximum Age")]
        public int? MaxAge { get; set; }

        // Used when linking to an event
        public int EventId { get; set; }

        [Display(Name = "Entry Fee (R)")]
        public decimal EntryFee { get; set; }
    }
}