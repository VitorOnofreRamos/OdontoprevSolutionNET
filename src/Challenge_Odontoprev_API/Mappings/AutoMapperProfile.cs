using AutoMapper;
using Challenge_Odontoprev_API.Models;
using Challenge_Odontoprev_API.DTOs;

namespace Challenge_Odontoprev_API.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile() {
        CreateMap<Paciente, PacienteReadDTO>();
        CreateMap<PacienteCreateDTO, Paciente>();

        CreateMap<Dentista, DentistaReadDTO>();
        CreateMap<DentistaCreateDTO, Dentista>();

        CreateMap<Consulta, ConsultaReadDTO>();
        CreateMap<ConsultaCreateDTO, Consulta>();

        CreateMap<HistoricoConsulta, HistoricoConsultaReadDTO>();
        CreateMap<HistoricoConsultaCreateDTO, HistoricoConsulta>();
    }
}
