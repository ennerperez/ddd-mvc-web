namespace Domain.Enums
{
    public enum Status : short
    {
        Draft = -10,
        Unknown = 0,
        Created = 10,
        Approved = 20,
        Rejected = 30,
        Paused = 40,
        Cancelled = 50,
        Deleted = 60
    }
}
