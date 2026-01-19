namespace NppCore.Services.Features.Player;

public interface IPlayerStatsService
{
    /// <summary>
    /// Ažurira statistiku nakon meča
    /// </summary>
    Task UpdateStatsAfterMatchAsync(
        Guid winnerId,
        string winnerUsername,
        int winnerScore,
        Guid loserId,
        string loserUsername,
        int loserScore
    );
}
