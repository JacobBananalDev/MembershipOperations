using MembershipOperations.Infrastructure.Persistence;
using MembershipOperations.Shared.Dto.Common;
using MembershipOperations.Shared.Dto.Members;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MembershipOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public MembersController(ApplicationDbContext db)
    {
        _db = db;
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
}
