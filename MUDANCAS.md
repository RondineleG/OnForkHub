# Registro de Mudanças

## 49bcb3d - 2026-11-19 18:46

- Criado classe `ApiEnvelope<T>` para modelar o envelope de resposta padrão `{ data, message, errors, id }` da API
- Criado classe `ApiVideoItem` para desserialização flexível de vídeos (suportando propriedades primitivas e Value Objects)
- Alterado serviço `VideoService` corrigindo rotas para `/api/v1/rest/videos` com fallback resiliente para `/api/v1/videos`
- Alterado serviço `VideoService` implementando desserialização de envelopes e suporte a `ApiVideoItem`
- Alterado componente `Shorts.razor` injetando `IVideoService` e carregando vídeos dinâmicos da API no `OnInitializedAsync`
- Adicionado fallback no `Shorts.razor` para `ShortsCatalog.Videos` caso a API esteja indisponível ou sem registros
- Atualizado configuração de CORS em `appsettings.Development.json` e `Program.cs` da API incluindo origens do Blazor Web (`http://localhost:8001`, `https://localhost:8000`)

## 1cbfff5 - 2026-11-19 18:02

- Criado documento de arquitetura e plano de integração em `docs/CONEXAO_FRONTEND_API.md`
