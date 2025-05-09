using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Challenge_Odontoprev_API.Models;

[Table("DENTISTA")]
public class Dentista : _BaseEntity
{
    [Key]
    [Column("ID_DENTISTA")]
    public override long Id { get; set; }

    [Required]
    [StringLength(100)]
    [Column("NOME")]
    public string Nome { get; set; }

    [Required]
    [StringLength(20)]
    [Column("CRO")]
    public string CRO { get; set; }

    [Required]
    [StringLength(50)]
    [Column("ESPECIALIDADE")]
    public string Especialidade { get; set; }

    [Required]
    [StringLength(20)]
    [Column("TELEFONE")]
    public string Telefone { get; set; }

    // Navegação (opcional)
    public ICollection<Consulta> Consultas { get; set; }
}