<p align="center">
  <img src="docs/assets/icon.png" alt="Ícone do Vemryx One" width="112">
</p>

<h1 align="center">Vemryx One</h1>

<p align="center">
  Diagnóstico, manutenção e otimização transparente para <strong>FiveM sobre GTAV Legacy</strong> no Windows.
</p>

<p align="center">
  <a href="https://marquezinii.github.io/FiveMCleaner/"><strong>Baixar para Windows</strong></a>
  &nbsp;·&nbsp;
  <a href="https://github.com/marquezinii/FiveMCleaner/releases">Releases</a>
  &nbsp;·&nbsp;
  <a href="docs/safety.md">Segurança</a>
  &nbsp;·&nbsp;
  <a href="docs/architecture.md">Arquitetura</a>
</p>

<p align="center">
  <a href="https://github.com/marquezinii/FiveMCleaner/actions/workflows/ci.yml"><img alt="Status da CI" src="https://img.shields.io/github/actions/workflow/status/marquezinii/FiveMCleaner/ci.yml?branch=main&style=flat-square&label=CI"></a>
  <a href="https://github.com/marquezinii/FiveMCleaner/releases/latest"><img alt="Última release" src="https://img.shields.io/github/v/release/marquezinii/FiveMCleaner?style=flat-square&color=06b6d4"></a>
  <img alt="Windows 10 e 11" src="https://img.shields.io/badge/Windows-10%20%7C%2011-2563eb?style=flat-square">
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10-512bd4?style=flat-square">
</p>

![Arte do Vemryx One](docs/assets/hero-vemryx-one.png)

> [!IMPORTANT]
> O Vemryx One suporta somente **FiveM em GTAV Legacy**. GTAV Enhanced é identificado e bloqueado com segurança até existir um adaptador dedicado, pesquisado e testado.

## Uma forma mais clara de cuidar do seu FiveM

Nada de scripts opacos ou promessas irreais. O Vemryx One detecta o ambiente, monta um plano compreensível e mostra o que será alterado antes de executar. Cada ação declara escopo, pré-condições, risco, resultado e rollback quando aplicável.

| Você vê | O que isso significa |
| --- | --- |
| Diagnóstico local | FiveM/GTA V, CPU, GPU, memória, armazenamento, rede, energia, drivers, processos e gargalos prováveis. |
| Plano antes da execução | Perfil, ações, impacto, privilégios e condições de cada alteração ficam explícitos. |
| Execução verificável | Snapshot, validação, journal local, progresso real e recuperação fazem parte do fluxo. |
| Histórico útil | Resultado por ação, relatório técnico sanitizado e comparação local antes/depois. |

## Perfis com escopo conhecido

| Perfil | Foco |
| --- | --- |
| **Leve** | Ajustes suaves, com prioridade para preservar a experiência visual. |
| **Médio** | Equilíbrio entre qualidade, responsividade e consistência. |
| **Agressivo** | Reduz efeitos e opções pesadas para máquinas mais limitadas. |

Perfis são composições de ações conhecidas — não listas genéricas de "tweaks". Manutenção de dados é sempre opt-in; caches protegidos, entitlements, plugins e autenticação nunca são tratados como lixo.

> [!WARNING]
> Nenhum software pode prometer FPS, ping ou ausência de stutter em todo PC ou servidor. O Vemryx One não desativa Defender, Firewall, SmartScreen ou UAC; não injeta código; não modifica binários ou memória do jogo; e não usa prioridade Realtime, afinidade fixa ou debloat genérico.

## Segurança por projeto

Uma alteração persistente passa por **descobrir → planejar → validar → aplicar → verificar → registrar**. O aplicativo principal não fica permanentemente elevado: operações administrativas usam um broker efêmero, tipado e allowlisted.

Dados como `game-storage`, `nui-storage`, `ipfs`, `CitizenFX.ini`, plugins, `ros_id.dat` e entitlements digitais são protegidos. O produto valida instalação, processos, caminhos e reparse points antes de alterar qualquer coisa.

Leia a [política de segurança](docs/safety.md) e as [evidências técnicas](docs/research.md).

## O que já está disponível

| Área | Disponível hoje |
| --- | --- |
| Otimizador | Prévia, perfis, progresso real, cancelamento seguro, resultado por ação e rollback. |
| Conta | Cadastro, login, Google com OAuth 2.0 + PKCE, verificação de e-mail, recuperação, senha, troca de credenciais e exclusão. |
| Privacidade | ID token somente em memória, refresh token protegido por DPAPI e telemetria limitada por consentimento. |
| Relatos de bug | Envio explícito com campos validados, e-mail e trecho de log opcionais — sem anexos automáticos. |
| Atualizações | Feed assinado, validação de origem/tamanho/SHA-256, staging, ativação atômica, health-check e rollback. |
| Operação privada | Dashboard autenticado para métricas e bugs, filtros defensivos e avisos ao vivo para o aplicativo. |

## Instalação e atualização

Baixe apenas pelas [Releases oficiais](https://github.com/marquezinii/FiveMCleaner/releases). Cada release `win-x64` inclui instalador, SHA-256, manifestos e pacotes gerados pelo mesmo workflow.

Após sua confirmação, a atualização verifica origem e integridade, preserva a versão anterior e só confirma a nova versão quando ela sinaliza saúde.

> [!NOTE]
> Os binários ainda não têm assinatura Authenticode pública. SmartScreen pode pedir confirmação por reputação. Confira a origem, a tag e o SHA-256; nunca desative proteções para instalar o aplicativo.

## Privacidade

O diagnóstico é local por padrão. Telemetria e crash reporting são sanitizados, allowlisted e nunca alteram o resultado de uma otimização. O dashboard não recebe arquivos pessoais, credenciais, tokens, cookies, clipboard, paths completos, dumps ou logs completos sem uma ação explícita.

Veja [telemetria](docs/telemetry.md) e [relatos de bug](docs/bug-reports.md).

## Desenvolvimento

Requisitos: Windows 10/11 x64, [.NET SDK 10.0.303](https://dotnet.microsoft.com/download/dotnet/10.0) e Node.js 24.19.0 LTS.

```powershell
dotnet restore Vemryx.One.slnx
dotnet build Vemryx.One.slnx --configuration Release --no-restore
dotnet test Vemryx.One.slnx --configuration Release --no-build
.\scripts\Verify-Safety.ps1
```

Para abrir uma demonstração segura sem FiveM/GTA instalado:

```powershell
.\scripts\Install-DevelopmentShortcut.ps1 -Build
```

Ela usa dados sintéticos e não grava configurações nem executa ações do sistema.

## Arquitetura

```text
App             WPF, conta, configurações e serviços de aplicação
Contracts       DTOs, IDs e contratos duráveis
Core            políticas, catálogo, planos e rollback
Windows         descoberta e integrações de Windows/FiveM
Broker          operações administrativas tipadas e allowlisted
Launcher/Updater atualização transacional e supervisão pós-update
Worker/Dashboard backend de conta, telemetria, bugs e operação privada
Website         site público estático
```

Consulte [docs/architecture.md](docs/architecture.md) para as fronteiras completas.

---

Vemryx One é um projeto comunitário independente, sem afiliação, endosso ou patrocínio de Rockstar Games, Cfx.re ou FiveM. O repositório e as URLs de distribuição ainda usam `FiveMCleaner` por compatibilidade durante a transição de marca.
