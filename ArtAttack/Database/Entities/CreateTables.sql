drop table if exists [OrderCheckpoints]
drop table if exists [TrackedOrders]
drop table if exists [Notification]
drop table if exists [Contract]


drop table if exists [Order]



drop table if exists [OrderHistory]
drop table if exists [UserWaitList]
drop table if exists [WaitListProduct]

drop table if exists [DummyProduct]

drop table if exists [DummyBuyer] 

drop table if exists [DummyCard]

drop table if exists [DummyWallet]

drop table if exists [DummySeller]

drop table if exists [PDF]
drop table if exists [PredefinedContract]

drop table if exists [OrderSummary]



create table [DummyBuyer](
	[ID] integer identity primary key,
	[name] varchar(64) not null
);


create table [DummyCard](
	[ID] int identity primary key,
	[cardholderName] varchar(50) not null,
	[cardNumber] varchar(20) not null,
	[cvc] varchar(3) not null,
	[month] varchar(2) not null,
	[year] varchar(2) not null,
	[country] varchar(30) not null,
	[balance] float 

);

create table [DummyWallet](
	[ID] integer identity primary key,
	[balance] float
);


create table [DummySeller](
	[ID] integer identity primary key,
	[name] varchar(64) not null
);

create table [PDF](
	[ID] integer identity primary key,
	[file] varbinary(MAX) not null
);


create table [PredefinedContract](
	[ID] integer identity primary key,
	[content] TEXT not null,
	--contract type should be expunged from Class Diagram as it is held by the Order Summary Table
);

create table [OrderSummary](
    [ID] int  identity primary key,

    [Subtotal] float ,
    [WarrantyTax] float,
    [DeliveryFee] float,
    [FinalTotal] float,
    [FullName] varchar(255),
    [Email] varchar(255),
    [PhoneNumber] varchar(10),
    [Address] varchar(255),
    [PostalCode] varchar(255),
    [AdditionalInfo] varchar(255),
	[ContractDetails] text null

);

create table [OrderHistory](
	[ID] int identity primary key
);


create table [DummyProduct](
	[ID] int identity primary key,
  [name] varchar(64) not null,
	[price] float not null,
	[SellerID] integer foreign key references [DummySeller]([ID]), 
	[productType] varchar(20) not null,
	[startDate] datetime,
	[endDate] datetime

);


create table [Order](
	[OrderID] int identity primary key,
	[ProductID] int not null foreign key references [DummyProduct]([ID]),
	[BuyerID] int not null foreign key references [DummyBuyer]([ID]),
    [ProductType] int not null foreign key references [DummyProduct]([ID]),
    [PaymentMethod] varchar(20),
    constraint [PaymentMethodConstraint] check ([PaymentMethod] in ('card', 'cash', 'wallet')),
	[OrderSummaryID] integer foreign key references [OrderSummary]([ID]),
	[OrderDate] DateTime,
	[OrderHistoryID] int foreign key references [OrderHistory]([ID])

);


alter table [Order] drop constraint [PaymentMethodConstraint]
alter table [Order] add constraint [PaymentMethodConstraint] check ([PaymentMethod] in ('card', 'cash', 'wallet') or [PaymentMethod] is NULL) 


create table [Contract](
	[ID] int identity primary key,

	[orderID] integer foreign key references [Order]([OrderID]),

	[contractStatus] Varchar(255) not null,
	constraint [ContractStatusConstraint] check ([contractStatus] in ('ACTIVE', 'RENEWED', 'EXPIRED')),
	[contractContent] TEXT,
	[renewalCount] integer not null,

	[predefinedContractID] integer foreign key references [PredefinedContract]([ID]),
	[pdfID] integer not null foreign key references [PDF]([ID]),
	[AdditionalTerms] TEXT,

	/* Added field to support contract renewals.
    Holds the ID of the original contract being renewed.
    Can be NULL if the contract is not a renewal. */
	[renewedFromContractID] int null 
);

--ALTER TABLE Contract
--ADD AdditionalTerms TEXT;

create table [Notification](
	[notificationID] int identity primary key not null,
	[recipientID] int not null,
	[category] varchar(25) not null, 
	constraint [NotificationCategoryConstraint]check ([category] in('CONTRACT_EXPIRATION', 'OUTBIDDED','ORDER_SHIPPING_PROGRESS','PRODUCT_AVAILABLE','PAYMENT_CONFIRMATION','PRODUCT_REMOVED','CONTRACT_RENEWAL_REQ','CONTRACT_RENEWAL_ANS','CONTRACT_RENEWAL_WAITLIST')),
	[timestamp] DateTime not null,
	[isRead] bit,
	--optional fields
	[contractID] int foreign key references [Contract]([ID]),
	[isAccepted] bit,
	[productID] int foreign key references [DummyProduct]([ID]),
	[orderID] int foreign key references [Order]([OrderID]),
	[shippingState] varchar(25),
	[deliveryDate] Datetime,
	[expirationDate] Datetime,
);


create table [TrackedOrders]
(
	[TrackedOrderID] int primary key identity(1,1),
	[EstimatedDeliveryDate] Date not null,
	[DeliveryAddress] varchar(255) not null,
	[OrderStatus] varchar(100) not null,
	constraint [TrackedOrderConstraint] 
		check ([OrderStatus] in ('PROCESSING','SHIPPED','IN_WAREHOUSE','IN_TRANSIT','OUT_FOR_DELIVERY','DELIVERED')),
	[OrderID] int foreign key references [Order]([OrderID]) unique not null
)



create table [OrderCheckpoints]
(
	[CheckpointID] int primary key identity(1,1),
	[Timestamp] datetime not null,
	[Location] varchar(255),
	[Description] varchar(255) not null,
	[CheckpointStatus] varchar(100) not null,
	constraint [OrderChekpointConstraint] 
		check ([CheckpointStatus] in ('PROCESSING','SHIPPED','IN_WAREHOUSE','IN_TRANSIT','OUT_FOR_DELIVERY','DELIVERED')),
	[TrackedOrderID] int foreign key references [TrackedOrders]([TrackedOrderID]) on delete cascade not null
)



create table [WaitListProduct]
(
	[waitListProductID] int identity primary key,

	[productID] int not null foreign key references [DummyProduct]([ID]) on delete cascade,
	
	[availableAgain] DateTime
);


create table [UserWaitList]
(
	[UserWaitListID] int identity primary key,

	[productWaitListID] int not null foreign key references [WaitListProduct]([WaitListProductID]),

	[userID] integer not null foreign key references [DummyBuyer]([ID]) on delete cascade,

	[joinedTime] DateTime not null,

	[positionInQueue] int not null
);




