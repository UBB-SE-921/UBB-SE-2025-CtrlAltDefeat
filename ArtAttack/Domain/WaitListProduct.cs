using System;

namespace ArtAttack.Domain
{
    public class WaitListProduct
    {
        public int waitListProductID { get; private set; }
        public int productID { get; private set; }
        public DateTime availableAgain { get; private set; }

        public void UpdateAvailability(DateTime newAvailableDate)
        {
            if (newAvailableDate < this.availableAgain)
            {
                throw new ArgumentException("Available date cannot be updated with a past date");
            }

            availableAgain = newAvailableDate;
        }

    }
}
