namespace Application.Features.Carts.Dtos;

public class CreateCartItemDto
{
    public string CartId { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public bool IsChecked { get; set; } = true;
}