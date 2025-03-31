Drop procedure if exists GetContractByID
Drop procedure if exists GetAllContracts
Drop procedure if exists GetContractHistory
Drop procedure if exists GetContractBuyer
Drop procedure if exists GetContractSeller
Drop procedure if exists AddContract
Drop procedure if exists GetOrderSummaryInformation
Drop procedure if exists GetPredefinedContractByID
Drop procedure if exists GetProductDetailsByContractID
Drop procedure if exists GetContractsByBuyer
Drop procedure if exists GetOrderDetails
Drop procedure if exists GetDeliveryDateByContractID
Drop procedure if exists GetPdfByContractID
drop procedure if exists AddNotification
drop procedure if exists DeleteNotification
drop procedure if exists uspDeleteOrderCheckpoint
drop procedure if exists uspDeleteTrackedOrder
drop procedure if exists AddCard
drop procedure if exists DeleteCard
drop procedure if exists UpdateCardBalance
drop procedure if exists GetBalance
drop trigger if exists tr_CreateWaitListForProduct
drop procedure if exists UpdateDummyProduct
drop procedure if exists DeleteDummyProduct
drop procedure if exists AddDummyProduct
drop procedure if exists AddWallet
drop procedure if exists DeleteWallet
drop procedure if exists UpdateWalletBalance
drop procedure if exists GetWalletBalance
drop procedure if exists GetNotificationsByRecipient
drop procedure if exists uspInsertOrderCheckpoint
drop procedure if exists uspInsertTrackedOrder
drop procedure if exists MarkNotificationAsRead
drop proc if exists [GetDummyProductsFromOrderHistory]
drop procedure if exists AddOrderSummary
drop procedure if exists DeleteOrderSummary
drop procedure if exists UpdateOrderSummary
drop procedure if exists AddOrder
drop procedure if exists DeleteOrder
drop procedure if exists UpdateOrder
drop procedure if exists get_orders_from_order_history
drop proc if exists get_borrowed_order_history
drop proc if exists get_new_or_used_order_history
drop proc if exists get_products_from_order_history
drop proc if exists get_orders_from_last_3_months
drop proc if exists get_orders_from_last_6_months
drop proc if exists get_orders_from_2025
drop proc if exists get_orders_from_2024
drop proc if exists get_orders_by_name
drop procedure if exists AddRenewedContract
drop procedure if exists UpdateRenewedContract
drop procedure if exists GetRenewedContracts
drop proc if exists uspUpdateOrderCheckpoint
drop proc if exists uspUpdateTrackedOrder
drop proc if exists AddUserToWaitlist
drop proc if exists GetUserWaitlistPosition
drop proc if exists RemoveUserFromWaitlist
drop proc if exists GetUsersInWaitlist
drop proc if exists GetOrderedWaitlistUsers
drop proc if exists CheckUserInProductWaitlist
Go

CREATE PROCEDURE GetContractByID
    @ContractID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM Contract
    WHERE ID = @ContractID;
END
GO

CREATE PROCEDURE GetPredefinedContractByID
    @PContractID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM PredefinedContract
    WHERE ID = @PContractID;
END
GO

CREATE PROCEDURE GetAllContracts
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM Contract;
END
GO


CREATE PROCEDURE GetContractHistory
    @ContractID INT
AS
BEGIN
    SET NOCOUNT ON;

    WITH ContractHistory AS
    (
        -- Start with the given contract
        SELECT *
        FROM Contract
        WHERE ID = @ContractID

        UNION ALL

        -- Recursively get the original contract from which this contract was renewed
        SELECT c.*
        FROM Contract c
        INNER JOIN ContractHistory ch ON c.ID = ch.renewedFromContractID
    )
    SELECT *
    FROM ContractHistory;
END
GO

CREATE PROCEDURE AddContract
    @OrderID INT,
    @ContractStatus VARCHAR(255),
    @ContractContent TEXT,
    @RenewalCount INT,
    @PredefinedContractID INT = NULL,
    @PDFID INT,
    @PDFFile VARBINARY(MAX),
    @AdditionalTerms TEXT = NULL,
    @RenewedFromContractID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Update the PDF table with the new file content.
        UPDATE PDF
        SET [file] = @PDFFile
        WHERE ID = @PDFID;
        
        -- Insert the new contract record.
        INSERT INTO Contract
            (orderID, contractStatus, contractContent, renewalCount, predefinedContractID, pdfID, AdditionalTerms, renewedFromContractID)
        VALUES
            (@OrderID, @ContractStatus, @ContractContent, @RenewalCount, @PredefinedContractID, @PDFID, @AdditionalTerms,@RenewedFromContractID);
        
        DECLARE @NewContractID BIGINT;
        SET @NewContractID = SCOPE_IDENTITY();

        -- Return the newly added contract record.
        SELECT 
            ID,
            orderID,
            contractStatus,
            contractContent,
            renewalCount,
            predefinedContractID,
            pdfID,
            renewedFromContractID
        FROM Contract
        WHERE ID = @NewContractID;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        -- Rethrow the error to the caller.
        THROW;
    END CATCH
END
GO

CREATE PROCEDURE GetContractSeller
    @ContractID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ds.ID AS SellerID,
           ds.name AS SellerName
    FROM Contract c
    INNER JOIN [Order] o ON c.orderID = o.OrderID
    INNER JOIN DummyProduct dp ON o.ProductId = dp.ID
    INNER JOIN DummySeller ds ON dp.SellerID = ds.ID
    WHERE c.ID = @ContractID;
END
GO

CREATE PROCEDURE GetContractBuyer
    @ContractID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT db.ID AS BuyerID,
           db.name AS BuyerName
    FROM Contract c
    INNER JOIN [Order] o ON c.orderID = o.OrderID
    INNER JOIN DummyBuyer db ON o.BuyerId = db.ID
    WHERE c.ID = @ContractID;
END
GO

CREATE PROCEDURE GetOrderSummaryInformation
    @ContractID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT os.*
    FROM Contract c
    INNER JOIN [Order] o ON c.orderID = o.OrderID
    INNER JOIN OrderSummary os ON o.OrderSummaryID = os.ID
    WHERE c.ID = @ContractID;
END
GO

CREATE PROCEDURE GetOrderDetails @ContractID INT
AS
BEGIN
    SET NOCOUNT ON
    SELECT o.PaymentMethod, o.OrderDate
    FROM [Contract] c
    INNER JOIN [Order] o on c.orderID = o.OrderID
    WHERE c.ID = @ContractID
END
GO

CREATE PROCEDURE GetProductDetailsByContractID
    @ContractID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT dp.startDate, dp.endDate, dp.price, dp.name
    FROM Contract c
    INNER JOIN [Order] o ON c.orderID = o.OrderID
    INNER JOIN DummyProduct dp ON o.ProductId = dp.ID
    WHERE c.ID = @ContractID;
END
GO

CREATE PROCEDURE GetContractsByBuyer
    @BuyerID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.*
    FROM Contract c
    INNER JOIN [Order] o ON c.orderID = o.OrderID
    WHERE o.BuyerID = @BuyerID;
END
GO


CREATE PROCEDURE GetDeliveryDateByContractID
    @ContractID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT t.EstimatedDeliveryDate
    FROM [Contract] c
    INNER JOIN TrackedOrders t ON c.OrderID = t.TrackedOrderID
    WHERE c.ID = @ContractID;
END;
GO

CREATE PROCEDURE GetPdfByContractID
    @ContractID BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT p.[file] AS PdfFile
    FROM [Contract] c
    INNER JOIN [PDF] p ON c.pdfID = p.ID
    WHERE c.ID = @ContractID;
END
GO

-- Get a specific contract by ID
EXEC GetContractByID @ContractID = 1;
GO

--Retrieve predefined contract
exec GetPredefinedContractByID @PContractID = 3
GO
-- Retrieve all contracts
EXEC GetAllContracts;
GO

-- Retrieve the complete contract renewal history (tracing through renewedFromContractID)
EXEC GetContractHistory @ContractID = 1;
GO

-- Add a new contract (including updating the PDF table)
-- Declare a sample binary value for the PDF file
DECLARE @SamplePDF VARBINARY(MAX) = 0x255044462D312E350D0A; -- Represents "%PDF-1.5" in hex

EXEC AddContract 
    @OrderID = 1,
    @ContractStatus = 'ACTIVE',
    @ContractContent = 'This is a sample contract content.',
    @RenewalCount = 0,
    @PredefinedContractID = NULL,    -- Optional parameter (set to NULL if not used)
    @PDFID = 1,
    @PDFFile = @SamplePDF,
    @AdditionalTerms = NULL,
    @RenewedFromContractID = NULL;     -- Set to a valid contract ID if this is a renewal, otherwise NULL
GO

--Get seller information related to a contract
EXEC GetContractSeller @ContractID = 2;
GO

--Get buyer information related to a contract
EXEC GetContractBuyer @ContractID = 2;
GO

--Get the order summary information for a contract
EXEC GetOrderSummaryInformation @ContractID = 2;
GO


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

GO
CREATE PROCEDURE DeleteNotification (
    @notificationID INT
) AS
BEGIN
    BEGIN TRY
        -- Check if notification exists first
        IF NOT EXISTS (SELECT 1 FROM [Notification] WHERE notificationID = @notificationID)
        BEGIN
            RAISERROR('Notification with ID %d does not exist', 16, 1, @notificationID);
            RETURN;
        END
        
        -- Delete the notification
        DELETE FROM [Notification] 
        WHERE notificationID = @notificationID;
    END TRY
    BEGIN CATCH
        DECLARE @err_message VARCHAR(2000)
        DECLARE @err_number INT
        SELECT @err_message = ERROR_MESSAGE(), @err_number = ERROR_NUMBER()
        RAISERROR(N'DeleteNotification: Error %i: %s', 16, 1, @err_number, @err_message)
    END CATCH
END
GO


go
create procedure uspDeleteOrderCheckpoint (@checkpointID bigint) as
begin
	begin try
		delete from OrderCheckpoints
		where [CheckpointID] = @checkpointID
	end try
	begin catch
		declare @err_message varchar(2000)
		declare @err_number int
		select @err_message = error_message(), @err_number = error_number()
		raiserror(N'uspDeleteOrderCheckpoint: Error %i: %s', 16, 1, @err_number, @err_message)
	end catch
end
go

create or alter proc [AddCard] 
@cname varchar(50),
@cnumber varchar(20),
@cvc varchar(3),
@mon varchar(2), 
@yr varchar(2),
@country varchar(30),
@balance float as
begin 
insert into [DummyCard] 
values (@cname,@cnumber,@cvc,@mon,@yr,@country,@balance)
end
go


create or alter proc [DeleteCard] @cardnumber varchar(20) as
begin

 delete from [DummyCard] where [cardNumber]=@cardnumber

end
go

create or alter proc [UpdateCardBalance]
@cnumber varchar(20),
@balance float
as
begin
update [DummyCard] 
set [balance]=@balance
where [cardNumber]=@cnumber
end
go

create or alter proc [GetBalance]
@cnumber varchar(20)
as
begin
select [balance] from [DummyCard] where [cardNumber]=@cnumber
end
go


CREATE OR ALTER TRIGGER tr_CreateWaitListForProduct
ON DummyProduct
AFTER INSERT, UPDATE
AS
BEGIN
    INSERT INTO WaitListProduct (productID, availableAgain)
    SELECT i.ID, i.endDate
    FROM inserted i
    LEFT JOIN WaitListProduct wp ON i.ID = wp.productID
    WHERE i.endDate IS NOT NULL
      AND wp.waitListProductID IS NULL;
END
GO

CREATE PROCEDURE UpdateDummyProduct
    @ID INT,
    @Name VARCHAR(64),
    @Price FLOAT,
    @SellerID INT,
    @ProductType VARCHAR(20),
    @StartDate DATETIME,
    @EndDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE DummyProduct
        SET [Name] = @Name,
            Price = @Price,
            SellerID = @SellerID,
            ProductType = @ProductType,
            StartDate = @StartDate,
            EndDate = @EndDate
        WHERE ID = @ID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE PROCEDURE DeleteDummyProduct
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM DummyProduct
        WHERE ID = @ID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE PROCEDURE AddDummyProduct
    @Name VARCHAR(64),
    @Price FLOAT,
    @SellerID INT,
    @ProductType VARCHAR(20),
    @StartDate DATETIME,
    @EndDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO DummyProduct ([Name], Price, SellerID, ProductType, StartDate, EndDate)
        VALUES (@Name, @Price, @SellerID, @ProductType, @StartDate, @EndDate);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


create or alter proc [AddWallet] 
@balance float as

begin 
insert into [DummyWallet] 
values (@balance)
end
go




create or alter proc [DeleteWallet] @id int as
begin
 delete from [DummyWallet] where [ID]=@id
end
go

create or alter proc [UpdateWalletBalance]
@id int,
@balance float
as
begin
update [DummyWallet] 
set [balance]=@balance
where [ID]=@id
end
go


create or alter proc [GetWalletBalance]
@id int
as
begin
select balance from [DummyWallet] where [ID]=@ID
end
go

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


go
create procedure uspInsertOrderCheckpoint (@timestamp datetime, @location varchar(255), @description varchar(255), @checkpointStatus varchar(100), @trackedOrderID int) as
begin
	begin try
		insert into OrderCheckpoints values(@timestamp, @location, @description, @checkpointStatus, @trackedOrderID)
		update TrackedOrders
		set OrderStatus = @checkpointStatus
		where TrackedOrderID = @trackedOrderID
	end try
	begin catch
		declare @err_message varchar(2000)
		declare @err_number int
		select @err_message = error_message(), @err_number = error_number()
		raiserror(N'uspInsertOrderCheckpoint: Error %i: %s', 16, 1, @err_number, @err_message)
	end catch
end
go

go
alter procedure uspInsertOrderCheckpoint 
    @timestamp datetime, 
    @location varchar(255), 
    @description varchar(255), 
    @checkpointStatus varchar(100), 
    @trackedOrderID int,
    @newCheckpointID int output
as
begin
    begin try
        insert into OrderCheckpoints (Timestamp, Location, Description, CheckpointStatus, TrackedOrderID) 
        output inserted.CheckpointID 
        values (@timestamp, @location, @description, @checkpointStatus, @trackedOrderID)

        set @newCheckpointID = scope_identity()
    end try
    begin catch
        declare @err_message varchar(2000)
        declare @err_number int
        select @err_message = error_message(), @err_number = error_number()
        raiserror(N'uspInsertOrderCheckpoint: Error %i: %s', 16, 1, @err_number, @err_message)
        set @newCheckpointID = -1
    end catch
end
go


go
create procedure uspInsertTrackedOrder (@estimatedDeliveryDate date, @deliveryAddress varchar(255), @orderStatus varchar(100), @orderID bigint) as
begin
	begin try
		insert into TrackedOrders values (@estimatedDeliveryDate, @deliveryAddress, @orderStatus, @orderID)
	end try
	begin catch
		declare @err_message varchar(2000)
		declare @err_number int
		select @err_message = error_message(), @err_number = error_number()
		raiserror(N'uspInsertTrackedOrder: Error %i: %s', 16, 1, @err_number, @err_message)
	end catch
end
go

go
alter procedure uspInsertTrackedOrder 
    @estimatedDeliveryDate date, 
    @deliveryAddress varchar(255), 
    @orderStatus varchar(100), 
    @orderID bigint,
    @newTrackedOrderID int output
as
begin
    begin try
        insert into TrackedOrders (EstimatedDeliveryDate, DeliveryAddress, OrderStatus, OrderID) 
        output inserted.TrackedOrderID 
        values (@estimatedDeliveryDate, @deliveryAddress, @orderStatus, @orderID)

        set @newTrackedOrderID = scope_identity()
    end try
    begin catch
        declare @err_message varchar(2000)
        declare @err_number int
        select @err_message = error_message(), @err_number = error_number()
        raiserror(N'uspInsertTrackedOrder: Error %i: %s', 16, 1, @err_number, @err_message)
        set @newTrackedOrderID = -1
    end catch
end
go

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


go
create or alter proc [GetDummyProductsFromOrderHistory] @orderHistory int 
as
begin
    select 
        o.OrderID, p.ID as productID, p.name, p.price, p.SellerID, p.productType,p.startDate, p.endDate ,o.BuyerID, o.PaymentMethod, o.OrderDate
    from [Order] o join [DummyProduct] p on o.ProductID = p.ID where o.OrderHistoryID = @orderHistory

end

go

create or alter procedure get_borrowed_order_history @BuyerID int
as
begin
    select o.[OrderID], o.[ProductID], o.[ProductType], o.[PaymentMethod], 
    o.[OrderDate], os.[Subtotal], os.[WarrantyTax], os.[DeliveryFee], 
    os.[finalTotal], os.[address], os.[AdditionalInfo], os.[ContractDetails] , p.[name]
    from [Order] o inner join [OrderSummary] os on o.[OrderSummaryID]=os.[ID]
    inner join [DummyProduct] p on o.[ProductID]=p.[ID]
    where p.[ProductType]='borrowed' and o.[BuyerID]=@BuyerID order by o.[OrderDate] desc;

end

go

create or alter procedure get_new_or_used_order_history @BuyerID int
as
begin
    select o.[OrderID], o.[ProductID], o.[ProductType], o.[PaymentMethod], 
    o.[OrderDate], os.[Subtotal], os.[DeliveryFee], 
    os.[finalTotal], os.[address], os.[AdditionalInfo], p.[name]
    from [Order] o inner join [OrderSummary] os on o.[OrderSummaryID]=os.[ID]
    inner join [DummyProduct] p on o.[ProductID]=p.[ID]
    where p.[ProductType]='new' or o.[ProductType]='used' and o.[BuyerID]=@BuyerID order by o.[OrderDate] desc;

end
go

create procedure get_products_from_order_history @OrderHistoryID int
as
begin
    select * from [Order] o
    where o.[OrderHistoryID]=@OrderHistoryID;

end
go



create procedure get_orders_from_last_3_months @BuyerID int
as
begin
    select o.[OrderID], o.[ProductID], o.[ProductType], o.[PaymentMethod], 
    o.[OrderDate], os.[Subtotal], os.[WarrantyTax], os.[DeliveryFee], 
    os.[finalTotal], os.[address], os.[AdditionalInfo], os.[ContractDetails] , p.[name]
    from [Order] o inner join [OrderSummary] os on o.[OrderSummaryID]=os.[ID]
    inner join [DummyProduct] p on o.[ProductID]=p.[ID]
    where o.[BuyerID]=@BuyerID and o.[OrderDate]>=dateadd(month, -3, getdate()) order by o.[OrderDate] desc;

end
go

create procedure get_orders_from_last_6_months @BuyerID int
as
begin
    select o.[OrderID], o.[ProductID], o.[ProductType], o.[PaymentMethod], 
    o.[OrderDate], os.[Subtotal], os.[WarrantyTax], os.[DeliveryFee], 
    os.[finalTotal], os.[address], os.[AdditionalInfo], os.[ContractDetails] , p.[name]
    from [Order] o inner join [OrderSummary] os on o.[OrderSummaryID]=os.[ID]
    inner join [DummyProduct] p on o.[ProductID]=p.[ID]
    where o.[BuyerID]=@BuyerID and o.[OrderDate]>=dateadd(month, -6, getdate()) order by o.[OrderDate] desc;

end
go

create procedure get_orders_from_2025 @BuyerID int
as
begin
    select o.[OrderID], o.[ProductID], o.[ProductType], o.[PaymentMethod], 
    o.[OrderDate], os.[Subtotal], os.[WarrantyTax], os.[DeliveryFee], 
    os.[finalTotal], os.[address], os.[AdditionalInfo], os.[ContractDetails] , p.[name]
    from [Order] o inner join [OrderSummary] os on o.[OrderSummaryID]=os.[ID]
    inner join [DummyProduct] p on o.[ProductID]=p.[ID]
    where o.[BuyerID]=@BuyerID and year(o.[OrderDate])=2025 order by o.[OrderDate] desc;

end

go

create procedure get_orders_from_2024 @BuyerID int
as
begin
    select o.[OrderID], o.[ProductID], o.[ProductType],  o.[PaymentMethod], 
    o.[OrderDate], os.[Subtotal], os.[WarrantyTax], os.[DeliveryFee], 
    os.[finalTotal], os.[address], os.[AdditionalInfo], os.[ContractDetails], p.[name]
    from [Order] o inner join [OrderSummary] os on o.[OrderSummaryID]=os.[ID]
    inner join [DummyProduct] p on o.[ProductID]=p.[ID]
    where o.[BuyerID]=@BuyerID and year(o.[OrderDate])=2024 order by o.[OrderDate] desc;

end
go

create procedure get_orders_by_name @BuyerID int, @text nvarchar(250)
as
begin
    select o.[OrderID], o.[ProductID], o.[ProductType], o.[PaymentMethod], 
    o.[OrderDate], os.[Subtotal], os.[WarrantyTax], os.[DeliveryFee], 
    os.[finalTotal], os.[address], os.[AdditionalInfo], os.[ContractDetails] , p.[name]
    from [Order] o inner join [OrderSummary] os on o.[OrderSummaryID]=os.[ID]
    inner join [DummyProduct] p on o.[ProductID]=p.[ID]
    where o.[BuyerID]=@BuyerID and p.[name] like '%@'+@text+'%' order by o.[OrderDate] desc;

end
go




CREATE PROCEDURE UpdateOrder
    @OrderID INT,
    @ProductType varchar(20),
    @PaymentMethod VARCHAR(20),
    @OrderDate datetime
   
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE [Order]
        SET [ProductType] = @ProductType,
            [PaymentMethod] = @PaymentMethod,
            [OrderDate]=@OrderDate
           
        WHERE OrderID = @OrderID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


CREATE PROCEDURE DeleteOrder
    @OrderID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM Orders
        WHERE OrderID = @OrderID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


CREATE PROCEDURE AddOrder
   @ProductID INT,
    @BuyerID INT,
    @ProductType varchar(20),
    @PaymentMethod VARCHAR(20),
    @OrderSummaryID INT,
    @OrderDate timestamp
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Orders ([ProductID],[BuyerID], [ProductType], [PaymentMethod], [OrderSummaryID], [OrderDate])
        VALUES (@ProductID, @BuyerID, @ProductType, @PaymentMethod, @OrderSummaryID, @OrderDate);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


CREATE PROCEDURE UpdateOrderSummary
    @ID INT,
    @Subtotal float,
    @WarrantyTax float,
    @DeliveryFee float,
    @FinalTotal float,
    @FullName varchar(255),
    @Email varchar(255),
    @PhoneNumber varchar(10),
    @Address varchar(255),
    @PostalCode varchar(255),
    @AdditionalInfo varchar(255),
	@ContractDetails text null
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE OrderSummary
        SET 
             [Subtotal]=@Subtotal,
             [WarrantyTax]=@WarrantyTax,
             [DeliveryFee]=@DeliveryFee,
             [FinalTotal]=@FinalTotal,
             [FullName]=@FullName,
             [Email]=@Email,
             [PhoneNumber]=@PhoneNumber,
             [Address]=@Address,
             [PostalCode]=@PostalCode,
             [AdditionalInfo]=@AdditionalInfo,
	         [ContractDetails]=@ContractDetails
           
        WHERE ID = @ID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


CREATE PROCEDURE DeleteOrderSummary
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM OrderSummary
        WHERE ID = @ID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


CREATE PROCEDURE AddOrderSummary
   @Subtotal float,
    @WarrantyTax float,
    @DeliveryFee float,
    @FinalTotal float,
    @FullName varchar(255),
    @Email varchar(255),
    @PhoneNumber varchar(10),
    @Address varchar(255),
    @PostalCode varchar(255),
    @AdditionalInfo varchar(255),
	@ContractDetails text null
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO OrderSummary ([Subtotal],[WarrantyTax], [DeliveryFee], [FinalTotal], [FullName], [Email], [PhoneNumber], [Address], [PostalCode], [AdditionalInfo]
, [ContractDetails])    VALUES (@Subtotal,@WarrantyTax, @DeliveryFee, @FinalTotal, @FullName, @Email, @PhoneNumber, @Address, @PostalCode, @AdditionalInfo
, @ContractDetails);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

create procedure get_orders_from_order_history @OrderHistoryID int
as
begin
    select * from [Order] o
    where o.[OrderHistoryID]=@OrderHistoryID;

end
go

/*
  This procedure inserts a renewed contract into the [Contract] table.
  Note: This procedure is used exclusively for contract renewals.
        The [contractStatus] is set to 'RENEWED' by default.
        The [renewedFromContractID] stores the original contract's ID,.
*/
CREATE PROCEDURE [AddRenewedContract]
    @orderID INT,
    @contractContent TEXT,
    @renewalCount INT,
    @predefinedContractID INT,
    @pdfID INT,
    @renewedFromContractID INT
AS
BEGIN
    INSERT INTO [Contract] (
        [orderID],
        [contractStatus],
        [contractContent],
        [renewalCount],
        [predefinedContractID],
        [pdfID],
        [renewedFromContractID]
    )
    VALUES (
        @orderID,
        'RENEWED',
        @contractContent,
        @renewalCount,
        @predefinedContractID,
        @pdfID,
        @renewedFromContractID
    );
END;
GO



/*
  This procedure updates the details of an existing renewed contract in the [Contract] table.
  Note: This procedure is used exclusively for contract renewals.
        The contract identified by [@contractID] must already have the status 'RENEWED'.
        Updates are applied to the borrowing period, contract content, renewal count, 
        predefined contract reference, and associated PDF file.
*/
CREATE PROCEDURE [UpdateRenewedContract]
    @contractID INT,
    @contractContent TEXT,
    @renewalCount INT,
    @predefinedContractID INT,
    @pdfID INT
AS
BEGIN
    -- Updates a renewed contract's details based on its ID.
    -- This should be used only for contracts with status 'RENEWED'.
    UPDATE [Contract]
    SET 
        [contractContent] = @contractContent,
        [renewalCount] = @renewalCount,
        [predefinedContractID] = @predefinedContractID,
        [pdfID] = @pdfID
    WHERE 
        [ID] = @contractID AND [contractStatus] = 'RENEWED';
END;
GO



/*
  This procedure retrieves all renewed contracts from the [Contract] table.
  Only contracts with [contractStatus] = 'RENEWED' are returned.
*/
CREATE PROCEDURE [GetRenewedContracts]
AS
BEGIN
    SELECT 
        [ID],
        [orderID],
        [contractStatus],
        [contractContent],
        [renewalCount],
        [predefinedContractID],
        [pdfID],
        [renewedFromContractID]
    FROM [Contract]
    WHERE [contractStatus] = 'RENEWED';
END;
GO

go
create procedure uspUpdateOrderCheckpoint (@checkpointID bigint, @timestamp datetime, @location varchar(255), @description varchar(255), @checkpointStatus varchar(100)) as
begin
	begin try
		update OrderCheckpoints
		set [Timestamp] = @timestamp, [Location] = @location, [Description] = @description, [CheckpointStatus] = @checkpointStatus
		where [CheckpointID] = @checkpointID
	end try
	begin catch
		declare @err_message varchar(2000)
		declare @err_number int
		select @err_message = error_message(), @err_number = error_number()
		raiserror(N'uspUpdateOrderCheckpoint: Error %i: %s', 16, 1, @err_number, @err_message)
	end catch
end
go

go
create procedure uspUpdateTrackedOrder (@trackedOrderID bigint, @estimatedDeliveryDate date, @orderStatus varchar(100)) as
begin
	begin try
		update TrackedOrders
		set [EstimatedDeliveryDate] = @estimatedDeliveryDate
		where [TrackedOrderID] = @trackedOrderID
	end try
	begin catch
		declare @err_message varchar(2000)
		declare @err_number int
		select @err_message = error_message(), @err_number = error_number()
		raiserror(N'uspUpdateTrackedOrder: Error %i: %s', 16, 1, @err_number, @err_message)
	end catch
end
go

go
alter procedure uspUpdateTrackedOrder (@trackedOrderID bigint, @estimatedDeliveryDate date, @orderStatus varchar(100)) as
begin
	begin try
		update TrackedOrders
		set [EstimatedDeliveryDate] = @estimatedDeliveryDate, [OrderStatus] = @orderStatus
		where [TrackedOrderID] = @trackedOrderID
	end try
	begin catch
		declare @err_message varchar(2000)
		declare @err_number int
		select @err_message = error_message(), @err_number = error_number()
		raiserror(N'uspUpdateTrackedOrder: Error %i: %s', 16, 1, @err_number, @err_message)
	end catch
end
go


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