using FarmRegistry.Application.Contracts.Fields;
using FarmRegistry.Application.Services;
using FarmRegistry.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace FarmRegistry.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/fields")]
[Produces("application/json")]
public class FieldsController : ControllerBase
{
    private readonly IFieldService _fieldService;

    public FieldsController(IFieldService fieldService)
    {
        _fieldService = fieldService;
    }

    /// <summary>
    /// Cria um novo talhão
    /// </summary>
    /// <param name="request">Dados do talhão</param>
    /// <returns>Talhão criado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(FieldResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateField([FromBody] CreateFieldRequest request)
    {
        try
        {
            var response = await _fieldService.CreateFieldAsync(request);
            return CreatedAtAction(nameof(GetFieldById), new { id = response.Id }, response);
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
    /// Obtém talhões filtrados por fazenda
    /// </summary>
    /// <param name="farmId">ID da fazenda (obrigatório)</param>
    /// <returns>Lista de talhões</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FieldResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFields([FromQuery] Guid? farmId = null)
    {
        if (!farmId.HasValue)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Parâmetro obrigatório",
                Detail = "O parâmetro 'farmId' é obrigatório.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var response = await _fieldService.GetFieldsByFarmIdAsync(farmId.Value);
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
    /// Obtém um talhão por ID
    /// </summary>
    /// <param name="id">ID do talhão</param>
    /// <returns>Talhão encontrado</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FieldResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFieldById(Guid id)
    {
        var response = await _fieldService.GetFieldByIdAsync(id);
        if (response == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Talhão não encontrado",
                Detail = $"Talhão com ID {id} não foi encontrado.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(response);
    }

    /// <summary>
    /// Atualiza um talhão
    /// </summary>
    /// <param name="id">ID do talhão</param>
    /// <param name="request">Dados atualizados do talhão</param>
    /// <returns>Talhão atualizado</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FieldResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateField(Guid id, [FromBody] UpdateFieldRequest request)
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
            var response = await _fieldService.UpdateFieldAsync(request);
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
    /// Ativa um talhão
    /// </summary>
    /// <param name="id">ID do talhão</param>
    /// <returns>Talhão ativado</returns>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(FieldResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateField(Guid id)
    {
        try
        {
            var response = await _fieldService.ActivateFieldAsync(id);
            return Ok(response);
        }
        catch (DomainException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Talhão não encontrado",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    /// <summary>
    /// Desativa um talhão
    /// </summary>
    /// <param name="id">ID do talhão</param>
    /// <returns>Talhão desativado</returns>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(FieldResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateField(Guid id)
    {
        try
        {
            var response = await _fieldService.DeactivateFieldAsync(id);
            return Ok(response);
        }
        catch (DomainException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Talhão não encontrado",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    /// <summary>
    /// Remove um talhão (deleção lógica)
    /// </summary>
    /// <param name="id">ID do talhão</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteField(Guid id)
    {
        try
        {
            await _fieldService.DeleteFieldAsync(id);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Talhão não encontrado",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }
}