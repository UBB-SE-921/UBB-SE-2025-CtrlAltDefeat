 --INSERT RENEWED CONTRACT
 --UPDATE RENEWED CONTRACT
 --SELECT RENEWED CONTRACT


GO

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