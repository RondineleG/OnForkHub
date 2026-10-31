# WebTorrent P2P Integration - OnForkHub

Esta documentação descreve como a reprodução P2P (WebTorrent 2.x) funciona hoje no OnForkHub.

## Arquitetura

O navegador baixa o vídeo em pedaços de vários peers e de web seeds HTTP ao mesmo tempo, e já começa a tocar enquanto baixa. Isso reduz a banda de saída do servidor: quanto mais gente assiste, mais peers o vídeo tem.

### Componentes

1.  **`wwwroot/js/webtorrentService.js`**: encapsula o cliente WebTorrent 2.x. Expõe `startDownload`, `stopDownload`, `getTorrentStats`, `createTorrent`, `updateConfig` (limites de banda) e `disposeWebTorrent`.
2.  **`wwwroot/webtorrent-sw.min.js`**: service worker do WebTorrent. O arquivo é transmitido por ele direto para o elemento `<video>` (`file.streamTo(video)`), então o decodificador nativo do navegador cuida dos formatos e do seek.
3.  **`wwwroot/lib/webtorrent`**: a biblioteca, incluída no projeto (sem CDN).
4.  **`wwwroot/js/shortsFeed.js`**: o feed de shorts. Carrega um vídeo por vez, mostra o estado do carregamento e as estatísticas (peers, velocidade, progresso).
5.  **`Services/ShortsCatalog.cs`**: catálogo fixo do feed. São filmes abertos da Blender Foundation publicados como `.torrent` com web seeds HTTP, então tocam mesmo sem nenhum outro peer online.
6.  **`IVideoService.EnableTorrentAsync`** (API): associa um Magnet URI a um vídeo existente.
7.  **`tools/p2p-seeder`**: worker Node experimental que semeia os vídeos armazenados. Veja o [README do seeder](../tools/p2p-seeder/README.md).

## Fluxo de funcionamento

1.  **Origem**: `startDownload` aceita um Magnet URI ou a URL de um arquivo `.torrent`.
2.  **Cliente**: na primeira chamada o cliente é criado e o service worker é registrado e ativado. Service workers exigem HTTPS ou `localhost`.
3.  **Download**: o cliente se conecta a peers por trackers WebRTC e a web seeds HTTP. O maior arquivo `.mp4`, `.m4v` ou `.webm` do torrent é o filme.
4.  **Reprodução**: o arquivo é ligado ao `<video>` por streaming e a promessa resolve quando o navegador consegue tocar (`canplay`).
5.  **Encerramento**: `stopDownload` destrói o torrent e libera a memória dos pedaços.

## Decisões que importam

*   **Pedaços em memória.** O armazenamento padrão do WebTorrent no navegador (OPFS) levava 20 a 30 s, ou nunca terminava, para preparar um arquivo do tamanho de um filme antes do primeiro byte. Com um store em memória (`MemoryChunkStore`) o torrent fica pronto em cerca de 1 s. Os pedaços só existem enquanto o torrent está ativo.
*   **Timeout por inatividade.** A falha só acontece depois de 30 s sem receber nenhum dado (`STALL_TIMEOUT_MS`). O relógio reinicia a cada evento `download`, então downloads lentos mas ativos continuam.
*   **Um vídeo por vez.** O feed carrega só o vídeo ativo. O próximo é pré-carregado depois que o atual começa a tocar, para os dois não disputarem a banda.
*   **Sem fallback para HTTP no player.** Os web seeds do próprio torrent cobrem a falta de peers. Um fallback para a URL de streaming da API não está implementado.

## Testes

```bash
node --test tests/webtorrentService.test.mjs
```

Os testes usam um cliente WebTorrent falso, e rodam também no CI (`player-tests.yml`).

## Configuração

Os trackers são os padrões do WebTorrent. Em produção, recomenda-se configurar trackers próprios para melhor conectividade.

```javascript
// Exemplo de Magnet URI suportado
// magnet:?xt=urn:btih:XXXXX&dn=video.mp4&tr=wss%3A%2F%2Ftracker.webtorrent.io
```

## Benefícios

*   **Escalabilidade**: quanto mais pessoas assistem, mais rápida e resiliente fica a rede.
*   **Redução de custos**: menor uso de banda de saída do provedor de nuvem.
*   **Performance**: em redes locais ou densas, a velocidade pode superar o download convencional.
