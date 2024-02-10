using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        // This will return an IQueryable.
        var query = _context.Auctions.OrderBy(c => c.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(c => c.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(c => c.Item)
            .FirstAsync(c => c.Id == id);

        if (auction == null) return NotFound();
        
        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        // TODO: add current user as seller
        auction.Seller = "test";
        _context.Auctions.Add(auction);

        // SaveChanges returns an integer for each entity saved. So if it returns 0, nothing was saved. If result, 
        //  therefore is 0, the save was not successful.
        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not save changes to the db.");
        
        return CreatedAtAction(nameof(GetAuctionById),new{auction.Id},
         _mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto auctionDto)
    {
        var auction = await _context.Auctions
            .Include(c => c.Item)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (auction == null) return NotFound("Unable to find auction.");
        
        // todo: check seller == username
        auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
        auction.Item.Year = auctionDto.Year ??  auction.Item.Year;
        auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = auctionDto.Mileage ??  auction.Item.Mileage;
        
        var result = await _context.SaveChangesAsync() > 0;

        if (result) return Ok();
        
        return BadRequest("Could not save changes to the db.");
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);
        if (auction == null) return NotFound("Unable to find auction.");
        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if (result) return Ok();
        return BadRequest("Could not save changes to the db.");
    }
}