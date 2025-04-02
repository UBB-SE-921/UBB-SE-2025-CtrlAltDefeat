using ArtAttack.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ArtAttack.Model
{
    public class WaitListModel : IWaitListModel
    {
        private readonly string _connectionString;

        public WaitListModel(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Adds a user to the waitlist for a specific product.
        /// </summary>
        /// <param name="userId">The ID of the user to be added to the waitlist. Must be a positive integer.</param>
        /// <param name="productWaitListId">The ID of the product waitlist. Must be a positive integer.</param>
        /// <exception cref="SqlException">Thrown when there is an error executing the SQL command.</exception>
        public void AddUserToWaitlist(int userId, int productWaitListId)
        {
            using (SqlConnection SQLconnection = new SqlConnection(_connectionString))
            {
                using (SqlCommand SQLcommand = new SqlCommand("AddUserToWaitlist", SQLconnection))
                {
                    SQLcommand.CommandType = CommandType.StoredProcedure;
                    SQLcommand.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    SQLcommand.Parameters.Add("@ProductWaitListID", SqlDbType.Int).Value = productWaitListId;

                    SQLconnection.Open();
                    SQLcommand.ExecuteNonQuery();
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
            using (SqlConnection SQLconnection = new SqlConnection(_connectionString))
            {
                using (SqlCommand SQLcommand = new SqlCommand("RemoveUserFromWaitlist", SQLconnection))
                {
                    SQLcommand.CommandType = CommandType.StoredProcedure;
                    SQLcommand.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    SQLcommand.Parameters.Add("@ProductWaitListID", SqlDbType.Int).Value = productWaitListId;

                    SQLconnection.Open();
                    SQLcommand.ExecuteNonQuery();
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

            using (SqlConnection SQLconnection = new SqlConnection(_connectionString))
            {
                using (SqlCommand SQLcommand = new SqlCommand("GetUsersInWaitlist", SQLconnection))
                {
                    SQLcommand.CommandType = CommandType.StoredProcedure;
                    SQLcommand.Parameters.Add("@WaitListProductID", SqlDbType.BigInt).Value = waitListProductId;

                    SQLconnection.Open();

                    using (SqlDataReader SQLDataReader = SQLcommand.ExecuteReader())
                    {
                        while (SQLDataReader.Read())
                        {
                            var userWaitListEntry = new UserWaitList
                            {
                                userID = SQLDataReader.GetInt32(SQLDataReader.GetOrdinal("userID")),
                                positionInQueue = SQLDataReader.GetInt32(SQLDataReader.GetOrdinal("positionInQueue")),
                                joinedTime = SQLDataReader.GetDateTime(SQLDataReader.GetOrdinal("joinedTime"))
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

            using (SqlConnection SQLconnection = new SqlConnection(_connectionString))
            {
                using (SqlCommand SQLcommand = new SqlCommand("GetUserWaitlists", SQLconnection))
                {
                    SQLcommand.CommandType = CommandType.StoredProcedure;
                    SQLcommand.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

                    SQLconnection.Open();

                    using (SqlDataReader SQLDataReader = SQLcommand.ExecuteReader())
                    {
                        while (SQLDataReader.Read())
                        {
                            var userWaitlist = new UserWaitList
                            {
                                userID = userId,
                                productWaitListID = SQLDataReader.GetInt32(SQLDataReader.GetOrdinal("productWaitListID")),
                                positionInQueue = SQLDataReader.GetInt32(SQLDataReader.GetOrdinal("positionInQueue")),
                                joinedTime = SQLDataReader.GetDateTime(SQLDataReader.GetOrdinal("joinedTime"))
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
            using (SqlConnection SQLconnection = new SqlConnection(_connectionString))
            {
                using (SqlCommand SQLcommand = new SqlCommand("GetWaitlistSize", SQLconnection))
                {
                    SQLcommand.CommandType = CommandType.StoredProcedure;
                    SQLcommand.Parameters.Add("@ProductWaitListID", SqlDbType.Int).Value = productWaitListId;
                    SqlParameter totalUsersParameter = new SqlParameter("@TotalUsers", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    SQLcommand.Parameters.Add(totalUsersParameter);

                    SQLconnection.Open();
                    SQLcommand.ExecuteNonQuery();

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
            using (SqlConnection SQLconnection = new SqlConnection(_connectionString))
            {
                using (SqlCommand SQLcommand = new SqlCommand("CheckUserInProductWaitlist", SQLconnection))
                {
                    SQLcommand.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    SQLcommand.Parameters.Add("@ProductID", SqlDbType.Int).Value = productId;

                    SQLconnection.Open();
                    isUserInWaitlist = SQLcommand.ExecuteScalar() != null;
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
            using (SqlConnection SQLconnection = new SqlConnection(_connectionString))
            {
                using (SqlCommand SQLcommand = new SqlCommand("GetUserWaitlistPosition", SQLconnection))
                {
                    SQLcommand.CommandType = CommandType.StoredProcedure;
                    SQLcommand.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;
                    SQLcommand.Parameters.Add("@ProductID", SqlDbType.Int).Value = productId;

                    SqlParameter positionOutputParameter = new SqlParameter("@Position", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    SQLcommand.Parameters.Add(positionOutputParameter);

                    SQLconnection.Open();
                    SQLcommand.ExecuteNonQuery();

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

            using (SqlConnection SQLconnection = new SqlConnection(_connectionString))
            {
                using (SqlCommand SQLcommand = new SqlCommand("GetOrderedWaitlistUsers", SQLconnection))
                {
                    SQLcommand.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;

                    SQLconnection.Open();

                    using (SqlDataReader SQLDataReader = SQLcommand.ExecuteReader())
                    {
                        while (SQLDataReader.Read())
                        {
                            var waitListUser = new UserWaitList
                            {
                                productWaitListID = SQLDataReader.GetInt32(SQLDataReader.GetOrdinal("productWaitListID")),
                                userID = SQLDataReader.GetInt32(SQLDataReader.GetOrdinal("userID")),
                                joinedTime = SQLDataReader.GetDateTime(SQLDataReader.GetOrdinal("joinedTime")),
                                positionInQueue = SQLDataReader.GetInt32(SQLDataReader.GetOrdinal("positionInQueue"))
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
