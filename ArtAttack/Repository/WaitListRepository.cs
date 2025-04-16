using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using ArtAttack.Domain;
using ArtAttack.Repository;
using ArtAttack.Shared;
using Microsoft.Data.SqlClient;

namespace ArtAttack.Repository
{
    public class WaitListRepository : IWaitListRepository
    {
        private readonly string connectionString;
        private readonly IDatabaseProvider databaseProvider;

        public WaitListRepository(string connectionString)
            : this(connectionString, new SqlDatabaseProvider())
        {
        }

        public WaitListRepository(string connectionString, IDatabaseProvider databaseProvider)
        {
            this.connectionString = connectionString;
            this.databaseProvider = databaseProvider;
        }

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
                                ProductWaitListID = waitListProductId,
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

                    if (totalUsersParam.Value != DBNull.Value)
                    {
                        waitListSize = Convert.ToInt32(totalUsersParam.Value);
                    }
                }
                connection.Close();
                return waitListSize;
            }
        }

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

                    if (positionParam.Value != DBNull.Value)
                    {
                        userWaitListposition = Convert.ToInt32(positionParam.Value);
                    }
                }
                connection.Close();
                return userWaitListposition;
            }
        }

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

        public int GetWaitlistProductId(int productId)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            using (SqlCommand sqlCommand = new SqlCommand(
                "SELECT WaitListProductID FROM WaitListProduct WHERE ProductID = @ProductId",
                sqlConnection))
            {
                sqlCommand.Parameters.AddWithValue("@ProductId", productId);
                sqlConnection.Open();
                object queryResult = sqlCommand.ExecuteScalar();
                if (queryResult != null)
                {
                    return Convert.ToInt32(queryResult);
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}