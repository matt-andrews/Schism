using Microsoft.EntityFrameworkCore;

namespace Schism.Hub.Data;

public interface IRepository
{
    Task<RegistrationTable?> GetByClientId(string clientid);
    Task<RegistrationTable[]> GetTablesByClient();
    Task AddAsync(RegistrationTable obj);
    Task<int> SaveChangesAsync();
}
internal class Repository(Context _context) : IRepository
{
    public async Task<RegistrationTable?> GetByClientId(string clientid)
    {
        return await _context.Registrations.FindAsync(clientid);
    }
    public async Task<RegistrationTable[]> GetTablesByClient()
    {
        return await _context.Registrations.AsNoTracking().ToArrayAsync();
    }
    public async Task AddAsync(RegistrationTable obj)
    {
        await _context.AddAsync(obj);
    }
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}