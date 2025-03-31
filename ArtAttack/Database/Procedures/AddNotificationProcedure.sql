GO
CREATE PROCEDURE AddNotification (
    @recipientID INT,
    @category VARCHAR(25),
    @contractID INT = NULL,
    @isAccepted BIT = NULL,
    @productID INT = NULL,
    @orderID INT = NULL,
    @shippingState VARCHAR(25) = NULL,
    @deliveryDate DATETIME = NULL,
    @expirationDate DATETIME = NULL
) AS
BEGIN
    BEGIN TRY
        -- Validate the category
        IF @category NOT IN (
            'CONTRACT_EXPIRATION', 'OUTBIDDED', 'ORDER_SHIPPING_PROGRESS',
            'PRODUCT_AVAILABLE', 'PAYMENT_CONFIRMATION', 'PRODUCT_REMOVED',
            'CONTRACT_RENEWAL_REQ', 'CONTRACT_RENEWAL_ANS', 'CONTRACT_RENEWAL_WAITLIST'
        )
        BEGIN
            RAISERROR('Invalid notification category: %s', 16, 1, @category);
            RETURN;
        END
        
        -- Insert the notification
        INSERT INTO [Notification] (
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
        )
        VALUES (
            @recipientID,
            @category,
            GETDATE(),
            0,
            @contractID,
            @isAccepted,
            @productID,
            @orderID,
            @shippingState,
            @deliveryDate,
            @expirationDate
        );
    END TRY
    BEGIN CATCH
        DECLARE @err_message VARCHAR(2000)
        DECLARE @err_number INT
        SELECT @err_message = ERROR_MESSAGE(), @err_number = ERROR_NUMBER()
        RAISERROR(N'AddNotification: Error %i: %s', 16, 1, @err_number, @err_message)
    END CATCH
END
GO

