GO
CREATE PROCEDURE GetNotificationsByRecipient (
    @recipientID INT
) AS
BEGIN
    BEGIN TRY
        -- Return all notifications for the given recipient
        SELECT 
            notificationID,
            recipientID,
            category,
            timestamp,
            isRead,
            contractID,
            isAccepted,
            productID,
            orderID,
            shippingState,
            deliveryDate,
            expirationDate
        FROM 
            [Notification]
        WHERE 
            recipientID = @recipientID
        ORDER BY 
            timestamp DESC;  -- Newest notifications first
    END TRY
    BEGIN CATCH
        DECLARE @err_message VARCHAR(2000)
        DECLARE @err_number INT
        SELECT @err_message = ERROR_MESSAGE(), @err_number = ERROR_NUMBER()
        RAISERROR(N'GetNotificationsByRecipient: Error %i: %s', 16, 1, @err_number, @err_message)
    END CATCH
END
GO
