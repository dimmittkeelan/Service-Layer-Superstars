using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services;
using LibraryApi.Dtos;

namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<BookResponse>>> GetBooks(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _bookService.GetBooksAsync(search, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BookResponse>> GetBookById(Guid id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
                return NotFound(new ErrorResponse("Book not found."));
            return Ok(book);
        }

        [HttpPost]
        public async Task<ActionResult> CreateBook([FromBody] CreateBookRequest input)
        {
            if (string.IsNullOrWhiteSpace(input.Title))
                return BadRequest(new ErrorResponse("Title is required."));
            if (string.IsNullOrWhiteSpace(input.Author))
                return BadRequest(new ErrorResponse("Author is required."));
            if (string.IsNullOrWhiteSpace(input.ISBN))
                return BadRequest(new ErrorResponse("ISBN is required."));

            try
            {
                var created = await _bookService.CreateBookAsync(input);
                return CreatedAtAction(nameof(GetBookById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult> UpdateBook(Guid id, [FromBody] UpdateBookRequest input)
        {
            try
            {
                var updated = await _bookService.UpdateBookAsync(id, input);
                if (updated == null)
                    return NotFound(new ErrorResponse("Book not found."));
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteBook(Guid id)
        {
            var deleted = await _bookService.DeleteBookAsync(id);
            if (deleted == null)
                return NotFound(new ErrorResponse("Book not found."));
            return Ok(deleted);
        }
    }
}
