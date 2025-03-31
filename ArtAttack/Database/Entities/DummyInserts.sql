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

