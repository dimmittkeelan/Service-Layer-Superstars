using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services;
using LibraryApi.Dtos;

namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api/members")]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public MemberController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberResponse>>> GetMembers()
        {
            var members = await _memberService.GetMembersAsync();
            return Ok(members);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<MemberResponse>> GetMemberById(Guid id)
        {
            var member = await _memberService.GetMemberByIdAsync(id);
            if (member == null)
            {
                return NotFound(new ErrorResponse("Member not found."));
            }
            return Ok(member);
        }

        [HttpPost]
        public async Task<ActionResult<MemberResponse>> CreateMember([FromBody] CreateMemberRequest input)
        {
            if (string.IsNullOrWhiteSpace(input.FullName))
            {
                return BadRequest(new ErrorResponse("FullName is required."));
            }

            if (string.IsNullOrWhiteSpace(input.Email))
            {
                return BadRequest(new ErrorResponse("Email is required."));
            }

            try
            {
                var member = await _memberService.CreateMemberAsync(input);
                return CreatedAtAction(nameof(GetMemberById), new { id = member.Id }, member);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<MemberResponse>> UpdateMember(Guid id, [FromBody] UpdateMemberRequest input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse("Invalid request data."));
            }

            try
            {
                var member = await _memberService.UpdateMemberAsync(id, input);
                if (member == null)
                {
                    return NotFound(new ErrorResponse("Member not found."));
                }
                return Ok(member);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteMember(Guid id)
        {
            var member = await _memberService.GetMemberByIdAsync(id);
            if (member == null)
            {
                return NotFound(new ErrorResponse("Member not found."));
            }

            await _memberService.DeleteMemberAsync(id);
            return Ok(new { message = "Member deleted successfully." });
        }
    }
}
