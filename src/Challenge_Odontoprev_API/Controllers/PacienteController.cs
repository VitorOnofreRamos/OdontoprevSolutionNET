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
public class PacienteController : ControllerBase
{
    private readonly _IRepository<Paciente> _repository;
    private readonly IMapper _mapper;

    public PacienteController(_IRepository<Paciente> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pacientes = await _repository.GetAll();
        return Ok(_mapper.Map<IEnumerable<PacienteReadDTO>>(pacientes));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var paciente = await _repository.GetById(id);
        if (paciente == null)
            return NotFound();
        return Ok(_mapper.Map<PacienteReadDTO>(paciente));
    }

    [HttpPost]
    public async Task<IActionResult> Create(PacienteCreateDTO dto)
    {
        var paciente = _mapper.Map<Paciente>(dto);
        await _repository.Insert(paciente);

        return CreatedAtAction(
            nameof(GetById), 
            new { id = paciente.Id}, 
            _mapper.Map<PacienteReadDTO>(paciente)
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, PacienteCreateDTO dto)
    {
        var existingPaciente = await _repository.GetById(id);
        if (existingPaciente == null)
            return NotFound();

        _mapper.Map(dto, existingPaciente);
        await _repository.Update(existingPaciente);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize("Admin")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repository.Delete(id);
        return NoContent();
    }
}
