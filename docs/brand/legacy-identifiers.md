# Identificadores legados permitidos

O nome público do produto é **Vemryx One**. As ocorrências restantes de
`FiveMCleaner` e `fivemcleaner` não são superfície de marca: são contratos
técnicos já publicados ou registros históricos. Não as renomeie sem uma
migração explícita, testada e reversível.

## Contratos de compatibilidade

- repositório `marquezinii/FiveMCleaner` e base do GitHub Pages `/FiveMCleaner`;
- `%LOCALAPPDATA%\FiveMCleaner`, mutex, entrada de inicialização, dados DPAPI e
  nomes de executável/assembly `FiveMCleaner.*` usados pelo instalador, updater,
  hashes, rollback e downgrade;
- aliases de release, runtime ZIP e árvore `artifacts/FiveMCleaner-win-x64`;
- a variável `FIVEMCLEANER_ENVIRONMENT`.

## Infraestrutura já implantada

- Worker, D1, Pages e domínio `fivemcleaner-*` da Cloudflare;
- projeto Firebase `fivemcleaner-app` e seus valores de `aud`/`iss`.

## Registro histórico

- `PROJECT_HISTORY.md`, `CHANGELOG.md` e `docs/superpowers/` preservam nomes,
  caminhos e comandos válidos quando suas decisões foram registradas.

Toda nova ocorrência fora destas categorias deve usar `Vemryx One` para texto
público ou `Vemryx.One.*` para identificadores internos.
