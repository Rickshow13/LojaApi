# LojaApi

Web API em **ASP.NET Core (.NET 10)** com um `ProdutosController` que lista, busca, cadastra (com validação) e remove produtos. Os dados ficam em uma lista em memória e todos os cenários são testados por um arquivo `.http` no VS Code.

> Exercício 1 da disciplina **Desenvolvimento Web .NET**, Aula 01, UNIP.

---

## Sumário

- [Sobre o projeto](#sobre-o-projeto)
- [Tecnologias](#tecnologias)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Pré-requisitos](#pré-requisitos)
- [Como executar](#como-executar)
- [Endpoints](#endpoints)
- [Modelo e validações](#modelo-e-validações)
- [Exemplos de requisição e resposta](#exemplos-de-requisição-e-resposta)
- [Testes](#testes)
- [Códigos de status](#códigos-de-status)
- [Limitações](#limitações)
- [Próximos passos](#próximos-passos)
- [Documentação](#documentação)
- [Autor](#autor)

---

## Sobre o projeto

A **LojaApi** gerencia produtos de uma loja. Ela expõe quatro endpoints REST sob a rota `api/produtos`:

- listar todos os produtos;
- buscar um produto pelo id;
- cadastrar um produto, com validação dos dados;
- remover um produto pelo id.

Os produtos são guardados em uma **lista em memória** (`static List<Produto>`), o que basta para a aula, mas significa que **os dados somem quando a API é reiniciada**.

---

## Tecnologias

| Tecnologia | Uso no projeto |
|---|---|
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | Compilar e executar a aplicação |
| C# | Linguagem do modelo e do controller |
| ASP.NET Core (Controllers) | Framework da Web API: rotas, validação e respostas |
| Data Annotations | Regras de validação do modelo (`[Required]`, `[StringLength]`, `[Range]`) |
| Kestrel | Servidor web embutido, que atende na porta 5080 |
| VS Code + C# Dev Kit | Editor e suporte a C# |
| REST Client (Huachao Mao) | Execução dos testes do arquivo `.http` |

---

## Estrutura do projeto

```
LojaApi/
├── Controllers/
│   └── ProdutosController.cs   # endpoints GET, POST e DELETE
├── Models/
│   └── Produto.cs              # modelo de dados e validações
├── Properties/
│   └── launchSettings.json     # porta fixada em 5080
├── docs/
│   └── Relatorio_LojaApi.docx  # relatório técnico do projeto
├── LojaApi.http                # testes das requisições
├── Program.cs                  # configuração e inicialização da API
├── LojaApi.csproj              # configuração do projeto
├── appsettings.json
└── appsettings.Development.json
```

---

## Pré-requisitos

- [.NET SDK](https://dotnet.microsoft.com/download) 8, 9 ou 10 (o projeto foi feito com a **10.0.401**). Confirme com:

  ```bash
  dotnet --version
  ```

- [VS Code](https://code.visualstudio.com/) com as extensões **C# Dev Kit** (Microsoft) e **REST Client** (Huachao Mao), caso queira rodar os testes `.http`.

---

## Como executar

```bash
# 1. clonar o repositório
git clone https://github.com/Rickshow13/LojaApi.git
cd LojaApi

# 2. compilar (esperado: 0 erros)
dotnet build

# 3. executar
dotnet run
```

A API fica disponível em **http://localhost:5080**. O terminal deve mostrar:

```
Now listening on: http://localhost:5080
Application started. Press Ctrl+C to shut down.
```

Um aviso `Failed to determine the https port for redirect` é normal, porque o projeto roda só em HTTP. Para parar a API, use `Ctrl + C`. Se mudar o código, pare e rode `dotnet run` de novo.

---

## Endpoints

Base: `http://localhost:5080/api/produtos`

| Método | Rota | O que faz | Sucesso | Erro |
|---|---|---|---|---|
| GET | `/api/produtos` | Lista todos os produtos | `200 OK` | — |
| GET | `/api/produtos/{id}` | Busca um produto pelo id | `200 OK` | `404 Not Found` |
| POST | `/api/produtos` | Cadastra um produto, com validação | `201 Created` + header `Location` | `400 Bad Request` |
| DELETE | `/api/produtos/{id}` | Remove um produto pelo id | `204 No Content` | `404 Not Found` |

O `{id}` aceita apenas números inteiros (`{id:int}`). Uma chamada como `GET /api/produtos/abc` não casa com nenhuma rota e responde `404`.

---

## Modelo e validações

```csharp
public class Produto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Range(0.01, 1_000_000, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Preco { get; set; }
}
```

| Campo | Regra |
|---|---|
| `Id` | Definido pelo servidor. Se o cliente enviar um `id` no POST, ele é ignorado. |
| `Nome` | Obrigatório, de 3 a 100 caracteres. |
| `Preco` | Maior que zero (de 0,01 a 1.000.000). Usa `decimal` para evitar erros de arredondamento com dinheiro. |

A validação é feita automaticamente pelo `[ApiController]` **antes** do método `Create` ser executado. Por isso o método não tem nenhum `if` de validação.

---

## Exemplos de requisição e resposta

### Cadastrar um produto (sucesso)

```http
POST http://localhost:5080/api/produtos
Content-Type: application/json

{ "nome": "Teclado mecânico", "preco": 349.90 }
```

Resposta:

```http
HTTP/1.1 201 Created
Location: http://localhost:5080/api/produtos/1
Content-Type: application/json; charset=utf-8

{
  "id": 1,
  "nome": "Teclado mecânico",
  "preco": 349.90
}
```

### Cadastrar um produto inválido

```http
POST http://localhost:5080/api/produtos
Content-Type: application/json

{ "nome": "TV", "preco": 0 }
```

Resposta (formato ProblemDetails):

```http
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json; charset=utf-8

{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Nome": [ "O nome deve ter entre 3 e 100 caracteres." ],
    "Preco": [ "O preço deve ser maior que zero." ]
  },
  "traceId": "..."
}
```

### Listar, buscar e remover

```http
GET    http://localhost:5080/api/produtos        # 200 OK, lista em JSON
GET    http://localhost:5080/api/produtos/1      # 200 OK, ou 404 se não existir
DELETE http://localhost:5080/api/produtos/1      # 204 No Content, ou 404 se não existir
```

---

## Testes

Os testes ficam no arquivo [`LojaApi.http`](LojaApi.http) e são executados com a extensão **REST Client**: com a API rodando, clique em **Send Request** acima de cada requisição, **na ordem de 1 a 7**.

| # | Requisição | Status esperado |
|---|---|---|
| 1 | `POST` Teclado mecânico, preço 349.90 | `201 Created` |
| 2 | `POST` TV, preço 0 (inválido) | `400 Bad Request` |
| 3 | `GET` lista | `200 OK` |
| 4 | `GET /1` | `200 OK` |
| 5 | `GET /999` | `404 Not Found` |
| 6 | `DELETE /1` | `204 No Content` |
| 7 | `DELETE /1` de novo | `404 Not Found` |

A **ordem importa**, porque os dados ficam em memória: o teste 4 depende do 1, e o 7 depende do 6.

### Casos extras de erro

| Requisição | Status esperado | Motivo |
|---|---|---|
| `POST` sem o campo `nome` | `400` | `[Required]` barra o cadastro |
| `POST` sem o cabeçalho `Content-Type` | `415` | A API não sabe em que formato está o corpo |
| `GET /abc` | `404` | `{id:int}` não aceita texto, e a rota não casa |
| `POST` com `"preco": -5` | `400` | `[Range]` exige preço maior que zero |

---

## Códigos de status

| Status | Significado neste projeto |
|---|---|
| `200 OK` | Deu certo e os dados vêm no corpo (leitura). |
| `201 Created` | Deu certo e algo novo foi criado. Vem com o produto e o header `Location`. |
| `204 No Content` | Deu certo, mas não há nada a devolver (remoção). |
| `400 Bad Request` | Os dados enviados quebram as regras de validação. |
| `404 Not Found` | O id não existe, ou a rota não casa. |
| `415 Unsupported Media Type` | O pedido veio sem `Content-Type: application/json`. |

---

## Limitações

- **Dados em memória:** somem quando a API é reiniciada. Não há banco de dados.
- **Sem controle de concorrência:** a lista comum não foi pensada para muitos pedidos simultâneos.
- **Execução local:** a API só responde em `localhost`, no próprio computador.
- **Sem autenticação:** qualquer cliente que alcançasse a API poderia cadastrar e remover produtos.

---

## Próximos passos

- Trocar a lista em memória por um **banco de dados** (por exemplo, SQL Server ou MySQL) com o Entity Framework Core.
- Implementar o endpoint **`PUT api/produtos/{id}`** para atualizar nome e preço (`204` ao atualizar, `404` se o id não existir).
- Usar **DTOs** para separar o contrato da API do modelo interno.
- Adicionar **autenticação** e **HTTPS**.
- **Publicar** a API em um servidor ou na nuvem.

---

## Documentação

O relatório técnico, com os conceitos, o código explicado, as ferramentas, os testes e os problemas encontrados, está em [`docs/Relatorio_LojaApi.docx`](docs/Relatorio_LojaApi.docx).

---

## Autor

**Henrique**, estudante de Análise e Desenvolvimento de Sistemas na UNIP.

GitHub: [@Rickshow13](https://github.com/Rickshow13)
