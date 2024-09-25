using Application.Features.ProductImageFiles.Dtos;
using Core.Application.Responses;

namespace Application.Features.Carts.Queries.GetList;

public class GetCartItemsQueryResponse :IResponse
{
    public string CartItemId { get; set; }
    public string ProductName { get; set; }
    public decimal? UnitPrice { get; set; }
    public int Quantity { get; set; }
    public ProductImageFileDto? ShowcaseImage { get; set; }
    public bool IsChecked { get; set; }
}