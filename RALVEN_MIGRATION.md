# Corte de geração para Ralven

Ralven é a identidade única da próxima geração suportada do produto. A versão será definida somente no fluxo oficial de release. As gerações anteriores foram declaradas sem suporte e não recebem ponte de execução, pacote, atualizador ou protocolo.

## O que muda

- solução, projetos, namespaces e assemblies usam `Ralven.*`;
- aplicativo, launcher, updater e broker usam executáveis Ralven;
- instalador, AppId, mutex, startup, dados locais e artefatos de release usam Ralven;
- site, dashboard, Worker, recursos, textos e documentação usam Ralven;
- o repositório oficial passa a ser `marquezinii/Ralven`;
- a versão da nova geração é decidida e registrada somente na publicação oficial.

Não existem aliases de binário ou download para versões sem suporte.

## Dados do usuário

Na primeira abertura, o aplicativo faz uma importação unidirecional e por allowlist a partir das raízes locais das gerações anteriores. A importação:

- copia somente preferências, sessão protegida pelo Windows, avatar, histórico e dados de rollback conhecidos;
- nunca sobrescreve dados Ralven existentes;
- nunca apaga ou altera a origem;
- ignora pacotes de atualização, requisições elevadas, logs e telemetria pendente;
- ignora junctions e links simbólicos;
- grava um marcador após concluir para não repetir a migração.

Essa é a única compatibilidade deliberada com gerações sem suporte.

## Infraestrutura externa

Alguns identificadores implantados não são marca pública e não podem ser renomeados apenas no código sem criar novos recursos:

- hostname do Worker em produção;
- nome e UUID do banco D1;
- projeto Firebase e chaves públicas associadas;
- trust anchors de assinatura já provisionados.

Eles ficam isolados e documentados como IDs de infraestrutura. A troca futura exige provisionamento, migração verificada e atualização coordenada; não deve ser simulada com URLs inexistentes.

## Marca oficial

Os anexos recebidos são referências visuais, não instruções executáveis nem evidência de funcionalidades. A biblioteca em `assets/brand` preserva as fontes, hashes, procedência, exports determinísticos, Inter licenciada e tokens consumidos pelo produto.

Ausências deliberadas:

- não há master vetorial aprovado do símbolo ou wordmark;
- não há domínio oficial confirmado;
- não há listagem de loja, nota pública ou métricas de marketing verificadas.

Até esses materiais existirem, nenhuma IA ou pessoa deve inventá-los.

## Critério de conclusão

- nenhum nome anterior aparece como identidade pública ou contrato de execução;
- nomes anteriores aparecem apenas no importador de dados, procedência das fontes e IDs externos imutáveis;
- build, testes, segurança, instalador e superfícies web passam;
- capturas dark/light confirmam o visual Ralven em páginas reais;
- repositório remoto e links oficiais resolvem para `marquezinii/Ralven`.
