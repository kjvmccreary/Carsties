using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionDbContext : DbContext
{
    public AuctionDbContext(DbContextOptions options) : base(options)
    {
        
    }
    
    public DbSet<Auction> Auctions { get; set; }
    // Don't need to specify Item entity in here because it's related to the Auction
    //  entity. As such, Entity Framework will go ahead and create it anyway. Who knew. 
}