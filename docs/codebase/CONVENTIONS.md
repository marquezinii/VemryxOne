# Convenções de código

## 1) Regras de nomes

| Item | Regra observada | Exemplo | Evidência |
| --- | --- | --- | --- |
| Tipos e arquivos C# | PascalCase; arquivo acompanha o tipo principal | `WindowsTransactionEngine.cs` | `src/Ralven.Windows/Engine/` |
| Métodos/propriedades C# | PascalCase; async termina em `Async` | `ExecuteAsync`, `LoadSettingsAsync` | `AppOptimizationService.cs` |
| Locais/campos privados C# | camelCase, sem prefixo `_` | `journalDirectory`, `brokerClient` | `AppOptimizationService.cs` |
| Constantes C# | PascalCase | `ProductionHost`, `TelemetryPath` | `RemoteServicesOptions.cs` |
| Módulos/funções JavaScript | arquivos e funções em camelCase | `firebaseIdToken.js`, `readBoundedJson` | `infra/cloudflare-worker/src/` |
| Bindings/env vars | `SCREAMING_SNAKE_CASE` | `TELEMETRY_DB`, `ADMIN_CSRF_SECRET` | `wrangler.toml`, `src/index.js` |
| Testes | `*Tests.cs` e `*.test.js`/`*.test.mjs` | `PlanBuilderTests.cs`, `rateLimit.test.js` | `tests/`, `infra/cloudflare-worker/test/` |

## 2) Formatação e análise

- `.editorconfig`: UTF-8, CRLF, newline final e trim; quatro espaços para C#/XAML/MSBuild e dois para Markdown/JSON/YAML.
- C#: nullable e implicit usings habilitados; linguagem 14; `dotnet format Ralven.slnx --no-restore` é o formatador compatível.
- Worker/dashboard: sem linter dedicado configurado; o estilo observado usa dois espaços, aspas simples e ES modules.

Comandos relevantes:

```powershell
dotnet format Ralven.slnx --no-restore
dotnet build Ralven.slnx --configuration Release --no-restore
```

## 3) Imports e módulos

- C# usa namespaces alinhados aos projetos e referências explícitas nos `.csproj`; `Contracts` e `Core` não importam WPF.
- `using` fica no topo; implicit usings reduz imports da BCL.
- Não há política de barrels confirmada; módulos públicos são consumidos diretamente pelo arquivo.

## 4) Erros e logging

- Contratos de execução preservam outcomes tipados (`Skipped`, `Failed`, `RolledBack`, etc.), não `bool` genérico.
- Exceções esperadas são capturadas por tipo; falhas críticas continuam visíveis e falhas opcionais não mascaram a operação principal.
- Broker e Worker devolvem códigos/mensagens genéricos; detalhes permanecem em journal/log local ou logs operacionais.
- Telemetria, crash e relatórios passam por schemas fechados/sanitizadores; nunca registrar tokens, senhas, cookies ou paths pessoais completos.
- Logging relevante: `BrokerDiagnosticsLog`, `UpdaterDiagnostics`, logs locais do app e Sentry somente após confirmação do aviso e com **Relatórios opcionais** ativo.

## 5) UI, localização e testes

- Texto público do WPF usa chaves equivalentes em `Strings.resx`, `Strings.pt-BR.resx` e `Strings.es.resx`; não adicionar texto localizado hardcoded.
- Reutilizar tokens/styles em `Themes/` e `assets/brand/tokens/` antes de criar variante visual.
- Testes .NET ficam por domínio em `tests/Ralven.Tests`; dependências de Windows/rede são substituídas por interfaces, fakes ou doubles.
- Mudanças de bug devem caracterizar comportamento e deixar regressão no menor nível que cubra a causa.

## 6) Git e documentação

- Commits seguem Conventional Commits.
- `main` é público; `dev/proxima-versao` integra; tarefas usam branch/worktree isolado e PR para dev.
- Tarefa comum não publica release, não incrementa versão e não edita o snapshot integrado sem o fluxo de integração.

## 7) Evidências

- `.editorconfig`
- `Directory.Build.props`
- `AGENTS.md`
- `AI_RULES.md`
- `src/Ralven.App/Resources/Strings.resx`
- `src/Ralven.App/Services/CrashReportSanitizer.cs`
