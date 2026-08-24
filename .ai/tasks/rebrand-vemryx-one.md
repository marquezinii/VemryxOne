# Rebranding Vemryx One

Branch: `task/rebrand-vemryx-one`, baseada em `origin/dev/proxima-versao` no commit `c110073`.

Fonte: `REBRANDING_VEMRYX_ONE.md`. O ícone oficial fornecido pelo usuário está preservado em `docs/brand/vemryx-one-icon-source.png`.

## Estado

- [x] Briefing integral lido e incorporado ao repositório.
- [x] Inventário inicial: 432 arquivos contêm o nome legado; cada ocorrência deve ser classificada, não substituída cegamente.
- [x] Ativo 5M removido e exports do ícone V/1 criados para app, site, documentação e dashboard.
- [x] Aplicar tokens visuais Vemryx e validar contraste/capturas.
- [x] Migrar identidade pública e localização para Vemryx One.
- [x] Projetar e testar compatibilidade de caminhos, mutex, persistência e sessões.
- [x] Implementar release ponte do instalador/updater antes de renomear executáveis e IDs externos.
- [x] Renomear projetos, arquivos e namespaces internos após estabilizar a compatibilidade.
- [ ] Atualizar backend, site, documentação, CI e allowlist de resíduos.
- [ ] Executar matriz final de build, testes, instalação, atualização, rollback e acessibilidade.

## Identificadores deliberadamente preservados nesta fase

- assemblies, namespaces e nomes de projetos `FiveMCleaner.*`;
- executáveis e layout de runtime usados pelo updater;
- `%LOCALAPPDATA%\FiveMCleaner`, aliases de mutex/startup, sessão DPAPI e chaves persistidas;
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

## Contrato da fase de compatibilidade

- A raiz ativa permanece `%LOCALAPPDATA%\FiveMCleaner` até a release-ponte. O destino será `%LOCALAPPDATA%\Vemryx\One`; a migração deve copiar, verificar e preservar a origem inicialmente, sem criar duas fontes graváveis de verdade.
- O inventário migrável inclui `settings.json`, `history.json`, `firebase.session`, avatares, journals transacionais, logs, telemetria pendente e staging de updates. Journals e estados do updater mantêm seus formatos e IDs duráveis.
- A sessão Firebase usa DPAPI `CurrentUser` sem entropia opcional; um teste move o blob criptografado entre as duas raízes e comprova leitura pelo mesmo usuário sem expor o token.
- O mutex/evento e o valor `HKCU\...\Run\FiveMCleaner` permanecem aliases ativos. A release-ponte deve reconhecer o alias antigo antes de ativar nomes Vemryx; não deve registrar dois lançamentos automáticos.
- `ProductIdentity.Name`, o `AppId` estável do Inno Setup, os executáveis e o layout assinado continuam legados por serem contratos de plano, instalação, downgrade e update — não superfícies públicas.

## Validação da fase de compatibilidade

- Regressões fixam os aliases legados do mutex, evento de ativação e registro de startup até a release-ponte.
- `SecureSession_RemainsReadableAfterMovingToTheNewProductDirectory` comprova a portabilidade do blob DPAPI entre a raiz legada e `%LOCALAPPDATA%\Vemryx\One` no mesmo usuário.
- `dotnet test FiveMCleaner.slnx --configuration Release --no-restore`: 998 testes aprovados.
- `dotnet build FiveMCleaner.slnx --configuration Release --no-restore`: aprovado, 0 warnings e 0 erros.
- `dotnet format FiveMCleaner.slnx --verify-no-changes --no-restore`: aprovado após normalização CRLF dos arquivos editados.
- `scripts/Verify-Safety.ps1`: aprovado; recompilou a solução e executou os 998 testes.
- `scripts/Verify-Installer.ps1 -ScriptOnly`: contrato legado do instalador aprovado.
- `scripts/Install-DevelopmentShortcut.ps1 -Build`: build espelhado e atalho de desenvolvimento reconstruídos.

## Release-ponte do instalador e updater

- O instalador público passa a se chamar `Vemryx One` e gera `VemryxOne-Setup-<versão>-win-x64.exe`; instalações novas usam `{autopf}\Vemryx One`.
- A mesma release gera `FiveMCleaner-Setup-<versão>-win-x64.exe` e o alias estável legado com os mesmos bytes e SHA-256. A atualização standalone antiga ainda encontra esse nome esperado e instala o produto renomeado in-place.
- `AppId`, `SetupMutex`, launcher, runtime ZIP, executáveis, valor de inicialização, dados locais e políticas de assinatura continuam tecnicamente legados. Isso preserva atualização, rollback e downgrade até a migração controlada dos identificadores externos.
- O workflow publica os dois aliases e atesta ambos; `Test-Installer.ps1` valida `Vemryx One` no registro de desinstalação, preservando a verificação do launcher e do valor de startup legado.

## Validação da release-ponte

- `InstallerBridgeContractTests` impede que o instalador público, o alias legado, o `AppId` estável ou o launcher técnico se desencontrem.
- `scripts/Build-Installer.ps1 -Version 1.4.3`: aprovado; os dois instaladores produzidos têm 146.049.695 bytes e SHA-256 `2bbe3142160108b52932d779e827fd725d0b099b504a3934553de4843b0c7cd2`.
- `scripts/Test-Installer.ps1`: aprovado; instalação, upgrade, hash de 721 arquivos, remoção das tasks e desinstalação silenciosa com preservação de dados locais.
- `dotnet build FiveMCleaner.slnx --configuration Release --no-restore`: aprovado, 0 warnings e 0 erros.
- `dotnet test FiveMCleaner.slnx --configuration Release --no-restore`: 999 testes aprovados.
- `dotnet format FiveMCleaner.slnx --verify-no-changes --no-restore`, `scripts/Verify-Safety.ps1` e `scripts/Verify-Installer.ps1 -ScriptOnly`: aprovados.

## Renomeação interna de projetos e namespaces

- A solução passou a ser `Vemryx.One.slnx`; projetos, diretórios, referências e namespaces internos usam `Vemryx.One.*`.
- `VemryxOneJson` substitui o auxiliar interno com o nome legado. Os assemblies e executáveis que compõem o layout instalado continuam explicitamente `FiveMCleaner.*`, preservando contratos de updater, instalador, hashes e rollback.
- O gatilho interno de hardening agora é `VemryxOneHarden`; a publicação continua gerando e verificando os binários legados exigidos pelo runtime.

## Validação da renomeação interna

- `dotnet build Vemryx.One.slnx --configuration Release`: aprovado, 0 avisos e 0 erros.
- `dotnet test Vemryx.One.slnx --configuration Release --no-build`: 999 testes aprovados.
- `dotnet format Vemryx.One.slnx --verify-no-changes --no-restore` e `git diff --check`: aprovados.
- `scripts/Build-Portable.ps1 -Runtime win-x64 -Configuration Release -Harden`: aprovado; o pacote mantém `FiveMCleaner.*` apenas como layout de compatibilidade.
- `scripts/Test-NoUnobfuscatedAssemblies.ps1`: aprovou a árvore publicada, o bundle do launcher e os dois ZIPs; nenhuma cópia não ofuscada, PDB ou mapa de ofuscação foi encontrado.
