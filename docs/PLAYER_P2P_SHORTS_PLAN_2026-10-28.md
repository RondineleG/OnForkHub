# OnForkHub — Plano do player vertical P2P (Shorts)

**Data:** 28/10/2026  
**Branch:** `feat/pair-programming`  
**Estado:** proposta de arquitetura; nenhuma implementação autorizada por este documento  
**Prioridade:** executar somente depois do encerramento e aceite do review técnico/P0-01.

## 1. Objetivo e decisões

- Experiência de feed vertical, vídeos aleatórios, duração máxima de **120 segundos**.
- **Blazor WebAssembly** permanece como interface; **TypeScript/JavaScript** encapsula WebTorrent, vídeo HTML5, buffer e ciclo de vida via JS Interop.
- MVP **sem autenticação, uploads de usuário, métricas de produto ou dashboard**; foco exclusivo em reproduzir e avançar vídeos com baixa latência.
- Uma lista inicial de aproximadamente 50–100 vídeos é **catálogo/metadados para seleção**, e **não** download integral no início.
- Número de 100 mil vídeos é hipótese de escala futura, não obrigação do MVP. Concorrência inicial projetada: abaixo de 10 espectadores.
- Sem alteração de código, PR, deploy ou `main` sem aprovação expressa por etapa.

## 2. Experiência desejada

1. Abrir a Home e selecionar aleatoriamente vídeos válidos do catálogo sem repeti-los até esgotar a janela de sessão.
2. Inicializar o primeiro vídeo rapidamente; exibir estado de carregamento se não estiver pronto, sem prometer início instantâneo em todas as redes.
3. Enquanto o usuário assiste ao vídeo atual, preparar os próximos **2–3** (configurável) e manter **1 anterior** para retorno.
4. Ao deslizar, reproduzir imediatamente quando o buffer mínimo estiver disponível; se não estiver, apresentar loading curto e fallback.
5. À medida que avança, manter uma **janela móvel**: liberar players, torrents, URLs de objeto e buffers fora da janela; repor catálogo sob demanda.
6. Em deslizes rápidos, cancelar pré-cargas antigas e priorizar o vídeo visível; impedir concorrência descontrolada.

## 3. Arquitetura de referência

```text
Blazor feed vertical (UI e eventos swipe)
  → JS Interop: PlayerCoordinator (TypeScript)
     → seleção / fila / prioridade / política de buffer e descarte
     → WebTorrent no navegador (WebRTC, peers e tracker compatível)
     → HTMLVideoElement / fallback HTTP por vídeo
  → API .NET: lista inicial { videoId, magnetUri ou infoHash, httpUrl, duração, poster }

Storage de vídeos e metadados → worker de publicação/torrent → seeder Docker compatível com WebRTC
                                         ↘ endpoint HTTP com Range + CORS adequado (fallback)
```

**Observação:** um torrent disponível em seeders BitTorrent tradicionais não implica conectividade com browsers; validar efetivamente WebRTC e tracker WebSocket/WSS. Seeder permanente pode usar `webtorrent-hybrid` ou solução equivalente validada na PoC. Múltiplos containers em uma VPS dividem a mesma banda; começar com um seeder e um pequeno lote de vídeos.

## 4. Política inicial de pré-carregamento

| Item | Regra para PoC |
|---|---|
| Vídeo visível | Prioridade máxima de transferência e renderização |
| Próximos | 2 em buffer inicial, terceiro apenas se houver banda/memória |
| Anterior | Reter 1 para navegação reversa |
| Catálogo da sessão | Receber apenas metadados, sem downloads em massa |
| Cache | Limite configurável em bytes, além de quantidade; LRU/evicção |
| Concorrência | No máximo 2 pré-cargas P2P ativas, além do vídeo atual (ajustar com medições) |
| Rede lenta ou economia de dados | Reduzir pré-cargas, priorizar visível e permitir fallback |
| Swipe rápido | Cancelar tarefas obsoletas, não recriar clientes WebTorrent desnecessariamente |

Um vídeo de 2 minutos não precisa estar 100% baixado para começar. Priorizar buffer inicial e continuidade, e não a transferência integral. Conferir se o container de vídeo é adequado a streaming progressivo (ex.: MP4 com metadados/moov no início).

## 5. Estados e contratos sugeridos (a validar na implementação)

- Estados por item: `idle`, `resolving`, `buffering`, `ready`, `playing`, `fallback`, `error`, `disposed`.
- Operações do módulo TS: `initialize`, `prepare`, `play`, `pause`, `setActive`, `cancel`, `dispose`.
- Um coordenador controla o cliente WebTorrent e a janela; componentes Razor não devem manter clientes/timers independentes.
- Eventos mínimos técnicos (somente depuração): tempo até primeiro frame, origem WebRTC/HTTP, peers, falhas, timeouts, consumo estimado de buffer e descarte.
- Impedir vazamento de handlers, streams, object URLs, timers, listeners, conexão peer e objetos torrent.
- A API de catálogo inicialmente pode ser simulada com fixtures; não bloquear a PoC por backend, login ou upload.

## 6. Fallback e riscos técnicos

- Se peer/metadata/buffer atrasar ou houver erro de tracker, iniciar fonte HTTP com suporte a `Range`; um timeout fixo único não atende a todas as redes — começar com limite configurável.
- Evitar que a troca P2P → HTTP reinicie o vídeo sem necessidade; preservar posição se possível e testar.
- Para vídeos raros e menos de 10 usuários simultâneos, é provável que o seeder/HTTP entregue grande parte do tráfego. P2P é experimento arquitetural, não garantia de economia.
- Browsers via WebRTC podem expor IPs e consumir upload do usuário; isso deve ser comunicado antes de disponibilização pública.
- Testar CORS, codec/contêiner suportado, autoplay bloqueado em dispositivos móveis, consumo de dados e ciclo de vida ao sair da página.
- Torrent não substitui política de conteúdo e autorização quando houver publicação de usuários no futuro.

## 7. PoC e critérios de aceite

**Fase A — player isolado (um vídeo):** reproduzir por WebTorrent com seeder Docker compatível, confirmar bytes provenientes de peer WebRTC (não apenas fallback), pausar, buscar posição, sair e liberar recursos.

**Fase B — feed curto (5 a 10 vídeos):** iniciar vídeo aleatório, pré-carregar dois próximos, navegar para frente e voltar, evitar vídeos repetidos na janela e liberar recursos dos descartados.

**Fase C — resiliência:** sem peers, tracker indisponível, HTTP indisponível, troca rápida de 10 vídeos, rede limitada, suspensão/retorno da aba e navegação em Android; nunca travar interface ou deixar reprodução oculta ativa.

**Fase D — aferição:** medir início do primeiro frame e atraso de transição em rede real; definir metas quantitativas a partir de linha de base observada, sem declarar 'instantâneo' como SLA não validado. Medir uso de memória, volume baixado e upload P2P.

**Aceite funcional mínimo:** primeiro vídeo reproduz, próximo vídeo pré-carregado inicia quando pronto, swipe rápido não gera vazamentos, fallback HTTP funciona sem peers e tudo é comprovado com testes manuais documentados e testes automatizados possíveis.

## 8. Backlog incremental proposto

1. Inspecionar player atual e confirmar o tráfego WebRTC do vídeo Sintel em browser real.
2. Extrair módulo TS de ciclo de vida (start/stop/dispose) preservando Blazor.
3. Introduzir seleção de catálogo e feed vertical com 5 vídeos de exemplo.
4. Introduzir fila de pré-carga e cancelamento por prioridade.
5. Implementar fallback HTTP robusto e estados de erro.
6. Executar matriz desktop/Android e medir qualidade de experiência.
7. Só depois avaliar upload → torrent → seeders Docker e expansão de catálogo.

**Governança:** cada item é uma etapa separada de análise → proposta → aprovação do responsável → implementação → testes → validação. Não abrir PR nem fazer deploy sem pedido explícito.

## 9. Fora de escopo do MVP

Autenticação, refresh tokens, uploads públicos, recomendações com IA, curtidas, métricas de negócio, monetização, replicação em dez VPS, 100 mil torrents ativos e migração para React.

## 10. Referências internas

- `docs/WEBTORRENT_INTEGRATION.md` (documentação de comportamento atual; confirmar contra código)
- `docs/TECHNICAL_REVIEW_2026-10-28.md` (review técnico prioritário)
- `src/Presentations/OnForkHub.Web/wwwroot/js/webtorrentService.js`
- `src/Presentations/OnForkHub.Web` (Blazor e player atual)

> Este documento registra decisões e hipóteses de PoC; não constitui implementação, benchmark nem validação operacional.
