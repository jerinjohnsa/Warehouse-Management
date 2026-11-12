
using Dapper;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Data;

namespace WarehouseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly DbConnectionFactory _connectionFactory;

        public WarehouseController(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            using var connection = _connectionFactory.CreateConnection();

            string sql = @"
                SELECT Id, Product, Quantity, WarehouseId
                FROM WarehouseInventory";

            var result = await connection.QueryAsync(sql);

            return Ok(result);
        }

        
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferStock(
            [FromBody] TransferRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            if (request.Quantity <= 0)
                return BadRequest("Quantity must be greater than zero.");

            if (request.FromWarehouseId == request.ToWarehouseId)
                return BadRequest("Source and destination cannot be the same.");

            using var connection = _connectionFactory.CreateConnection();

            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                
                string deductSql = @"
                    UPDATE WarehouseInventory
                    SET Quantity = Quantity - @Quantity
                    WHERE Product = @Product
                    AND WarehouseId = @FromWarehouseId
                    AND Quantity >= @Quantity";

                int rowsAffected = await connection.ExecuteAsync(
                    deductSql, request, transaction);

                if (rowsAffected == 0)
                {
                    transaction.Rollback();

                    return BadRequest(
                        "Product not found or insufficient stock.");
                }

                
                string checkSql = @"
                    SELECT COUNT(1)
                    FROM WarehouseInventory
                    WHERE Product = @Product
                    AND WarehouseId = @ToWarehouseId";

                int productExists = await connection.ExecuteScalarAsync<int>(
                    checkSql, request, transaction);

                if (productExists > 0)
                {
                    
                    string updateSql = @"
                        UPDATE WarehouseInventory
                        SET Quantity = Quantity + @Quantity
                        WHERE Product = @Product
                        AND WarehouseId = @ToWarehouseId";

                    await connection.ExecuteAsync(
                        updateSql, request, transaction);
                }
                else
                {
                    
                    string insertSql = @"
                        INSERT INTO WarehouseInventory
                        (Product, Quantity, WarehouseId)
                        VALUES
                        (@Product, @Quantity, @ToWarehouseId)";

                    await connection.ExecuteAsync(
                        insertSql, request, transaction);
                }

                
                transaction.Commit();

                return Ok(new
                {
                    Message = "Stock transferred successfully!",
                    Product = request.Product,
                    QuantityTransferred = request.Quantity,
                    FromWarehouse = request.FromWarehouseId,
                    ToWarehouse = request.ToWarehouseId
                });
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
