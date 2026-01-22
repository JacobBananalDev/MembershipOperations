using MembershipOperations.Domain.Entities;
using MembershipOperations.Domain.Services;
using MembershipOperations.Infrastructure.Persistence;
using MembershipOperations.Shared.Dto.Common;
using MembershipOperations.Shared.Dto.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MembershipOperations.Api.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditLogger _audit;

    public MembersController(ApplicationDbContext db, IAuditLogger audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<MemberDto>>> GetMembers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 25 : pageSize;
        pageSize = Math.Min(pageSize, 100);

        var query = _db.Members.AsNoTracking().AsQueryable();

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(m =>
                m.FirstName.Contains(search) ||
                m.LastName.Contains(search) ||
                (m.Email != null && m.Email.Contains(search)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(m => m.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MemberDto
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Email = m.Email,
                IsActive = m.IsActive,
                CreatedAtUtc = m.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(new PagedResult<MemberDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MemberDto>> GetMemberById(int id)
    {
        var member = await _db.Members.AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MemberDto
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Email = m.Email,
                IsActive = m.IsActive,
                CreatedAtUtc = m.CreatedAtUtc
            })
            .FirstOrDefaultAsync();

        if (member == null)
            return NotFound();

        return Ok(member);
    }

    [HttpPost]
    public async Task<ActionResult<MemberDto>> CreateMember([FromBody] CreateMemberRequest request)
    {
        // Optional uniqueness check
        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();

        if (email != null)
        {
            var emailExists = await _db.Members.AnyAsync(m =>
                m.Email != null && m.Email.ToLower() == email.ToLower());

            if (emailExists)
                return Conflict(new ProblemDetails { Title = "Email already exists." });
        }

        var entity = new Member
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Members.Add(entity);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(
    actor: User.Identity?.Name ?? "unknown",
    action: "MemberCreated",
    entityType: "Member",
    entityId: entity.Id.ToString(),
    detailsJson: System.Text.Json.JsonSerializer.Serialize(new
    {
        entity.FirstName,
        entity.LastName,
        entity.Email
    })
);

        var dto = new MemberDto
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc
        };

        return CreatedAtAction(nameof(GetMemberById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateMember(int id, [FromBody] UpdateMemberRequest request)
    {
        var entity = await _db.Members.FirstOrDefaultAsync(m => m.Id == id);
        if (entity == null)
            return NotFound();

        // Optional uniqueness check if email changes
        var newEmail = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        if (newEmail != null && !string.Equals(newEmail, entity.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailExists = await _db.Members.AnyAsync(m => m.Email == newEmail && m.Id != id);
            if (emailExists)
                return Conflict(new ProblemDetails { Title = "Email already exists." });
        }

        entity.FirstName = request.FirstName.Trim();
        entity.LastName = request.LastName.Trim();
        entity.Email = newEmail;
        entity.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        await _audit.LogAsync(
    actor: User.Identity?.Name ?? "unknown",
    action: "MemberUpdated",
    entityType: "Member",
    entityId: entity.Id.ToString(),
    detailsJson: System.Text.Json.JsonSerializer.Serialize(new
    {
        entity.FirstName,
        entity.LastName,
        entity.Email,
        entity.IsActive
    })
);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var entity = await _db.Members.FirstOrDefaultAsync(m => m.Id == id);
        if (entity == null)
            return NotFound();

        // Soft-delete for traceability
        entity.IsActive = false;

        await _db.SaveChangesAsync();

        await _audit.LogAsync(
    actor: User.Identity?.Name ?? "unknown",
    action: "MemberDeactivated",
    entityType: "Member",
    entityId: entity.Id.ToString()
);


        return NoContent();
    }
}
