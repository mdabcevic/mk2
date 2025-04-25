using Bartender.Data;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bartender.Domain.Repositories;

public class TableRepository(AppDbContext context) : Repository<Table>(context), ITableRepository
{

    public async Task<List<Table>> GetAllByPlaceAsync(int placeId)
    {
        return await Query()
        .Where(t => t.PlaceId == placeId)
        .ToListAsync();
    }
    public async Task<bool> ExistsByLabelAsync(int placeId, string label)
    {
        return await Query()
            .AnyAsync(t => t.PlaceId == placeId &&
                           t.Label.Equals(label, StringComparison.CurrentCultureIgnoreCase));
    }

    public async Task<Table?> GetByPlaceLabelAsync(int placeId, string label)
    {
        return await Query()
            .FirstOrDefaultAsync(t => t.PlaceId == placeId &&
                                      t.Label.ToLower() == label.ToLower());
    }

    public async Task<Dictionary<string, Table>> GetByPlaceAsLabelDictionaryAsync(int placeId)
    {
        return await Query()
            .Where(t => t.PlaceId == placeId)
            .ToDictionaryAsync(t => t.Label, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<List<Table>> GetActiveByPlaceAsync(int placeId)
    {
        return await Query()
            .Where(t => t.PlaceId == placeId && !t.IsDisabled)
            .ToListAsync();
    }

    public async Task<Table?> GetBySaltAsync(string salt)
    {
        return await Query()
            .FirstOrDefaultAsync(t => t.QrSalt == salt);
    }
}
