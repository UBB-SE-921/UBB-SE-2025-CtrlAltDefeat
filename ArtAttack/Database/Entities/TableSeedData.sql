

-- Using sample hex binary data for file content
INSERT INTO PDF ([file]) VALUES (0xCAFEBABE);
INSERT INTO PDF ([file]) VALUES (0xDEADBEEF);
INSERT INTO PDF ([file]) VALUES (0xB16B00B5);
INSERT INTO PDF ([file]) VALUES (0xFEEDFACE);
INSERT INTO PDF ([file]) VALUES (0xC001D00D);

insert into [DummyWallet] 
values (1200.0);

insert into [DummyCard]
values 
('Alice Johnson', '5425233430109903', '123', '08', '26', 'Romania', 1500.50),
('Bob Smith', '2222420000001113	', '456', '12', '27', 'Canada', 1500.00),
('Charlie Brown', '2223000048410010', '789', '01', '25', 'Spain', 1500.00)


insert into [DummyBuyer]
values
('Buyer1')

insert into [DummySeller]
values
('Seller1'),('Seller2'),('Seller3')


insert into [DummyProduct]
values
('Toothbrush', 10.00, 1, 'new', NULL,NULL),
('Hair Dryer', 150.00, 1, 'borrowed', NULL,NULL),
('Ancient Chinese Vase', 100, 2, 'bid',NULL,NULL),
('Wallet Top Up', 200,NULL,'refill',NULL,NULL),
('IPhone 4 Penguin Phone Case', 75.00, 3, 'used', NULL,NULL)



insert into [OrderHistory] default values
insert into [OrderHistory] default values
insert into [OrderHistory] default values



insert into [OrderSummary] (Subtotal, WarrantyTax, DeliveryFee, FinalTotal, FullName, Email, PhoneNumber, [Address], PostalCode, AdditionalInfo, ContractDetails)
values (0, 0, 0, 0, 'Placeholder1', 'placeholder@email.com', '0000000000', 'TBD', '000000', 'N/A', NULL);

insert into [Order] (ProductID, BuyerID, ProductType, PaymentMethod, OrderSummaryID, OrderHistoryID)
values (1, 1, 1, NULL, 1, 1);

insert into [Order] (ProductID, BuyerID, ProductType, PaymentMethod, OrderSummaryID,OrderHistoryID)
values (2, 1, 2, NULL, 1, 1);

insert into [Order] (ProductID, BuyerID, ProductType, PaymentMethod, OrderSummaryID,  OrderHistoryID)
values (5, 1, 5, NULL, 1, 1);

--order 2: bid product purchase
insert into [OrderSummary] (Subtotal, WarrantyTax, DeliveryFee, FinalTotal, FullName, Email, PhoneNumber, [Address], PostalCode, AdditionalInfo, ContractDetails)
values (0, 0, 0, 0, 'Placeholder2', 'placeholder@email.com', '0000000000', 'TBD', '000000', 'N/A', NULL);

insert into [Order] (ProductID, BuyerID, ProductType, PaymentMethod, OrderSummaryID, OrderHistoryID)
values (3, 1, 3, NULL, 2, 2);

--order 3: wallet refill
insert into [OrderSummary] (Subtotal, WarrantyTax, DeliveryFee, FinalTotal, FullName, Email, PhoneNumber, [Address], PostalCode, AdditionalInfo, ContractDetails)
values (0, 0, 0, 0, 'Placeholder3', 'placeholder@email.com', '0000000000', 'TBD', '000000', 'N/A', NULL);

insert into [Order] (ProductID, BuyerID, ProductType, PaymentMethod, OrderSummaryID, OrderHistoryID)
values (4, 1, 4, NULL, 3, 3);


/****************************
* 10. Contract
*****************************/
-- For predefinedContractID we use NULL since we are excluding PredefinedContract inserts.
-- pdfID references PDF table and orderID references Order.

DELETE FROM Contract;
  DBCC CHECKIDENT ('Contract', RESEED, 0);
INSERT INTO Contract (orderID, contractStatus, contractContent, renewalCount, predefinedContractID, pdfID, renewedFromContractID)
  VALUES (2, 'ACTIVE', 'Contract for order 2', 0, NULL, 1, NULL);


  
/****************************
* 11. Notification
*****************************/
-- Using various allowed categories and assuming recipientID corresponds to DummyBuyer,
-- contractID from Contract, productID from DummyProduct and orderID from Order.
INSERT INTO Notification (recipientID, category, timestamp, isRead, contractID, isAccepted, productID, orderID, shippingState, deliveryDate, expirationDate)
  VALUES (1, 'PAYMENT_CONFIRMATION', '2025-03-28 10:05:00', 0, 1, 1, 1, 1, 'PROCESSING', '2025-03-30', '2025-04-30');

/****************************
* 12. TrackedOrders
*****************************/
-- Each TrackedOrder must reference a unique OrderID from the Order table.
INSERT INTO TrackedOrders (EstimatedDeliveryDate, DeliveryAddress, OrderStatus, OrderID)
  VALUES ('2025-04-05', '123 Main St', 'PROCESSING', 1);
INSERT INTO TrackedOrders (EstimatedDeliveryDate, DeliveryAddress, OrderStatus, OrderID)
  VALUES ('2025-04-06', '456 Oak Ave', 'SHIPPED', 2);
INSERT INTO TrackedOrders (EstimatedDeliveryDate, DeliveryAddress, OrderStatus, OrderID)
  VALUES ('2025-04-07', '789 Pine Rd', 'IN_WAREHOUSE', 3);
INSERT INTO TrackedOrders (EstimatedDeliveryDate, DeliveryAddress, OrderStatus, OrderID)
  VALUES ('2025-04-08', '321 Maple Ln', 'IN_TRANSIT', 4);
INSERT INTO TrackedOrders (EstimatedDeliveryDate, DeliveryAddress, OrderStatus, OrderID)
  VALUES ('2025-04-09', '654 Cedar Blvd', 'OUT_FOR_DELIVERY', 5);

/****************************
* 13. OrderCheckpoints
*****************************/
-- Each checkpoint is linked to a TrackedOrder (IDs 1 to 5).
INSERT INTO OrderCheckpoints (Timestamp, Location, Description, CheckpointStatus, TrackedOrderID)
  VALUES ('2025-03-29 09:00:00', 'Warehouse', 'Order received at warehouse', 'PROCESSING', 1);
INSERT INTO OrderCheckpoints (Timestamp, Location, Description, CheckpointStatus, TrackedOrderID)
  VALUES ('2025-03-29 12:00:00', 'Distribution Center', 'Order dispatched', 'SHIPPED', 2);
INSERT INTO OrderCheckpoints (Timestamp, Location, Description, CheckpointStatus, TrackedOrderID)
  VALUES ('2025-03-30 08:00:00', 'Local Hub', 'Order arrived at local hub', 'IN_WAREHOUSE', 3);
INSERT INTO OrderCheckpoints (Timestamp, Location, Description, CheckpointStatus, TrackedOrderID)
  VALUES ('2025-03-30 10:00:00', 'On the way', 'Order in transit', 'IN_TRANSIT', 4);
INSERT INTO OrderCheckpoints (Timestamp, Location, Description, CheckpointStatus, TrackedOrderID)
  VALUES ('2025-03-30 15:00:00', 'Customer Area', 'Out for delivery', 'OUT_FOR_DELIVERY', 5);

/****************************
* 14. WaitListProduct
*****************************/
-- Each wait list product references a product from DummyProduct.
INSERT INTO WaitListProduct (productID, availableAgain) VALUES (2, '2025-04-10 09:00:00');

/****************************
* 15. UserWaitList
*****************************/
-- Each UserWaitList references a WaitListProduct and a DummyBuyer.
INSERT INTO UserWaitList (productWaitListID, userID, joinedTime, positionInQueue)
  VALUES (2, 2, '2025-03-28 09:35:00', 1);



INSERT INTO PredefinedContract (content)
VALUES (
'Product Purchase Agreement
This Purchase Agreement ("Agreement") is made and entered into as of {AgreementDate} by and between {SellerName} ("Seller") and {BuyerName} ("Buyer").
WHEREAS, the Seller desires to sell and the Buyer desires to purchase the product {ProductDescription} under the terms and conditions set forth herein;
NOW, THEREFORE, in consideration of the mutual covenants and promises contained herein, the parties agree as follows:
1. Purchase Price: The Buyer shall pay {Price} by {PaymentMethod}.
2. Delivery: The product shall be delivered on or before {DeliveryDate}.
3. Additional Terms: {AdditionalTerms}.
IN WITNESS WHEREOF, the parties have executed this Agreement as of the date first written above.'
);


-- Predefined Contract for Selling a Product
INSERT INTO PredefinedContract (content)
VALUES (
'Sales Agreement
This Sales Agreement ("Agreement") is entered into on {AgreementDate} by and between {SellerName} ("Seller") and {BuyerName} ("Buyer").
The Seller agrees to sell, and the Buyer agrees to purchase, the product {ProductDescription} for the total sum of {Price}.
Payment shall be made by {PaymentMethod} and delivery is scheduled for {DeliveryDate}.
Additional Provisions: {AdditionalTerms}.
IN WITNESS WHEREOF, the parties have executed this Agreement as of the date stated above.'
);



-- Predefined Contract for Borrowing a Product
INSERT INTO PredefinedContract (content)
VALUES (
'Product Loan Agreement
This Loan Agreement ("Agreement") is made on {AgreementDate} between {SellerName} ("Lender") and {BuyerName} ("Borrower").
The Borrower agrees to borrow the product {ProductDescription} for a period of {LoanPeriod} days, with a due date of {DueDate}.
The Borrower shall maintain the product in good condition and, in the event of a late return, shall incur a late fee of {LateFee} per day.
Additional Terms: {AdditionalTerms}.
IN WITNESS WHEREOF, the parties have executed this Agreement as of the date first above written.'
);


