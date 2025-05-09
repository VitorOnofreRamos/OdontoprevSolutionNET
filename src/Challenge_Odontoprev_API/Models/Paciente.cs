using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Challenge_Odontoprev_API.Models;

[Table("PACIENTE")]
public class Paciente : _BaseEntity
{
    [Key]
    [Column("ID_PACIENTE")]
    public override long Id { get; set; }

    [Required]
    [StringLength(30)]
    [Column("NOME")]
    public string Nome { get; set; }

    [Required]
    [Column("DATA_NASCIMENTO")]
    public DateTime Data_Nascimento { get; set; }

    [Required]
    [StringLength(14)]
    [Column("CPF")]
    public string CPF { get; set; }

    [Required]
    [StringLength(200)]
    [Column("ENDERECO")]
    public string Endereco { get; set; }

    [Required]
    [StringLength(20)]
    [Column("TELEFONE")]
    public string Telefone { get; set; }

    [Required]
    [Column("CARTEIRINHA")]
    public long Carteirinha { get; set; }

    public ICollection<Consulta> Consultas { get; set; }
}