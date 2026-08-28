# Identificadores externos e importação local

Ralven é o único nome público e o único contrato de execução suportado. Esta lista documenta as poucas ocorrências anteriores que não representam branding ativo.

## Importação local

`LegacyDataImporter` reconhece as raízes locais das duas gerações anteriores somente para copiar, uma vez e por allowlist, dados pessoais conhecidos. Os nomes aparecem ali e no teste correspondente porque são as chaves reais no filesystem.

Não existem aliases de executável, assembly, mutex, startup, instalador, pacote, repositório ou updater.

## IDs externos implantados

O hostname do Worker e o nome do banco D1 ainda contêm o identificador histórico original. O `database_id` e o projeto Firebase também são recursos já provisionados. Alterar texto no repositório não os renomeia e apontaria o aplicativo para recursos inexistentes.

Pelo mesmo motivo, o workflow lê o segredo GitHub já provisionado
`FIVEMCLEANER_SIGNING_PASSWORD` e o expõe somente ao processo como
`RALVEN_SIGNING_PASSWORD`. O nome interno do secret não é exibido ao usuário e
preservá-lo evita reentrada manual do material que protege a cadeia de update.

Esses valores podem aparecer somente em:

- configurações e validações exatas do cliente;
- `infra/cloudflare-worker/wrangler.toml` e comandos operacionais;
- CSP/origem do dashboard;
- referência ao secret de assinatura no workflow de release;
- testes que congelam o host permitido;
- documentação operacional que os identifica explicitamente como infraestrutura.

Uma futura troca exige criar os recursos, migrar dados, configurar segredos, validar produção e só então alterar os consumidores. Até lá, esses IDs não devem ser apresentados como nome do produto.

## Fontes recebidas

Nomes originais de arquivos, hashes e referências às gerações anteriores podem existir na procedência imutável de `assets/brand/source/received`. Isso registra origem; não autoriza reutilizar a marca antiga.
