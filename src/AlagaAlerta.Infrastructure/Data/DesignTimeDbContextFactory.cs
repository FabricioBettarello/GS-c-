using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlagaAlerta.Infrastructure.Data;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AlagaAlertaDbContext>
{
    public AlagaAlertaDbContext CreateDbContext(string[] args)
    {
        var conexao = Environment.GetEnvironmentVariable("ConnectionStrings__AlagaAlerta")
            ?? "Host=localhost;Port=5433;Database=alagaalerta;Username=alaga;Password=alaga";

        var options = new DbContextOptionsBuilder<AlagaAlertaDbContext>()
            .UseNpgsql(conexao)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AlagaAlertaDbContext(options);
    }
}
