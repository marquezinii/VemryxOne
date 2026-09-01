# Gate de diagnósticos da release pública

- Agente: Codex
- Branch: `feat/telemetry-privacy-experience`
- Objetivo: impedir uma release stable sem Worker/D1, dashboard e Sentry aptos a receber os diagnósticos da versão.
- Status: implementado e validado em 01/09/2026.

## Alterações

- O workflow stable migra e publica o Worker antes de criar a GitHub Release.
- Um smoke sintético valida o aceite do Worker, a persistência do contrato completo no D1 e a disponibilidade do dashboard; a linha sintética é removida no final.
- O mesmo gate exige que o Sentry aceite um crash sintético de release sem dados de usuário.
- O artefato passa a exigir a configuração Production e o SDK Sentry corretos.
- O fechamento do SDK recebeu janela explícita de cinco segundos para flush.

## Limites

- Nenhuma migration, publicação ou release de produção foi executada por esta tarefa.
- O evento sintético do Sentry fica retido e é identificado pela tag `ralven.release_smoke=true`.

## Validação

- Build Release: zero avisos e zero erros.
- Testes .NET: 1.319 aprovados.
- Worker: 239 testes aprovados; dashboard: 51 testes aprovados.
- Instalador `1.5.1`: contrato de fonte e artefato aprovados.
- Safety, format, YAML/PowerShell, diff e auditoria NuGet aprovados.
- O preflight remoto recusou o D1 público ainda pré-migration antes de enviar novo evento; o smoke anterior foi removido e a consulta final confirmou zero linhas sintéticas.
