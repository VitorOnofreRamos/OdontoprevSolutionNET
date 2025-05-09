using Microsoft.EntityFrameworkCore;
using Challenge_Odontoprev_API.Models;

namespace Challenge_Odontoprev_API.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public DbSet<Paciente> Patients { get; set; }
    public DbSet<Dentista> Doctors { get; set; }
    public DbSet<Consulta> Consultas { get; set; }
    public DbSet<HistoricoConsulta> Historicos { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>("SEQ_PACIENTE")
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.Entity<Paciente>()
            .Property(c => c.Id)
            .HasDefaultValueSql("SEQ_PACIENTE.NEXTVAL");

        modelBuilder.HasSequence<long>("SEQ_CONSULTA")
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.Entity<Dentista>()
            .Property(c => c.Id)
            .HasDefaultValueSql("SEQ_CONSULTA.NEXTVAL");

        modelBuilder.HasSequence<long>("SEQ_DENTISTA")
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.Entity<Consulta>()
            .Property(c => c.Id)
            .HasDefaultValueSql("SEQ_DENTISTA.NEXTVAL");

        modelBuilder.HasSequence<long>("SEQ_HISTORICO")
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.Entity<HistoricoConsulta>()
            .Property(c => c.Id)
            .HasDefaultValueSql("SEQ_HISTORICO.NEXTVAL");
    }
}
