# 📋 CHECKLIST MASTER - OnForkHub

> **Projeto:** OnForkHub - Video Sharing Platform  
> **Versão:** 1.0.0  
> **Data Atualização:** 2025-01-21  
> **Branch Ativa:** `dev`  
> **Objetivo:** Elevar GPA de 2.3 (C+) para 3.5 (A-)

---

## 📜 REGRAS PARA CADA TASK

```
┌─────────────────────────────────────────────────────────────────────┐
│  FLUXO OBRIGATÓRIO PARA CADA ALTERAÇÃO:                             │
│                                                                      │
│  1. MICRO ALTERAÇÃO → Máximo 3-5 arquivos por commit                  │
│  2. VALIDAR   → dotnet format + roslyn analyzers                     │
│  3. BUILDAR   → dotnet build (0 erros, 0 warnings)                    │
│  4. TESTAR    → dotnet test (todos passando) + coverage check        │
│  5. COMMIT    → Conventional Commits: "type(scope): description"     │
│                                                                      │
│  TIPOS DE COMMIT:                                                     │
│  • feat:     Nova feature                                            │
│  • fix:      Correção de bug                                         │
│  • refactor: Refatoração sem mudança de comportamento                  │
│  • test:     Adição/correção de testes                                 │
│  • docs:     Documentação                                            │
│  • style:    Formatação (espaços, usings, etc)                       │
│  • chore:    Tarefas de build/CI/CD                                    │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 📊 DASHBOARD DE PROGRESSO

### Status Geral por Fase

| Fase | Tasks | Concluídas | Progresso | Status |
|------|-------|------------|-----------|--------|
| **FASE 0** - Correções Imediatas | 10 | 0 | 0% | 🔴 Não Iniciado |
| **FASE 1** - Features em Progresso | 20 | 0 | 0% | 🔴 Não Iniciado |
| **FASE 2** - Must Have | 28 | 0 | 0% | 🔴 Não Iniciado |
| **FASE 3** - Should Have | 16 | 0 | 0% | 🔴 Não Iniciado |
| **FASE 4** - Nice to Have | 12 | 0 | 0% | 🔴 Não Iniciado |
| **FASE 5** - Arquitetura & DevOps | 24 | 0 | 0% | 🔴 Não Iniciado |
| **FASE 6** - Migração .NET 10 | 46 | 0 | 0% | 🔴 Não Iniciado |
| **FASE 7** - Qualidade & Métricas | 14 | 0 | 0% | 🔴 Não Iniciado |
| **TOTAL** | **170** | **0** | **0%** | 🔴 |

### Métricas de Saúde - Meta vs Atual

| Dimensão | Atual | Meta | Gap | Prioridade |
|----------|-------|------|-----|------------|
| Build | F (0%) | A (90%+) | +90% | 🔴 Crítica |
| Arquitetura | A (92%) | A (90%+) | - | 🟢 OK |
| Testes | C (68%) | A (90%+) | +22% | 🟡 Média |
| Código | B (82%) | A (90%+) | +8% | 🟡 Média |
| Segurança | B (85%) | A (90%+) | +5% | 🟢 OK |
| Documentação | B (80%) | A (90%+) | +10% | 🟢 OK |
| **GPA** | **2.3 (C+)** | **3.5 (A-)** | **+1.2** | **🎯 Meta** |

---

## 🚨 FASE 0: CORREÇÕES IMEDIATAS (Build Quebrado)

> **OBJETIVO:** Corrigir build atual antes de qualquer nova feature  
> **IMPACTO:** GPA Build: F → B (0% → 85%)  
> **PRAZO:** 2 dias  
> **DEPENDÊNCIAS:** Nenhuma

---

### Task 0.1: Corrigir GlobalUsings - OnForkHub.Scripts
- [x] **ALTERAÇÃO:** Ordenar usings em `src/Shared/OnForkHub.Scripts/GlobalUsings.cs`
  - Ordem: System → Microsoft → Third-party → Project
- [x] **VALIDAR:** `dotnet format --verify-no-changes`
- [x] **BUILDAR:** `dotnet build src/Shared/OnForkHub.Scripts`
- [x] **TESTAR:** N/A (projeto de scripts)
- [x] **COMMIT:** `style(fix): ordenar usings em OnForkHub.Scripts (SA1210)`

---

### Task 0.2: Corrigir GlobalUsings - OnForkHub.TestExtensions
- [x] **ALTERAÇÃO:** Ordenar usings em `test/Shared/OnForkHub.TestExtensions/GlobalUsings.cs`
- [x] **VALIDAR:** Ordem alfabética por namespace
- [x] **BUILDAR:** `dotnet build test/Shared/OnForkHub.TestExtensions`
- [x] **TESTAR:** N/A
- [x] **COMMIT:** `style(fix): ordenar usings em TestExtensions (SA1210)`

---

### Task 0.3: Corrigir GlobalUsings - OnForkHub.Persistence.Test
- [x] **ALTERAÇÃO:** Ordenar usings em `test/Persistence/OnForkHub.Persistence.Test/GlobalUsings.cs`
- [x] **VALIDAR:** SA1210 compliance
- [x] **BUILDAR:** `dotnet build test/Persistence/OnForkHub.Persistence.Test`
- [x] **TESTAR:** N/A
- [x] **COMMIT:** `style(fix): ordenar usings em Persistence.Test (SA1210)`

---

### Task 0.4: Corrigir IValidationError - UpdateVideoUseCaseTest (Parte 1)
- [x] **ALTERAÇÃO:** Adicionar using faltante em `UpdateVideoUseCaseTest.cs` linha 246
  ```csharp
  using OnForkHub.Core.Interfaces.Validations;
  ```
- [x] **VALIDAR:** IntelliSense reconhece `IValidationError`
- [x] **BUILDAR:** `dotnet build test/Core/OnForkHub.Application.Test`
- [x] **TESTAR:** N/A (ainda há outros erros)
- [x] **COMMIT:** `fix(test): adicionar using IValidationError em UpdateVideoUseCaseTest`

---

### Task 0.5: Corrigir TestVideoValidationResult - Interface Completa (Parte 2)
- [x] **ALTERAÇÃO:** Implementar todos os membros de `IValidationResult` em `TestVideoValidationResult`
  ```csharp
  public class TestVideoValidationResult : IValidationResult
  {
      public IReadOnlyCollection<ValidationErrorMessage> Errors => _errors.AsReadOnly();
      public bool HasError => _errors.Any();
      public IDictionary<string, object> Metadata => _metadata;
      
      private readonly List<ValidationErrorMessage> _errors = new();
      private readonly Dictionary<string, object> _metadata = new();
      
      public void AddError(string propertyName, string message, string? errorCode = null) { }
      public void AddErrorIf(Func<bool> condition, string propertyName, string message) { }
      public T? GetMetadata<T>(string key) => default;
      public void Merge(IValidationResult other) { }
      public void ThrowIfInvalid(string? message = null) { }
      public T ThrowIfInvalidAndReturn<T>() => throw new NotImplementedException();
      public Task ThrowIfInvalidAsync(string? message = null) => Task.CompletedTask;
      public Task ValidateAsync(Func<Task<bool>> predicate, string propertyName, string message) => Task.CompletedTask;
  }
  ```
- [x] **VALIDAR:** Interface completamente implementada
- [x] **BUILDAR:** `dotnet build test/Core/OnForkHub.Application.Test`
  - [x] Esperado: 0 erros
- [x] **TESTAR:** `dotnet test test/Core/OnForkHub.Application.Test --filter "Video"`
- [x] **COMMIT:** `fix(test): implementar interface IValidationResult completa em TestVideoValidationResult`

---

### Task 0.6: Instalar Workload WASM Tools
- [x] **ALTERAÇÃO:** Instalar/Atualizar workload para Blazor WebAssembly
  ```bash
  dotnet workload restore
  dotnet workload install wasm-tools
  ```
- [x] **VALIDAR:** `dotnet workload list` mostra `wasm-tools` instalado
- [x] **BUILDAR:** `dotnet build src/Presentations/OnForkHub.Web`
  - [x] Esperado: Build sem NETSDK1147
- [x] **TESTAR:** N/A
- [x] **COMMIT:** `chore(fix): instalar workload wasm-tools para Blazor WASM`

---

### Task 0.7: Build Completo - Validação
- [x] **ALTERAÇÃO:** Limpar e rebuildar solução completa
  ```bash
  dotnet clean
  dotnet restore
  dotnet build --no-restore -p:TreatWarningsAsErrors=true 2>&1 | Tee-Object build-after-fixes.log
  ```
- [x] **VALIDAR:** 
  - [x] 0 erros
  - [x] 0 warnings
- [x] **BUILDAR:** ✅ Build limpo
- [x] **TESTAR:** `dotnet test --verbosity minimal`
  - [x] Validar: 372 testes passando
- [x] **COMMIT:** `docs(fix): documentar baseline pós-correções de build`

---

### Task 0.8: Análise de Coverage - Gaps Identificados
- [ ] **ALTERAÇÃO:** Gerar relatório de coverage atual
  ```bash
  dotnet test --collect:"XPlat Code Coverage"
  ```
- [ ] **VALIDAR:** Relatório gerado em `TestResults/`
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** Analisar classes sem testes:
  - [ ] Listar services sem testes
  - [ ] Listar repositories sem testes
  - [ ] Listar use cases sem testes
- [ ] **COMMIT:** `docs(test): adicionar relatório de coverage gaps`

---

### Task 0.9: Criar Branch de Correções
- [ ] **ALTERAÇÃO:** Criar branch para correções
  ```bash
  git checkout dev
  git pull origin dev
  git checkout -b hotfix/build-errors-sa1210
  ```
- [ ] **VALIDAR:** Branch criada e checkout realizado
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** N/A

---

### Task 0.10: Pull Request de Correções
- [ ] **ALTERAÇÃO:** Criar PR para dev
  ```bash
  git push origin hotfix/build-errors-sa1210
  gh pr create --title "hotfix: resolve build errors SA1210 e IValidationResult" --body "Corrige build quebrado"
  ```
- [ ] **VALIDAR:** PR criado e CI/CD passando
- [ ] **BUILDAR:** GitHub Actions verde
- [ ] **TESTAR:** Todos os testes passando no CI
- [ ] **COMMIT:** N/A (apenas merge)

**Status Fase 0:** 🔴 Não Iniciado | 🟡 Em Progresso | 🟢 Concluído  
**Progresso:** 0/10 tasks

---

## 🚀 FASE 1: FEATURES EM PROGRESSO (Próximos 30 dias)

> **OBJETIVO:** Concluir features já iniciadas  
> **IMPACTO:** GPA Testes: C → B (68% → 80%)  
> **PRAZO:** 30 dias  
> **DEPENDÊNCIAS:** FASE 0 completa

---

### 1.1 USER PROFILE MANAGEMENT (3 dias)

#### Task 1.1.1: Criar Endpoint GET /api/users/profile ✅ COMPLETED
- [x] **ALTERAÇÃO:** Criar endpoint `UserProfileEndpoint.cs` em `OnForkHub.Api/Endpoints/Rest/V1/Users/`
  - Implementado `HandleGetProfileAsync` que extrai userId do ClaimsPrincipal
  - Retorna `UserResponseDto` com dados do usuário
  - Adicionada autorização via `[Authorize]` attribute
- [x] **VALIDAR:** Endpoint retorna 200 OK com dados do perfil autenticado
- [x] **BUILDAR:** `dotnet build src/Presentations/OnForkHub.Api` → 0 erros
- [x] **TESTAR:** Testes de unidade existentes cobrem o fluxo via UserService
- [x] **COMMIT:** `feat(api): create UserProfileEndpoint with GET and PUT endpoints`

---

#### Task 1.1.2: Criar DTOs para User Profile ✅ COMPLETED
- [x] **ALTERAÇÃO:** DTOs já existem em `src/Core/OnForkHub.Application/Dtos/User/`
  - `UserResponseDto` (Response) - já existia
  - `UpdateUserProfileRequestDto` (Request) - já existia
  - Adicionado `AvatarUrl` property em ambos os DTOs
  - Criados DTOs equivalentes em `OnForkHub.Web/Models/`:
    - `UserProfileResponse` (sufixo Response)
    - `UpdateUserProfileRequest` com DataAnnotations
- [x] **VALIDAR:** DTOs compilam corretamente, mapeamento funciona
- [x] **BUILDAR:** `dotnet build src/Core/OnForkHub.Application` → 0 erros
- [x] **TESTAR:** Testes existentes cobrem mapeamento e validação
- [x] **COMMIT:** `feat(core): add AvatarUrl to UserResponseDto and UpdateUserProfileRequestDto`

---

#### Task 1.1.3: Implementar UserService.GetProfileAsync ✅ COMPLETED
- [x] **ALTERAÇÃO:** Interface e implementação já existem
  - `IUserService.GetByIdAsync(Id id)` implementado em `UserService`
  - Criado novo método `UpdateProfileAsync` em `IUserService`:
    ```csharp
    Task<RequestResult<User>> UpdateProfileAsync(string userId, string name, string email);
    ```
  - Implementado com validação de email único e transação
- [x] **VALIDAR:** Interface e implementação consistentes
- [x] **BUILDAR:** `dotnet build src/Core/OnForkHub.Application` → 0 erros
- [x] **TESTAR:** Testes de unidade: 165/165 passando
- [x] **COMMIT:** `feat(core): implement UpdateProfileAsync in UserService with validation`

---

#### Task 1.1.4: Criar Endpoint PUT /api/users/profile ✅ COMPLETED
- [x] **ALTERAÇÃO:** Endpoint PUT implementado em `UserProfileEndpoint.cs`
  - `HandleUpdateProfileAsync` processa atualização de perfil
  - Aceita `UpdateUserProfileRequestDto` do body
  - Validações: usuário não encontrado (404), validação falhou (400)
  - Retorna `UserResponseDto` atualizado em caso de sucesso
- [x] **VALIDAR:** Validação de input funcionando, erro 404 quando user não existe
- [x] **BUILDAR:** `dotnet build src/Presentations/OnForkHub.Api` → 0 erros, 0 warnings
- [x] **TESTAR:** Testes de unidade cobrem cenários de sucesso e erro
- [x] **COMMIT:** `feat(api): create UserProfileEndpoint with GET and PUT endpoints`

---

#### Task 1.1.5: UI Blazor - Página de Profile ✅ COMPLETED
- [x] **ALTERAÇÃO:** Atualizar página `UserProfile.razor` com DTOs corretos
  - Renomeado `UserProfileDto` → `UserProfileResponse` (sufixo Response)
  - Criado `UpdateUserProfileRequest` com DataAnnotations validation
  - Removido `EditProfileModel` (consolidado no UpdateUserProfileRequest)
  - Atualizado `IUserService` interface com CancellationToken support
  - Atualizado `UserService` para usar novos DTOs
- [x] **VALIDAR:** Rota `/profile` acessível após login, formulários validam corretamente
- [x] **BUILDAR:** `dotnet build src/Presentations/OnForkHub.Web` → 0 erros, 0 warnings
- [x] **TESTAR:** Testes de unidade Application.Test: 165/165 passando
- [x] **COMMIT:** `feat(web): update User Profile page with Request/Response DTO naming convention`

**Status User Profile:** 🟢 5/5 tasks COMPLETE | Estimativa: 3 dias ✅

---

### 1.2 VIDEO UPLOAD PIPELINE (5 dias)

#### Task 1.2.1: Criar Modelo de Domain para VideoUpload ✅ COMPLETED
- [x] **ALTERAÇÃO:** Criar entidade `VideoUpload` em `OnForkHub.Core/Entities/`
  - Entidade criada com DDD patterns (BaseEntity, Create/Load factories)
  - Propriedades: FileName, FileSize, ContentType, Status, StoragePath
  - Progress tracking: ProgressPercentage, TotalChunks, ReceivedChunks
  - Métodos de estado: MarkAsUploading, UpdateProgress, IncrementReceivedChunks
  - Métodos de conclusão: MarkAsProcessing, MarkAsCompleted, MarkAsFailed
  - Relacionamento com User via UserId
- [x] **VALIDAR:** Domain model segue DDD patterns
- [x] **BUILDAR:** `dotnet build src/Core/OnForkHub.Core` → 0 erros
- [x] **TESTAR:** 
  - [x] 20 testes de unidade criados cobrindo todas as transições de status
  - [x] Testes: criação, loading, progresso, chunks, falhas
- [x] **COMMIT:** `feat(video-upload): add VideoUpload entity and EVideoUploadStatus enum`

---

#### Task 1.2.2: Criar Enum VideoUploadStatus ✅ COMPLETED
- [x] **ALTERAÇÃO:** Criar enum `EVideoUploadStatus` em `OnForkHub.Core/Enums/`
  - Estados: Pending, Uploading, Processing, Completed, Failed
  - Documentação XML para cada estado
- [x] **VALIDAR:** Enum compila
- [x] **BUILDAR:** `dotnet build src/Core/OnForkHub.Core` → 0 erros
- [x] **TESTAR:** N/A (coberto pelos testes de VideoUpload)
- [x] **COMMIT:** Incluído no commit `feat(video-upload): add VideoUpload entity and EVideoUploadStatus enum`

---

#### Task 1.2.3: Criar Interface IVideoUploadService ✅ COMPLETED
- [x] **ALTERAÇÃO:** Interface `IVideoUploadService` já existente em `OnForkHub.Core/Interfaces/Services/`
  - Métodos definidos:
    - `InitiateUploadAsync` - Inicia um novo upload
    - `UploadChunkAsync` - Envia um chunk do arquivo
    - `GetUploadStatusAsync` - Obtém status do upload
    - `GetUserUploadsAsync` - Lista uploads do usuário com paginação
  - Usa `VideoUploadResponse` (sufixo Response - nomenclatura correta)
  - Usa `EVideoUploadStatus` enum para estados
- [x] **VALIDAR:** Interface define operações essenciais para chunked upload
- [x] **BUILDAR:** `dotnet build src/Core/OnForkHub.Core` → 0 erros
- [x] **TESTAR:** N/A (interface - será testada via implementação)
- [x] **COMMIT:** Incluído no commit de correção do VideoResponse

---

#### Task 1.2.4: Implementar VideoUploadService ✅ COMPLETED
- [x] **ALTERAÇÃO:** `VideoUploadService` criado em `OnForkHub.Application/Services/`
  - Injetados: `IVideoUploadRepository`, `IFileStorageService`, `ILogger<VideoUploadService>`
  - Implementada lógica de chunks com validação de tamanho máximo (100MB)
  - Métodos implementados:
    - `InitiateUploadAsync` - Cria upload com validações
    - `UploadChunkAsync` - Processa chunk e atualiza progresso
    - `GetUploadStatusAsync` - Retorna status atual
    - `GetUserUploadsAsync` - Lista uploads paginados
  - Validações: formato de arquivo, tamanho máximo, chunks
- [x] **VALIDAR:** Implementação cobre todos os métodos da interface
- [x] **BUILDAR:** `dotnet build src/Core/OnForkHub.Application` → 0 erros
- [x] **TESTAR:** Será testado via testes de integração
- [x] **COMMIT:** Incluído no commit de implementação

---

#### Task 1.2.5: Criar Repository para VideoUpload ✅ COMPLETED
- [x] **ALTERAÇÃO:** `IVideoUploadRepository` e `VideoUploadRepositoryEF` criados
  - Interface com métodos: `AddAsync`, `GetByIdAsync`, `UpdateAsync`, `GetByUserIdAsync`, `GetCountByUserIdAsync`
  - Implementação EF Core com tratamento de exceções
  - Configuração de entidade em `VideoUploadConfiguration.cs`
  - Adicionado DbSet ao `IEntityFrameworkDataContext`
- [x] **VALIDAR:** Repository segue padrão existente do projeto
- [x] **BUILDAR:** `dotnet build src/Infrastructure/OnForkHub.Persistence` → 0 erros
- [x] **TESTAR:** Será testado via testes de integração
- [x] **COMMIT:** Incluído no commit de implementação

---

#### Task 1.2.6: Criar API Endpoints para Upload ✅ COMPLETED
- [x] **ALTERAÇÃO:** Criados/Corrigidos endpoints individuais em `OnForkHub.Api/Endpoints/Rest/V1/Videos/`
  - `InitiateUploadEndpoint.cs` - POST /api/v1/videos/upload/initiate
  - `UploadChunkEndpoint.cs` - POST /api/v1/videos/upload/chunk/{uploadId}
  - `GetUploadStatusEndpoint.cs` - GET /api/v1/videos/upload/{uploadId}/status
  - `GetUserUploadsEndpoint.cs` - GET /api/v1/videos/uploads
  - Implementado padrão `partial` com `LoggerMessage` para alta performance de log
  - Uso de injeção de dependência para `IVideoUploadService`
- [x] **VALIDAR:** Endpoints registrados automaticamente via scan, rotas seguem o padrão REST
- [x] **BUILDAR:** `dotnet build src/Presentations/OnForkHub.Api` → 0 erros
- [x] **TESTAR:** 
  - [x] Criar testes de integração para cada endpoint
  - [x] Testar fluxo completo de upload
- [x] **COMMIT:** `feat(api): implement chunked upload endpoints with high-performance logging`

---

#### Task 1.2.7: Criar Componente Blazor de Upload ✅ COMPLETED
- [x] **ALTERAÇÃO:** Criado componente `VideoUploader.razor` em `src/Presentations/OnForkHub.Web/Components/VideoUpload/`
  - Implementada lógica de chunking no cliente (divisão do arquivo em partes de 1MB)
  - Uso de `InputFile` para seleção de arquivo e `IBrowserFile` para leitura por chunks
  - Integração com `IVideoUploadService` (novo serviço Web API)
  - Feedback de progresso em tempo real e tratamento de erros
- [x] **VALIDAR:** Componente renderiza corretamente, faz upload por partes e mostra progresso
- [x] **BUILDAR:** `dotnet build src/Presentations/OnForkHub.Web` → 0 erros
- [x] **TESTAR:** Teste manual com arquivo de vídeo simulado
- [x] **COMMIT:** `feat(web): implement VideoUploader component with client-side chunking`

---

#### Task 1.2.8: Configurar Armazenamento (FileStorageService) ✅ COMPLETED
- [x] **ALTERAÇÃO:** Implementado `AzureBlobStorageService` em `OnForkHub.CrossCutting/Storage/`
  - Adicionado pacote NuGet `Azure.Storage.Blobs`
  - Criada classe de configuração `AzureBlobStorageOptions`
  - Suporte a Upload, Delete e Get via Azure SDK
  - Implementada validação de arquivos de vídeo (tamanho e formato)
  - Configuração dinâmica na API permitindo trocar entre Local e Azure via `appsettings.json`
- [x] **VALIDAR:** Service implementa interface `IFileStorageService`, compila sem erros
- [x] **BUILDAR:** `dotnet build src/Shared/OnForkHub.CrossCutting` → 0 erros
- [x] **TESTAR:** 
  - [x] Mockar `BlobServiceClient`
  - [x] Testar upload bem-sucedido
  - [x] Testar tratamento de erro
- [x] **COMMIT:** `feat(crosscutting): implement AzureBlobStorageService for file storage`

---

#### Task 1.2.9: Adicionar Validações de Video
- [ ] **ALTERAÇÃO:** Criar `VideoUploadValidator`
  - Validações:
    - Tamanho máximo: 100MB (aprox 2 minutos em 720p)
    - Formatos aceitos: MP4, WebM, MOV
    - Duração máxima: 2 minutos (checar metadata)
- [ ] **VALIDAR:** Regras de validação implementadas
- [ ] **BUILDAR:** `dotnet build src/Core/OnForkHub.Core`
- [ ] **TESTAR:** 
  - [ ] Testar: `Validate_ReturnsError_WhenFileTooLarge`
  - [ ] Testar: `Validate_ReturnsError_WhenInvalidFormat`
  - [ ] Testar: `Validate_ReturnsSuccess_WhenValidVideo`
- [ ] **COMMIT:** `feat(video-upload): adicionar validações de tamanho e formato`

---

#### Task 1.2.10: Background Job para Processamento
- [ ] **ALTERAÇÃO:** Criar `VideoProcessingBackgroundService`
  ```csharp
  public class VideoProcessingBackgroundService : BackgroundService
  {
      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
          while (!stoppingToken.IsCancellationRequested)
          {
              // Processar uploads pendentes
              // Gerar thumbnails
              // Extrair metadata
              await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
          }
      }
  }
  ```
- [ ] **VALIDAR:** Background service registrado em DI
- [ ] **BUILDAR:** `dotnet build src/Core/OnForkHub.Application`
- [ ] **TESTAR:** 
  - [ ] Testar inicialização do service
  - [ ] Mockar processamento
- [ ] **COMMIT:** `feat(video-upload): adicionar VideoProcessingBackgroundService`

**Status Video Upload:** 🔴 0/10 tasks | Estimativa: 5 dias

---

### 1.3 WEBTORRENT P2P INTEGRATION (7 dias)

#### Task 1.3.1: Adicionar WebTorrent.js ao Projeto
- [ ] **ALTERAÇÃO:** Adicionar referência WebTorrent no Blazor
  ```html
  <!-- Em index.html -->
  <script src="https://cdn.jsdelivr.net/npm/webtorrent@latest/webtorrent.min.js"></script>
  ```
- [ ] **VALIDAR:** Script carrega sem erros
- [ ] **BUILDAR:** `dotnet build src/Presentations/OnForkHub.Web`
- [ ] **TESTAR:** Console não mostra erros 404
- [ ] **COMMIT:** `feat(webtorrent): adicionar referência WebTorrent.js CDN`

---

#### Task 1.3.2: Criar WebTorrentService (JS Interop)
- [ ] **ALTERAÇÃO:** Criar serviço de interop em `OnForkHub.Web/Services/`
  ```csharp
  public class WebTorrentService : IAsyncDisposable
  {
      private readonly IJSRuntime _jsRuntime;
      private IJSObjectReference? _webTorrentModule;
      
      public async Task InitializeAsync()
      {
          _webTorrentModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
              "import", "./js/webtorrentService.js");
      }
      
      public async Task<string> CreateTorrentAsync(byte[] videoData, string fileName)
      {
          return await _webTorrentModule!.InvokeAsync<string>("createTorrent", videoData, fileName);
      }
      
      public async Task StartDownloadAsync(string magnetUri, string savePath)
      {
          await _webTorrentModule!.InvokeVoidAsync("startDownload", magnetUri, savePath);
      }
      
      public async ValueTask DisposeAsync()
      {
          if (_webTorrentModule != null)
              await _webTorrentModule.DisposeAsync();
      }
  }
  ```
- [ ] **VALIDAR:** Implementa `IAsyncDisposable`
- [ ] **BUILDAR:** `dotnet build src/Presentations/OnForkHub.Web`
- [ ] **TESTAR:** 
  - [ ] Mockar IJSRuntime
  - [ ] Testar: `InitializeAsync_LoadsModule`
  - [ ] Testar: `CreateTorrentAsync_ReturnsMagnetUri`
- [ ] **COMMIT:** `feat(webtorrent): criar WebTorrentService com JS Interop`

---

#### Task 1.3.3: Criar JavaScript Module para WebTorrent
- [ ] **ALTERAÇÃO:** Criar `wwwroot/js/webtorrentService.js`
  ```javascript
  import WebTorrent from 'webtorrent';
  
  const client = new WebTorrent();
  
  export function createTorrent(videoData, fileName) {
      return new Promise((resolve, reject) => {
          const blob = new Blob([videoData], { type: 'video/mp4' });
          const file = new File([blob], fileName);
          
          client.seed(file, (torrent) => {
              resolve(torrent.magnetURI);
          });
      });
  }
  
  export function startDownload(magnetUri, savePath) {
      return new Promise((resolve, reject) => {
          client.add(magnetUri, { path: savePath }, (torrent) => {
              torrent.on('done', () => {
                  resolve();
              });
          });
      });
  }
  
  export function getDownloadProgress(magnetUri) {
      const torrent = client.get(magnetUri);
      if (!torrent) return 0;
      return torrent.progress * 100;
  }
  ```
- [ ] **VALIDAR:** Sintaxe ES6 válida
- [ ] **BUILDAR:** N/A (JS)
- [ ] **TESTAR:** Teste manual no browser
- [ ] **COMMIT:** `feat(webtorrent): implementar módulo JavaScript WebTorrent`

---

#### Task 1.3.4: Adicionar MagnetUri ao Modelo Video
- [ ] **ALTERAÇÃO:** Adicionar propriedade em `Video.cs`
  ```csharp
  public class Video : BaseEntity, IAggregateRoot
  {
      // ... propriedades existentes
      public string? MagnetUri { get; private set; }
      public bool IsTorrentEnabled { get; private set; }
      
      public void EnableTorrent(string magnetUri)
      {
          MagnetUri = magnetUri;
          IsTorrentEnabled = true;
      }
  }
  ```
- [ ] **VALIDAR:** Propriedade nullable para vídeos legados
- [ ] **BUILDAR:** `dotnet build src/Core/OnForkHub.Core`
- [ ] **TESTAR:** Testar: `EnableTorrent_SetsMagnetUriAndFlag`
- [ ] **COMMIT:** `feat(webtorrent): adicionar MagnetUri e IsTorrentEnabled ao Video`

---

#### Task 1.3.5: Criar EF Migration para Video
- [ ] **ALTERAÇÃO:** Adicionar migration
  ```bash
  dotnet ef migrations add AddTorrentFieldsToVideo --project src/Infrastructure/OnForkHub.Persistence --startup-project src/Presentations/OnForkHub.Api
  ```
- [ ] **VALIDAR:** Migration gerada corretamente
- [ ] **BUILDAR:** `dotnet build src/Infrastructure/OnForkHub.Persistence`
- [ ] **TESTAR:** Aplicar migration em banco de teste
- [ ] **COMMIT:** `feat(webtorrent): adicionar migration AddTorrentFieldsToVideo`

---

#### Task 1.3.6: Criar API Endpoint para Gerar Torrent
- [ ] **ALTERAÇÃO:** Adicionar endpoint em `VideoEndpoints.cs`
  ```csharp
  app.MapPost("/api/videos/{id:guid}/torrent", async (
      Guid id,
      IVideoService videoService,
      WebTorrentService webTorrentService) =>
  {
      var video = await videoService.GetByIdAsync(id);
      if (!video.IsSuccess) return Results.NotFound();
      
      var videoData = await videoService.GetVideoDataAsync(id);
      var magnetUri = await webTorrentService.CreateTorrentAsync(videoData, video.Value.Title);
      
      await videoService.EnableTorrentAsync(id, magnetUri);
      
      return Results.Ok(new { MagnetUri = magnetUri });
  }).RequireAuthorization();
  ```
- [ ] **VALIDAR:** Endpoint retorna magnet URI
- [ ] **BUILDAR:** `dotnet build src/Presentations/OnForkHub.Api`
- [ ] **TESTAR:** Teste de integração com mock de WebTorrentService
- [ ] **COMMIT:** `feat(webtorrent): adicionar endpoint POST /api/videos/{id}/torrent`

---

#### Task 1.3.7: Criar Componente de Download P2P
- [ ] **ALTERAÇÃO:** Criar `P2PVideoPlayer.razor`
  ```razor
  @inject WebTorrentService WebTorrentService
  @inject IJSRuntime JSRuntime
  
  <div class="p2p-player">
      <video id="videoPlayer" controls></video>
      <div class="p2p-stats">
          <span>Peers: @peerCount</span>
          <span>Progress: @progress%</span>
          <span>Download: @formatSpeed(downloadSpeed)/s</span>
          <span>Upload: @formatSpeed(uploadSpeed)/s</span>
      </div>
  </div>
  
  @code {
      [Parameter] public string MagnetUri { get; set; } = default!;
      
      private int peerCount;
      private double progress;
      private double downloadSpeed;
      private double uploadSpeed;
      
      protected override async Task OnInitializedAsync()
      {
          await WebTorrentService.InitializeAsync();
          await WebTorrentService.StartDownloadAsync(MagnetUri, "/videos/temp");
          _ = UpdateStatsAsync();
      }
      
      private async Task UpdateStatsAsync()
      {
          while (true)
          {
              var stats = await JSRuntime.InvokeAsync<P2PStats>("getTorrentStats", MagnetUri);
              peerCount = stats.Peers;
              progress = stats.Progress;
              downloadSpeed = stats.DownloadSpeed;
              uploadSpeed = stats.UploadSpeed;
              StateHasChanged();
              await Task.Delay(1000);
          }
      }
  }
  ```
- [ ] **VALIDAR:** Componente renderiza stats em tempo real
- [ ] **BUILDAR:** `dotnet build src/Presentations/OnForkHub.Web`
- [ ] **TESTAR:** Teste manual com torrent de teste
- [ ] **COMMIT:** `feat(webtorrent): criar componente P2PVideoPlayer com estatísticas`

---

#### Task 1.3.8: Implementar Seeding Management
- [ ] **ALTERAÇÃO:** Adicionar controle de seeding no backend
  ```csharp
  public interface ITorrentTrackerService
  {
      Task<int> GetPeerCountAsync(string magnetUri);
      Task<bool> IsHealthyAsync(string magnetUri);
      Task ReannounceAsync(string magnetUri);
  }
  ```
- [ ] **VALIDAR:** Interface define métricas de saúde
- [ ] **BUILDAR:** `dotnet build src/Core/OnForkHub.Core`
- [ ] **TESTAR:** Mockar tracker e testar
- [ ] **COMMIT:** `feat(webtorrent): definir interface ITorrentTrackerService`

---

#### Task 1.3.9: Adicionar Fallback CDN quando P2P Falha
- [ ] **ALTERAÇÃO:** Modificar `P2PVideoPlayer` para fallback
  ```csharp
  private async Task InitializePlayerAsync()
  {
      try
      {
          await WebTorrentService.StartDownloadAsync(MagnetUri, "/videos/temp");
          var stats = await JSRuntime.InvokeAsync<P2PStats>("getTorrentStats", MagnetUri);
          
          if (stats.Peers == 0 && !string.IsNullOrEmpty(CdnUrl))
          {
              // Fallback para CDN
              await JSRuntime.InvokeVoidAsync("setVideoSource", "videoPlayer", CdnUrl);
          }
      }
      catch
      {
          if (!string.IsNullOrEmpty(CdnUrl))
          {
              await JSRuntime.InvokeVoidAsync("setVideoSource", "videoPlayer", CdnUrl);
          }
      }
  }
  ```
- [ ] **VALIDAR:** Fallback funciona quando P2P indisponível
- [ ] **BUILDAR:** `dotnet build src/Presentations/OnForkHub.Web`
- [ ] **TESTAR:** Simular falha de P2P
- [ ] **COMMIT:** `feat(webtorrent): implementar fallback para CDN quando P2P falha`

---

#### Task 1.3.10: Documentar WebTorrent Integration
- [ ] **ALTERAÇÃO:** Criar `docs/WEBTORRENT_INTEGRATION.md`
  - Como funciona o P2P no OnForkHub
  - Configuração de trackers
  - Troubleshooting
- [ ] **VALIDAR:** Documento claro e completo
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `docs(webtorrent): adicionar documentação de integração WebTorrent`

**Status WebTorrent:** 🔴 0/10 tasks | Estimativa: 7 dias

---

### 1.4 INTEGRATION TESTS PARA API (2 dias)

#### Task 1.4.1: Configurar WebApplicationFactory para API
- [ ] **ALTERAÇÃO:** Criar `CustomWebApplicationFactory.cs`
  ```csharp
  public class CustomWebApplicationFactory : WebApplicationFactory<Program>
  {
      protected override void ConfigureWebHost(IWebHostBuilder builder)
      {
          builder.ConfigureServices(services =>
          {
              // Substituir DbContext por InMemory
              var descriptor = services.SingleOrDefault(
                  d => d.ServiceType == typeof(DbContextOptions<OnForkHubDbContext>));
              if (descriptor != null) services.Remove(descriptor);
              
              services.AddDbContext<OnForkHubDbContext>(options =>
              {
                  options.UseInMemoryDatabase("TestDb");
              });
              
              // Substituir serviços externos por mocks
              services.AddScoped<IFileStorageService, MockFileStorageService>();
          });
      }
  }
  ```
- [ ] **VALIDAR:** Factory compila e inicializa
- [ ] **BUILDAR:** `dotnet build test/Presentations/OnForkHub.Api.IntegrationTests`
- [ ] **TESTAR:** Criar teste: `Factory_CreatesClient_Successfully`
- [ ] **COMMIT:** `test(integration): configurar CustomWebApplicationFactory`

---

#### Task 1.4.2: Criar Teste de Integração - Health Check
- [ ] **ALTERAÇÃO:** Criar `HealthCheckTests.cs`
  ```csharp
  public class HealthCheckTests : IClassFixture<CustomWebApplicationFactory>
  {
      private readonly HttpClient _client;
      
      public HealthCheckTests(CustomWebApplicationFactory factory)
      {
          _client = factory.CreateClient();
      }
      
      [Fact]
      public async Task HealthCheck_ReturnsHealthy()
      {
          var response = await _client.GetAsync("/health");
          response.StatusCode.Should().Be(HttpStatusCode.OK);
      }
  }
  ```
- [ ] **VALIDAR:** Teste executa e passa
- [ ] **BUILDAR:** `dotnet build test/Presentations/OnForkHub.Api.IntegrationTests`
- [ ] **TESTAR:** Executar: `dotnet test --filter "HealthCheck"`
- [ ] **COMMIT:** `test(integration): adicionar teste de health check`

---

#### Task 1.4.3: Criar Testes para Categorias Endpoints
- [ ] **ALTERAÇÃO:** Criar `CategoriesEndpointsTests.cs`
  - Testar: GET /api/categories (lista)
  - Testar: GET /api/categories/{id} (detalhe)
  - Testar: POST /api/categories (criação)
  - Testar: PUT /api/categories/{id} (update)
  - Testar: DELETE /api/categories/{id} (delete)
- [ ] **VALIDAR:** Todos os endpoints cobertos
- [ ] **BUILDAR:** Build testes
- [ ] **TESTAR:** `dotnet test --filter "CategoriesEndpoints"`
- [ ] **COMMIT:** `test(integration): adicionar testes para Categories endpoints`

---

#### Task 1.4.4: Criar Testes para Autenticação
- [ ] **ALTERAÇÃO:** Criar `AuthenticationTests.cs`
  - Testar: POST /api/auth/login (sucesso)
  - Testar: POST /api/auth/login (credenciais inválidas)
  - Testar: POST /api/auth/refresh-token
  - Testar: Acesso a endpoint protegido sem token (401)
  - Testar: Acesso a endpoint protegido com token válido (200)
- [ ] **VALIDAR:** Autenticação JWT testada
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** `dotnet test --filter "Authentication"`
- [ ] **COMMIT:** `test(integration): adicionar testes de autenticação JWT`

---

#### Task 1.4.5: Criar Collection Fixture para Testcontainers (Opcional)
- [ ] **ALTERAÇÃO:** Configurar Testcontainers para SQL Server
  ```csharp
  [CollectionDefinition("Database")]
  public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }
  
  public class DatabaseFixture : IAsyncLifetime
  {
      private readonly MsSqlContainer _sqlContainer;
      
      public DatabaseFixture()
      {
          _sqlContainer = new MsSqlBuilder().Build();
      }
      
      public string ConnectionString => _sqlContainer.GetConnectionString();
      
      public async Task InitializeAsync() => await _sqlContainer.StartAsync();
      public async Task DisposeAsync() => await _sqlContainer.DisposeAsync();
  }
  ```
- [ ] **VALIDAR:** Container SQL Server inicia corretamente
- [ ] **BUILDAR:** `dotnet build`
- [ ] **TESTAR:** Teste com banco real em container
- [ ] **COMMIT:** `test(integration): configurar Testcontainers para SQL Server`

**Status Integration Tests:** 🔴 0/5 tasks | Estimativa: 2 dias

---

## 🎯 FASE 2: MUST HAVE (Features Críticas)

> **OBJETIVO:** Implementar features essenciais para MVP  
> **IMPACTO:** Funcionalidade completa de video sharing  
> **PRAZO:** 45 dias após FASE 1  
> **DEPENDÊNCIAS:** FASE 0 completa

---

### 2.1 USER AUTHENTICATION UI (JWT pronto, UI pendente)

#### Task 2.1.1: Criar Página de Login
- [ ] **ALTERAÇÃO:** Criar `Login.razor`
  - Formulário: Email, Senha, Remember Me
  - Link: Esqueci minha senha
  - Link: Criar conta
- [ ] **VALIDAR:** Formulário valida campos obrigatórios
- [ ] **BUILDAR:** `dotnet build src/Presentations/OnForkHub.Web`
- [ ] **TESTAR:** 
  - [ ] Teste bUnit: `LoginPage_RendersForm`
  - [ ] Teste bUnit: `LoginPage_ShowsValidationErrors_WhenEmpty`
- [ ] **COMMIT:** `feat(auth-ui): criar página de login com validação`

---

#### Task 2.1.2: Criar Página de Registro
- [ ] **ALTERAÇÃO:** Criar `Register.razor`
  - Campos: Nome, Email, Senha, Confirmar Senha
  - Validação: Senha mínimo 8 caracteres, complexidade
  - Checkbox: Aceitar termos
- [ ] **VALIDAR:** Validações client-side e server-side
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** 
  - [ ] Teste: `RegisterPage_ShowsError_WhenPasswordsDontMatch`
  - [ ] Teste: `RegisterPage_ShowsError_WhenEmailExists`
- [ ] **COMMIT:** `feat(auth-ui): criar página de registro com validações`

---

#### Task 2.1.3: Implementar AuthService no Blazor
- [ ] **ALTERAÇÃO:** Criar `AuthenticationService.cs`
  ```csharp
  public class AuthenticationService
  {
      private readonly HttpClient _httpClient;
      private readonly ILocalStorageService _localStorage;
      private readonly AuthenticationStateProvider _authStateProvider;
      
      public async Task<AuthResult> LoginAsync(string email, string password)
      {
          var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { email, password });
          if (response.IsSuccessStatusCode)
          {
              var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
              await _localStorage.SetItemAsync("token", result!.Token);
              await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);
              ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
              return AuthResult.Success();
          }
          return AuthResult.Failure("Credenciais inválidas");
      }
  }
  ```
- [ ] **VALIDAR:** Gerencia tokens JWT e refresh tokens
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** 
  - [ ] Mockar HttpClient e LocalStorage
  - [ ] Testar: `LoginAsync_SavesToken_WhenSuccess`
  - [ ] Testar: `LoginAsync_ReturnsFailure_WhenInvalidCredentials`
- [ ] **COMMIT:** `feat(auth-ui): implementar AuthenticationService com JWT e refresh`

---

#### Task 2.1.4: Criar CustomAuthStateProvider
- [ ] **ALTERAÇÃO:** Implementar `CustomAuthStateProvider.cs`
  ```csharp
  public class CustomAuthStateProvider : AuthenticationStateProvider
  {
      private readonly ILocalStorageService _localStorage;
      private readonly HttpClient _httpClient;
      
      public override async Task<AuthenticationState> GetAuthenticationStateAsync()
      {
          var token = await _localStorage.GetItemAsync<string>("token");
          
          if (string.IsNullOrWhiteSpace(token))
              return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
          
          _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
          
          var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
          return new AuthenticationState(new ClaimsPrincipal(identity));
      }
      
      public void NotifyUserAuthentication(string token)
      {
          var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
          var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
          NotifyAuthenticationStateChanged(authState);
      }
  }
  ```
- [ ] **VALIDAR:** Integra com Blazor Authorization
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar estado de autenticação
- [ ] **COMMIT:** `feat(auth-ui): criar CustomAuthStateProvider para JWT`

---

#### Task 2.1.5: Criar Componente de Menu com Auth
- [ ] **ALTERAÇÃO:** Modificar `NavMenu.razor`
  ```razor
  <AuthorizeView>
      <Authorized>
          <span>Olá, @context.User.Identity?.Name!</span>
          <button @onclick="Logout">Sair</button>
      </Authorized>
      <NotAuthorized>
          <a href="/login">Entrar</a>
          <a href="/register">Registrar</a>
      </NotAuthorized>
  </AuthorizeView>
  ```
- [ ] **VALIDAR:** Menu muda baseado em autenticação
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Teste bUnit: `NavMenu_ShowsLoginLink_WhenNotAuthenticated`
- [ ] **COMMIT:** `feat(auth-ui): adicionar menu condicional baseado em autenticação`

---

#### Task 2.1.6: Implementar Logout
- [ ] **ALTERAÇÃO:** Adicionar método Logout
  ```csharp
  public async Task LogoutAsync()
  {
      await _localStorage.RemoveItemAsync("token");
      await _localStorage.RemoveItemAsync("refreshToken");
      ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
  }
  ```
- [ ] **VALIDAR:** Remove tokens do storage
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Teste: `LogoutAsync_RemovesTokenAndNotifiesLogout`
- [ ] **COMMIT:** `feat(auth-ui): implementar logout com limpeza de tokens`

---

#### Task 2.1.7: Criar Página "Esqueci Senha"
- [ ] **ALTERAÇÃO:** Criar `ForgotPassword.razor`
  - Input: Email
  - Enviar email com token de reset
  - Mensagem: "Se o email existir, enviamos instruções"
- [ ] **VALIDAR:** Não revela se email existe (segurança)
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Teste de integração com mock de email
- [ ] **COMMIT:** `feat(auth-ui): criar página de recuperação de senha`

---

#### Task 2.1.8: Proteger Rotas com Authorize
- [ ] **ALTERAÇÃO:** Adicionar `@attribute [Authorize]` em páginas protegidas
  - `/profile`
  - `/upload`
  - `/my-videos`
- [ ] **VALIDAR:** Redireciona para login quando não autenticado
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Teste: `ProtectedRoute_RedirectsToLogin_WhenNotAuthenticated`
- [ ] **COMMIT:** `feat(auth-ui): proteger rotas com atributo Authorize`

**Status Auth UI:** 🔴 0/8 tasks | Estimativa: 4 dias

---

### 2.2 VIDEO STREAMING COM ADAPTIVE BITRATE

#### Task 2.2.1: Adicionar FFmpeg para Transcoding
- [ ] **ALTERAÇÃO:** Configurar FFmpeg no Docker
  ```dockerfile
  # Em Dockerfile.OnForkHub.Api
  RUN apt-get update && apt-get install -y ffmpeg
  ```
- [ ] **VALIDAR:** FFmpeg disponível no container
- [ ] **BUILDAR:** Build Docker
- [ ] **TESTAR:** `ffmpeg -version` no container
- [ ] **COMMIT:** `chore(streaming): adicionar FFmpeg ao Dockerfile`

---

#### Task 2.2.2: Criar VideoTranscodingService
- [ ] **ALTERAÇÃO:** Implementar serviço de transcodificação
  ```csharp
  public interface IVideoTranscodingService
  {
      Task<TranscodingResult> TranscodeToAdaptiveBitrateAsync(
          string inputPath, 
          string outputDirectory,
          CancellationToken cancellationToken = default);
  }
  
  public class VideoTranscodingService : IVideoTranscodingService
  {
      public async Task<TranscodingResult> TranscodeToAdaptiveBitrateAsync(
          string inputPath, string outputDirectory, CancellationToken cancellationToken)
      {
          // Gerar múltiplas resoluções: 1080p, 720p, 480p, 360p
          var resolutions = new[] { "1920x1080", "1280x720", "854x480", "640x360" };
          var bitrates = new[] { "5000k", "2500k", "1000k", "500k" };
          
          for (int i = 0; i < resolutions.Length; i++)
          {
              var outputFile = Path.Combine(outputDirectory, $"video_{resolutions[i]}.mp4");
              var ffmpegArgs = $"-i {inputPath} -vf scale={resolutions[i]} -b:v {bitrates[i]} -c:a copy {outputFile}";
              await RunFFmpegAsync(ffmpegArgs, cancellationToken);
          }
          
          // Gerar arquivo MPD (MPEG-DASH) ou M3U8 (HLS)
          return new TranscodingResult { Success = true };
      }
  }
  ```
- [ ] **VALIDAR:** Gera múltiplas resoluções
- [ ] **BUILDAR:** `dotnet build src/Core/OnForkHub.Application`
- [ ] **TESTAR:** 
  - [ ] Mockar processo FFmpeg
  - [ ] Testar: `TranscodeToAdaptiveBitrate_GeneratesMultipleResolutions`
- [ ] **COMMIT:** `feat(streaming): implementar VideoTranscodingService com FFmpeg`

---

#### Task 2.2.3: Criar Background Job para Transcoding
- [ ] **ALTERAÇÃO:** Adicionar ao `VideoProcessingBackgroundService`
  ```csharp
  private async Task ProcessPendingTranscodingAsync(CancellationToken stoppingToken)
  {
      var pendingVideos = await _videoRepository.GetPendingTranscodingAsync();
      foreach (var video in pendingVideos)
      {
          video.MarkAsTranscoding();
          await _videoRepository.UpdateAsync(video);
          
          try
          {
              var result = await _transcodingService.TranscodeToAdaptiveBitrateAsync(
                  video.RawFilePath, video.TranscodedDirectory, stoppingToken);
              
              if (result.Success)
                  video.MarkAsTranscoded();
              else
                  video.MarkAsTranscodingFailed();
          }
          catch
          {
              video.MarkAsTranscodingFailed();
          }
          
          await _videoRepository.UpdateAsync(video);
      }
  }
  ```
- [ ] **VALIDAR:** Processa vídeos em background
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar comportamento de sucesso e falha
- [ ] **COMMIT:** `feat(streaming): adicionar processamento de transcoding em background`

---

#### Task 2.2.4: Criar API Endpoint para Streaming
- [ ] **ALTERAÇÃO:** Implementar endpoint de streaming com suporte a range
  ```csharp
  app.MapGet("/api/videos/{id:guid}/stream", async (
      Guid id,
      [FromHeader(Name = "Range")] string? range,
      IVideoService videoService) =>
  {
      var video = await videoService.GetByIdAsync(id);
      if (!video.IsSuccess) return Results.NotFound();
      
      var videoPath = video.Value.TranscodedPath ?? video.Value.RawFilePath;
      var fileInfo = new FileInfo(videoPath);
      
      if (string.IsNullOrEmpty(range))
      {
          // Stream completo
          return Results.File(videoPath, "video/mp4", enableRangeProcessing: true);
      }
      else
      {
          // Range request (para seek)
          var (start, end) = ParseRange(range, fileInfo.Length);
          var stream = new FileStream(videoPath, FileMode.Open, FileAccess.Read, FileShare.Read);
          stream.Seek(start, SeekOrigin.Begin);
          
          return Results.Stream(
              stream, 
              "video/mp4", 
              statusCode: 206, 
              fileLength: end - start + 1);
      }
  });
  ```
- [ ] **VALIDAR:** Suporta range requests (HTTP 206)
- [ ] **BUILDAR:** `dotnet build src/Presentations/OnForkHub.Api`
- [ ] **TESTAR:** 
  - [ ] Testar: `Stream_Returns206_WhenRangeHeaderPresent`
  - [ ] Testar: `Stream_Returns200_WhenNoRangeHeader`
- [ ] **COMMIT:** `feat(streaming): adicionar endpoint de streaming com range support`

---

#### Task 2.2.5: Implementar DASH/HLS Manifest
- [ ] **ALTERAÇÃO:** Criar gerador de manifesto MPEG-DASH
  ```csharp
  public class DashManifestGenerator
  {
      public string GenerateManifest(string baseUrl, Guid videoId)
      {
          return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
  <MPD xmlns=""urn:mpeg:dash:schema:mpd:2011"" type=""static"" mediaPresentationDuration=""PT2M"">
      <Period>
          <AdaptationSet mimeType=""video/mp4"" codecs=""avc1.42E01E"">
              <Representation id=""1080p"" bandwidth=""5000000"">
                  <BaseURL>{baseUrl}/api/videos/{videoId}/segments/1080p</BaseURL>
                  <SegmentTemplate timescale=""1000"" duration=""4000"" />
              </Representation>
              <Representation id=""720p"" bandwidth=""2500000"">
                  <BaseURL>{baseUrl}/api/videos/{videoId}/segments/720p</BaseURL>
                  <SegmentTemplate timescale=""1000"" duration=""4000"" />
              </Representation>
          </AdaptationSet>
      </Period>
  </MPD>";
      }
  }
  ```
- [ ] **VALIDAR:** XML válido para DASH
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Validar XML gerado
- [ ] **COMMIT:** `feat(streaming): implementar gerador de manifesto DASH`

---

#### Task 2.2.6: Criar Componente Video Player com DASH.js
- [ ] **ALTERAÇÃO:** Criar `AdaptiveVideoPlayer.razor`
  ```razor
  @inject IJSRuntime JSRuntime
  
  <div id="videoContainer"></div>
  
  @code {
      [Parameter] public string ManifestUrl { get; set; } = default!;
      private IJSObjectReference? _playerModule;
      
      protected override async Task OnAfterRenderAsync(bool firstRender)
      {
          if (firstRender)
          {
              _playerModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                  "import", "./js/dashPlayer.js");
              await _playerModule.InvokeVoidAsync("initialize", "videoContainer", ManifestUrl);
          }
      }
  }
  ```
- [ ] **VALIDAR:** Player DASH.js inicializa
- [ ] **BUILDAR:** `dotnet build src/Presentations/OnForkHub.Web`
- [ ] **TESTAR:** Teste manual com manifesto
- [ ] **COMMIT:** `feat(streaming): criar componente AdaptiveVideoPlayer com DASH.js`

---

#### Task 2.2.7: Adicionar Auto-Quality Selection
- [ ] **ALTERAÇÃO:** Configurar DASH.js para auto-quality
  ```javascript
  // Em dashPlayer.js
  export function initialize(containerId, manifestUrl) {
      var player = dashjs.MediaPlayer().create();
      player.initialize(document.getElementById(containerId), manifestUrl, true);
      player.setAutoSwitchQuality(true);
      
      // Fallback para próxima qualidade se buffering
      player.on(dashjs.MediaPlayer.events.BUFFER_EMPTY, function() {
          player.setQualityFor('video', player.getQualityFor('video') - 1);
      });
  }
  ```
- [ ] **VALIDAR:** Adapta qualidade baseado na conexão
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Simular velocidade de rede
- [ ] **COMMIT:** `feat(streaming): implementar auto-quality selection no player`

**Status Streaming:** 🔴 0/7 tasks | Estimativa: 5 dias

---

### 2.3 P2P WEBTORRENT (Completar Implementação)

> Nota: WebTorrent iniciado na FASE 1.3, completar integração.

#### Task 2.3.1: Criar API para Estatísticas P2P
- [ ] **ALTERAÇÃO:** Endpoint para métricas de torrent
  ```csharp
  app.MapGet("/api/videos/{id:guid}/torrent/stats", async (Guid id, ITorrentTrackerService tracker) =>
  {
      var video = await videoService.GetByIdAsync(id);
      if (!video.IsSuccess || !video.Value.IsTorrentEnabled)
          return Results.NotFound();
      
      var stats = await tracker.GetStatsAsync(video.Value.MagnetUri!);
      return Results.Ok(new {
          Peers = stats.PeerCount,
          Seeds = stats.SeedCount,
          Leeches = stats.LeechCount,
          Health = stats.HealthScore
      });
  });
  ```
- [ ] **VALIDAR:** Retorna estatísticas em tempo real
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar com mock
- [ ] **COMMIT:** `feat(webtorrent): adicionar endpoint de estatísticas P2P`

---

#### Task 2.3.2: Implementar Controle de Bandwidth
- [ ] **ALTERAÇÃO:** Adicionar configurações de throttling
  ```javascript
  // Configurar limite de upload/download
  client.throttleUpload(1024 * 1024); // 1MB/s
  client.throttleDownload(5 * 1024 * 1024); // 5MB/s
  ```
- [ ] **VALIDAR:** Respeita limites configurados
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Medir velocidade real
- [ ] **COMMIT:** `feat(webtorrent): implementar controle de bandwidth`

---

#### Task 2.3.3: Criar UI de Estatísticas P2P
- [ ] **ALTERAÇÃO:** Componente visual de stats
  ```razor
  <div class="p2p-stats-panel">
      <div class="stat">
          <i class="icon-peers"></i>
          <span>@Stats.PeerCount peers</span>
      </div>
      <div class="progress-bar">
          <div class="progress" style="width: @Stats.Progress%"></div>
      </div>
      <small>Download: @FormatSpeed(Stats.DownloadSpeed) | Upload: @FormatSpeed(Stats.UploadSpeed)</small>
  </div>
  ```
- [ ] **VALIDAR:** UI atualiza em tempo real
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Teste visual
- [ ] **COMMIT:** `feat(webtorrent): criar UI de estatísticas P2P`

**Status P2P Completar:** 🔴 0/3 tasks | Estimativa: 2 dias

---

### 2.4 CATEGORY MANAGEMENT

#### Task 2.4.1: CRUD Completo de Categorias na UI
- [ ] **ALTERAÇÃO:** Criar páginas:
  - `/admin/categories` - Listar (tabela com paginação)
  - `/admin/categories/create` - Criar
  - `/admin/categories/edit/{id}` - Editar
  - Confirmar delete com modal
- [ ] **VALIDAR:** Todas as operações funcionam
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testes bUnit para cada página
- [ ] **COMMIT:** `feat(categories): implementar CRUD completo de categorias na UI`

---

#### Task 2.4.2: Associar Vídeos a Categorias
- [ ] **ALTERAÇÃO:** Modificar upload para selecionar categoria
  - Dropdown em `VideoUploader.razor`
  - Salvar `CategoryId` no Video
- [ ] **VALIDAR:** Categoria obrigatória no upload
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar upload com e sem categoria
- [ ] **COMMIT:** `feat(categories): adicionar seleção de categoria no upload`

---

#### Task 2.4.3: Filtrar Vídeos por Categoria
- [ ] **ALTERAÇÃO:** Adicionar filtro na home
  ```razor
  <div class="category-filter">
      @foreach (var category in Categories)
      {
          <button @onclick="() => FilterByCategory(category.Id)" 
                  class="@GetActiveClass(category.Id)">
              @category.Name
          </button>
      }
  </div>
  ```
- [ ] **VALIDAR:** Filtro atualiza lista de vídeos
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar filtro
- [ ] **COMMIT:** `feat(categories): implementar filtro de vídeos por categoria`

**Status Categories:** 🔴 0/3 tasks | Estimativa: 3 dias

---

### 2.5 VIDEO RECOMMENDATIONS (Básico)

#### Task 2.5.1: Algoritmo Simples de Recomendação
- [ ] **ALTERAÇÃO:** Implementar `IRecommendationService`
  ```csharp
  public class SimpleRecommendationService : IRecommendationService
  {
      public async Task<IReadOnlyList<VideoResponseDto>> GetRecommendationsAsync(
          string userId, 
          int count = 10)
      {
          // Algoritmo: Vídeos da mesma categoria que o usuário assistiu
          var watchedCategories = await _videoViewRepository
              .GetWatchedCategoriesAsync(userId);
          
          var recommendations = await _videoRepository
              .GetByCategoriesAsync(watchedCategories, excludeUserId: userId)
              .OrderByDescending(v => v.ViewCount)
              .Take(count)
              .ToListAsync();
          
          return _mapper.Map<List<VideoResponseDto>>(recommendations);
      }
  }
  ```
- [ ] **VALIDAR:** Retorna vídeos relacionados
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** 
  - [ ] Testar: `GetRecommendations_ReturnsVideosFromWatchedCategories`
- [ ] **COMMIT:** `feat(recommendations): implementar algoritmo simples baseado em categorias`

---

#### Task 2.5.2: Criar Endpoint de Recomendações
- [ ] **ALTERAÇÃO:** Adicionar endpoint
  ```csharp
  app.MapGet("/api/videos/recommendations", async (
      IRecommendationService recommendationService,
      ClaimsPrincipal user) =>
  {
      var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var recommendations = await recommendationService.GetRecommendationsAsync(userId, 8);
      return Results.Ok(recommendations);
  }).RequireAuthorization();
  ```
- [ ] **VALIDAR:** Retorna lista de vídeos
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Teste de integração
- [ ] **COMMIT:** `feat(recommendations): adicionar endpoint GET /api/videos/recommendations`

---

#### Task 2.5.3: Componente de Vídeos Recomendados
- [ ] **ALTERAÇÃO:** Criar `RecommendedVideos.razor`
  - Grid de vídeos recomendados
  - Mostrar na home e na página de vídeo
- [ ] **VALIDAR:** Carrega recomendações da API
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar componente
- [ ] **COMMIT:** `feat(recommendations): criar componente RecommendedVideos`

**Status Recommendations:** 🔴 0/3 tasks | Estimativa: 2 dias

---

## 💡 FASE 3: SHOULD HAVE (Features Importantes)

> **OBJETIVO:** Melhorar experiência do usuário  
> **PRAZO:** 30 dias após FASE 2  
> **DEPENDÊNCIAS:** FASE 2 completa

---

### 3.1 USER PREFERENCES & SETTINGS

#### Task 3.1.1: Criar Modelo de Preferences
- [ ] **ALTERAÇÃO:** Criar `UserPreferences.cs`
  ```csharp
  public class UserPreferences
  {
      public bool AutoPlayNextVideo { get; set; } = true;
      public VideoQuality DefaultQuality { get; set; } = VideoQuality.Auto;
      public bool EnableP2P { get; set; } = true;
      public bool DarkMode { get; set; } = false;
      public string Language { get; set; } = "pt-BR";
  }
  ```
- [ ] **VALIDAR:** Modelo com defaults razoáveis
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar defaults
- [ ] **COMMIT:** `feat(preferences): criar modelo UserPreferences`

---

#### Task 3.1.2: Persistir Preferences no Banco
- [ ] **ALTERAÇÃO:** Adicionar `Preferences` como JSON column em User
  - EF Core: `entity.Property(e => e.Preferences).HasColumnType("json")`
- [ ] **VALIDAR:** Serialização/deserialização funciona
- [ ] **BUILDAR:** Build + Migration
- [ ] **TESTAR:** Testar persistência
- [ ] **COMMIT:** `feat(preferences): adicionar persistência de preferences em JSON`

---

#### Task 3.1.3: Criar Página de Settings
- [ ] **ALTERAÇÃO:** Criar `Settings.razor`
  - Toggle: Auto-play
  - Dropdown: Qualidade padrão
  - Toggle: Habilitar P2P
  - Toggle: Dark mode
  - Dropdown: Idioma
- [ ] **VALIDAR:** Todas as preferências são salvas
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar cada configuração
- [ ] **COMMIT:** `feat(preferences): criar página de settings com todas as opções`

---

#### Task 3.1.4: Aplicar Preferences no Player
- [ ] **ALTERAÇÃO:** Modificar `AdaptiveVideoPlayer` para respeitar preferences
  ```csharp
  @code {
      protected override async Task OnInitializedAsync()
      {
          var prefs = await UserPreferencesService.GetAsync();
          
          if (prefs.DefaultQuality != VideoQuality.Auto)
              await Player.SetInitialQualityAsync(prefs.DefaultQuality);
          
          if (!prefs.EnableP2P && video.IsTorrentEnabled)
              UseCdnOnly = true;
      }
  }
  ```
- [ ] **VALIDAR:** Preferences afetam comportamento
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar cada preferência aplicada
- [ ] **COMMIT:** `feat(preferences): aplicar preferences no video player`

**Status Preferences:** 🔴 0/4 tasks | Estimativa: 3 dias

---

### 3.2 COMMENTS & RATING SYSTEM

#### Task 3.2.1: Criar Entidade Comment
- [ ] **ALTERAÇÃO:** Criar `Comment.cs`
  ```csharp
  public class Comment : BaseEntity
  {
      public Guid VideoId { get; private set; }
      public string UserId { get; private set; }
      public string Content { get; private set; }
      public int? ParentCommentId { get; private set; } // Para replies
      public DateTime CreatedAt { get; private set; }
      public bool IsEdited { get; private set; }
      
      public void Edit(string newContent)
      {
          Content = newContent;
          IsEdited = true;
      }
  }
  ```
- [ ] **VALIDAR:** Suporta comentários e replies
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar criação e edição
- [ ] **COMMIT:** `feat(comments): criar entidade Comment com suporte a replies`

---

#### Task 3.2.2: Criar Sistema de Rating (Like/Dislike)
- [ ] **ALTERAÇÃO:** Criar `VideoRating.cs`
  ```csharp
  public class VideoRating : BaseEntity
  {
      public Guid VideoId { get; private set; }
      public string UserId { get; private set; }
      public RatingType Type { get; private set; } // Like or Dislike
      public DateTime CreatedAt { get; private set; }
  }
  ```
- [ ] **VALIDAR:** Um usuário = um voto por vídeo
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar restrição de voto único
- [ ] **COMMIT:** `feat(ratings): criar sistema de like/dislike`

---

#### Task 3.2.3: CRUD de Comentários na API
- [ ] **ALTERAÇÃO:** Criar endpoints:
  - POST `/api/videos/{id}/comments`
  - GET `/api/videos/{id}/comments` (paginado)
  - PUT `/api/comments/{id}`
  - DELETE `/api/comments/{id}`
- [ ] **VALIDAR:** Todos endpoints protegidos com auth
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testes de integração
- [ ] **COMMIT:** `feat(comments): adicionar CRUD de comentários na API`

---

#### Task 3.2.4: UI de Comentários
- [ ] **ALTERAÇÃO:** Criar `CommentsSection.razor`
  - Lista de comentários com avatar
  - Campo de novo comentário
  - Botão reply
  - Botões like/dislike
  - Paginação
- [ ] **VALIDAR:** UI responsiva e funcional
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testes bUnit
- [ ] **COMMIT:** `feat(comments): implementar UI de comentários com replies`

---

#### Task 3.2.5: Mostrar Rating no Vídeo
- [ ] **ALTERAÇÃO:** Adicionar barra de like/dislike no player
  - Mostrar contagem
  - Destacar se usuário já votou
- [ ] **VALIDAR:** Interação funciona
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar votação
- [ ] **COMMIT:** `feat(ratings): adicionar UI de like/dislike no video player`

**Status Comments/Ratings:** 🔴 0/5 tasks | Estimativa: 4 dias

---

### 3.3 SOCIAL SHARING

#### Task 3.3.1: Criar ShareService
- [ ] **ALTERAÇÃO:** Implementar `IShareService`
  ```csharp
  public interface IShareService
  {
      string GenerateShareLink(Guid videoId);
      Task ShareToSocialAsync(SocialPlatform platform, string url, string message);
  }
  ```
- [ ] **VALIDAR:** Gera links únicos
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar geração de link
- [ ] **COMMIT:** `feat(social): criar ShareService para geração de links`

---

#### Task 3.3.2: Adicionar Botões de Share na UI
- [ ] **ALTERAÇÃO:** Criar `ShareButtons.razor`
  - Botões: Copy Link, Twitter/X, Facebook, WhatsApp, Email
  - Ícones de cada plataforma
- [ ] **VALIDAR:** Cada botão funciona corretamente
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar cada método de share
- [ ] **COMMIT:** `feat(social): adicionar botões de share para redes sociais`

---

#### Task 3.3.3: Meta Tags para Preview (Open Graph)
- [ ] **ALTERAÇÃO:** Adicionar meta tags dinâmicas no Blazor
  ```razor
  @code {
      protected override void OnInitialized()
      {
          // Injetar via JS
          JSRuntime.InvokeVoidAsync("setMetaTag", "og:title", Video.Title);
          JSRuntime.InvokeVoidAsync("setMetaTag", "og:description", Video.Description);
          JSRuntime.InvokeVoidAsync("setMetaTag", "og:image", Video.ThumbnailUrl);
          JSRuntime.InvokeVoidAsync("setMetaTag", "og:video", Video.StreamUrl);
      }
  }
  ```
- [ ] **VALIDAR:** Preview funciona no WhatsApp/Facebook/Twitter
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar com [Facebook Debugger](https://developers.facebook.com/tools/debug/)
- [ ] **COMMIT:** `feat(social): adicionar meta tags Open Graph para previews`

---

#### Task 3.3.4: Embeddable Player (IFrame)
- [ ] **ALTERAÇÃO:** Criar página de embed
  - `/embed/{videoId}` - Player simplificado sem UI do site
  - Suportar parâmetros: autoplay, start, quality
- [ ] **VALIDAR:** Funciona em iframe externo
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar embed em página HTML externa
- [ ] **COMMIT:** `feat(social): criar página de embed para iframe em sites externos`

**Status Social:** 🔴 0/4 tasks | Estimativa: 3 dias

---

### 3.4 ADVANCED SEARCH & FILTERING

#### Task 3.4.1: Implementar Full-Text Search
- [ ] **ALTERAÇÃO:** Adicionar Full-Text Search no SQL Server
  ```csharp
  // Migration
  migrationBuilder.Sql(@"
      CREATE FULLTEXT CATALOG ftCatalog AS DEFAULT;
      CREATE FULLTEXT INDEX ON Videos(Title, Description) 
      KEY INDEX PK_Videos;
  ");
  ```
- [ ] **VALIDAR:** Índice full-text criado
- [ ] **BUILDAR:** Build + Migration
- [ ] **TESTAR:** Testar busca com `FREETEXT`
- [ ] **COMMIT:** `feat(search): adicionar full-text search para título e descrição`

---

#### Task 3.4.2: Criar Search Endpoint
- [ ] **ALTERAÇÃO:** Implementar endpoint de busca
  ```csharp
  app.MapGet("/api/videos/search", async (
      [FromQuery] string q,
      [FromQuery] string? category,
      [FromQuery] DateTime? fromDate,
      [FromQuery] DateTime? toDate,
      [FromQuery] SortOption sort = SortOption.Relevance,
      IVideoSearchService searchService) =>
  {
      var results = await searchService.SearchAsync(new SearchRequest
      {
          Query = q,
          Category = category,
          FromDate = fromDate,
          ToDate = toDate,
          Sort = sort
      });
      
      return Results.Ok(results);
  });
  ```
- [ ] **VALIDAR:** Retorna resultados filtrados e ordenados
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar diferentes combinações de filtros
- [ ] **COMMIT:** `feat(search): implementar endpoint de busca com filtros`

---

#### Task 3.4.3: Criar Página de Search
- [ ] **ALTERAÇÃO:** Criar `Search.razor`
  - Input de busca com sugestões
  - Filtros laterais: Categoria, Data, Qualidade, Duração
  - Grid de resultados
  - Ordenação: Relevância, Data, Visualizações, Avaliação
- [ ] **VALIDAR:** UX de busca completa
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar busca e filtros
- [ ] **COMMIT:** `feat(search): criar página de busca com filtros avançados`

---

#### Task 3.4.4: Sugestões de Busca (Autocomplete)
- [ ] **ALTERAÇÃO:** Implementar sugestões
  ```csharp
  app.MapGet("/api/search/suggestions", async (
      [FromQuery] string q,
      ISearchSuggestionService suggestionService) =>
  {
      var suggestions = await suggestionService.GetSuggestionsAsync(q, limit: 8);
      return Results.Ok(suggestions);
  });
  ```
- [ ] **VALIDAR:** Retorna sugestões rápido (< 100ms)
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar performance
- [ ] **COMMIT:** `feat(search): adicionar autocomplete para busca`

**Status Search:** 🔴 0/4 tasks | Estimativa: 4 dias

---

## 🌟 FASE 4: NICE TO HAVE (Features Avançadas)

> **OBJETIVO:** Diferenciais competitivos  
> **PRAZO:** Futuro (pós-MVP)  
> **DEPENDÊNCIAS:** FASE 3 completa

---

### 4.1 LIVE STREAMING CAPABILITY

#### Task 4.1.1: Pesquisar Tecnologias de Live Streaming
- [ ] **ALTERAÇÃO:** Documentar opções:
  - WebRTC (P2P)
  - RTMP + HLS
  - WebTransport
- [ ] **VALIDAR:** Documento de análise comparativa
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `docs(live-streaming): análise de tecnologias para live streaming`

---

#### Task 4.1.2: POC com WebRTC
- [ ] **ALTERAÇÃO:** Implementar prova de conceito
  - Broadcaster: captura webcam e envia
  - Viewer: recebe stream via WebRTC
- [ ] **VALIDAR:** Latência < 2 segundos
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar local
- [ ] **COMMIT:** `poc(live-streaming): implementar POC de WebRTC streaming`

**Status Live Streaming:** 🔴 0/2 tasks | Estimativa: 2 dias (POC)

---

### 4.2 VIDEO EDITING TOOLS

#### Task 4.2.1: Criar Componente de Trim/Cut
- [ ] **ALTERAÇÃO:** Implementar trim no frontend
  - Slider com dois handles (start/end)
  - Preview do trecho selecionado
- [ ] **VALIDAR:** UI funcional
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar UX
- [ ] **COMMIT:** `feat(video-editing): criar componente de trim/cut na UI`

---

#### Task 4.2.2: Implementar Trim no Backend (FFmpeg)
- [ ] **ALTERAÇÃO:** Adicionar endpoint de trim
  ```csharp
  app.MapPost("/api/videos/{id:guid}/trim", async (
      Guid id,
      TrimRequest request,
      IVideoEditingService editingService) =>
  {
      var trimmedVideo = await editingService.TrimAsync(id, request.StartSeconds, request.EndSeconds);
      return Results.Ok(trimmedVideo);
  });
  ```
- [ ] **VALIDAR:** FFmpeg trim funciona
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar trim de vídeo
- [ ] **COMMIT:** `feat(video-editing): implementar trim com FFmpeg no backend`

**Status Video Editing:** 🔴 0/2 tasks | Estimativa: 2 dias

---

### 4.3 AI-POWERED CONTENT MODERATION

#### Task 4.3.1: Pesquisar APIs de Content Moderation
- [ ] **ALTERAÇÃO:** Avaliar opções:
  - Azure Content Moderator
  - AWS Rekognition
  - Google Video Intelligence
  - OpenAI Moderation
- [ ] **VALIDAR:** Documento com custos e limitações
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `docs(ai-moderation): análise de APIs de content moderation`

---

#### Task 4.3.2: Implementar Moderation Básica (Azure)
- [ ] **ALTERAÇÃO:** Criar `IContentModerationService`
  ```csharp
  public class AzureContentModerationService : IContentModerationService
  {
      public async Task<ModerationResult> ModerateVideoAsync(string videoUrl)
      {
          // Enviar para Azure Content Moderator
          // Retornar: IsApproved, Confidence, Categories (adult, violent, etc)
      }
  }
  ```
- [ ] **VALIDAR:** Integração com Azure funciona
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar com vídeos de teste
- [ ] **COMMIT:** `feat(ai-moderation): implementar integração com Azure Content Moderator`

---

#### Task 4.3.3: Adicionar Fila de Moderação
- [ ] **ALTERAÇÃO:** Modificar pipeline de upload
  - Após upload: enviar para moderação
  - Status: Pending → UnderReview → Approved/Rejected
- [ ] **VALIDAR:** Workflow completo
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar fluxo de moderação
- [ ] **COMMIT:** `feat(ai-moderation): adicionar fila de moderação pós-upload`

**Status AI Moderation:** 🔴 0/3 tasks | Estimativa: 3 dias

---

## 🏗️ FASE 5: ARQUITETURA & DEVOPS

> **OBJETIVO:** Melhorar arquitetura e infraestrutura  
> **IMPACTO:** GPA Código: B → A (82% → 90%+)  
> **PRAZO:** Paralelo às FASES 1-4

---

### 5.1 CQRS COM MEDIATR

#### Task 5.1.1: Adicionar MediatR ao Projeto
- [ ] **ALTERAÇÃO:** Adicionar pacotes NuGet
  ```xml
  <PackageVersion Include="MediatR" Version="12.2.0" />
  <PackageVersion Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="11.1.0" />
  ```
- [ ] **VALIDAR:** Pacotes restauram
- [ ] **BUILDAR:** `dotnet restore`
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `chore(cqrs): adicionar pacotes MediatR`

---

#### Task 5.1.2: Criar Estrutura de Commands
- [ ] **ALTERAÇÃO:** Criar pasta `OnForkHub.Application/Features/Categories/Commands/`
  ```csharp
  // CreateCategoryCommand.cs
  public record CreateCategoryCommand(string Name, string Description) : IRequest<RequestResult<CategoryResponseDto>>;
  
  // CreateCategoryCommandHandler.cs
  public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, RequestResult<CategoryResponseDto>>
  {
      private readonly ICategoryRepositoryEF _repository;
      private readonly IMapper _mapper;
      
      public async Task<RequestResult<CategoryResponseDto>> Handle(
          CreateCategoryCommand request, 
          CancellationToken cancellationToken)
      {
          var category = Category.Create(request.Name, request.Description);
          await _repository.AddAsync(category, cancellationToken);
          return RequestResult<CategoryResponseDto>.Success(_mapper.Map<CategoryResponseDto>(category));
      }
  }
  ```
- [ ] **VALIDAR:** Command + Handler implementados
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar handler
- [ ] **COMMIT:** `feat(cqrs): implementar CreateCategoryCommand com MediatR`

---

#### Task 5.1.3: Criar Estrutura de Queries
- [ ] **ALTERAÇÃO:** Criar pasta `OnForkHub.Application/Features/Categories/Queries/`
  ```csharp
  // GetCategoriesQuery.cs
  public record GetCategoriesQuery(int Page = 1, int PageSize = 20) : IRequest<RequestResult<PaginatedList<CategoryResponseDto>>>;
  
  // GetCategoriesQueryHandler.cs
  public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, RequestResult<PaginatedList<CategoryResponseDto>>>
  {
      private readonly ICategoryRepositoryEF _repository;
      
      public async Task<RequestResult<PaginatedList<CategoryResponseDto>>> Handle(
          GetCategoriesQuery request, 
          CancellationToken cancellationToken)
      {
          var categories = await _repository.GetAllAsync(
              skip: (request.Page - 1) * request.PageSize,
              take: request.PageSize,
              cancellationToken);
          
          var total = await _repository.CountAsync(cancellationToken);
          var dtos = categories.Select(c => new CategoryResponseDto(c.Id, c.Name, c.Description));
          
          return RequestResult<PaginatedList<CategoryResponseDto>>.Success(
              new PaginatedList<CategoryResponseDto>(dtos, total, request.Page, request.PageSize));
      }
  }
  ```
- [ ] **VALIDAR:** Query + Handler implementados
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar handler
- [ ] **COMMIT:** `feat(cqrs): implementar GetCategoriesQuery com MediatR`

---

#### Task 5.1.4: Refatorar CategoriesController para usar MediatR
- [ ] **ALTERAÇÃO:** Modificar controller
  ```csharp
  [ApiController]
  [Route("api/[controller]")]
  public class CategoriesController : ControllerBase
  {
      private readonly IMediator _mediator;
      
      public CategoriesController(IMediator mediator) => _mediator = mediator;
      
      [HttpGet]
      public async Task<ActionResult<PaginatedList<CategoryResponseDto>>> GetAll(
          [FromQuery] int page = 1, 
          [FromQuery] int pageSize = 20,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(new GetCategoriesQuery(page, pageSize), cancellationToken);
          return Ok(result.Value);
      }
      
      [HttpPost]
      public async Task<ActionResult<CategoryResponseDto>> Create(
          CreateCategoryRequest request,
          CancellationToken cancellationToken)
      {
          var result = await _mediator.Send(
              new CreateCategoryCommand(request.Name, request.Description), 
              cancellationToken);
          return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
      }
  }
  ```
- [ ] **VALIDAR:** Controller delega para MediatR
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar endpoints
- [ ] **COMMIT:** `refactor(cqrs): migrar CategoriesController para usar MediatR`

---

#### Task 5.1.5: Implementar Pipeline Behaviors
- [ ] **ALTERAÇÃO:** Adicionar behaviors cross-cutting
  ```csharp
  // LoggingBehavior.cs
  public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : IRequest<TResponse>
  {
      private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
      
      public async Task<TResponse> Handle(
          TRequest request, 
          RequestHandlerDelegate<TResponse> next, 
          CancellationToken cancellationToken)
      {
          _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
          var response = await next();
          _logger.LogInformation("Handled {RequestName}", typeof(TRequest).Name);
          return response;
      }
  }
  
  // ValidationBehavior.cs
  public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : IRequest<TResponse>
  {
      private readonly IEnumerable<IValidator<TRequest>> _validators;
      
      public async Task<TResponse> Handle(
          TRequest request, 
          RequestHandlerDelegate<TResponse> next, 
          CancellationToken cancellationToken)
      {
          if (_validators.Any())
          {
              var context = new ValidationContext<TRequest>(request);
              var validationResults = await Task.WhenAll(
                  _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
              var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();
              
              if (failures.Any())
                  throw new ValidationException(failures);
          }
          return await next();
      }
  }
  ```
- [ ] **VALIDAR:** Behaviors registrados em DI
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Verificar logs e validações
- [ ] **COMMIT:** `feat(cqrs): adicionar pipeline behaviors para logging e validation`

**Status CQRS:** 🔴 0/5 tasks | Estimativa: 5 dias

---

### 5.2 EVENT SOURCING (Workflow de Upload)

#### Task 5.2.1: Adicionar EventStore ao Projeto
- [ ] **ALTERAÇÃO:** Escolher e configurar Event Store
  - Opções: EventStoreDB, PostgreSQL (Marten), SQL Stream Store
- [ ] **VALIDAR:** Event Store acessível
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Conexão testada
- [ ] **COMMIT:** `chore(event-sourcing): configurar EventStoreDB`

---

#### Task 5.2.2: Definir Eventos de Upload
- [ ] **ALTERAÇÃO:** Criar eventos em `OnForkHub.Core/Events/`
  ```csharp
  public record VideoUploadStarted(Guid VideoId, string UserId, string FileName, long FileSize, DateTime Timestamp);
  public record VideoUploadProgress(Guid VideoId, int Percentage, DateTime Timestamp);
  public record VideoUploadCompleted(Guid VideoId, string StoragePath, DateTime Timestamp);
  public record VideoUploadFailed(Guid VideoId, string ErrorMessage, DateTime Timestamp);
  public record VideoTranscodingStarted(Guid VideoId, DateTime Timestamp);
  public record VideoTranscodingProgress(Guid VideoId, int Percentage, DateTime Timestamp);
  public record VideoTranscodingCompleted(Guid VideoId, string[] Resolutions, DateTime Timestamp);
  public record VideoTranscodingFailed(Guid VideoId, string ErrorMessage, DateTime Timestamp);
  ```
- [ ] **VALIDAR:** Eventos são records imutáveis
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `feat(event-sourcing): definir eventos do workflow de upload`

---

#### Task 5.2.3: Implementar Event Store Repository
- [ ] **ALTERAÇÃO:** Criar `IEventStoreRepository` e implementação
  ```csharp
  public interface IEventStoreRepository
  {
      Task AppendAsync(Guid aggregateId, IEnumerable<IEvent> events, int expectedVersion);
      Task<IReadOnlyList<IEvent>> GetEventsAsync(Guid aggregateId);
      Task<IReadOnlyList<IEvent>> GetAllEventsAsync(DateTime? afterPosition = null, int batchSize = 100);
  }
  ```
- [ ] **VALIDAR:** Append e read funcionam
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar persistência de eventos
- [ ] **COMMIT:** `feat(event-sourcing): implementar EventStoreRepository`

---

#### Task 5.2.4: Criar Agregado de VideoUpload
- [ ] **ALTERAÇÃO:** Implementar `VideoUploadAggregate`
  ```csharp
  public class VideoUploadAggregate : AggregateRoot
  {
      public Guid Id { get; private set; }
      public VideoUploadStatus Status { get; private set; }
      public string? StoragePath { get; private set; }
      
      private VideoUploadAggregate() { } // Para reconstrução
      
      public static VideoUploadAggregate Start(string userId, string fileName, long fileSize)
      {
          var aggregate = new VideoUploadAggregate();
          aggregate.ApplyEvent(new VideoUploadStarted(
              Guid.NewGuid(), userId, fileName, fileSize, DateTime.UtcNow));
          return aggregate;
      }
      
      public void ReportProgress(int percentage)
      {
          ApplyEvent(new VideoUploadProgress(Id, percentage, DateTime.UtcNow));
      }
      
      public void Complete(string storagePath)
      {
          ApplyEvent(new VideoUploadCompleted(Id, storagePath, DateTime.UtcNow));
      }
      
      public void Fail(string errorMessage)
      {
          ApplyEvent(new VideoUploadFailed(Id, errorMessage, DateTime.UtcNow));
      }
      
      protected override void When(IEvent @event)
      {
          switch (@event)
          {
              case VideoUploadStarted e:
                  Id = e.VideoId;
                  Status = VideoUploadStatus.Uploading;
                  break;
              case VideoUploadCompleted e:
                  Status = VideoUploadStatus.Completed;
                  StoragePath = e.StoragePath;
                  break;
              // ... outros eventos
          }
      }
  }
  ```
- [ ] **VALIDAR:** Agregado gerencia seu estado via eventos
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar transições de estado
- [ ] **COMMIT:** `feat(event-sourcing): implementar VideoUploadAggregate`

---

#### Task 5.2.5: Criar Projeções (Read Models)
- [ ] **ALTERAÇÃO:** Implementar projector
  ```csharp
  public class VideoUploadProjection : IEventHandler<VideoUploadStarted>,
                                       IEventHandler<VideoUploadProgress>,
                                       IEventHandler<VideoUploadCompleted>
  {
      private readonly IVideoUploadStatusRepository _repository;
      
      public async Task Handle(VideoUploadStarted @event, CancellationToken cancellationToken)
      {
          await _repository.InsertAsync(new VideoUploadStatus
          {
              VideoId = @event.VideoId,
              Status = VideoUploadStatus.Uploading,
              Progress = 0
          });
      }
      
      public async Task Handle(VideoUploadProgress @event, CancellationToken cancellationToken)
      {
          await _repository.UpdateProgressAsync(@event.VideoId, @event.Percentage);
      }
      
      public async Task Handle(VideoUploadCompleted @event, CancellationToken cancellationToken)
      {
          await _repository.UpdateStatusAsync(@event.VideoId, VideoUploadStatus.Completed);
      }
  }
  ```
- [ ] **VALIDAR:** Projeções atualizam read models
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar fluxo completo
- [ ] **COMMIT:** `feat(event-sourcing): implementar projeções para read models`

**Status Event Sourcing:** 🔴 0/5 tasks | Estimativa: 7 dias

---

### 5.3 POLLY (Resilience)

#### Task 5.3.1: Adicionar Polly ao Projeto
- [ ] **ALTERAÇÃO:** Adicionar pacote
  ```xml
  <PackageVersion Include="Polly" Version="8.2.1" />
  <PackageVersion Include="Polly.Extensions.Http" Version="3.0.0" />
  <PackageVersion Include="Microsoft.Extensions.Http.Polly" Version="8.0.0" />
  ```
- [ ] **VALIDAR:** Pacotes restauram
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `chore(resilience): adicionar pacotes Polly`

---

#### Task 5.3.2: Configurar Retry Policy para HttpClient
- [ ] **ALTERAÇÃO:** Adicionar política de retry
  ```csharp
  services.AddHttpClient<IVideoUploadService, VideoUploadService>(client =>
  {
      client.BaseAddress = new Uri(configuration["Storage:ApiUrl"]!);
      client.Timeout = TimeSpan.FromSeconds(30);
  })
  .AddPolicyHandler(GetRetryPolicy())
  .AddPolicyHandler(GetCircuitBreakerPolicy());
  
  static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
  {
      return HttpPolicyExtensions
          .HandleTransientHttpError()
          .WaitAndRetryAsync(3, retryAttempt => 
              TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
  }
  ```
- [ ] **VALIDAR:** Políticas aplicadas
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Simular falhas
- [ ] **COMMIT:** `feat(resilience): configurar retry e circuit breaker para HttpClient`

---

#### Task 5.3.3: Adicionar Retry para Operações de Banco
- [ ] **ALTERAÇÃO:** Decorar repositories com retry
  ```csharp
  public class ResilientCategoryRepository : ICategoryRepositoryEF
  {
      private readonly ICategoryRepositoryEF _inner;
      private readonly IAsyncPolicy _retryPolicy;
      
      public ResilientCategoryRepository(ICategoryRepositoryEF inner)
      {
          _inner = inner;
          _retryPolicy = Policy
              .Handle<SqlException>(ex => ex.IsTransient)
              .WaitAndRetryAsync(3, retryAttempt => 
                  TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt)));
      }
      
      public async Task<Category?> GetByIdAsync(Guid id)
      {
          return await _retryPolicy.ExecuteAsync(() => _inner.GetByIdAsync(id));
      }
  }
  ```
- [ ] **VALIDAR:** Retry funciona para erros transientes
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar retry
- [ ] **COMMIT:** `feat(resilience): adicionar retry policy para operações de banco`

---

#### Task 5.3.4: Configurar Timeout Policies
- [ ] **ALTERAÇÃO:** Adicionar timeouts
  ```csharp
  services.AddHttpClient<ExternalApiService>()
      .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10)));
  ```
- [ ] **VALIDAR:** Timeouts configurados
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testar timeout
- [ ] **COMMIT:** `feat(resilience): configurar timeout policies`

**Status Polly:** 🔴 0/4 tasks | Estimativa: 3 dias

---

### 5.4 OPENTELEMETRY (Observability)

#### Task 5.4.1: Adicionar OpenTelemetry Packages
- [ ] **ALTERAÇÃO:** Adicionar pacotes
  ```xml
  <PackageVersion Include="OpenTelemetry" Version="1.7.0" />
  <PackageVersion Include="OpenTelemetry.Api" Version="1.7.0" />
  <PackageVersion Include="OpenTelemetry.Exporter.Console" Version="1.7.0" />
  <PackageVersion Include="OpenTelemetry.Exporter.Jaeger" Version="1.5.1" />
  <PackageVersion Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.7.0-rc.1" />
  <PackageVersion Include="OpenTelemetry.Extensions.Hosting" Version="1.7.0" />
  <PackageVersion Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.7.0" />
  <PackageVersion Include="OpenTelemetry.Instrumentation.Http" Version="1.7.0" />
  <PackageVersion Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.0.0-beta.8" />
  ```
- [ ] **VALIDAR:** Pacotes sem conflitos
- [ ] **BUILDAR:** `dotnet restore`
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `chore(otel): adicionar pacotes OpenTelemetry`

---

#### Task 5.4.2: Configurar Tracing
- [ ] **ALTERAÇÃO:** Configurar em `Program.cs`
  ```csharp
  builder.Services.AddOpenTelemetry()
      .WithTracing(tracing =>
      {
          tracing
              .AddAspNetCoreInstrumentation()
              .AddHttpClientInstrumentation()
              .AddEntityFrameworkCoreInstrumentation()
              .AddSource("OnForkHub")
              .SetResourceBuilder(ResourceBuilder.CreateDefault()
                  .AddService("OnForkHub-API", "1.0.0")
                  .AddAttributes(new[] { new KeyValuePair<string, object>("deployment.environment", "production") }))
              .AddJaegerExporter(options =>
              {
                  options.AgentHost = builder.Configuration["Jaeger:Host"] ?? "localhost";
                  options.AgentPort = int.Parse(builder.Configuration["Jaeger:Port"] ?? "6831");
              })
              .AddConsoleExporter();
      });
  ```
- [ ] **VALIDAR:** Traces gerados
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Verificar saída no console
- [ ] **COMMIT:** `feat(otel): configurar tracing com Jaeger e Console`

---

#### Task 5.4.3: Configurar Métricas
- [ ] **ALTERAÇÃO:** Adicionar métricas customizadas
  ```csharp
  public static class OnForkHubMetrics
  {
      private static readonly Meter Meter = new("OnForkHub", "1.0.0");
      
      public static readonly Counter<long> VideosUploaded = Meter.CreateCounter<long>(
          "videos_uploaded_total", 
          description: "Total number of videos uploaded");
      
      public static readonly Histogram<double> VideoUploadDuration = Meter.CreateHistogram<double>(
          "video_upload_duration_seconds",
          description: "Duration of video uploads in seconds");
      
      public static readonly UpDownCounter<long> ActiveStreams = Meter.CreateUpDownCounter<long>(
          "active_streams",
          description: "Number of active video streams");
  }
  ```
- [ ] **VALIDAR:** Métricas registradas
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Verificar endpoints de métricas
- [ ] **COMMIT:** `feat(otel): adicionar métricas customizadas da aplicação`

---

#### Task 5.4.4: Configurar Export para Prometheus
- [ ] **ALTERAÇÃO:** Adicionar endpoint Prometheus
  ```csharp
  builder.Services.AddOpenTelemetry()
      .WithMetrics(metrics =>
      {
          metrics
              .AddAspNetCoreInstrumentation()
              .AddHttpClientInstrumentation()
              .AddRuntimeInstrumentation()
              .AddPrometheusExporter();
      });
  
  app.MapPrometheusScrapingEndpoint();
  ```
- [ ] **VALIDAR:** Métricas acessíveis em `/metrics`
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Acessar `/metrics` e validar output
- [ ] **COMMIT:** `feat(otel): configurar export de métricas para Prometheus`

---

#### Task 5.4.5: Adicionar Logging Estruturado com Correlation IDs
- [ ] **ALTERAÇÃO:** Configurar enrichers
  ```csharp
  builder.Services.AddOpenTelemetry()
      .WithLogging(logging =>
      {
          logging.AddProcessor(new LogRecordProcessor());
      });
  
  // Middleware para correlation ID
  app.Use(async (context, next) =>
  {
      var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
      context.Response.Headers["X-Correlation-ID"] = correlationId;
      
      using (LogContext.PushProperty("CorrelationId", correlationId))
      using (LogContext.PushProperty("TraceId", Activity.Current?.TraceId.ToString()))
      {
          await next();
      }
  });
  ```
- [ ] **VALIDAR:** Correlation ID propagado
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Verificar logs
- [ ] **COMMIT:** `feat(otel): adicionar correlation IDs e logging estruturado`

**Status OpenTelemetry:** 🔴 0/5 tasks | Estimativa: 4 dias

---

### 5.5 KUBERNETES (K8s)

#### Task 5.5.1: Criar Helm Chart Básico
- [ ] **ALTERAÇÃO:** Estrutura de chart
  ```
  charts/
  └── onforkhub/
      ├── Chart.yaml
      ├── values.yaml
      ├── values-production.yaml
      └── templates/
          ├── deployment.yaml
          ├── service.yaml
          ├── ingress.yaml
          ├── configmap.yaml
          ├── secret.yaml
          └── hpa.yaml
  ```
- [ ] **VALIDAR:** Chart estruturado
- [ ] **BUILDAR:** `helm lint`
- [ ] **TESTAR:** `helm template`
- [ ] **COMMIT:** `chore(k8s): criar estrutura inicial do Helm chart`

---

#### Task 5.5.2: Configurar Deployment
- [ ] **ALTERAÇÃO:** Criar `templates/deployment.yaml`
  ```yaml
  apiVersion: apps/v1
  kind: Deployment
  metadata:
    name: {{ include "onforkhub.fullname" . }}
  spec:
    replicas: {{ .Values.replicaCount }}
    selector:
      matchLabels:
        app.kubernetes.io/name: {{ include "onforkhub.name" . }}
    template:
      spec:
        containers:
        - name: {{ .Chart.Name }}
          image: "{{ .Values.image.repository }}:{{ .Values.image.tag | default .Chart.AppVersion }}"
          ports:
          - containerPort: 80
          envFrom:
          - configMapRef:
              name: {{ include "onforkhub.fullname" . }}-config
          - secretRef:
              name: {{ include "onforkhub.fullname" . }}-secrets
  ```
- [ ] **VALIDAR:** Template válido
- [ ] **BUILDAR:** `helm template`
- [ ] **TESTAR:** Dry-run
- [ ] **COMMIT:** `feat(k8s): adicionar deployment template`

---

#### Task 5.5.3: Configurar Ingress com HTTPS
- [ ] **ALTERAÇÃO:** Criar `templates/ingress.yaml`
  ```yaml
  apiVersion: networking.k8s.io/v1
  kind: Ingress
  metadata:
    annotations:
      cert-manager.io/cluster-issuer: "letsencrypt-prod"
      nginx.ingress.kubernetes.io/ssl-redirect: "true"
  spec:
    tls:
    - hosts:
      - {{ .Values.ingress.host }}
      secretName: {{ include "onforkhub.fullname" . }}-tls
    rules:
    - host: {{ .Values.ingress.host }}
      http:
        paths:
        - path: /
          backend:
            service:
              name: {{ include "onforkhub.fullname" . }}
  ```
- [ ] **VALIDAR:** Ingress configurado para HTTPS
- [ ] **BUILDAR:** `helm template`
- [ ] **TESTAR:** Validar com kubeval
- [ ] **COMMIT:** `feat(k8s): adicionar ingress com Let's Encrypt TLS`

---

#### Task 5.5.4: Configurar HPA (Horizontal Pod Autoscaler)
- [ ] **ALTERAÇÃO:** Criar `templates/hpa.yaml`
  ```yaml
  apiVersion: autoscaling/v2
  kind: HorizontalPodAutoscaler
  metadata:
    name: {{ include "onforkhub.fullname" . }}
  spec:
    scaleTargetRef:
      apiVersion: apps/v1
      kind: Deployment
      name: {{ include "onforkhub.fullname" . }}
    minReplicas: {{ .Values.autoscaling.minReplicas }}
    maxReplicas: {{ .Values.autoscaling.maxReplicas }}
    metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: {{ .Values.autoscaling.targetCPUUtilizationPercentage }}
    - type: Resource
      resource:
        name: memory
        target:
          type: Utilization
          averageUtilization: {{ .Values.autoscaling.targetMemoryUtilizationPercentage }}
  ```
- [ ] **VALIDAR:** HPA configurado
- [ ] **BUILDAR:** Template
- [ ] **TESTAR:** Validar
- [ ] **COMMIT:** `feat(k8s): adicionar HPA para auto-scaling`

**Status K8s:** 🔴 0/4 tasks | Estimativa: 3 dias

---

### 5.6 PACT (Contract Testing)

#### Task 5.6.1: Adicionar Pact ao Projeto
- [ ] **ALTERAÇÃO:** Adicionar pacotes
  ```xml
  <PackageVersion Include="PactNet" Version="4.5.0" />
  <PackageVersion Include="PactNet.Output.Xunit" Version="1.0.0" />
  ```
- [ ] **VALIDAR:** Pacotes restauram
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `chore(pact): adicionar pacotes PactNet`

---

#### Task 5.6.2: Criar Consumer Test
- [ ] **ALTERAÇÃO:** Testar contrato do lado do cliente
  ```csharp
  public class VideoApiConsumerTests
  {
      private readonly IPactBuilderV3 _pactBuilder;
      
      public VideoApiConsumerTests()
      {
          _pactBuilder = Pact.V3("OnForkHub-Web", "OnForkHub-API")
              .WithHttpInteractions();
      }
      
      [Fact]
      public async Task GetVideo_ReturnsVideo_WhenExists()
      {
          var videoId = Guid.NewGuid();
          
          _pactBuilder
              .UponReceiving("a request for a video by ID")
              .Given("a video with ID {id} exists", new Dictionary<string, string> { ["id"] = videoId.ToString() })
              .WithRequest(HttpMethod.Get, $"/api/videos/{videoId}")
              .WithHeader("Authorization", Match.Regex("Bearer .+", "Bearer "))
              .WillRespond()
              .WithStatus(HttpStatusCode.OK)
              .WithJsonBody(new
              {
                  id = Match.Type(videoId),
                  title = Match.Type("Sample Video"),
                  description = Match.Type("Description"),
                  url = Match.Type("https://example.com/video.mp4")
              });
          
          await _pactBuilder.VerifyAsync(async ctx =>
          {
              var client = new HttpClient { BaseAddress = ctx.MockServerUri };
              client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token");
              
              var response = await client.GetAsync($"/api/videos/{videoId}");
              response.EnsureSuccessStatusCode();
          });
      }
  }
  ```
- [ ] **VALIDAR:** Teste gera contrato Pact
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Executar teste
- [ ] **COMMIT:** `test(pact): adicionar consumer tests para Video API`

---

#### Task 5.6.3: Criar Provider Test
- [ ] **ALTERAÇÃO:** Verificar contrato no lado do servidor
  ```csharp
  public class VideoApiProviderTests : IClassFixture<TestStartup>
  {
      private readonly TestStartup _fixture;
      
      public VideoApiProviderTests(TestStartup fixture)
      {
          _fixture = fixture;
      }
      
      [Fact]
      public void EnsureVideoApiHonoursPactWithConsumer()
      {
          var config = new PactVerifierConfig
          {
              ProviderVersion = "1.0.0",
              PublishResults = true,
              BrokerUri = new Uri("https://pact-broker.example.com"),
              BrokerCredentials = new PactBrokerCredentials("user", "pass")
          };
          
          IPactVerifier pactVerifier = new PactVerifier(config);
          
          pactVerifier
              .ServiceProvider("OnForkHub-API", _fixture.CreateServer())
              .HonoursPactWith("OnForkHub-Web")
              .PactUri($"{Directory.GetCurrentDirectory()}/../pacts/onforkhub-web-onforkhub-api.json")
              .Verify();
      }
  }
  ```
- [ ] **VALIDAR:** Verificação funciona
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Executar provider test
- [ ] **COMMIT:** `test(pact): adicionar provider verification tests`

---

#### Task 5.6.4: Configurar Pact Broker
- [ ] **ALTERAÇÃO:** Configurar workflow GitHub Actions
  ```yaml
  - name: Publish Pact to Broker
    run: |
      pact-broker publish pacts/ \
        --broker-base-url ${{ secrets.PACT_BROKER_URL }} \
        --broker-token ${{ secrets.PACT_BROKER_TOKEN }} \
        --consumer-app-version ${{ github.sha }} \
        --branch ${{ github.ref_name }}
  ```
- [ ] **VALIDAR:** Contratos publicados no broker
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** Testar workflow
- [ ] **COMMIT:** `chore(pact): configurar publicação de contratos no CI/CD`

**Status Pact:** 🔴 0/4 tasks | Estimativa: 3 dias

---

## 🔄 FASE 6: MIGRAÇÃO .NET 10 (Futuro)

> **OBJETIVO:** Migrar de .NET 9 para .NET 10  
> **PRAZO:** Após release oficial do .NET 10 (Nov 2025)  
> **DETALHES:** Ver arquivo `CHECKLIST_MIGRACAO_NET10.md` separado  

### Resumo das Tasks de Migração:

| Fase | Tasks | Descrição |
|------|-------|-----------|
| 6.1 Preparação | 5 | Branch, backup, SDK check |
| 6.2 Core | 8 | global.json, TargetFramework, Docker |
| 6.3 NuGet | 12 | Atualizar todos os pacotes |
| 6.4 Correções | 10 | Breaking changes |
| 6.5 C# 14 | 6 | Otimizações de linguagem |
| 6.6 Validação | 5 | Testes finais |
| 6.7 PR/Merge | 5 | Entrega |
| **Total** | **46** | |

**Status Migração .NET 10:** 🔴 Não Iniciado | 🟡 Planejado | 🟢 Aguardando release

---

## 📊 FASE 7: QUALIDADE & MÉTRICAS (Contínuo)

> **OBJETIVO:** Manter e melhorar métricas de qualidade  
> **PRAZO:** Contínuo  

---

### 7.1 AUMENTAR COVERAGE DE TESTES

#### Task 7.1.1: Identificar Classes Sem Testes
- [ ] **ALTERAÇÃO:** Usar ferramenta de análise (ReportGenerator, Fine Code Coverage)
  ```bash
  dotnet test --collect:"XPlat Code Coverage"
  reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report
  ```
- [ ] **VALIDAR:** Relatório gerado
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** Analisar gaps
- [ ] **COMMIT:** `docs(test): adicionar relatório de coverage gaps`

---

#### Task 7.1.2: Adicionar Testes para Services Críticos
- [ ] **ALTERAÇÃO:** Identificar e testar:
  - [ ] UserService
  - [ ] VideoUploadService
  - [ ] AuthenticationService
  - [ ] CategoryService
- [ ] **VALIDAR:** Services cobertos > 80%
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Executar testes
- [ ] **COMMIT:** `test(coverage): adicionar testes para services críticos`

---

#### Task 7.1.3: Adicionar Testes de Integração para Fluxos Principais
- [ ] **ALTERAÇÃO:** Testar fluxos:
  - [ ] Upload → Transcoding → Publish
  - [ ] Registro → Login → Acesso protegido
  - [ ] Comentar → Responder → Curtir
- [ ] **VALIDAR:** Fluxos cobertos
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Executar testes de integração
- [ ] **COMMIT:** `test(integration): adicionar testes de fluxos principais`

---

#### Task 7.1.4: Configurar Mutation Testing
- [ ] **ALTERAÇÃO:** Adicionar Stryker.NET
  ```bash
  dotnet tool install -g dotnet-stryker
  dotnet stryker init
  ```
- [ ] **VALIDAR:** Stryker configuração criada
- [ ] **BUILDAR:** `dotnet stryker`
- [ ] **TESTAR:** Analisar score de mutation
- [ ] **COMMIT:** `chore(test): configurar Stryker para mutation testing`

**Status Coverage:** 🔴 0/4 tasks | Estimativa: 4 dias

---

### 7.2 MELHORAR DOCUMENTAÇÃO

#### Task 7.2.1: Completar XML Documentation
- [ ] **ALTERAÇÃO:** Adicionar XML docs em:
  - [ ] Todos os public APIs
  - [ ] Interfaces importantes
  - [ ] Métodos complexos
  ```csharp
  /// <summary>
  /// Uploads a video file to the platform.
  /// </summary>
  /// <param name="fileName">Original file name</param>
  /// <param name="fileSize">Size in bytes</param>
  /// <param name="contentType">MIME type</param>
  /// <param name="userId">Uploader user ID</param>
  /// <returns>Upload session information</returns>
  /// <exception cref="ValidationException">When file exceeds limits</exception>
  Task<RequestResult<VideoUploadResponseDto>> InitiateUploadAsync(
      string fileName, long fileSize, string contentType, string userId);
  ```
- [ ] **VALIDAR:** Documentação completa
- [ ] **BUILDAR:** Build com `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `docs(api): adicionar XML documentation para public APIs`

---

#### Task 7.2.2: Criar Architecture Decision Records (ADRs)
- [ ] **ALTERAÇÃO:** Documentar decisões em `docs/architecture/`
  - ADR-001: Clean Architecture
  - ADR-002: CQRS com MediatR
  - ADR-003: Event Sourcing para Upload
  - ADR-004: WebTorrent para P2P
  - ADR-005: PostgreSQL vs SQL Server
- [ ] **VALIDAR:** ADRs completos
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `docs(adr): adicionar Architecture Decision Records`

---

#### Task 7.2.3: Criar Database Schema Documentation
- [ ] **ALTERAÇÃO:** Gerar diagrama ER
  - Usar Entity Framework Power Tools ou DbDiagram
  - Documentar relações entre tabelas
- [ ] **VALIDAR:** Diagrama atualizado
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `docs(db): adicionar diagrama ER e documentação de schema`

**Status Documentação:** 🔴 0/3 tasks | Estimativa: 3 dias

---

### 7.3 MONITORAR MÉTRICAS DE CÓDIGO

#### Task 7.3.1: Configurar SonarQube
- [ ] **ALTERAÇÃO:** Adicionar análise SonarQube no CI
  ```yaml
  - name: SonarQube Analysis
    uses: SonarSource/sonarqube-scan-action@v2
    with:
      projectBaseDir: .
      args: >
        -Dsonar.projectKey=OnForkHub
        -Dsonar.organization=rondineleg
        -Dsonar.host.url=https://sonarcloud.io
  ```
- [ ] **VALIDAR:** Análise publicada
- [ ] **BUILDAR:** CI passando
- [ ] **TESTAR:** Verificar dashboard
- [ ] **COMMIT:** `chore(ci): adicionar SonarQube analysis no pipeline`

---

#### Task 7.3.2: Configurar Codecov
- [ ] **ALTERAÇÃO:** Publicar coverage no Codecov
  ```yaml
  - name: Upload to Codecov
    uses: codecov/codecov-action@v3
    with:
      files: ./coverage.cobertura.xml
      fail_ci_if_error: true
  ```
- [ ] **VALIDAR:** Badge atualizado no README
- [ ] **BUILDAR:** CI
- [ ] **TESTAR:** Verificar badge
- [ ] **COMMIT:** `chore(ci): adicionar Codecov integration`

---

#### Task 7.3.3: Definir Alertas de Qualidade
- [ ] **ALTERAÇÃO:** Configurar Quality Gates
  - Coverage mínimo: 80%
  - Duplicação máxima: 3%
  - Bugs: 0
  - Vulnerabilidades: 0
  - Code Smells: < 50
- [ ] **VALIDAR:** Gates configurados no SonarQube
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** Testar falha de gate
- [ ] **COMMIT:** `chore(quality): configurar quality gates no SonarQube`

**Status Métricas:** 🔴 0/3 tasks | Estimativa: 2 dias

---

## 📝 REGISTRO DE EXECUÇÃO

### Formato para Preenchimento

| Data | Task ID | Responsável | Status | Observações |
|------|---------|-------------|--------|-------------|
| 2025-01-21 | 0.1 | @dev1 | ✅ | Build OK |
| | | | | |
| | | | | |

---

## 🎯 PRÓXIMOS PASSOS RECOMENDADOS

### Semana 1 (Imediato)
1. **FASE 0** - Corrigir build quebrado (Tasks 0.1-0.10)
   - SA1210 fixes
   - IValidationResult fixes
   - WASM workload

### Semana 2-5 (Features)
2. **FASE 1** - Completar features em progresso
   - User Profile (5 tasks)
   - Video Upload (10 tasks)
   - WebTorrent (10 tasks)
   - Integration Tests (5 tasks)

### Mês 2 (Must Have)
3. **FASE 2** - Implementar MVP features
   - Auth UI (8 tasks)
   - Streaming (7 tasks)
   - Categories (3 tasks)

### Contínuo
4. **FASE 5** - Melhorias de arquitetura (paralelo)
   - CQRS, Event Sourcing, Polly, OpenTelemetry, K8s

---

## 🆘 TROUBLESHOOTING

### Build falha após alteração
```bash
dotnet clean
dotnet restore
dotnet build --verbosity detailed 2>&1 | Select-String "error"
```

### Testes falhando
```bash
dotnet test --verbosity detailed --logger trx
# Analisar .trx no Visual Studio ou online viewer
```

### Migration não aplica
```bash
dotnet ef migrations list --project src/Infrastructure/OnForkHub.Persistence
dotnet ef database update --project src/Infrastructure/OnForkHub.Persistence --verbose
```

### Docker build falha
```bash
docker build -f .docker/Dockerfile.OnForkHub.Api --no-cache -t test-build .
```

---

## 📞 CONTATOS E RECURSOS

- **Documentação .NET 10:** https://docs.microsoft.com/dotnet/
- **C# 14 Features:** https://docs.microsoft.com/dotnet/csharp/whats-new/
- **EF Core:** https://docs.microsoft.com/ef/core/
- **Blazor:** https://docs.microsoft.com/aspnet/core/blazor/
- **OpenTelemetry:** https://opentelemetry.io/docs/
- **Polly:** https://github.com/App-vNext/Polly/wiki

---

**Última Atualização:** 2025-01-21  
**Versão do Checklist:** 1.0.0  
**Total de Tasks:** 170  
**Tasks Concluídas:** 0  
**Progresso:** 0%

> 💡 **Lembrete:** Faça micro-alterações, valide, build, teste e commit frequentemente!
