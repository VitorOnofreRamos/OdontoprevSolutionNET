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
public class ConsultaController : ControllerBase
{
    private readonly _IRepository<Consulta> _repository;
    private readonly IMapper _mapper;

    public ConsultaController(_IRepository<Consulta> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var consulta = await _repository.GetAll();
        return Ok(_mapper.Map<IEnumerable<ConsultaReadDTO>>(consulta));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var consulta = await _repository.GetById(id);
        if (consulta == null)
            return NotFound();
        return Ok(_mapper.Map<ConsultaReadDTO>(consulta));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ConsultaCreateDTO dto)
    {
        var consulta = _mapper.Map<Consulta>(dto);
        await _repository.Insert(consulta);

        return CreatedAtAction(
            nameof(GetById),
            new { id = consulta.Id },
            _mapper.Map<ConsultaReadDTO>(consulta)
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, ConsultaCreateDTO dto)
    {
        var existingConsulta = await _repository.GetById(id);
        if (existingConsulta == null)
            return NotFound();

        _mapper.Map(dto, existingConsulta);
        await _repository.Update(existingConsulta);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repository.Delete(id);
        return NoContent();
    }
}

