using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Dtos
{
    public class CreateBookRequest : IValidatableObject
    {
        [Required]
        [MinLength(1)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public string ISBN { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "TotalCopies must be greater than 0.")]
        public int TotalCopies { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "AvailableCopies must be greater than or equal to 0.")]
        public int AvailableCopies { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AvailableCopies > TotalCopies)
            {
                yield return new ValidationResult(
                    "AvailableCopies must not exceed TotalCopies.",
                    new[] { nameof(AvailableCopies), nameof(TotalCopies) });
            }
        }
    }
}