using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;

namespace NexaGram.API.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class DirectMessagesController : ControllerBase
{
    private readonly IDirectMessageService _dms;

    public DirectMessagesController(IDirectMessageService dms) => _dms = dms;

    [HttpGet("conversations")]
    public async Task<ActionResult<IReadOnlyList<ConversationDto>>> GetConversations(CancellationToken ct)
    {
        var result = await _dms.GetConversationsAsync(GetUserId(), ct);
        return Ok(result);
    }

    [HttpGet("{participantId:guid}")]
    public async Task<ActionResult<PagedResult<DirectMessageDto>>> GetMessages(
        Guid participantId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken ct = default)
    {
        var result = await _dms.GetMessagesAsync(GetUserId(), participantId, page, pageSize, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DirectMessageDto>> Send(SendMessageRequest request, CancellationToken ct)
    {
        var result = await _dms.SendAsync(GetUserId(), request, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        await _dms.MarkReadAsync(id, GetUserId(), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _dms.DeleteAsync(id, GetUserId(), ct);
        return NoContent();
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
