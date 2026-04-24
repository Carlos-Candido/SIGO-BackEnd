using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGO.Objects.Contracts;
using SIGO.Objects.Dtos.Entities;
using SIGO.Security;
using SIGO.Services.Interfaces;

namespace SIGO.Controllers
{
    [Route("api/veiculos")]
    [ApiController]
    [Authorize(Policy = AuthorizationPolicies.SelfServiceAccess)]
    public class VeiculoController : ControllerBase
    {
        private readonly IVeiculoService _veiculoService;
        private readonly ICurrentUserService _currentUserService;
        private readonly Response _response;

        public VeiculoController(
            IVeiculoService veiculoService,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _veiculoService = veiculoService;
            _currentUserService = currentUserService;
            _response = new Response();
        }

        [HttpGet]
        [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Oficina},{SystemRoles.Funcionario},{SystemRoles.Cliente}")]
        public async Task<IActionResult> Get()
        {
            var veiculos = await _veiculoService.GetAll();
            if (_currentUserService.IsInRole(SystemRoles.Cliente))
            {
                var clienteId = _currentUserService.UserId;
                veiculos = veiculos.Where(v => clienteId.HasValue && v.ClienteId == clienteId.Value);
            }

            _response.Code = ResponseEnum.SUCCESS;
            _response.Data = veiculos;
            _response.Message = "Veículos listados com sucesso";

            return Ok(_response);
        }

        [HttpGet("placa/{placa}")]
        [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Oficina},{SystemRoles.Funcionario},{SystemRoles.Cliente}")]
        public async Task<IActionResult> GetByPlaca(string placa)
        {
            var veiculos = await _veiculoService.GetByPlaca(placa);
            if (_currentUserService.IsInRole(SystemRoles.Cliente))
            {
                var clienteId = _currentUserService.UserId;
                veiculos = veiculos.Where(v => v is not null && clienteId.HasValue && v.ClienteId == clienteId.Value);
            }

            if (!veiculos.Any())
            {
                _response.Code = ResponseEnum.NOT_FOUND;
                _response.Data = null;
                _response.Message = "Nenhum veículo encontrado com essa placa";
                return NotFound(_response);
            }

            _response.Code = ResponseEnum.SUCCESS;
            _response.Data = veiculos;
            _response.Message = "Veículos encontrados com sucesso";
            return Ok(_response);
        }

        [HttpGet("tipo/{tipo}")]
        [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Oficina},{SystemRoles.Funcionario},{SystemRoles.Cliente}")]
        public async Task<IActionResult> GetByTipo(string tipo)
        {
            var veiculos = await _veiculoService.GetByTipo(tipo);
            if (_currentUserService.IsInRole(SystemRoles.Cliente))
            {
                var clienteId = _currentUserService.UserId;
                veiculos = veiculos.Where(v => clienteId.HasValue && v.ClienteId == clienteId.Value);
            }

            if (!veiculos.Any())
            {
                _response.Code = ResponseEnum.NOT_FOUND;
                _response.Data = null;
                _response.Message = "Nenhum veículo encontrado com esse tipo";
                return NotFound(_response);
            }

            _response.Code = ResponseEnum.SUCCESS;
            _response.Data = veiculos;
            _response.Message = "Veículos encontrados com sucesso";
            return Ok(_response);
        }

        [HttpPost]
        [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Oficina}")]
        public async Task<IActionResult> Create(VeiculoDTO veiculoDto)
        {
            await _veiculoService.Create(veiculoDto);
            return Ok(new { Message = "Veículo cadastrado com sucesso" });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Oficina},{SystemRoles.Funcionario}")]
        public async Task<IActionResult> Update(int id, VeiculoDTO veiculoDto)
        {
            try
            {
                await _veiculoService.UpdateVeiculo(veiculoDto, id);
                return Ok(new { Message = "Veículo atualizado com sucesso" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Oficina},{SystemRoles.Funcionario}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _veiculoService.Remove(id);
            return Ok(new { Message = "Veículo removido com sucesso" });
        }

    }
}
