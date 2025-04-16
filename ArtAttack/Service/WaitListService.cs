using System;
using System.Collections.Generic;
using System.Linq;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Repository;
using ArtAttack.Shared;

namespace ArtAttack.Service
{
    public class WaitListService : IWaitListService
    {
        private readonly IWaitListRepository waitListRepository;
        private readonly INotificationDataAdapter notificationAdapter;

        public WaitListService(string connectionString)
        {
            waitListRepository = new WaitListRepository(connectionString);
            notificationAdapter = new NotificationDataAdapter(connectionString);
        }

        public WaitListService(IWaitListRepository waitListRepository, INotificationDataAdapter notificationAdapter)
        {
            this.waitListRepository = waitListRepository;
            this.notificationAdapter = notificationAdapter;
        }

        public void AddUserToWaitlist(int userId, int productId)
        {
            waitListRepository.AddUserToWaitlist(userId, productId);
        }

        public void RemoveUserFromWaitlist(int userId, int productId)
        {
            waitListRepository.RemoveUserFromWaitlist(userId, productId);
        }

        public List<UserWaitList> GetUsersInWaitlist(int waitListProductId)
        {
            return waitListRepository.GetUsersInWaitlist(waitListProductId);
        }

        public List<UserWaitList> GetUserWaitlists(int userId)
        {
            return waitListRepository.GetUserWaitlists(userId);
        }

        public int GetWaitlistSize(int productWaitListId)
        {
            return waitListRepository.GetWaitlistSize(productWaitListId);
        }

        public bool IsUserInWaitlist(int userId, int productWaitListId)
        {
            return waitListRepository.IsUserInWaitlist(userId, productWaitListId);
        }

        public int GetUserWaitlistPosition(int userId, int productId)
        {
            return waitListRepository.GetUserWaitlistPosition(userId, productId);
        }

        public void ScheduleRestockAlerts(int productId, DateTime restockDate)
        {
            int waitlistProductId = waitListRepository.GetWaitlistProductId(productId);
            if (waitlistProductId <= 0)
            {
                return;
            }

            var waitlistUsers = waitListRepository.GetUsersInWaitlist(waitlistProductId)
                     .OrderBy(u => u.PositionInQueue)
                     .ToList();

            for (int userIndex = 0; userIndex < waitlistUsers.Count; userIndex++)
            {
                var notification = new ProductAvailableNotification(
                    recipientId: waitlistUsers[userIndex].UserID,
                    timestamp: CalculateNotifyTime(restockDate, userIndex),
                    productId: productId,
                    isRead: false);

                notificationAdapter.AddNotification(notification);
            }
        }

        private DateTime CalculateNotifyTime(DateTime restockDate, int positionInQueue)
        {
            return positionInQueue switch
            {
                0 => restockDate.AddHours(-48), // First in queue
                1 => restockDate.AddHours(-24), // Second in queue
                _ => restockDate.AddHours(-12) // Everyone else
            };
        }

        private string GetNotificationMessage(int positionInQueue, DateTime restockDate)
        {
            string timeDescription = (restockDate - DateTime.Now).TotalHours > 24
                ? $"on {restockDate:MMM dd}"
                : $"in {(int)(restockDate - DateTime.Now).TotalHours} hours";

            return positionInQueue switch
            {
                1 => $"You're FIRST in line! Product restocking {timeDescription}.",
                2 => $"You're SECOND in line. Product restocking {timeDescription}.",
                _ => $"You're #{positionInQueue} in queue. Product restocking {timeDescription}."
            };
        }
    }
}