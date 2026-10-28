# OnForkHub — Arquitetura e validação do player P2P

**Data:** 2026-10-28  
**Status:** implementação parcial em andamento; validação final concentrada após todas as correções viáveis  
**Branch de trabalho:** `feat/pair-programming`

## 1. Objetivo e prioridade

Entregar reprodução real de vídeos do catálogo diretamente no navegador, usando o player Blazor WebAssembly + WebTorrent existente, sem download manual. A prioridade é fazer o vídeo tocar e validar o fluxo completo; autenticação, redesign do frontend e migração para React ficam fora do MVP atual.

**Critério de aceite do MVP:** publicar um vídeo curto de teste autorizado, gerar seu torrent, disponibilizar ao menos um seeder WebRTC, abrir o vídeo no OnForkHub, iniciar reprodução, usar play/pause/seek/fullscreen e comprovar a estratégia de fallback quando o P2P falhar.

## 2. O que existe no repositório

- `src/Presentations/OnForkHub.Web/Components/VideoPlayer/P2PVideoPlayer.razor`: componente visual do player, estatísticas de peers, tentativa de fallback HTTP e ciclo de vida.
- `src/Presentations/OnForkHub.Web/Services/WebTorrentService.cs`: camada Blazor/JS Interop.
- `src/Presentations/OnForkHub.Web/wwwroot/js/webtorrentService.js`: cliente WebTorrent, criação de torrent, reprodução e estatísticas.
- `src/Presentations/OnForkHub.Web/Pages/VideoDetail.razor`: seleção do player a partir dos dados do vídeo.
- `docs/WEBTORRENT_INTEGRATION.md`: documentação anterior, que deverá ser conciliada com os resultados dos testes.

**Estado das alterações recentes:** foram adicionados tratamentos de inicialização, erros e cancelamento, além de um arquivo de testes Node. A existência desses commits **não comprova** teste de navegador, funcionamento P2P ou compilação do Blazor. Antes de chamar a correção de concluída, executar testes e revisar conjuntamente a implementação do ciclo de vida.

## 3. Fluxo desejado: upload até reprodução

1. Usuário envia vídeo para a API ASP.NET Core.
2. Backend valida o arquivo (tamanho, extensão real, MIME, duração e segurança básica de upload), registra o item no catálogo e armazena o original de forma durável.
3. Worker prepara arquivo adequado ao navegador (ex.: MP4 H.264/AAC) e gera metadados `.torrent`/magnet com trackers apropriados.
4. Worker de seeding compatível com **WebRTC/WebTorrent** recebe o arquivo e permanece online para disponibilizar os primeiros blocos.
5. No Blazor, a página solicita os metadados de reprodução da API (visibilidade e disponibilidade do item), então inicializa WebTorrent pelo JS Interop.
6. O cliente encontra peers WebRTC, recebe os blocos necessários e entrega o fluxo ao elemento `video`. Usuários podem contribuir como peers enquanto estão conectados.
7. Se houver falha ou insuficiência de dados, o player tenta uma URL HTTP autorizada, se disponível.
8. Ao trocar de vídeo/sair da página, interromper a sessão, o polling e os recursos de reprodução.

**Observação:** seeders BitTorrent comuns não necessariamente atendem navegadores; validar conexão real de um worker WebRTC/híbrido. Vários contêineres numa só VPS não criam diversidade de rede nem alta disponibilidade.

## 4. Topologia inicial e evolução

**MVP:** uma VPS, Docker, um worker WebRTC/híbrido capaz de servir múltiplos torrents e volume persistente para os vídeos de teste; API e frontend permanecem independentes do processo de seeding. Medir CPU, RAM, disco, tráfego de saída, latência e disponibilidade.

**Próxima etapa:** segundo seeder em VPS/provedor distinto se métricas ou continuidade exigirem.

**Escala:** múltiplos nós distribuídos, inclusão/remoção controlada de conteúdo, replicação de arquivos e monitoramento de disponibilidade. Não assumir que dez instâncias na mesma VPS equivalem a dez seeders úteis.

**Armazenamento:** decidir entre cópia permanente controlada pela plataforma e seeds temporários; para disponibilidade previsível, recomenda-se cópia persistente independente da vida do contêiner.

## 5. Público x privado (decisão de arquitetura)

A API é autoridade para visibilidade e metadados do catálogo. Porém, **magnet URI/torrent não implementa revogação de acesso**: alguém que já conhece o hash pode tentar obter o conteúdo de outros peers. Não chamar um torrent aberto de privado apenas porque o frontend esconde o link.

Para o MVP, usar vídeos de teste públicos/autorizados. Antes de ativar vídeos privados, escolher entrega autenticada por HTTP, criptografia e gestão de chaves, ou outro mecanismo que efetivamente proteja os bytes. Não tratar isso como trabalho de login neste momento; é uma restrição arquitetural da distribuição P2P.

## 6. Matriz de diagnóstico do vídeo que falhou

| Cenário | Evidência a coletar | Hipótese verificada |
|---|---|---|
| Mesmo arquivo por HTTP | erro do elemento video, `canPlayType`, codec | incompatibilidade de codec/contêiner |
| Mesmo torrent com seeder WebRTC conhecido | número de peers, conexão tracker, taxa de download | conectividade P2P |
| Vídeo curto MP4 conhecido | primeiro frame, tempo de início | integração JS/Blazor |
| Arquivo maior, mesmo codec | buffering, throughput e busca | insuficiência de banda / disponibilidade de blocos |
| Sem peers | tempo até fallback e reprodução HTTP | resiliência do player |
| Troca de vídeo/saída de página | sessões abertas, listeners, timers | vazamento de recursos |

Tamanho do arquivo isoladamente não determina se um vídeo toca. Comparar bitrate necessário, taxa efetiva, codecs, metadados e peers.

## 7. Testes e critérios de aceite

**Teste unitário do módulo JS:** inicialização bem-sucedida, elemento ausente, formato não suportado, falha do torrent, timeout e cancelamento durante inicialização; adicionar cobertura para troca de vídeo e chamadas duplicadas.

**Teste de integração:** verificar `WebTorrentService.cs` ↔ JavaScript; montagem e descarte do componente Razor; fallback; nenhuma exceção não observada em polling.

**Teste real em navegador:** testar 1 seeder WebRTC + 1 viewer, depois 1 seeder + 2 viewers em redes distintas, incluindo seek e reconexão. Documentar navegador, codec, tamanho, número de peers, tempo para primeiro frame e logs.

**Gate de conclusão:** `node --test tests/webtorrentService.test.mjs`, build .NET relevante e teste de reprodução em navegador aprovados, com evidências registradas. Não marcar um teste como aprovado sem execução verificável.

## 8. Backlog priorizado

1. Executar/revisar os testes JS existentes e corrigir falhas reais.
2. Executar build do frontend e revisar cancelamento, timeout e fallback com a API.
3. Validar um vídeo MP4 curto usando torrent com seeder WebRTC conhecido.
4. Demonstrar dois navegadores reproduzindo o mesmo vídeo e medindo peers/banda.
5. Criar o fluxo mínimo upload → geração torrent → seed persistente → catálogo → player, em incrementos aprovados.
6. Definir política de retenção, supervisão do seeder e telemetria antes de expandir infraestrutura.

## 9. Governança das alterações

- Trabalhar somente em `feat/pair-programming`, usando convenção de commits e observando as regras/hooks do projeto.
- **Sem PR, merge, deploy ou alteração da `main`** sem autorização expressa.
- Apresentar escopo, arquivos afetados e impacto antes de cada próxima mudança; o usuário autorizou corrigir os itens do documento em sequência e concentrar a validação no final; mudanças externas de infraestrutura ainda dependem de acesso e configuração.
- Este documento não implica aprovação automática das mudanças futuras.

## 10. Decisões pendentes

- Armazenamento persistente do original ou seeding temporário?
- Onde executar o primeiro worker (VPS existente ou nova)?
- Limites iniciais de duração/tamanho/codec e qualidade?
- Política para conteúdo privado (fora do MVP funcional)?

## 11. Registro de execução — 2026-10-28

**Alterações adicionais desta rodada:**
- `eb14ee8`: isola o elemento de vídeo gerenciado pelo JavaScript da renderização Blazor e deixa de substituir P2P inicializado apenas por contagem instantânea de peers.
- `da4830e`: adiciona workflow de testes JavaScript em Node 22 para a branch de trabalho, sem deploy.
- `6a7856f`: remove parâmetros não declarados ao instanciar `P2PVideoPlayer`; vídeos marcados como torrent não dependem da preferência de perfil para usar P2P.
- `3c07bb2`: desacopla o carregamento do player da consulta de perfil/autenticação e trata falhas de favoritos como opcionais.

**Validação ainda não comprovada:** resultado do workflow Node; build do Blazor; reprodução real em browser; conexão seeder WebRTC; upload→geração do torrent→seeding persistente; fallback HTTP com URL efetiva. O ambiente de execução disponível não tinha acesso de rede para clonar o repositório e não tinha SDK dotnet instalado. Nenhum resultado de CI ou de playback foi fabricado.

**Dependências para o aceite final:** disponibilizar arquivo de teste e um seeder WebRTC alcançável; definir armazenamento/infraestrutura ou credenciais de um ambiente de testes autorizado; obter logs de browser e pipeline; revisar o fallback HTTP do catálogo (o modelo atual expõe somente `Url` e `IsTorrent`, sem URL de fallback separada). Não considerar o escopo encerrado.


## 12. Auditoria incremental — 2026-10-28 (segunda rodada)

### Alterações confirmadas nesta rodada

- `7993ed9` — conecta o componente P2P à rota HTTP já existente `/api/v1/videos/{id}/stream`. O stream depende de um arquivo realmente armazenado; a URL não garante fallback enquanto o backend mantiver apenas magnet URI.
- `c4129c6` — informa erro de reprodução ao usuário e evita que uma falha de limpeza JS impeça a tentativa de fallback.

### Bloqueios técnicos confirmados por leitura do código

- `VideoUploadService.UploadChunkAsync` possui TODO explícito: apenas incrementa contador, **não grava nem monta os chunks**. Portanto upload real não está implementado.
- `VideoUpload.Create` inicia o estado Pending, mas `UploadChunkAsync` não faz a transição para Uploading antes de `MarkAsProcessing`. O último chunk pode falhar por violar a transição de domínio.
- `StreamEndpoint` busca `video.Url.Value` diretamente no armazenamento; se o endereço salvo for o magnet, não há fallback HTTP comprovado. É necessária uma referência persistida separada para a cópia reproduzível.
- `EnableTorrentEndpoint` apenas registra magnet URI; não cria o torrent nem semeia o arquivo. É necessário worker compatível com peers WebRTC.
- A reprodução real de 1 seeder + múltiplos navegadores, reconexão, seek e medição de banda depende de ambiente de teste e não pode ser marcada como aprovada por testes de mocks.

### Política de conclusão

Não assinalar upload, geração torrent, seeder, reprodução real, fallback e monitoramento como concluídos antes de persistência de arquivo, configuração de seeder, resultados de CI e teste funcional comprovados. Manter backend e front Blazor existentes e preservar separação entre catálogo, mídia HTTP e magnet.
