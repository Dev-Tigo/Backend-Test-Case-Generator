# Test Case Generator

Uma API backend em ASP.NET Core para gerar casos de teste automaticamente utilizando a API do Google Gemini. O projeto utiliza IA para criar casos de teste baseados em prompts personalizados.

## 🚀 Características

- **API RESTful** com ASP.NET Core 10
- **Integração com Google Gemini** para geração de casos de teste via IA
- **CORS** configurado para integração com frontend
- **OpenAPI/Swagger** para documentação
- **Modelo otimizado** com compilação AOT
- **Secrets Management** para proteção de chaves de API
- **Slim Builder** para performance otimizada

## 📋 Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Chave de API do [Google Gemini](https://makersuite.google.com/app/apikey)
- Visual Studio Community 2026 (ou superior) ou Visual Studio Code

## 🔧 Instalação

1. **Clone o repositório**
   ```powershell
   git clone https://github.com/Dev-Tigo/Backend-Test-Case-Generator.git
   cd Test-Case-Generator
   ```

2. **Configure suas secrets**
   ```powershell
   dotnet user-secrets init
   dotnet user-secrets set "Gemini:ApiKey" "sua-chave-de-api-do-gemini"
   ```

3. **Restaure as dependências**
   ```powershell
   dotnet restore
   ```

4. **Execute o projeto**
   ```powershell
   dotnet run
   ```

A API estará disponível em `https://localhost:5001` (ou conforme configurado)

## 📚 Endpoints

### POST `/api/generate-test-cases`

Gera casos de teste automaticamente usando a IA Gemini.

**Request Body:**
```json
{
  "userPrompt": "Descrição do que testar",
  "systemPrompt": "Instruções de sistema para a IA",
  "model": "gemini-3.6-flash"
}
```

**Response:**
```json
{
  "testCases": [
    {
      "name": "Nome do caso de teste",
      "description": "Descrição",
      "steps": ["passo 1", "passo 2"]
    }
  ]
}
```

## ⚙️ Configuração

### Variáveis de Ambiente

O projeto utiliza `UserSecrets` para gerenciar configurações sensíveis:

- `Gemini:ApiKey` - Sua chave de API do Google Gemini

### CORS

Por padrão, a API aceita requisições do frontend em `http://localhost:5173`. Modifique em `Program.cs` conforme necessário.

## 📂 Estrutura do Projeto

```
Test-Case-Generator/
├── Program.cs              # Configuração da aplicação e endpoints
├── appsettings.json       # Configurações da aplicação
├── Test-Case-Generator.csproj
└── Models/                # (Modelos de dados)
```

## 🛠️ Desenvolvimento

### Compilar
```powershell
dotnet build
```

### Executar em Development
```powershell
dotnet run
```

### Ver documentação OpenAPI
Acesse `https://localhost:5001/openapi/v1.json` quando o projeto estiver rodando.

## 🔐 Segurança

- Nunca cometa sua chave de API no repositório
- Sempre use `dotnet user-secrets` para dados sensíveis
- A API restringe requisições via CORS apenas ao frontend autorizado

## 📦 Dependências

- `Microsoft.AspNetCore.OpenApi` - Suporte para OpenAPI/Swagger
- `Microsoft.Extensions.Configuration.UserSecrets` - Gerenciamento seguro de secrets

## 🚢 Deployment

O projeto está configurado para compilação AOT e é otimizado para performance em produção. Para fazer deploy em produção:

```powershell
dotnet publish -c Release
```

## 📝 Licença

Este projeto está licenciado sob a MIT License - veja o arquivo [LICENSE](LICENSE) para detalhes.

## 👨‍💻 Autor

**Dev-Tigo**

- GitHub: [@Dev-Tigo](https://github.com/Dev-Tigo)

## 🤝 Contribuições

Contribuições são bem-vindas! Sinta-se livre para abrir issues e pull requests.

## ❓ Suporte

Se encontrar problemas, abra uma issue no repositório GitHub.

---

**Desenvolvido com ❤️ usando ASP.NET Core 10**
