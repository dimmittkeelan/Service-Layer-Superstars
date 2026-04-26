using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services;
using LibraryApi.Dtos;

namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class BorrowRecordController : ControllerBase
    {
        private readonly IBorrowRecordService _borrowRecordService;

        public BorrowRecordController(IBorrowRecordService borrowRecordService)
        {
            _borrowRecordService = borrowRecordService;
        }

        [HttpPost("borrow")]
        public async Task<ActionResult<BorrowRecordResponse>> BorrowBook([FromBody] BorrowRequest input)
        {
            if (input.BookId == Guid.Empty)
            {
                return BadRequest(new ErrorResponse("BookId is required."));
            }

            if (input.MemberId == Guid.Empty)
            {
                return BadRequest(new ErrorResponse("MemberId is required."));
            }

            try
            {
                var borrowRecord = await _borrowRecordService.BorrowBookAsync(input);
                return CreatedAtAction(nameof(GetMemberBorrowHistory), new { memberId = borrowRecord.MemberId }, borrowRecord);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
        }

        [HttpPost("return")]
        public async Task<ActionResult<BorrowRecordResponse>> ReturnBook([FromBody] ReturnRequest input)
        {
            if (input.BorrowRecordId == Guid.Empty)
            {
                return BadRequest(new ErrorResponse("BorrowRecordId is required."));
            }

            try
            {
                var borrowRecord = await _borrowRecordService.ReturnBookAsync(input);
                return Ok(borrowRecord);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
        }

        [HttpGet("borrow-records")]
        public async Task<ActionResult<IEnumerable<BorrowRecordResponse>>> GetAllBorrowRecords()
        {
            try
            {
                var records = await _borrowRecordService.GetAllBorrowRecordsAsync();
                return Ok(records);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
        }

        [HttpGet("borrow-records/member/{memberId:guid}")]
        public async Task<ActionResult<IEnumerable<BorrowRecordResponse>>> GetMemberBorrowHistory(Guid memberId)
        {
            if (memberId == Guid.Empty)
            {
                return BadRequest(new ErrorResponse("MemberId is required."));
            }

            try
            {
                var records = await _borrowRecordService.GetMemberBorrowHistoryAsync(memberId);
                return Ok(records);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
        }
    }
}
