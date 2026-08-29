# Test Case Generator

Uma API backend em ASP.NET Core para gerar casos de teste automaticamente utilizando IA. Suporta múltiplos provedores (Google Gemini e Groq) para criar casos de teste baseados em prompts personalizados.

## 🚀 Características

- **API RESTful** com ASP.NET Core 10
- **Múltiplos Provedores de IA** - Google Gemini e Groq
- **Retry Automático** para falhas intermitentes (Groq)
- **CORS** configurado para integração com frontend
- **OpenAPI/Swagger** para documentação
- **Modelo otimizado** com compilação AOT
- **Secrets Management** para proteção de chaves de API
- **Slim Builder** para performance otimizada
- **Arquitetura Modular** - separação clara de responsabilidades

## 📋 Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Chaves de API:
  - [Google Gemini](https://makersuite.google.com/app/apikey) (opcional)
  - [Groq](https://console.groq.com/keys) (opcional)
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
   dotnet user-secrets set "Groq:ApiKey" "sua-chave-de-api-do-groq"
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

Gera casos de teste automaticamente usando IA.

**Request Body:**
```json
{
  "provider": "gemini",
  "model": "gemini-3.6-flash",
  "systemPrompt": "Você é um expert em QA. Gere casos de teste em formato JSON...",
  "userPrompt": "Gere 3 casos de teste para validação de login"
}
```

**Parâmetros:**
- `provider` (string, opcional): `"gemini"` ou `"groq"`. Padrão: `"gemini"`
- `model` (string): Modelo de IA específico
  - Gemini: `"gemini-3.6-flash"`, `"gemini-pro"`, etc.
  - Groq: `"openai/gpt-oss-120b"`, `"deepseek-r1-distill-llama-70b"`, etc.
- `systemPrompt` (string): Instruções de sistema para a IA
- `userPrompt` (string): Descrição do que testar

**Response (200 OK):**
```json
{
  "casos_de_teste": [
    {
      "id": "TC001",
      "titulo": "Login válido",
      "pre_condicoes": "Usuário não autenticado",
      "modulo": "Autenticação",
      "tipo": "Funcional",
      "prioridade": "Alta",
      "passos": [
        "Abrir página de login",
        "Inserir credenciais válidas",
        "Clicar em 'Entrar'"
      ],
      "dados_teste": "user@example.com / senha123",
      "resultado_esperado": "Usuário autenticado e redirecionado para dashboard"
    }
  ]
}
```

**Response (4xx/5xx):**
```json
{
  "error": "mensagem de erro descritiva"
}
```

## ⚙️ Configuração

### Variáveis de Ambiente

O projeto utiliza `UserSecrets` para gerenciar configurações sensíveis:

- `Gemini:ApiKey` - Sua chave de API do Google Gemini
- `Groq:ApiKey` - Sua chave de API do Groq

### CORS

Por padrão, a API aceita requisições do frontend em `http://localhost:5173`. Modifique em `Program.cs` conforme necessário:

```csharp
policy.WithOrigins("http://localhost:5173")
      .AllowAnyMethod()
      .AllowAnyHeader();
```

## 📂 Estrutura do Projeto

```
Test-Case-Generator/
├── Program.cs                          # Configuração da aplicação (DI, CORS, endpoints)
├── Contracts.cs                        # DTOs do contrato com o frontend
├── AppJsonSerializerContext.cs         # Contexto JSON serializer com source generation
├── TestCaseResponseParser.cs           # Parse e limpeza de respostas da IA
├── appsettings.json                    # Configurações da aplicação
├── Test-Case-Generator.csproj
└── Providers/
    ├── AiGenerationResult.cs           # Resultado unificado dos providers
    ├── GeminiClient.cs                 # Integração com Google Gemini
    └── GroqClient.cs                   # Integração com Groq (com retry automático)
```

### Descripção dos Arquivos

| Arquivo | Responsabilidade |
|---------|------------------|
| `Program.cs` | Wiring da aplicação (DI, CORS, middleware, endpoints) |
| `Contracts.cs` | DTOs públicos (`GenerateRequest`, `GenerateResponse`, `TestCase`, `ErrorResponse`) |
| `AppJsonSerializerContext.cs` | Context JSON com source code generation para performance AOT |
| `TestCaseResponseParser.cs` | Lógica centralizada para parse e limpeza de respostas (remove markdown, valida JSON) |
| `Providers/AiGenerationResult.cs` | Record unificado de resultados dos providers |
| `Providers/GeminiClient.cs` | DTOs e método `CallAsync()` para integração com Gemini |
| `Providers/GroqClient.cs` | DTOs e método `CallAsync()` com retry automático para Groq |

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

### Estrutura de Uso

```csharp
// No endpoint, os provedores são chamados assim:
var result = provider switch
{
    "groq" => await GroqClient.CallAsync(client, groqApiKey, request),
    "gemini" => await GeminiClient.CallAsync(client, geminiApiKey, request),
    _ => new AiGenerationResult(null, $"Unknown provider: {provider}", 400)
};

// O resultado é então parseado:
var (parsed, parseError) = TestCaseResponseParser.ParseResponse(result.Text!);
```

## 🔐 Segurança

- Nunca cometa suas chaves de API no repositório
- Sempre use `dotnet user-secrets` para dados sensíveis
- A API valida requisições via CORS apenas do frontend autorizado
- Secrets são carregados do User Secrets (desenvolvimento) ou variáveis de ambiente (produção)

## 📦 Dependências

- `Microsoft.AspNetCore.OpenApi` (v10.0.11) - Suporte para OpenAPI/Swagger
- `Microsoft.Extensions.Configuration.UserSecrets` (v10.0.0) - Gerenciamento seguro de secrets

## 🚢 Deployment

O projeto está configurado para compilação AOT e é otimizado para performance em produção. Para fazer deploy:

```powershell
dotnet publish -c Release
```

A aplicação suporta:
- **Compilação AOT** - binários nativos mais rápidos
- **Invariant Globalization** - para aplicações sem dependências de localização
- **Slim Builder** - apenas middleware necessário

## 📝 Licença

Este projeto está licenciado sob a MIT License - veja o arquivo [LICENSE](LICENSE) para detalhes.

## 👨‍💻 Autor

**Dev-Tigo**

- GitHub: [@Dev-Tigo](https://github.com/Dev-Tigo)

## 🤝 Contribuições

Contribuições são bem-vindas! Sinta-se livre para:
1. Fork o projeto
2. Criar uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abrir um Pull Request

## ❓ Suporte

Se encontrar problemas:
1. Consulte a [documentação da API](#-endpoints)
2. Verifique se suas chaves de API estão configuradas corretamente
3. Abra uma [issue no GitHub](https://github.com/Dev-Tigo/Backend-Test-Case-Generator/issues)

---

**Desenvolvido com ❤️ usando ASP.NET Core 10**
