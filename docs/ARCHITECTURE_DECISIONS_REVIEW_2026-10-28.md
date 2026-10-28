# ADR — Decisões de arquitetura e limites de validação

**Data:** 2026-10-28
**Status:** análise; decisões de remoção ou migração dependem de métricas e testes.

## Persistência
A solução contém referências a EF Core/SQL Server, Cosmos DB e RavenDB. Até haver inventário dos pontos de leitura/escrita e persistência efetivamente usada, **não remover providers**: risco de regressão e migração de dados. Próxima decisão deve mapear ownership de entidades e custos operacionais.

## Interfaces de API
REST e implementações GraphQL coexistem. Qualquer consolidação exige listar consumidores, endpoints/schema utilizados e compatibilidade; não há evidência suficiente para desativar uma interface.

## Front e streaming
Blazor WebAssembly permanece no MVP; o player WebTorrent em JavaScript deve ter ciclo de vida explícito e fallback HTTP. Experiência tipo Shorts e pré-carregamento estão planejados em `docs/PLAYER_P2P_SHORTS_PLAN_2026-10-28.md` e **não integram as correções do review**.

## Critérios de produção e medição
Instrumentar, em futura fase autorizada, tempo até primeiro frame e p95, taxa de fallback HTTP, falhas de tracker, bytes P2P vs HTTP, memória do navegador e custo por vídeo; não declarar metas atingidas sem dados.

## Controles de acesso GitHub
A integração atual recebeu HTTP 403 ao consultar branch protection da `main`. Configuração e exigência de checks obrigatórios requerem permissão administrativa; **não verificados**.

## Validação pendente
Deploy real e rollback exigem ambiente controlado e autorização específica. Testes de persistência real e reprodução de navegador demandam ambiente externo; não substituí-los por teste unitário nem afirmar conclusão sem execução.

## Revisão de dependências no código — 28/10/2026

**Evidência inspecionada:**
- `src/Infrastructure/OnForkHub.Persistence/OnForkHub.Persistence.csproj` referencia `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.Azure.Cosmos` e `RavenDB.Client`.
- `src/Presentations/OnForkHub.Api/Extensions/CommonServicesExtension.cs` registra `EntityFrameworkDataContext` com `UseSqlServer`, `IDocumentStore` / `RavenDbDataContext` e os repositórios EF e RavenDB de categorias.
- `src/Infrastructure/OnForkHub.Persistence/Repositories/CategoryRepositoryCosmos.cs` implementa persistência de categorias no Cosmos, porém nenhum registro equivalente foi encontrado no arquivo de DI inspecionado. Não é prova de ausência de registro em toda a solução.
- `src/Presentations/OnForkHub.Api/Program.cs` expõe REST quando `AppSettings:ApiMode` é `Rest` ou `All`, HotChocolate quando `HotChocolate` ou `All`, e GraphQL.NET quando `GraphQLNet` ou `All`.

**Decisões para preservar comportamento:**
1. Manter SQL Server/EF Core e RavenDB, pois há caminhos ativos de inicialização e registro de serviços.
2. Classificar Cosmos DB como candidato a descontinuação *somente após* busca global de consumidores, testes de integração e análise de dados; nenhuma remoção nesta revisão.
3. Manter REST e ambos adaptadores GraphQL; `ApiMode` permite seleção, mas a coexistência amplia superfície de suporte.
4. Para o MVP de player anônimo, não acrescentar novas dependências obrigatórias de banco nem exigir login. Uma simplificação estrutural é trabalho futuro, não correção sem risco.
5. Não executar migração de dados, desligar endpoints ou alterar DI nesta fase. Evidência estática não garante segurança dessas mudanças.

**Aceite desta etapa:** revisão documental concluída; simplificação de produção **não autorizada nem validada**.
