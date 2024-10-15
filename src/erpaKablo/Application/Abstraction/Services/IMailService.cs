using Application.Features.Orders.Dtos;

namespace Application.Abstraction.Services;

public interface IMailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = true);
    Task SendEmailAsync(string[] tos, string subject, string body, bool isBodyHtml = true);
    Task SendPasswordResetEmailAsync(string to,string userId, string resetToken);
    Task SendCompletedOrderEmailAsync(string to, string orderCode, string orderDescription, string orderAddress, DateTime orderCreatedDate, string userName, List<OrderCartItemDto> orderCartItems, float orderTotalPrice);

}
