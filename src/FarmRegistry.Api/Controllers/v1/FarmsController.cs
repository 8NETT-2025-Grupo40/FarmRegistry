using FarmRegistry.Application.Contracts.Common;
using FarmRegistry.Application.Contracts.Farms;
using FarmRegistry.Application.Services;
using FarmRegistry.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace FarmRegistry.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/farms")]
[Produces("application/json")]
public class FarmsController : ControllerBase
{
    private readonly IFarmService _farmService;
    private readonly IUserContext _userContext;

    public FarmsController(IFarmService farmService, IUserContext userContext)
    {
        _farmService = farmService;
        _userContext = userContext;
    }

    /// <summary>
    /// Cria uma nova fazenda
    /// </summary>
    /// <param name="request">Dados da fazenda</param>
    /// <returns>Fazenda criada</returns>
    [HttpPost]
    [ProducesResponseType(typeof(FarmResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFarm([FromBody] CreateFarmRequest request)
    {
        try
        {
            var response = await _farmService.CreateFarmAsync(request);
            return CreatedAtAction(nameof(GetFarmById), new { id = response.Id }, response);
        }
        catch (DomainException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Erro de domínio",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Obtém todas as fazendas do usuário
    /// </summary>
    /// <returns>Lista de fazendas</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FarmResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFarms()
    {
        var response = await _farmService.GetFarmsAsync(_userContext.OwnerId);
        return Ok(response);
    }

    /// <summary>
    /// Obtém uma fazenda por ID
    /// </summary>
    /// <param name="id">ID da fazenda</param>
    /// <returns>Fazenda encontrada</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FarmResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFarmById(Guid id)
    {
        var response = await _farmService.GetFarmByIdAsync(id);
        if (response == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Fazenda não encontrada",
                Detail = $"Fazenda com ID {id} não foi encontrada.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(response);
    }

    /// <summary>
    /// Atualiza uma fazenda
    /// </summary>
    /// <param name="id">ID da fazenda</param>
    /// <param name="request">Dados atualizados da fazenda</param>
    /// <returns>Fazenda atualizada</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FarmResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFarm(Guid id, [FromBody] UpdateFarmRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "ID inconsistente",
                Detail = "O ID da URL não confere com o ID do corpo da requisição.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var response = await _farmService.UpdateFarmAsync(request);
            return Ok(response);
        }
        catch (DomainException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Erro de domínio",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Ativa uma fazenda
    /// </summary>
    /// <param name="id">ID da fazenda</param>
    /// <returns>Fazenda ativada</returns>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(FarmResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateFarm(Guid id)
    {
        try
        {
            var response = await _farmService.ActivateFarmAsync(id);
            return Ok(response);
        }
        catch (DomainException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Fazenda não encontrada",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    /// <summary>
    /// Desativa uma fazenda
    /// </summary>
    /// <param name="id">ID da fazenda</param>
    /// <returns>Fazenda desativada</returns>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(FarmResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateFarm(Guid id)
    {
        try
        {
            var response = await _farmService.DeactivateFarmAsync(id);
            return Ok(response);
        }
        catch (DomainException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Fazenda não encontrada",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    /// <summary>
    /// Remove uma fazenda (deleção lógica)
    /// </summary>
    /// <param name="id">ID da fazenda</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFarm(Guid id)
    {
        try
        {
            await _farmService.DeleteFarmAsync(id);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Fazenda não encontrada",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }
}