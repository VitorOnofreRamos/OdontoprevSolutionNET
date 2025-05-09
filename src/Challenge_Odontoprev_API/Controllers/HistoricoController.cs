using Challenge_Odontoprev_API.Models;
using Challenge_Odontoprev_API.Repositories;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Challenge_Odontoprev_API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Challenge_Odontoprev_API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class HistoricoController : ControllerBase
{
    private readonly _IRepository<HistoricoConsulta> _repository;
    private readonly IMapper _mapper;

    public HistoricoController(_IRepository<HistoricoConsulta> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var historico = await _repository.GetAll();
        return Ok(_mapper.Map<IEnumerable<HistoricoConsultaReadDTO>>(historico));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var historico = await _repository.GetById(id);
        if (historico == null)
            return NotFound();
        return Ok(_mapper.Map<HistoricoConsultaReadDTO>(historico));
    }

    [HttpPost]
    public async Task<IActionResult> Create(HistoricoConsultaCreateDTO dto)
    {
        var historico = _mapper.Map<HistoricoConsulta>(dto);
        await _repository.Insert(historico);

        return CreatedAtAction(
            nameof(GetById),
            new { id = historico.Id },
            _mapper.Map<HistoricoConsultaReadDTO>(historico)
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, HistoricoConsultaCreateDTO dto)
    {
        var existingHistorico = await _repository.GetById(id);
        if (existingHistorico == null)
            return NotFound();

        _mapper.Map(dto, existingHistorico);
        await _repository.Update(existingHistorico);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repository.Delete(id);
        return NoContent();
    }
}

