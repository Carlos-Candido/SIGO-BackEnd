using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGO.Objects.Contracts;
using SIGO.Objects.Dtos.Entities;
using SIGO.Security;
using SIGO.Services.Interfaces;
using SIGO.Utils;
using SIGO.Validation;

namespace SIGO.Controllers
{
    [Route("api/oficinas")]
    [ApiController]
    [Authorize(Policy = AuthorizationPolicies.FullAccess)]
    public class OficinaController : ControllerBase
    {
        private readonly IOficinaService _oficinaService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly Response _response;

        public OficinaController(
            IOficinaService oficinaService,
            IMapper mapper,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _oficinaService = oficinaService;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _response = new Response();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var cores = await _oficinaService.GetAll();

            _response.Code = ResponseEnum.SUCCESS;
            _response.Data = cores;
            _response.Message = "Cores listadas com sucesso";

            return Ok(_response);
        }

        [HttpGet("nome/{nome}")]
        public async Task<IActionResult> GetByName(string nome)
        {
            var cores = await _oficinaService.GetByName(nome);

            if (!cores.Any())
            {
                _response.Code = ResponseEnum.NOT_FOUND;
                _response.Data = null;
                _response.Message = "Nenhuma cor encontrada";
                return NotFound(_response);
            }

            _response.Code = ResponseEnum.SUCCESS;
            _response.Data = cores;
            _response.Message = "Cores encontradas com sucesso";
            return Ok(_response);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create(OficinaDTO oficinaDto)
        {
            try
            {
                SanitizeOficina(oficinaDto);

                // hash da senha antes de salvar
                oficinaDto.Senha = _passwordHasher.Hash(oficinaDto.Senha);

                await _oficinaService.Create(oficinaDto);
                return Ok(new { Message = "Oficina cadastrada com sucesso" });
            }
            catch (BusinessValidationException ex)
            {
                return BadRequest(new { Message = "Dados inválidos", Errors = ex.Errors });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] OficinaDTO oficinaDto)
        {
            if (oficinaDto == null)
            {
                _response.Code = ResponseEnum.INVALID;
                _response.Data = null;
                _response.Message = "Dados inválidos";

                return BadRequest(_response);
            }
            // força o id da URL no DTO (evita mismatch)
            oficinaDto.Id = id;

                try
            {
                SanitizeOficina(oficinaDto);
                await _oficinaService.Update(oficinaDto, id);
                return Ok(new { Message = "Oficina atualizada com sucesso" });
            }
            catch (BusinessValidationException ex)
            {
                return BadRequest(new { Message = "Dados inválidos", Errors = ex.Errors });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = "Oficina não encontrada" });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _oficinaService.Remove(id);
                return Ok(new { Message = "Oficina removida com sucesso" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = "Oficina não encontrada" });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] Login login)
        {
            if (login is null)
            {
                _response.Code = ResponseEnum.INVALID;
                _response.Data = null;
                _response.Message = "Dados inválidos";

                return BadRequest(_response);
            }

            try
            {
                login.Password = _passwordHasher.Hash(login.Password);
                var oficinaDTO = await _oficinaService.Login(login);

                if (oficinaDTO is null)
                {
                    _response.Code = ResponseEnum.INVALID;
                    _response.Data = null;
                    _response.Message = "Email ou senha incorretos";

                    return BadRequest(_response);
                }

                var token = _jwtTokenService.GenerateToken(new JwtTokenRequest
                {
                    UserId = oficinaDTO.Id,
                    Name = oficinaDTO.Nome,
                    Email = oficinaDTO.Email,
                    Role = SystemRoles.Oficina
                });
                _response.Code = ResponseEnum.SUCCESS;
                _response.Data = token;
                _response.Message = "Login realizado com sucesso";

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Code = ResponseEnum.ERROR;
                _response.Message = "Não foi possível realizar o login";
                _response.Data = new
                {
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace ?? "No stack trace available"
                };

                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }

        private static void SanitizeOficina(OficinaDTO oficinaDTO)
        {
            oficinaDTO.CNPJ = SanitizeHelper.ApenasDigitos(oficinaDTO.CNPJ);
        }
    }
}
