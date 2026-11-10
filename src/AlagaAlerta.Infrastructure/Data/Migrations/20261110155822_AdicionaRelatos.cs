using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlagaAlerta.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaRelatos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "relatos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    intensidade = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    status = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    bairro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ponto_risco_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_relatos", x => x.id);
                    table.ForeignKey(
                        name: "fk_relatos_bairros_bairro_id",
                        column: x => x.bairro_id,
                        principalTable: "bairros",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_relatos_pontos_risco_ponto_risco_id",
                        column: x => x.ponto_risco_id,
                        principalTable: "pontos_risco",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_relatos_bairro_id_status",
                table: "relatos",
                columns: new[] { "bairro_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_relatos_ponto_risco_id",
                table: "relatos",
                column: "ponto_risco_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "relatos");
        }
    }
}
