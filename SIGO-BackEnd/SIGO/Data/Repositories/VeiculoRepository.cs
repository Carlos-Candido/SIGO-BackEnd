using Microsoft.EntityFrameworkCore;
using SIGO.Data.Interfaces;
using SIGO.Objects.Models;

namespace SIGO.Data.Repositories
{
    public class VeiculoRepository : GenericRepository<Veiculo>, IVeiculoRepository
    {
        private readonly AppDbContext _context;

        public VeiculoRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<IEnumerable<Veiculo>> Get()
        {
            return await _context.Veiculos
                .Include(v => v.Cliente)
                .ToListAsync();
        }

        public async Task<IEnumerable<Veiculo>> GetByPlaca(string placa)
        {
            return await _context.Veiculos
                .Include(v => v.Cliente)
                .Where(v => v.PlacaVeiculo.Contains(placa))
                .ToListAsync();
        }

        public async Task<IEnumerable<Veiculo>> GetByTipo(string tipo)
        {
            return await _context.Veiculos
                .Include(v => v.Cliente)
                .Where(v => v.TipoVeiculo.Contains(tipo))
                .ToListAsync();
        }

        public async Task<Veiculo?> GetById(int id)
        {
            return await _context.Veiculos
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == id);
        }
        public async Task UpdateVeiculo(Veiculo veiculo)
        {
            // Atualiza só os campos necessários
            var existing = await _context.Veiculos.FindAsync(veiculo.Id);

            if (existing == null)
                throw new KeyNotFoundException($"Veículo com id {veiculo.Id} não encontrado.");

            // Atualiza os campos desejados
            existing.PlacaVeiculo = veiculo.PlacaVeiculo;
            existing.AnoFab = veiculo.AnoFab;
            existing.Id = veiculo.Id; // mantém relacionamento correto

            _context.Veiculos.Update(existing);
            await _context.SaveChangesAsync();
        }
    }
}
