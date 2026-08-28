# Proveniência dos ativos Ralven

## Material recebido

| Destino canônico | Origem | SHA-256 |
| --- | --- | --- |
| `source/received/ralven-app-icon-original.png` | `2515d683-e6ab-4ca8-a089-e3ca3f8cf728.png` | `07B4C6E60C1AD68CB57162BF7F10D81BABCF060F47BD0022C182658A9773C928` |
| `source/received/guidelines/ralven-brand-guidelines-01.png` | captura `23_30_24` | `FD8133667CD6A24C211E9FCF8589D1D12F20AE175A247AFD30C5EF0A21F5274E` |
| `source/received/guidelines/ralven-brand-guidelines-02.png` | captura `23_30_29` | `D004FC8226DD1EAAA67198CC213E47F7970DF7714C8B1649F26C019A7A6BD543` |
| `source/received/guidelines/ralven-brand-guidelines-03.png` | captura `23_30_33` | `CA5AA4378165FCBB01E0F4D43A3867EE204D150A203FDC6E2B2737DCBFCECAED` |
| `source/received/guidelines/ralven-brand-guidelines-04.png` | captura `23_30_37` | `3ACDB234B0CB009DDC9C5A010CBE947A6A75B26109B0D46E22DF073BC97BAEBB` |
| `source/received/guidelines/ralven-brand-guidelines-05.png` | captura `23_30_51` | `4DF3AC9CC2AA21CCF5F7CF9D072DFBB827F06CCE6F7AD6D22534E47223AC9AAD` |
| `source/received/guidelines/ralven-brand-guidelines-06.png` | captura `23_30_55` | `D187D78040CD5EFE707D3B3C598221A273532FECBF56D39BCBC2AC048F4A9771` |
| `source/received/ralven-atmosphere-background-original.png` | background gerado nesta tarefa e adotado como ativo Ralven | `9ABC3C4923DDD051D1CBA62EE4A1DD0C73BCF36AF79073531888F4D357A60A1C` |

As capturas `23_30_43` e `23_30_47` não foram armazenadas porque são cópias
byte a byte das páginas 03 e 04, respectivamente.

Os seis boards têm 1448 × 1086 px, RGB e não possuem alpha. O app icon original
tem 1254 × 1254 px, RGBA. O background tem 1672 × 941 px, RGB.

## Background gerado

- Ferramenta: gerador de imagens integrado do Codex.
- Modo: `stylized-concept`.
- Uso: fundo abstrato para hero/preview, sempre com texto e interface reais
  sobrepostos pelo produto.
- Prompt: fundo tecnológico abstrato premium em 16:9, com planos carvão e
  grafite, textura metálica sutil, cortes diagonais precisos, peso visual à
  direita e espaço negativo à esquerda; paleta restrita às seis cores Ralven;
  sem texto, letras, logo, UI, pessoas, jogos, alegações, neon ou watermark.

## Inter 4.1

- Projeto oficial: `https://github.com/rsms/inter`
- Release: `https://github.com/rsms/inter/releases/tag/v4.1`
- Pacote: `https://github.com/rsms/inter/releases/download/v4.1/Inter-4.1.zip`
- SHA-256 do ZIP baixado: `9883FDD4A49D4FB66BD8177BA6625EF9A64AA45899767DDE3D36AA425756B11E`
- Licença incluída: SIL Open Font License 1.1.

Arquivos preservados do pacote oficial:

| Arquivo | SHA-256 |
| --- | --- |
| `fonts/inter-4.1/InterVariable.ttf` | `4989B125924991B90D05B2D16E0E388C48F7D5BB8B30539BBF9C755278D0CCAF` |
| `fonts/inter-4.1/InterVariable-Italic.ttf` | `D6F1F6A172D9E588438DB9F986FD5CFAD7B30F644374080A8A9D4D91E344586F` |
| `fonts/inter-4.1/web/InterVariable.woff2` | `693B77D4F32EE9B8BFC995589B5FAD5E99ADF2832738661F5402F9978429A8E3` |
| `fonts/inter-4.1/web/InterVariable-Italic.woff2` | `E564F652916DB6C139570FEFB9524A77C4D48F30C92928DE9DB19B6B5C7A262A` |
| `fonts/inter-4.1/LICENSE.txt` | `262481E844521B326F5ECD053E59B98C8B2DA78C8EE1BDBB6E8174305E54935A` |

## Derivação do app icon

1. Verificar o SHA-256 do PNG original.
2. Localizar o menor retângulo que contém pixels com alpha maior ou igual a 2.
3. Excluir apenas o ruído alpha 1 fora do tile visível.
4. Centralizar o retângulo em canvas quadrado transparente, sem esticar.
5. Redimensionar com bicúbico de alta qualidade.
6. Empacotar os frames PNG no contêiner ICO, sem conversão para paleta.
7. Copiar os exports 1024 px e ICO para `src/Ralven.App/Assets/`.

`CHECKSUMS.sha256` cobre fontes, fontes recebidas, tokens, guideline e exports.
Ele é regenerado pelo mesmo script após qualquer exportação autorizada.
