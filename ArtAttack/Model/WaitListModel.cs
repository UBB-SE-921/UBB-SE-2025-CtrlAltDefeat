using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using ArtAttack.Shared;

namespace ArtAttack.Model
{
    public class WaitListModel : IWaitListModel
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        /// <summary>
        /// Default constructor for WaitListModel.
        /// </summary>
        /// <param name="connectionString">The database connection string. Cannot be null or empty.</param>
        /// <remarks>
        /// Initializes a new instance of the WaitListModel class with the specified connection string
        /// and a default SqlDatabaseProvider. This constructor is typically used in production code.
        /// </remarks>
        public WaitListModel(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        /// <summary>
        /// Constructor for WaitListModel with dependency injection support.
        /// </summary>
        /// <param name="connectionString">The database connection string. Cannot be null or empty.</param>
        /// <param name="databaseProvider">The database provider implementation to use for database operations. Cannot be null.</param>
        /// <remarks>
        /// Initializes a new instance of the WaitListModel class with the specified connection string
        /// and database provider. This constructor enables dependency injection and is primarily used for testing
        /// with mock database providers.
        /// </remarks>
        public WaitListModel(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString;
            this.databaseProvider = databaseProvider;
        }

        /// <summary>
        /// Adds a user to the waitlist for a specific product.
        /// </summary>
        /// <param name="userId">The ID of the user to be added to the waitlist. Must be a positive integer.</param>
        /// <param name="productWaitListId">The ID of the product waitlist. Must be a positive integer.</param>
        public void AddUserToWaitlist(int userId, int productWaitListId)
        {
            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "AddUserToWaitlist";
                    command.CommandType = CommandType.StoredProcedure;

                    IDbDataParameter userIdParam = command.CreateParameter();
                    userIdParam.ParameterName = "@UserID";
                    userIdParam.Value = userId;
                    command.Parameters.Add(userIdParam);

                    IDbDataParameter productIdParam = command.CreateParameter();
                    productIdParam.ParameterName = "@ProductWaitListID";
                    productIdParam.Value = productWaitListId;
                    command.Parameters.Add(productIdParam);

                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        /// <summary>
        /// Removes a user from the waitlist and adjusts the queue positions.
        /// </summary>
        /// <param name="userId">The ID of the user to be removed from the waitlist. Must be a positive integer.</param>
        /// <param name="productWaitListId">The ID of the product waitlist. Must be a positive integer.</param>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public void RemoveUserFromWaitlist(int userId, int productWaitListId)
        {
            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "RemoveUserFromWaitlist";
                    command.CommandType = CommandType.StoredProcedure;

                    IDbDataParameter userIdParam = command.CreateParameter();
                    userIdParam.ParameterName = "@UserID";
                    userIdParam.Value = userId;
                    command.Parameters.Add(userIdParam);

                    IDbDataParameter productIdParam = command.CreateParameter();
                    productIdParam.ParameterName = "@ProductWaitListID";
                    productIdParam.Value = productWaitListId;
                    command.Parameters.Add(productIdParam);

                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        /// <summary>
        /// Retrieves all users in a waitlist for a given product.
        /// </summary>
        /// <param name="waitListProductId">The ID of the product waitlist. Must be a positive integer.</param>
        /// <returns>A list of UserWaitList objects representing the users in the waitlist.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public List<UserWaitList> GetUsersInWaitlist(int waitListProductId)
        {
            var usersInWaitList = new List<UserWaitList>();

            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "GetUsersInWaitlist";
                    command.CommandType = CommandType.StoredProcedure;

                    IDbDataParameter productIdParam = command.CreateParameter();
                    productIdParam.ParameterName = "@WaitListProductID";
                    productIdParam.Value = waitListProductId;
                    command.Parameters.Add(productIdParam);

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var userWaitListEntry = new UserWaitList
                            {
                                UserID = reader.GetInt32(reader.GetOrdinal("userID")),
                                ProductWaitListID = waitListProductId, // Add this line
                                PositionInQueue = reader.GetInt32(reader.GetOrdinal("positionInQueue")),
                                JoinedTime = reader.GetDateTime(reader.GetOrdinal("joinedTime"))
                            };
                            usersInWaitList.Add(userWaitListEntry);
                        }
                    }
                }
                connection.Close();
            }

            return usersInWaitList;
        }

        /// <summary>
        /// Gets all waitlists that a user is part of.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <returns>A list of UserWaitList objects representing the waitlists the user is part of.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public List<UserWaitList> GetUserWaitlists(int userId)
        {
            var userWaitlists = new List<UserWaitList>();

            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "GetUserWaitlists";
                    command.CommandType = CommandType.StoredProcedure;

                    IDbDataParameter userIdParam = command.CreateParameter();
                    userIdParam.ParameterName = "@UserID";
                    userIdParam.Value = userId;
                    command.Parameters.Add(userIdParam);

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var userWaitlist = new UserWaitList
                            {
                                UserID = userId,
                                ProductWaitListID = reader.GetInt32(reader.GetOrdinal("productWaitListID")),
                                PositionInQueue = reader.GetInt32(reader.GetOrdinal("positionInQueue")),
                                JoinedTime = reader.GetDateTime(reader.GetOrdinal("joinedTime"))
                            };

                            userWaitlists.Add(userWaitlist);
                        }
                    }
                }
                connection.Close();
            }

            return userWaitlists;
        }

        /// <summary>
        /// Gets the number of users in a product's waitlist.
        /// </summary>
        /// <param name="productWaitListId">The ID of the product waitlist. Must be a positive integer.</param>
        /// <returns>The number of users in the waitlist.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public int GetWaitlistSize(int productWaitListId)
        {
            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                int waitListSize = 0;
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "GetWaitlistSize";
                    command.CommandType = CommandType.StoredProcedure;

                    IDbDataParameter productIdParam = command.CreateParameter();
                    productIdParam.ParameterName = "@ProductWaitListID";
                    productIdParam.Value = productWaitListId;
                    command.Parameters.Add(productIdParam);

                    IDbDataParameter totalUsersParam = command.CreateParameter();
                    totalUsersParam.ParameterName = "@TotalUsers";
                    totalUsersParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(totalUsersParam);

                    command.ExecuteNonQuery();

                    // Get the output parameter value
                    if (totalUsersParam.Value != DBNull.Value)
                    {
                        waitListSize = Convert.ToInt32(totalUsersParam.Value);
                    }
                }
                connection.Close();
                return waitListSize;
            }
        }

        /// <summary>
        /// Checks if a user is in a product's waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>True if the user is in the waitlist, otherwise false.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public bool IsUserInWaitlist(int userId, int productId)
        {
            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                bool isInWaitlist = false;
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "CheckUserInProductWaitlist";
                    command.CommandType = CommandType.StoredProcedure;

                    IDbDataParameter userIdParam = command.CreateParameter();
                    userIdParam.ParameterName = "@UserID";
                    userIdParam.Value = userId;
                    command.Parameters.Add(userIdParam);

                    IDbDataParameter productIdParam = command.CreateParameter();
                    productIdParam.ParameterName = "@ProductID";
                    productIdParam.Value = productId;
                    command.Parameters.Add(productIdParam);

                    object result = command.ExecuteScalar();
                    isInWaitlist = result != null && result != DBNull.Value;
                }
                connection.Close();
                return isInWaitlist;
            }
        }

        /// <summary>
        /// Gets the position of a user in a product's waitlist.
        /// </summary>
        /// <param name="userId">The ID of the user. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>The position of the user in the waitlist, or -1 if the user is not in the waitlist.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public int GetUserWaitlistPosition(int userId, int productId)
        {
            Debug.WriteLine($"GetUserWaitlistPosition: UserID: {userId}, ProductID: {productId}");
            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                int userWaitListposition = -1;
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "GetUserWaitlistPosition";
                    command.CommandType = CommandType.StoredProcedure;

                    IDbDataParameter userIdParam = command.CreateParameter();
                    userIdParam.ParameterName = "@UserID";
                    userIdParam.Value = userId;
                    command.Parameters.Add(userIdParam);

                    IDbDataParameter productIdParam = command.CreateParameter();
                    productIdParam.ParameterName = "@ProductID";
                    productIdParam.Value = productId;
                    command.Parameters.Add(productIdParam);

                    IDbDataParameter positionParam = command.CreateParameter();
                    positionParam.ParameterName = "@Position";
                    positionParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(positionParam);

                    command.ExecuteNonQuery();

                    // Get the output parameter value
                    if (positionParam.Value != DBNull.Value)
                    {
                        userWaitListposition = Convert.ToInt32(positionParam.Value);
                    }
                }
                connection.Close();
                return userWaitListposition;
            }
        }

        /// <summary>
        /// Retrieves all users in a waitlist for a given product, ordered by their position in the queue.
        /// </summary>
        /// <param name="productId">The ID of the product. Must be a positive integer.</param>
        /// <returns>A list of UserWaitList objects representing the users in the waitlist, ordered by their position in the queue.</returns>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public List<UserWaitList> GetUsersInWaitlistOrdered(int productId)
        {
            var orderedWaitlistUsers = new List<UserWaitList>();

            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            {
                connection.Open();
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "GetOrderedWaitlistUsers";
                    command.CommandType = CommandType.StoredProcedure;

                    IDbDataParameter productIdParam = command.CreateParameter();
                    productIdParam.ParameterName = "@ProductId";
                    productIdParam.Value = productId;
                    command.Parameters.Add(productIdParam);

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var waitListUser = new UserWaitList
                            {
                                ProductWaitListID = reader.GetInt32(reader.GetOrdinal("productWaitListID")),
                                UserID = reader.GetInt32(reader.GetOrdinal("userID")),
                                JoinedTime = reader.GetDateTime(reader.GetOrdinal("joinedTime")),
                                PositionInQueue = reader.GetInt32(reader.GetOrdinal("positionInQueue"))
                            };
                            orderedWaitlistUsers.Add(waitListUser);
                        }
                    }
                }
                connection.Close();
            }

            return orderedWaitlistUsers;
        }
    }
}