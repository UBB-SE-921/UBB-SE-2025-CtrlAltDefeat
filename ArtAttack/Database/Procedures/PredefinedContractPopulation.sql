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


select * from PredefinedContract
