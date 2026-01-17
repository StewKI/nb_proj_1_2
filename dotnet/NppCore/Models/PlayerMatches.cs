public class PlayerMatches
{
    public Guid PlayerId { get; set; }
    public string Year { get; set; }=string.Empty;
    public DateTimeOffset Match_time { get; set; }
    public Guid OpponentId { get; set; }
    public string OpponentUsername { get; set; }=string.Empty ;
    public string Score { get; set; }=string.Empty;
    public string Result { get; set; }=string.Empty;

}