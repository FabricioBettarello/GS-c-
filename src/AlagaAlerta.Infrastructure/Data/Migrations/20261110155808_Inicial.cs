using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlagaAlerta.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bairros",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    zona = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bairros", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pontos_risco",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    nivel_atencao_cm = table.Column<double>(type: "double precision", nullable: false),
                    nivel_critico_cm = table.Column<double>(type: "double precision", nullable: false),
                    bairro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pontos_risco", x => x.id);
                    table.ForeignKey(
                        name: "fk_pontos_risco_bairros_bairro_id",
                        column: x => x.bairro_id,
                        principalTable: "bairros",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "alertas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nivel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    mensagem = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    encerrado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ponto_risco_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alertas", x => x.id);
                    table.ForeignKey(
                        name: "fk_alertas_pontos_risco_ponto_risco_id",
                        column: x => x.ponto_risco_id,
                        principalTable: "pontos_risco",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sensores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    ponto_risco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sensores", x => x.id);
                    table.ForeignKey(
                        name: "fk_sensores_pontos_risco_ponto_risco_id",
                        column: x => x.ponto_risco_id,
                        principalTable: "pontos_risco",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "leituras",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nivel_agua_cm = table.Column<double>(type: "double precision", nullable: false),
                    chuva_mm_hora = table.Column<double>(type: "double precision", nullable: true),
                    registrada_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sensor_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_leituras", x => x.id);
                    table.ForeignKey(
                        name: "fk_leituras_sensores_sensor_id",
                        column: x => x.sensor_id,
                        principalTable: "sensores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_alertas_ponto_risco_id_ativo",
                table: "alertas",
                columns: new[] { "ponto_risco_id", "ativo" });

            migrationBuilder.CreateIndex(
                name: "ix_bairros_nome",
                table: "bairros",
                column: "nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_leituras_sensor_id_registrada_em",
                table: "leituras",
                columns: new[] { "sensor_id", "registrada_em" });

            migrationBuilder.CreateIndex(
                name: "ix_pontos_risco_bairro_id",
                table: "pontos_risco",
                column: "bairro_id");

            migrationBuilder.CreateIndex(
                name: "ix_sensores_codigo",
                table: "sensores",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sensores_ponto_risco_id",
                table: "sensores",
                column: "ponto_risco_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alertas");

            migrationBuilder.DropTable(
                name: "leituras");

            migrationBuilder.DropTable(
                name: "sensores");

            migrationBuilder.DropTable(
                name: "pontos_risco");

            migrationBuilder.DropTable(
                name: "bairros");
        }
    }
}
