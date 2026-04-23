# 📋 Checklist de Migração para .NET 10

> **Projeto:** OnForkHub  
> **Data Início:** 2025-01-21  
> **Branch:** `feature/migrate-net10`  
> **Responsável:** _[Preencher]_

---

## 🎯 Objetivo

Migrar o projeto OnForkHub de **.NET 9** para **.NET 10** com **zero downtime** no desenvolvimento, mantendo todos os 372 testes passando e aproveitando novos recursos do C# 14.

---

## 📜 Regras para Cada Task

```
┌─────────────────────────────────────────────────────────────┐
│  FLUXO OBRIGATÓRIO PARA CADA ALTERAÇÃO:                    │
│                                                             │
│  1. MICRO ALTERAÇÃO → Máximo 5 arquivos por commit          │
│  2. VALIDAR   → dotnet format + análise estática           │
│  3. BUILDAR   → dotnet build (0 erros, 0 warnings)         │
│  4. TESTAR    → dotnet test (todos passando)               │
│  5. COMMIT    → Convenção: "feat(migrate): [descrição]"     │
└─────────────────────────────────────────────────────────────┘
```

---

## 📊 Status Geral

| Fase | Progresso | Status |
|------|-----------|--------|
| Fase 1: Preparação | 0/5 | 🔴 Não Iniciado |
| Fase 2: Atualização Core | 0/8 | 🔴 Não Iniciado |
| Fase 3: Pacotes NuGet | 0/12 | 🔴 Não Iniciado |
| Fase 4: Correções Breaking Changes | 0/10 | 🔴 Não Iniciado |
| Fase 5: Otimizações C# 14 | 0/6 | 🔴 Não Iniciado |
| Fase 6: Validação Final | 0/5 | 🔴 Não Iniciado |

**Progresso Total:** 0/46 tasks (0%)

---

## 🔧 FASE 1: PREPARAÇÃO (Branch e Backup)

### Task 1.1: Criar Branch de Migração
- [ ] **ALTERAÇÃO:** Criar branch `feature/migrate-net10` a partir de `dev`
  ```bash
  git checkout dev
  git pull origin dev
  git checkout -b feature/migrate-net10
  ```
- [ ] **VALIDAR:** Branch criada corretamente
  ```bash
  git branch --show-current
  ```
- [ ] **BUILDAR:** N/A (apenas git)
- [ ] **TESTAR:** N/A (apenas git)
- [ ] **COMMIT:** N/A (apenas branch)

---

### Task 1.2: Backup do global.json Atual
- [ ] **ALTERAÇÃO:** Criar cópia de segurança do `global.json`
  ```bash
  Copy-Item global.json global.json.backup
  ```
- [ ] **VALIDAR:** Arquivo backup existe
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `docs(migrate): backup global.json antes da migração`

---

### Task 1.3: Verificar Versão do SDK .NET 10
- [ ] **ALTERAÇÃO:** Listar SDKs instalados
  ```bash
  dotnet --list-sdks
  ```
- [ ] **VALIDAR:** SDK 10.0.xxx está instalado
  - Se NÃO: instalar via `winget install Microsoft.DotNet.SDK.10`
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** N/A (documentar em comentário)

---

### Task 1.4: Análise de Dependências Atuais
- [ ] **ALTERAÇÃO:** Gerar relatório de pacotes atuais
  ```bash
  dotnet list package --format json > packages-before.json
  ```
- [ ] **VALIDAR:** Arquivo `packages-before.json` gerado
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `docs(migrate): snapshot de pacotes NuGet atuais`

---

### Task 1.5: Verificar Status Atual do Build
- [ ] **ALTERAÇÃO:** Limpar e restaurar packages
  ```bash
  dotnet clean
  dotnet restore
  ```
- [ ] **VALIDAR:** Restore completo sem erros
- [ ] **BUILDAR:** Compilar projeto atual
  ```bash
  dotnet build --no-restore 2>&1 | Tee-Object build-before.log
  ```
- [ ] **TESTAR:** Executar todos os testes atuais
  ```bash
  dotnet test --no-build --verbosity minimal 2>&1 | Tee-Object tests-before.log
  ```
  - [ ] Validar: 372 testes passando
- [ ] **COMMIT:** N/A (documentar baseline)

**Status Fase 1:** 🔴 Não Iniciado | 🟡 Em Progresso | 🟢 Concluído

---

## 🔄 FASE 2: ATUALIZAÇÃO CORE (Target Framework)

### Task 2.1: Atualizar global.json para .NET 10
- [ ] **ALTERAÇÃO:** Modificar `global.json`
  ```json
  {
    "sdk": {
      "version": "10.0.100",
      "rollForward": "latestPatch"
    }
  }
  ```
- [ ] **VALIDAR:** Formato JSON válido
  ```bash
  Get-Content global.json | ConvertFrom-Json
  ```
- [ ] **BUILDAR:** N/A (apenas config)
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `feat(migrate): atualizar global.json para .NET 10 SDK`

---

### Task 2.2: Atualizar Directory.Build.props - TargetFramework
- [ ] **ALTERAÇÃO:** Modificar `Directory.Build.props`
  ```xml
  <TargetFramework>net10.0</TargetFramework>
  ```
  - Localizar: `<TargetFramework>net9.0</TargetFramework>`
  - Substituir por: `<TargetFramework>net10.0</TargetFramework>`
- [ ] **VALIDAR:** XML válido
- [ ] **BUILDAR:** Testar restore
  ```bash
  dotnet restore
  ```
  - [ ] Esperado: Warnings sobre versões de pacotes incompatíveis (aceitável)
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `feat(migrate): atualizar TargetFramework para net10.0`

---

### Task 2.3: Atualizar Projetos de Teste (Multi-targeting temporário)
- [ ] **ALTERAÇÃO:** Modificar projetos de teste para multi-targeting
  - Arquivos:
    - `test/Core/OnForkHub.Core.Test/OnForkHub.Core.Test.csproj`
    - `test/Core/OnForkHub.Application.Test/OnForkHub.Application.Test.csproj`
    - `test/Persistence/OnForkHub.Persistence.Test/OnForkHub.Persistence.Test.csproj`
    - `test/Presentations/OnForkHub.Api.IntegrationTests/OnForkHub.Api.IntegrationTests.csproj`
    - `test/Shared/OnForkHub.CrossCutting.Tests/OnForkHub.CrossCutting.Tests.csproj`
    - `test/Shared/OnForkHub.TestExtensions/OnForkHub.TestExtensions.csproj`
  ```xml
  <TargetFrameworks>net9.0;net10.0</TargetFrameworks>
  ```
- [ ] **VALIDAR:** XML válido em todos os arquivos
- [ ] **BUILDAR:** Testar build
  ```bash
  dotnet build --framework net10.0 test/Core/OnForkHub.Core.Test
  ```
- [ ] **TESTAR:** Executar testes em ambos frameworks
  ```bash
  dotnet test test/Core/OnForkHub.Core.Test --framework net10.0
  ```
- [ ] **COMMIT:** `feat(migrate): adicionar multi-targeting net10.0 nos projetos de teste`

---

### Task 2.4: Atualizar LangVersion para C# 14
- [ ] **ALTERAÇÃO:** Modificar `Directory.Build.props`
  ```xml
  <LangVersion>14.0</LangVersion>
  ```
  - Ou: `<LangVersion>preview</LangVersion>` (se 14.0 ainda não estável)
- [ ] **VALIDAR:** Sintaxe XML correta
- [ ] **BUILDAR:** Compilar um projeto simples
  ```bash
  dotnet build src/Core/OnForkHub.Core
  ```
- [ ] **TESTAR:** Verificar se compila com nova versão
- [ ] **COMMIT:** `feat(migrate): atualizar LangVersion para C# 14`

---

### Task 2.5: Corrigir Warnings de Obsolescência (Passo 1)
- [ ] **ALTERAÇÃO:** Identificar warnings de obsolescência
  ```bash
  dotnet build --no-restore 2>&1 | Select-String "warning|obsolete" | Tee-Object obsolete-warnings.log
  ```
- [ ] **VALIDAR:** Log criado com warnings identificados
- [ ] **BUILDAR:** N/A (apenas diagnóstico)
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `docs(migrate): documentar warnings de obsolescência iniciais`

---

### Task 2.6: Atualizar Projetos Blazor WASM
- [ ] **ALTERAÇÃO:** Verificar workload wasm-tools
  ```bash
  dotnet workload list
  ```
- [ ] **VALIDAR:** Workload `wasm-tools` versão 10.0.xxx instalado
  - Se NÃO: `dotnet workload install wasm-tools`
- [ ] **BUILDAR:** Compilar projeto Web
  ```bash
  dotnet build src/Presentations/OnForkHub.Web
  ```
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `feat(migrate): atualizar workload wasm-tools para .NET 10`

---

### Task 2.7: Verificar Dockerfiles
- [ ] **ALTERAÇÃO:** Atualizar imagens base nos Dockerfiles
  - `Dockerfile.OnForkHub.Api`:
    ```dockerfile
    FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
    FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
    ```
  - `Dockerfile.OnForkHub.Web`:
    ```dockerfile
    FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
    FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
    ```
- [ ] **VALIDAR:** Sintaxe Dockerfile correta
- [ ] **BUILDAR:** Testar build Docker (se Docker disponível)
  ```bash
  docker build -f .docker/Dockerfile.OnForkHub.Api -t onforkhub-api:test .
  ```
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `feat(migrate): atualizar imagens Docker para .NET 10`

---

### Task 2.8: Atualizar DevContainer
- [ ] **ALTERAÇÃO:** Modificar `.devcontainer/Dockerfile`
  ```dockerfile
  FROM mcr.microsoft.com/dotnet/sdk:10.0
  ```
- [ ] **VALIDAR:** Dockerfile válido
- [ ] **BUILDAR:** N/A (opcional: testar devcontainer)
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `feat(migrate): atualizar DevContainer para .NET 10 SDK`

**Status Fase 2:** 🔴 Não Iniciado | 🟡 Em Progresso | 🟢 Concluído

---

## 📦 FASE 3: ATUALIZAÇÃO PACOTES NUGET

### Task 3.1: Atualizar Pacotes Microsoft.Extensions.*
- [ ] **ALTERAÇÃO:** Atualizar versões no `Directory.Packages.props`
  - Localizar: `Version="9.0.2"` (Microsoft.Extensions)
  - Substituir por: `Version="10.0.0"` (ou latest preview)
  - Pacotes afetados (~11):
    - Microsoft.Extensions.DependencyInjection
    - Microsoft.Extensions.DependencyInjection.Abstractions
    - Microsoft.Extensions.Configuration
    - Microsoft.Extensions.Configuration.Abstractions
    - Microsoft.Extensions.Configuration.Binder
    - Microsoft.Extensions.Configuration.Json
    - Microsoft.Extensions.Configuration.EnvironmentVariables
    - Microsoft.Extensions.Logging
    - Microsoft.Extensions.Logging.Abstractions
    - Microsoft.Extensions.Logging.Console
    - Microsoft.Extensions.Options
    - Microsoft.Extensions.Options.ConfigurationExtensions
    - Microsoft.Extensions.Caching.Memory
    - Microsoft.Extensions.Caching.StackExchangeRedis
    - Microsoft.Extensions.Hosting
- [ ] **VALIDAR:** XML válido
- [ ] **BUILDAR:** Restaurar packages
  ```bash
  dotnet restore
  ```
- [ ] **TESTAR:** Compilar projeto Core
  ```bash
  dotnet build src/Core/OnForkHub.Core
  ```
- [ ] **COMMIT:** `feat(migrate): atualizar Microsoft.Extensions.* para v10.0.0`

---

### Task 3.2: Atualizar EF Core 9 → EF Core 10
- [ ] **ALTERAÇÃO:** Atualizar pacotes EF Core
  - `Microsoft.EntityFrameworkCore`: 9.0.6 → 10.0.0
  - `Microsoft.EntityFrameworkCore.Design`: 9.0.0 → 10.0.0
  - `Microsoft.EntityFrameworkCore.SqlServer`: 9.0.0 → 10.0.0
  - `Microsoft.EntityFrameworkCore.Tools`: 9.0.0 → 10.0.0
  - `Microsoft.EntityFrameworkCore.InMemory`: 9.0.0 → 10.0.0
  - `Microsoft.EntityFrameworkCore.Sqlite`: 9.0.0 → 10.0.0
- [ ] **VALIDAR:** Versões consistentes (todos 10.0.0)
- [ ] **BUILDAR:** Compilar Persistence
  ```bash
  dotnet build src/Infrastructure/OnForkHub.Persistence
  ```
- [ ] **TESTAR:** Executar testes de Persistence
  ```bash
  dotnet test test/Persistence/OnForkHub.Persistence.Test
  ```
- [ ] **COMMIT:** `feat(migrate): atualizar Entity Framework Core 9 → 10`

---

### Task 3.3: Atualizar ASP.NET Core 9 → ASP.NET Core 10
- [ ] **ALTERAÇÃO:** Atualizar pacotes ASP.NET Core
  - `Microsoft.AspNetCore.Components.Web`: 9.0.0 → 10.0.0
  - `Microsoft.AspNetCore.Components.WebAssembly`: 9.0.6 → 10.0.0
  - `Microsoft.AspNetCore.Components.WebAssembly.DevServer`: 9.0.6 → 10.0.0
  - `Microsoft.AspNetCore.Authorization`: 9.0.6 → 10.0.0
  - `Microsoft.AspNetCore.Components.Authorization`: 9.0.6 → 10.0.0
  - `Microsoft.AspNetCore.Components.WebAssembly.Authentication`: 9.0.0 → 10.0.0
  - `Microsoft.AspNetCore.Authentication.JwtBearer`: 9.0.0 → 10.0.0
  - `Microsoft.AspNetCore.OpenApi`: 9.0.0 → 10.0.0
  - `Microsoft.AspNetCore.Identity.EntityFrameworkCore`: 9.0.2 → 10.0.0
  - `Microsoft.AspNetCore.Mvc.Testing`: 9.0.0 → 10.0.0
- [ ] **VALIDAR:** Versões consistentes
- [ ] **BUILDAR:** Compilar Api e Web
  ```bash
  dotnet build src/Presentations/OnForkHub.Api
  dotnet build src/Presentations/OnForkHub.Web
  ```
- [ ] **TESTAR:** Executar testes de API
  ```bash
  dotnet test test/Presentations/OnForkHub.Api.IntegrationTests
  ```
- [ ] **COMMIT:** `feat(migrate): atualizar ASP.NET Core 9 → 10`

---

### Task 3.4: Atualizar Analyzers
- [ ] **ALTERAÇÃO:** Atualizar analyzers
  - `Microsoft.CodeAnalysis.NetAnalyzers`: 9.0.0 → 10.0.0
  - `StyleCop.Analyzers`: manter 1.2.0-beta.556 (verificar compatibilidade)
- [ ] **VALIDAR:** Analyzers compatíveis com .NET 10
- [ ] **BUILDAR:** Compilar solução completa
  ```bash
  dotnet build --no-restore 2>&1 | Select-String "analyzer|warning" | Tee-Object analyzer-warnings.log
  ```
- [ ] **TESTAR:** Verificar se novos analyzers funcionam
- [ ] **COMMIT:** `feat(migrate): atualizar NetAnalyzers para v10.0.0`

---

### Task 3.5: Atualizar System.* Packages
- [ ] **ALTERAÇÃO:** Atualizar System packages
  - `System.Text.Json`: 9.0.0 → 10.0.0
  - `System.Net.Http.Json`: 9.0.0 → 10.0.0
  - `System.IdentityModel.Tokens.Jwt`: 8.0.1 → verificar compatibilidade com .NET 10
- [ ] **VALIDAR:** Versões atualizadas
- [ ] **BUILDAR:** Compilar solução
- [ ] **TESTAR:** Todos os testes
  ```bash
  dotnet test --verbosity minimal
  ```
- [ ] **COMMIT:** `feat(migrate): atualizar System packages para v10.0.0`

---

### Task 3.6: Verificar Pacotes de Terceiros (Parte 1)
- [ ] **ALTERAÇÃO:** Verificar compatibilidade dos seguintes pacotes:
  - `HotChocolate` 15.1.5 → verificar versão .NET 10
  - `HotChocolate.AspNetCore` 15.1.5
  - `HotChocolate.Subscriptions.InMemory` 15.1.0
  - `HotChocolate.Data.EntityFramework` 15.1.0
  - `Asp.Versioning.Http` 8.1.0
  - `Asp.Versioning.Mvc.ApiExplorer` 8.1.0
- [ ] **VALIDAR:** Consultar [nuget.org](https://nuget.org) para versões compatíveis
- [ ] **BUILDAR:** Se houver updates, compilar
- [ ] **TESTAR:** Testar endpoints GraphQL
- [ ] **COMMIT:** `feat(migrate): verificar/atualizar HotChocolate para .NET 10`

---

### Task 3.7: Verificar Pacotes de Terceiros (Parte 2)
- [ ] **ALTERAÇÃO:** Verificar compatibilidade:
  - `FluentAssertions` 8.3.0 → verificar v9.x
  - `NSubstitute` 5.3.0 → verificar compatibilidade
  - `xunit` 2.9.3 → verificar v3.x (major upgrade!)
  - `xunit.runner.visualstudio` 2.8.2
  - `coverlet.collector` 6.0.2
  - `coverlet.msbuild` 6.0.2
- [ ] **VALIDAR:** Testar se versões atuais funcionam
- [ ] **BUILDAR:** Compilar testes
- [ ] **TESTAR:** Executar todos os testes
- [ ] **COMMIT:** `feat(migrate): verificar pacotes de teste com .NET 10`

---

### Task 3.8: Verificar RavenDB e CosmosDB
- [ ] **ALTERAÇÃO:** Verificar compatibilidade:
  - `RavenDB.Client` 6.2.4
  - `Microsoft.Azure.Cosmos` 3.43.0
- [ ] **VALIDAR:** Consultar release notes
- [ ] **BUILDAR:** Compilar Persistence
- [ ] **TESTAR:** Testes de Persistence
- [ ] **COMMIT:** `feat(migrate): verificar RavenDB e CosmosDB com .NET 10`

---

### Task 3.9: Verificar GraphQL (não HotChocolate)
- [ ] **ALTERAÇÃO:** Verificar compatibilidade:
  - `GraphQL` 7.8.0
  - `GraphQL.Server.Core` 5.2.2
- [ ] **VALIDAR:** Consultar documentação
- [ ] **BUILDAR:** Compilar projetos
- [ ] **TESTAR:** Testes relacionados
- [ ] **COMMIT:** `feat(migrate): verificar GraphQL packages`

---

### Task 3.10: Verificar Swashbuckle / OpenAPI
- [ ] **ALTERAÇÃO:** Verificar compatibilidade Swashbuckle:
  - `Swashbuckle.AspNetCore` 7.2.0
  - `Swashbuckle.AspNetCore.Swagger` 7.2.0
  - `Swashbuckle.AspNetCore.SwaggerGen` 7.2.0
  - `Swashbuckle.AspNetCore.SwaggerUI` 7.2.0
- [ ] **VALIDAR:** Nota: .NET 10 tem OpenAPI built-in, considerar remover Swashbuckle
- [ ] **BUILDAR:** Compilar API
- [ ] **TESTAR:** Verificar se Swagger funciona
- [ ] **COMMIT:** `feat(migrate): verificar Swashbuckle compatibilidade`

---

### Task 3.11: Verificar BCrypt e outros utilitários
- [ ] **ALTERAÇÃO:** Verificar:
  - `BCrypt.Net-Next` 4.0.3
  - `Microsoft.TypeScript.MSBuild` 5.8.3
  - `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` 1.21.0
- [ ] **VALIDAR:** Consultar compatibilidade
- [ ] **BUILDAR:** Compilar
- [ ] **TESTAR:** Testes de autenticação
- [ ] **COMMIT:** `feat(migrate): verificar utilitários BCrypt e TypeScript`

---

### Task 3.12: Documentar Pacotes Não Atualizáveis
- [ ] **ALTERAÇÃO:** Criar arquivo `docs/NET10_MIGRATION_PACKAGES.md`
  - Listar pacotes que ficarão na versão 9.x temporariamente
  - Explicar razões (aguardando updates de terceiros)
- [ ] **VALIDAR:** Documento criado
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `docs(migrate): documentar pacotes pendentes de atualização`

**Status Fase 3:** 🔴 Não Iniciado | 🟡 Em Progresso | 🟢 Concluído

---

## 🛠️ FASE 4: CORREÇÕES BREAKING CHANGES

### Task 4.1: Corrigir System.Text.Json Breaking Changes
- [ ] **ALTERAÇÃO:** Verificar uso de APIs obsoletas em:
  - `src/Core/OnForkHub.Core`
  - `src/Core/OnForkHub.Application`
- [ ] **VALIDAR:** Buscar por `JsonSerializerOptions` com propriedades obsoletas
- [ ] **BUILDAR:** Compilar Core
  ```bash
  dotnet build src/Core/OnForkHub.Core 2>&1 | Select-String "error|obsolete"
  ```
- [ ] **TESTAR:** Testes de serialização
  ```bash
  dotnet test test/Core/OnForkHub.Core.Test --filter "Serialization"
  ```
- [ ] **COMMIT:** `fix(migrate): corrigir breaking changes System.Text.Json`

---

### Task 4.2: Corrigir EF Core Breaking Changes
- [ ] **ALTERAÇÃO:** Verificar queries que podem ter mudado comportamento
  - Buscar por `ExecuteUpdate` e `ExecuteDelete`
  - Verificar `AsNoTracking` behavior
- [ ] **VALIDAR:** Analisar logs de build
- [ ] **BUILDAR:** Compilar Persistence
- [ ] **TESTAR:** Testes de EF Core
  ```bash
  dotnet test test/Persistence/OnForkHub.Persistence.Test --verbosity normal
  ```
- [ ] **COMMIT:** `fix(migrate): corrigir breaking changes EF Core 10`

---

### Task 4.3: Corrigir ASP.NET Core Breaking Changes
- [ ] **ALTERAÇÃO:** Verificar:
  - Minimal APIs (mudanças em `MapGet`, `MapPost`)
  - Authentication/Authorization (JWT changes)
  - Rate Limiting APIs
- [ ] **VALIDAR:** Compilar API
- [ ] **BUILDAR:** Build completo
  ```bash
  dotnet build src/Presentations/OnForkHub.Api
  ```
- [ ] **TESTAR:** Testes de integração
  ```bash
  dotnet test test/Presentations/OnForkHub.Api.IntegrationTests
  ```
- [ ] **COMMIT:** `fix(migrate): corrigir breaking changes ASP.NET Core 10`

---

### Task 4.4: Corrigir Blazor WASM Breaking Changes
- [ ] **ALTERAÇÃO:** Verificar:
  - Render modes
  - Component lifecycle
  - JavaScript interop
- [ ] **VALIDAR:** Compilar Web
- [ ] **BUILDAR:** Build Web
  ```bash
  dotnet build src/Presentations/OnForkHub.Web
  ```
- [ ] **TESTAR:** N/A (testes manuais depois)
- [ ] **COMMIT:** `fix(migrate): corrigir breaking changes Blazor WASM`

---

### Task 4.5: Corrigir ValidationService (IValidationResult)
- [ ] **ALTERAÇÃO:** Resolver problemas de interface em:
  - `test/Core/OnForkHub.Application.Test/UseCases/Videos/UpdateVideoUseCaseTest.cs`
- [ ] **VALIDAR:** Implementar métodos faltantes de `IValidationResult`
  - `AddError`
  - `AddErrorIf`
  - `GetMetadata<T>`
  - `Merge`
  - `ThrowIfInvalid`
  - `ThrowIfInvalidAndReturn`
  - `ThrowIfInvalidAsync`
  - `ValidateAsync`
  - Propriedade `Errors`
  - Propriedade `HasError`
  - Propriedade `Metadata`
- [ ] **BUILDAR:** Compilar testes
  ```bash
  dotnet build test/Core/OnForkHub.Application.Test
  ```
- [ ] **TESTAR:** Executar testes de Video
  ```bash
  dotnet test test/Core/OnForkHub.Application.Test --filter "Video"
  ```
- [ ] **COMMIT:** `fix(migrate): corrigir implementação IValidationResult nos testes`

---

### Task 4.6: Corrigir StyleCop Warnings (SA1210)
- [ ] **ALTERAÇÃO:** Ordenar usings em:
  - `src/Shared/OnForkHub.Scripts/GlobalUsings.cs`
  - `test/Shared/OnForkHub.TestExtensions/GlobalUsings.cs`
  - `test/Persistence/OnForkHub.Persistence.Test/GlobalUsings.cs`
- [ ] **VALIDAR:** Ordem alfabética por namespace
- [ ] **BUILDAR:** Compilar
  ```bash
  dotnet build --no-restore 2>&1 | Select-String "SA1210"
  ```
  - [ ] Esperado: 0 ocorrências
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `style(migrate): corrigir SA1210 (usings ordenados alfabeticamente)`

---

### Task 4.7: Corrigir IValidationError Missing
- [ ] **ALTERAÇÃO:** Adicionar `using` ou implementação de `IValidationError` em:
  - `test/Core/OnForkHub.Application.Test/UseCases/Videos/UpdateVideoUseCaseTest.cs` (linhas 246, 251)
- [ ] **VALIDAR:** Compilar testes
- [ ] **BUILDAR:** Build testes
  ```bash
  dotnet build test/Core/OnForkHub.Application.Test
  ```
  - [ ] Esperado: 0 erros
- [ ] **TESTAR:** Testes de Video
- [ ] **COMMIT:** `fix(migrate): adicionar using IValidationError em testes de Video`

---

### Task 4.8: Remover Multi-targeting (Cleanup)
- [ ] **ALTERAÇÃO:** Remover `net9.0` dos projetos de teste
  - Voltar para `<TargetFramework>net10.0</TargetFramework>` apenas
  - Projetos afetados: 6 projetos de teste
- [ ] **VALIDAR:** XML válido
- [ ] **BUILDAR:** Build todos os testes
  ```bash
  dotnet build test/
  ```
- [ ] **TESTAR:** Todos os testes
  ```bash
  dotnet test test/ --verbosity minimal
  ```
- [ ] **COMMIT:** `feat(migrate): remover multi-targeting net9.0 (somente net10.0)`

---

### Task 4.9: Resolver Conflitos de Dependências
- [ ] **ALTERAÇÃO:** Se houver conflitos de versão, usar `Directory.Packages.props` para forçar versões
  ```xml
  <PackageVersion Include="System.Text.Json" Version="10.0.0" />
  ```
- [ ] **VALIDAR:** Compilar solução completa
- [ ] **BUILDAR:** Build completo
  ```bash
  dotnet build --no-restore
  ```
- [ ] **TESTAR:** Todos os testes
- [ ] **COMMIT:** `fix(migrate): resolver conflitos de versão de pacotes`

---

### Task 4.10: Verificar Runtime Changes
- [ ] **ALTERAÇÃO:** Verificar se há código dependente de:
  - Garbage Collection behavior
  - ThreadPool changes
  - JIT compiler changes
- [ ] **VALIDAR:** Review em código de alta performance (se houver)
- [ ] **BUILDAR:** Build completo
- [ ] **TESTAR:** Testes de stress/performance (se houver)
- [ ] **COMMIT:** `fix(migrate): ajustes para runtime .NET 10`

**Status Fase 4:** 🔴 Não Iniciado | 🟡 Em Progresso | 🟢 Concluído

---

## ✨ FASE 5: OTIMIZAÇÕES C# 14

### Task 5.1: Aplicar Field Keyword (C# 14)
- [ ] **ALTERAÇÃO:** Refatorar propriedades auto-implementadas para usar `field`
  ```csharp
  // Antes
  public string Name { get; set; } = string.Empty;
  
  // Depois (C# 14)
  public string Name { get => field; set => field = value ?? throw new ArgumentNullException(); }
  ```
  - Arquivos candidatos em `src/Core/OnForkHub.Core/Entities/`
- [ ] **VALIDAR:** Sintaxe C# 14 correta
- [ ] **BUILDAR:** Compilar
- [ ] **TESTAR:** Testes de entidades
- [ ] **COMMIT:** `feat(csharp14): aplicar field keyword em propriedades`

---

### Task 5.2: Aplicar Extension Members (C# 14)
- [ ] **ALTERAÇÃO:** Converter static extension methods para `extension` (se aplicável)
  ```csharp
  // Antes
  public static class StringExtensions { public static bool IsNullOrEmpty(this string s) => ...; }
  
  // Depois (C# 14)
  extension StringExtensions for string { public bool IsNullOrEmpty() => ...; }
  ```
- [ ] **VALIDAR:** Sintaxe correta
- [ ] **BUILDAR:** Compilar
- [ ] **TESTAR:** Testes de extensions
- [ ] **COMMIT:** `feat(csharp14): refatorar para extension members`

---

### Task 5.3: Otimizar Primary Constructors
- [ ] **ALTERAÇÃO:** Refatorar classes para usar primary constructors onde fizer sentido
  - Foco em Services e Handlers
- [ ] **VALIDAR:** Sintaxe correta
- [ ] **BUILDAR:** Compilar
- [ ] **TESTAR:** Testes afetados
- [ ] **COMMIT:** `refactor(csharp14): otimizar primary constructors`

---

### Task 5.4: Usar Collection Expressions (Onde Aplicável)
- [ ] **ALTERAÇÃO:** Substituir `new List<T>()` por `[]` onde apropriado
  - Já parcialmente implementado, verificar oportunidades restantes
- [ ] **VALIDAR:** Compilar
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testes
- [ ] **COMMIT:** `refactor(csharp14): usar collection expressions`

---

### Task 5.5: Aplicar Pattern Matching Avançado
- [ ] **ALTERAÇÃO:** Usar novos padrões de pattern matching do C# 14
  - List patterns
  - Enhanced switch expressions
- [ ] **VALIDAR:** Compilar
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testes
- [ ] **COMMIT:** `feat(csharp14): aplicar pattern matching avançado`

---

### Task 5.6: Modernizar Records (Se Aplicável)
- [ ] **ALTERAÇÃO:** Verificar se há classes que deveriam ser `record`
  - DTOs, ValueObjects
- [ ] **VALIDAR:** Compilar
- [ ] **BUILDAR:** Build
- [ ] **TESTAR:** Testes
- [ ] **COMMIT:** `refactor(csharp14): converter classes apropriadas para records`

**Status Fase 5:** 🔴 Não Iniciado | 🟡 Em Progresso | 🟢 Concluído

---

## ✅ FASE 6: VALIDAÇÃO FINAL

### Task 6.1: Build Limpo Completo
- [ ] **ALTERAÇÃO:** Executar build limpo
  ```bash
  dotnet clean
  dotnet restore
  dotnet build --no-restore 2>&1 | Tee-Object build-final.log
  ```
- [ ] **VALIDAR:** 
  - [ ] 0 erros
  - [ ] 0 warnings (ou mínimo aceitável)
- [ ] **BUILDAR:** ✅ Completo
- [ ] **TESTAR:** N/A (próxima task)
- [ ] **COMMIT:** N/A (apenas validação)

---

### Task 6.2: Execução Completa de Testes
- [ ] **ALTERAÇÃO:** Executar todos os testes
  ```bash
  dotnet test --verbosity normal 2>&1 | Tee-Object tests-final.log
  ```
- [ ] **VALIDAR:**
  - [ ] 372+ testes passando
  - [ ] 0 falhando
  - [ ] Tempo de execução aceitável (< 60s)
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** ✅ Todos passando
- [ ] **COMMIT:** N/A (apenas validação)

---

### Task 6.3: Análise Estática Final
- [ ] **ALTERAÇÃO:** Executar análise estática
  ```bash
  dotnet format --verify-no-changes
  dotnet build --no-restore -p:TreatWarningsAsErrors=true
  ```
- [ ] **VALIDAR:**
  - [ ] 0 StyleCop warnings
  - [ ] 0 Analyzer warnings
  - [ ] Código formatado corretamente
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `style(migrate): formatação final pós-migração`

---

### Task 6.4: Teste de Integração da API
- [ ] **ALTERAÇÃO:** Executar testes de integração específicos
  ```bash
  dotnet test test/Presentations/OnForkHub.Api.IntegrationTests --verbosity normal
  ```
- [ ] **VALIDAR:**
  - [ ] Todos endpoints funcionando
  - [ ] Swagger/OpenAPI carregando
  - [ ] Autenticação JWT funcionando
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** ✅ Integração passando
- [ ] **COMMIT:** N/A (apenas validação)

---

### Task 6.5: Documentar Mudanças
- [ ] **ALTERAÇÃO:** Criar `docs/NET10_MIGRATION_SUMMARY.md`
  - Resumo das mudanças realizadas
  - Pacotes atualizados
  - Breaking changes corrigidos
  - Features C# 14 aplicadas
  - Performance improvements (se mensuráveis)
- [ ] **VALIDAR:** Documento completo e revisado
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** `docs(migrate): adicionar resumo da migração para .NET 10`

**Status Fase 6:** 🔴 Não Iniciado | 🟡 Em Progresso | 🟢 Concluído

---

## 🚀 FASE 7: PULL REQUEST E MERGE

### Task 7.1: Criar Pull Request
- [ ] **ALTERAÇÃO:** Criar PR para `dev`
  ```bash
  git push origin feature/migrate-net10
  gh pr create --title "feat: migrate to .NET 10" --body-file docs/NET10_MIGRATION_SUMMARY.md
  ```
- [ ] **VALIDAR:** PR criado com:
  - [ ] Título seguindo Conventional Commits
  - [ ] Descrição completa
  - [ ] Checklist de verificações
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** N/A

---

### Task 7.2: Code Review
- [ ] **ALTERAÇÃO:** Solicitar review de pelo menos 1 desenvolvedor
- [ ] **VALIDAR:** Aprovação recebida
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** CI/CD passando (GitHub Actions)
- [ ] **COMMIT:** N/A

---

### Task 7.3: Merge para dev
- [ ] **ALTERAÇÃO:** Merge do PR
  ```bash
  gh pr merge --squash --delete-branch
  ```
- [ ] **VALIDAR:** Merge completo sem conflitos
- [ ] **BUILDAR:** Build na branch dev
- [ ] **TESTAR:** Testes na branch dev
- [ ] **COMMIT:** N/A

---

### Task 7.4: Validação Pós-Merge
- [ ] **ALTERAÇÃO:** Verificar build na branch dev
  ```bash
  git checkout dev
  git pull origin dev
  dotnet build
  dotnet test
  ```
- [ ] **VALIDAR:** Tudo funcionando na dev
- [ ] **BUILDAR:** ✅
- [ ] **TESTAR:** ✅
- [ ] **COMMIT:** N/A

---

### Task 7.5: Tag de Release (Opcional)
- [ ] **ALTERAÇÃO:** Criar tag para marcar migração
  ```bash
  git tag -a v2.0.0-net10 -m "Release: Migração completa para .NET 10"
  git push origin v2.0.0-net10
  ```
- [ ] **VALIDAR:** Tag criada
- [ ] **BUILDAR:** N/A
- [ ] **TESTAR:** N/A
- [ ] **COMMIT:** N/A

**Status Fase 7:** 🔴 Não Iniciado | 🟡 Em Progresso | 🟢 Concluído

---

## 📊 Resumo de Commits Esperados

| Fase | Commits Esperados |
|------|-------------------|
| Fase 1 | 2 commits (backup + baseline) |
| Fase 2 | 8 commits (atualização core) |
| Fase 3 | 12 commits (atualização pacotes) |
| Fase 4 | 10 commits (correções) |
| Fase 5 | 6 commits (otimizações C# 14) |
| Fase 6 | 1 commit (formatação) |
| Fase 7 | N/A (merge apenas) |
| **Total** | **~39 commits** |

---

## 🆘 Troubleshooting

### Erro: "Workload wasm-tools não encontrado"
```bash
dotnet workload restore
dotnet workload install wasm-tools
```

### Erro: "Pacote X não compatível com net10.0"
- Verificar se há versão preview ou atualizada do pacote
- Se não houver, manter em net9.0 temporariamente
- Adicionar à documentação de pacotes pendentes

### Erro: "Breaking change em Y"
1. Consultar: https://docs.microsoft.com/en-us/dotnet/core/compatibility/10.0
2. Aplicar migração guiada pela documentação oficial
3. Criar teste específico para validar comportamento

### Erro: "Testes falhando após migração"
```bash
# Executar testes com detalhes
dotnet test --verbosity detailed --logger trx
# Analisar resultados e corrigir individualmente
```

---

## 📁 Arquivos Chave para Monitorar

| Arquivo | Importância | Ações |
|---------|-------------|-------|
| `global.json` | 🔴 Crítico | SDK version |
| `Directory.Build.props` | 🔴 Crítico | TargetFramework, LangVersion |
| `Directory.Packages.props` | 🔴 Crítico | Versions de todos os pacotes |
| `src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj` | 🟡 Alta | Blazor WASM |
| `test/*/GlobalUsings.cs` | 🟡 Alta | SA1210 fixes |
| `test/*/UpdateVideoUseCaseTest.cs` | 🟡 Alta | IValidationResult fixes |
| `.docker/Dockerfile.*` | 🟡 Alta | Imagem base |
| `.devcontainer/Dockerfile` | 🟢 Média | Dev environment |

---

## 📝 Notas de Progresso

| Data | Task | Responsável | Status | Observações |
|------|------|-------------|--------|-------------|
| | | | | |
| | | | | |
| | | | | |

---

## ✅ Checklist de Conclusão

- [ ] Todas as 46 tasks concluídas
- [ ] Build limpo (0 erros, 0 warnings)
- [ ] 372+ testes passando
- [ ] Documentação atualizada
- [ ] PR mergeado para dev
- [ ] Tag de release criada (opcional)

---

**Data de Conclusão Prevista:** ___/___/______  
**Responsável pela Migração:** _________________  
**Revisor:** _________________

---

> 💡 **Dica:** Faça commits pequenos e frequentes. Se algo der errado, é mais fácil fazer `git revert` em um commit pequeno do que em um commit gigante.
