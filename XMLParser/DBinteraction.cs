using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XMLParser.Parser;

namespace XMLParser
{
    public class DBinteraction
    {
        public interface IDbConnection
        {
            void Open();
            void Close();
            int ExecuteNonQuery(string query, params NpgsqlParameter[] parameters);
            object ExecuteScalar(string query, params NpgsqlParameter[] parameters);
        }

        public class PostgresConnection : IDbConnection
        {
            private readonly string connectionString;
            private NpgsqlConnection connection;

            public PostgresConnection(string connectionString)
            {
                this.connectionString = connectionString;
            }

            public void Open()
            {
                connection = new NpgsqlConnection(connectionString);
                connection.Open();
            }

            public void Close()
            {
                connection.Close();
            }

            public int ExecuteNonQuery(string query, params NpgsqlParameter[] parameters)
            {
                var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddRange(parameters);
                return command.ExecuteNonQuery();
            }

            public object ExecuteScalar(string query, params NpgsqlParameter[] parameters)
            {
                var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddRange(parameters);
                return command.ExecuteScalar();
            }
        }

        public class UserRepository
        {
            private readonly IDbConnection dbConnection;

            public UserRepository(IDbConnection dbConnection)
            {
                this.dbConnection = dbConnection;
            }

            public int GetOrCreateUser(User user)
            {
                string query = $"SELECT * FROM \"Users\" WHERE \"FIO\" = @fio";
                var parameter = new NpgsqlParameter("@fio", user.FIO);

                object result = dbConnection.ExecuteScalar(query, parameter);

                if (result == null)
                {
                    query = $"INSERT INTO \"Users\" (\"FIO\",\"email\") VALUES (@fio, @email) RETURNING user_id;";
                    var insertParameters = new[]
                    {
                    new NpgsqlParameter("@fio", user.FIO),
                    new NpgsqlParameter("@email", user.Email)
                    };
                    return Convert.ToInt32(dbConnection.ExecuteScalar(query, insertParameters));
                }

                return Convert.ToInt32(result);
            }
        }

        public class OrderRepository
        {
            private readonly IDbConnection dbConnection;

            public OrderRepository(IDbConnection dbConnection)
            {
                this.dbConnection = dbConnection;
            }

            public int GetOrCreateOrder(Order item, int userId)
            {
                string query = $"SELECT * FROM \"Orders\" WHERE \"number\" = @orderNumber";
                var parameter = new NpgsqlParameter("@orderNumber", item.No);

                object result = dbConnection.ExecuteScalar(query, parameter);

                if (result == null)
                {
                    query = $"INSERT INTO \"Orders\" (\"user_id\", \"number\", \"date\", \"sum\") VALUES (@userId, @orderNumber, @regDate, @sum::real) RETURNING order_id;";
                    var insertParameters = new[]
                    {
                    new NpgsqlParameter("@userId", userId),
                    new NpgsqlParameter("@orderNumber", item.No),
                    new NpgsqlParameter("@regDate", item.RegDate),
                    new NpgsqlParameter("@sum", item.Sum.ToString("F2", CultureInfo.InvariantCulture))
                    };
                    return Convert.ToInt32(dbConnection.ExecuteScalar(query, insertParameters));
                }

                return Convert.ToInt32(result);
            }
        }

        public class ProductRepository
        {
            private readonly IDbConnection dbConnection;

            public ProductRepository(IDbConnection dbConnection)
            {
                this.dbConnection = dbConnection;
            }

            public int GetOrCreateProduct(Product product)
            {
                string query = $"SELECT * FROM \"Product\" WHERE \"name\" = @productName";
                var parameter = new NpgsqlParameter("@productName", product.Name);

                object result = dbConnection.ExecuteScalar(query, parameter);

                if (result == null)
                {
                    query = $"INSERT INTO \"Product\" (\"name\",\"price\") VALUES (@productName, @price::real) RETURNING product_id;";
                    var insertParameters = new[]
                    {
                    new NpgsqlParameter("@productName", product.Name),
                    new NpgsqlParameter("@price", product.Price.ToString("F2", CultureInfo.InvariantCulture))
                    };
                    return Convert.ToInt32(dbConnection.ExecuteScalar(query, insertParameters));
                }

                return Convert.ToInt32(result);
            }
        }

        public class OrderService
        {
            private readonly IDbConnection dbConnection;
            private readonly UserRepository userRepository;
            private readonly OrderRepository orderRepository;
            private readonly ProductRepository productRepository;

            public OrderService(IDbConnection dbConnection)
            {
                this.dbConnection = dbConnection;
                this.userRepository = new UserRepository(dbConnection);
                this.orderRepository = new OrderRepository(dbConnection);
                this.productRepository = new ProductRepository(dbConnection);
            }

            public void EnterDataToDB(List<Order> orders)
            {
                dbConnection.Open();

                foreach (var order in orders)
                {
                    int userId = userRepository.GetOrCreateUser(order.User);
                    int orderId = orderRepository.GetOrCreateOrder(order, userId);

                    foreach (var product in order.Products)
                    {
                        int productId = productRepository.GetOrCreateProduct(product);

                        string query = $"INSERT INTO \"Products_in_Order\" (\"order_id\",\"product_id\",\"count\") VALUES (@orderId, @productId, @quantity)";
                        var parameters = new[]
                        {
                        new NpgsqlParameter("@orderId", orderId),
                        new NpgsqlParameter("@productId", productId),
                        new NpgsqlParameter("@quantity", product.Quantity)
                        };
                        dbConnection.ExecuteNonQuery(query, parameters);
                    }
                }

                dbConnection.Close();
            }
        }
    }
}
