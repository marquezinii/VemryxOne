---
title: "Rebranding Estratégico e Operacional — FiveMCleaner → Vemryx One"
document_id: "BRAND-REB-001"
version: "1.0.0"
status: "Direção estratégica definida; validação jurídica e implementação pendentes"
date: "2026-08-22"
owner: "Vemryx"
product: "Vemryx One"
previous_name: "FiveMCleaner"
primary_language: "pt-BR"
classification: "Documento interno de marca, produto, design, engenharia e lançamento"
---

# Rebranding Estratégico e Operacional — FiveMCleaner → Vemryx One

> **Documento-fonte do rebranding.** Este arquivo consolida a estratégia de mudança de **FiveMCleaner** para **Vemryx One**, cobrindo naming, posicionamento, arquitetura de marca, identidade visual, linguagem, interface, produto, migração técnica, distribuição, comunicação, aspectos jurídicos, testes, governança e critérios de conclusão.

## Avisos importantes

1. **`Vemryx One` é o nome estratégico recomendado e adotado neste documento**, mas o lançamento público definitivo deve ocorrer somente após busca formal de anterioridade e análise de marcas semelhantes.
2. Este documento **não substitui aconselhamento jurídico**. Registro de marca, classificação, contratos, termos e uso de marcas de terceiros devem ser confirmados por profissional adequado quando necessário.
3. O rebranding **não autoriza prometer recursos que ainda não existem**. A mensagem pública deve acompanhar o estágio real do produto.
4. A implementação técnica não deve ser feita por substituição global cega. Identificadores públicos, persistidos, internos e vinculados ao atualizador exigem estratégias diferentes.
5. Segurança, reversibilidade, transparência e preservação de dados são requisitos obrigatórios da migração.
6. O nome antigo deve continuar reconhecido em caminhos de migração, busca, suporte e atualização pelo período necessário.

---

## Sumário

1. [Resumo executivo](#1-resumo-executivo)
2. [Contexto e motivação](#2-contexto-e-motivação)
3. [Objetivos, escopo e não objetivos](#3-objetivos-escopo-e-não-objetivos)
4. [Decisões centrais](#4-decisões-centrais)
5. [Arquitetura de marca](#5-arquitetura-de-marca)
6. [Estratégia de naming](#6-estratégia-de-naming)
7. [Posicionamento de mercado](#7-posicionamento-de-mercado)
8. [Públicos e necessidades](#8-públicos-e-necessidades)
9. [Fundamentos da marca](#9-fundamentos-da-marca)
10. [Sistema de mensagens](#10-sistema-de-mensagens)
11. [Voz, tom e terminologia](#11-voz-tom-e-terminologia)
12. [Identidade visual](#12-identidade-visual)
13. [Sistema de interface e UX](#13-sistema-de-interface-e-ux)
14. [Arquitetura do produto](#14-arquitetura-do-produto)
15. [Alinhamento entre rebranding e expansão](#15-alinhamento-entre-rebranding-e-expansão)
16. [Rebranding técnico](#16-rebranding-técnico)
17. [Estratégia de migração](#17-estratégia-de-migração)
18. [Instalador, atualizador e distribuição](#18-instalador-atualizador-e-distribuição)
19. [Backend, autenticação, pagamentos e telemetria](#19-backend-autenticação-pagamentos-e-telemetria)
20. [Segurança do rebranding](#20-segurança-do-rebranding)
21. [Testes e garantia de qualidade](#21-testes-e-garantia-de-qualidade)
22. [Website, SEO e documentação](#22-website-seo-e-documentação)
23. [GitHub, Discord, suporte e canais](#23-github-discord-suporte-e-canais)
24. [Aspectos jurídicos e propriedade intelectual](#24-aspectos-jurídicos-e-propriedade-intelectual)
25. [Plano de lançamento e comunicação](#25-plano-de-lançamento-e-comunicação)
26. [Métricas e indicadores](#26-métricas-e-indicadores)
27. [Riscos e mitigações](#27-riscos-e-mitigações)
28. [Governança da marca](#28-governança-da-marca)
29. [Regras para agentes de IA e implementadores](#29-regras-para-agentes-de-ia-e-implementadores)
30. [Checklist mestre](#30-checklist-mestre)
31. [Critérios de aceite e Definition of Done](#31-critérios-de-aceite-e-definition-of-done)
32. [Decisões pendentes](#32-decisões-pendentes)
33. [Mapa de substituição](#33-mapa-de-substituição)
34. [Apêndices técnicos](#34-apêndices-técnicos)
35. [Referências](#35-referências)

---

# 1. Resumo executivo

O **FiveMCleaner** nasceu como uma ferramenta especializada em limpeza, diagnóstico e otimização para FiveM. O produto, porém, está evoluindo para uma proposta maior: simplificar tarefas técnicas, repetitivas e trabalhosas no Windows por meio de ações guiadas, seguras, explicadas e reversíveis.

A identidade atual limita essa evolução por três razões principais:

- `FiveM` é uma marca de terceiro e não deve permanecer como elemento central da identidade comercial de um produto independente;
- `Cleaner` reduz a percepção do produto a limpeza de cache, temporários e armazenamento;
- o nome não comporta adequadamente automações, diagnóstico, configuração assistida, integrações, produtividade e otimização geral do computador.

A decisão estratégica é migrar para:

# **Vemryx One**

## Tagline principal

> **Seu PC, simplificado.**

## Descritor de categoria

> **Automação, desempenho e configuração para Windows.**

## Posicionamento resumido

> **Vemryx One é uma central para otimizar, configurar, diagnosticar e automatizar tarefas do Windows com transparência, segurança e possibilidade de desfazer alterações.**

## Princípios inegociáveis

- FiveM passa a ser **uma integração**, não a identidade do produto.
- O aplicativo não será apresentado como “booster mágico”, “milagre de FPS” ou scareware.
- Toda ação relevante deve explicar o que fará, por que fará, qual impacto pode causar, quais permissões exige e como desfazer.
- A mudança não pode causar perda de configurações, backups, históricos, sessões, licenças ou capacidade de atualização.
- A nova identidade deve funcionar em tamanhos pequenos, em uma cor e em alto contraste.
- O nome `Vemryx One` não será traduzido.
- O nome antigo continuará reconhecido em SEO, suporte, atualização e migração durante a transição.

---

# 2. Contexto e motivação

## 2.1. Dependência de uma marca de terceiro

FiveM é marca registrada de terceiro. Mesmo quando a menção descritiva para indicar compatibilidade pode ser legítima, construir a identidade principal do produto em torno dessa marca gera riscos estratégicos:

- aparência de afiliação oficial;
- dependência comercial de um ecossistema controlado por terceiros;
- dificuldade de expansão para outros jogos e aplicativos;
- possíveis restrições em anúncios, lojas, parcerias e registro de marca;
- fragilidade caso regras, licenças ou políticas mudem;
- dificuldade de formar patrimônio de marca próprio.

A estratégia correta é usar `FiveM` somente quando necessário para descrever compatibilidade, integração ou funcionalidade específica.

## 2.2. Limitação semântica de “Cleaner”

O termo `Cleaner` comunica principalmente:

- remoção de arquivos temporários;
- limpeza de cache;
- liberação de espaço;
- manutenção básica.

O produto futuro pretende incluir:

- otimização e configuração de desempenho;
- diagnóstico e correção orientada;
- instalação e configuração de ferramentas;
- automações multietapas;
- perfis para jogos e aplicativos;
- configuração assistida de ReShade;
- manutenção geral do Windows;
- restauração e histórico de alterações;
- produtividade e conveniência;
- possíveis recursos de assistência inteligente.

O nome antigo representa apenas uma fração do produto.

## 2.3. Momento adequado para a mudança

O rebranding deve ocorrer antes de:

- crescimento expressivo da base de usuários;
- monetização consolidada;
- assinatura digital e reputação pública mais forte;
- parcerias de maior alcance;
- expansão para múltiplos jogos e aplicativos;
- grande volume de documentação e vídeos externos;
- dependência técnica mais profunda de identificadores antigos.

Quanto mais tarde a mudança acontecer, maior será o custo de migração, a perda de reconhecimento e o risco de inconsistência.

## 2.4. Oportunidade criada

A nova marca permite reposicionar o aplicativo não como “mais um cleaner”, mas como:

> **Uma central operacional para remover atrito técnico do Windows.**

Isso cria uma categoria própria entre manutenção, automação, diagnóstico e configuração assistida.

---

# 3. Objetivos, escopo e não objetivos

## 3.1. Objetivos estratégicos

- Remover a dependência identitária de FiveM.
- Permitir expansão para Windows, jogos, aplicativos e produtividade.
- Construir reconhecimento em torno da marca proprietária `Vemryx`.
- Posicionar o produto como moderno, confiável, transparente e reversível.
- Criar uma identidade internacionalizável.
- Sustentar planos gratuito, Pro e empresarial.
- Facilitar parcerias sem aparentar afiliação indevida com terceiros.

## 3.2. Objetivos de produto

- Unificar manutenção, automação, desempenho, diagnóstico e integrações.
- Organizar ações pela intenção do usuário, não apenas por detalhes técnicos.
- Manter FiveM como integração de destaque durante a transição.
- Preparar o produto para ReShade, GTA V, outros jogos e tarefas gerais.
- Transformar tarefas longas em fluxos orientados de poucos passos.

## 3.3. Objetivos visuais

- Substituir o monograma `5M` por um símbolo proprietário e escalável.
- Abandonar a estética excessivamente metálica de “game booster antigo”.
- Adotar preto, grafite, índigo e ciano como base visual.
- Manter laranja apenas como acento temático da área de jogos, quando útil.
- Criar variantes claras, escuras, monocromáticas e de alto contraste.
- Garantir legibilidade e reconhecimento em tamanhos pequenos.

## 3.4. Objetivos técnicos

- Preservar atualização in-place sempre que possível.
- Migrar dados de forma atômica, verificável e idempotente.
- Evitar duas instalações independentes.
- Manter compatibilidade com configurações e versões anteriores.
- Atualizar identificadores públicos sem quebrar IDs internos necessários.
- Preservar histórico de telemetria, licenças e contas.

## 3.5. Não objetivos

O rebranding não deve justificar:

- reescrever todo o aplicativo de uma vez;
- trocar framework sem necessidade;
- refatorar áreas não relacionadas sem testes;
- lançar funcionalidades inexistentes apenas para combinar com a nova mensagem;
- alterar políticas de privacidade silenciosamente;
- apagar dados antigos logo após a primeira execução;
- remover a especialização em FiveM;
- transformar cada recurso em uma submarca;
- mudar IDs de backend apenas por estética.

---

# 4. Decisões centrais

| Área | Decisão |
|---|---|
| Empresa | **Vemryx** |
| Produto | **Vemryx One** |
| Nome anterior | FiveMCleaner |
| Tagline | **Seu PC, simplificado.** |
| Categoria | Automação, desempenho e configuração para Windows |
| Papel de FiveM | Integração compatível, não marca principal |
| Planos | Vemryx One Free, Vemryx One Pro e futuramente Business |
| Símbolo | `V` geométrico com referência discreta ao número `1` |
| Paleta | Preto/grafite + índigo + ciano; laranja para jogos |
| Tipografia do app | Segoe UI Variable, fallback Segoe UI |
| Tipografia de marketing | Geist Sans ou Manrope |
| Tipografia técnica | JetBrains Mono, apenas em dados técnicos |
| Personalidade | Técnica, calma, clara, segura, moderna e honesta |
| Transição | “Vemryx One — anteriormente FiveMCleaner” |
| Princípio técnico | Migração versionada, segura e reversível |
| Situação jurídica | Nome escolhido estrategicamente; liberação formal pendente |

---

# 5. Arquitetura de marca

## 5.1. Estrutura principal

```text
Vemryx                         ← marca da empresa
└── Vemryx One                ← aplicativo desktop principal
    ├── Free                  ← plano gratuito
    ├── Pro                   ← plano pago individual
    └── Business              ← plano futuro para equipes/dispositivos
```

## 5.2. Estrutura funcional

```text
Vemryx One
├── Início
├── Desempenho
├── Manutenção
├── Automações
├── Jogos e aplicativos
│   ├── FiveM
│   ├── GTA V
│   ├── ReShade
│   └── futuras integrações
├── Diagnóstico
├── Histórico
└── Configurações
```

## 5.3. Relação entre empresa e produto

- `Vemryx` acumula reputação institucional.
- `Vemryx One` acumula reputação específica do aplicativo.
- O endosso `by Vemryx` pode aparecer em páginas institucionais e tela Sobre.
- O logotipo corporativo e o ícone do produto não devem ser confundidos.
- O símbolo do produto pode derivar do `V` corporativo, mas precisa ser reconhecível como aplicação específica.

## 5.4. Regra para futuras marcas

A Vemryx deve evitar criar nomes próprios para todos os recursos.

- produto independente e comercialmente relevante: pode receber nome próprio;
- recurso interno: usa nome funcional;
- integração: usa nome de terceiro apenas de forma descritiva;
- tecnologia de bastidor: permanece sem destaque de marketing.

### Correto

- Vemryx One
- Automações
- Histórico de alterações
- Integração com FiveM
- Instalação assistida do ReShade

### Evitar

- Vemryx HyperBoost Engine Ultra
- Vemryx CleanX
- Vemryx ReShade Pro Max
- Vemryx FiveM Edition como identidade principal

## 5.5. URLs recomendadas

```text
vemryx.com/one                  Página do produto
vemryx.com/download             Download
vemryx.com/pricing              Planos
vemryx.com/integrations         Integrações
vemryx.com/fivemcleaner         Explicação da mudança
vemryx.com/security             Segurança e reversibilidade
vemryx.com/privacy              Privacidade
docs.vemryx.com                 Documentação
status.vemryx.com               Status
support.vemryx.com              Ajuda futura
app.vemryx.com                  Painel web futuro
```

---

# 6. Estratégia de naming

## 6.1. Nome selecionado

# **Vemryx One**

## 6.2. Justificativa

O nome comunica:

1. um único lugar para várias necessidades;
2. ações resolvidas com poucos cliques;
3. centralização de ferramentas;
4. principal produto da empresa;
5. capacidade de crescer sem depender de um jogo;
6. aplicabilidade internacional;
7. conexão direta com a marca Vemryx.

## 6.3. Vantagens

- não contém marca de terceiro;
- não limita o produto a limpeza;
- não limita o produto a jogos;
- não limita o produto a inteligência artificial;
- funciona com Free, Pro e Business;
- permite expansão modular sem nova troca de nome;
- é curto para ícone, site, instalador e comunicação.

## 6.4. Limitações reconhecidas

- `One` é genérico e bastante usado;
- a distintividade depende do conjunto `Vemryx One`;
- a disponibilidade jurídica não pode ser presumida;
- a busca deve considerar grafia, fonética, elementos visuais e classes relacionadas.

## 6.5. Alternativas consideradas

| Nome | Vantagem | Limitação | Decisão |
|---|---|---|---|
| Vemryx One | Amplo, memorável, centralizador | `One` é genérico | **Selecionado** |
| Vemryx Desktop | Muito claro | Pouco memorável | Reserva conservadora |
| Vemryx Pilot | Comunica orientação | Muito usado em software | Não selecionado |
| Vemryx Orbit | Sugere ecossistema | Mais abstrato | Não selecionado |
| Vemryx Core | Técnico e forte | Parece focado apenas em baixo nível | Não selecionado |
| Vemryx Flow | Bom para automação | Fraco para diagnóstico e desempenho | Pode virar conceito de recurso |
| Vemryx Cleaner | Continuidade | Mantém limitação antiga | Rejeitado |
| Vemryx Optimizer | Clareza | Genérico e limitado | Rejeitado |
| Vemryx Booster | Apelo gamer | Associado a promessas pouco confiáveis | Rejeitado |
| Vemryx Gaming | Público claro | Impede expansão geral | Rejeitado |
| Vemryx AI | Tendência atual | Amarra o produto a uma tecnologia | Rejeitado |

## 6.6. Escrita oficial

- Forma correta: `Vemryx One`
- Caixa alta em logo ou rótulo curto: `VEMRYX ONE`
- Forma técnica compacta: `VemryxOne`
- Slug: `vemryx-one` ou `one`
- Nunca traduzir `One`.
- Evitar `Vemrix One`, `Vemryx 1`, `VemryxOne` em texto comum e `Vemryx ONE` sem necessidade visual.

## 6.7. Uso do nome antigo

`FiveMCleaner` poderá permanecer apenas em:

- página de transição;
- histórico e changelog;
- migração de dados;
- aliases de busca;
- compatibilidade do atualizador;
- suporte a versões antigas;
- testes de migração;
- documentação histórica;
- allowlist técnica.

Ele não deve permanecer como título principal, nome de plano, nome de novo instalador ou identidade visual.

---

# 7. Posicionamento de mercado

## 7.1. Categoria recomendada

> **Central de automação, desempenho e configuração para Windows.**

O produto ocupa o espaço entre:

- otimizador de sistema;
- ferramenta de manutenção;
- assistente de configuração;
- instalador orientado;
- central de diagnóstico;
- gerenciador de integrações;
- plataforma de automações locais.

## 7.2. Declaração de posicionamento

> Para usuários de Windows que perdem tempo com tutoriais, configurações manuais e correções arriscadas, o Vemryx One transforma tarefas técnicas em ações guiadas, explicadas e reversíveis. Diferentemente de cleaners e boosters genéricos, o produto prioriza transparência, controle e resultados verificáveis.

## 7.3. Proposta de valor

> **Resolver tarefas técnicas sem exigir que o usuário se torne especialista.**

## 7.4. Promessa central

> **Complexidade removida sem remover o controle.**

## 7.5. Diferenciais

- ações explicadas antes da execução;
- impacto esperado e riscos apresentados;
- histórico completo;
- possibilidade de desfazer;
- backup, quarentena e journal quando aplicável;
- automações especializadas por aplicativo;
- ausência de linguagem alarmista;
- resultados mensuráveis quando houver medição válida;
- foco em segurança e previsibilidade;
- arquitetura modular.

## 7.6. Razões para acreditar

A comunicação poderá se apoiar em provas reais, conforme implementação:

- mecanismo de transações e rollback;
- testes automatizados;
- validação prévia;
- quarentena;
- histórico e logs claros;
- diagnóstico antes de alterar;
- restauração;
- documentação de cada otimização.

Somente recursos existentes devem ser usados como prova pública.

## 7.7. Território competitivo

> **Automação local confiável para tarefas de Windows, jogos e aplicativos.**

## 7.8. Anti-posicionamento

O produto não é:

- antivírus;
- substituto do Windows Update;
- ferramenta de overclock automático;
- removedor genérico de “erros” inventados;
- promessa de FPS garantido;
- cheat, mod menu ou ferramenta de evasão;
- produto oficial de FiveM, Rockstar ou Take-Two;
- ferramenta que altera o PC sem explicar.

## 7.9. Posicionamento por estágio

### Estágio A — foco ainda majoritário em FiveM

> **Vemryx One simplifica manutenção e otimização do Windows, com ferramentas especializadas para FiveM.**

### Estágio B — ReShade e outras integrações

> **Vemryx One automatiza configurações, otimizações e tarefas técnicas de jogos e aplicativos no Windows.**

### Estágio C — plataforma ampla

> **Vemryx One é a central para automatizar, configurar, diagnosticar e manter seu PC.**

A comunicação pública deve usar o estágio que corresponde ao produto real.

---

# 8. Públicos e necessidades

## 8.1. Usuário iniciante de FiveM

### Necessidades

- limpar cache sem apagar dados importantes;
- resolver erros comuns;
- melhorar estabilidade;
- entender o que pode ser feito com segurança;
- evitar tutoriais conflitantes.

### Mensagem

> Ferramentas especializadas para FiveM, com explicações e opção de desfazer.

## 8.2. Jogador de PC não técnico

### Necessidades

- instalar ReShade corretamente;
- configurar ferramentas sem editar pastas manualmente;
- aplicar perfis por jogo;
- reduzir tarefas repetitivas;
- saber se uma recomendação realmente se aplica ao PC.

### Mensagem

> Tarefas complicadas de jogos e aplicativos resolvidas por fluxos guiados.

## 8.3. Usuário geral de Windows

### Necessidades

- organizar inicialização;
- remover temporários com segurança;
- diagnosticar problemas;
- automatizar manutenção;
- visualizar alterações anteriores.

### Mensagem

> Menos configuração manual, mais controle sobre o computador.

## 8.4. Usuário avançado

### Necessidades

- saber exatamente o que será alterado;
- escolher itens individualmente;
- revisar comandos, chaves e arquivos;
- exportar diagnóstico;
- criar perfis;
- evitar abstrações que ocultem efeitos.

### Mensagem

> Automação sem caixa-preta: revise, personalize, execute e reverta.

## 8.5. Equipes e pequenos negócios — futuro

### Necessidades

- aplicar políticas consistentes;
- administrar múltiplos computadores;
- registrar auditoria;
- controlar permissões;
- distribuir perfis internos.

### Mensagem

> Padronize tarefas e configurações com rastreabilidade.

## 8.6. Jobs to Be Done

- “Quando eu não sei configurar uma ferramenta, quero que o aplicativo faça o processo com segurança para eu não precisar seguir tutoriais.”
- “Quando meu jogo apresenta problema, quero diagnosticar antes de apagar ou alterar coisas aleatoriamente.”
- “Quando aplico uma otimização, quero saber o que mudou e conseguir desfazer.”
- “Quando uso o app com frequência, quero que ele evite ações repetitivas desnecessárias.”
- “Quando uma tarefa exige administrador, quero entender por quê.”

---

# 9. Fundamentos da marca

## 9.1. Propósito

> Reduzir o atrito técnico entre pessoas e computadores.

## 9.2. Missão

> Transformar tarefas complexas do Windows em experiências claras, seguras e reversíveis.

## 9.3. Visão

> Tornar-se uma plataforma confiável para automatizar e simplificar tarefas de computadores pessoais, jogos e aplicativos.

## 9.4. Valores

### Clareza

O usuário entende o que está acontecendo.

### Controle

O usuário escolhe, confirma e pode desfazer.

### Segurança

Ações são validadas, limitadas e protegidas por recuperação.

### Honestidade

Nenhum benefício é prometido sem base técnica razoável.

### Simplicidade

A complexidade é absorvida pelo produto, não empurrada ao usuário.

### Reversibilidade

Alterações importantes deixam histórico e caminho de retorno.

### Compatibilidade

O produto considera o estado real do computador.

## 9.5. Pilares

| Pilar | Significado |
|---|---|
| Simplicidade | Tarefas longas viram fluxos claros |
| Controle | O usuário escolhe e revisa |
| Segurança | Validação, backup e privilégio mínimo |
| Reversibilidade | Alterações podem ser rastreadas e desfeitas |
| Automação | Repetição e configuração manual são reduzidas |
| Honestidade | Sem medo, exagero ou métricas inventadas |

## 9.6. Personalidade

A marca deve parecer:

- competente;
- calma;
- moderna;
- objetiva;
- previsível;
- técnica sem ser hostil;
- acessível sem ser infantil;
- premium sem ser extravagante.

A marca não deve parecer:

- agressivamente gamer;
- alarmista;
- misteriosa;
- cheia de jargões vazios;
- invasiva;
- paternalista;
- “milagrosa”.

## 9.7. Arquétipo funcional

Combinação recomendada:

- **Guia:** conduz tarefas complexas;
- **Especialista:** demonstra competência técnica;
- **Guardião:** protege contra alterações inadequadas.

Evitar o arquétipo de “mago” que promete resultados inexplicáveis.

---

# 10. Sistema de mensagens

## 10.1. Tagline oficial

> **Seu PC, simplificado.**

### Traduções propostas

| Idioma | Tagline |
|---|---|
| Português do Brasil | Seu PC, simplificado. |
| Inglês | Your PC, simplified. |
| Espanhol | Tu PC, simplificado. |

As traduções devem passar por revisão linguística.

## 10.2. Descritor oficial

> **Automação, desempenho e configuração para Windows.**

## 10.3. Frase curta

> Otimize, configure, diagnostique e automatize tarefas do seu PC com segurança.

## 10.4. Descrição curta

> Vemryx One transforma tarefas técnicas do Windows em ações guiadas, transparentes e reversíveis.

## 10.5. Descrição média

> Vemryx One é uma central de automação, desempenho e configuração para Windows. O aplicativo ajuda a manter o sistema, diagnosticar problemas, aplicar perfis e configurar jogos e ferramentas por meio de fluxos claros, com histórico e possibilidade de desfazer alterações.

## 10.6. Descrição longa

> Vemryx One foi criado para reduzir o tempo e a incerteza envolvidos em tarefas técnicas do Windows. Em vez de obrigar o usuário a seguir tutoriais, editar arquivos manualmente ou aplicar otimizações sem entender os efeitos, o produto reúne manutenção, diagnóstico, automação e integrações em uma experiência guiada. Cada ação relevante informa o que será alterado, o impacto esperado, as permissões necessárias e as opções de recuperação disponíveis.

## 10.7. Elevator pitch

> O Vemryx One é uma central para Windows que resolve tarefas técnicas com poucos cliques. Ele reúne otimização, manutenção, diagnóstico e automações para jogos e aplicativos, sempre mostrando o que será feito e permitindo reverter alterações quando possível.

## 10.8. Mensagens secundárias

- Menos tutoriais. Mais controle.
- Tarefas complicadas, resolvidas com clareza.
- Automatize sem perder o controle.
- Veja o que muda. Aplique com segurança. Desfaça quando precisar.
- Seu computador não precisa ser uma caixa-preta.
- Configuração técnica sem sofrimento desnecessário.

## 10.9. Hierarquia de mensagem

1. **Benefício:** simplificar tarefas do PC.
2. **Forma:** automação guiada.
3. **Confiança:** transparência e reversibilidade.
4. **Escopo:** manutenção, desempenho, diagnóstico e integrações.
5. **Prova:** histórico, rollback, validação e testes.

## 10.10. Mensagem de transição

> **FiveMCleaner agora é Vemryx One.** O projeto nasceu para simplificar manutenção e otimização do FiveM. Agora está evoluindo para uma central mais ampla de automação, desempenho e configuração para Windows. Seus dados, recursos e histórico continuam no mesmo produto.

## 10.11. História da marca

> O produto começou resolvendo um problema específico: o tempo perdido com limpeza, configuração e otimização para FiveM. À medida que o projeto amadureceu, ficou claro que a mesma abordagem poderia simplificar muitas outras tarefas do Windows. A mudança para Vemryx One representa essa evolução sem abandonar a especialização construída no FiveM.

## 10.12. Matriz de claims

| Claim | Uso | Condição |
|---|---|---|
| “Simplifica tarefas técnicas” | Permitido | Deve existir fluxo real que simplifique |
| “Pode desfazer alterações” | Permitido com qualificação | Somente onde rollback existe |
| “Melhora desempenho” | Permitido com contexto | Informar condição e incerteza |
| “Libera espaço” | Permitido | Mostrar estimativa ou resultado real |
| “Corrige problemas comuns” | Permitido | Especificar quais problemas |
| “Aumenta FPS” | Somente com evidência | Nunca garantir valor universal |
| “Elimina lag” | Evitar | Promessa ampla e enganosa |
| “100% seguro” | Proibido | Nenhum software é livre de risco |
| “Corrige todos os erros” | Proibido | Impossível sustentar |
| “Seu PC está em perigo” | Proibido | Linguagem alarmista |
| “Encontramos milhares de problemas” | Proibido | Métrica manipulativa |

## 10.13. Declaração de independência

> Vemryx One é um produto independente. Marcas de terceiros pertencem aos seus respectivos titulares. A menção a jogos, aplicativos ou serviços indica compatibilidade e não implica afiliação, patrocínio ou endosso.

---

# 11. Voz, tom e terminologia

## 11.1. Princípios de voz

### Clara

Explica o essencial sem esconder impacto técnico.

### Direta

Evita parágrafos longos em ações simples.

### Calma

Não cria ansiedade para induzir cliques.

### Técnica

Usa termos corretos e oferece detalhes sob demanda.

### Honesta

Distingue fato, estimativa, recomendação e incerteza.

### Respeitosa

Não trata o usuário como incapaz.

## 11.2. Tom por situação

| Situação | Tom |
|---|---|
| Tela inicial | Objetivo e acolhedor |
| Diagnóstico | Analítico e neutro |
| Ação destrutiva | Preciso e cauteloso |
| Sucesso | Informativo, sem exagero |
| Erro recuperável | Calmo e orientado a solução |
| Erro grave | Transparente e específico |
| Cobrança | Claro e sem manipulação |
| Segurança | Sério e direto |
| Marketing | Confiante, sem absolutos |

## 11.3. Vocabulário preferido

- ação;
- recomendação;
- impacto esperado;
- alteração;
- diagnóstico;
- compatibilidade;
- histórico;
- restaurar;
- desfazer;
- revisar;
- aplicar;
- verificação;
- arquivos temporários;
- integração;
- automação;
- perfil.

## 11.4. Vocabulário a evitar

- turbinar;
- milagroso;
- supervelocidade;
- eliminar todos os erros;
- corrigir tudo;
- risco zero;
- FPS garantido;
- computador doente;
- lixo perigoso, quando for apenas cache;
- problema crítico sem criticidade real;
- boost extremo;
- hacker mode.

## 11.5. Taxonomia oficial

| Termo | Definição |
|---|---|
| Ação | Operação individual executável |
| Automação | Fluxo com múltiplas ações coordenadas |
| Perfil | Conjunto reutilizável de configurações |
| Integração | Suporte específico a jogo ou aplicativo |
| Diagnóstico | Inspeção sem alteração inicial |
| Recomendação | Ação sugerida com base no estado detectado |
| Histórico | Registro das operações e resultados |
| Restauração | Retorno a estado salvo ou anterior |
| Quarentena | Armazenamento protegido antes de remoção definitiva |
| Impacto | Efeito esperado sobre sistema, dados ou experiência |

## 11.6. Exemplos de microcopy

### Recomendação

**Evitar:**

> Seu PC tem 17 problemas! Corrija agora.

**Usar:**

> Encontramos 4 recomendações aplicáveis ao seu computador. Revise o impacto de cada uma antes de continuar.

### Limpeza

**Evitar:**

> Excluir lixo.

**Usar:**

> Remover arquivos temporários recriáveis.

### Administrador

**Evitar:**

> Precisamos de administrador.

**Usar:**

> Esta ação altera uma configuração protegida do Windows e precisa de permissão de administrador. Nenhuma outra alteração será executada nessa etapa.

### Sucesso

**Evitar:**

> Seu PC foi turbinado!

**Usar:**

> A ação foi concluída. 1,8 GB de arquivos temporários foram removidos e o resultado foi registrado no Histórico.

### Falha

**Evitar:**

> Algo deu errado.

**Usar:**

> Não foi possível atualizar a configuração porque o arquivo está em uso. Feche o FiveM e tente novamente. Nenhuma alteração parcial foi mantida.

---

# 12. Identidade visual

## 12.1. Auditoria do ícone atual

Arquivo analisado: `Ícone moderno com monograma 5M.png`.

### Propriedades técnicas verificadas

- dimensões: **1254 × 1254 pixels**;
- modo de cor: **RGB**;
- canal alpha: **inexistente**;
- transparência: **não é real**;
- o padrão quadriculado está incorporado à imagem.

### Problemas estratégicos

- o monograma `5M` depende diretamente do nome antigo;
- o número `5` continua remetendo a FiveM;
- a letra `M` não se relaciona com Vemryx One;
- a estética metálica e tridimensional remete a utilitários gamer antigos;
- brilho, bevel, partículas e linhas finas perdem qualidade em tamanhos reduzidos;
- laranja/prata limita a identidade à categoria gamer;
- a imagem não serve como fonte vetorial;
- a falsa transparência inviabiliza uso profissional em fundos variados.

### Decisão

O ícone atual não será adaptado como identidade principal. Poderá ser preservado apenas em histórico ou material de “antes e depois”.

## 12.2. Conceito do novo símbolo

### **V + 1**

O símbolo deve combinar:

- o `V` da Vemryx;
- referência discreta ao número `1`;
- sensação de convergência, fluxo ou simplificação;
- geometria simples e proprietária.

O `1` pode surgir por:

- espaço negativo;
- uma das hastes do `V`;
- corte interno;
- alinhamento vertical secundário;
- módulo geométrico que não prejudique a leitura.

O símbolo não deve parecer `VI`, `V1` genérico, marca automotiva, seta comum ou logotipo de criptomoeda.

## 12.3. Significados desejados

- vários caminhos convergindo em uma solução;
- redução de complexidade;
- avanço controlado;
- centralização;
- transformação de processo manual em ação única;
- conexão entre Vemryx e seu principal produto.

## 12.4. Restrições formais

- no máximo três formas principais;
- contorno legível em 16 px;
- versão monocromática funcional;
- sem texto dentro do ícone;
- sem vassoura;
- sem foguete;
- sem velocímetro;
- sem raio genérico;
- sem escudo ou engrenagem genéricos como símbolo principal;
- sem partículas decorativas;
- sem brilho metálico obrigatório;
- sem transparência falsa;
- sem dependência de gradiente para reconhecimento.

## 12.5. Estrutura do ícone

Direção recomendada:

- quadrado arredondado;
- fundo escuro fosco;
- símbolo central V/1;
- margem de segurança mínima de 10%;
- contraste alto;
- correção óptica para tamanhos pequenos;
- variante simplificada para bandeja e 16 px;
- variante de alto contraste.

### Versões obrigatórias

1. ícone principal em fundo escuro;
2. ícone claro;
3. monocromático preto;
4. monocromático branco;
5. símbolo sem fundo;
6. bandeja simplificada;
7. favicon;
8. `.ico` multirresolução;
9. SVG mestre;
10. PNG 1024 px com alpha real.

## 12.6. Relação com logo corporativo

- O símbolo corporativo da Vemryx permanece assinatura da empresa.
- O ícone V/1 deve ser reconhecido como derivação de produto.
- O wordmark pode usar `Vemryx` em destaque e `One` como complemento.
- O ícone do produto não substitui o logo institucional em documentos corporativos.

## 12.7. Lockups recomendados

```text
[ símbolo V/1 ] Vemryx One
[ símbolo V/1 ] VEMRYX
                ONE
[ símbolo V/1 ]
Vemryx One
by Vemryx
```

## 12.8. Área de proteção

Definir `x` como a espessura principal da haste.

- área mínima ao redor: `1x`;
- ao lado de textos/logos: `1,5x`;
- em co-branding: `2x`;
- nenhum elemento invade essa área.

## 12.9. Tamanhos mínimos sugeridos

| Ativo | Tamanho mínimo |
|---|---:|
| Símbolo digital | 16 px |
| Símbolo detalhado | 32 px |
| Wordmark horizontal | 96 px de largura |
| Assinatura com tagline | 180 px de largura |
| Favicon | versão dedicada 16/32 px |
| Bandeja | versão dedicada 16/20/24 px |

Os valores finais devem ser validados com o desenho definitivo.

## 12.10. Usos proibidos

- distorcer proporções;
- inclinar ou rotacionar;
- adicionar sombra pesada;
- aplicar bevel metálico;
- usar cores fora da paleta;
- colocar em fundo sem contraste;
- contornar arbitrariamente;
- adicionar `FiveM` dentro do símbolo;
- usar versão detalhada em 16 px;
- recriar por IA sem vetor mestre;
- usar PNG como fonte de edição.

## 12.11. Paleta principal — tema escuro

| Token | Nome | Hex | Uso |
|---|---|---:|---|
| `bg.canvas` | Vemryx Ink | `#0B0D12` | Fundo principal |
| `bg.surface` | Graphite | `#131722` | Cartões e painéis |
| `bg.elevated` | Slate | `#1A2030` | Superfícies elevadas |
| `border.subtle` | Steel Line | `#2A3243` | Bordas discretas |
| `text.primary` | Cloud White | `#F7F9FC` | Texto principal |
| `text.secondary` | Mist Gray | `#97A0B3` | Texto secundário |
| `brand.accent` | Vemryx Indigo | `#5B7CFF` | Links, foco, seleção e ícones |
| `brand.solid` | Indigo Solid | `#4B64F2` | Botão preenchido com texto branco |
| `brand.cyan` | Electric Cyan | `#27C8FF` | Destaque secundário |
| `status.success` | Emerald | `#32D583` | Sucesso |
| `status.warning` | Amber | `#F5B942` | Aviso |
| `status.error` | Signal Red | `#F04438` | Erro |
| `status.dangerSolid` | Deep Red | `#D92D20` | Botão destrutivo |
| `category.gaming` | Legacy Orange | `#FF8A1F` | Área de jogos |

### Regras de contraste

- `#F7F9FC` sobre `#0B0D12` é o par principal de texto.
- `#5B7CFF` funciona como acento em superfícies escuras.
- `#4B64F2` é separado para preenchimentos com texto branco.
- ciano, verde, amarelo e laranja devem usar texto escuro quando forem fundos preenchidos.
- nenhum estado depende apenas de cor; combinar ícone, texto ou forma.

## 12.12. Tema claro recomendado

| Token | Hex | Uso |
|---|---:|---|
| `light.bg.canvas` | `#F5F7FB` | Fundo |
| `light.bg.surface` | `#FFFFFF` | Cartões |
| `light.bg.elevated` | `#EEF2F8` | Áreas elevadas |
| `light.border.subtle` | `#D8DEEA` | Bordas |
| `light.text.primary` | `#11141B` | Texto principal |
| `light.text.secondary` | `#5B6473` | Texto secundário |
| `light.brand.solid` | `#4057D6` | Botões e destaque |

O tema claro pode ser posterior, mas os tokens devem nascer preparados.

## 12.13. Gradiente

Uso apenas em marketing, hero, ilustrações ou detalhes grandes:

```css
linear-gradient(135deg, #5B7CFF 0%, #27C8FF 100%)
```

Não usar em texto pequeno, em todos os botões ou como único indicador de estado.

## 12.14. Tipografia

### Aplicativo

```text
Segoe UI Variable
Fallback: Segoe UI, Arial, sans-serif
```

### Website e marketing

Preferência:

```text
Geist Sans
```

Alternativa:

```text
Manrope
```

### Dados técnicos

```text
JetBrains Mono
```

Usar somente em caminhos, hashes, IDs, logs, versões, comandos e valores técnicos.

## 12.15. Escala tipográfica

| Estilo | Tamanho/altura | Peso |
|---|---|---:|
| Display | 40/48 | 700 |
| H1 | 32/40 | 700 |
| H2 | 24/32 | 600 |
| H3 | 20/28 | 600 |
| Título de cartão | 16/24 | 600 |
| Corpo | 14/22 ou 15/22 | 400 |
| Rótulo | 13/18 | 500 |
| Legenda | 12/16 | 400 |
| Overline | 11/16 | 600 |

Evitar caixa alta em textos longos.

## 12.16. Iconografia

- estilo linear ou filled consistente;
- grade de 20 e 24 px;
- espessura uniforme;
- cantos levemente arredondados;
- preferir Fluent ou conjunto próprio compatível;
- não misturar 3D, emoji e outline;
- logos de terceiros somente quando permitido;
- integrações podem usar ícone neutro e nome textual.

## 12.17. Ilustrações e imagens

### Usar

- diagramas de fluxo;
- caminhos convergentes;
- telas reais;
- visualizações técnicas limpas;
- formas geométricas discretas;
- capturas sem dados pessoais.

### Evitar

- gamer genérico com headset;
- raios e velocímetros;
- computadores explodindo em luz;
- números de desempenho inventados;
- telas falsas;
- excesso de neon.

## 12.18. Movimento

- transições funcionais de 120–220 ms;
- easing suave;
- não esconder estado crítico com animação;
- oferecer redução de movimento;
- evitar loops decorativos permanentes;
- progresso deve representar trabalho real.

## 12.19. Pacote de ativos

```text
assets/brand/
├── source/
│   ├── vemryx-one-symbol.svg
│   ├── vemryx-one-wordmark.svg
│   └── vemryx-one-lockup.svg
├── export/
│   ├── logo/
│   ├── app-icon/
│   ├── tray/
│   ├── favicon/
│   ├── installer/
│   ├── social/
│   └── store/
├── tokens/
│   ├── colors.xaml
│   ├── colors.css
│   └── brand.json
└── guidelines/
    └── BRAND_GUIDELINES.md
```

### Exportações mínimas

- SVG mestre;
- PNG 1024, 512, 256, 128, 64 e 32 px;
- ICO com 16, 20, 24, 32, 48, 64, 128 e 256 px;
- favicon 16, 32 e 48 px;
- bandeja 16, 20 e 24 px;
- Open Graph 1200 × 630;
- avatar 512 × 512;
- banners apropriados para cada canal.

---

# 13. Sistema de interface e UX

## 13.1. Princípio geral

A interface deve parecer uma plataforma confiável do Windows, não um “booster gamer” agressivo.

## 13.2. Direção visual

- fundos escuros foscos;
- superfícies bem separadas;
- cantos entre 8 e 16 px;
- bordas discretas;
- sombras leves;
- hierarquia tipográfica clara;
- gradiente apenas em destaques;
- densidade moderada;
- espaços em múltiplos de 4 e 8 px;
- ícones consistentes;
- animações curtas;
- estados explícitos.

## 13.3. Princípios de UX

### Mostrar antes de executar

O usuário revisa mudanças relevantes.

### Diagnosticar antes de recomendar

Não apresentar ação genérica se ela não se aplica.

### Explicar sem sobrecarregar

Resumo simples por padrão e detalhes técnicos sob demanda.

### Dar saída segura

Cancelar, desfazer, restaurar ou recuperar quando aplicável.

### Evitar repetição desnecessária

Limpezas consideram recência, necessidade e impacto.

### Não esconder privilégios

Informar quando e por que haverá elevação.

## 13.4. Navegação

```text
Início
Desempenho
Manutenção
Automações
Jogos e aplicativos
Diagnóstico
Histórico
Configurações
```

A navegação deve refletir objetivos do usuário, não a estrutura interna do código.

## 13.5. Tela inicial

Priorizar:

- status real do computador;
- recomendações;
- última manutenção;
- ações em andamento;
- integrações detectadas;
- espaço recuperável estimado;
- alterações recentes;
- alertas reais;
- atalhos contextuais.

### Evitar

- “Saúde do PC: 42%” sem metodologia;
- contagem inflada de problemas;
- botão enorme de “OTIMIZAR AGORA” sem revisão;
- pressão comercial dentro de alerta técnico.

### Exemplo

```text
4 recomendações disponíveis
2 exigem reinicialização
1,8 GB de arquivos temporários podem ser removidos
Última manutenção: há 12 dias
```

## 13.6. Cartão de recomendação

Mostrar:

- nome;
- descrição;
- motivo;
- impacto esperado;
- risco;
- necessidade de administrador;
- necessidade de reinicialização;
- reversibilidade;
- origem da recomendação;
- detalhes;
- seleção explícita.

## 13.7. Detalhes da ação

```text
O que esta ação faz
Por que foi recomendada
O que será alterado
Arquivos/chaves/serviços envolvidos
Impacto esperado
Possíveis efeitos colaterais
Permissões necessárias
Como o Vemryx One protege a alteração
Como desfazer
```

## 13.8. Níveis de risco

| Nível | Uso |
|---|---|
| Baixo | Temporários recriáveis e mudanças triviais |
| Moderado | Configurações com efeito perceptível ou reinicialização |
| Elevado | Alterações avançadas com backup e confirmação extra |
| Bloqueado | Ação incompatível ou insegura no estado atual |

O risco deve ser baseado em impacto e reversibilidade.

## 13.9. Componentes

- botão primário;
- botão secundário;
- botão destrutivo;
- cartão de recomendação;
- painel de diagnóstico;
- resumo de impacto;
- badge de risco;
- badge de administrador;
- indicador de reversibilidade;
- progresso real;
- timeline de histórico;
- diálogo de confirmação;
- comparação antes/depois;
- log expansível;
- toast de resultado;
- banner de transição.

## 13.10. Estados obrigatórios

- padrão;
- hover;
- foco;
- pressionado;
- desabilitado com motivo;
- carregando;
- sucesso;
- aviso;
- erro recuperável;
- erro não recuperável;
- cancelado;
- revertido;
- parcial com recuperação;
- incompatível.

## 13.11. Acessibilidade

Meta recomendada:

- WCAG 2.2 nível AA como referência;
- UI Automation do Windows;
- navegação completa por teclado;
- ordem de tabulação lógica;
- foco visível;
- nomes acessíveis em controles;
- suporte a leitor de tela;
- alto contraste;
- escala de texto e DPI;
- não depender só de cor;
- áreas clicáveis confortáveis;
- redução de movimento;
- mensagens compreensíveis para dificuldades cognitivas.

## 13.12. Localização

Idiomas previstos:

- português do Brasil;
- inglês;
- espanhol.

Regras:

- nenhuma string visível hardcoded em XAML ou código;
- `Vemryx One` não é traduzido;
- nomes de terceiros não são traduzidos;
- chaves seguem prefixos por contexto;
- layouts aceitam expansão de texto;
- screenshots de loja devem ser localizadas.

### Prefixos sugeridos

```text
Brand.*
Navigation.*
Home.*
Performance.*
Maintenance.*
Automations.*
Integrations.*
Diagnostics.*
History.*
Settings.*
Migration.*
Rebrand.*
Legal.*
```

---

# 14. Arquitetura do produto

## 14.1. Áreas principais

### Início

Resumo, recomendações e ações recentes.

### Desempenho

Perfis, inicialização, processos e ajustes aplicáveis.

### Manutenção

Temporários, caches, armazenamento e manutenção recorrente.

### Automações

Fluxos multietapas para tarefas complexas.

### Jogos e aplicativos

Integrações específicas com detecção, configuração e diagnóstico.

### Diagnóstico

Inspeção do sistema e correções orientadas.

### Histórico

Alterações, backups, quarentena, resultados e restaurações.

### Configurações

Conta, idioma, tema, telemetria, atualização, segurança e preferências.

## 14.2. Estrutura de integrações

```text
Jogos e aplicativos
├── FiveM
│   ├── Diagnosticar instalação
│   ├── Limpar caches específicos
│   ├── Aplicar perfil de desempenho
│   ├── Corrigir problemas comuns
│   └── Restaurar alterações
├── GTA V
├── ReShade
│   ├── Verificar compatibilidade
│   ├── Instalar
│   ├── Atualizar
│   ├── Configurar
│   ├── Gerenciar presets
│   └── Restaurar
└── Futuras integrações
```

## 14.3. Modelo conceitual

```text
Diagnóstico → Recomendação → Revisão → Execução → Verificação → Histórico → Restauração
```

## 14.4. Regra de modularidade

Cada integração deve:

- detectar aplicabilidade;
- declarar requisitos;
- listar ações suportadas;
- informar riscos;
- fornecer verificação;
- fornecer rollback quando possível;
- não depender de assets da marca principal;
- poder ser desabilitada sem quebrar o núcleo.

## 14.5. Nomenclatura técnica sugerida

```text
Vemryx.One.App
Vemryx.One.Core
Vemryx.One.Infrastructure
Vemryx.One.Broker
Vemryx.One.Updater
Vemryx.One.Modules.Windows
Vemryx.One.Modules.FiveM
Vemryx.One.Modules.GtaV
Vemryx.One.Modules.ReShade
Vemryx.One.Tests
```

Os nomes finais devem respeitar a solução real e podem ser migrados gradualmente.

---

# 15. Alinhamento entre rebranding e expansão

## 15.1. O rebranding pode ocorrer antes da expansão completa

Não é necessário esperar suporte a muitos jogos. A comunicação deve ser honesta:

> Vemryx One está expandindo o que começou no FiveMCleaner. As ferramentas especializadas em FiveM continuam disponíveis, enquanto novas automações e integrações serão adicionadas ao produto.

## 15.2. Não prometer plataforma universal antes da hora

Enquanto a maioria das funções ainda for centrada em FiveM, evitar:

- “Tudo o que seu PC precisa”;
- “O gerenciador universal de todos os jogos”;
- “Automatiza qualquer tarefa do Windows”.

Usar:

- “uma central em expansão”;
- “começando por FiveM e manutenção do Windows”;
- “novas integrações serão adicionadas gradualmente”.

## 15.3. Exemplo de automação: ReShade

Fluxo recomendado:

1. detectar jogo e executável;
2. identificar API gráfica compatível;
3. explicar o que será instalado;
4. baixar de fonte confiável;
5. verificar assinatura ou hash quando disponível;
6. criar backup;
7. instalar componentes;
8. permitir seleção de pacotes e presets;
9. validar arquivos;
10. mostrar como abrir o overlay;
11. oferecer desinstalação limpa;
12. registrar tudo no Histórico.

O fluxo não deve apenas copiar arquivos silenciosamente.

## 15.4. Proteção contra uso excessivo

O produto deve considerar:

- data da última execução;
- quantidade acumulada;
- estado atual do cache;
- custo de reconstrução;
- uso recente do jogo;
- intervalo mínimo recomendado;
- opção manual avançada com explicação.

Mensagem exemplo:

> Este cache foi limpo recentemente e ainda não há acúmulo relevante. Repetir a limpeza agora provavelmente não trará benefício e pode aumentar o tempo de carregamento na próxima abertura.

---

# 16. Rebranding técnico

## 16.1. Princípios

- não usar substituição global sem classificação;
- separar identidade pública de identidade persistida;
- preservar IDs técnicos quando a mudança quebraria atualização ou dados;
- criar aliases para o legado;
- tornar migrações idempotentes;
- registrar cada etapa;
- testar instalação nova, atualização e rollback;
- permitir convivência temporária de nomes internos quando necessário.

## 16.2. Classificação de identificadores

### Devem mudar publicamente

- nome exibido;
- ícone;
- textos;
- título de janela;
- atalhos;
- nome do instalador;
- nome em Aplicativos instalados;
- website;
- documentação;
- Discord;
- release notes;
- remetente de e-mail;
- nomes de planos.

### Podem mudar com migração

- executável;
- diretórios;
- registro;
- namespaces;
- AppUserModelID;
- serviços;
- tarefas agendadas;
- mutex;
- logs;
- identificadores de telemetria;
- nomes em consoles externos.

### Não devem mudar sem motivo

- IDs de usuário;
- IDs de licença e entitlement;
- IDs de produto no backend;
- chaves primárias;
- UpgradeCode/AppId necessário ao update;
- segredos e chaves criptográficas;
- endpoints estáveis;
- eventos usados em séries históricas;
- IDs de pagamentos;
- project IDs internos cuja troca cria risco.

## 16.3. Mapa recomendado

| Contexto | Valor | Observação |
|---|---|---|
| CompanyName | `Vemryx` | Marca da empresa |
| ProductName | `Vemryx One` | Nome público |
| Executável | `VemryxOne.exe` | Avaliar updater |
| Instalador | `VemryxOneSetup.exe` | Nome público |
| Launcher | `VemryxOne.Launcher.exe` | Se existir |
| Updater | `VemryxOne.Updater.exe` | Se existir |
| Broker/serviço | `VemryxOneBroker` | Nome interno |
| Start Menu | `Vemryx One` | Pasta/atalho |
| AppUserModelID | `Vemryx.One` | Validar packaging |
| Mutex | `Global\Vemryx.One` | Manter alias legado na transição |
| Registry | `HKCU\Software\Vemryx\One` | Migrar legado |
| LocalAppData | `%LocalAppData%\Vemryx\One` | Dados por usuário |
| ProgramData | `%ProgramData%\Vemryx\One` | Dados compartilhados |
| User-Agent | `VemryxOne/<versão>` | Backend aceita antigo e novo |
| Protocolo futuro | `vemryx-one://` | Só se necessário |
| Telemetria | `vemryx-one` | Preservar continuidade |
| Log prefix | `[Vemryx One]` | Sem segredos |

## 16.4. Caminhos legados esperados

Confirmar no repositório e instalador:

```text
%LocalAppData%\FiveMCleaner
%LocalAppData%\FiveM Cleaner
%AppData%\FiveMCleaner
%ProgramData%\FiveMCleaner
%ProgramFiles%\FiveMCleaner
%ProgramFiles(x86)%\FiveMCleaner
HKCU\Software\FiveMCleaner
HKLM\Software\FiveMCleaner
```

A lista definitiva deve vir do código e dos pacotes publicados.

## 16.5. Estratégia de namespaces

### Etapa 1 — identidade pública

- strings;
- assets;
- metadados;
- instalador;
- site;
- comunicação.

### Etapa 2 — aliases e compatibilidade

- caminhos novos;
- leitura de configurações antigas;
- migração;
- aliases de API;
- updater ponte.

### Etapa 3 — namespaces internos

- projetos;
- assemblies;
- namespaces;
- testes;
- scripts.

### Etapa 4 — remoção controlada

- remover ocorrências não permitidas;
- manter apenas allowlist histórica e de migração;
- garantir que versões antigas ainda atualizem.

## 16.6. Busca de ocorrências

```bash
rg -n -i "FiveMCleaner|FiveM Cleaner|5M Cleaner|5MCleaner|5M" .
```

Classificar cada ocorrência em:

- substituir;
- preservar por compatibilidade;
- preservar por referência legítima a FiveM;
- remover;
- migrar;
- documentar em allowlist.

Não substituir automaticamente `FiveM` dentro do módulo de integração.

---

# 17. Estratégia de migração

## 17.1. Objetivo

O usuário deve perceber a mudança como atualização do mesmo produto, não como instalação independente.

## 17.2. Fluxo recomendado

```text
Detectar legado
→ Validar origem e permissões
→ Criar snapshot
→ Copiar para staging
→ Migrar esquema
→ Verificar integridade
→ Ativar atomicamente
→ Marcar conclusão
→ Manter fallback legado
→ Limpar somente após estabilidade confirmada
```

## 17.3. Requisitos

- idempotente;
- atômica quando possível;
- retomável;
- verificável;
- com logs;
- com backup;
- sem apagar origem imediatamente;
- segura contra diretórios manipulados;
- compatível com múltiplos usuários;
- testada com versões reais.

## 17.4. Dados a preservar

- configurações;
- idioma;
- tema;
- preferências;
- consentimento de telemetria;
- conta e sessão, quando seguro;
- licenças e entitlements;
- histórico;
- backups;
- quarentena;
- journal de transações;
- diagnósticos;
- perfis personalizados;
- exclusões;
- canal de atualização;
- onboarding;
- integrações configuradas.

## 17.5. Dados recriáveis

- cache de interface;
- thumbnails;
- downloads temporários;
- arquivos intermediários;
- índice reconstruível;
- logs além da retenção;
- artefatos de atualização concluída.

A decisão deve ser documentada por tipo de dado.

## 17.6. Criptografia e sessão

- verificar uso de DPAPI;
- verificar entropia ligada a nome, caminho ou assembly;
- não registrar token descriptografado;
- migrar segredos no mesmo contexto de usuário;
- solicitar novo login apenas quando inevitável;
- explicar ao usuário se a sessão expirar;
- não mudar audience, client ID ou redirect URI sem compatibilidade.

## 17.7. Pseudocódigo

```text
acquire migration mutex

if migration marker is complete:
    use new location
    exit

locate trusted legacy installations
validate directory ownership and reparse points
create protected snapshot
copy legacy data to staging
migrate configuration schema
verify expected files and hashes
atomically promote staging to target
write migration marker with source version and timestamp
start app using new location
retain legacy data until release gate permits cleanup
```

## 17.8. Falha e recuperação

Se qualquer etapa falhar:

- não ativar estado parcial;
- preservar legado;
- registrar erro sanitizado;
- permitir nova tentativa;
- oferecer exportação de diagnóstico;
- evitar loop infinito;
- nunca apagar a única cópia válida.

## 17.9. Limpeza do legado

Só remover quando:

- migração estiver concluída;
- app tiver iniciado com sucesso;
- integridade tiver sido verificada;
- versão estável confirmar compatibilidade;
- downgrade não depender dos dados;
- suporte não exigir os arquivos.

Preferência: mover para backup legado e excluir por política posterior.

## 17.10. Rollback

Permitir:

- reinstalar versão anterior;
- restaurar backup;
- manter licenças;
- manter contas;
- impedir downgrade destrutivo de esquema;
- identificar incompatibilidade;
- conservar logs suficientes.

---

# 18. Instalador, atualizador e distribuição

## 18.1. Release ponte

Antes da primeira versão pública nova, publicar uma versão ponte sob a linha antiga que:

- reconheça o novo manifesto;
- aceite o novo pacote;
- prepare aliases de caminhos;
- inclua lógica de migração;
- permita ao updater antigo baixar a versão nova;
- informe a futura mudança.

Isso reduz o risco de versões antigas ficarem presas.

## 18.2. Identidade do instalador

### Inno Setup

- preservar `AppId` quando necessário ao update in-place;
- mudar `AppName` e textos públicos;
- testar desinstalador antigo e novo;
- atualizar atalhos sem duplicação.

### MSI/WiX

- preservar `UpgradeCode` para a mesma linha;
- usar `ProductCode` conforme regras da versão;
- configurar major upgrade;
- testar detecção e remoção da versão anterior.

### MSIX

- mudança de Package Identity pode criar um app novo;
- avaliar pacote ponte, migração explícita ou preservação técnica;
- Publisher deve corresponder ao certificado/Store.

A tecnologia real determina a implementação.

## 18.3. Aplicativos instalados

Resultado esperado:

```text
Nome: Vemryx One
Publisher: titular legal real
Versão: versão atual
Ícone: novo símbolo
Suporte: vemryx.com/support
```

Não declarar uma `Ltda.` inexistente.

## 18.4. Atalhos

- atualizar/remover `FiveMCleaner` no Menu Iniciar;
- atualizar atalho da área de trabalho;
- não criar duplicata;
- preservar preferência do usuário;
- atualizar cache de ícone;
- validar pinagem antiga na barra de tarefas.

## 18.5. Atualizador

- aceitar feeds antigo e novo na transição;
- validar assinatura e hash;
- mostrar mudança de nome;
- manter canal estável/beta;
- registrar resultado;
- impedir downgrade acidental;
- ter fallback de manifesto;
- não depender apenas do nome do executável.

## 18.6. Artefatos

```text
VemryxOneSetup-x64-<versão>.exe
VemryxOne-Portable-x64-<versão>.zip
VemryxOne-<versão>.msix
VemryxOne-Checksums-<versão>.txt
VemryxOne-SBOM-<versão>.json
```

Publicar apenas formatos suportados.

## 18.7. Assinatura digital

- assinar arquivos públicos quando houver certificado;
- nome exibido deve corresponder ao titular;
- verificar assinatura no pipeline;
- publicar checksum;
- proteger segredos de assinatura;
- atualizar descrição de arquivo sem falsificar entidade jurídica.

---

# 19. Backend, autenticação, pagamentos e telemetria

## 19.1. Princípio

Não recriar projetos de backend só para trocar nome visual. IDs técnicos estáveis podem continuar internos.

## 19.2. Autenticação

Atualizar:

- nome exibido;
- remetente dos e-mails;
- templates de cadastro;
- confirmação;
- recuperação de senha;
- alteração de senha;
- novo acesso;
- 2FA;
- OAuth consent screen;
- domínios autorizados;
- links de retorno;
- políticas e termos.

Preservar:

- IDs de usuário;
- contas;
- refresh tokens quando válidos;
- provedores vinculados;
- histórico de segurança;
- preferências.

## 19.3. Firebase — caso permaneça

- não recriar projeto apenas pelo nome;
- atualizar display name;
- revisar templates de autenticação;
- revisar domínios autorizados;
- revisar remetente e identidade;
- manter `projectId` se a troca criar risco;
- testar recuperação e confirmação;
- verificar deep links antigos;
- não expor chaves administrativas no cliente.

## 19.4. Pagamentos

Atualizar:

- nome do produto;
- planos;
- checkout;
- recibos;
- e-mails;
- área de conta;
- descritor comercial permitido;
- cancelamento;
- termos;
- materiais de preço.

Preservar:

- customer IDs;
- subscription IDs;
- payment IDs;
- entitlements;
- renovação;
- histórico financeiro;
- vínculos conta/plano.

## 19.5. Planos

### Comunicação pública

- Vemryx One Free
- Vemryx One Pro
- Vemryx One Business — futuro

### Dentro do app

```text
Produto: Vemryx One
Plano atual: Gratuito
```

ou

```text
Produto: Vemryx One
Plano atual: Pro
```

## 19.6. Telemetria

Preservar séries históricas:

- manter eventos úteis;
- adicionar `brand_generation = 2`;
- adicionar `product_name = vemryx-one`;
- manter alias antigo;
- migrar dashboards;
- registrar eventos de migração;
- não coletar caminhos completos, nomes de usuário ou conteúdo sensível sem necessidade e consentimento.

### Eventos sugeridos

```text
rebrand_banner_viewed
rebrand_details_opened
legacy_install_detected
legacy_migration_started
legacy_migration_completed
legacy_migration_failed
legacy_cleanup_eligible
legacy_cleanup_completed
new_brand_first_launch
```

### Propriedades

```text
legacy_version
migration_schema_version
migration_result
failure_stage
rollback_available
install_mode
locale
channel
```

## 19.7. Crash reporting

- atualizar nome exibido;
- manter associação histórica de releases;
- mapear versões antigas e novas;
- preservar símbolos;
- evitar projetos duplicados sem necessidade;
- sanitizar caminhos e dados pessoais.

---

# 20. Segurança do rebranding

## 20.1. Migração como superfície de ataque

Considerar:

- junctions e symlinks;
- reparse points;
- proprietário inesperado;
- arquivos manipulados;
- permissões amplas;
- path traversal;
- configurações malformadas;
- DLLs/executáveis em diretórios de dados;
- downgrade malicioso;
- manifesto adulterado.

## 20.2. Requisitos

- validar caminho canônico;
- verificar proprietário e ACL quando relevante;
- não executar binários encontrados em dados legados;
- usar staging confiável;
- limitar formatos aceitos;
- validar esquema e tamanho;
- rejeitar arquivos inesperados;
- usar escrita atômica;
- assinar updates;
- verificar checksum;
- manter privilégio mínimo;
- elevar apenas para ação necessária;
- não migrar segredos para logs.

## 20.3. Updater

- HTTPS;
- assinatura de pacote;
- autenticação de manifesto;
- proteção contra rollback;
- validação de versão;
- download seguro;
- substituição atômica;
- fallback íntegro;
- mensagem clara em falha.

## 20.4. Integridade de marca

- impedir distribuição oficial por canais não controlados;
- publicar checksums;
- centralizar downloads no domínio oficial;
- documentar canais legítimos;
- oferecer verificação de versão;
- assinar releases quando possível;
- manter contato de segurança.

## 20.5. Downloads de terceiros

Automações para ReShade e outras ferramentas devem:

- usar fonte oficial;
- validar integridade;
- respeitar licença;
- mostrar origem;
- não reempacotar sem permissão;
- permitir cancelamento;
- não executar conteúdo inesperado;
- registrar versão instalada.

---

# 21. Testes e garantia de qualidade

## 21.1. Matriz mínima

| Cenário | Resultado esperado |
|---|---|
| Instalação limpa | Inicia sem artefatos antigos |
| Atualização da última versão | Dados e atalhos preservados |
| Atualização de versões antigas | Migração ou mensagem clara |
| Interrupção na migração | Legado preservado e retry possível |
| Configuração corrompida | Isolamento e recuperação segura |
| Usuário sem administrador | App inicia; elevação só quando necessária |
| FiveM instalado | Integração detectada |
| FiveM ausente | App continua funcional |
| Múltiplos usuários | Dados não se misturam |
| Offline | Recursos locais funcionam; rede falha com clareza |
| pt-BR | Sem string antiga ou hardcoded |
| inglês | Marca e strings corretas |
| espanhol | Marca e strings corretas |
| Alto contraste | Controles visíveis |
| DPI 125/150/200% | Layout sem cortes |
| Teclado | Fluxos acessíveis |
| Leitor de tela | Nomes e estados disponíveis |
| Desinstalação | Remove app conforme política |
| Reinstalação | Detecta dados existentes |
| Downgrade | Não corrompe esquema |
| Rollback | Restaura estado validado |

## 21.2. Testes de migração

- unitários por versão de esquema;
- integração com diretórios temporários;
- fixtures de versões reais;
- idempotência;
- falta de espaço;
- acesso negado;
- arquivo em uso;
- interrupção simulada;
- symlink/junction;
- configuração maliciosa;
- DPAPI;
- migração parcial;
- cleanup;
- restauração.

## 21.3. Testes visuais

- ícone em 16, 20, 24, 32, 48, 64, 128 e 256 px;
- tema escuro;
- tema claro, se disponível;
- alto contraste;
- strings longas;
- escalas de tela;
- título da janela;
- instalador;
- desinstalador;
- notificações;
- login;
- conta;
- changelog;
- splash;
- tray.

## 21.4. Resíduos

Criar allowlist de ocorrências permitidas do nome antigo. Qualquer ocorrência fora dela falha o pipeline.

Exemplos permitidos:

```text
LegacyProductNames.cs
MigrationFromFiveMCleanerTests.cs
CHANGELOG.md
texto de transição
caminhos legados
```

## 21.5. Não regressão

- testes existentes passam;
- lógica de otimização não muda sem ticket específico;
- rollback continua funcional;
- licenças são reconhecidas;
- updater funciona;
- inicialização não piora de forma relevante;
- telemetria e logs não vazam dados.

---

# 22. Website, SEO e documentação

## 22.1. Estrutura do site

```text
Home
Produto
Recursos
Integrações
Segurança e reversibilidade
Planos
Download
Changelog
Documentação
Suporte
Sobre a Vemryx
Privacidade
Termos
```

## 22.2. Hero

### Título

> Seu PC, simplificado.

### Subtítulo

> Automatize tarefas, ajuste o desempenho e configure jogos e aplicativos com clareza, segurança e opção de desfazer.

### CTAs

- Baixar para Windows
- Ver como funciona

## 22.3. SEO

### Title

```text
Vemryx One — Automação, desempenho e configuração para Windows
```

### Meta description

```text
Simplifique tarefas técnicas do Windows com automações guiadas, diagnóstico, manutenção, integrações para jogos e alterações reversíveis.
```

### Termos de transição

- FiveMCleaner agora é Vemryx One;
- ferramenta para FiveM e Windows;
- otimização segura para FiveM;
- instalar ReShade facilmente, quando a função existir.

## 22.4. Página de transição

URL:

```text
vemryx.com/fivemcleaner
```

Conteúdo:

- explicação da mudança;
- confirmação de que é o mesmo projeto;
- continuidade de conta e dados;
- motivo da expansão;
- download oficial;
- FAQ;
- declaração de independência.

## 22.5. Redirecionamentos

- redirecionar URLs antigas para equivalentes;
- preservar parâmetros;
- evitar cadeias;
- atualizar canonical;
- manter página explicativa para o nome antigo;
- atualizar sitemap;
- monitorar 404.

## 22.6. GitHub Pages antigo

Caso permaneça em `marquezinii.github.io/FiveMCleaner/`:

- manter página mínima de transição;
- apontar para `vemryx.com/one` ou `/download`;
- não depender apenas de redirect automático do rename;
- preservar releases antigos quando necessário;
- atualizar README, tópicos e descrição.

## 22.7. Documentação

Separar:

- visão geral;
- segurança;
- instalação;
- migração;
- recursos;
- integrações;
- diagnóstico;
- restauração;
- privacidade;
- solução de problemas;
- versão legada.

## 22.8. Nomes de arquivos

```text
vemryx-one-installation.md
vemryx-one-fivem-integration.md
vemryx-one-reshade.md
migrating-from-fivemcleaner.md
```

## 22.9. Dados estruturados

Quando aplicável, incluir:

- nome;
- sistema operacional;
- categoria;
- versão;
- preço real;
- URL oficial;
- desenvolvedor;
- screenshots reais.

Não publicar avaliações inventadas.

---

# 23. GitHub, Discord, suporte e canais

## 23.1. GitHub

### Repositório

Opções:

1. renomear para `Vemryx-One`;
2. manter nome técnico temporariamente e alterar apresentação;
3. criar organização `Vemryx` e transferir.

Recomendação:

```text
github.com/vemryx/Vemryx-One
```

Antes de transferir/renomear, verificar:

- Actions;
- secrets;
- packages;
- Pages;
- badges;
- links externos;
- submodules;
- webhooks;
- clone URLs;
- pipelines de release;
- atualizador.

### README

```text
# Vemryx One
Automação, desempenho e configuração para Windows.

> Anteriormente FiveMCleaner.
```

### Topics

```text
windows
windows-utilities
pc-maintenance
automation
fivem
reshade
csharp
wpf
```

Usar apenas os correspondentes a recursos existentes.

## 23.2. Discord

### Nome do servidor

Preferência institucional:

```text
Vemryx
```

Categoria do produto:

```text
Vemryx One
```

Atualizar:

- nome;
- ícone;
- banner;
- descrição;
- onboarding;
- regras;
- FAQ;
- cargos;
- webhooks;
- bots;
- embeds;
- mensagens fixadas;
- download;
- links;
- suporte;
- bug report;
- roadmap;
- previews.

### Canais

```text
#comece-aqui
#anúncios
#download
#atualizações
#roadmap
#prévias-de-desenvolvimento
#ajuda
#bugs
#sugestões
#status
```

## 23.3. E-mails

```text
contact@vemryx.com
support@vemryx.com
security@vemryx.com
privacy@vemryx.com
billing@vemryx.com
noreply@vemryx.com
```

Remetentes:

```text
Vemryx One
Vemryx Support
Vemryx Security
```

Assuntos:

```text
[Vemryx One] Confirme seu e-mail
[Vemryx One] Redefinição de senha
[Vemryx One] Nova atualização disponível
```

## 23.4. Suporte

A central deve reconhecer ambos os nomes:

> Procurando ajuda sobre o FiveMCleaner? O aplicativo agora se chama Vemryx One. Os artigos e recursos antigos continuam válidos, salvo indicação em contrário.

## 23.5. Relatórios de bug

Atualizar:

- nome;
- versão;
- formulário;
- nomes dos anexos;
- cabeçalho dos logs;
- URL de privacidade;
- campos de migração;
- instruções de reprodução.

## 23.6. Redes

- priorizar `@vemryx`;
- usar `@vemryxone` somente quando necessário;
- evitar perfis redundantes;
- usar logo corporativo em canais da empresa;
- usar ícone de produto em canais do aplicativo.

---

# 24. Aspectos jurídicos e propriedade intelectual

## 24.1. FiveM

Fontes oficiais indicam que FiveM é marca registrada de Take-Two Interactive Software, Inc. A nova marca reduz a dependência dessa identidade, mas a integração pode continuar descrita de forma compatível e não enganosa.

## 24.2. Uso de marcas de terceiros

- usar somente o necessário para indicar compatibilidade;
- não incorporar a marca ao nome principal;
- não usar logotipo sem permissão;
- não imitar identidade oficial;
- não afirmar “oficial”, “parceiro” ou “aprovado” sem autorização;
- exibir declaração de independência;
- respeitar licenças de assets e downloads;
- revisar termos de cada integração.

## 24.3. Busca para Vemryx One

Incluir:

- `VEMRYX`;
- `VEMRYX ONE`;
- variações fonéticas;
- grafias semelhantes;
- símbolos visualmente próximos;
- segmentos de software e tecnologia;
- Brasil;
- bases internacionais relevantes;
- domínios;
- lojas;
- redes sociais;
- GitHub;
- produtos não registrados com risco de conflito.

Busca sem resultado exato não equivale a liberação jurídica.

## 24.4. Bases recomendadas

- busca de marcas do INPI;
- Classificação de Nice adotada pelo INPI;
- TMview/EUIPO;
- WIPO Global Brand Database;
- pesquisa comercial e web complementar.

## 24.5. Classes possivelmente relevantes

### Classe 9

Pode abranger software baixável e produtos de tecnologia da informação.

### Classe 42

Pode abranger desenvolvimento de software, SaaS, PaaS e serviços tecnológicos.

As especificações exatas devem descrever produtos e serviços reais.

## 24.6. Estratégia de proteção

1. busca preliminar;
2. análise de semelhança;
3. definição das especificações;
4. pedido da marca nominativa `VEMRYX`;
5. avaliação de pedido `VEMRYX ONE`;
6. pedido do símbolo quando finalizado;
7. monitoramento de oposições;
8. expansão internacional quando houver necessidade real.

## 24.7. Domínio não substitui marca

Ter `vemryx.com` é importante, mas não garante exclusividade de marca. Domínio, nome empresarial, marca registrada e nome de produto são institutos diferentes.

## 24.8. Documentos legais

Atualizar:

- termos de uso;
- política de privacidade;
- EULA;
- política de telemetria;
- reembolso;
- assinatura;
- contratos de parceria;
- licença do software;
- política de segurança;
- avisos de marcas;
- copyright;
- informações do titular.

## 24.9. Entidade jurídica e Publisher

Até existir pessoa jurídica formal:

- não usar `Vemryx Tecnologia Ltda.` como entidade constituída;
- usar nome civil ou titular jurídico correto quando exigido;
- manter `Vemryx` como marca comercial onde permitido;
- alinhar certificado, faturamento e termos à realidade.

## 24.10. Assets e software de terceiros

- manter inventário de licenças;
- preservar notices;
- não remover atribuições obrigatórias;
- verificar licença de fontes e ícones;
- verificar redistribuição de ferramentas;
- preferir download oficial quando reempacotamento não for autorizado.

---

# 25. Plano de lançamento e comunicação

## 25.1. Gates

### Gate 0 — nome e risco

- busca preliminar;
- domínio e handles;
- decisão formal;
- plano alternativo se inviável.

### Gate 1 — identidade

- símbolo final;
- wordmark;
- paleta;
- tipografia;
- exports;
- guideline;
- teste em tamanhos pequenos.

### Gate 2 — preparação técnica

- release ponte;
- aliases;
- migração;
- updater;
- instalador;
- localização;
- telemetria;
- testes.

### Gate 3 — ambiente interno

- build interna;
- upgrade;
- revisão visual;
- revisão de strings;
- revisão jurídica;
- validação de segurança.

### Gate 4 — beta controlada

- usuários voluntários;
- falhas de migração;
- teste de suporte;
- ajuste de mensagens;
- validação de SEO e downloads.

### Gate 5 — lançamento público

- site;
- instalador;
- anúncio;
- Discord;
- GitHub;
- documentação;
- e-mails;
- FAQ;
- monitoramento.

### Gate 6 — consolidação

- resolver resíduos;
- atualizar menções externas;
- acompanhar métricas;
- reduzir uso do nome antigo;
- manter compatibilidade necessária.

## 25.2. Nome de transição

```text
Vemryx One
Anteriormente FiveMCleaner
```

Não usar permanentemente como lockup.

## 25.3. Modal no aplicativo

### Título

> FiveMCleaner agora é Vemryx One

### Texto

> O aplicativo está evoluindo para uma central mais ampla de automação, desempenho e configuração para Windows. As ferramentas de FiveM continuam disponíveis, e seus dados, configurações e histórico serão preservados durante a atualização.

### Ações

- Conhecer o Vemryx One
- Continuar

O modal não deve bloquear repetidamente.

## 25.4. Release notes

```markdown
# FiveMCleaner agora é Vemryx One

Esta versão inicia uma nova fase do projeto. O aplicativo mantém suas ferramentas especializadas para FiveM, mas passa a adotar uma identidade preparada para manutenção do Windows, automações e novas integrações.

## O que mudou

- novo nome e identidade visual;
- nova organização de navegação;
- migração automática das configurações existentes;
- atualização de instalador, atalhos e documentação;
- continuidade de conta, histórico e recursos.

## O que não mudou

- o projeto continua sendo desenvolvido pela mesma equipe;
- suas configurações e dados permanecem preservados;
- os recursos de FiveM continuam disponíveis;
- segurança e capacidade de desfazer continuam prioridades.
```

## 25.5. Anúncio para Discord

```markdown
# FiveMCleaner agora é Vemryx One

O FiveMCleaner nasceu para simplificar limpeza, configuração e otimização para FiveM. Com a evolução do projeto, o nome antigo passou a representar apenas uma parte do que queremos construir.

A partir de agora, o aplicativo passa a se chamar **Vemryx One**: uma central em expansão para automatizar tarefas, melhorar a manutenção, diagnosticar problemas e configurar jogos e aplicativos no Windows.

As ferramentas de FiveM continuam no aplicativo. Contas, configurações, histórico e recursos existentes serão preservados durante a atualização.

**Novo nome:** Vemryx One<br>
**Tagline:** Seu PC, simplificado.
```

## 25.6. FAQ

### O FiveMCleaner acabou?

Não. O mesmo projeto evoluiu e agora se chama Vemryx One.

### As ferramentas de FiveM serão removidas?

Não. FiveM passa a ser uma integração especializada dentro de um produto mais amplo.

### Vou perder configurações ou histórico?

A atualização será projetada para migrar dados automaticamente, com backup e recuperação.

### Preciso pagar novamente?

Não por causa da troca de nome. Licenças e planos devem continuar na mesma conta, conforme regras comerciais vigentes.

### É outro aplicativo?

Não. A intenção é atualizar o mesmo produto.

### Por que mudou?

Porque o produto não será limitado a FiveM nem apenas a limpeza.

### É oficial do FiveM?

Não. Vemryx One é independente e oferece recursos compatíveis.

### Posso continuar procurando por FiveMCleaner?

Sim. A documentação e a página de transição reconhecerão o nome antigo.

## 25.7. Comunicação para parceiros

Kit com:

- explicação curta;
- novo logo;
- link oficial;
- descrição atualizada;
- instrução de substituição de assets;
- aviso sobre nome antigo;
- declaração de independência;
- screenshots reais.

## 25.8. Co-branding com servidores FiveM

Formato:

```text
[Logo do servidor] × [Logo Vemryx One]
Ferramenta recomendada pela comunidade/servidor
```

Só usar “parceiro oficial” com contrato ou autorização real. Não sugerir aprovação da Rockstar, Take-Two ou Cfx.

---

# 26. Métricas e indicadores

## 26.1. Técnicas

- atualização bem-sucedida;
- migração concluída;
- falhas por etapa;
- reversões;
- sessões sem crash;
- instalações duplicadas;
- perda de sessão;
- problemas de licença;
- falhas do updater;
- tempo da primeira inicialização.

## 26.2. Metas internas sugeridas

| Métrica | Meta |
|---|---:|
| Migração sem perda de dados nos testes | 100% |
| Atualização na matriz suportada | ≥ 99,5% em rollout estável |
| Ocorrências públicas indevidas do nome antigo | 0 |
| Links críticos quebrados | 0 |
| Duplicação causada pelo rebrand | 0 nos cenários suportados |
| Strings hardcoded de marca | 0 |
| Fluxos críticos acessíveis por teclado | 100% |
| Assets sem versão monocromática | 0 |

Metas internas não são garantias públicas.

## 26.3. Marca

- tráfego por `Vemryx One`;
- tráfego legado de `FiveMCleaner`;
- reconhecimento do novo nome;
- cliques no banner;
- dúvidas sobre legitimidade;
- menções corretas;
- atualização de parceiros;
- downloads no domínio oficial.

## 26.4. Negócio

- conversão de download;
- ativação;
- retenção;
- conversão Free → Pro;
- cancelamentos atribuídos à mudança;
- tickets por mil usuários;
- adoção de integrações.

## 26.5. Privacidade

- coletar somente o necessário;
- documentar finalidade;
- respeitar consentimento;
- evitar dados pessoais em eventos;
- não coletar caminhos completos;
- aplicar retenção e acesso controlado.

---

# 27. Riscos e mitigações

| Risco | Impacto | Mitigação |
|---|---|---|
| Nome juridicamente indisponível | Alto | Busca formal antes do lançamento |
| Usuários acharem que é outro app | Alto | “Anteriormente FiveMCleaner” e update in-place |
| Perda de dados | Crítico | Snapshot, staging, verificação e rollback |
| Duas instalações | Alto | Preservar identidade de upgrade e detectar legado |
| Updater antigo não encontrar nova versão | Crítico | Release ponte e feed compatível |
| SmartScreen/reputação afetada | Médio/alto | Assinatura consistente e domínio oficial |
| SEO perdido | Alto | Redirects e página de transição |
| Referências antigas espalhadas | Médio | Inventário, busca e allowlist |
| Promessa maior que produto atual | Alto | Posicionamento por estágio |
| Ícone parecido com outra marca | Alto | Busca visual e refinamento vetorial |
| Mudança de publisher gerar desconfiança | Alto | Publisher correto e comunicação prévia |
| Licenças deixarem de funcionar | Crítico | Preservar IDs e testar entitlement |
| Sessões expirarem | Médio | Compatibilidade e fallback de login |
| Telemetria fragmentada | Médio | Alias e propriedade de geração |
| Strings não traduzidas | Médio | Recursos localizados e teste |
| Uso indevido de logos de terceiros | Alto | Política de integrações e revisão jurídica |
| Caminho legado manipulado | Crítico | ACL, reparse points e staging seguro |
| Rebrand virar refatoração infinita | Alto | Escopo, gates e commits separados |
| Perda de identidade gamer | Médio | Laranja reservado a jogos e história de origem |
| Conflito entre marca corporativa e produto | Médio | Masterbrand + produto endossado |

---

# 28. Governança da marca

## 28.1. Fonte de verdade

```text
/REBRANDING_VEMRYX_ONE.md
/BRAND.md
/assets/brand/
/src/.../Resources/BrandTokens.xaml
/docs/brand/
```

Este documento define estratégia. `BRAND.md` pode ser uma versão curta para uso diário.

## 28.2. Responsabilidades

| Papel | Responsabilidade |
|---|---|
| Produto | Aprovar escopo e posicionamento |
| Marca | Aprovar logo, cores e mensagens |
| Engenharia | Aprovar migração e compatibilidade |
| Segurança | Revisar updater, dados e privilégios |
| Jurídico | Validar marca, termos e terceiros |
| Suporte | Preparar FAQ e transição |

Uma pessoa pode acumular papéis, mas as revisões devem permanecer separadas conceitualmente.

## 28.3. Controle de mudanças

Mudanças em nome, símbolo, paleta principal ou posicionamento exigem:

- proposta escrita;
- motivo;
- impacto técnico;
- impacto jurídico;
- impacto em usuários;
- aprovação;
- atualização deste documento;
- nova versão do guideline.

## 28.4. Versionamento de assets

```text
brand-v1.0.0
brand-v1.1.0
```

- patch: correção de exportação;
- minor: nova variante sem mudar símbolo;
- major: mudança de símbolo, nome ou sistema principal.

## 28.5. Congelamento

Após lançamento estável, evitar nova troca por preferência estética. Nova mudança só por:

- impedimento jurídico;
- mudança estrutural de empresa;
- aquisição/fusão;
- evidência forte de falha estratégica.

---

# 29. Regras para agentes de IA e implementadores

## 29.1. Ordem de leitura

1. este documento;
2. `AGENTS.md`;
3. `CLAUDE.md`, se aplicável;
4. regras de segurança;
5. documentação do instalador/updater;
6. arquitetura de persistência;
7. testes de rollback e migração.

## 29.2. Regras obrigatórias

- não executar replace global indiscriminado;
- não renomear IDs persistidos sem migração;
- não alterar lógica de otimização em tarefa de branding;
- não apagar caminhos antigos na primeira execução;
- não remover compatibilidade do updater antigo sem gate;
- não introduzir strings hardcoded;
- não usar assets rasterizados como fonte;
- não inventar razão social;
- não afirmar afiliação com FiveM;
- não alterar telemetria sem revisão de privacidade;
- não misturar rebrand com refatoração não relacionada;
- não quebrar testes para “resolver depois”.

## 29.3. Commits

```text
chore(brand): add Vemryx One brand tokens and assets
feat(migration): add legacy FiveMCleaner data discovery
feat(migration): migrate settings to Vemryx One paths
chore(installer): update public product identity
chore(localization): replace public brand strings
feat(updater): support bridge update to Vemryx One
chore(docs): publish rebrand transition documentation
```

## 29.4. PRs separadas

1. tokens e assets;
2. strings/localização;
3. migração;
4. instalador/updater;
5. backend/telemetria;
6. website/documentação;
7. limpeza de resíduos.

## 29.5. Evidências na PR

- escopo;
- matriz de impactos;
- testes;
- screenshots;
- identificadores preservados;
- identificadores alterados;
- rollback;
- busca por nome antigo;
- riscos conhecidos;
- confirmação de ausência de dados reais.

## 29.6. Guardrail

O pipeline permite `FiveM` em integrações e documentação compatível, mas bloqueia `FiveMCleaner` fora da allowlist.

## 29.7. Regra de incerteza

Quando não souber se um identificador pode ser renomeado:

1. localizar criação;
2. localizar persistência;
3. localizar consumidores externos;
4. verificar instalador/updater;
5. adicionar teste;
6. preservar até haver migração segura.

---

# 30. Checklist mestre

## 30.1. Estratégia

- [ ] Confirmar `Vemryx One` após busca formal.
- [ ] Confirmar tagline.
- [ ] Confirmar descritor.
- [ ] Confirmar arquitetura masterbrand + produto.
- [ ] Confirmar Free, Pro e Business futuro.
- [ ] Definir pronúncia oficial, se necessário.
- [ ] Congelar regras de naming.

## 30.2. Jurídico

- [ ] Pesquisar `VEMRYX` no INPI.
- [ ] Pesquisar `VEMRYX ONE` no INPI.
- [ ] Pesquisar variantes fonéticas e visuais.
- [ ] Pesquisar bases internacionais.
- [ ] Definir classes e especificações.
- [ ] Revisar uso de FiveM.
- [ ] Revisar ReShade e outros terceiros.
- [ ] Atualizar termos e privacidade.
- [ ] Definir titular jurídico real.
- [ ] Revisar licenças de fontes, ícones e assets.

## 30.3. Identidade visual

- [ ] Criar símbolo V/1 em vetor.
- [ ] Testar em 16 px.
- [ ] Criar versão monocromática.
- [ ] Criar alto contraste.
- [ ] Criar wordmark horizontal.
- [ ] Criar lockup vertical.
- [ ] Criar favicon.
- [ ] Criar `.ico` multirresolução.
- [ ] Criar bandeja.
- [ ] Criar assets de instalador.
- [ ] Criar assets de website.
- [ ] Criar Open Graph.
- [ ] Criar avatar e banner.
- [ ] Documentar clearspace e proibições.
- [ ] Garantir alpha real.

## 30.4. Design system

- [ ] Criar tokens escuros.
- [ ] Preparar tokens claros.
- [ ] Validar contraste.
- [ ] Definir tipografia.
- [ ] Definir iconografia.
- [ ] Definir espaçamento e raios.
- [ ] Definir estados.
- [ ] Definir redução de movimento.
- [ ] Validar alto contraste.
- [ ] Validar teclado e leitor de tela.

## 30.5. Aplicativo

- [ ] Atualizar títulos.
- [ ] Atualizar ícone do executável.
- [ ] Atualizar splash.
- [ ] Atualizar tray.
- [ ] Atualizar Sobre.
- [ ] Atualizar login e conta.
- [ ] Atualizar navegação.
- [ ] Atualizar notificações.
- [ ] Atualizar diálogos.
- [ ] Atualizar logs visíveis.
- [ ] Atualizar exportações.
- [ ] Remover hardcodes.
- [ ] Revisar pt-BR, en e es.

## 30.6. Migração

- [ ] Inventariar caminhos legados.
- [ ] Inventariar registro.
- [ ] Inventariar IDs persistidos.
- [ ] Criar marcador de migração.
- [ ] Criar snapshot.
- [ ] Implementar staging.
- [ ] Implementar verificação.
- [ ] Implementar ativação atômica.
- [ ] Implementar rollback.
- [ ] Implementar retry.
- [ ] Preservar DPAPI/sessão.
- [ ] Testar interrupção.
- [ ] Testar symlink/junction.
- [ ] Não apagar legado prematuramente.

## 30.7. Instalador e updater

- [ ] Identificar tecnologia real.
- [ ] Preservar AppId/UpgradeCode necessário.
- [ ] Criar release ponte.
- [ ] Atualizar nome público.
- [ ] Atualizar Publisher corretamente.
- [ ] Atualizar atalhos.
- [ ] Evitar duplicação.
- [ ] Atualizar desinstalador.
- [ ] Atualizar feed.
- [ ] Validar assinatura e hash.
- [ ] Testar versões suportadas.

## 30.8. Backend e contas

- [ ] Atualizar display names.
- [ ] Atualizar e-mails.
- [ ] Atualizar OAuth consent.
- [ ] Preservar IDs de usuário.
- [ ] Preservar sessões quando possível.
- [ ] Preservar licenças.
- [ ] Preservar assinaturas.
- [ ] Atualizar recibos e checkout.
- [ ] Atualizar telemetria sem quebrar séries.
- [ ] Atualizar crash reporting.

## 30.9. Website e SEO

- [ ] Publicar `/one`.
- [ ] Publicar `/fivemcleaner`.
- [ ] Configurar redirects.
- [ ] Atualizar title e meta.
- [ ] Atualizar sitemap.
- [ ] Atualizar canonical.
- [ ] Atualizar downloads.
- [ ] Atualizar screenshots.
- [ ] Atualizar schema.
- [ ] Monitorar 404.
- [ ] Preservar links antigos importantes.

## 30.10. Comunidade e suporte

- [ ] Renomear Discord.
- [ ] Atualizar ícone e banner.
- [ ] Atualizar webhooks.
- [ ] Atualizar FAQ.
- [ ] Atualizar mensagens fixadas.
- [ ] Atualizar canais e cargos.
- [ ] Preparar anúncio.
- [ ] Preparar respostas de suporte.
- [ ] Comunicar parceiros.
- [ ] Atualizar materiais externos.

## 30.11. GitHub

- [ ] Decidir organização/repositório.
- [ ] Atualizar README.
- [ ] Atualizar badges.
- [ ] Atualizar topics.
- [ ] Atualizar Actions.
- [ ] Atualizar artefatos.
- [ ] Atualizar templates de issue/PR.
- [ ] Atualizar SECURITY.md.
- [ ] Atualizar releases.
- [ ] Criar allowlist do legado.

## 30.12. Lançamento

- [ ] Build final assinado.
- [ ] Testes críticos aprovados.
- [ ] Backup e rollback validados.
- [ ] Site publicado.
- [ ] Documentação publicada.
- [ ] FAQ publicada.
- [ ] Anúncio pronto.
- [ ] Suporte preparado.
- [ ] Monitoramento ativo.
- [ ] Plano de contenção definido.

## 30.13. Pós-lançamento

- [ ] Monitorar falhas de migração.
- [ ] Monitorar duplicação.
- [ ] Monitorar tickets de licença.
- [ ] Monitorar SEO.
- [ ] Corrigir links externos.
- [ ] Remover resíduos não permitidos.
- [ ] Avaliar limpeza segura do legado.
- [ ] Atualizar este documento.

---

# 31. Critérios de aceite e Definition of Done

## Marca

- nome validado conforme processo definido;
- logo final em vetor;
- variantes mínimas exportadas;
- paleta e tipografia documentadas;
- ícone funciona em 16 px;
- nenhuma transparência falsa.

## Produto

- superfícies públicas usam Vemryx One;
- FiveM aparece apenas como integração/referência legítima;
- nenhuma promessa excede recursos reais;
- navegação representa a arquitetura nova;
- strings estão localizadas.

## Técnica

- atualização in-place testada;
- migração idempotente;
- dados preservados;
- updater antigo alcança nova versão;
- instalação duplicada evitada;
- licenças e contas válidas;
- rollback validado;
- testes críticos passam.

## Segurança

- pacotes verificados;
- caminhos legados tratados como entrada não confiável;
- segredos não aparecem em logs;
- elevação mínima;
- downloads de terceiros verificados;
- privacidade corresponde ao comportamento.

## Comunicação

- site e página de transição publicados;
- FAQ disponível;
- Discord, GitHub e e-mails atualizados;
- parceiros receberam kit;
- redirects testados.

## Resíduos

- `FiveMCleaner` só existe na allowlist;
- nenhum asset antigo permanece por engano;
- não há dois nomes conflitantes na mesma tela;
- releases usam nomenclatura nova.

---

# 32. Decisões pendentes

| Decisão | Recomendação | Status |
|---|---|---|
| Liberação jurídica de Vemryx One | Realizar busca formal | Pendente |
| Símbolo final | V geométrico + 1 em espaço negativo | Pendente |
| Relação com logo corporativo | Produto derivado, não substituto | Pendente de desenho |
| Fonte do site | Geist Sans | Pendente |
| Tema claro no lançamento | Preparar tokens; implementar conforme escopo | Pendente |
| Versão de lançamento | Incrementar major se mudança estrutural | Pendente |
| Rename do repositório | `Vemryx-One` em organização Vemryx | Pendente |
| Package identity | Preservar quando necessário | Depende do instalador |
| Nome do plano gratuito | Exibir “Gratuito” no produto | Pendente final |
| Business | Reservar; não lançar antes da função | Futuro |
| Assistente inteligente | Não incluir `AI` no produto | Futuro |
| Duração da transição | Baseada em versões e adoção | Pendente |

---

# 33. Mapa de substituição

| Antigo | Novo | Observação |
|---|---|---|
| FiveMCleaner | Vemryx One | Nome público |
| FiveM Cleaner | Vemryx One | Variante textual |
| 5M | V/1 | Conceito de símbolo |
| FiveMCleaner Pro | Vemryx One Pro | Plano |
| FiveMCleaner Business | Vemryx One Business | Futuro |
| FiveMCleaner.exe | VemryxOne.exe | Com migração/updater |
| FiveMCleaner Setup | Vemryx One Setup | Instalador |
| FiveMCleaner Updater | Vemryx One Updater | Updater |
| FiveM como marca central | FiveM como integração | Estratégia |
| Cleaner | Manutenção | Área funcional |
| Boost | Desempenho/Perfil | Terminologia honesta |
| Problemas encontrados | Recomendações disponíveis | Evitar scareware |
| Otimizar tudo | Revisar e aplicar | Mais controle |
| Lixo | Arquivos temporários | Precisão |

---

# 34. Apêndices técnicos

## 34.1. Tokens XAML

> Base inicial. Adaptar ao padrão real de `ResourceDictionary`.

```xml
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Color x:Key="VemryxColorCanvas">#FF0B0D12</Color>
    <Color x:Key="VemryxColorSurface">#FF131722</Color>
    <Color x:Key="VemryxColorElevated">#FF1A2030</Color>
    <Color x:Key="VemryxColorBorderSubtle">#FF2A3243</Color>

    <Color x:Key="VemryxColorTextPrimary">#FFF7F9FC</Color>
    <Color x:Key="VemryxColorTextSecondary">#FF97A0B3</Color>

    <Color x:Key="VemryxColorBrandAccent">#FF5B7CFF</Color>
    <Color x:Key="VemryxColorBrandSolid">#FF4B64F2</Color>
    <Color x:Key="VemryxColorBrandCyan">#FF27C8FF</Color>

    <Color x:Key="VemryxColorSuccess">#FF32D583</Color>
    <Color x:Key="VemryxColorWarning">#FFF5B942</Color>
    <Color x:Key="VemryxColorError">#FFF04438</Color>
    <Color x:Key="VemryxColorDangerSolid">#FFD92D20</Color>
    <Color x:Key="VemryxColorGaming">#FFFF8A1F</Color>

    <SolidColorBrush x:Key="VemryxBrushCanvas" Color="{StaticResource VemryxColorCanvas}" />
    <SolidColorBrush x:Key="VemryxBrushSurface" Color="{StaticResource VemryxColorSurface}" />
    <SolidColorBrush x:Key="VemryxBrushElevated" Color="{StaticResource VemryxColorElevated}" />
    <SolidColorBrush x:Key="VemryxBrushTextPrimary" Color="{StaticResource VemryxColorTextPrimary}" />
    <SolidColorBrush x:Key="VemryxBrushTextSecondary" Color="{StaticResource VemryxColorTextSecondary}" />
    <SolidColorBrush x:Key="VemryxBrushBrandAccent" Color="{StaticResource VemryxColorBrandAccent}" />
    <SolidColorBrush x:Key="VemryxBrushBrandSolid" Color="{StaticResource VemryxColorBrandSolid}" />
</ResourceDictionary>
```

## 34.2. Tokens CSS

```css
:root {
  --vx-bg-canvas: #0b0d12;
  --vx-bg-surface: #131722;
  --vx-bg-elevated: #1a2030;
  --vx-border-subtle: #2a3243;

  --vx-text-primary: #f7f9fc;
  --vx-text-secondary: #97a0b3;

  --vx-brand-accent: #5b7cff;
  --vx-brand-solid: #4b64f2;
  --vx-brand-cyan: #27c8ff;

  --vx-success: #32d583;
  --vx-warning: #f5b942;
  --vx-error: #f04438;
  --vx-danger-solid: #d92d20;
  --vx-gaming: #ff8a1f;

  --vx-gradient-brand: linear-gradient(135deg, #5b7cff 0%, #27c8ff 100%);

  --vx-radius-control: 8px;
  --vx-radius-card: 12px;
  --vx-radius-modal: 16px;

  --vx-space-1: 4px;
  --vx-space-2: 8px;
  --vx-space-3: 12px;
  --vx-space-4: 16px;
  --vx-space-6: 24px;
  --vx-space-8: 32px;
}
```

## 34.3. Manifesto de marca JSON

```json
{
  "company": "Vemryx",
  "product": "Vemryx One",
  "previousProductName": "FiveMCleaner",
  "tagline": "Seu PC, simplificado.",
  "descriptor": "Automação, desempenho e configuração para Windows.",
  "slug": "vemryx-one",
  "website": "https://vemryx.com/one",
  "support": "https://vemryx.com/support",
  "legalStatus": "pending-clearance",
  "brandGeneration": 2
}
```

## 34.4. Chaves de localização

```text
Brand.CompanyName
Brand.ProductName
Brand.PreviousProductName
Brand.Tagline
Brand.Descriptor
Rebrand.Banner.Title
Rebrand.Banner.Description
Rebrand.Modal.Title
Rebrand.Modal.Body
Rebrand.Modal.LearnMore
Rebrand.Modal.Continue
Migration.LegacyDetected.Title
Migration.LegacyDetected.Description
Migration.Progress.Preparing
Migration.Progress.Copying
Migration.Progress.Verifying
Migration.Progress.Completed
Migration.Error.Generic
Migration.Error.AccessDenied
Migration.Error.InsufficientSpace
Legal.ThirdPartyTrademarkDisclaimer
```

## 34.5. Identidade centralizada

```csharp
public static class ProductIdentity
{
    public const string CompanyName = "Vemryx";
    public const string ProductName = "Vemryx One";
    public const string ProductSlug = "vemryx-one";
    public const string LegacyProductName = "FiveMCleaner";
    public const int BrandGeneration = 2;
}
```

Valores de UI continuam localizados quando apropriado.

## 34.6. Allowlist do legado

```text
src/**/Migration/**
tests/**/Legacy/**
docs/migrating-from-fivemcleaner.md
CHANGELOG.md
REBRANDING_VEMRYX_ONE.md
```

## 34.7. Inventário técnico

```markdown
| Identificador | Local | Público? | Persistido? | Externo? | Ação | Teste |
|---|---|---:|---:|---:|---|---|
| FiveMCleaner.exe | Installer | Sim | Não | Updater | Migrar | Upgrade E2E |
| %LocalAppData%\\FiveMCleaner | App | Não | Sim | Não | Migrar | Data migration |
| UpgradeCode | WiX | Não | Sim | Windows Installer | Preservar | Major upgrade |
```

## 34.8. Relatório de migração

```json
{
  "migrationSchema": 1,
  "sourceProduct": "FiveMCleaner",
  "targetProduct": "Vemryx One",
  "sourceVersion": "<version>",
  "targetVersion": "<version>",
  "status": "completed",
  "startedAtUtc": "<timestamp>",
  "completedAtUtc": "<timestamp>",
  "rollbackAvailable": true,
  "sensitiveDataIncluded": false
}
```

## 34.9. Página Sobre

```text
Vemryx One
Versão <versão>
Automação, desempenho e configuração para Windows.

Desenvolvido pela Vemryx
Anteriormente FiveMCleaner

Site
Documentação
Licenças de terceiros
Privacidade
Termos
Verificar atualizações
```

## 34.10. Compatibilidade

```text
FiveM é mencionado exclusivamente para indicar compatibilidade de determinados recursos. Vemryx One é um produto independente e não representa afiliação, patrocínio ou endosso pelos titulares das marcas citadas.
```

O texto final deve passar por revisão jurídica.

---

# 35. Referências

Fontes consultadas em **22 de agosto de 2026**:

1. **Cfx Support — Rockstar Games Creator Platform License Agreement**<br>
   https://support.cfx.re/hc/en-us/articles/24856975424924-Rockstar-Games-Creator-Platform-License-Agreement

2. **INPI — Classificação de produtos e serviços para marcas**<br>
   https://www.gov.br/inpi/pt-br/servicos/marcas/classificacao-marcas

3. **EUIPO — Disponibilidade e pesquisa de marcas**<br>
   https://www.euipo.europa.eu/en/trade-marks/before-applying/availability

4. **WIPO — Nice Classification**<br>
   https://www.wipo.int/en/web/classification-nice

5. **W3C — Web Content Accessibility Guidelines 2.2**<br>
   https://www.w3.org/TR/WCAG22/

6. **Microsoft Learn — Design Windows apps overview**<br>
   https://learn.microsoft.com/en-us/windows/apps/design/

7. **Microsoft Learn — Develop accessible Windows apps**<br>
   https://learn.microsoft.com/en-us/windows/apps/develop/accessibility

8. **Microsoft Learn — Packaging overview for Windows apps**<br>
   https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/packaging/

---

# Encerramento

O rebranding para **Vemryx One** não é apenas uma troca de nome. Ele reposiciona o projeto para uma categoria maior, reduz dependência de marcas de terceiros, cria espaço para expansão e estabelece uma identidade coerente com um produto profissional de automação e manutenção para Windows.

A execução correta exige quatro frentes sincronizadas:

1. **marca juridicamente defensável;**
2. **identidade visual escalável;**
3. **produto honesto e coerente com a promessa;**
4. **migração técnica sem perda de confiança ou dados.**

## Síntese final

```text
EMPRESA
Vemryx

PRODUTO
Vemryx One

TAGLINE
Seu PC, simplificado.

CATEGORIA
Automação, desempenho e configuração para Windows.

PROMESSA
Complexidade removida sem remover o controle.

SÍMBOLO
V geométrico com referência discreta ao número 1.

PALETA
Preto/grafite + índigo + ciano.
Laranja reservado à categoria de jogos.

PERSONALIDADE
Moderna, técnica, calma, transparente e confiável.

DIFERENCIAL
Ações guiadas, explicadas e reversíveis — não apenas limpeza ou boost.
```
