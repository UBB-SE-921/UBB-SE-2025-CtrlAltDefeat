
GO
CREATE PROCEDURE MarkNotificationAsRead (
    @notificationID INT
) AS
BEGIN
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM [Notification] WHERE notificationID = @notificationID)
        BEGIN
            RAISERROR('Notification with ID %d does not exist', 16, 1, @notificationID);
            RETURN;
        END

        UPDATE [Notification] 
        SET isRead = 1 
        WHERE notificationID = @notificationID;
        
    END TRY
    BEGIN CATCH
        DECLARE @err_message VARCHAR(2000)
        DECLARE @err_number INT
        SELECT @err_message = ERROR_MESSAGE(), @err_number = ERROR_NUMBER()
        RAISERROR(N'usp_MarkNotificationAsRead: Error %i: %s', 16, 1, @err_number, @err_message)
    END CATCH
END
GO