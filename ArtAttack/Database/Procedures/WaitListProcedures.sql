
--procedure to add a new user to the waitlist of a product
--procedure to delete a user from a waitlist of a product
-- Select all users in the waitlist for the given product
-- Select all waitlists the user has joined



create or alter procedure AddUserToWaitlist
    @UserID int,
    @ProductID int
as
begin
    set nocount on;

     DECLARE @WaitListProductID INT;
        
     SELECT @WaitListProductID = waitListProductID 
     FROM WaitListProduct 
     WHERE productID = @ProductID;

    declare @NextPosition int;
    select @NextPosition = isnull(max(positionInQueue), 0) + 1
    from UserWaitList
    where productWaitListID = @WaitListProductID;

    insert into UserWaitList (productWaitListID, userID, joinedTime, positionInQueue)
    values (@WaitListProductID, @UserID, getdate(), @NextPosition);
end;
go

CREATE OR ALTER PROCEDURE GetUserWaitlistPosition
    @UserID INT,
    @ProductID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get the user's position in the waitlist for the specified product
    SELECT uw.positionInQueue
    FROM UserWaitList uw
    JOIN WaitListProduct wp ON uw.productWaitListID = wp.waitListProductID
    WHERE uw.userID = @UserID AND wp.productID = @ProductID;
    
    -- If no rows returned, the user isn't on this waitlist
    -- The C# code will handle this as position -1
END
GO

--procedure to delete a user from a given waitList
create or ALTER PROCEDURE RemoveUserFromWaitlist
    @UserID INT,
    @ProductID INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @WaitListProductID INT;
    DECLARE @UserPosition INT;

    SELECT @WaitListProductID = waitListProductID 
    FROM WaitListProduct 
    WHERE productID = @ProductID;

    IF @WaitListProductID IS NULL
    BEGIN
        PRINT 'No matching product found in the waitlist.';
        RETURN;
    END

    SELECT @UserPosition = positionInQueue
    FROM UserWaitList
    WHERE userID = @UserID AND productWaitListID = @WaitListProductID;

    IF @UserPosition IS NULL
    BEGIN
        PRINT 'User not found in the waitlist.';
        RETURN;
    END

    DELETE FROM UserWaitList
    WHERE userID = @UserID AND productWaitListID = @WaitListProductID;

    UPDATE UserWaitList
    SET positionInQueue = positionInQueue - 1
    WHERE productWaitListID = @WaitListProductID AND positionInQueue > @UserPosition;

    PRINT 'User removed and positions updated successfully.';
END;

        
-- Select all users in the waitlist for the given product
go
create or alter PROCEDURE GetUsersInWaitlist
    @ProductID INT
AS
BEGIN
    DECLARE @WaitListProductID INT;
        
     SELECT @WaitListProductID = waitListProductID 
     FROM WaitListProduct 
     WHERE productID = @ProductID;

    SELECT
        UserWaitList.[userID],
        UserWaitList.[positionInQueue],
        UserWaitList.[joinedTime]
    FROM [UserWaitList] UserWaitList
    WHERE UserWaitList.[productWaitListID] = @WaitListProductID
    ORDER BY UserWaitList.[positionInQueue] ASC;
END;


 -- Select all waitlists the user has joined
 go

 create or alter procedure GetOrderedWaitlistUsers
    @ProductId int
as
begin
    SET NOCOUNT on;
    
    select 
        uw.productWaitListID,
        uw.userID,
        uw.joinedTime,
        uw.positionInQueue
    from UserWaitList uw
    join WaitListProduct wp ON uw.productWaitListID = wp.WaitListProductID
    where wp.ProductID = @ProductId
    order BY uw.positionInQueue asc;
end;
go

create or alter procedure CheckUserInProductWaitlist
    @UserID int,
    @ProductID int
as
begin
    set nocount on;
    
    select 1 AS IsInWaitlist
    from UserWaitList uw
    join WaitListProduct wp ON uw.productWaitListID = wp.waitListProductID
    where uw.userID = @UserID AND wp.productID = @ProductID;
END;
GO


