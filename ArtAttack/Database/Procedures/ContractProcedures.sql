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


