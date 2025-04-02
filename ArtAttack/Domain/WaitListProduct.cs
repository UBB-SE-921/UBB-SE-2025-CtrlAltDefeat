using System;

namespace ArtAttack.Domain
{
    public class WaitListProduct
    {
        public int WaitListProductID { get; private set; }
        public int ProductID { get; private set; }
        public DateTime AvailableAgain { get; private set; }

        public void UpdateAvailability(DateTime newAvailableDate)
        {
            if (newAvailableDate < this.AvailableAgain)
            {
                throw new ArgumentException("Available date cannot be updated with a past date");
            }

            AvailableAgain = newAvailableDate;
        }
    }
}
