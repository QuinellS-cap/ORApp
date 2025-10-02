using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ORApp.Data.Context;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ORApp.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Worker> _logger;

        public Worker(IHttpClientFactory httpClientFactory,IServiceProvider serviceProvider, ILogger<Worker> logger)
        {
            _httpClientFactory = httpClientFactory;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var client = _httpClientFactory.CreateClient("RapidApi");
            var request = new HttpRequestMessage(HttpMethod.Get, "/v3/predictions?fixture=198772");

            using var response = await client.SendAsync(request, stoppingToken);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync(stoppingToken);
            _logger.LogInformation("RapidAPI Response: {Body}", body);

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ORDBContext>();

            // 1. Predictions and related data
            var predictions = await db.Predictions
                .Include(p => p.Fixture)
                .ThenInclude(f => f.League)
                .Include(p => p.Fixture)
                    .ThenInclude(f => f.HomeTeam)
                .Include(p => p.Fixture)
                    .ThenInclude(f => f.AwayTeam)
                .ToListAsync(stoppingToken);

            foreach (var prediction in predictions)
            {
                var fixture = prediction.Fixture;
                _logger.LogInformation("Prediction {PredictionId}: Fixture {FixtureId}, League: {League}, Home: {Home}, Away: {Away}",
                    prediction.PredictionId,
                    fixture?.FixtureId,
                    fixture?.League?.Name,
                    fixture?.HomeTeam?.Name,
                    fixture?.AwayTeam?.Name
                );
            }

            // 2. Fixtures and related data
            var fixtures = await db.Fixtures
                .Include(f => f.League)
                .Include(f => f.Season)
                .Include(f => f.HomeTeam)
                .Include(f => f.AwayTeam)
                .Include(f => f.Venue)
                .Include(f => f.FixtureStatus)
                .ToListAsync(stoppingToken);

            foreach (var fixture in fixtures)
            {
                _logger.LogInformation("Fixture {FixtureId}: {Home} vs {Away} in {League} at {Venue}",
                    fixture.FixtureId,
                    fixture.HomeTeam?.Name,
                    fixture.AwayTeam?.Name,
                    fixture.League?.Name,
                    fixture.Venue?.Name
                );
            }

            // 3. Odds for each fixture
            var odds = await db.Odds
                .Include(o => o.Fixture)
                .Include(o => o.Market)
                .Include(o => o.Outcome)
                .Include(o => o.Provider)
                .ToListAsync(stoppingToken);

            foreach (var odd in odds)
            {
                _logger.LogInformation("Odds {OddsId}: Fixture {FixtureId}, Market: {Market}, Outcome: {Outcome}, Provider: {Provider}",
                    odd.OddsId,
                    odd.FixtureId,
                    odd.Market?.Name,
                    odd.Outcome?.Name,
                    odd.Provider?.Name
                );
            }

            // 4. Teams and Players
            var teams = await db.Teams
                .Include(t => t.Country)
                .Include(t => t.Players)
                .ToListAsync(stoppingToken);

            foreach (var team in teams)
            {
                _logger.LogInformation("Team {TeamId}: {TeamName} ({Country}), Players: {PlayerCount}",
                    team.TeamId,
                    team.Name,
                    team.Country?.Name,
                    team.Players?.Count ?? 0
                );
            }

            // 5. Leagues and Seasons
            var leagues = await db.Leagues
                .Include(l => l.Country)
                .Include(l => l.Seasons)
                .ToListAsync(stoppingToken);

            foreach (var league in leagues)
            {
                _logger.LogInformation("League {LeagueId}: {LeagueName} ({Country}), Seasons: {SeasonCount}",
                    league.LeagueId,
                    league.Name,
                    league.Country?.Name,
                    league.Seasons?.Count ?? 0
                );
            }

            // 6. Standings
            var standings = await db.Standings
                .Include(s => s.League)
                .Include(s => s.Season)
                .Include(s => s.Team)
                .ToListAsync(stoppingToken);

            foreach (var standing in standings)
            {
                _logger.LogInformation("Standing: League {League}, Season {Season}, Team {Team}",
                    standing.League?.Name,
                    standing.Season?.Year,
                    standing.Team?.Name
                );
            }

            // 7. Statisticals
            var statisticals = await db.Statisticals
                .Include(s => s.Fixture)
                .Include(s => s.Player)
                .Include(s => s.Team)
                .ToListAsync(stoppingToken);

            foreach (var stat in statisticals)
            {
                _logger.LogInformation("Statistical: Fixture {Fixture}, Player {Player}, Team {Team}",
                    stat.Fixture?.FixtureId,
                    stat.Player?.FirstName + ' ' + stat.Player?.LastName,
                    stat.Team?.Name
                );
            }

            // 8. Lineups
            var lineups = await db.Lineups
                .Include(l => l.Fixture)
                .Include(l => l.Player)
                .Include(l => l.Team)
                .Include(l => l.Position)
                .ToListAsync(stoppingToken);

            foreach (var lineup in lineups)
            {
                _logger.LogInformation("Lineup: Fixture {Fixture}, Player {Player}, Team {Team}, Position {Position}",
                    lineup.Fixture?.FixtureId,
                    lineup.Player?.FirstName + ' ' + lineup.Player?.LastName,
                    lineup.Team?.Name,
                    lineup.Position?.Code
                );
            }

            // 9. Coaches
            var coaches = await db.Coaches
                .Include(c => c.Team)
                .ToListAsync(stoppingToken);

            foreach (var coach in coaches)
            {
                _logger.LogInformation("Coach: {CoachName}, Team: {Team}", coach.Name, coach.Team?.Name);
            }

            // 10. Countries, Venues, EventTypes, FixtureStatuses, etc.
            var countries = await db.Countries.ToListAsync(stoppingToken);
            var venues = await db.Venues.ToListAsync(stoppingToken);
            var eventTypes = await db.EventTypes.ToListAsync(stoppingToken);
            var fixtureStatuses = await db.FixtureStatuses.ToListAsync(stoppingToken);

            _logger.LogInformation("Countries: {Count}, Venues: {Count}, EventTypes: {Count}, FixtureStatuses: {Count}",
                countries.Count, venues.Count, eventTypes.Count, fixtureStatuses.Count);

            // 11. ApiIngest, ProviderWebhookEvents, Payments, Subscriptions, Users, etc.
            var apiIngests = await db.ApiIngest.ToListAsync(stoppingToken);
            var providerWebhookEvents = await db.ProviderWebhookEvents.ToListAsync(stoppingToken);
            var payments = await db.Payments.ToListAsync(stoppingToken);
            var subscriptions = await db.Subscriptions.ToListAsync(stoppingToken);
            var users = await db.Users.ToListAsync(stoppingToken);

            _logger.LogInformation("ApiIngest: {Count}, ProviderWebhookEvents: {Count}, Payments: {Count}, Subscriptions: {Count}, Users: {Count}",
                apiIngests.Count, providerWebhookEvents.Count, payments.Count, subscriptions.Count, users.Count);

            _logger.LogInformation("Worker finished at: {time}", DateTimeOffset.Now);
        }
    }
}
