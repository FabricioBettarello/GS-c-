# Evidências

| Arquivo | O que mostra |
|---|---|
| `01-migracoes.png` | API iniciando com o perfil `Postgres`: as migrações `Inicial` e `AdicionaRelatos` aplicadas e o seed inserido |
| `02-banco.png` | Tabelas criadas no PostgreSQL 16, histórico `__EFMigrationsHistory`, pontos de risco com os limites e os alertas gravados |
| `03-swagger.png` | Todos os endpoints no Swagger (`/swagger`) |
| `04-scalar.png` | A mesma documentação no Scalar (`/scalar/v1`) |
| `05-post-leitura-critica.png` | `POST /api/v1/leituras` com 34 cm: resposta 201, nível `CRITICO`, ação `CRIADO` |
| `06-painel.png` | `GET /api/v1/painel` com os indicadores por bairro e os pontos com mais alertas |
| `07-simulador.png` | Simulador do ESP32: alerta criado, escalado para crítico, rebaixado e encerrado |
| `08-testes.png` | `dotnet test` com os 47 testes aprovados |
