# 🎯 OnForkHub - Checklist Final para Produção

**Data de Criação:** 2026-04-06
**Baseado em:** ROADMAP.md, PROGRESS_LOG.md, roadmap_to_prod.md
**Branch Atual:** `feature/phase4-user-features`
**Status Geral:** ~65% completo para produção

---

## 📊 Resumo Executivo

| Área | Maturidade | Status |
|------|-----------|--------|
| **Backend API** | 8.0/10 | ✅ Bom |
| **Database** | 7.0/10 | ⚠️ Atenção |
| **Frontend** | 3.0/10 | 🔴 Crítico |
| **Segurança** | 6.5/10 | ⚠️ Atenção |
| **Testes** | 4.5/10 | 🔴 Crítico |
| **CI/CD** | 8.0/10 | ✅ Bom |
| **Documentação** | 8.5/10 | ✅ Excelente |

**Bloqueios Críticos para Produção:**
1. ❌ Frontend desconectado da API (demo hardcoded)
2. ❌ Refresh Tokens em memória (perdidos no restart)
3. ❌ Cobertura de testes ~40% (meta: 70%+)
4. ❌ Migrations do EF Core não existem
5. ❌ 11 Use Cases sem testes

---

## 🔴 PRIORIDADE 1 - SEGURANÇA CRÍTICA (Sprint 1.2)

*Estes itens bloqueiam produção diretamente por riscos de segurança.*

### 1.1 Persistir Refresh Tokens
- [ ] **CRÍTICO:** Migrar `ConcurrentDictionary` para banco de dados ou Redis
  - **Local:** `src/Shared/OnForkHub.CrossCutting/Authentication/JwtTokenService.cs` (linha 17)
  - **Problema:** Tokens são perdidos no restart do servidor e não escalam horizontalmente
  - **Solução:** Criar tabela `RefreshTokens` no banco com campos: Token, UserId, ExpiresAt, RevokedAt, CreatedByIp
  - **Critério de Aceite:** Tokens persistem após restart da API e funcionam em múltiplas instâncias

### 1.2 Proteger Notification Endpoints
- [ ] **ALTO:** Extrair UserId do JWT Claims em vez de query parameter
  - **Local:** Endpoints de notificação recebem userId como query param
  - **Vulnerabilidade:** Usuário pode acessar notificações de outros usuários
  - **Solução:** `var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);`
  - **Critério de Aceite:** Teste de integração falha quando userId não corresponde ao token

### 1.3 Rate Limiting para Auth
- [ ] **ALTO:** Implementar limite específico para Login/Register
  - **Problema:** Sem proteção contra brute force em endpoints de autenticação
  - **Solução:** Policy separada com 10 req/min por IP para endpoints `/auth/*`
  - **Critério de Aceite:** Após 10 tentativas em 1 minuto, retorna 429 Too Many Requests

### 1.4 Endurecer CSP
- [ ] **MÉDIO:** Remover `'unsafe-inline'` e `'unsafe-eval'` do Content-Security-Policy
  - **Local:** Security Headers middleware
  - **Risco:** XSS attacks
  - **Solução:** Usar nonces ou hashes para scripts inline
  - **Critério de Aceite:** Console do browser sem warnings de CSP, aplicação funciona normalmente

---

## 🔴 PRIORIDADE 2 - BANCO DE DADOS (Sprint 1.3)

*Sem migrations e índices, o banco pode corromper ou ter performance ruim.*

### 2.1 Criar Migrations
- [x] **CRÍTICO:** Gerar migration inicial
  - ✅ Adicionado `Microsoft.EntityFrameworkCore.Design` ao projeto API
  - ✅ Criado `EntityFrameworkDataContextFactory` para design-time
  - ⚠️ Migration pendente de geração (EF CLI travando durante build)
  - **Ação Necessária:** Executar manualmente: `cd src/Presentations/OnForkHub.Api && dotnet ef migrations add InitialCreate -o Migrations`
  - **Local:** `src/Presentations/OnForkHub.Api/Migrations/`
  - **Entidades incluídas:** User, Video, Category, Notification, RefreshToken
  - **Critério de Aceite:** `dotnet ef database update` aplica schema completo em banco vazio

### 2.2 Índices Compostos
- [ ] **ALTO:** Criar índices compostos para queries frequentes
  - [ ] Notifications: `(UserId, Status, CreatedAt)`
  - [ ] Videos: `(UserId, Status, CreatedAt)`
  - [ ] Videos: `(CategoryId, Status, CreatedAt)` para listagens filtradas
  - **Local:** Arquivos de configuração em `src/Infrastructure/OnForkHub.Persistence/Configurations/`
  - **Critério de Aceite:** Execution plans mostram Index Seek em vez de Table Scan

### 2.3 EF Core Retry Policy
- [ ] **MÉDIO:** Implementar retry policy para falhas transitórias
  ```csharp
  options.UseSqlServer(conn, sql => 
    sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(2), null));
  ```
  - **Critério de Aceite:** Testes de resiliência com falhas simuladas passam

### 2.4 Padronizar Multi-Database
- [ ] **MÉDIO:** Decidir estratégia para Cosmos/RavenDB
  - **Problema:** Apenas Category tem implementações alternativas; User, Video, Notification são EF only
  - **Opções:** 
    1. Implementar repositórios alternativos para todas entidades
    2. Remover Cosmos/RavenDB e focar apenas em EF Core + SQL Server
  - **Critério de Aceite:** Documentação clara sobre qual banco usar e porquê

---

## 🔴 PRIORIDADE 3 - TESTES (Sprint 2.1, 2.2, 2.3)

*40% de cobertura é insuficiente para produção. Meta: 70%+*

### 3.1 Testar Use Cases Faltantes (11 total)
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
- [ ] `LoginUserUseCase` (existente mas cobertura mínima)

**Local:** `test/OnForkHub.Application.Test/UseCases/`
**Critério de Aceite:** Cada use case com testes de sucesso, falha, e edge cases (null, IDs inexistentes)

### 3.2 Testes de Integração para API Endpoints
- [ ] **Auth endpoints:** register → login → refresh → revoke (fluxo completo)
- [ ] **Video endpoints:** CRUD completo (6 endpoints)
- [ ] **Notification endpoints:** Criar, listar, marcar como lida (8 endpoints)
- [ ] **Category endpoints:** CRUD completo com paginação

**Local:** `test/OnForkHub.Application.Test/Endpoints/` ou projeto separado
**Critério de Aceite:** Testes falham quando endpoints retornam status codes errados ou dados incorretos

### 3.3 Testes de Repositórios
- [ ] Usar InMemory DbContext para testes isolados
- [ ] Testar operações CRUD de cada repositório
- [ ] Testar queries complexas (paginação, filtros, includes)

**Local:** `test/OnForkHub.Persistence.Test/`
**Critério de Aceite:** Repositórios funcionam com banco em memória e retornam dados corretos

### 3.4 CI/CD Quality Gates
- [x] `dotnet test` adicionado ao pipeline (2026-02-16)
- [ ] **ALTO:** Configurar upload de cobertura para Codecov
- [ ] **ALTO:** Definir threshold mínimo de 70% (bloqueia merge se abaixo)
- [ ] **MÉDIO:** Tornar Qodana bloqueante (`continue-on-error: false`)
- [ ] **BAIXO:** Adicionar badge de cobertura ao README

**Local:** `.github/workflows/build-and-deploy.yml`
**Critério de Aceite:** PR é bloqueado automaticamente se cobertura < 70%

---

## 🔴 PRIORIDADE 4 - FRONTEND BLOZZER (Sprint 3.1, 3.2, 3.3, 3.4)

*O frontend atual é um demo hardcoded. Esta é a maior lacuna para produção.*

### 4.1 Infraestrutura Frontend
- [ ] **CRÍTICO:** Configurar HttpClient com base URL configurável
  - **Local:** `src/Presentations/OnForkHub.Web/Program.cs`
  - **Solução:** `builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new(builder.Configuration["ApiBaseUrl"]) });`
  - **Critério de Aceite:** Frontend consegue fazer GET para `/api/videos` e receber dados reais

- [ ] **CRÍTICO:** Implementar `AuthenticationStateProvider`
  - **Local:** Novo arquivo `src/Presentations/OnForkHub.Web/Auth/AuthStateProvider.cs`
  - **Funcionalidades:**
    - Armazenar JWT token em localStorage/sessionStorage
    - Expor estado de autenticação (autenticado/não autenticado)
    - Adicionar token automaticamente em requests (Authorization header)
  - **Critério de Aceite:** Após login, token é persistido e incluído em requests subsequentes

- [ ] **ALTO:** Criar service layer para chamadas de API
  - [ ] `IVideoApiService` (list, get, create, update, delete)
  - [ ] `ICategoryApiService` (list, get, create, update, delete)
  - [ ] `IAuthApiService` (login, register, refresh, logout)
  - [ ] `INotificationApiService` (list, mark as read, mark all as read)
  - [ ] `IUserProfileApiService` (get, update)
  - **Critério de Aceite:** Components não fazem chamadas HTTP diretas, usam services

- [ ] **ALTO:** Criar componentes base reutilizáveis
  - [ ] `DataTable.razor` (tabela com paginação, ordenação, filtros)
  - [ ] `Modal.razor` (dialog reutilizável)
  - [ ] `Toast.razor` (notificações de sucesso/erro)
  - [ ] `Pagination.razor` (controles de paginação)
  - [ ] `FormField.razor` (input com label, validação, erro)
  - [ ] `SearchBar.razor` (busca com debounce)
  - **Critério de Aceite:** Componentes documentados e usados em pelo menos 2 páginas cada

### 4.2 Páginas de Autenticação
- [ ] **CRÍTICO:** `Login.razor`
  - Formulário com email/senha
  - Validação client-side (DataAnnotations)
  - Exibir erros da API
  - Redirecionar após login sucesso
  - Link para "Esqueci minha senha"
  - **Critério de Aceite:** Login funcional redireciona para Home com usuário autenticado

- [ ] **CRÍTICO:** `Register.razor`
  - Formulário com nome, email, senha, confirmação de senha
  - Validação de força de senha
  - Validação de email único
  - Redirecionar para login após registro
  - **Critério de Aceite:** Registro cria usuário no banco e permite login

- [ ] **ALTO:** Protected Routes
  - Configurar `<AuthorizeRouteView>` no `App.razor`
  - Redirect automático para `/login` quando não autenticado
  - Definir quais páginas requerem autenticação
  - **Critério de Aceite:** Usuário não autenticado é redirecionado para login ao acessar páginas protegidas

### 4.3 Páginas de CRUD
- [ ] **CRÍTICO:** `VideoList.razor`
  - Listar vídeos com paginação
  - Busca por título/descrição
  - Filtros por categoria, status, data
  - Ordenação (mais recente, mais visto, etc.)
  - Cards de vídeo com thumbnail, título, duração, views
  - Empty state quando não há vídeos
  - **Critério de Aceite:** Lista atualizada ao criar/deletar vídeos, paginação funcional

- [ ] **CRÍTICO:** `VideoDetail.razor`
  - Player de vídeo funcional (Plyr já integrado)
  - Metadados do vídeo (título, descrição, categoria, data, views)
  - Botões de compartilhar, favoritar (se autenticado)
  - Seção de comentários (se implementado)
  - **Critério de Aceite:** Vídeo carrega e reproduz corretamente

- [ ] **CRÍTICO:** `VideoUpload.razor`
  - Formulário com título, descrição, categoria, thumbnail
  - Upload de arquivo de vídeo com drag-and-drop
  - Barra de progresso de upload
  - Validação de tamanho e formato (máx 2 min, mp4/webm)
  - Preview após upload
  - **Critério de Aceite:** Vídeo é enviado, processado e aparece na lista

- [ ] **ALTO:** `CategoryList.razor`
  - CRUD básico de categorias (apenas admin)
  - Listagem com nome, descrição, quantidade de vídeos
  - Formulário de criação/edição em modal
  - Confirmação antes de deletar
  - **Critério de Aceite:** Categorias persistidas e refletidas na lista

- [ ] **ALTO:** `UserProfile.razor`
  - Visualizar perfil (nome, email, data de cadastro)
  - Editar perfil (nome, foto de perfil)
  - Trocar senha (senha atual, nova senha, confirmação)
  - Histórico de vídeos do usuário
  - **Critério de Aceite:** Alterações salvas refletem no backend

### 4.4 UX Polish
- [ ] **ALTO:** `NotificationCenter.razor`
  - Ícone no menu com badge de contagem
  - Dropdown com últimas notificações
  - Marcar individualmente ou todas como lidas
  - Redirecionar para conteúdo da notificação ao clicar
  - **Critério de Aceite:** Badge atualiza em tempo real (ou ao refresh)

- [ ] **ALTO:** Loading States
  - Skeleton loaders para listas
  - Spinner para formulários em submit
  - Botões desabilitados durante loading
  - **Critério de Aceite:** Nenhuma chamada de API sem feedback visual

- [ ] **ALTO:** Empty States
  - Mensagem amigável quando listas estão vazias
  - Call-to-action (ex: "Nenhum vídeo encontrado. Seja o primeiro a fazer upload!")
  - Ilustrações ou ícones visuais
  - **Critério de Aceite:** Todas as listas têm empty state customizado

- [ ] **MÉDIO:** Validação Client-Side
  - DataAnnotations em todos os formulários
  - Mensagens de erro em tempo real
  - Destaque visual de campos inválidos
  - **Critério de Aceite:** Formulários não submetem dados inválidos

- [ ] **MÉDIO:** Responsividade Mobile
  - Testar todas as páginas em mobile (320px, 375px, 425px)
  - Menu colapsável (hamburger)
  - Tabelas adaptadas para mobile (cards ou scroll horizontal)
  - **Critério de Aceite:** Lighthouse mobile score > 80

- [ ] **BAIXO:** Internacionalização (pt-BR / en-US)
  - Usar `IStringLocalizer` para textos
  - Arquivos `.resx` para cada idioma
  - Seletor de idioma no menu
  - **Critério de Aceite:** Troca de idioma funciona sem reload

- [ ] **BAIXO:** Dark/Light Mode Toggle
  - CSS variables para temas
  - Toggle no menu
  - Persistir preferência em localStorage
  - **Critério de Aceite:** Troca de tema instantânea e persistente

---

## 🟡 PRIORIDADE 5 - DÉBITO TÉCNICO (Sprint 1.1 continuado)

*Melhorias de qualidade de código que facilitam manutenção.*

### 5.1 Padronização de Nomenclatura
- [ ] **MÉDIO:** Renomear interfaces de repositório (remover sufixo `EF`)
  - `IUserRepositoryEF` → `IUserRepository`
  - `IVideoRepositoryEF` → `IVideoRepository`
  - `ICategoryRepositoryEF` → `ICategoryRepository`
  - `INotificationRepositoryEF` → `INotificationRepository`
  - **Critério de Aceite:** Build passa, todos os testes passam

- [ ] **BAIXO:** Unificar interfaces de validação
  - Escolher entre `IValidationService` e `IEntityValidator` (não ambos)
  - **Critério de Aceite:** Um único serviço de validação usado consistentemente

### 5.2 Código Morto
- [ ] **MÉDIO:** Decidir sobre multi-database
  - `CategoryRepositoryCosmos` e `CategoryRepositoryRavenDB` estão implementados mas não usados em produção
  - **Opções:** 
    1. Usar para todos os repositórios (decidir qual banco é primário)
    2. Remover e focar apenas em EF Core + SQL Server
  - **Critério de Aceite:** Documentação clara e código removido ou integrado

- [ ] **BAIXO:** Avaliar `BaseService` wrapper
  - Try-catch genérico pode ser substituído por middleware global
  - **Critério de Aceite:** Exception handling centralizado, código duplicado removido

### 5.3 Padronização de Respostas API
- [ ] **MÉDIO:** Todos endpoints devem usar `BaseEndPoint.HandleUseCase`
  - **Problema:** Notification endpoints usam try-catch manual
  - **Solução:** Refatorar para usar padrão consistente
  - **Critério de Aceite:** Zero try-catch blocks em endpoints, todos usam HandleUseCase

### 5.4 GraphQL Cleanup
- [ ] **BAIXO:** Simplificar GraphQL Handlers
  - Atualmente são pass-through (apenas delegam para Use Cases)
  - **Solução:** Remover handlers e chamar Use Cases diretamente nos resolvers
  - **Critério de Aceite:** GraphQL funciona com menos código

---

## 🟢 PRIORIDADE 6 - FUNCIONALIDADES FUTURAS (Phase 4+)

*Features desejáveis mas não bloqueiam produção.*

### 6.1 User Features (Phase 4)
- [ ] User profile management (avatar, bio, links sociais)
- [ ] Video upload com progress tracking e transcoding
- [ ] Busca avançada com filtros múltiplos
- [ ] Sistema de notificações push (WebSocket/SignalR)
- [ ] Favoritos/Bookmarks
- [ ] Motor de recomendações (vídeos similares, trending)

### 6.2 WebTorrent (Phase 5)
- [ ] Geração automática de torrent para vídeos enviados
- [ ] Seeding/peer management
- [ ] Throttle de bandwidth
- [ ] UI de progresso P2P

### 6.3 Analytics (Phase 6)
- [ ] View count tracking
- [ ] User engagement metrics
- [ ] Performance monitoring dashboard
- [ ] Application Insights integration

### 6.4 Mobile (Phase 7)
- [ ] PWA enhancements (offline, install prompt)
- [ ] Mobile app (React Native/Flutter)
- [ ] Push notifications

---

## 📋 CHECKLIST DE PRÉ-PRODUÇÃO

*Execute esta checklist antes do primeiro deploy em produção.*

### Segurança
- [ ] Refresh Tokens persistidos em banco/Redis
- [ ] JWT Secret Key com pelo menos 256 bits (52 caracteres base64)
- [ ] Rate limiting ativo para endpoints de auth
- [ ] CSP sem `'unsafe-inline'` e `'unsafe-eval'`
- [ ] CORS configurado com origins explícitos (não `*`)
- [ ] HTTPS forçado (HSTS header presente)
- [ ] Dependencies atualizadas (sem vulnerabilities conhecidas)
- [ ] Scan de segurança passa (dotnet audit, Snyk, ou similar)

### Banco de Dados
- [ ] Migrations versionadas e aplicadas
- [ ] Índices compostos criados
- [ ] Retry policy configurada
- [ ] Plano de backup automatizado
- [ ] Plano de rollback testado
- [ ] Seed data para desenvolvimento

### Testes
- [ ] Cobertura de testes ≥ 70%
- [ ] Todos os testes passam no CI/CD
- [ ] Testes de integração para endpoints críticos
- [ ] Testes de carga básicos passam (100 usuários simultâneos)

### Frontend
- [ ] HttpClient configurado com base URL
- [ ] AuthenticationStateProvider implementado
- [ ] Login/Register funcionais
- [ ] CRUD de vídeos funcional
- [ ] Loading states em todas as chamadas
- [ ] Error handling visual (Toast/Snackbar)
- [ ] Responsividade testada (mobile, tablet, desktop)

### DevOps
- [ ] Ambiente de homologação separado
- [ ] Monitoramento ativo (Application Insights/Grafana)
- [ ] Alertas configurados para erros críticos
- [ ] Rollback automatizado funcional
- [ ] Health checks respondendo
- [ ] SSL válido (não expired)

### Documentação
- [ ] README atualizado com setup instructions
- [ ] API_REFERENCE.md cobrindo todos os endpoints
- [ ] Troubleshooting guide para erros comuns
- [ ] Architecture decision records (ADRs) para decisões importantes
- [ ] CHANGELOG.md com histórico de versões

---

## 🎯 PLANO DE AÇÃO RECOMENDADO

### Semana 1-2: Segurança Crítica
**Foco:** Resolver itens que bloqueiam produção por risco de segurança.
- [ ] Persistir Refresh Tokens (2-3 dias)
- [ ] Proteger Notification Endpoints (1 dia)
- [ ] Rate limiting para auth (1 dia)
- [ ] Testes de integração para auth (2 dias)

**Entregável:** API segura para deploy com tokens persistentes.

### Semana 3-4: Database & Testes
**Foco:** Criar migrations, índices, e elevar cobertura de testes.
- [ ] Gerar migration inicial (1 dia)
- [ ] Criar índices compostos (1 dia)
- [ ] Testar 11 Use Cases faltantes (3-4 dias)
- [ ] Testes de integração para API (3-4 dias)

**Entregável:** Banco versionado e cobertura de testes ≥ 60%.

### Semana 5-8: Frontend (MVP)
**Foco:** Transformar frontend de demo em aplicação funcional.
- [ ] Infraestrutura (HttpClient, AuthStateProvider, Services) (3-4 dias)
- [ ] Login/Register pages (2-3 dias)
- [ ] VideoList/VideoDetail pages (3-4 dias)
- [ ] VideoUpload page (2-3 dias)
- [ ] Loading states, error handling, empty states (2-3 dias)

**Entregável:** Frontend funcional com autenticação e CRUD de vídeos.

### Semana 9-10: UX Polish & Débito Técnico
**Foco:** Melhorar qualidade de código e experiência do usuário.
- [ ] Componentes base reutilizáveis (2-3 dias)
- [ ] Notification center (1-2 dias)
- [ ] User profile page (2 dias)
- [ ] Padronizar nomenclatura de repositórios (1 dia)
- [ ] Remover código morto (1 dia)

**Entregável:** Aplicação polida e pronta para usuários beta.

### Semana 11-12: Pré-Produção
**Foco:** Checklist final e deploy em produção.
- [ ] Executar checklist de pré-produção (2-3 dias)
- [ ] Testes de carga e performance (2 dias)
- [ ] Ambiente de homologação (1 dia)
- [ ] Deploy em produção (1 dia)
- [ ] Monitoramento pós-deploy (2-3 dias)

**Entregável:** **PRODUÇÃO LIVE!** 🎉

---

## 📊 MÉTRICAS DE PROGRESSO

### Status Atual
| Métrica | Valor | Meta |
|---------|-------|------|
| Testes unitários passando | 372 | 500+ |
| Cobertura de testes | ~40% | 70%+ |
| Use Cases testados | 5/16 | 16/16 |
| Endpoints testados | 5/22+ | 22+/22+ |
| Páginas frontend funcionais | 2/12 | 12/12 |
| Violaciones StyleCop | 0 | 0 ✅ |
| Build errors | 0 | 0 ✅ |

### Como Atualizar
1. Execute testes: `dotnet test --collect:"XPlat Code Coverage"`
2. Verifique cobertura: relatórios em `TestResults/`
3. Atualize esta seção com novos números
4. Commit: `docs(checklist): update metrics`

---

## 🔗 LINKS ÚTEIS

| Recurso | Local |
|---------|-------|
| ROADMAP completo | `ROADMAP.md` |
| Progress Log | `PROGRESS_LOG.md` |
| Auditoria detalhada | `roadmap_to_prod.md` |
| API Reference | `docs/API_REFERENCE.md` |
| Architecture | `docs/ARCHITECTURE.md` |
| Troubleshooting | `docs/TROUBLESHOOTING.md` |
| Contributing Guide | `CONTRIBUTING.md` |
| Branch atual | `feature/phase4-user-features` |

---

## 📝 NOTAS

- **Critérios de Aceite:** Cada item tem critérios objetivos. Marque como feito apenas quando todos forem atendidos.
- **Prioridades:**
  - 🔴 **CRÍTICO:** Bloqueia produção diretamente
  - 🟡 **ALTO:** Importante mas não bloqueia deploy imediato
  - 🟢 **MÉDIO/BAIXO:** Melhorias de qualidade ou features desejáveis
- **Estimativas:** Baseadas em complexidade relativa. Ajuste conforme velocidade da equipe.
- **Revisão:** Atualize este documento a cada sprint completada.

---

**Próximo Passo Recomendado:** Comece pela **Prioridade 1 (Segurança Crítica)** → Persistir Refresh Tokens.

**Comando para iniciar:**
```bash
git checkout feature/phase4-user-features
code "src/Shared/OnForkHub.CrossCutting/Authentication/JwtTokenService.cs"
```

Boa sorte! 🚀
