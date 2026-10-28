# OnForkHub — Revisão técnica e plano de ação

**Data:** 28/10/2026  
**Repositório:** RondineleG/OnForkHub  
**Branch de trabalho:** feat/pair-programming  
**Escopo mantido:** plataforma de vídeos, streaming CDN + WebTorrent; **sem pivot de produto**.

> Revisão estática baseada em arquivos consultados via GitHub. Não executamos build, testes, deployment, testes de carga ou auditoria de segurança completa. Os números publicados nos checklists não foram reproduzidos.

## Diagnóstico executivo

**Fortalezas:** solução em camadas (Core, Application, Persistence, API, Web e testes), injeção de dependências, gerenciamento centralizado de pacotes, analisadores, configuração Docker, GitHub Actions e documentação ampla.

**Risco principal:** os controles de qualidade e deploy não estão completamente alinhados com a configuração atual da solução; também há divergência entre a implementação, a documentação e as regras de branch.

## Achados priorizados

| ID | Prioridade | Evidência | Impacto / recomendação |
|---|---|---|---|
| 01 | P0 | `Directory.Build.props` define `net10.0`; `global.json` fixa SDK `10.0.201`; `.github/workflows/build-and-deploy.yml` instala `9.0.x` | Build de CI pode falhar. Instalar SDK compatível com o `global.json`, validar Dockerfiles e reproduzir restore/build/test. |
| 02 | P0 | `.github/workflows/allow-only-dev-to-main-merges.yaml` interpola `github.event.pull_request.head.ref` diretamente em `run` | Risco de injeção de shell por nome de branch controlado externamente. Passar valor por `env` e tratar com aspas; tornar check obrigatório para a `main`. |
| 03 | P1 | Workflow publica imagens com SHA e `latest`, mas deploy altera Compose para `latest` e roda limpeza prévia | Implantação não é estritamente reprodutível; revisar cleanup, utilizar digest/SHA fixo, health gate e rollback testado. |
| 04 | P1 | `README.md` ainda fala .NET 9; `ROADMAP.md` (jan/2026) lista pendências; `CHECKLIST_MASTER.md` (abr/2026) declara 180/180 tarefas e 467 testes | Consolidar documentação por release; somente declarar conclusão com evidência atual de build, testes e comportamento. |
| 05 | P1 | Hook `.husky/commit-msg` possui regra específica para `feature/*`, mas não para `feat/*` | `feat/pair-programming` cai apenas na validação genérica. Escolher convenção canônica e alinhar hooks, workflows e README. |
| 06 | P1 | `OnForkHub.Web.csproj` tem `TypeScriptCompileBlocked=true`; o roadmap registra falhas em compilação TypeScript | Revisar pipeline JS/TS e garantir validação independente quando houver TypeScript ativo. |
| 07 | P1 | `webtorrentService.js` cria cliente único e inicia seed/download sem tratamento explícito de erros nem descarte | Testar tracker indisponível, nenhum peer, cancelamento, desconexão e fallback para CDN. Verificar API de throttling na versão instalada. |
| 08 | P2 | Persistence referencia SQL Server/EF Core, Cosmos DB e RavenDB; API registra REST e dois adaptadores GraphQL | Documentar motivos, fronteiras e ownership de dados; simplificar apenas onde não houver caso de uso real. |
| 09 | P1 | Solução tem testes unitários e de integração, mas números do checklist não foram reproduzidos | Executar `dotnet test`, coletar cobertura confiável, testar persistência real e publicar artefatos no CI. |
| 10 | P1 | Hooks de formatação e testes são locais e não são executados em commits feitos via GitHub API | Exigir checagens equivalentes no CI e configurar branch protection. |

## Hooks e fluxo de commits — leitura do repositório

- **`.husky/commit-msg`** exige `<type>(<scope>): <description>` (scope opcional), com tipos como `feat`, `fix`, `docs`, `refactor`, `test` e `chore`. Uma mensagem válida para este documento é `docs(audit): document technical review and remediation plan`.
- **Regra de tipos por branch:** reconhece `feature/*`, `hotfix/*`, `bugfix/*` e `release/*`, **não** `feat/*`. A branch solicitada continua válida na checagem geral, mas não tem restrição específica.
- **`.husky/pre-commit`** roda CSharpier e `dotnet format style/analyzers`; quando aplica correções, executa `git add .`, com risco de incluir mudanças não relacionadas. Preferir staging seletivo.
- **`.husky/pre-push`** impede push direto em `main` e `dev`, valida condições para `uat` e executa `dotnet test --no-restore`. O arquivo usa `#!/bin/sh` com sintaxe `[[ ... ]]`: verificar compatibilidade de shell.
- **`.github/workflows/validate-commit.yaml`** valida mensagens em push e PR. Tornar status obrigatório no servidor.
- **Atenção:** a integração do GitHub escreve via API e não dispara hooks locais. O commit de documentação deste relatório não equivale a executar CSharpier, testes ou analisadores.

## Plano de remediação

### P0 — correções de bloqueio

1. Corrigir setup do SDK .NET 10 e validar build/test limpos no CI.
2. Corrigir interpolação insegura de nome de branch no GitHub Actions.
3. Garantir proteção da `main` e status checks obrigatórios.

**Aceite:** build reproduzível, PR com origem inválida rejeitado e sem execução de conteúdo externo.

### P1 — qualidade operacional

4. Fixar imagens por commit SHA ou digest e implementar rollback verificado.
5. Adicionar smoke tests de API e publicação de resultados de testes/cobertura.
6. Validar pipeline de vídeo ponta a ponta: upload → transcode → publicação → WebTorrent → fallback CDN.
7. Unificar convenção de branches, hooks e documentação.
8. Verificar compilação JS/TS em pipeline explícito.

**Aceite:** deploy recuperável, testes observáveis, playback resiliente e documentação sincronizada.

### P2 — simplificação baseada em dados

9. Produzir ADR para decisões de bancos, API REST e GraphQL.
10. Medir p95 de reprodução, falhas de tracker, taxa de fallback, banda do servidor, latência e custo por vídeo.
11. Remover componentes somente com evidência de custo ou baixa utilização.

## Considerações de segurança e privacidade

O uso de WebTorrent/WebRTC pode expor IPs aos peers; avaliar transparência ao usuário e requisitos de privacidade. Revisar autenticação/autorização, validação de uploads, limites de recursos, CORS, políticas de conteúdo e gestão de secrets. Estes são itens de auditoria, **não** alegações de vulnerabilidades adicionais confirmadas.

## Arquivos de referência consultados

- `README.md`, `ROADMAP.md`, `CHECKLIST_MASTER.md`, `docs/ARCHITECTURE.md`, `docs/WEBTORRENT_INTEGRATION.md`
- `Directory.Build.props`, `global.json`, `OnForkHub.slnx`
- `src/Presentations/OnForkHub.Api/Program.cs`
- `src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj`
- `src/Presentations/OnForkHub.Web/wwwroot/js/webtorrentService.js`
- `src/Infrastructure/OnForkHub.Persistence/OnForkHub.Persistence.csproj`
- `.github/workflows/build-and-deploy.yml`, `.github/workflows/allow-only-dev-to-main-merges.yaml`, `.github/workflows/validate-commit.yaml`
- `.husky/commit-msg`, `.husky/pre-commit`, `.husky/pre-push`, `.husky/task-runner.json`

**Estado:** relatório para revisão; nenhuma alteração na aplicação, nenhum PR e nenhuma execução dos testes.
