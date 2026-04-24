using AutoMapper;
using Moq;
using SIGO.Data.Interfaces;
using SIGO.Objects.Dtos.Entities;
using SIGO.Services.Entities;
using SIGO.Validation;
using Xunit;

namespace SIGO.Tests.Services
{
    public class ClienteServiceTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock = new();
        private readonly Mock<ITelefoneRepository> _telefoneRepositoryMock = new();
        private readonly Mock<IMapper> _mapperMock = new();

        [Fact]
        public async Task Create_DeveRetornarTodosErros_QuandoCpfCnpjECepForemInvalidos()
        {
            var service = CreateService();
            var dto = new ClienteDTO
            {
                Nome = "Cliente",
                Email = "cliente@test.com",
                Cpf_Cnpj = "111",
                Cep = "123"
            };

            var exception = await Assert.ThrowsAsync<BusinessValidationException>(() => service.Create(dto));

            Assert.Contains(exception.Errors, error =>
                error.Field == nameof(ClienteDTO.Cpf_Cnpj) &&
                error.Message == "CPF/CNPJ inválido.");
            Assert.Contains(exception.Errors, error =>
                error.Field == nameof(ClienteDTO.Cep) &&
                error.Message == "CEP inválido. O CEP deve conter 8 dígitos.");
            Assert.Equal(2, exception.Errors.Count);
            _clienteRepositoryMock.Verify(r => r.Add(It.IsAny<SIGO.Objects.Models.Cliente>()), Times.Never);
        }

        private ClienteService CreateService()
        {
            var cpfValidator = new CpfValidator();
            var cnpjValidator = new CnpjValidator();
            var cpfCnpjValidator = new CpfCnpjValidator(cpfValidator, cnpjValidator);

            return new ClienteService(
                _clienteRepositoryMock.Object,
                _telefoneRepositoryMock.Object,
                _mapperMock.Object,
                cpfCnpjValidator);
        }
    }
}
