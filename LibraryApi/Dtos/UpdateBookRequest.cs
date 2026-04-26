using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Dtos
{
    public class UpdateBookRequest : IValidatableObject
    {
        public string? Title { get; set; }

        public string? Author { get; set; }

        public string? ISBN { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "TotalCopies must be greater than 0.")]
        public int? TotalCopies { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "AvailableCopies must be greater than or equal to 0.")]
        public int? AvailableCopies { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Title is not null && string.IsNullOrWhiteSpace(Title))
            {
                yield return new ValidationResult(
                    "Title cannot be empty when provided.",
                    new[] { nameof(Title) });
            }

            if (Author is not null && string.IsNullOrWhiteSpace(Author))
            {
                yield return new ValidationResult(
                    "Author cannot be empty when provided.",
                    new[] { nameof(Author) });
            }

            if (ISBN is not null && string.IsNullOrWhiteSpace(ISBN))
            {
                yield return new ValidationResult(
                    "ISBN cannot be empty when provided.",
                    new[] { nameof(ISBN) });
            }

            if (AvailableCopies.HasValue && TotalCopies.HasValue && AvailableCopies.Value > TotalCopies.Value)
            {
                yield return new ValidationResult(
                    "AvailableCopies must not exceed TotalCopies.",
                    new[] { nameof(AvailableCopies), nameof(TotalCopies) });
            }
        }
    }
}