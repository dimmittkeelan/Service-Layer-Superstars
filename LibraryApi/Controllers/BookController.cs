using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services;
using LibraryApi.Models;
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
        public ActionResult<IEnumerable<Book>> GetBooks()
        {
            var books = _bookService.GetBooks();
            return Ok(books);
        }
        [HttpGet("{id:guid}")]
        public ActionResult<Book> GetBookById(Guid id)
        {
            var book = _bookService.GetBookById(id);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }
        [HttpPost]
        public ActionResult CreateBook([FromBody] CreateBookRequest input)
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
            var created = _bookService.CreateBook(input);
            return CreatedAtAction(nameof(GetBookById), new { id = created.Id }, created);
        }

    }
}