# Rebranding Vemryx One

Branch: `task/rebrand-vemryx-one`, baseada em `origin/dev/proxima-versao` no commit `c110073`.

Fonte: `REBRANDING_VEMRYX_ONE.md`. O ícone oficial fornecido pelo usuário está preservado em `docs/brand/vemryx-one-icon-source.png`.

## Estado

- [x] Briefing integral lido e incorporado ao repositório.
- [x] Inventário inicial: 432 arquivos contêm o nome legado; cada ocorrência deve ser classificada, não substituída cegamente.
- [x] Ativo 5M removido e exports do ícone V/1 criados para app, site, documentação e dashboard.
- [ ] Aplicar tokens visuais Vemryx e validar contraste/capturas.
- [ ] Migrar identidade pública e localização para Vemryx One.
- [ ] Projetar e testar compatibilidade de caminhos, mutex, persistência e sessões.
- [ ] Implementar release ponte do instalador/updater antes de renomear executáveis e IDs externos.
- [ ] Renomear projetos, arquivos e namespaces internos após estabilizar a compatibilidade.
- [ ] Atualizar backend, site, documentação, CI e allowlist de resíduos.
- [ ] Executar matriz final de build, testes, instalação, atualização, rollback e acessibilidade.

## Identificadores deliberadamente preservados nesta fase

- assemblies, namespaces e nomes de projetos `FiveMCleaner.*`;
- executáveis e layout de runtime usados pelo updater;
- `%LOCALAPPDATA%\FiveMCleaner`, mutexes, entropia DPAPI e chaves persistidas;
- AppId do instalador, produto/manifesto assinado, URLs e nomes de artefatos de release.

Eles só mudam junto de aliases, migração idempotente, testes e rollback. O nome `FiveM` continua legítimo para a integração com FiveM.

## Validação da fase de ativos

- `dotnet test tests/FiveMCleaner.Tests/FiveMCleaner.Tests.csproj --configuration Release --no-restore`: 979 aprovados.
- `dotnet build FiveMCleaner.slnx --configuration Release --no-restore`: aprovado, 0 warnings e 0 erros após restore completo do worktree.
- `scripts/Verify-Installer.ps1 -ScriptOnly`: contrato do instalador aprovado.
- `dotnet format FiveMCleaner.slnx --verify-no-changes --no-restore`: aprovado.
- `scripts/Verify-Safety.ps1`: aprovado; recompilou a solução e executou os 979 testes.
- `scripts/Install-DevelopmentShortcut.ps1 -Build`: build espelhado e atalho de desenvolvimento reconstruídos.
- `BrandAssetTests` verifica fonte/PNG idênticos, alpha, tamanhos do ICO e ausência dos ativos 5M antigos.
