# Hardening do motor de otimização

- Agente: Codex
- Branch: `feat/adaptive-windows-optimization`
- Objetivo: auditar as 66 ações existentes e fechar falhas de execução,
  pós-condição, snapshot, rollback e resultado sem adicionar novos tweaks.
- Status: concluído localmente e pronto para integração.

## Mudanças

- Snapshots de limpeza, entitlement e `commandline.txt` foram vinculados aos
  caminhos canônicos da ação; adulteração e conflitos falham fechados.
- Quarentenas apagam somente arquivos declarados no manifesto e compensam o
  scope corrente mesmo se houver cancelamento no meio do primeiro lote.
- Escritas de registro, plano de energia e ASPM agora têm pós-verificação,
  compensação e proteção contra alterações concorrentes do usuário.
- ASPM captura AC/DC e o GUID do plano separadamente, usa a ABI nativa correta
  e preserva os dois valores no rollback sem operar em outro plano.
- O motor separa `Verified` de `Skipped`, não afirma rollback de ação
  irreversível, recupera quarentena pré-commit e finaliza journals interrompidos
  como erro auditável.
- Falha ou cancelamento do broker preserva ações locais já confirmadas.
- Histórico persiste o perfil real; a UI voltou a explicar o bloqueio seguro
  do FiveM Enhanced.
- Textos que prometiam lock de cache ou redução universal de latência foram
  alinhados ao comportamento e à evidência real.

## Validação

- Build Release dos testes: 0 avisos e 0 erros.
- Suíte completa: 1.254/1.254 testes aprovados.
- `scripts/Verify-Safety.ps1 -SkipTests`: aprovado.
- `dotnet format Ralven.slnx --verify-no-changes --no-restore`: aprovado.
- Auditoria NuGet direta/transitiva: nenhum pacote vulnerável encontrado.
- `git diff --check`: aprovado.
- Duas revisões independentes de hardening concluídas; os achados finais foram
  corrigidos e rechecados.

## Decisões e limitações

- Nenhuma otimização foi aplicada na máquina real durante os testes; integrações
  Windows foram validadas por contratos, filesystem temporário e doubles.
- Snapshots antigos incompatíveis de ASPM, `commandline.txt` e entitlement são
  rejeitados; o Histórico não oferece rollback quando a versão não coincide.
- Nenhum push, PR, merge, versão ou release faz parte desta tarefa.
