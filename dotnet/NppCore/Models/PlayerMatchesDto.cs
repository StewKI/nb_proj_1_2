namespace NppCore.Models;

public record PlayerMatchesRequest(
    Guid OpponentId, 
    string OpponentUsername, 
    string Score, 
    string Result, 
    string Year, 
    DateTimeOffset Match_time
);

public record PlayerMatchesResponse
(
    Guid PlayerId,
    string OpponentUsername,
    string Result,
    string Score,
    DateTimeOffset Match_time
);