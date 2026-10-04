using LojaApi.Models;           // para enxergar a classe Produto
using Microsoft.AspNetCore.Mvc; // controllers, ActionResult e atributos HTTP

namespace LojaApi.Controllers;

[ApiController]                 // validação automática, inferência de parâmetros, erros ProblemDetails
[Route("api/[controller]")]     // [controller] vira "produtos" -> api/produtos
public class ProdutosController : ControllerBase
{
    // "Banco" em memória. static: a lista sobrevive entre requisições,
    // pois o ASP.NET cria um controller NOVO a cada requisição.
    private static readonly List<Produto> _db = new();
    private static int _proximoId = 1;

    // GET api/produtos -> 200 + lista
    [HttpGet]
    public ActionResult<IEnumerable<Produto>> GetAll() => Ok(_db);

    // GET api/produtos/5 -> 200 + produto, ou 404
    [HttpGet("{id:int}")]
    public ActionResult<Produto> GetById(int id)
    {
        var produto = _db.FirstOrDefault(p => p.Id == id);
        return produto is null ? NotFound() : Ok(produto);
    }

    // POST api/produtos -> 201 + Location, ou 400 (validação automática)
    [HttpPost]
    public ActionResult<Produto> Create(Produto novo)
    {
        novo.Id = _proximoId++;   // o servidor decide o Id
        _db.Add(novo);
        return CreatedAtAction(nameof(GetById), new { id = novo.Id }, novo);
    }

    // DELETE api/produtos/5 -> 204, ou 404
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var removidos = _db.RemoveAll(p => p.Id == id);
        return removidos == 0 ? NotFound() : NoContent();
    }
}