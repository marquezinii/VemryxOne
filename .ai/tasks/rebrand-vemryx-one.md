# Rebranding Vemryx One

Branch: `task/rebrand-vemryx-one`, baseada em `origin/dev/proxima-versao` no commit `c110073`.

Fonte: `REBRANDING_VEMRYX_ONE.md`. O ícone oficial fornecido pelo usuário está preservado em `docs/brand/vemryx-one-icon-source.png`.

## Estado

- [x] Briefing integral lido e incorporado ao repositório.
- [x] Inventário inicial: 432 arquivos contêm o nome legado; cada ocorrência deve ser classificada, não substituída cegamente.
- [x] Ativo 5M removido e exports do ícone V/1 criados para app, site, documentação e dashboard.
- [x] Aplicar tokens visuais Vemryx e validar contraste/capturas.
- [x] Migrar identidade pública e localização para Vemryx One.
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

## Validação da fase visual

- Paletas escura e clara do aplicativo, callback OAuth, site público e dashboard alinhados aos tokens Vemryx; laranja preservado apenas como acento da categoria de jogos.
- `ThemeTokenContractTests` verifica os tokens centrais e contraste; 993 testes .NET aprovados.
- `dotnet build FiveMCleaner.slnx --configuration Release --no-restore`: aprovado, 0 warnings e 0 erros.
- Site: build estático Next.js e 3 testes aprovados. O ambiente tinha Node.js 26.7.0, acima do Node.js 24 declarado, mas a validação terminou sem falhas.
- Dashboard: 48 testes aprovados, incluindo contrato da paleta e gráficos.
- Oito capturas reais (`Overview`, `Optimizer`, `History`, `Settings`, nos temas claro e escuro) revisadas; foco, seleção, navegação, gradientes e texto de botões permanecem legíveis.

## Validação da fase de identidade pública

- `Vemryx One` substitui o nome legado nas superfícies públicas do aplicativo, bandeja, callback OAuth, relatório técnico e catálogos em inglês, português e espanhol.
- `ProductIdentity.Name`, assemblies, caminhos e protocolos continuam deliberadamente legados até a fase de compatibilidade; `ProductIdentity.DisplayName` separa a marca pública sem migração prematura.
- `LocalizedInterfaceContractTests` cobre as novas chaves e impede o retorno do nome legado aos três catálogos públicos.
- `dotnet build FiveMCleaner.slnx --configuration Release --no-restore`: aprovado, 0 warnings e 0 erros.
- `dotnet test FiveMCleaner.slnx --configuration Release --no-build`: 994 testes aprovados.
- `dotnet format FiveMCleaner.slnx --verify-no-changes --no-restore`: aprovado.
- `scripts/Install-DevelopmentShortcut.ps1 -Build`: build espelhado e atalho de desenvolvimento reconstruídos.
