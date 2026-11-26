# Plano de Remediacao do OnForkHub — Estado Atual

> Documento de checkpoint do plano de correcao executado com Devin.
> Ultima atualizacao: 2026-11-26 (commit dc8d849a).
> Para retomar o trabalho, leia a secao "O que falta" e as pendencias conhecidas.

## Verificacao atual

| Suite | Resultado |
|---|---|
| Core.Test | 297/297 |
| Application.Test | 149/149 |
| CrossCutting.Tests | 147/147 |
| Persistence.Test | 9/9 |
| Integration Tests | 34/34 |
| **Total** | **636/636 testes passando, build verde** |

## O que foi feito

### Prioridade 0 — desbloquear engenharia (CONCLUIDA, exceto billing)

Commit `6e6fb764`:

- CI reativada: workflows `validate-dotnet10` e `player-tests` disparam em pushes
  para `main`/`dev` e em PRs (antes apontavam para branch inexistente).
- `.gitattributes` criado forcando CRLF em `.cs` (convencao dominante do projeto).
- `global.json` usa `10.0.100` + `rollForward: latestFeature`.
- **NAO RESOLVIDO**: billing do GitHub — os workflows continuam falhando no GitHub
  antes de iniciar ate a conta ser regularizada. Item manual do usuario.

### Prioridade 1 — identidade e autorizacao (CONCLUIDA)

Commits `3ec4b33e`, `3eaef103`, `253d8786`:

- Modelo unico de ID externo: GUID simples, sem prefixo `Video/`/`User/` (parsing
  tolera legado prefixado). Categorias migraram de `long` para GUID string.
- Migration `NormalizeEntityIdsAndRefreshTokenHash` normaliza PKs e FKs existentes
  (drop/recreate ordenado das FKs cascade da join `VideoCategories`).
- Ownership real: update/delete de video exigem dono ou admin; `RequesterUserId`/
  `RequesterIsAdmin` derivados do JWT, nunca do corpo. `InitiateUpload` ignora
  `userId` do corpo — dono e sempre o `NameIdentifier` do token.
- Roles: `User.Role` persistido; primeiro usuario registrado vira Admin (bootstrap
  simples, race-prone — aceitavel neste estagio). Endpoints de categoria exigem
  policy `CanManageContent`.
- Refresh token reescrito: hash SHA256 em banco (nunca texto claro), binding com
  o usuario do access token, rotacao atomica (claim condicional via `ExecuteUpdate`
  com fallback nao-relacional), deteccao de reuso revogando a familia, async
  end-to-end, excecoes logadas.
- Validacao de arquivo por magic bytes (`VideoFileSignatureValidator`) alem de
  extensao/MIME.
- Descoberta: `ApplicationUser : IdentityUser<long>` e codigo morto (nunca
  registrado no DI) — candidato a remocao futura.

### Prioridade 2 — fluxo de upload/publicacao (CONCLUIDA)

Commits `966dc10d`, `a417fe6c`, `dc8d849a`:

- Maquina de estados real do upload: `MarkAsStored` -> `StartProcessing` ->
  `CompleteProcessing`/`FailProcessing`, com artefatos `ThumbnailPath` e
  `TranscodedVariants` na entidade.
- Modelo decidido: o ultimo chunk monta/valida/armazena sincronamente no request
  (status -> Completed); o processamento (transcode/thumbnails) e assincrono;
  publish exige upload Completed.
- Fila real de processamento: `Channel<T>` via `IVideoProcessingChannel`;
  background service reescrito como consumidor da fila com recuperacao de uploads
  pendentes no startup. O servico antigo estava estruturalmente quebrado
  (consultava userId vazio, passava URLs ao ffmpeg em vez de paths fisicos).
- Transcode detecta ffmpeg ausente e degrada graciosamente sem travar a esteira;
  perfis sem upscale. Thumbnail propagada ao video publicado; streaming suporta
  selecao de qualidade.
- Seed de categorias via migration (`SeedDefaultCategories`) — instalacao nova
  nao fica mais impossibilitada de publicar.
- Correcao estrutural do EF (bug de producao real): entidades com chave string
  atribuida em codigo nao sao distinguiveis de novas pelo graph traversal —
  categorias eram re-inseridas no publish. Repositorios agora aplicam o padrao
  de grafos desconectados: detach defensivo de conflitos + `Attach` explicito
  de entidades existentes.
- Teste E2E completo com autenticacao JWT real (sem mocks): registro -> login ->
  initiate -> chunks -> processamento -> publish -> feed -> stream.
- Login invalido retorna 401 (corrigido semanticamente); testes de auth sem
  assercoes fracas que aceitavam multiplos resultados.

## O que falta

### Pendencias conhecidas fora do plano original

1. **Billing do GitHub** (manual) — sem isso a CI nao executa mesmo com gatilhos corretos.
2. **`dotnet format --verify` diverge do SA1137** em lambdas com atributos nos
   endpoints — reconciliar `.editorconfig` deliberadamente e rodar `dotnet format`.
3. **Remover `ApplicationUser`/`IdentityDataContext`** — infraestrutura morta.

### Prioridade 3 — estabilizar contratos (NAO INICIADA)

1. Nunca serializar entidades de dominio diretamente.
2. Criar DTOs estaveis de video, categoria, upload e usuario.
3. Padronizar rotas e envelope.
4. Atualizar frontend e p2p-seeder para o contrato final.
5. Documentar o contrato no OpenAPI.

### Prioridade 4 — fortalecer testes (PARCIAL — auth real ja feita no P2)

1. **SQL Server em container/Testcontainers** — ALTO VALOR: as migrations de
   normalizacao/seed e o padrao de attach nunca rodaram contra SQL Server real;
   o InMemory diverge justamente na area que mais quebrou (tracking/identidade).
2. ~~Remover mocks de autenticacao dos testes end-to-end principais~~ (FEITO no P2).
3. Corrigir testes fracos restantes que aceitam multiplos resultados.
4. Adicionar testes reais de ownership, refresh token, categoria, publicacao e
   autorizacao (parcialmente feito — revisar cobertura).
5. Corrigir o teste flaky de timestamp.

### Prioridade 5 — completar o produto (NAO INICIADA)

1. Implementar login/registro no frontend.
2. Implementar upload real em chunks.
3. Implementar perfil, favoritos, comentarios e likes ligados a API.
4. Substituir fallbacks silenciosos por erros visiveis e telemetria.
5. Validar P2P em ambiente real antes de trata-lo como recurso de producao.

## Riscos e observacoes para quem retomar

- **ffmpeg nao esta instalado** na maquina de desenvolvimento — a degradação
  graciosa e exercitada nos testes; para transcode real, instalar ffmpeg.
- Testes de integracao rodam contra EF InMemory — comportamentos de tracking/
  identidade divergem do SQL Server (vide correcao de grafos desconectados).
- Migration `NormalizeEntityIdsAndRefreshTokenHash` invalida todos os refresh
  tokens legados (estavam em texto claro) — logout forcado e intencional.
- Bootstrap de admin: o primeiro usuario registrado vira Admin — em ambiente
  compartilhado, garantir que a conta administrativa seja criada primeiro.
- Comandos EF exigem `--context EntityFrameworkDataContext` (ha multiplos DbContexts).
- Edicoes em arquivos `.cs` devem preservar CRLF (warnings-as-errors + StyleCop).
