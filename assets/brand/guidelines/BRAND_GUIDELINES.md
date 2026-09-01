# Ralven — guia de identidade visual

Esta pasta é a fonte canônica dos ativos visuais da Ralven. Os PNGs recebidos
em `source/received/` são imutáveis; ativos prontos para consumo ficam em
`export/` e são regenerados por `scripts/Export-RalvenBrandAssets.ps1`.

## Identidade

- Nome público: **Ralven**.
- Símbolo: `R` geométrico combinado com um elemento de velocidade e direção.
- Conceitos: movimento, direção, eficiência, performance, praticidade,
  confiança, precisão e inovação.
- Tipografia: **Inter 4.1**, com Thin, Light, Regular, Medium, SemiBold, Bold e
  ExtraBold.
- Iconografia: linear, minimalista, com cantos arredondados e traço uniforme.

## Paleta oficial

| Nome | Hex |
| --- | --- |
| Carvão | `#0A0A0B` |
| Grafite | `#111214` |
| Cinza escuro | `#1D1E21` |
| Cinza | `#2D2E33` |
| Cinza claro | `#A6A7AC` |
| Branco | `#FFFFFF` |

Essas cores descrevem a marca. Cores semânticas de sucesso, aviso, erro,
informação e foco continuam sendo tokens de produto separados e devem manter
contraste e acessibilidade.

## Uso do símbolo

- Preserve proporção, cores e orientação.
- Garanta contraste e área de respiro equivalente à altura do `R`.
- O tamanho mínimo indicado pelo material recebido é 16 px.
- Não aplique o símbolo sobre fundos complexos.
- Não recorte o símbolo de dentro dos boards de guideline.
- Não trace nem vetorize automaticamente o PNG oficial.

O único master gráfico isolado recebido é o app icon rasterizado, com o símbolo
dentro do tile escuro. Os lockups horizontal, empilhado, monocromático, outline,
wordmark e símbolo sem fundo aparecem apenas em boards achatados. Eles não devem
ser fabricados a partir dessas capturas; permanecem pendentes de arquivos
vetoriais aprovados.

## Exportações oficiais

- PNG quadrado: 16, 20, 24, 32, 40, 48, 64, 96, 128, 192, 256, 512 e 1024 px.
- ICO multiframe: 16, 20, 24, 32, 40, 48, 64, 128 e 256 px.
- Background atmosférico: 1672 × 941 px, preservado sem recompressão.

O exportador descarta somente ruído isolado com alpha 1, centraliza o conteúdo
visível em canvas quadrado transparente e redimensiona sem distorção. Nenhuma
forma do logo é redesenhada.

## Boards não são especificação de produto

As telas de módulos, instalador, notificações, menus, loja e marketing são
referências visuais. Elas não comprovam recursos, compatibilidade, avaliações,
compras, domínios, redes sociais ou ganhos universais de FPS/latência. Nenhuma
dessas alegações deve virar comportamento ou texto público sem evidência própria.

## Tipografia distribuída

`fonts/inter-4.1/` contém os arquivos variáveis TTF e WOFF2 oficiais da release
4.1, além da licença SIL Open Font License 1.1. Use os arquivos locais para não
depender de uma fonte instalada na máquina nem de CDN em runtime.
