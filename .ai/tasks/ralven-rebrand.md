# Rebrand completo para Ralven

## Escopo autorizado

- identidade pública, visual e textual;
- solução, projetos, namespaces, assemblies e executáveis;
- instalador, updater, mutex, startup, dados locais e artefatos;
- aplicativo WPF, site, Worker e dashboard;
- biblioteca oficial a partir dos anexos fornecidos;
- quebra de compatibilidade com gerações anteriores, já declaradas sem suporte.

Não faz parte desta tarefa publicar release, alterar versão, criar tag, promover `main` ou implantar infraestrutura de produção.

## Decisões

- Ralven é o único nome público e técnico suportado.
- Não manter aliases de execução, download ou atualização.
- Preservar somente importação unidirecional e por allowlist de dados pessoais locais; não apagar a origem.
- IDs externos já implantados (Worker/D1/Firebase/trust anchors) permanecem como infraestrutura até existir migração provisionada.
- Mockups são referência visual, não prova de recursos, métricas, domínio, loja ou compatibilidade.
- Tema escuro canônico, tema claro/sistema acessíveis; paleta oficial neutra e Inter incorporada.

## Entregáveis

- [x] corte mecânico da solução para `Ralven.*`;
- [x] executáveis, dados locais, mutex, startup, instalador e artifacts Ralven;
- [x] importador seguro de dados locais antigos;
- [x] biblioteca de marca, exports, fontes, tokens e procedência;
- [x] interface WPF e localização;
- [x] website, Worker e dashboard;
- [x] documentação canônica e inventário de codebase;
- [x] build, testes, safety, installer contract e capturas dark/light;
- [ ] commit, push e PR para `dev/proxima-versao`;
- [ ] rename do repositório GitHub para `marquezinii/Ralven`.

## Restrições operacionais deste turno

O usuário posteriormente liberou intervenções locais. Ainda assim, nenhuma
validação funcional pede elevação desnecessária: o instalador foi construído e
validado sem ser instalado, e nenhuma infraestrutura remota foi implantada.

## Validação

- `dotnet format Ralven.slnx --no-restore`;
- build `Release` sem avisos ou erros;
- 1.011/1.011 testes aprovados pelo executável xUnit v3;
- `scripts/Verify-Safety.ps1` aprovado;
- contrato e build completo do instalador aprovados;
- testes de site, Worker e dashboard aprovados, incluindo dry-run do Worker;
- capturas reais dark/light inspecionadas e contraste do CTA desabilitado
  corrigido no estilo compartilhado.
