﻿using System;

namespace ArtAttack.Domain
{
    public class UserWaitList
    {
        public int UserWaitListID { get; set; }
        public int ProductWaitListID { get; set; }
        public int UserID { get; set; }
        public DateTime JoinedTime { get; set; }
        public int PositionInQueue { get; set; }
    }
}
