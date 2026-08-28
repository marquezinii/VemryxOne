# Pontos de atenção do código

## 1) Riscos priorizados

| Severidade | Ponto | Evidência | Impacto | Ação sugerida |
| --- | --- | --- | --- | --- |
| alta | Binários públicos ainda sem Authenticode | `README.md`, `docs/release-preview.md` | SmartScreen depende de reputação e o hash não prova identidade do editor | Adotar certificado e manter hash/assinatura de manifestos |
| alta | Transação, broker e updater são superfícies críticas e extensas | `WindowsTransactionEngine.cs`, `Ralven.Broker/Program.cs`, `Ralven.UpdateRuntime/` | Regressão pode afetar dados, rollback, elevação ou cadeia de update | Alterações pequenas, testes de falha e validação de artefato completa |
| média | IDs Cloudflare/Firebase implantados conservam nomes anteriores | `docs/brand/infrastructure-identifiers.md`, `wrangler.toml` | Renomear por engano criaria recurso novo ou quebraria clientes | Tratar como IDs imutáveis até migração operacional planejada |
| média | Development e Production compartilham Worker/D1 | comentário e coluna `environment` em `wrangler.toml`/migrations | Isolamento é lógico; erro de filtro aumenta blast radius | Preservar validators/testes; separar recursos só com plano de dados |
| média | Não há baseline automatizada de performance | `.github/workflows/ci.yml` | Regressões de startup/diagnóstico podem passar sem sinal quantitativo | Medir antes de definir threshold; adicionar benchmark apenas com meta real |

## 2) Dívida técnica

| Item | Evidência | Risco se ignorado | Tratamento seguro |
| --- | --- | --- | --- |
| `AppOptimizationService.cs` tem cerca de 1.228 linhas | `src/Ralven.App/Services/AppOptimizationService.cs` | Diagnóstico, settings e orquestração têm alta superfície de regressão | Extrair somente quando uma mudança real atravessar responsabilidades, preservando testes |
| `WindowsTransactionEngine.cs` tem cerca de 1.054 linhas | `src/Ralven.Windows/Engine/WindowsTransactionEngine.cs` | Semântica de outcomes/cancelamento/rollback fica difícil de revisar | Não refatorar junto com feature; caracterizar cada fluxo antes |
| `Controls.xaml` tem cerca de 912 linhas | `src/Ralven.App/Themes/Controls.xaml` | Mudança de style pode afetar muitas telas | Reusar tokens e validar dark/light, teclado e localização |
| Router do Worker concentra cerca de 535 linhas | `infra/cloudflare-worker/src/index.js` | Alteração de rota pode cruzar auth, CORS e rate limiting | Manter handlers de domínio separados e testes por rota |

Não foram encontrados marcadores `TODO`, `FIXME` ou `HACK` em código de produção no inventário atual; os `[TODO]` destes mapas representam dados não versionados, não dívida confirmada.

## 3) Segurança

| Risco | Categoria | Mitigação atual | Gap |
| --- | --- | --- | --- |
| Elevação administrativa | OWASP A01 / fronteira local | broker efêmero, operação tipada, allowlist, revalidação e timeout | toda operação nova exige análise e testes de ampliação de escopo |
| Substituição/downgrade de update | supply chain | assinatura, origem, tamanho, SHA-256, staging, version floor e health receipt | falta Authenticode público nos executáveis |
| Abuso de rotas públicas | OWASP A04/A05 | body limitado, schemas fechados, CORS, CSRF e rate limits | monitoramento/SLO operacional não está versionado |
| Exposição de dados | OWASP A02 | consentimento, DPAPI, sanitização e payload allowlisted | rotação formal de secrets não está documentada no repo |

## 4) Performance e escala

| Ponto | Evidência | Sintoma atual | Risco | Próximo passo |
| --- | --- | --- | --- | --- |
| Diagnóstico local | `AppOptimizationService.DiagnoseAsync` paraleliza sondas independentes | nenhum problema confirmado | novas sondas podem serializar I/O pesado | medir duração por etapa antes de otimizar |
| Fila de telemetria | limite de 200 itens/14 dias em `CloudflareTelemetryService.cs` | crescimento já limitado | perda deliberada dos eventos mais antigos em indisponibilidade longa | manter métrica de falhas e limites alinhados ao Worker |
| Queries D1/dashboard | `src/stats/queries.js`, migrations | nenhum gargalo confirmado | volume futuro pode elevar latência/custo | observar latência e plano de query antes de indexar |
| UI/XAML | views e styles extensos | nenhum travamento confirmado | regressões de layout e renderização | usar capturas reais e não micro-otimizar sem medição |

## 5) Áreas frágeis e de alto churn

O histórico de 90 dias aponta maior churn em `PROJECT_STATE.md`, janela principal, resources/localização, `MainViewModel`, Worker, instalador e workflows de release. Parte do log ainda registra os caminhos anteriores ao rebranding; os equivalentes atuais são:

| Área atual | Sinal histórico | Estratégia segura |
| --- | --- | --- |
| `src/Ralven.App/MainWindow*.xaml*` e `Views/` | janela principal lidera alterações de UI | captura dark/light, teclado e teste de resources |
| `src/Ralven.App/Resources/Strings*.resx` | chaves mudam junto com UX | manter en/pt-BR/es e placeholders idênticos |
| `src/Ralven.App/ViewModels/MainViewModel*.cs` | orquestra muitos fluxos | alterar partial correto e rodar testes focados + suíte |
| `infra/cloudflare-worker/src/index.js` | router mudou repetidamente | validar auth/CORS/rate limit em todas as rotas tocadas |
| `installer/Ralven.iss` e `.github/workflows/release.yml` | identidade e release têm churn | usar scripts oficiais; não editar release junto com tarefa comum |

## 6) `[ASK USER]`

1. Nenhuma pergunta de intenção está pendente para operar o estado atual. A separação lógica Development/Production e a preservação dos IDs externos estão explicitamente documentadas como decisões vigentes.

## 7) Intenção versus realidade

- Não foi confirmada divergência funcional entre `PRODUCT.md`/`docs/architecture.md` e os módulos atuais.
- A identidade pública e os assemblies são Ralven; os identificadores externos com nomes anteriores permanecem apenas por continuidade da infraestrutura, conforme documentação específica.
- A promessa de cadeia de release segura já existe no código; a ausência de Authenticode é declarada publicamente e continua sendo o principal gap, não uma capacidade ocultamente prometida.

## 8) Evidências

- `README.md`
- `PRODUCT.md`
- `docs/architecture.md`
- `docs/release-preview.md`
- `docs/brand/infrastructure-identifiers.md`
- `src/Ralven.App/Services/AppOptimizationService.cs`
- `src/Ralven.Windows/Engine/WindowsTransactionEngine.cs`
- `infra/cloudflare-worker/wrangler.toml`
- `.github/workflows/ci.yml`
- Histórico obtido com `git log --since="90 days ago" --name-only`
