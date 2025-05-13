# Odontoprev System

## Visão Geral

O Odontoprev System é uma solução completa para gestão de clínicas odontológicas, composta por uma API principal para o gerenciamento de pacientes, dentistas, consultas e histórico médico, além de um microsserviço dedicado para autenticação e autorização. O sistema utiliza uma arquitetura moderna baseada em microsserviços, com comunicação assíncrona via RabbitMQ e múltiplas tecnologias de armazenamento de dados.

![Arquitetura do Sistema](https://via.placeholder.com/800x400?text=Arquitetura+do+Sistema+Odontoprev)

## Estrutura do Projeto

O projeto está organizado na seguinte estrutura:

```
OdontoprevSystem/
├── src/
│   ├── Challenge_Odontoprev_API/     # API principal para gestão odontológica
│   │   ├── Auth/                     # Middleware de autenticação JWT
│   │   ├── Controllers/              # Controladores da API
│   │   ├── DTOs/                     # Objetos de transferência de dados
│   │   ├── Infrastructure/           # Configuração de banco de dados e UoW
│   │   ├── Mappings/                 # Configurações do AutoMapper
│   │   ├── Models/                   # Entidades de domínio
│   │   ├── Repositories/             # Padrão repositório para acesso a dados
│   │   └── Services/                 # Serviços de negócio
│   │
│   └── Auth.API/                     # Microsserviço de autenticação
│       ├── Controllers/              # Controladores de autenticação
│       ├── Data/                     # Acesso ao MongoDB
│       ├── DTOs/                     # Objetos de transferência de dados 
│       ├── Models/                   # Modelo de usuário
│       └── Services/                 # Serviços de autenticação e JWT
│
└── docker/                           # Configuração Docker
    └── docker-compose.yml            # Orquestração de contêineres
```

## Tecnologias Utilizadas

### API Principal (Challenge_Odontoprev_API)
- **ASP.NET Core 8.0**: Framework moderno para desenvolvimento de APIs
- **Entity Framework Core**: ORM para acesso ao banco de dados Oracle
- **AutoMapper**: Mapeamento entre entidades e DTOs
- **Swagger/OpenAPI**: Documentação de API interativa
- **Oracle Database**: Armazenamento relacional principal
- **JWT Authentication**: Proteção de endpoints

### Microsserviço de Autenticação (Auth.API)
- **ASP.NET Core 8.0**: Framework moderno para desenvolvimento de APIs
- **MongoDB Driver**: Acesso ao banco de dados MongoDB
- **JWT**: Geração e validação de tokens para autenticação
- **RabbitMQ.Client**: Cliente para comunicação assíncrona

### Infraestrutura
- **Docker & Docker Compose**: Containerização e orquestração
- **RabbitMQ**: Comunicação assíncrona entre microsserviços
- **MongoDB**: Armazenamento de dados de usuários
- **Oracle Database**: Armazenamento de dados odontológicos

## Instalação e Execução

### Pré-requisitos
- Docker e Docker Compose
- .NET SDK 8.0 (para desenvolvimento)

### Executando com Docker

1. Clone o repositório:
   ```bash
   git clone https://github.com/seu-usuario/OdontoprevSolutionNET.git
   cd OdontoprevSolutionNET
   ```

2. Inicie os contêineres:
   ```bash
   cd docker
   docker-compose up -d
   ```

3. Acesse as interfaces:
   - API Principal: http://localhost:5000/swagger
   - Auth API: http://localhost:5001/swagger
   - RabbitMQ Management: http://localhost:15672 (guest/guest)

### Desenvolvimento Local

1. Configure as conexões com banco de dados:
   - Oracle (API Principal)
   - MongoDB (Auth API)

2. Execute os projetos:
   ```bash
   cd src/Auth.API
   dotnet run

   cd src/Challenge_Odontoprev_API
   dotnet run
   ```

## Autenticação e Autorização

O sistema utiliza autenticação baseada em JWT (JSON Web Tokens) para proteger seus endpoints. O fluxo de autenticação funciona da seguinte forma:

1. **Registro de Usuário**:
   - POST `/api/auth/register` (Auth.API)
   - Cria um novo usuário no MongoDB
   - Publica evento no RabbitMQ

2. **Login**:
   - POST `/api/auth/login` (Auth.API)
   - Verifica credenciais e gera token JWT
   - Publica evento no RabbitMQ

3. **Acesso Protegido**:
   - Inclua o token JWT no cabeçalho `Authorization` como `Bearer {token}`
   - O middleware JWT valida o token antes de permitir acesso aos endpoints protegidos

## API Principal - Endpoints

### Pacientes

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| GET | `/api/paciente` | Lista todos os pacientes | Requerida |
| GET | `/api/paciente/{id}` | Obtém paciente por ID | Requerida |
| POST | `/api/paciente` | Cria novo paciente | Requerida |
| PUT | `/api/paciente/{id}` | Atualiza paciente existente | Requerida |
| DELETE | `/api/paciente/{id}` | Remove paciente | Requerida (Admin) |

### Dentistas

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| GET | `/api/dentista` | Lista todos os dentistas | Requerida |
| GET | `/api/dentista/{id}` | Obtém dentista por ID | Requerida |
| POST | `/api/dentista` | Cria novo dentista | Requerida |
| PUT | `/api/dentista/{id}` | Atualiza dentista existente | Requerida |
| DELETE | `/api/dentista/{id}` | Remove dentista | Requerida (Admin) |

### Consultas

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| GET | `/api/consulta` | Lista todas as consultas | Requerida |
| GET | `/api/consulta/{id}` | Obtém consulta por ID | Requerida |
| POST | `/api/consulta` | Agenda nova consulta | Requerida |
| PUT | `/api/consulta/{id}` | Atualiza consulta existente | Requerida |
| DELETE | `/api/consulta/{id}` | Cancela consulta | Requerida |

### Histórico

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| GET | `/api/historico` | Lista todo o histórico | Requerida |
| GET | `/api/historico/{id}` | Obtém registro histórico por ID | Requerida |
| POST | `/api/historico` | Cria novo registro histórico | Requerida |
| PUT | `/api/historico/{id}` | Atualiza registro histórico | Requerida |
| DELETE | `/api/historico/{id}` | Remove registro histórico | Requerida (Admin) |

## Auth API - Endpoints

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| POST | `/api/auth/register` | Registra novo usuário | Não requerida |
| POST | `/api/auth/login` | Autentica usuário e retorna token JWT | Não requerida |

## Modelos de Dados

### Paciente
```json
{
  "id": 1,
  "nome": "Nome do Paciente",
  "data_Nascimento": "1990-01-01T00:00:00",
  "cpf": "123.456.789-00",
  "endereco": "Endereço completo",
  "telefone": "(11) 99999-9999",
  "carteirinha": 123456789
}
```

### Dentista
```json
{
  "id": 1,
  "nome": "Nome do Dentista",
  "cro": "CRO-SP 12345",
  "especialidade": "Ortodontia",
  "telefone": "(11) 98888-8888"
}
```

### Consulta
```json
{
  "id": 1,
  "data_Consulta": "2025-05-15T14:30:00",
  "id_Paciente": 1,
  "id_Dentista": 1,
  "status": "Agendada"
}
```

### Histórico de Consulta
```json
{
  "id": 1,
  "id_Consulta": 1,
  "data_Atendimento": "2025-05-15T14:30:00",
  "motivo_Consulta": "Revisão ortodôntica",
  "observacoes": "Paciente relatou desconforto no aparelho"
}
```

### Usuário (Auth)
```json
{
  "username": "usuario_teste",
  "email": "usuario@exemplo.com",
  "password": "Senha@123",
  "role": "Admin"
}
```

## Comunicação entre Serviços

O sistema utiliza RabbitMQ para comunicação assíncrona entre os microsserviços, com os seguintes eventos:

1. **UserCreated** - Enviado quando um novo usuário é registrado:
   ```json
   {
     "id": "user-id-guid",
     "username": "nome_usuario",
     "email": "email@exemplo.com",
     "role": "User",
     "createdAt": "2025-05-10T10:15:30Z"
   }
   ```

2. **UserLoggedIn** - Enviado quando um usuário faz login:
   ```json
   {
     "id": "user-id-guid",
     "username": "nome_usuario",
     "loggedInAt": "2025-05-10T14:25:10Z"
   }
   ```

## Segurança

O sistema implementa várias camadas de segurança:

1. **Autenticação JWT**: Tokens com tempo de expiração e assinatura criptográfica
2. **Autorização baseada em roles**: Controle de acesso granular por função de usuário
3. **Senhas com hash**: Armazenamento seguro com salt único por usuário
4. **HTTPS**: Comunicação criptografada (em produção)
5. **Validação de entrada**: Proteção contra injeção e ataques comuns

## Escalabilidade

A arquitetura de microsserviços permite escalar componentes de forma independente:

- **API Principal**: Pode ser escalada horizontalmente conforme a demanda de operações odontológicas
- **Auth API**: Pode ser escalada para lidar com picos de autenticação
- **RabbitMQ**: Fornece buffer para comunicações assíncronas, permitindo processamento resiliente
- **Bancos de dados**: MongoDB pode ser configurado em cluster; Oracle pode ser configurado com réplicas

## Monitoramento

Para monitoramento em produção, recomenda-se:

- **Logs**: Implementados no código com diferentes níveis de severidade
- **Métricas**: Podem ser integradas usando Prometheus e Grafana
- **Tracing**: Pode ser implementado com OpenTelemetry

## Testes

Para executar os testes (quando implementados):

```bash
cd tests
dotnet test
```

## Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/nova-funcionalidade`)
3. Faça commit das alterações (`git commit -m 'Adiciona nova funcionalidade'`)
4. Faça push para a branch (`git push origin feature/nova-funcionalidade`)
5. Abra um Pull Request

## Licença

Este projeto está licenciado sob a licença MIT - veja o arquivo LICENSE.md para detalhes.

## Contato

Para dúvidas ou sugestões, entre em contato com a equipe de desenvolvimento:

- **Vitor Onofre Ramos | RM553241**
- **Pedro Henrique | RM553801**
- **Beatriz Silva | RM552600**
