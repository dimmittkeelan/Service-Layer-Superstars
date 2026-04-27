using LibraryApi.DTOs.Borrow;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/borrow")]
public class BorrowController : ControllerBase
{
    private readonly IBorrowService _borrowService;

    public BorrowController(IBorrowService borrowService)
    {
        _borrowService = borrowService;
    }

    [HttpPost("borrow")]
    public async Task<ActionResult<BorrowRecordResponseDto>> Borrow([FromBody] BorrowBookRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _borrowService.BorrowAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("return")]
    public async Task<ActionResult<BorrowRecordResponseDto>> Return([FromBody] ReturnBookRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _borrowService.ReturnAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<BorrowRecordResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var records = await _borrowService.GetAllAsync(cancellationToken);
        return Ok(records);
    }

    [HttpGet("member/{memberId:int}")]
    public async Task<ActionResult<IReadOnlyCollection<BorrowRecordResponseDto>>> GetByMember(int memberId, CancellationToken cancellationToken)
    {
        var records = await _borrowService.GetByMemberIdAsync(memberId, cancellationToken);
        return Ok(records);
    }
}
