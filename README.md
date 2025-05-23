# 🦷 Challenge Odontoprev Solution

Uma solução completa para gerenciamento de clínicas odontológicas com **análise de sentimentos integrada** usando Machine Learning. O sistema é composto por múltiplas APIs com autenticação JWT, banco de dados Oracle e MongoDB, além de testes automatizados para validação da IA.

## 🏛️ Arquitetura da Solução

### 📦 Componentes Principais

```
OdontoprevSolution/
├── 🔐 Auth.API/                     # Autenticação JWT + MongoDB
├── 🦷 Challenge_Odontoprev_API/     # API Principal + Oracle DB
├── 🧪 Challenge_Odontoprev_API.Tests/ # Testes de Machine Learning
└── 📚 Database-Scripts/             # Scripts Oracle (PL/SQL)
```

### 🔗 Fluxo de Arquitetura

```mermaid
graph TD
    A[Frontend] --> B[Auth.API - JWT]
    B --> C[MongoDB - Usuários]
    A --> D[Odontoprev API]
    D --> E[Oracle DB - Dados Clínicos]
    D --> F[ML.NET - Análise Sentimentos]
    G[Tests xUnit] --> F
```

## 🚀 Funcionalidades por Componente

### 🔐 **Auth.API - Sistema de Autenticação**

#### **Características:**
- **JWT Bearer Authentication** com refresh tokens
- **MongoDB** para armazenamento de usuários
- **Hash de senhas** com salt personalizado
- **Roles** de usuário (Admin, User)
- **Controle de sessão** e last login

#### **Modelo de Dados (MongoDB):**
```json
{
  "_id": "ObjectId",
  "Username": "João Silva",
  "Email": "joao.silva@example.com", 
  "CPF": "123.456.789-00",
  "PasswordHash": "hash_criptografado",
  "PasswordSalt": "salt_personalizado",
  "Phone": "(11) 98765-4321",
  "Role": "Admin", // Admin | User
  "CreatedAt": "2025-01-15T10:30:00Z",
  "LastLogin": "2025-01-15T14:20:00Z",
  "Active": true
}
```

#### **Endpoints de Autenticação:**
```http
POST /api/auth/register     # Registrar usuário
POST /api/auth/login        # Login e geração do JWT
POST /api/auth/refresh      # Renovar token
GET  /api/auth/profile      # Perfil do usuário
PUT  /api/auth/profile      # Atualizar perfil
```

### **Para saber como utilizar os Tokens gerados pela API, clique aqui -> [https://youtu.be/vfD20FaQ2Gk](https://youtu.be/vfD20FaQ2Gk)**

### 🦷 **Challenge_Odontoprev_API - API Principal**

#### **Características:**
- **Oracle Database** com procedures PL/SQL
- **Entity Framework Core** para ORM
- **AutoMapper** para DTOs
- **Machine Learning** com ML.NET
- **Análise de Sentimentos** das observações médicas
- **Proteção JWT** em todos os endpoints

#### **Funcionalidades Principais:**
- ✅ **Gestão de Pacientes** - CRUD completo
- ✅ **Gestão de Dentistas** - Cadastro e especialidades  
- ✅ **Gestão de Consultas** - Agendamento e controle
- ✅ **Histórico Médico** - Registro detalhado
- 🤖 **Análise de Sentimentos** - IA para satisfação do paciente

#### **Endpoints Principais:**
```http
# Pacientes
GET    /api/paciente          # Listar todos
POST   /api/paciente          # Criar novo
GET    /api/paciente/{id}     # Buscar por ID
PUT    /api/paciente/{id}     # Atualizar
DELETE /api/paciente/{id}     # Excluir

# Dentistas  
GET    /api/dentista          # Listar todos
POST   /api/dentista          # Criar novo
GET    /api/dentista/{id}     # Buscar por ID
PUT    /api/dentista/{id}     # Atualizar
DELETE /api/dentista/{id}     # Excluir

# Consultas
GET    /api/consulta          # Listar todas
POST   /api/consulta          # Criar nova
GET    /api/consulta/{id}     # Buscar por ID
PUT    /api/consulta/{id}     # Atualizar
DELETE /api/consulta/{id}     # Excluir

# Histórico
GET    /api/historico         # Listar todos
POST   /api/historico         # Criar novo
GET    /api/historico/{id}    # Buscar por ID
PUT    /api/historico/{id}    # Atualizar
DELETE /api/historico/{id}    # Excluir

# 🤖 Análise de Sentimentos (IA)
GET /api/historicoanalysis/paciente/{id}           # Análise por paciente
GET /api/historicoanalysis/consulta/{id}           # Análise por consulta  
GET /api/historicoanalysis/estatisticas/paciente/{id} # Estatísticas de sentimento
```

### 🧪 **Challenge_Odontoprev_API.Tests - Testes de IA**

#### **Características:**
- **xUnit Framework** para testes unitários
- **Moq** para mock objects
- **Testes específicos** para Machine Learning
- **Validação da acurácia** do modelo de IA
- **Testes de integração** end-to-end

#### **Cobertura de Testes:**
- ✅ Classificação básica (positivo/negativo)
- ✅ Casos específicos com contexto de "dor"
- ✅ Validação de entrada e robustez
- ✅ Consistência e confiança dos resultados
- ✅ Performance e edge cases

## 🤖 Machine Learning - Análise de Sentimentos

### **Como Funciona:**
- **Algoritmo**: FastTree Binary Classification (ML.NET)
- **Dataset**: 80 exemplos específicos do contexto odontológico
- **Treinamento**: Automático na inicialização da API
- **Calibração**: Platt Calibration para probabilidades precisas

### **Exemplos de Classificação:**
```csharp
// ✅ Positivos
"Paciente muito satisfeito com o resultado"    → Positivo (95% confiança)
"Procedimento realizado com sucesso"           → Positivo (92% confiança) 
"Paciente não sente mais dor"                  → Positivo (88% confiança)

// ❌ Negativos
"O paciente está com dor intensa"              → Negativo (97% confiança)
"Complicações graves durante procedimento"     → Negativo (94% confiança)
"Tratamento não funcionou"                     → Negativo (91% confiança)
```

### **Resposta da API:**
```json
{
  "historicoId": 123,
  "consultaId": 456,
  "dataAtendimento": "2024-01-15T10:30:00",
  "motivoConsulta": "Limpeza dental",
  "observacoes": "Paciente muito satisfeito com o resultado",
  "sentimentResult": {
    "text": "Paciente muito satisfeito com o resultado",
    "isPositive": true,
    "confidence": 0.95,
    "sentimentCategory": "Positivo"
  }
}
```

## 🛠️ Tecnologias Utilizadas

### **Backend Principal**
- **.NET 8** - Framework base
- **ASP.NET Core** - APIs REST
- **Entity Framework Core** - ORM para Oracle
- **ML.NET** - Machine Learning
- **AutoMapper** - Mapeamento de objetos

### **Autenticação**
- **MongoDB** - Banco de usuários
- **JWT Bearer** - Tokens de autenticação
- **BCrypt** - Hash de senhas
- **Role-based** - Controle de acesso

### **Banco de Dados**
- **Oracle Database** - Dados clínicos
- **PL/SQL Procedures** - Operações CRUD
- **MongoDB** - Dados de autenticação

### **Testes & DevOps**
- **xUnit** - Framework de testes
- **Moq** - Mock objects
- **Swagger/OpenAPI** - Documentação
- **Docker** - Containerização

## 🔧 Configuração e Instalação

### **Pré-requisitos**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Oracle Database 19c+](https://www.oracle.com/database/)
- [MongoDB 6.0+](https://www.mongodb.com/try/download/community)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/)

### **1. Clonar o Repositório**
```bash
git clone https://github.com/VitorOnofreRamos/OdontoprevSolution.git
cd OdontoprevSolution
```

### **2. Configurar Banco de Dados Oracle**

#### **Executar Scripts na Ordem:**
```bash
# ⚠️ IMPORTANTE: Execute na ordem exata!
1. PkgFunAuxiliares.sql              # Funções auxiliares
2. PkgFunValidacaoOdontoprev.sql     # Funções de validação  
3. PkgProceduresCRUDOdontoprev.sql   # Procedures CRUD
```

### **3. Configurar MongoDB**
```bash
# Instalar MongoDB Community
# Criar database: OdontoprevAuth
# Criar collection: Users
```

### **4. Configurar appsettings.json**

#### **Auth.API/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017/OdontoprevAuth"
  },
  "JwtSettings": {
    "Secret": "sua_chave_secreta_jwt_com_pelo_menos_32_caracteres",
    "Issuer": "Auth.API",
    "Audience": "OdontoprevClients",
    "ExpirationInMinutes": 60
  }
}
```

#### **Challenge_Odontoprev_API/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "OracleConnection": "Data Source=localhost:1521/XE;User Id=seu_usuario;Password=sua_senha;"
  },
  "JwtSettings": {
    "Secret": "mesma_chave_secreta_do_auth_api",
    "Issuer": "Auth.API", 
    "Audience": "OdontoprevClients"
  }
}
```

### **5. Restaurar Dependências**
```bash
# Restaurar todos os projetos
dotnet restore

# Ou individualmente
cd Auth.API && dotnet restore
cd ../Challenge_Odontoprev_API && dotnet restore  
cd ../Challenge_Odontoprev_API.Tests && dotnet restore
```

### **6. Executar as Aplicações**

#### **Terminal 1 - Auth.API:**
```bash
cd Auth.API
dotnet run
# Disponível em: https://localhost:5001
```

#### **Terminal 2 - API Principal:**
```bash
cd Challenge_Odontoprev_API
dotnet run  
# Disponível em: https://localhost:5062
```

## 🔐 Fluxo de Autenticação

### **1. Registrar Usuário**
```http
POST https://localhost:5001/api/auth/register
Content-Type: application/json

{
  "username": "João Silva",
  "email": "joao.silva@example.com",
  "cpf": "123.456.789-00", 
  "password": "MinhaSenh@123",
  "phone": "(11) 98765-4321",
  "role": "Admin"
}
```

### **2. Fazer Login**
```http
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "joao.silva@example.com",
  "password": "MinhaSenh@123"
}
```

### **3. Usar Token na API Principal**
```http
GET https://localhost:5062/api/paciente
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 🧪 Executar Testes

### **Todos os Testes**
```bash
cd Challenge_Odontoprev_API.Tests
dotnet test
```

### **Testes Específicos da IA**
```bash
# Testes simples de classificação
dotnet test --filter "SimpleSentimentTests"

# Testes específicos de casos com dor
dotnet test --filter "Should_Classify_Pain_Comments_As_Negative"

# Testes com saída detalhada
dotnet test --logger "console;verbosity=detailed"
```

### **Resultados Esperados**
```
✅ Should_Classify_Positive_Comment
✅ Should_Classify_Negative_Comment  
✅ Should_Classify_Pain_Comments_As_Negative
✅ Should_Classify_Pain_Relief_As_Positive
✅ Should_Throw_Exception_For_Empty_Text
✅ Should_Return_Consistent_Results
```

## 📊 Monitoramento e Saúde

### **Health Checks**
```http
# Verificar saúde da IA
GET https://localhost:5062/health/ml

# Verificar autenticação  
GET https://localhost:5001/health
```

## 📡 Documentação da API

### **Swagger Endpoints:**
- **Auth.API**: `https://localhost:5001/swagger`
- **API Principal**: `https://localhost:5062/swagger`

## 🐳 Docker (Opcional)

### **docker-compose.yml**
```yaml
version: '3.8'
services:
  mongodb:
    image: mongo:6.0
    ports:
      - "27017:27017"
    
  auth-api:
    build: ./Auth.API
    ports:
      - "5001:5001"
    depends_on:
      - mongodb
      
  odontoprev-api:
    build: ./Challenge_Odontoprev_API  
    ports:
      - "5062:5062"
    depends_on:
      - auth-api
```

```bash
docker-compose up -d
```

## 🚀 Deploy em Produção

### **Azure App Service**
1. **Auth.API**: Deploy com MongoDB Atlas
2. **API Principal**: Deploy com Oracle Cloud
3. **Configurar CORS** para frontend
4. **SSL Certificates** obrigatórios

### **Variáveis de Ambiente**
```bash
# Auth.API
MONGODB_CONNECTION_STRING=mongodb+srv://...
JWT_SECRET=sua_chave_producao

# API Principal  
ORACLE_CONNECTION_STRING=Data Source=...
JWT_SECRET=mesma_chave_auth_api
```

## 📈 Roadmap

### **Próximas Funcionalidades**
- [ ] **Dashboard Analytics** com métricas de IA
- [ ] **Notificações Push** para agendamentos
- [ ] **Relatórios PDF** automáticos
- [ ] **Mobile App** Flutter/React Native
- [ ] **Análise de Imagens** com Computer Vision
- [ ] **Chatbot** integrado com IA

### **Melhorias Técnicas**
- [ ] **Redis Cache** para tokens JWT
- [ ] **Message Queue** com RabbitMQ
- [ ] **Microservices** com Docker Swarm
- [ ] **GraphQL** endpoint
- [ ] **Real-time** com SignalR

## 👥 Integrantes

**Turma 2TDSPS - FIAP**

| Nome | RM | GitHub |
|------|-------|---------|
| Vitor Onofre Ramos | RM553241 | [@VitorOnofreRamos](https://github.com/VitorOnofreRamos) |
| Pedro Henrique | RM553801 | [@PedroHenrique](https://github.com/pedrohenrique) |
| Beatriz Silva | RM552600 | [@BeatrizSilva](https://github.com/beatrizsilva) |

## 🐛 Problemas Conhecidos

- **Modelo de IA**: Alguns edge cases podem ter baixa confiança (~50-60%)
- **Performance**: Treinamento inicial da IA leva ~10-15 segundos
- **Memória**: Modelo ML.NET carregado usa ~50-80MB RAM
- **JWT**: Tokens não são invalidados no logout (usar blacklist para produção)

## 📞 Suporte

- **Email**: suporte@odontoprev.com.br
- **Issues**: [GitHub Issues](https://github.com/VitorOnofreRamos/OdontoprevSolution/issues)
- **Wiki**: [Documentação Técnica](https://github.com/VitorOnofreRamos/OdontoprevSolution/wiki)

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

<div align="center">

**Desenvolvido com ❤️ para revolucionar o atendimento odontológico com Inteligência Artificial**

![.NET](https://img.shields.io/badge/.NET-8.0-blue?style=for-the-badge)
![ML.NET](https://img.shields.io/badge/ML.NET-AI-green?style=for-the-badge)
![Oracle](https://img.shields.io/badge/Oracle-Database-red?style=for-the-badge)
![MongoDB](https://img.shields.io/badge/MongoDB-Auth-darkgreen?style=for-the-badge)
![JWT](https://img.shields.io/badge/JWT-Security-orange?style=for-the-badge)
![Tests](https://img.shields.io/badge/Tests-xUnit-yellow?style=for-the-badge)

</div>