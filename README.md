# ValidProfiles API

API para gerenciamento de perfis de usuários com validação de permissões. Esta aplicação permite o cadastro, consulta, atualização e exclusão de perfis, além de validar permissões específicas para cada perfil.

## Índice

- [ValidProfiles API](#validprofiles-api)
  - [Índice](#índice)
  - [Tecnologias Utilizadas](#tecnologias-utilizadas)
  - [Motivações](#motivações)
  - [Arquitetura](#arquitetura)
    - [Camadas](#camadas)
    - [Componentes Principais](#componentes-principais)
  - [Diagrama de Arquitetura](#diagrama-de-arquitetura)
  - [Fluxo de Funcionamento](#fluxo-de-funcionamento)
  - [Como Executar a API](#como-executar-a-api)
    - [Pré-requisitos](#pré-requisitos)
    - [Passos para Execução](#passos-para-execução)
    - [Executando os Testes](#executando-os-testes)
  - [Configurações](#configurações)
    - [Logging](#logging)
    - [Cache](#cache)
  - [Contratos da API](#contratos-da-api)
    - [Perfis (Profiles)](#perfis-profiles)
      - [GET /api/profiles](#get-apiprofiles)
      - [GET /api/profiles/{name}](#get-apiprofilesname)
      - [POST /api/profiles](#post-apiprofiles)
      - [PUT /api/profiles/{name}](#put-apiprofilesname)
      - [DELETE /api/profiles/{name}](#delete-apiprofilesname)
    - [Validação de Permissões](#validação-de-permissões)
      - [POST /api/profiles/{name}/validate](#post-apiprofilesnamevalidate)
    - [Cache](#cache-1)
      - [POST /api/cache/refresh](#post-apicacherefresh)
      - [DELETE /api/cache](#delete-apicache)
  - [Considerações Finais](#considerações-finais)
    - [Mudanças em Relação ao Contrato Original](#mudanças-em-relação-ao-contrato-original)
    - [Alteração do Contrato de Dados](#alteração-do-contrato-de-dados)
    - [Possíveis Inconsistências com Background Service](#possíveis-inconsistências-com-background-service)
    - [Internacionalização dos Logs](#internacionalização-dos-logs)

## Tecnologias Utilizadas

- **.NET 9**: Framework de desenvolvimento moderno e de alto desempenho
- **ASP.NET Core**: Framework para construção de APIs RESTful
- **Serilog**: Biblioteca para logging estruturado
- **Memory Cache**: Mecanismo de cache em memória para melhorar a performance
- **Swagger**: Documentação interativa da API
- **xUnit**: Framework para testes unitários
- **Moq**: Biblioteca para criação de mocks em testes

## Motivações

- **Arquitetura em Camadas**: Separação clara de responsabilidades para facilitar manutenção e evolução
- **Cache em Memória**: Redução de consultas ao repositório para melhorar performance
- **Background Service**: Atualização periódica de dados para garantir consistência
- **Testes Unitários**: Garantia de qualidade e cobertura de código
- **Logging Estruturado**: Facilidade para monitoramento e diagnóstico de problemas

## Arquitetura

A aplicação segue uma arquitetura em camadas com os seguintes componentes:

### Camadas

1. **ValidProfiles.API**: 
   - Controllers e configuração da aplicação
   - Middleware para tratamento global de exceções
   - Configuração do Swagger

2. **ValidProfiles.Application**: 
   - Serviços com regras de negócio
   - DTOs para transferência de dados
   - Interfaces de serviços

3. **ValidProfiles.Domain**: 
   - Entidades de domínio
   - Interfaces de repositório
   - Exceções personalizadas
   - Constantes e mensagens de erro

4. **ValidProfiles.Infrastructure**: 
   - Implementação de repositórios
   - Mecanismo de cache
   - Serviços de background para processamento assíncrono
   - Configuração de injeção de dependências

5. **ValidProfiles.Tests**:
   - Testes unitários para todas as camadas
   - Mocks para isolamento de componentes

### Componentes Principais

- **ProfileController**: Endpoints da API
- **ProfileService**: Regras de negócio para gerenciamento de perfis
- **ProfileCacheService**: Camada de cache para otimização de performance
- **ProfileRepository**: Acesso aos dados
- **ProfileUpdateBackgroundService**: Atualização periódica de perfis

## Diagrama de Arquitetura

```
┌────────────────┐     ┌────────────────┐     ┌────────────────┐
│                │     │                │     │                │
│  API (Controllers) ◄─┼─► Application  ◄─────┼─► Domain       │
│                │     │  (Services)    │     │  (Entities)    │
└────────┬───────┘     └────────┬───────┘     └────────────────┘
         │                      │                       ▲
         │                      │                       │
         │                      ▼                       │
         │             ┌────────────────┐               │
         │             │                │               │
         └────────────►│ Infrastructure ├───────────────┘
                       │                │
                       └────────────────┘
                              │
                              ▼
                       ┌────────────────┐
                       │   Background   │
                       │   Services     │
                       └────────────────┘
```

## Fluxo de Funcionamento

1. O cliente faz uma requisição HTTP para a API
2. O controller recebe a requisição e chama o serviço apropriado
3. O serviço consulta primeiro o cache para verificar se os dados estão disponíveis
4. Se não estiverem no cache, o serviço consulta o repositório
5. Os dados retornados são armazenados no cache para futuras consultas
6. Em paralelo, um serviço de background atualiza periodicamente os dados do cache

## Como Executar a API

### Pré-requisitos

- .NET 9 SDK instalado
- Visual Studio 2022 ou VS Code (opcional)

### Passos para Execução

1. Clone o repositório:
   ```
   git clone https://github.com/seu-usuario/ValidProfiles.git
   ```

2. Navegue até a pasta do projeto:
   ```
   cd ValidProfiles/src
   ```

3. Restaure os pacotes:
   ```
   dotnet restore
   ```

4. Execute a aplicação:
   ```
   cd ValidProfiles.API
   dotnet run
   ```

5. Acesse a documentação Swagger:
   ```
   http://localhost:5000/swagger
   ```

### Executando os Testes

```
dotnet test
```

## Configurações

A aplicação pode ser configurada através do arquivo `appsettings.json` com as seguintes opções:

### Logging

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "WriteTo": [
    {
      "Name": "Console"
    },
    {
      "Name": "File",
      "Args": {
        "path": "logs/log-.txt",
        "rollingInterval": "Day",
        "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}"
      }
    }
  ]
}
```

### Cache

O serviço de cache é configurado no `Program.cs` com um intervalo padrão de 5 minutos para atualização:

```csharp
builder.Services.AddHostedService<ProfileUpdateBackgroundService>(sp => 
    new ProfileUpdateBackgroundService(
        sp.GetRequiredService<ILogger<ProfileUpdateBackgroundService>>(),
        sp,
        TimeSpan.FromMinutes(5)
    )
);
```

## Contratos da API

A API ValidProfiles possui os seguintes endpoints principais:

### Perfis (Profiles)

#### GET /api/profiles

Retorna todos os perfis disponíveis.

**Resposta (200 OK):**
```json
{
  "profiles": [
    {
      "name": "string",
      "parameters": {
        "permissionKey1": true,
        "permissionKey2": false
      }
    }
  ]
}
```

#### GET /api/profiles/{name}

Retorna um perfil específico pelo nome.

**Parâmetros:**
- `name` (string, obrigatório): Nome do perfil

**Resposta (200 OK):**
```json
{
  "name": "string",
  "parameters": {
    "permissionKey1": true,
    "permissionKey2": false
  }
}
```

**Validações:**
- O nome do perfil não pode ser vazio
- O perfil deve existir no sistema

**Possíveis erros:**
- 400 Bad Request: Nome do perfil inválido
- 404 Not Found: Perfil não encontrado

#### POST /api/profiles

Cria um novo perfil.

**Corpo da requisição:**
```json
{
  "name": "string",
  "parameters": {
    "permissionKey1": true,
    "permissionKey2": false
  }
}
```

**Validações:**
- O nome do perfil não pode ser vazio
- O nome do perfil deve ser único
- A lista de parâmetros não pode ser vazia

**Resposta (201 Created):**
```json
{
  "name": "string",
  "parameters": {
    "permissionKey1": true,
    "permissionKey2": false
  }
}
```

**Possíveis erros:**
- 400 Bad Request: Dados inválidos
- 409 Conflict: Perfil já existe

#### PUT /api/profiles/{name}

Atualiza um perfil existente.

**Parâmetros:**
- `name` (string, obrigatório): Nome do perfil

**Corpo da requisição:**
```json
{
  "parameters": {
    "permissionKey1": true,
    "permissionKey2": false
  }
}
```

**Validações:**
- O nome do perfil não pode ser vazio
- O perfil deve existir no sistema
- A lista de parâmetros não pode ser vazia

**Resposta (200 OK):**
```json
{
  "name": "string",
  "parameters": {
    "permissionKey1": true,
    "permissionKey2": false
  }
}
```

**Possíveis erros:**
- 400 Bad Request: Dados inválidos
- 404 Not Found: Perfil não encontrado

#### DELETE /api/profiles/{name}

Remove um perfil existente.

**Parâmetros:**
- `name` (string, obrigatório): Nome do perfil

**Validações:**
- O nome do perfil não pode ser vazio
- O perfil deve existir no sistema

**Resposta (204 No Content)**

**Possíveis erros:**
- 400 Bad Request: Nome do perfil inválido
- 404 Not Found: Perfil não encontrado

### Validação de Permissões

#### POST /api/profiles/{name}/validate

Valida permissões para um perfil específico.

**Parâmetros:**
- `name` (string, obrigatório): Nome do perfil

**Corpo da requisição:**
```json
{
  "actions": [
    "CanEdit",
    "CanDelete",
    "CanView"
  ]
}
```

**Validações:**
- O nome do perfil não pode ser vazio
- O perfil deve existir no sistema
- A lista de ações não pode ser vazia

**Resposta (200 OK):**
```json
{
  "profileName": "string",
  "results": {
    "CanEdit": "Allowed",
    "CanDelete": "Denied",
    "CanView": "Undefined"
  }
}
```

**Possíveis valores para resultados:**
- `"Allowed"`: Permissão concedida
- `"Denied"`: Permissão negada
- `"Undefined"`: Permissão não definida para o perfil

**Possíveis erros:**
- 400 Bad Request: Dados inválidos ou lista de ações vazia
- 404 Not Found: Perfil não encontrado

### Cache

#### POST /api/cache/refresh

Força uma atualização completa do cache de perfis.

**Resposta (204 No Content)**

#### DELETE /api/cache

Limpa o cache de perfis.

**Resposta (204 No Content)**

## Considerações Finais

### Mudanças em Relação ao Contrato Original

No exercício original, foi solicitado que a rota de validação fosse implementada como GET. No entanto, optei por utilizar POST seguindo a semântica do RESTful:

- **GET**: Deve ser usado para operações idempotentes que apenas recuperam dados
- **POST**: Mais apropriado para operações que enviam dados e realizam validações

A validação de permissões envia uma lista de ações para serem verificadas, o que semanticamente é mais adequado como POST.

### Alteração do Contrato de Dados

O contrato original usava uma estrutura `<string, string>` para representar as permissões. Alterei para `<string, bool>` pelos seguintes motivos:

1. **Consistência de Tipo**: Booleanos representam claramente estados de permissão (permitido/negado)
2. **Eficiência**: Eliminação da necessidade de conversão entre strings e valores lógicos
3. **Facilidade de Implementação**: Simplificação da lógica de validação de permissões

### Possíveis Inconsistências com Background Service

O serviço de background realiza atualizações periódicas dos perfis, o que poderia causar inconsistências momentâneas nos dados:

- Uma validação solicitada imediatamente após uma alteração de perfil poderia usar dados desatualizados do cache
- Implementei o serviço para atualizar todo o cache a cada 5 minutos, minimizando o período de inconsistência
- Para casos críticos onde a consistência imediata é necessária, o sistema poderia ser estendido para invalidar o cache específico após alterações

### Internacionalização dos Logs

Durante a refatoração da aplicação, a maioria das mensagens foi convertida de português para inglês. No entanto, algumas mensagens de log permanecem em português, especialmente no repositório:

```
[2025-02-27 04:05:32.773 -03:00 DBG] Obtendo todos os perfis
```

Em uma próxima atualização, seria interessante completar a internacionalização para garantir consistência em todas as mensagens de log do sistema.

Estas considerações são importantes para entender os trade-offs entre performance e consistência em sistemas distribuídos. 