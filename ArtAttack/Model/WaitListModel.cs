using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtAttack.Domain;
using Microsoft.Data.SqlClient;

namespace ArtAttack.Model
{
    public class WaitListModel : IWaitListModel
    {
        private readonly string connectionString;

        public WaitListModel(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Adds a user to the waitlist for a specific product.
        /// </summary>
        /// <param name="userId">The ID of the user to be added to the waitlist. Must be a positive integer.</param>
        /// <param name="productWaitListId">The ID of the product waitlist. Must be a positive integer.</param>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public void AddUserToWaitlist(int userId, int productWaitListId)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("AddUserToWaitlist", sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    sqlCommand.Parameters.Add("@ProductWaitListID", SqlDbType.Int).Value = productWaitListId;

                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
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
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("RemoveUserFromWaitlist", sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    sqlCommand.Parameters.Add("@ProductWaitListID", SqlDbType.Int).Value = productWaitListId;

                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
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

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("GetUsersInWaitlist", sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.Add("@WaitListProductID", SqlDbType.BigInt).Value = waitListProductId;

                    sqlConnection.Open();

                    using (SqlDataReader sQLDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sQLDataReader.Read())
                        {
                            var userWaitListEntry = new UserWaitList
                            {
                                UserID = sQLDataReader.GetInt32(sQLDataReader.GetOrdinal("userID")),
                                PositionInQueue = sQLDataReader.GetInt32(sQLDataReader.GetOrdinal("positionInQueue")),
                                JoinedTime = sQLDataReader.GetDateTime(sQLDataReader.GetOrdinal("joinedTime"))
                            };
                            usersInWaitList.Add(userWaitListEntry);
                        }
                    }
                }
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

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("GetUserWaitlists", sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

                    sqlConnection.Open();

                    using (SqlDataReader sQLDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sQLDataReader.Read())
                        {
                            var userWaitlist = new UserWaitList
                            {
                                UserID = userId,
                                ProductWaitListID = sQLDataReader.GetInt32(sQLDataReader.GetOrdinal("productWaitListID")),
                                PositionInQueue = sQLDataReader.GetInt32(sQLDataReader.GetOrdinal("positionInQueue")),
                                JoinedTime = sQLDataReader.GetDateTime(sQLDataReader.GetOrdinal("joinedTime"))
                            };

                            userWaitlists.Add(userWaitlist);
                        }
                    }
                }
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
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("GetWaitlistSize", sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.Add("@ProductWaitListID", SqlDbType.Int).Value = productWaitListId;
                    SqlParameter totalUsersParameter = new SqlParameter("@TotalUsers", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    sqlCommand.Parameters.Add(totalUsersParameter);

                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();

                    return (int)totalUsersParameter.Value;
                }
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
            bool isUserInWaitlist = false;
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("CheckUserInProductWaitlist", sqlConnection))
                {
                    sqlCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    sqlCommand.Parameters.Add("@ProductID", SqlDbType.Int).Value = productId;

                    sqlConnection.Open();
                    isUserInWaitlist = sqlCommand.ExecuteScalar() != null;
                    return isUserInWaitlist;
                }
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
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("GetUserWaitlistPosition", sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    sqlCommand.Parameters.Add("@ProductID", SqlDbType.Int).Value = productId;

                    SqlParameter positionOutputParameter = new SqlParameter("@Position", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    sqlCommand.Parameters.Add(positionOutputParameter);

                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();

                    if (positionOutputParameter.Value != DBNull.Value)
                    {
                        return (int)positionOutputParameter.Value;
                    }
                    else
                    {
                        return -1;
                    }
                }
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

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("GetOrderedWaitlistUsers", sqlConnection))
                {
                    sqlCommand.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;

                    sqlConnection.Open();

                    using (SqlDataReader sQLDataReader = sqlCommand.ExecuteReader())
                    {
                        while (sQLDataReader.Read())
                        {
                            var waitListUser = new UserWaitList
                            {
                                ProductWaitListID = sQLDataReader.GetInt32(sQLDataReader.GetOrdinal("productWaitListID")),
                                UserID = sQLDataReader.GetInt32(sQLDataReader.GetOrdinal("userID")),
                                JoinedTime = sQLDataReader.GetDateTime(sQLDataReader.GetOrdinal("joinedTime")),
                                PositionInQueue = sQLDataReader.GetInt32(sQLDataReader.GetOrdinal("positionInQueue"))
                            };
                            orderedWaitlistUsers.Add(waitListUser);
                        }
                    }
                }
            }

            return orderedWaitlistUsers;
        }
    }
}