# Plataforma de Compartilhamento de Vídeos com Suporte a Torrent e CDN

Este repositório contém uma plataforma de compartilhamento de vídeos curtos (até 2 minutos) com distribuição híbrida de conteúdo usando torrents e CDN. A aplicação é construída com uma API em C# (ASP.NET Core), front-end em Blazor WebAssembly, WebTorrent para streaming P2P no navegador e suporte a Docker para orquestrar contêineres de seed.


## Visão Geral do Projeto

Este projeto visa criar uma plataforma escalável e distribuída para compartilhamento de vídeos, onde os usuários podem fazer upload de vídeos em diversos formatos, que são convertidos para torrents e disponibilizados via CDN e WebTorrent. Isso permite que o conteúdo seja servido de forma rápida pela CDN e, ao mesmo tempo, aproveita a rede P2P para reduzir o custo de largura de banda e melhorar a distribuição.

## Tecnologias Utilizadas

- **Back-end**: C# com ASP.NET Core para API RESTful
- **Front-end**: Blazor WebAssembly
- **Storage**: Azure Blob Storage ou AWS S3
- **Serverless Functions**: Azure Functions ou AWS Lambada
- **Streaming P2P**: WebTorrent para streaming de torrents diretamente no navegador
- **Contêineres**: Docker para contêineres de seed
- **Conversão para Torrent**: MonoTorrent (biblioteca C# para manipulação de torrents)
- **CDN**: Configurada com suporte a P2P (opções:Azure CDN, Peer5, Streamroot, ou CDNBye)
- **Banco de Dados**: Azure Cosmos DB para metadados dos vídeos e logs de moderação
- **Git Flow**: Gerenciamento de branches

---

## Histórias de Usuário

### 1. Configuração de Infraestrutura

#### História 1.1: Configurar Azure Cosmos DB para Armazenamento de Dados
- **Tarefas**:
  - [ ] Criar uma instância do Cosmos DB com as coleções `Usuarios`, `Videos`, `Categorias` e `Termos`.
  - [ ] Configurar a estrutura de dados para suportar consultas eficientes e garantir escalabilidade.
  - [ ] Implementar política de segurança e backup para o Cosmos DB.
  
#### História 1.2: Configurar Azure Blob Storage para Armazenamento de Vídeos e Torrents
- **Tarefas**:
  - [ ] Criar um Blob Storage com uma estrutura organizada em pastas para armazenar vídeos convertidos e arquivos torrents em lotes de até 100.
  - [ ] Implementar controle de acesso para garantir que os dados estejam seguros.
  - [ ] Configurar monitoramento para alertar sobre erros ou desempenho.

### 2. Desenvolvimento do Backend API

#### História 2.1: Implementar Endpoint de Upload de Vídeo
- **Descrição**: Permitir que os usuários façam upload de vídeos em diferentes formatos.
- **Tarefas**:
  - [ ] Criar o endpoint `/upload` que recebe o vídeo e armazena temporariamente no servidor.
  - [ ] Realizar validação do formato e tamanho do vídeo.

#### História 2.2: Integrar Moderação Automática com Azure Content Moderator
- **Descrição**: Realizar moderação automatizada dos vídeos para garantir conformidade com as diretrizes de conteúdo.
- **Tarefas**:
  - [ ] Configurar o serviço Azure Content Moderator para processar vídeos carregados.
  - [ ] Validar o conteúdo antes da publicação e armazenar os resultados de moderação no Cosmos DB.

#### História 2.3: Implementar Serviço de Conversão de Vídeos para MP4
- **Descrição**: Converter vídeos para o formato `.mp4` para garantir compatibilidade.
- **Tarefas**:
  - [ ] Implementar conversão automática de vídeos para `.mp4`.
  - [ ] Armazenar vídeos convertidos no Blob Storage.

#### História 2.4: Implementar Geração de Torrents para Vídeos Convertidos
- **Descrição**: Gerar arquivos torrent para cada vídeo convertido e armazená-los em lotes.
- **Tarefas**:
  - [ ] Gerar torrent para cada vídeo convertido usando `MonoTorrent`.
  - [ ] Armazenar o caminho do arquivo torrent no Cosmos DB.
  - [ ] Organizar torrents em lotes de 100.

### 3. Seed dos Torrents com Docker

#### História 3.1: Configurar Docker para Seed dos Lotes de Torrents
- **Descrição**: Utilizar Docker para servir arquivos torrents, distribuindo a carga.
- **Tarefas**:
  - [ ] Criar contêineres Docker para realizar seed de torrents.
  - [ ] Organizar contêineres para gerenciar torrents em lotes de 100 arquivos.

#### História 3.2: Implementar Escalabilidade com Azure Kubernetes Service (AKS)
- **Descrição**: Gerenciar a escalabilidade de contêineres Docker para seeds.
- **Tarefas**:
  - [ ] Configurar o AKS para gerenciar e escalar automaticamente os contêineres com base na demanda.

### 4. Desenvolvimento do Frontend em Blazor WebAssembly

#### História 4.1: Criar Interface de Upload de Vídeos
- **Descrição**: Interface amigável para upload de vídeos.
- **Tarefas**:
  - [ ] Desenvolver página de upload conectada ao endpoint `/upload`.
  - [ ] Exibir progresso e feedback ao usuário durante o upload.

#### História 4.2: Implementar Listagem de Vídeos Aprovados
- **Descrição**: Exibir vídeos aprovados para visualização.
- **Tarefas**:
  - [ ] Criar página para listar vídeos aprovados.
  - [ ] Atualizar listagem em tempo real com novos vídeos.

#### História 4.3: Criar Player para Reprodução de Vídeos via Torrent
- **Descrição**: Player para vídeos disponíveis via torrent com WebTorrent.
- **Tarefas**:
  - [ ] Integrar WebTorrent para streaming direto no navegador.
  - [ ] Garantir fallback para CDN em navegadores incompatíveis.

#### História 4.4: Página de Termos e Condições de Uso
- **Descrição**: Página de termos de uso exigindo aceitação antes de acessar o conteúdo.
- **Tarefas**:
  - [ ] Criar página que exibe os Termos e Condições.
  - [ ] Solicitar que o usuário aceite os termos antes de acessar vídeos.

---

## Configuração do Ambiente

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) SDK para desenvolvimetno
- [WebTorrent](https://webtorrent.io/) para gerenciamento de pacotes e dependências WebTorrent
- [Docker](https://www.docker.com/) para contêineres de seed e orquestração

### Configuração Inicial

1. Clone este repositório:
   ```bash
   git clone https://github.com/RondineleG/OnForkHub.git
   cd OnForkHub
