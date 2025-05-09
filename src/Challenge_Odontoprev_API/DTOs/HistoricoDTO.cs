namespace Challenge_Odontoprev_API.DTOs;

public class HistoricoConsultaCreateDTO
{
    public long ID_Consulta { get; set; }
    public DateTime Data_Atendimento { get; set; }
    public string Motivo_Consulta { get; set; }
    public string Observacoes { get; set; }
}

public class HistoricoConsultaReadDTO
{
    public long ID { get; set; }
    public long ID_Consulta { get; set; }
    public DateTime Data_Atendimento { get; set; }
    public string Motivo_Consulta { get; set; }
    public string Observacoes { get; set; }
}
