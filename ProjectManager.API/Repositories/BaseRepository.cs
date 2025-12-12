using Microsoft.EntityFrameworkCore;
using ProjectManager.API.Data;

namespace ProjectManager.API.Repositories;

public abstract class BaseRepository
{
    protected readonly ApplicationDbContext _context;

    protected BaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    protected async Task<bool> SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}