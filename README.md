# ValidProfiles API

API para gerenciamento de perfis de usuários com validação de permissões.

## Tecnologias

- **.NET 9** com **ASP.NET Core**
- **Serilog** para logging estruturado
- **Swagger** para documentação interativa
- **xUnit** e **Moq** para testes unitários

## Arquitetura

A aplicação utiliza uma arquitetura em camadas:

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

### Camadas:

1. **ValidProfiles.API**: Controllers e configuração da aplicação
2. **ValidProfiles.Application**: Serviços com regras de negócio e DTOs
3. **ValidProfiles.Domain**: Entidades, interfaces de repositório e constantes
4. **ValidProfiles.Infrastructure**: Repositórios e serviços de background
5. **ValidProfiles.Tests**: Testes unitários

## Como Executar

### Pré-requisitos

- .NET 9 SDK

### Execução

```bash
# Clonar o repositório
git clone https://github.com/Celourence/ValidProfileAPI

# Navegar até a pasta do projeto
cd ValidProfiles/src

# Restaurar pacotes
dotnet restore

# Executar a aplicação
cd ValidProfiles.API
dotnet run

# Acessar Swagger
http://localhost:5000/swagger

# Executar testes
cd ..
dotnet test
```

## Endpoints da API

### Perfis

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | /api/profiles | Retorna todos os perfis |
| GET | /api/profiles/{name} | Retorna um perfil específico |
| POST | /api/profiles | Cria um novo perfil |
| PUT | /api/profiles/{name} | Atualiza um perfil existente |
| DELETE | /api/profiles/{name} | Remove um perfil |

### Validação

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | /api/profiles/{name}/validate | Valida permissões para um perfil |

## Exemplos de Uso

### Criar Perfil

```http
POST /api/profiles
Content-Type: application/json

{
  "name": "Editor",
  "parameters": {
    "canEdit": true,
    "canDelete": false,
    "canView": true
  }
}
```

### Validar Permissões

```http
POST /api/profiles/Editor/validate
Content-Type: application/json

{
  "actions": ["canEdit", "canDelete", "canView"]
}
```

## Desafios e Considerações

1. **Semântica RESTful**: A validação foi implementada como POST em vez de GET para seguir melhores práticas de API.

2. **Contrato de Dados**: Parâmetros usam `<string, bool>` em vez de `<string, string>` para maior eficiência e clareza.

3. **Background Service**: Um serviço em segundo plano atualiza periodicamente os valores dos perfis com valores aleatórios, simulando atualizações externas.

4. **Internacionalização**: As mensagens de log e comentários do código foram padronizados em inglês para manter consistência.



5. **Testes Unitários**: Cobertura completa de testes para garantir qualidade do código e facilitar refatorações futuras. 