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
