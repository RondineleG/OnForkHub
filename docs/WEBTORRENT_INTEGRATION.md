# WebTorrent P2P Integration - OnForkHub

Esta documentação descreve como o sistema de compartilhamento de vídeo P2P (WebTorrent) está implementado no OnForkHub.

## Arquitetura

O sistema utiliza a biblioteca **WebTorrent.js** para permitir que os usuários compartilhem partes do vídeo diretamente entre si, reduzindo a carga nos servidores de armazenamento e custos de banda.

### Componentes

1.  **WebTorrentService (C#)**: Serviço de interoperabilidade (JS Interop) no Blazor que expõe as funcionalidades do WebTorrent para o código .NET.
2.  **webtorrentService.js**: Módulo JavaScript que encapsula a biblioteca WebTorrent.js e gerencia o cliente P2P no navegador.
3.  **P2PVideoPlayer (Razor)**: Componente de UI que renderiza o vídeo via P2P, mostra estatísticas em tempo real (peers, progresso, velocidades) e gerencia o fallback para CDN.
4.  **IVideoService.EnableTorrentAsync**: Endpoint na API que permite associar um Magnet URI a um vídeo existente.

## Fluxo de Funcionamento

1.  **Seed**: Quando um vídeo é carregado ou processado, um Magnet URI pode ser gerado semeando o arquivo a partir do navegador do autor ou de um servidor de seeding.
2.  **Download**: Quando um usuário assiste a um vídeo com P2P ativado:
    *   O navegador tenta se conectar a outros peers via trackers WebRTC.
    *   O vídeo é baixado em pedaços (chunks) de múltiplos peers simultaneamente.
    *   O vídeo é "anexado" ao elemento HTML5 video à medida que os chunks chegam.
3.  **Fallback**: Se não houver peers disponíveis após 5 segundos, o player alterna automaticamente para o streaming convencional via HTTP (CDN/Storage).

## Configuração

Os trackers utilizados são os padrões do WebTorrent. Para ambientes de produção, recomenda-se configurar trackers privados ou próprios para garantir maior conectividade.

```javascript
// Exemplo de Magnet URI suportado
// magnet:?xt=urn:btih:XXXXX&dn=video.mp4&tr=wss%3A%2F%2Ftracker.webtorrent.io
```

## Benefícios

*   **Escalabilidade**: Quanto mais pessoas assistem, mais rápida e resiliente se torna a rede.
*   **Redução de Custos**: Menor uso de banda de saída do provedor de nuvem.
*   **Performance**: Em redes locais ou densas, a velocidade pode superar o download convencional.
