using Application.Features.Orders.Dtos;
using Domain;

namespace Application.Abstraction.Services;

public interface IMailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = true);
    Task SendEmailAsync(string[] tos, string subject, string body, bool isBodyHtml = true);
    Task SendPasswordResetEmailAsync(string to,string userId, string resetToken);
    Task SendCompletedOrderEmailAsync(string to, string orderCode, string orderDescription, UserAddress orderAddress,
        DateTime orderCreatedDate, string userName, List<OrderItemDto> orderCartItems, decimal? orderTotalPrice);

}
