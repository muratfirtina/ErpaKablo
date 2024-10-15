using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class OrderItemRepository : EfRepositoryBase<OrderItem, string, ErpaKabloDbContext>, IOrderItemRepository
    {
        private readonly IProductRepository _productRepository;

        public OrderItemRepository(ErpaKabloDbContext context, IProductRepository productRepository) : base(context)
        {
            _productRepository = productRepository;
        }

        // OrderItem'ı silme ve stoğu geri yükleme işlemi
        public async Task<bool> RemoveOrderItemAsync(string? orderItemId)
        {
            using var transaction = await Context.Database.BeginTransactionAsync();  // Transaction başlat

            try
            {
                // OrderItem'ı ve bağlı olduğu Product'ı bul
                var orderItem = await Query().Include(oi => oi.Product)
                    .FirstOrDefaultAsync(oi => oi.Id == orderItemId);

                if (orderItem == null) throw new Exception("Order Item not found.");

                // Ürünün stoğunu geri yükle
                var product = orderItem.Product;
                if (product != null)
                {
                    product.Stock += orderItem.Quantity;
                    await _productRepository.UpdateAsync(product);
                }

                // OrderItem'ı sil
                await DeleteAsync(orderItem);
                await transaction.CommitAsync();  // Transaction'ı commit et
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();  // Hata olursa rollback yap
                throw;
            }
        }

        // OrderItem'ın miktarını güncelleme ve stoğu kontrol etme işlemi
        public async Task<bool> UpdateOrderItemQuantityAsync(string orderItemId, int newQuantity)
        {
            using var transaction = await Context.Database.BeginTransactionAsync();  // Transaction başlat

            try
            {
                // OrderItem'ı ve bağlı olduğu Product'ı bul
                var orderItem = await Query().Include(oi => oi.Product)
                    .FirstOrDefaultAsync(oi => oi.Id == orderItemId);

                if (orderItem == null) throw new Exception("Order Item not found.");
                var product = orderItem.Product;
                if (product == null) throw new Exception("Product not found.");

                // Stok kontrolü: Yeni quantity mevcut stoktan fazla ise hata ver
                int stockDifference = newQuantity - orderItem.Quantity;
                if (stockDifference > product.Stock)
                {
                    throw new Exception($"Not enough stock for product {product.Name}. Available stock: {product.Stock}");
                }

                // Stok güncelleme: Artış varsa stoğu düşür, azalma varsa stoğu arttır
                product.Stock -= stockDifference;
                await _productRepository.UpdateAsync(product);

                // OrderItem'ı güncelle
                orderItem.Quantity = newQuantity;
                await UpdateAsync(orderItem);

                await transaction.CommitAsync();  // Transaction'ı commit et
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();  // Hata olursa rollback yap
                throw;
            }
        }
    }
}