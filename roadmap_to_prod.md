# OnForkHub - Roadmap to Production

**Projeto:** OnForkHub - Plataforma de Video Streaming
**Stack:** C# / .NET 9 / ASP.NET Core / Blazor WebAssembly / SQL Server / EF Core
**Data da Auditoria:** 2026-02-15
**Ultima Atualizacao:** 2026-02-16
**Branch Auditada:** `feature/phase4-user-features`
**Status Atual:** Sprint 1.1 e 1.2 parcialmente concluidos - Hardening em andamento

---

## Sumario Executivo

O OnForkHub possui uma base arquitetural solida com Clean Architecture, Domain-Driven Design, e infraestrutura DevOps madura. Porem, existem lacunas criticas que impedem o deploy em producao: **refresh tokens em memoria**, **frontend desconectado da API**, **cobertura de testes insuficiente (~40%)**, e **testes nao executados no CI/CD**. Este documento mapeia todas as pendencias organizadas por prioridade e fase.

**Score Geral de Maturidade:** 7.0/10 (atualizado apos correcoes)

| Dimensao | Score Anterior | Score Atual | Status |
|----------|---------------|-------------|--------|
| Domain Layer | 7.3/10 | 8.5/10 | Bom (HotChocolate removido, GraphQL movido) |
| Application Layer | 6.5/10 | 7.5/10 | Bom (DIP corrigido, V3 removido) |
| Infrastructure/Persistence | 7.0/10 | 7.5/10 | Bom (AsNoTracking fixado, filename corrigido) |
| API Layer | 7.5/10 | 8.0/10 | Bom (CORS, JWT config, auth/authz pipeline) |
| Frontend (Blazor) | 3.0/10 | 3.0/10 | Critico (sem alteracoes ainda) |
| Seguranca | 5.5/10 | 6.5/10 | Atencao (CORS configurado, JWT no appsettings) |
| Testes | 4.5/10 | 4.5/10 | Critico (sem novos testes ainda) |
| CI/CD | 7.0/10 | 8.0/10 | Bom (testes adicionados ao pipeline) |
| DevOps/Infra | 7.5/10 | 8.0/10 | Bom (appsettings por ambiente) |

---

## 1. Estrutura do Squad Recomendado

### Tech Lead (.NET)
- Aprovar PRs e garantir padroes arquiteturais
- Resolver violacoes identificadas neste documento (GraphQL no Domain, acoplamento Application->Persistence)
- Definir ADRs (Architecture Decision Records) para decisoes-chave

### Backend Developer (C#/ASP.NET)
- Implementar correcoes de seguranca (refresh tokens persistidos, CORS)
- Completar endpoints faltantes (User Profile CRUD)
- Resolver problemas de performance (N+1, AsNoTracking)

### Frontend Developer (Blazor)
- Integrar frontend com API REST
- Implementar telas de autenticacao (Login/Register)
- Construir paginas de CRUD (Videos, Categories)
- Adicionar estados de loading, tratamento de erros

### DBA / SQL Specialist
- Revisar indices compostos
- Implementar migrations versionadas
- Configurar ambientes separados (dev/homolog/prod)
- Implementar estrategia de backup

### UX/UI Designer
- Mapear jornadas de usuario completas
- Criar design system documentado
- Revisar responsividade e acessibilidade
- Implementar feedback visual em todas as acoes

### QA
- Elevar cobertura de testes para 70%+
- Criar testes de integracao para todos os endpoints
- Implementar testes de carga basicos
- Garantir pipeline bloqueando merge sem testes

### PO
- Priorizar backlog baseado neste roadmap
- Definir criterios de aceite para cada fase
- Coordenar homologacao com usuarios reais

---

## 2. Diagnostico Arquitetural Detalhado

### 2.1 Mapa de Dependencias (CORRIGIDO em 2026-02-16)

```
OnForkHub.Core (Domain) -- LIMPO: sem frameworks externos
  +-- OnForkHub.Abstractions (Resources/Localization)
  +-- Microsoft.Extensions.DependencyInjection.Abstractions

OnForkHub.Application
  +-- OnForkHub.Core
  +-- OnForkHub.CrossCutting
  +-- BCrypt.Net-Next

OnForkHub.Persistence (Infrastructure)
  +-- OnForkHub.Core

OnForkHub.CrossCutting (Shared)
  +-- OnForkHub.Core
  +-- HotChocolate (movido do Core)
  +-- GraphQL

OnForkHub.Api (Presentation - Composition Root)
  +-- OnForkHub.Application
  +-- OnForkHub.Persistence
  +-- OnForkHub.CrossCutting

OnForkHub.Web (Presentation)
  +-- OnForkHub.Web.Components
```

### 2.2 Violacoes Arquiteturais Identificadas

| # | Violacao | Severidade | Status |
|---|---------|-----------|--------|
| V1 | ~~HotChocolate no Domain Layer~~ | ~~CRITICA~~ | CORRIGIDO - Removido do Core.csproj, movido para CrossCutting |
| V2 | ~~Application referencia Persistence~~ | ~~ALTA~~ | CORRIGIDO - ProjectReference removida, repos registrados no Composition Root |
| V3 | ~~GraphQL interfaces/classes no Domain~~ | ~~ALTA~~ | CORRIGIDO - Movido para CrossCutting/GraphQL/ |
| V4 | **JsonConverter no Domain** - Acoplamento a serializacao | MEDIA | PENDENTE |
| V5 | **RequestResult no Domain** - Pattern de Application dentro de Entities | MEDIA | PENDENTE |
| V6 | **Repository interfaces com sufixo EF** - Detalhe de implementacao no nome | BAIXA | PENDENTE |

### 2.3 Mapa de Dependencias Corrigido (Target)

```
OnForkHub.Core (Domain) -- ZERO dependencias externas (exceto Abstractions)
  +-- OnForkHub.Abstractions

OnForkHub.Application
  +-- OnForkHub.Core (apenas)

OnForkHub.Persistence
  +-- OnForkHub.Core
  +-- OnForkHub.Application (para implementar interfaces)

OnForkHub.CrossCutting
  +-- OnForkHub.Core

OnForkHub.Api
  +-- OnForkHub.Application
  +-- OnForkHub.Persistence (DI registration only)
  +-- OnForkHub.CrossCutting
```

---

## 3. Backend Checklist

### 3.1 Arquitetura

- [x] Domain com entidades ricas (factory methods, validacao encapsulada)
- [x] Value Objects implementados (Email, Name, Title, Url, Id)
- [x] Excecoes de dominio especificas (DomainException, ValidationException, NotFoundException, ConflictException)
- [x] Repository interfaces no Domain
- [x] Use Cases com interface IUseCase<TInput, TOutput>
- [x] BaseEndPoint com mapeamento padrao de status HTTP
- [x] Middleware global de erro (GlobalExceptionHandlerMiddleware)
- [x] Response padrao da API (MapResponse com EResultStatus)
- [x] Nullability habilitado (Directory.Build.props)
- [x] Analyzers ativos (StyleCop + Microsoft.CodeAnalysis.NetAnalyzers)
- [x] ~~Remover HotChocolate do OnForkHub.Core.csproj~~ (CORRIGIDO 2026-02-16)
- [x] ~~Mover GraphQL interfaces/classes para CrossCutting~~ (CORRIGIDO 2026-02-16)
- [x] ~~Remover referencia Application -> Persistence~~ (CORRIGIDO 2026-02-16)
- [ ] **Implementar Domain Events** (inexistente - IAggregateRoot existe mas sem DomainEvents)
- [ ] **Reduzir duplicacao de validacao** (ValidateEntityState() + ValidationService ambos usados)
- [ ] CQRS aplicado (atualmente nao ha separacao Command/Query - avaliar necessidade)
- [ ] Logging estruturado com Serilog (atualmente usa ILogger padrao)

### 3.2 Performance

- [x] AsNoTracking em queries de leitura (maioria dos repositorios)
- [x] AsSplitQuery para carregamento de relacionamentos (VideoRepositoryEF)
- [x] Paginacao implementada (PagedResultDto, PaginationRequestDto)
- [x] Cache estrategico com Memory (implementado e configuravel)
- [x] Response Compression middleware ativo
- [x] ~~FIX: CategoryRepositoryEF.GetTotalCountAsync() sem AsNoTracking()~~ (CORRIGIDO 2026-02-16)
- [x] ~~FIX: NotificationRepositoryEF.MarkAllAsReadAsync() otimizado~~ (CORRIGIDO 2026-02-16 - adicionado early return e removido Entry.State manual)
- [ ] **FIX: VideoRepositoryEF usa string Contains() para filtro de categoria** - nao aproveita indices
- [ ] Indices compostos faltantes: (UserId, Status, CreatedAt) em Notifications
- [ ] **Revisar: GetAllEndpoint de Categories com paginacao hardcoded (page=1, size=10)**
- [ ] Benchmark baseline para endpoints criticos
- [ ] Cache Redis habilitado para producao (atualmente `"UseRedis": false`)

### 3.3 Seguranca

- [x] JWT implementado (JwtTokenService com HmacSha256)
- [x] Refresh Token implementado (geracao, validacao, revogacao)
- [x] Security Headers (X-Content-Type-Options, X-Frame-Options, CSP, HSTS)
- [x] Rate Limiting configurado (100 req/min geral, 500 auth, 50 anon)
- [x] Authorization em endpoints de escrita (RequireAuthorization())
- [x] BCrypt para hash de senhas (salt factor 12)
- [x] ClockSkew = TimeSpan.Zero (previne token replay)
- [ ] **CRITICO: Refresh Tokens armazenados em ConcurrentDictionary (memoria)**
  - `JwtTokenService.cs` linha 17: `static ConcurrentDictionary<string, RefreshTokenInfo>`
  - Problema: tokens perdidos no restart, nao escala horizontalmente
  - Solucao: persistir em banco ou Redis
- [x] ~~Configuracao JWT adicionada ao appsettings.json~~ (CORRIGIDO 2026-02-16 - Jwt:SecretKey, Issuer, Audience configurados)
- [x] ~~CORS configurado explicitamente~~ (CORRIGIDO 2026-02-16 - Policy "DefaultPolicy" com origins configuraveis por ambiente)
- [ ] **ALTO: Notification endpoints recebem UserId como query parameter**
  - Deve extrair do JWT Claims (ClaimTypes.NameIdentifier)
  - Vulnerabilidade: usuario pode acessar notificacoes de outros
- [ ] **MEDIO: Rate limiting sem protecao especifica em endpoints de auth**
  - Login/Register sem limite separado contra brute force
- [ ] **MEDIO: CSP permite 'unsafe-inline' e 'unsafe-eval'**
  - Risco de XSS - restringir quando possivel
- [ ] Protecao contra over-posting (usar [Bind] ou DTOs dedicados - parcialmente implementado)
- [x] ~~Catalog de banco renomeado de "Persons" para "OnForkHub"~~ (CORRIGIDO 2026-02-16)
- [ ] Implementar audit trail para operacoes criticas
- [ ] Rotacao de chaves JWT

---

## 4. Banco de Dados

### 4.1 Estado Atual

| Aspecto | Status | Detalhe |
|---------|--------|---------|
| ORM | EF Core 9 | SQL Server + Cosmos + RavenDB |
| Entity Configs | Fluent API | 4 configuracoes (User, Video, Category, Notification) |
| Migrations | NAO ENCONTRADAS | Risco de schema drift |
| Indices | Parciais | Faltam compostos |
| Soft Delete | Notification apenas | Nao padronizado |
| Connection String | Hardcoded | `localhost`, catalog `Persons` |

### 4.2 Checklist

- [x] Configuracoes de entidade via Fluent API (IEntityTypeConfiguration)
- [x] Value Objects mapeados como Owned Entities (Email, Name, Title)
- [x] Indices em colunas de busca frequente (CreatedAt, Email, Title, UserId)
- [x] Enum storage como string (NotificationConfiguration)
- [x] Cascade delete configurado onde apropriado (User -> Videos)
- [x] MaxLength definido em todas as colunas string
- [ ] **CRITICO: Migrations nao encontradas no projeto**
  - Sem historico de alteracoes de schema
  - Sem script de rollback
  - Recomendacao: `dotnet ef migrations add InitialCreate`
- [ ] **ALTO: Indice composto faltante** em Notifications: (UserId, Status, CreatedAt)
- [ ] **ALTO: Indice composto faltante** em Videos: (UserId, CreatedAt)
- [x] ~~Connection string com catalog corrigido para "OnForkHub"~~ (CORRIGIDO 2026-02-16)
- [ ] **MEDIO: Multi-database inconsistente** - Apenas Category tem Cosmos/RavenDB; User, Video, Notification sao EF only
- [ ] Soft delete padronizado (apenas Notification tem Delete status)
- [ ] Scripts de seed data para desenvolvimento
- [ ] Ambiente de homologacao isolado
- [ ] Plano de backup automatizado
- [ ] Plano de rollback documentado
- [ ] Retry policy no EF Core:
  ```csharp
  options.UseSqlServer(conn, sql => sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(2), null));
  ```

---

## 5. Frontend (Blazor WebAssembly)

### 5.1 Estado Atual - CRITICO

O frontend Blazor e a area mais defasada do projeto. Funciona como um **player de video standalone** sem integracao com a API backend.

| Aspecto | Status | Detalhe |
|---------|--------|---------|
| Integracao com API | NAO EXISTE | Videos hardcoded no codigo |
| Autenticacao UI | NAO EXISTE | Sem Login/Register |
| CRUD Pages | NAO EXISTE | Sem paginas de gestao |
| State Management | BASICO | Variaveis de componente |
| Internacionalizacao | NAO EXISTE | Texto em portugues hardcoded |
| Componentes | 6 total | Menu, Footer, Layout, VideoCard, Home, TorrentPlayer |
| Acessibilidade | PARCIAL | FocusOnNavigate, alert roles |

### 5.2 Checklist

**Estrutura:**
- [x] Separacao entre Pages e Components
- [x] Layout responsivo com CSS Variables
- [x] Bootstrap 5.3 integrado
- [x] Player de video funcional (Plyr)
- [x] WebTorrent integration
- [ ] **CRITICO: Integrar HttpClient com API REST**
  - Configurar base URL, headers, interceptors
- [ ] **CRITICO: Implementar AuthenticationStateProvider**
  - Token storage (localStorage/sessionStorage)
  - Login/Logout flow
- [ ] **CRITICO: Criar paginas essenciais:**
  - Login.razor
  - Register.razor
  - VideoList.razor (com busca e filtros)
  - VideoUpload.razor
  - CategoryManagement.razor
  - UserProfile.razor
  - NotificationCenter.razor
- [ ] **ALTO: Componentizacao real**
  - Criar componentes reutilizaveis: DataTable, Pagination, SearchBar, Modal, Toast, FormField
- [ ] **ALTO: Estado organizado**
  - Implementar service-based state management ou Fluxor
- [ ] **ALTO: Loading states em TODAS as chamadas de API**
- [ ] **ALTO: Tratamento de erro visual (Toast/Snackbar)**
- [ ] **MEDIO: Validacao client-side com DataAnnotations**
- [ ] **MEDIO: Responsividade validada em mobile**
- [ ] **BAIXO: Internacionalizacao (pt-BR / en-US)**
- [ ] **BAIXO: Dark/Light mode toggle**

### 5.3 Paginas Funcionais vs. Necessarias

```
Existentes:           Necessarias para producao:
-----------           -------------------------
Home.razor      -->   Home.razor (integrado com API)
TorrentPlayer   -->   TorrentPlayer.razor
                      Login.razor ***
                      Register.razor ***
                      VideoList.razor ***
                      VideoDetail.razor ***
                      VideoUpload.razor ***
                      CategoryList.razor
                      UserProfile.razor
                      NotificationCenter.razor
                      AdminDashboard.razor
                      Error.razor (pagina de erro)
                      NotFound.razor (404)
```

---

## 6. UX Checklist

- [ ] Fluxo de usuario mapeado (jornadas: cadastro -> login -> upload -> assistir)
- [ ] Tela de erro amigavel (Error Boundary customizado)
- [ ] Feedback visual em acoes (Toast/Snackbar para success/error)
- [ ] Confirmacao em acoes destrutivas (Modal de confirmacao em Delete)
- [ ] Consistencia de design (design tokens documentados)
- [ ] Padrao visual documentado (Style Guide)
- [ ] **Star/Share buttons funcionais** (atualmente sem handler na Home.razor)
- [ ] **Menu com navegacao completa** (atualmente so tem Home)
- [ ] **Breadcrumbs para navegacao profunda**
- [ ] **Empty states para listas vazias**
- [ ] **Skeleton loading para conteudo**

---

## 7. QA Estrategico

### 7.1 Cobertura Atual por Camada

| Camada | Itens Total | Testados | Cobertura | Meta |
|--------|------------|----------|-----------|------|
| Core Entities | 8 | 8 | 100% | 100% |
| Value Objects | 5+ | 5+ | 100% | 100% |
| Validations | 4+ | 4+ | 100% | 100% |
| Services | 5 | 3 | 60% | 80% |
| **Use Cases** | **16** | **5** | **31%** | **80%** |
| **API Endpoints** | **22+** | **5** | **23%** | **70%** |
| Repositories | 6 | 1 | 17% | 60% |
| DTOs | 15+ | 4 | 27% | 50% |
| Middleware | 3 | 2 | 67% | 80% |
| Authorization | 4+ | 3+ | 75% | 90% |
| **OVERALL** | **~100+** | **~40** | **~40%** | **70%** |

### 7.2 Testes Faltantes por Prioridade

**CRITICOS (Bloqueia producao):**
- [ ] Use Cases faltantes (11 de 16 nao testados):
  - [ ] `DeleteCategoryUseCase`
  - [ ] `GetAllCategoryUseCase`
  - [ ] `GetByIdCategoryUseCase`
  - [ ] `UpdateCategoryUseCase`
  - [ ] `GetUserProfileUseCase`
  - [ ] `UpdateUserProfileUseCase`
  - [ ] `DeleteVideoUseCase`
  - [ ] `GetAllVideoUseCase`
  - [ ] `GetByIdVideoUseCase`
  - [ ] `UpdateVideoUseCase`
  - [ ] `LoginUserUseCase` (existe mas cobertura minima)
- [ ] Testes de integracao para endpoints de Auth (Login, Register, Refresh)
- [ ] Testes de integracao para endpoints de Video (6 endpoints)
- [ ] Testes de integracao para endpoints de Notification (8 endpoints)

**ALTOS:**
- [ ] Testes de repositorio com InMemory DbContext
- [ ] Testes de fluxo de autenticacao completo (register -> login -> refresh -> revoke)
- [ ] Testes de autorizacao (policies, roles)
- [ ] Testes de NotificationService

**MEDIOS:**
- [ ] Teste de carga basico (k6 ou NBomber)
- [ ] Testes de DTO validation (request/response)
- [ ] Testes de GraphQL handlers

**BAIXOS:**
- [ ] Mutation testing (Stryker.NET)
- [ ] E2E tests para Blazor (Playwright)
- [ ] Contract testing para API

### 7.3 Checklist QA

- [ ] Cobertura minima 70% no Core (atualmente ~40%)
- [ ] Testes de integracao para todos os endpoints
- [ ] Teste de carga basico
- [ ] Automacao de testes criticos
- [ ] **CRITICO: Pipeline bloqueando merge sem testes (testes nao rodam no CI!)**
- [ ] Plano de testes documentado
- [ ] Testes de regressao automatizados

---

## 8. DevOps / Infraestrutura

### 8.1 Estado Atual

| Aspecto | Status | Detalhe |
|---------|--------|---------|
| GitHub Workflows | 6 configurados | Build, deploy, quality, branch protection |
| Docker | Multi-stage builds | API (512MB) + Web (128MB) + Nginx |
| SSL | ZeroSSL | Validacao automatica no deploy |
| Health Checks | Configurados | Container-level |
| Code Quality | CSharpier + Qodana | Pre-commit + CI |
| Git Hooks | 7 hooks (Husky) | Pre-commit, pre-push, commit-msg |
| Notifications | Telegram | PR + Fork notifications |

### 8.2 Checklist

- [x] Ambiente Dev (local com Docker)
- [x] CI/CD funcional (GitHub Actions + Docker + VPS)
- [x] Dockerfile otimizado (multi-stage, caching, memory limits)
- [x] Docker Compose com reverse proxy (Nginx)
- [x] Versionamento semantico (commit conventions + Husky)
- [x] Healthcheck endpoints configurados
- [x] Code formatting automatico (pre-commit hooks)
- [x] Branch protection (main/dev protegidos)
- [x] SSL/HTTPS configurado
- [x] ~~Testes adicionados ao CI/CD pipeline~~ (CORRIGIDO 2026-02-16 - steps de restore, build, test com cobertura)
- [ ] **ALTO: Qodana nao bloqueia build** (`continue-on-error: true`)
- [x] ~~Variavies de ambiente por ambiente~~ (CORRIGIDO 2026-02-16 - appsettings.Development.json e Production.json criados)
- [ ] **ALTO: Cobertura nao e rastreada/reportada no CI**
  - Coverlet gera relatorios, mas nao sao publicados
  - Implementar Codecov ou similar
- [ ] **MEDIO: IP hardcoded nos health checks** do workflow (172.245.152.43)
- [ ] Ambiente de Homologacao separado
- [ ] Monitoramento ativo (Application Insights / Grafana)
- [ ] Alertas automaticos para erros em producao
- [ ] Rollback automatizado no deploy

---

## 9. Debt Kill List

### 9.1 Codigo Morto / Desnecessario

| Item | Localizacao | Acao |
|------|------------|------|
| ~~V3 Use Cases duplicados~~ | ~~`UseCases/Categories/V3/`~~ | REMOVIDO (2026-02-16) |
| ~~GraphQL no Domain~~ | ~~`OnForkHub.Core/GraphQL/`~~ | MOVIDO para CrossCutting (2026-02-16) |
| **Multi-database inconsistente** | `CategoryRepositoryCosmos`, `CategoryRepositoryRavenDB` | DECIDIR: usar para todos ou remover |
| **CategoryServiceRavenDB** | `Application/Services/CategoryServiceRavenDB.cs` | Se RavenDB nao e estrategico, REMOVER |
| **BaseService wrapper** | `Application/Services/Base/BaseService.cs` | AVALIAR - try-catch generico pode ser middleware |
| **GraphQL Handlers pass-through** | `Application/GraphQL/Handlers/` | SIMPLIFICAR - apenas delegam para Use Cases |
| ~~IdentityDataContext.cs.cs~~ | ~~`Persistence/Contexts/`~~ | CORRIGIDO para IdentityDataContext.cs (2026-02-16) |

### 9.2 Acoplamentos Desnecessarios

| Acoplamento | Impacto | Acao |
|------------|---------|------|
| ~~`Application -> Persistence`~~ | ~~Quebra DIP~~ | CORRIGIDO (2026-02-16) |
| ~~`HotChocolate em Core.csproj`~~ | ~~Domain impuro~~ | CORRIGIDO (2026-02-16) |
| `JsonConverter` em entities | Acoplamento a serializacao | Mover para infra/mappers |
| `BCrypt` em Application | OK para agora | Considerar abstrair como IPasswordHasher |
| `RequestResult` em Domain entities | Mistura Application/Domain | Considerar entities lancarem excecoes, Application converte para RequestResult |

### 9.3 Padronizacao Pendente

- [ ] **Nomenclatura de Repository interfaces**: remover sufixo `EF` (IUserRepositoryEF -> IUserRepository)
- [ ] **Validacao interface unificada**: escolher entre `IValidationService` e `IEntityValidator` (nao ambos)
- [ ] **DI registration**: usar auto-registration OU manual (nao ambos em DependencyInjection.cs)
- [ ] **DTO factory methods**: mover `CategoryRequestDto.Create()` e `VideoCreateRequestDto.ToVideo()` para mappers
- [ ] **Tuple parameters em Use Cases**: substituir `(Id, DTO)` por record dedicado
  - `UpdateUserProfileUseCase` usa `(Id UserId, UpdateUserProfileRequestDto Request)`
- [x] ~~Title.Value setter corrigido para `{ get; private set; }`~~ (CORRIGIDO 2026-02-16)
- [ ] **Padrao unico de resposta API**: todos os endpoints devem usar BaseEndPoint.HandleUseCase
  - Notification endpoints usam try-catch manual em vez de HandleUseCase
- [ ] **Excecoes genericas**: refinar catch blocks em BaseService (nao engolir stack traces)
- [ ] **Comentarios obsoletos**: remover `#pragma warning disable` desnecessarios

---

## 10. Roadmap Estrategico

### Fase 1 - Hardening Tecnico (2-3 sprints)

**Objetivo:** Corrigir violacoes arquiteturais e gaps de seguranca criticos.

#### Sprint 1.1 - Correcoes Arquiteturais (CONCLUIDO 2026-02-16)
- [x] Remover HotChocolate de OnForkHub.Core.csproj
- [x] Mover `Core/GraphQL/` e `Core/Interfaces/GraphQL/` para CrossCutting
- [x] Mover `Core/Interfaces/Configuration/IEndpointAsync` para CrossCutting (descoberto durante execucao)
- [x] Remover referencia Application -> Persistence no .csproj
- [x] Repositorios ja registrados via DI no API layer (Composition Root)
- [x] Remover V3 use cases duplicados
- [ ] Renomear repository interfaces (remover sufixo EF) - PENDENTE
- [x] Fix: `Title.Value` setter -> `{ get; private set; }`
- [x] Fix: `IdentityDataContext.cs.cs` -> `IdentityDataContext.cs`
- [x] Fix: Indentacao SA1137 em 4 endpoints (UploadEndpoint, LoginEndpoint, RegisterEndpoint, RefreshTokenEndpoint)

#### Sprint 1.2 - Seguranca Critica (PARCIALMENTE CONCLUIDO 2026-02-16)
- [ ] Persistir Refresh Tokens no banco (tabela dedicada ou Redis) - PENDENTE
- [x] Adicionar configuracao JWT ao appsettings (Secret, Issuer, Audience)
- [x] Configurar CORS restritivo (origins explicitos por ambiente)
- [x] Adicionar Authentication + Authorization middleware no Program.cs
- [ ] Extrair UserId do JWT Claims nos Notification endpoints - PENDENTE
- [ ] Rate limiting especifico para endpoints de auth - PENDENTE
- [x] Renomear catalog do banco de "Persons" para "OnForkHub"
- [x] Criar appsettings.Development.json e appsettings.Production.json

#### Sprint 1.3 - Performance & Database
- [x] Fix: AsNoTracking em CategoryRepositoryEF.GetTotalCountAsync()
- [x] Fix: NotificationRepositoryEF.MarkAllAsReadAsync() otimizado (early return + sem Entry.State manual)
- [ ] Adicionar indices compostos (UserId+Status+CreatedAt em Notifications)
- [ ] Implementar EF Core retry policy
- [ ] Criar migration inicial com `dotnet ef migrations add InitialCreate`
- [ ] Versionar scripts de banco

### Fase 2 - Cobertura de Testes (2-3 sprints)

**Objetivo:** Atingir 70% de cobertura e habilitar quality gate no CI.

#### Sprint 2.1 - Testes de Use Cases
- [ ] Testar todos os 11 use cases faltantes
- [ ] Validar cenarios de sucesso e falha
- [ ] Testar edge cases (null inputs, IDs inexistentes, duplicatas)

#### Sprint 2.2 - Testes de Integracao
- [ ] Testes de integracao para Auth endpoints (register -> login -> refresh -> revoke)
- [ ] Testes de integracao para Video endpoints (CRUD completo)
- [ ] Testes de integracao para Notification endpoints
- [ ] Testes de repositorio com InMemory DbContext

#### Sprint 2.3 - CI/CD Quality Gates
- [x] Adicionar `dotnet test` ao build-and-deploy.yml (CORRIGIDO 2026-02-16)
- [ ] Configurar upload de cobertura (Codecov)
- [ ] Definir threshold minimo (70%)
- [ ] Tornar Qodana bloqueante (`continue-on-error: false`)
- [ ] Adicionar badge de cobertura ao README

### Fase 3 - Frontend & UX (3-4 sprints)

**Objetivo:** Transformar o frontend de demo em aplicacao funcional.

#### Sprint 3.1 - Infraestrutura Frontend
- [ ] Configurar HttpClient com base URL e token interceptor
- [ ] Implementar AuthenticationStateProvider
- [ ] Criar service layer para API calls (IVideoApiService, ICategoryApiService, etc.)
- [ ] Configurar token storage seguro
- [ ] Criar componentes base (DataTable, Modal, Toast, Pagination, FormField)

#### Sprint 3.2 - Paginas de Autenticacao
- [ ] Login.razor com validacao
- [ ] Register.razor com validacao
- [ ] Forgot Password flow (se necessario)
- [ ] Protected routes (AuthorizeRouteView)
- [ ] Redirect para login quando nao autenticado

#### Sprint 3.3 - Paginas de CRUD
- [ ] VideoList.razor (busca, filtros, paginacao)
- [ ] VideoDetail.razor (player + metadata)
- [ ] VideoUpload.razor (upload com progress bar)
- [ ] CategoryList.razor (CRUD basico)
- [ ] UserProfile.razor (visualizar/editar perfil)

#### Sprint 3.4 - UX Polish
- [ ] NotificationCenter.razor (badge com contagem)
- [ ] Error boundaries customizados
- [ ] Loading states em todas as chamadas
- [ ] Empty states para listas vazias
- [ ] Toast notifications para feedback
- [ ] Validacao client-side em todos os formularios
- [ ] Responsividade validada (mobile, tablet, desktop)

### Fase 4 - Pre-Producao (1-2 sprints)

**Objetivo:** Validar estabilidade e preparar go-live.

#### Sprint 4.1 - Estabilizacao
- [ ] QA intensivo com cenarios reais
- [ ] Teste de carga basico (k6 / NBomber - 100 usuarios simultaneos)
- [ ] Fix de bugs identificados
- [ ] Logs auditaveis com Serilog (estruturado, por ambiente)
- [ ] Monitoramento: healthcheck endpoint expandido (/health/ready, /health/live)

#### Sprint 4.2 - Go-Live Preparation
- [ ] Checklist de seguranca final (OWASP Top 10)
- [ ] Backup de banco validado (restore testado)
- [ ] Plano de rollback documentado e testado
- [ ] Documentacao minima da API (Swagger atualizado)
- [ ] README com setup instructions
- [ ] Configurar ambiente de producao (env vars, secrets)
- [ ] DNS e SSL final configurados

### Fase 5 - Go Live

- [ ] Deploy controlado (canary ou blue-green se possivel)
- [ ] Monitoramento ativo nas primeiras 48h
- [ ] War room com equipe disponivel
- [ ] Metricas de baseline (response time, error rate, throughput)
- [ ] Post-mortem do deploy (lições aprendidas)

---

## 11. Criterio de Pronto para Producao

O sistema **SO entra em producao** quando TODOS os itens abaixo estiverem marcados:

### Gate 1 - Seguranca
- [ ] Zero vulnerabilidade critica aberta
- [ ] Refresh tokens persistidos (nao em memoria)
- [ ] JWT configurado com secrets seguros (nao default)
- [ ] CORS restritivo configurado
- [ ] UserId extraido de JWT Claims (nao query params)
- [ ] Rate limiting em auth endpoints
- [ ] HTTPS obrigatorio
- [ ] Security headers ativos e validados

### Gate 2 - Qualidade
- [ ] Testes passando (100% green)
- [ ] Cobertura >= 70% no Core
- [ ] Testes de integracao para todos os endpoints criticos
- [ ] Pipeline executando testes antes do deploy
- [ ] Zero erro critico no Qodana

### Gate 3 - Estabilidade
- [ ] Logs estruturados ativos
- [ ] Banco otimizado (indices, queries)
- [ ] Migrations versionadas e aplicadas
- [ ] Retry policy configurado no EF Core
- [ ] Health check endpoints funcionando

### Gate 4 - Operacional
- [ ] Pipeline CI/CD validado end-to-end
- [ ] Ambientes separados (dev/staging/prod)
- [ ] Variavies de ambiente configuradas
- [ ] Plano de rollback documentado e testado
- [ ] Backup de banco configurado e validado
- [ ] Monitoramento ativo configurado

### Gate 5 - Produto
- [ ] Frontend integrado com API
- [ ] Fluxo de autenticacao funcional
- [ ] UX validado (feedback visual, loading states, error handling)
- [ ] Documentacao minima da API
- [ ] README atualizado

---

## 12. Pontos Positivos (Manter e Fortalecer)

O projeto possui fundamentos solidos que devem ser preservados:

1. **Domain-Driven Design bem aplicado**: Entidades ricas com factory methods, Value Objects auto-validantes, excecoes de dominio especificas
2. **Clean Architecture (maioria)**: Separacao clara de camadas, repository pattern, use cases
3. **Validacao robusta**: Framework de validacao proprio com ValidationBuilder, ValidationResult fluente
4. **DevOps maduro**: 6 workflows GitHub Actions, Docker multi-stage, Husky hooks, deploy automatizado
5. **Code Quality tools**: CSharpier + dotnet format + StyleCop + Qodana + EditorConfig
6. **Error handling padronizado**: BaseEndPoint com mapeamento consistente de EResultStatus -> HTTP status
7. **Internacionalizacao**: Resources files com suporte a pt-BR e en-US (Domain layer)
8. **Persistencia multi-database**: Suporte a SQL Server, Cosmos DB, RavenDB (embora inconsistente)
9. **Test infrastructure**: xUnit + NSubstitute + FluentAssertions + TestExtensions customizadas
10. **Security headers**: OWASP headers implementados, rate limiting, JWT auth

---

## 13. Historico de Correcoes Executadas

### Batch 1 - Executado em 2026-02-16 (11 itens corrigidos)

| # | Correcao | Tipo | Arquivos Alterados |
|---|---------|------|-------------------|
| 1 | Remover HotChocolate do Core.csproj | Arquitetura | `OnForkHub.Core.csproj` |
| 2 | Mover GraphQL do Domain para CrossCutting | Arquitetura | 28+ arquivos (interfaces, classes, usings) |
| 3 | Mover IEndpointAsync para CrossCutting | Arquitetura | 23 arquivos (endpoints + extension) |
| 4 | Remover referencia Application -> Persistence | Arquitetura | `OnForkHub.Application.csproj`, `DependencyInjection.cs` |
| 5 | Remover V3 Use Cases duplicados | Debt | Removidos 2 arquivos |
| 6 | Fix AsNoTracking em CategoryRepositoryEF | Performance | `CategoryRepositoryEF.cs` |
| 7 | Otimizar NotificationRepositoryEF.MarkAllAsReadAsync | Performance | `NotificationRepositoryEF.cs` |
| 8 | Fix IdentityDataContext.cs.cs filename | Debt | Renomeado para `.cs` |
| 9 | Fix Title.Value setter para private set | Domain | `Title.cs` |
| 10 | Configurar CORS restritivo + Auth pipeline | Seguranca | `Program.cs` |
| 11 | Criar appsettings por ambiente + JWT config | Seguranca | `appsettings.json`, `.Development.json`, `.Production.json` |
| 12 | Adicionar dotnet test ao CI/CD | CI/CD | `build-and-deploy.yml` |
| 13 | Fix indentacao SA1137 em 4 endpoints | Code Quality | 4 endpoint files |
| 14 | Corrigir catalog "Persons" -> "OnForkHub" | Config | `appsettings.json` |

**Build validado com sucesso apos todas as correcoes.**

---

## 14. Proximos Passos Imediatos (Pendentes)

Acoes prioritarias restantes:

1. **Persistir Refresh Tokens no banco/Redis** (seguranca critica)
2. **Extrair UserId do JWT Claims nos Notification endpoints** (seguranca)
3. **Rate limiting especifico para auth endpoints** (seguranca)
4. **Criar testes para os 11 use cases faltantes** (qualidade)
5. **Criar testes de integracao para Auth/Video/Notification endpoints** (qualidade)
6. **Adicionar indices compostos no banco** (performance)
7. **Implementar EF Core retry policy** (resiliencia)
8. **Integrar frontend Blazor com API REST** (produto)

---

## 15. Estimativa de Esforco por Fase

| Fase | Sprints | Pessoas | Focus |
|------|---------|---------|-------|
| Fase 1 - Hardening | 2-3 | 2 devs | Backend + Seguranca |
| Fase 2 - Testes | 2-3 | 2 devs + 1 QA | Cobertura |
| Fase 3 - Frontend | 3-4 | 1-2 frontend devs | Blazor + UX |
| Fase 4 - Pre-Prod | 1-2 | Squad completo | Estabilizacao |
| Fase 5 - Go Live | 1 | Squad completo | Deploy |
| **TOTAL** | **9-13** | - | - |

---

> **Documento gerado por auditoria automatizada do codebase.**
> **Revisao humana recomendada para validacao de prioridades de negocio.**
