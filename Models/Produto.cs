using System.ComponentModel.DataAnnotations; // atributos de validação

namespace LojaApi.Models;

public class Produto
{
    // Quem define o Id é o servidor, por isso não tem validação.
    public int Id { get; set; }

    // Obrigatório, entre 3 e 100 caracteres.
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    // Entre 0,01 e 1.000.000 (garante preço > 0).
    [Range(0.01, 1_000_000, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Preco { get; set; }
}