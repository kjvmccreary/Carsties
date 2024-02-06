using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    // GET
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        var query = DB.PagedSearch<Item,Item>();

        // query.Sort(c => c.Ascending(a => a.Make));

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(c => c.Ascending(d => d.Make)),
            "new" => query.Sort(c => c.Descending(d => d.CreatedAt)),
            _ => query.Sort(c => c.Ascending(d => d.AuctionEnd))
        };
        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(c => c.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Match(c => c.AuctionEnd < DateTime.UtcNow.AddHours(6)
                                             && c.AuctionEnd > DateTime.UtcNow),
            _ => query.Match(c => c.AuctionEnd > DateTime.UtcNow)
        };

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(c => c.Seller == searchParams.Seller);
        }

        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(c => c.Winner == searchParams.Winner);
        }

        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);
        
        var result = await query.ExecuteAsync();

        return Ok(new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
}