using AutoMapper;
using SIGO.Data.Interfaces;
using SIGO.Objects.Contracts;
using SIGO.Objects.Dtos.Entities;
using SIGO.Objects.Models;
using SIGO.Services.Interfaces;
using SIGO.Validation;
using System.Linq;

namespace SIGO.Services.Entities
{
    public class ClienteService : GenericService<Cliente, ClienteDTO>, IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;
        private readonly ICpfCnpjValidator _cpfCnpjValidator;

        private readonly ITelefoneRepository _telefoneRepository;

        public ClienteService(
            IClienteRepository clienteRepository,
            ITelefoneRepository telefoneRepository,
            IMapper mapper,
            ICpfCnpjValidator cpfCnpjValidator)
            : base(clienteRepository, mapper)
        {
            _clienteRepository = clienteRepository;
            _telefoneRepository = telefoneRepository;
            _mapper = mapper;
            _cpfCnpjValidator = cpfCnpjValidator;
        }
        public override async Task<IEnumerable<ClienteDTO>> GetAll()
        {
            var entities = await _clienteRepository.Get();
            return _mapper.Map<IEnumerable<ClienteDTO>>(entities);
        }

        public async Task<ClienteDTO?> GetByIdWithDetails(int id)
        {
            var entity = await _clienteRepository.GetByIdWithDetails(id);
            return _mapper.Map<ClienteDTO?>(entity);
        }

        public async Task<IEnumerable<ClienteDTO>> GetByNameWithDetails(string nome)
        {
            var entities = await _clienteRepository.GetByNameWithDetails(nome);
            return _mapper.Map<IEnumerable<ClienteDTO>>(entities);
        }

        public async Task<ClienteDTO?> GetById(int id)
        {
            var entity = await _clienteRepository.GetById(id);
            return _mapper.Map<ClienteDTO?>(entity);
        }

        public new async Task Create(ClienteDTO clienteDTO)
        {
            await ValidateCliente(clienteDTO);
            clienteDTO.Cpf_Cnpj = _cpfCnpjValidator.Normalize(clienteDTO.Cpf_Cnpj!);

            var cliente = _mapper.Map<Cliente>(clienteDTO);
            await _clienteRepository.Add(cliente);
        }

        public override async Task Update(ClienteDTO clienteDTO, int id)
        {
            var existingCliente = await _clienteRepository.GetById(id);
            if (existingCliente == null)
            {
                throw new KeyNotFoundException($"Cliente com id {id} não encontrado.");
            }

            await ValidateCliente(clienteDTO, id);
            clienteDTO.Cpf_Cnpj = _cpfCnpjValidator.Normalize(clienteDTO.Cpf_Cnpj!);

            clienteDTO.Id = id;

            // Atualiza dados do cliente
            var clienteEntity = _mapper.Map<Cliente>(clienteDTO);
            await _clienteRepository.Update(clienteEntity);

            // Sincroniza telefones (atualiza existentes e adiciona novos)
            if (clienteDTO.Telefones != null)
            {
                foreach (var telefoneDto in clienteDTO.Telefones)
                {
                    telefoneDto.ClienteId = id;
                    var telefoneEntity = _mapper.Map<Telefone>(telefoneDto);

                    if (telefoneEntity.Id > 0)
                    {
                        await _telefoneRepository.Update(telefoneEntity);
                    }
                    else
                    {
                        await _telefoneRepository.Add(telefoneEntity);
                    }
                }
            }

        }
        public async Task<ClienteDTO> Login(Login login)
        {
            var professor = await _clienteRepository.Login(login);

            if (professor is not null) professor.Senha = ""; // Oculta a senha
            return _mapper.Map<ClienteDTO>(professor);
        }

        public async Task ValidarCpfCnpj(string? documento, int? ignoreId = null)
        {
            var errors = new List<ValidationError>();

            if (!_cpfCnpjValidator.IsValid(documento))
            {
                errors.Add(new ValidationError(nameof(ClienteDTO.Cpf_Cnpj), "CPF/CNPJ inválido."));
                throw new BusinessValidationException(errors);
            }

            var documentoNormalizado = _cpfCnpjValidator.Normalize(documento!);
            var existe = await _clienteRepository.ExistsByCpfCnpj(documentoNormalizado, ignoreId);

            if (existe)
                errors.Add(new ValidationError(nameof(ClienteDTO.Cpf_Cnpj), "CPF/CNPJ já cadastrado."));

            ThrowIfInvalid(errors);
        }

        public async Task ValidarNomeEmail(string? nome, string? email, int? ignoreId = null)
        {
            var errors = new List<ValidationError>();

            if (!string.IsNullOrWhiteSpace(nome))
            {
                var nomeJaExiste = await _clienteRepository.ExistsByNome(nome, ignoreId);
                if (nomeJaExiste)
                    errors.Add(new ValidationError(nameof(ClienteDTO.Nome), "Já existe cliente cadastrado com este nome."));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var emailJaExiste = await _clienteRepository.ExistsByEmail(email, ignoreId);
                if (emailJaExiste)
                    errors.Add(new ValidationError(nameof(ClienteDTO.Email), "Já existe cliente cadastrado com este e-mail."));
            }

            ThrowIfInvalid(errors);
        }

        private async Task ValidateCliente(ClienteDTO clienteDTO, int? ignoreId = null)
        {
            var errors = new List<ValidationError>();

            await AddCpfCnpjErrors(clienteDTO.Cpf_Cnpj, errors, ignoreId);
            await AddNomeEmailErrors(clienteDTO.Nome, clienteDTO.Email, errors, ignoreId);
            AddCepErrors(clienteDTO.Cep, errors);

            ThrowIfInvalid(errors);
        }

        private async Task AddCpfCnpjErrors(string? documento, ICollection<ValidationError> errors, int? ignoreId = null)
        {
            if (!_cpfCnpjValidator.IsValid(documento))
            {
                errors.Add(new ValidationError(nameof(ClienteDTO.Cpf_Cnpj), "CPF/CNPJ inválido."));
                return;
            }

            var documentoNormalizado = _cpfCnpjValidator.Normalize(documento!);
            var existe = await _clienteRepository.ExistsByCpfCnpj(documentoNormalizado, ignoreId);

            if (existe)
                errors.Add(new ValidationError(nameof(ClienteDTO.Cpf_Cnpj), "CPF/CNPJ já cadastrado."));
        }

        private async Task AddNomeEmailErrors(string? nome, string? email, ICollection<ValidationError> errors, int? ignoreId = null)
        {
            if (!string.IsNullOrWhiteSpace(nome))
            {
                var nomeJaExiste = await _clienteRepository.ExistsByNome(nome, ignoreId);
                if (nomeJaExiste)
                    errors.Add(new ValidationError(nameof(ClienteDTO.Nome), "Já existe cliente cadastrado com este nome."));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var emailJaExiste = await _clienteRepository.ExistsByEmail(email, ignoreId);
                if (emailJaExiste)
                    errors.Add(new ValidationError(nameof(ClienteDTO.Email), "Já existe cliente cadastrado com este e-mail."));
            }
        }

        private static void AddCepErrors(string? cep, ICollection<ValidationError> errors)
        {
            var normalizedCep = new string((cep ?? string.Empty).Where(char.IsDigit).ToArray());
            if (normalizedCep.Length != 8)
                errors.Add(new ValidationError(nameof(ClienteDTO.Cep), "CEP inválido. O CEP deve conter 8 dígitos."));
        }

        private static void ThrowIfInvalid(IReadOnlyCollection<ValidationError> errors)
        {
            if (errors.Count > 0)
                throw new BusinessValidationException(errors);
        }

    }
}
