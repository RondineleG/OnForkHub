# OnForkHub — Plano de Conexão Frontend ↔ API

> Revisado em 19/11/2026. Build verde ✅ | 636 testes passando ✅ | Branch: `main`

---

## Diagnóstico: onde está o gap hoje

### O que a API expõe (já pronto, funcional)

| Rota | Método | Auth | Função |
|---|---|---|---|
| `GET /api/v1/rest/videos?page=1&itemsPerPage=10` | GET | Anônimo | Lista vídeos paginados |
| `GET /api/v1/rest/videos/{id}` | GET | Anônimo | Busca vídeo por ID |
| `GET /api/v1/rest/videos/search?q=...` | GET | Anônimo | Pesquisa full-text |
| `POST /api/v1/rest/videos/{id}/views` | POST | Anônimo | Incrementa view count |
| `POST /api/v1/auth/login` | POST | Anônimo | Login → JWT tokens |
| `POST /api/v1/auth/register` | POST | Anônimo | Registro de usuário |
| `POST /api/v1/videos/upload/initiate` | POST | JWT | Inicia upload chunked |
| `PUT /api/v1/videos/upload/{uploadId}/chunk` | PUT | JWT | Envia chunk |
| `GET /api/v1/videos/upload/{uploadId}/status` | GET | JWT | Status do upload |
| `POST /api/v1/videos/upload/{uploadId}/publish` | POST | JWT | Publica vídeo |
| `GET /api/v1/rest/videos/{id}/stream` | GET | Anônimo | Stream direto MP4/WebM |

**Formato de resposta**: envelope `{ data, errors, message, date, id }` para a maioria dos endpoints.  
**GetAll específico**: retorna `RequestResult<IEnumerable<Video>>`, não uma lista direta.

### O que o Frontend tem (já pronto, mas desconectado)

| Componente | Estado | Problema |
|---|---|---|
| `Shorts.razor` | HTML+JS pronto | Lê `ShortsCatalog` (lista hardcoded) em vez da API |
| `shortsFeed.js` | Pronto | Espera `data-source` com torrent URL/magnet no DOM |
| `VideoService.cs` | Pronto | Mapeia `VideoResponse → Video` corretamente |
| `IVideoService` | Pronto | Interface completa implementada |
| `HttpClient` registrado | Pronto | `BaseAddress = http://localhost:9000` |
| `Embed.razor` | Pronto | Usa `IVideoService.GetVideoByIdAsync()` ✅ |
| CORS na API | Pronto | Permite `http://localhost:5000`, `http://localhost:3000` |
| `appsettings.json` Web | Pronto | `ApiBaseUrl: "http://localhost:9000"` correto |

---

## Os 3 problemas que bloqueiam tudo

### Problema 1 — Rota errada no `VideoService.cs`

O serviço chama `/api/v1/videos`, mas a API registra as rotas com prefixo `rest/`
via `GetDefaultRestRoute()` em `BaseEndPoint<TEntity>`:

```
Rota real  → /api/v1/rest/videos
Código Web → /api/v1/videos       ← 404 garantido
```

**Correção** — trocar em todos os métodos de `VideoService.cs`:

```csharp
// GetAllAsync
var url = $"/api/v1/rest/videos?page={page}&itemsPerPage={pageSize}";

// GetByIdAsync
var response = await _httpClient.GetAsync($"/api/v1/rest/videos/{id}");

// SearchAsync
var url = $"/api/v1/rest/videos/search?q={q}&page={page}&pageSize={pageSize}";

// IncrementViewCountAsync
await _httpClient.PostAsync($"/api/v1/rest/videos/{videoId}/views", null);

// DeleteVideoAsync
var response = await _httpClient.DeleteAsync($"/api/v1/rest/videos/{id}");
```

### Problema 2 — Desserialização do envelope quebrada

A API retorna `{ "data": [...], "message": "..." }`, mas o `VideoService` tenta
ler como `List<VideoResponse>` diretamente — vai retornar `null` ou lançar exceção.

**Correção** — criar `ApiEnvelope<T>` e desembrulhar:

```csharp
// Novo arquivo: src/Presentations/OnForkHub.Web/Models/ApiEnvelope.cs
namespace OnForkHub.Web.Models;

public sealed class ApiEnvelope<T>
{
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}
```

```csharp
// VideoService.GetAllAsync — antes:
var result = await response.Content.ReadFromJsonAsync<List<VideoResponse>>();

// Depois:
var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<List<VideoResponse>>>();
var result = envelope?.Data;
```

### Problema 3 — `Shorts.razor` não usa `IVideoService`

O componente principal do feed lê uma lista hardcoded e nunca chama a API,
mesmo com o serviço registrado no DI e funcionando.

**Correção** — adicionar inject, `OnInitializedAsync` com fallback gracioso:

```razor
@page "/"
@implements IAsyncDisposable
@inject IJSRuntime JS
@inject IVideoService VideoService   <!-- ADICIONAR

@code {
    private List<Video> _videos = [];
    private bool _loading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _videos = await VideoService.GetAllAsync(page: 1, pageSize: 10);
        }
        catch
        {
            // Fallback: catálogo estático Blender se a API estiver offline
            _videos = ShortsCatalog.Videos
                .Select(v => new Video { Title = v.Title, Description = v.Description, MagnetUri = v.TorrentUrl })
                .ToList();
        }
        finally { _loading = false; }
    }
}

@* No markup, substituir ShortsCatalog.Videos por _videos *@
@for (var i = 0; i < _videos.Count; i++)
{
    var video = _videos[i];
    var source = video.MagnetUri ?? $"/api/v1/rest/videos/{video.Id}/stream";
    <article class="short" data-index="@i" data-source="@source" aria-label="@video.Title">
```

---

## Problema extra — CORS não inclui as portas do Blazor

```json
// src/Presentations/OnForkHub.Api/appsettings.Development.json
// Adicionar as portas 8000/8001 (launchSettings do Web):
"Cors": {
    "AllowedOrigins": [
        "http://localhost:5000",
        "https://localhost:5001",
        "http://localhost:3000",
        "http://localhost:8001",
        "https://localhost:8000"
    ]
}
```

---

## Pré-requisito para rodar local — banco de dados

A API exige SQL Server com o banco `OnForkHub`. Criar e migrar:

```bash
# Na raiz do repositório
dotnet ef database update \
  --project src/Infrastructure/OnForkHub.Persistence \
  --startup-project src/Presentations/OnForkHub.Api
```

`appsettings.Development.json` já usa `trusted_connection=true` (autenticação Windows),
então basta ter o SQL Server local rodando.

Para ter dados no feed, inserir pelo menos um registro na tabela `Videos` via
seed ou pelo endpoint `POST /api/v1/rest/videos` (após autenticar).

---

## Como rodar local após as correções

```bash
# Terminal 1 — API na porta 9000
cd src/Presentations/OnForkHub.Api
dotnet run

# Terminal 2 — Blazor WASM na porta 8001
cd src/Presentations/OnForkHub.Web
dotnet run
```

Abrir `http://localhost:8001` no navegador.  
Swagger disponível em `http://localhost:9000/swagger`.

---

## Ordem de execução das correções

```
1. ApiEnvelope<T>  (novo arquivo)
2. VideoService.cs (rotas + desembrulhar envelope)
3. appsettings.Development.json da API (adicionar portas CORS)
4. Shorts.razor (inject + OnInitializedAsync + data-source dinâmico)
5. Banco de dados (migrations + seed mínimo)
6. Testar end-to-end
```

---

## Itens abertos pós-conexão básica

| Item | Impacto | Ação |
|---|---|---|
| `GetAllVideoUseCase` retorna a entidade de domínio `Video` em vez de `VideoResponse` — serialização expõe campos internos | Alto | Mapear para `VideoResponse` dentro do use case ou no endpoint |
| `VideoResponse.FromVideo()` não preenche `ThumbnailUrl`, `Duration`, `ViewCount` | Médio | Adicionar os campos faltantes no mapper |
| `VideoService.LikeVideoAsync` / `UnlikeVideoAsync` retornam `true` sempre (stub) | Médio | Conectar ao `RatingEndpoint` real |
| Feed WebTorrent exige HTTPS ou `localhost` para Service Worker | Baixo em dev | Já funciona em localhost; em outro IP precisa de HTTPS |
| Sem seed de dados local → feed vazio mesmo após conectar | Bloqueador | Criar seed de desenvolvimento com 2–3 vídeos de teste |
