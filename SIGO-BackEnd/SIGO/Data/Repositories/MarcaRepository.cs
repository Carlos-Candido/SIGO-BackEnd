using Microsoft.EntityFrameworkCore;
using SIGO.Data.Interfaces;
using SIGO.Objects.Models;

namespace SIGO.Data.Repositories
{
    public class MarcaRepository : GenericRepository<Marca>, IMarcaRepository
    {
        private readonly AppDbContext _context;

        public MarcaRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<IEnumerable<Marca>> Get()
        {
            return await _context.Marcas.ToListAsync();
        }

        public async Task<IEnumerable<Marca>> GetByName(string nomeMarca)
        {
            return await _context.Marcas
                .Where(m => m.Nome.Contains(nomeMarca))
                .ToListAsync();
        }

        public async Task<Marca?> GetById(int idMarca)
        {
            return await _context.Marcas
                .FirstOrDefaultAsync(m => m.Id == idMarca);
        }
    }
}