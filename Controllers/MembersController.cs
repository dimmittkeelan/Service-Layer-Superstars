using LibraryApi.DTOs.Members;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;

    public MembersController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<MemberResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var members = await _memberService.GetAllAsync(cancellationToken);
        return Ok(members);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MemberResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var member = await _memberService.GetByIdAsync(id, cancellationToken);
        return Ok(member);
    }

    [HttpPost]
    public async Task<ActionResult<MemberResponseDto>> Create([FromBody] CreateMemberRequestDto request, CancellationToken cancellationToken)
    {
        var created = await _memberService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MemberResponseDto>> Update(int id, [FromBody] UpdateMemberRequestDto request, CancellationToken cancellationToken)
    {
        var updated = await _memberService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _memberService.DeleteAsync(id, cancellationToken);
        return Ok();
    }
}
