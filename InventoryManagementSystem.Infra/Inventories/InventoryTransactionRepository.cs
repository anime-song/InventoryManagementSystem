using InventoryManagementSystem.Domain.Helpers;
using InventoryManagementSystem.Domain.Inventories;
using InventoryManagementSystem.Domain.Inventories.Requests;
using LiteDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Infra.Inventories
{
    public sealed class InventoryTransactionRepository : IInventoryTransactionRepository
    {
        private readonly ILiteCollection<InventoryTransactionEntity> _collection;

        public InventoryTransactionRepository(LiteDatabase db)
        {
            _collection = db.GetCollection<InventoryTransactionEntity>("InventoryTransaction");
        }

        public void Add(InventoryTransaction transaction)
        {
            var entity = ToEntity(transaction);
            _collection.Insert(entity);
        }

        public IEnumerable<InventoryTransaction> FindAll()
        {
            return _collection.FindAll().Select(ToDomain);
        }

        public PageResult<InventoryTransaction> FindWithPaginate(FindTransactionRequest request, int page, int fetchCount)
        {
            var query = _collection.Query();
            query = ApplyFilters(query, request);

            var transactions = query.OrderByDescending(x => x.TransactionDate)
                    .Skip((page - 1) * fetchCount)
                    .Limit(fetchCount)
                    .ToList()
                    .Select(ToDomain);
            var totalDataCount = _collection.Count();

            return new PageResult<InventoryTransaction>(
                currentPageIndex: page,
                totalDataCount: totalDataCount,
                pageInDataCount: fetchCount,
                pageData: transactions
            );
        }

        public IEnumerable<InventoryTransaction> Find(FindTransactionRequest request)
        {
            var query = _collection.Query();
            query = ApplyFilters(query, request);

            return query.ToList().Select(ToDomain);
        }

        public InventoryTransaction? FindById(int id)
        {
            var entity = _collection.FindById(id);
            return entity is null ? null : ToDomain(entity);
        }

        private ILiteQueryable<InventoryTransactionEntity> ApplyFilters(
            ILiteQueryable<InventoryTransactionEntity> query,
            FindTransactionRequest request)
        {
            if (request.InventoryId.HasValue)
                query = query.Where(x => x.InventoryId == request.InventoryId);

            if (request.TransactionPeriodStart.HasValue)
                query = query.Where(x => x.TransactionDate >= request.TransactionPeriodStart);

            if (request.TransactionPeriodEnd.HasValue)
                query = query.Where(x => x.TransactionDate <= request.TransactionPeriodEnd);

            return query;
        }

        private static InventoryTransaction ToDomain(InventoryTransactionEntity entity)
        {
            return InventoryTransaction.FromPersistence(
                id: entity.Id,
                transactionType: new TransactionType(entity.TransactionType),
                transactionDate: entity.TransactionDate,
                quantity: entity.Quantity,
                inventoryId: entity.InventoryId,
                canceledTransactionId: entity.CanceledTransactionId);
        }

        private static InventoryTransactionEntity ToEntity(InventoryTransaction model)
        {
            return new InventoryTransactionEntity()
            {
                Id = model.Id ?? 0,
                InventoryId = model.InventoryId,
                CanceledTransactionId = model.CanceledTransactionId,
                Quantity = model.Quantity,
                TransactionDate = model.TransactionDate,
                TransactionType = model.TransactionType.Value
            };
        }
    }
}