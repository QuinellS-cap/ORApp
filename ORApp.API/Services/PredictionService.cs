// ORApp.API/Services/FixtureService.cs (Assuming it exists in the repo structure)

using ORApp.Data; // For entities
using ORApp.Shared.Models; // For DTOs
using Microsoft.EntityFrameworkCore; // For Include

namespace ORApp.API.Services
{
    public class PredictionService : IFixtureService
    {
        private readonly OddsRaidersDbContext _context;

        public PredictionService(OddsRaidersDbContext context)
        {
            _context = context;
        }

        public async Task<List<FixtureDto>> GetFixturesAsync()
        {
            // Fetch fixtures and include related data
            var fixtureEntities = await _context.Fixtures
                .Include(f => f.Venue)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .ToListAsync();

            // Map entities to DTOs
            var fixtureDtos = fixtureEntities.Select(f => new FixtureDto
            {
                Id = f.Id,
                Sport = f.Sport,
                League = f.League,
                Date = f.Date,
                VenueName = f.Venue.Name, // Assuming navigation properties are set up
                HomeTeamName = f.HomeTeam.Name,
                AwayTeamName = f.AwayTeam.Name
            }).ToList();

            return fixtureDtos;
        }
    }
}