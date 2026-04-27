using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApi.Services;
using LibraryApi.Dtos;

namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api/borrow")]
    public class BorrowRecordController : ControllerBase
    {
        private readonly IBorrowRecordService _borrowRecordService;

        public BorrowRecordController(IBorrowRecordService borrowRecordService)
        {
            _borrowRecordService = borrowRecordService;
        }

        [HttpPost]
        public async Task<ActionResult<BorrowRecordResponse>> BorrowBook([FromBody] BorrowRequest input)
        {
            if (input.BookId == Guid.Empty)
                return BadRequest(new ErrorResponse("BookId is required."));
            if (input.MemberId == Guid.Empty)
                return BadRequest(new ErrorResponse("MemberId is required."));

            try
            {
                var record = await _borrowRecordService.BorrowBookAsync(input);
                return CreatedAtAction(nameof(GetMemberBorrowHistory), new { memberId = record.MemberId }, record);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new ErrorResponse("The book is no longer available due to a concurrent request. Please try again."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
        }

        // POST /api/borrow/return
        [HttpPost("return")]
        public async Task<ActionResult<BorrowRecordResponse>> ReturnBook([FromBody] ReturnRequest input)
        {
            if (input.BorrowRecordId == Guid.Empty)
                return BadRequest(new ErrorResponse("BorrowRecordId is required."));

            try
            {
                var record = await _borrowRecordService.ReturnBookAsync(input);
                return Ok(record);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
        }

        // GET /api/borrow
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BorrowRecordResponse>>> GetAllBorrowRecords()
        {
            var records = await _borrowRecordService.GetAllBorrowRecordsAsync();
            return Ok(records);
        }

        // GET /api/borrow/member/{memberId}
        [HttpGet("member/{memberId:guid}")]
        public async Task<ActionResult<IEnumerable<BorrowRecordResponse>>> GetMemberBorrowHistory(Guid memberId)
        {
            if (memberId == Guid.Empty)
                return BadRequest(new ErrorResponse("MemberId is required."));

            try
            {
                var records = await _borrowRecordService.GetMemberBorrowHistoryAsync(memberId);
                return Ok(records);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
        }
    }
}
