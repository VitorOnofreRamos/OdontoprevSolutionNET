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
public class DentistaController : ControllerBase
{
    private readonly _IRepository<Dentista> _repository;
    private readonly IMapper _mapper;

    public DentistaController(_IRepository<Dentista> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var dentista = await _repository.GetAll();
        return Ok(_mapper.Map<IEnumerable<DentistaReadDTO>>(dentista));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var dentista = await _repository.GetById(id);
        if (dentista == null)
            return NotFound();
        return Ok(_mapper.Map<DentistaReadDTO>(dentista));
    }

    [HttpPost]
    public async Task<IActionResult> Create(DentistaCreateDTO dto)
    {
        var dentista = _mapper.Map<Dentista>(dto);
        await _repository.Insert(dentista);

        return CreatedAtAction(
            nameof(GetById),
            new { id = dentista.Id },
            _mapper.Map<DentistaReadDTO>(dentista)
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, DentistaCreateDTO dto)
    {
        var existingDentista = await _repository.GetById(id);
        if (existingDentista == null)
            return NotFound();

        _mapper.Map(dto, existingDentista);
        await _repository.Update(existingDentista);

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

