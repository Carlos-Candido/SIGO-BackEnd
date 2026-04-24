using AutoMapper;
using SIGO.Data.Interfaces;
using SIGO.Data.Repositories;
using SIGO.Objects.Dtos.Entities;
using SIGO.Objects.Models;
using SIGO.Services.Interfaces;
using System.Linq;
using SIGO.Objects.Contracts;
using SIGO.Validation;

namespace SIGO.Services.Entities
{
    public class FuncionarioService : GenericService<Funcionario, FuncionarioDTO>, IFuncionarioService
    {
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IMapper _mapper;
        private readonly ICpfValidator _cpfValidator;

        public FuncionarioService(
            IFuncionarioRepository funcionarioRepository,
            IMapper mapper,
            ICpfValidator cpfValidator)
            : base(funcionarioRepository, mapper)
        {
            _funcionarioRepository = funcionarioRepository;
            _mapper = mapper;
            _cpfValidator = cpfValidator;
        }

        public async Task<FuncionarioDTO?> Login(Login login)
        {
            var funcionario = await _funcionarioRepository.Login(login);

            if (funcionario is not null) funcionario.Senha = "";
            return _mapper.Map<FuncionarioDTO?>(funcionario);
        }

        public async Task<IEnumerable<FuncionarioDTO>> GetFuncionarioByNome(string nome)
        {
            var entities = await _funcionarioRepository.GetFuncionarioByNome(nome);
            return _mapper.Map<IEnumerable<FuncionarioDTO>>(entities);
        }

        public new async Task Create(FuncionarioDTO funcionarioDTO)
        {
            await ValidateFuncionario(funcionarioDTO);
            funcionarioDTO.Cpf = _cpfValidator.Normalize(funcionarioDTO.Cpf);
            await base.Create(funcionarioDTO);
        }

        public override async Task Update(FuncionarioDTO funcionarioDTO, int id)
        {
            await ValidateFuncionario(funcionarioDTO, id);
            funcionarioDTO.Cpf = _cpfValidator.Normalize(funcionarioDTO.Cpf);
            await base.Update(funcionarioDTO, id);
        }

        public async Task ValidarCpf(string? cpf, int? ignoreId = null)
        {
            var errors = new List<ValidationError>();
            await AddCpfErrors(cpf, errors, ignoreId);
            ThrowIfInvalid(errors);
        }

        private async Task ValidateFuncionario(FuncionarioDTO funcionarioDTO, int? ignoreId = null)
        {
            var errors = new List<ValidationError>();
            await AddCpfErrors(funcionarioDTO.Cpf, errors, ignoreId);
            ThrowIfInvalid(errors);
        }

        private async Task AddCpfErrors(string? cpf, ICollection<ValidationError> errors, int? ignoreId = null)
        {
            if (!_cpfValidator.IsValid(cpf))
            {
                errors.Add(new ValidationError(nameof(FuncionarioDTO.Cpf), "CPF inválido."));
                return;
            }

            var cpfNormalizado = _cpfValidator.Normalize(cpf!);
            var existe = await _funcionarioRepository.ExistsByCpf(cpfNormalizado, ignoreId);

            if (existe)
                errors.Add(new ValidationError(nameof(FuncionarioDTO.Cpf), "CPF já cadastrado."));
        }

        private static void ThrowIfInvalid(IReadOnlyCollection<ValidationError> errors)
        {
            if (errors.Count > 0)
                throw new BusinessValidationException(errors);
        }

    }
}
