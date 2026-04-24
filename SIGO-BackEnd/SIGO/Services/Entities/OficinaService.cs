using AutoMapper;
using SIGO.Data.Interfaces;
using SIGO.Objects.Dtos.Entities;
using SIGO.Objects.Models;
using SIGO.Services.Interfaces;
using System.Linq;
using SIGO.Objects.Contracts;
using SIGO.Validation;

namespace SIGO.Services.Entities
{
    public class OficinaService : GenericService<Oficina, OficinaDTO>, IOficinaService
    {
        private readonly IOficinaRepository _oficinaRepository;
        private readonly IMapper _mapper;
        private readonly ICnpjValidator _cnpjValidator;

        public OficinaService(
            IOficinaRepository oficinaRepository,
            IMapper mapper,
            ICnpjValidator cnpjValidator)
            : base(oficinaRepository, mapper)
        {
            _oficinaRepository = oficinaRepository;
            _mapper = mapper;
            _cnpjValidator = cnpjValidator;
        }

        public async Task<OficinaDTO?> Login(Login login)
        {
            var oficina = await _oficinaRepository.Login(login);

            if (oficina is not null) oficina.Senha = "";
            return _mapper.Map<OficinaDTO?>(oficina);
        }

        public async Task<IEnumerable<OficinaDTO>> GetByName(string nomeOficina)
        {
            var oficinas = await _oficinaRepository.GetByName(nomeOficina);
            return _mapper.Map<IEnumerable<OficinaDTO>>(oficinas);
        }

        public new async Task Create(OficinaDTO oficinaDTO)
        {
            await ValidateOficina(oficinaDTO);
            oficinaDTO.CNPJ = _cnpjValidator.Normalize(oficinaDTO.CNPJ!);
            await base.Create(oficinaDTO);
        }

        public override async Task Update(OficinaDTO oficinaDTO, int id)
        {
            await ValidateOficina(oficinaDTO, id);
            oficinaDTO.CNPJ = _cnpjValidator.Normalize(oficinaDTO.CNPJ!);
            await base.Update(oficinaDTO, id);
        }

        public async Task ValidarCnpj(string? cnpj, int? ignoreId = null)
        {
            var errors = new List<ValidationError>();
            await AddCnpjErrors(cnpj, errors, ignoreId);
            ThrowIfInvalid(errors);
        }

        private async Task ValidateOficina(OficinaDTO oficinaDTO, int? ignoreId = null)
        {
            var errors = new List<ValidationError>();
            await AddCnpjErrors(oficinaDTO.CNPJ, errors, ignoreId);
            ThrowIfInvalid(errors);
        }

        private async Task AddCnpjErrors(string? cnpj, ICollection<ValidationError> errors, int? ignoreId = null)
        {
            if (!_cnpjValidator.IsValid(cnpj))
            {
                errors.Add(new ValidationError(nameof(OficinaDTO.CNPJ), "CNPJ inválido."));
                return;
            }

            var cnpjNormalizado = _cnpjValidator.Normalize(cnpj!);
            var existe = await _oficinaRepository.ExistsByCnpj(cnpjNormalizado, ignoreId);
            if (existe)
                errors.Add(new ValidationError(nameof(OficinaDTO.CNPJ), "CNPJ já cadastrado."));
        }

        private static void ThrowIfInvalid(IReadOnlyCollection<ValidationError> errors)
        {
            if (errors.Count > 0)
                throw new BusinessValidationException(errors);
        }

    }
}
