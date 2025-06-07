# GSAuth - Sistema de Doações com ML

## 📋 Sobre o Projeto

O **GSAuth** é uma plataforma de gerenciamento de doações que utiliza Machine Learning para otimizar o matching entre necessidades e doações. O sistema conecta doadores, organizações beneficentes e pessoas em necessidade através de um algoritmo inteligente de compatibilidade.

### 🎯 Funcionalidades Principais

- **Autenticação JWT**: Sistema seguro de login e registro
- **Gestão de Usuários**: Diferentes tipos de usuários (Doador, Membro de ONG, Admin)
- **Cadastro de Necessidades**: ONGs podem cadastrar necessidades específicas
- **Registro de Doações**: Doadores podem oferecer itens para doação
- **Matching Inteligente**: Algoritmo ML que calcula compatibilidade entre doações e necessidades
- **Sistema de Organizações**: Gestão de ONGs e organizações beneficentes

## 🏗️ Arquitetura

### Estrutura do Projeto

```
GSAuth/
├── Controllers/           # Controllers da API
│   ├── AuthController.cs         # Autenticação
│   ├── ModelsController/          # CRUD dos modelos
│   │   ├── DonationController.cs
│   │   ├── MatchController.cs
│   │   ├── NeedController.cs
│   │   └── OrganizationController.cs
│   └── TestController.cs          # Endpoints de teste
├── DTOs/                  # Data Transfer Objects
├── Infrastructure/        # Configuração do banco
├── ML/                   # Machine Learning
│   ├── Models/                   # Modelos ML
│   └── Services/                 # Serviços ML
├── Models/               # Entidades do domínio
├── Repositories/         # Camada de dados
├── Services/             # Lógica de negócio
└── Tests/                # Testes automatizados
```

### 🤖 Sistema de Machine Learning

O sistema utiliza **Microsoft ML.NET** para criar um modelo de compatibilidade que analisa:

- **Correspondência de Categoria**: Compatibilidade entre tipos de itens
- **Distância Geográfica**: Proximidade entre doador e beneficiário
- **Proporção de Quantidade**: Relação entre quantidade oferecida e necessária
- **Fator de Urgência**: Prioridade da necessidade
- **Fator Temporal**: Proximidade de prazos
- **Confiabilidade do Doador**: Histórico de doações
- **Credibilidade da Organização**: Reputação da ONG

## 🛠️ Tecnologias Utilizadas

### Backend
- **.NET 8**: Framework principal
- **ASP.NET Core**: Web API
- **Entity Framework Core**: ORM
- **Oracle Database**: Banco de dados
- **ML.NET**: Machine Learning
- **JWT**: Autenticação
- **AutoMapper**: Mapeamento de objetos

### Testes
- **xUnit**: Framework de testes
- **Moq**: Mock objects
- **FluentAssertions**: Assertions fluentes

### DevOps
- **Docker**: Containerização
- **Swagger**: Documentação da API

## 📦 Instalação e Configuração

### Pré-requisitos

- .NET 8 SDK
- Oracle Database
- Docker (opcional)

### 1. Clone o Repositório

```bash
git clone https://github.com/seu-usuario/GSAuth.git
cd GSAuth
```

### 2. Configuração do Banco de Dados

Edite o arquivo `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=seu-servidor:1521/orcl;User Id=seu-usuario;Password=sua-senha;"
  },
  "Jwt": {
    "Secret": "SuaChaveSecreta_MinimoDe32Caracteres",
    "ExpirationMinutes": 60
  }
}
```

### 3. Executar Migrações

```bash
dotnet ef database update
```

### 4. Executar a Aplicação

```bash
dotnet run
```

A API estará disponível em: `https://localhost:5000/swagger`

### 🐳 Usando Docker

```bash
# Build da imagem
docker build -t gsauth .

# Executar container
docker run -p 8080:8080 gsauth
```

## 🔑 Endpoints Principais

### Autenticação

```http
POST /api/auth/register    # Registro de usuário
POST /api/auth/login       # Login
GET /api/auth/me          # Perfil do usuário
POST /api/auth/change-password  # Alterar senha
DELETE /api/auth/delete-account # Deletar conta
```

### Necessidades

```http
GET /api/need             # Listar necessidades
POST /api/need            # Criar necessidade
PUT /api/need/{id}        # Atualizar necessidade
DELETE /api/need/{id}     # Deletar necessidade
```

### Doações

```http
GET /api/donation         # Listar doações
POST /api/donation        # Criar doação
PUT /api/donation/{id}    # Atualizar doação
DELETE /api/donation/{id} # Deletar doação
```

### Matches (ML)

```http
GET /api/match            # Listar matches
POST /api/match           # Criar match
POST /api/match/calculate-compatibility  # Calcular compatibilidade
POST /api/match/train-model             # Treinar modelo ML
GET /api/match/model-status             # Status do modelo
```

## 👥 Tipos de Usuário

### 1. **DONOR** (Doador)
- Pode criar doações
- Visualizar matches de suas doações
- Gerenciar perfil pessoal

### 2. **NGO_MEMBER** (Membro de ONG)
- Pode criar necessidades
- Gerenciar necessidades da organização
- Aceitar/rejeitar matches

### 3. **ADMIN** (Administrador)
- Acesso total ao sistema
- Gerenciar usuários e organizações
- Treinar modelos ML

## 🧪 Executando Testes

```bash
# Executar todos os testes
dotnet test

# Executar testes com relatório de cobertura
dotnet test --collect:"XPlat Code Coverage"

# Executar testes específicos
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=Performance"
```

### Categorias de Teste

- **Unit Tests**: Testes unitários dos serviços
- **Integration Tests**: Testes de integração do ML
- **Performance Tests**: Testes de performance do algoritmo

## 🤖 Machine Learning

### Treinamento do Modelo

O modelo pode ser treinado via API:

```http
POST /api/match/train-model
```

### Features Utilizadas

1. **CategoryMatch**: Compatibilidade de categorias (0-1)
2. **LocationDistance**: Distância normalizada (0-1)
3. **QuantityRatio**: Proporção de quantidades (0-1)
4. **UrgencyFactor**: Fator de urgência (0-1)
5. **TimeFactor**: Fator temporal (0-1)
6. **ExpirationFactor**: Fator de expiração (0-1)
7. **DonorReliability**: Confiabilidade do doador (0-1)
8. **OrganizationTrust**: Confiança na organização (0-1)

### Algoritmo

Utiliza **FastTree Regression** para prever scores de compatibilidade (0-100).

## 📊 Monitoramento e Logging

O sistema utiliza logging estruturado para:
- Autenticação de usuários
- Operações CRUD
- Treinamento de modelos ML
- Cálculos de compatibilidade

## 🔒 Segurança

### Autenticação JWT
- Tokens com expiração configurável
- Claims personalizadas para autorização
- Middleware de autenticação

### Autorização
- Role-based access control
- Proteção de endpoints sensíveis
- Validação de propriedade de recursos

### Proteção de Dados
- Hash de senhas com salt
- Validação de entrada
- Sanitização de dados

## 🚀 Deploy

### Variáveis de Ambiente

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=sua-string-conexao
Jwt__Secret=sua-chave-secreta
```

### Docker Compose (Exemplo)

```yaml
version: '3.8'
services:
  gsauth:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Data Source=oracle:1521/orcl;User Id=user;Password=pass;
    depends_on:
      - oracle

  oracle:
    image: container-registry.oracle.com/database/express:latest
    ports:
      - "1521:1521"
    environment:
      - ORACLE_PWD=yourpassword
```

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

### Convenções

- Use convenção de commits semânticos
- Mantenha testes com cobertura > 80%
- Documente APIs com Swagger
- Siga padrões de código C#

## 📈 Roadmap

- [ ] **v2.0**: Sistema de notificações em tempo real
- [ ] **v2.1**: API de geolocalização avançada
- [ ] **v2.2**: Dashboard analytics para ONGs
- [ ] **v2.3**: App mobile React Native
- [ ] **v2.4**: Integração com redes sociais
- [ ] **v2.5**: Sistema de avaliação e feedback

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 📞 Contato

**Equipe de Desenvolvimento**
- Email: contato@gsauth.com
- LinkedIn: [GSAuth Team](https://linkedin.com/company/gsauth)

## 🙏 Agradecimentos

- FIAP pela orientação acadêmica
- Comunidade .NET pelo suporte
- Colaboradores e testadores
- ONGs parceiras no desenvolvimento

---

**GSAuth** - Conectando corações através da tecnologia 💙