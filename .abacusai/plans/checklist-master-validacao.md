# Plano de Validação e Alinhamento do `CHECKLIST_MASTER.md`

## Contexto
O `CHECKLIST_MASTER.md` está parcialmente desalinhado com o estado atual da codebase. Há evidências de tarefas marcadas como concluídas no corpo do documento, mas dashboards e status agregados ainda mostram progresso zerado em várias seções. Também existem inconsistências estruturais (itens duplicados) e metadados defasados (branch/data).

Objetivo: validar o estado real do código versus o checklist e atualizar o documento para refletir a situação atual com precisão, sem inventar progresso não comprovado.

## Abordagem recomendada
1. Consolidar baseline real da codebase e do documento
   - Confirmar status de arquivos alterados via git.
   - Confirmar quais tarefas têm evidência objetiva de implementação (arquivos existentes, endpoints/testes presentes).
   - Mapear discrepâncias entre:
     - checkboxes de tarefas (`[x]`/`[ ]`),
     - linhas de status por seção,
     - dashboard global.

2. Corrigir inconsistências de metadados e estrutura no checklist
   - Atualizar cabeçalho com branch atual e data de atualização real.
   - Remover/normalizar duplicidade da seção `Task 1.4.1` e linha duplicada de status WebTorrent.
   - Padronizar status agregados para cada bloco conforme os itens já marcados.

3. Recalcular progresso por bloco e geral
   - Recontar tarefas concluídas por fase/seção com base em marcação e evidências em código.
   - Ajustar:
     - `Status Fase 0` e `Progresso` (atualmente divergente de 0.1–0.7 marcadas como concluídas),
     - `Status Video Upload` (atualmente 0/10 apesar de 1.2.1–1.2.10 marcadas ✅),
     - `Status Integration Tests` (atualmente 0/5, mas há implementação parcial em testes).
   - Atualizar tabela de dashboard (`TOTAL`, concluídas e percentual).

4. Validar tasks de integração (1.4.x) com precisão
   - Marcar como concluído/parcial/não iniciado com base em evidência:
     - `test/Presentations/OnForkHub.Api.IntegrationTests/Infrastructure/CustomWebApplicationFactory.cs`
     - `test/Presentations/OnForkHub.Api.IntegrationTests/HealthCheck/HealthCheckTests.cs`
     - `test/Presentations/OnForkHub.Api.IntegrationTests/Endpoints/CategoriesEndpointsTests.cs`
     - `test/Presentations/OnForkHub.Api.IntegrationTests/Auth/AuthenticationTests.cs`
   - Não declarar concluído quando a cobertura do requisito da task estiver apenas parcial.

5. Revisão final de consistência interna do checklist
   - Garantir que totais por fase batem com subtarefas.
   - Garantir que status textuais e percentuais batem com checkboxes.
   - Garantir ausência de seções duplicadas ou conflitantes.

## Evidências já observadas (baseline)
- `CHECKLIST_MASTER.md` contém divergências claras:
  - dashboard mostra `TOTAL concluídas = 0`, mas há múltiplas tasks com `[x]`.
  - `Status Fase 0` e `Progresso: 0/10` conflitam com 0.1–0.7 marcadas concluídas.
  - `Status Video Upload: 0/10` conflita com 1.2.1–1.2.10 marcadas concluídas.
  - seção `Task 1.4.1` aparece duplicada.
- Existem arquivos reais para Integration Tests 1.4.x:
  - `CustomWebApplicationFactory.cs`
  - `HealthCheckTests.cs`
  - `CategoriesEndpointsTests.cs`
  - `AuthenticationTests.cs`
- O repositório está na branch `feature/phase4-user-features` com alterações locais em API e testes.

## Arquivos críticos a modificar
- `CHECKLIST_MASTER.md` (arquivo principal de alinhamento)

## Arquivos de referência para validação (sem alteração funcional)
- `test/Presentations/OnForkHub.Api.IntegrationTests/Infrastructure/CustomWebApplicationFactory.cs`
- `test/Presentations/OnForkHub.Api.IntegrationTests/HealthCheck/HealthCheckTests.cs`
- `test/Presentations/OnForkHub.Api.IntegrationTests/Endpoints/CategoriesEndpointsTests.cs`
- `test/Presentations/OnForkHub.Api.IntegrationTests/Auth/AuthenticationTests.cs`
- `src/Presentations/OnForkHub.Api/Program.cs`

## Reuso de funções/utilitários existentes
- Padrão de integração via `CustomWebApplicationFactory` para comprovar status de tasks 1.4.x.
- Endpoints REST já mapeados no `Program.cs` para validar coerência entre checklist e cobertura de testes.

## Verificação
1. Verificação documental
   - Conferir se todo status agregado corresponde às subtarefas marcadas.
   - Conferir se totais do dashboard batem com a soma das fases.

2. Verificação de evidência em código
   - Confirmar existência e escopo dos arquivos de teste/endpoints citados no checklist.

3. Verificação de execução (quando sair do plan mode)
   - Rodar build e testes para validar que o status reportado é reproduzível:
     - `dotnet build OnForkHub.slnx`
     - `dotnet test OnForkHub.slnx --verbosity minimal`
   - Opcional para coverage:
     - `dotnet test --collect:"XPlat Code Coverage"`

4. Critério de pronto
   - `CHECKLIST_MASTER.md` sem inconsistências internas de progresso.
   - Status por seção condizente com o estado real dos arquivos da codebase.
   - Sem duplicidades estruturais no documento.