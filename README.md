# Resale API

API para gerenciamento de revendas e pedidos de bebidas, desenvolvida para integração com a API da Brewery.

## Funcionalidades

### 1. Cadastro de Revendas (CRUD Completo)

- **CNPJ**: Obrigatório e validado
- **Razão Social**: Obrigatória
- **Nome Fantasia**: Obrigatório
- **Email**: Obrigatório e validado
- **Telefones**: Opcional, múltiplos telefones com validação
- **Contatos**: Obrigatório, múltiplos contatos com um principal
- **Endereços de Entrega**: Obrigatório, múltiplos endereços

### 2. Recebimento de Pedidos dos Clientes

- Identificação do cliente
- Lista de produtos com quantidades
- Sem regra de pedido mínimo na revenda
- Resposta com identificação do pedido e lista de itens

### 3. Integração com API da Brewery

- Consolidação de pedidos da revenda
- **Regra de pedido mínimo**: 1000 unidades
- Tratamento de instabilidade da API com retry automático
- Mock service para desenvolvimento/testes

## Tecnologias Utilizadas

- **.NET 8.0**
- **Entity Framework Core** com SQL Server
- **Swagger/OpenAPI** para documentação
- **FluentValidation** para validações
- **Polly** para políticas de retry
- **Serilog** para logging estruturado

## Configuração

### Pré-requisitos

- .NET 8.0 SDK
- SQL Server (LocalDB ou instância completa)
- Visual Studio 2022 ou VS Code

### Configuração do Banco de Dados

1. Atualize a connection string no `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=resale-api;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

2. Execute as migrations:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Executando a Aplicação

```bash
dotnet run
```

A API estará disponível em:

- **HTTPS**: https://localhost:7001
- **HTTP**: http://localhost:5001
- **Swagger**: https://localhost:7001/swagger

## Endpoints Principais

### Revendas

- `POST /api/resellers` - Criar revenda
- `GET /api/resellers` - Listar revendas (paginado)
- `GET /api/resellers/{id}` - Buscar revenda por ID
- `GET /api/resellers/cnpj/{cnpj}` - Buscar revenda por CNPJ
- `PUT /api/resellers/{id}` - Atualizar revenda
- `DELETE /api/resellers/{id}` - Desativar revenda

### Pedidos dos Clientes

- `POST /api/customerorders` - Criar pedido do cliente
- `GET /api/customerorders` - Listar pedidos (paginado)
- `GET /api/customerorders/reseller/{id}` - Pedidos por revenda
- `GET /api/customerorders/reseller/{id}/pending` - Pedidos pendentes
- `PATCH /api/customerorders/{id}/status` - Atualizar status
- `POST /api/customerorders/{id}/cancel` - Cancelar pedido

### Pedidos para Brewery

- `POST /api/breweryorders` - Criar e enviar pedido para Brewery
- `GET /api/breweryorders` - Listar pedidos Brewery (paginado)
- `GET /api/breweryorders/pending` - Pedidos pendentes
- `GET /api/breweryorders/failed` - Pedidos com falha
- `POST /api/breweryorders/{id}/retry` - Retentar envio
- `POST /api/breweryorders/process-pending` - Processar pendentes

### Produtos

- `GET /api/products` - Listar produtos ativos
- `GET /api/products/{id}` - Buscar produto por ID
- `GET /api/products/brand/{brand}` - Produtos por marca
- `GET /api/products/category/{category}` - Produtos por categoria
- `POST /api/products` - Criar produto (para demonstração)

## Validações Implementadas

### CNPJ

- Formato válido (14 dígitos)
- Dígitos verificadores corretos
- Unicidade no sistema

### Email

- Formato válido
- Unicidade no sistema

### Telefone

- Formato brasileiro válido
- Suporte a celular e fixo

### Regras de Negócio

- **Contatos**: Pelo menos um contato principal por revenda
- **Endereços**: Pelo menos um endereço padrão por revenda
- **Pedidos Brewery**: Mínimo de 1000 unidades consolidadas
- **Consolidação**: Produtos iguais são somados automaticamente

## Tratamento de Falhas

### API da Brewery

- **Retry Policy**: 3 tentativas com backoff exponencial
- **Timeout**: 30 seconds configurável
- **Mock Service**: Para desenvolvimento e testes
- **Logging**: Detalhado para troubleshooting

### Persistência de Pedidos

- Pedidos nunca são perdidos
- Estados intermediários salvos no banco
- Possibilidade de retry manual via API

## Estrutura do Projeto

```
resale-api/
├── Controllers/         # Controladores da API
├── Data/               # Contexto do Entity Framework
├── DTOs/               # Data Transfer Objects
├── Models/             # Modelos de domínio
├── Repositories/       # Padrão Repository
├── Services/           # Lógica de negócio
├── Migrations/         # Migrations do EF Core
└── Properties/         # Configurações de launch
```

## Observabilidade

### Logging

- **Structured Logging** com Serilog
- **Log Levels** configuráveis por ambiente
- **Correlation IDs** para rastreamento de requisições

### Métricas

- Contadores de pedidos criados/enviados
- Tempos de resposta da API Brewery
- Taxa de sucesso/falha dos envios

### Health Checks

- Disponibilidade do banco de dados
- Conectividade com API Brewery
- Status dos serviços críticos

## Segurança

### Validações

- Input validation em todos os endpoints
- Sanitização de dados de entrada
- Rate limiting configurável

### Dados Sensíveis

- Logs não incluem dados pessoais
- Connection strings criptografadas em produção
- Tokens de API seguros

## Testes

### Testes Unitários

```bash
dotnet test
```

### Testes de Integração

- Testes com banco em memória
- Mock da API Brewery
- Cenários de retry e falha

## Deploy

### Ambiente de Produção

1. Configure as variáveis de ambiente:

   - `ConnectionStrings__DefaultConnection`
   - `BreweryApi__BaseUrl`

- `BreweryApi__UseMockService=false`

2. Execute as migrations:

```bash
dotnet ef database update --environment Production
```

3. Publique a aplicação:

```bash
dotnet publish -c Release
```

## Monitoring em Produção

- **Application Insights** para telemetria
- **Health Checks** endpoint: `/health`
- **Prometheus** metrics: `/metrics`
- **Swagger** desabilitado em produção

## Contato

Para dúvidas ou suporte, entre em contato com a equipe de desenvolvimento.
