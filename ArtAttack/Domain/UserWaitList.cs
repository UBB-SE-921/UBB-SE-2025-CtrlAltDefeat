using System;


namespace ArtAttack.Domain
{
    public class UserWaitList
    {
        public int userWaitListID { get; set; }
        public int productWaitListID { get; set; }
        public int userID { get; set; }
        public DateTime joinedTime { get; set; }
        public int positionInQueue { get; set; }

        /*public void MoveUpInQueue()
        {
            if (positionInQueue <= 1)
                throw new DomainException("Already at top position");

            positionInQueue--;
        }*/
    }
}
