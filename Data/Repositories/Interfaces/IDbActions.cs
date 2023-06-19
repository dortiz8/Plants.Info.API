namespace Plants.info.API.Data.Repository
{
    public interface IDbActions
    {
        Task<bool> SaveAllChangesAsync();
    }
}
