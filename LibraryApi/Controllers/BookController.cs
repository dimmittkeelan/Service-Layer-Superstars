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
        public async Task<ActionResult<IEnumerable<BookResponse>>> GetBooks()
        {
            var books = await _bookService.GetBooksAsync();
            return Ok(books);
        }
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BookResponse>> GetBookById(Guid id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }
        [HttpPost]
        public async Task<ActionResult> CreateBook([FromBody] CreateBookRequest input)
        {
            if (string.IsNullOrWhiteSpace(input.Title))
            {
                return BadRequest("Title is required.");
            }
            else if (string.IsNullOrWhiteSpace(input.Author))
            {
                return BadRequest("Author is required.");
            }
            else if (string.IsNullOrWhiteSpace(input.ISBN))
            {
                return BadRequest("ISBN is required.");
            }
            var created = await _bookService.CreateBookAsync(input);
            return CreatedAtAction(nameof(GetBookById), new { id = created.Id }, created);
        }
        
        [HttpPut("{id:guid}")]
        public async Task<ActionResult> UpdateBook(Guid id, [FromBody] UpdateBookRequest input)
        {

            var updated = await _bookService.UpdateBookAsync(id, input);
            if (updated == null)
            {
                return NotFound();
            }
            return Ok(updated);
        }
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteBook(Guid id)
        {
            var deleted = await _bookService.DeleteBookAsync(id);
            if (deleted == null)
            {
                return NotFound();
            }
            return Ok(deleted);
        }

    }
}