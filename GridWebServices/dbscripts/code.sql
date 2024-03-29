USE [Grid]
GO
/****** Object:  UserDefinedFunction [dbo].[GetTeminationDate]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetTeminationDate]
(
	
)
RETURNS Date
AS
BEGIN
	-- Declare the return variable here
	DECLARE @EffectiveDate DATE;
	DECLARE @TerminationCutOffDate INT;
	SELECT @TerminationCutOffDate = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'TerminationCutOffDate'
	-- Add the T-SQL statements to compute the return value here
	SELECT @EffectiveDate = CASE WHEN DAY(GETDATE()) <= @TerminationCutOffDate THEN DATEADD (dd, -1, DATEADD(mm, DATEDIFF(mm, 0, GETDATE()) + 1, 0)) ELSE DATEADD (dd, -1, DATEADD(mm, DATEDIFF(mm, 0, GETDATE()) + 2, 0)) END

	-- Return the result of the function
	RETURN @EffectiveDate
END
GO
/****** Object:  UserDefinedFunction [dbo].[ufnGetCRNumber]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT dbo.ufnGetCRNumber()
CREATE FUNCTION [dbo].[ufnGetCRNumber]()  
RETURNS NVARCHAR(1000) 
AS   
-- Returns the stock level for the product.  
BEGIN  
	

    DECLARE @ret NVARCHAR(1000); 
	DECLARE @CRStartNumber INT = 10000

	SELECT @CRStartNumber = ConfigValue FROM Config where ConfigKey = 'StartingCRNumber'

	DECLARE @Year NVARCHAR(2);
	SET @Year = CAST(RIGHT(Year(getDate()),2) AS NVARCHAR);
	DECLARE @OrderCount INT = 0
	DECLARE @FinalCount INT =0
		SELECT  @OrderCount = COUNT(1) 
	FROM ChangeRequests WHERE YEAR(RequestOn) = YEAR(GETDATE())
		AND (OrderStatus = 1
		OR OrderStatus = 2
		OR OrderStatus = 3
		OR OrderStatus = 4
		OR OrderStatus = 5
		OR OrderStatus = 6
		OR OrderStatus = 7)
	


	SELECT @FinalCount = @OrderCount + @CRStartNumber
	
    
	DECLARE @FinalNumberWithPadding NVARCHAR(12)

	
	SELECT @FinalNumberWithPadding = CAST(LEFT(REPLICATE('0', (12-LEN(@FinalCount))) + CAST(@FinalCount AS VARCHAR), 12) AS NVARCHAR)

	SELECT @ret = 'CC' + @Year + @FinalNumberWithPadding
    RETURN @ret;  
END; 


--SELECT CAST(LEFT(REPLICATE('0', (12-LEN(10001))) + CAST(10001 AS VARCHAR), 12) AS NVARCHAR)
GO
/****** Object:  UserDefinedFunction [dbo].[ufnGetInvoiceNumber]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT dbo.ufnGetInvoiceNumber()
CREATE FUNCTION [dbo].[ufnGetInvoiceNumber]()  
RETURNS NVARCHAR(1000) 
AS   
-- Returns the stock level for the product.  
BEGIN  
	

    DECLARE @ret NVARCHAR(1000); 
	DECLARE @CRStartNumber INT = 10000

	SELECT @CRStartNumber = ConfigValue FROM Config where ConfigKey = 'StartingInvoiceNumber'

	DECLARE @Year NVARCHAR(2);
	SET @Year = CAST(RIGHT(Year(getDate()),2) AS NVARCHAR);
	DECLARE @OrderCount INT = 0
	DECLARE @FinalCount INT =0
		SELECT  @OrderCount = COUNT(1) 
	FROM Payments WHERE YEAR(PaymentOn) = YEAR(GETDATE())
		AND (ApiResult = 'SUCCESS'
		--OR PaymentStatus = 2
		--OR PaymentStatus = 3
		--OR PaymentStatus = 4
		--OR PaymentStatus = 5
		--OR PaymentStatus = 6
		--OR PaymentStatus = 7
		)
	


	SELECT @FinalCount = @OrderCount + @CRStartNumber
	
    
	DECLARE @FinalNumberWithPadding NVARCHAR(12)

	
	SELECT @FinalNumberWithPadding = CAST(LEFT(REPLICATE('0', (12-LEN(@FinalCount))) + CAST(@FinalCount AS VARCHAR), 12) AS NVARCHAR)

	SELECT @ret = 'OC' + @Year + @FinalNumberWithPadding
    RETURN @ret;  
END; 


--SELECT CAST(LEFT(REPLICATE('0', (12-LEN(10001))) + CAST(10001 AS VARCHAR), 12) AS NVARCHAR)
GO
/****** Object:  UserDefinedFunction [dbo].[ufnGetOrderNumber]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT dbo.ufnGetOrderNumber()
CREATE FUNCTION [dbo].[ufnGetOrderNumber]()  
RETURNS NVARCHAR(1000) 
AS   
-- Returns the stock level for the product.  
BEGIN  
	

    DECLARE @ret NVARCHAR(1000); 
	DECLARE @CRStartNumber INT = 10000

	SELECT @CRStartNumber = ConfigValue FROM Config where ConfigKey = 'StartingOrderNumber'

	DECLARE @Year NVARCHAR(2);
	SET @Year = CAST(RIGHT(Year(getDate()),2) AS NVARCHAR);
	DECLARE @OrderCount INT = 0
	DECLARE @FinalCount INT =0
		SELECT  @OrderCount = COUNT(1) 
	FROM Orders WHERE YEAR(ProcessedOn) = YEAR(GETDATE())
		AND (OrderStatus = 1
		OR OrderStatus = 2
		OR OrderStatus = 3
		OR OrderStatus = 4
		OR OrderStatus = 5
		OR OrderStatus = 6
		OR OrderStatus = 7)
	


	SELECT @FinalCount = @OrderCount + @CRStartNumber
	
    
	DECLARE @FinalNumberWithPadding NVARCHAR(12)

	
	SELECT @FinalNumberWithPadding = CAST(LEFT(REPLICATE('0', (12-LEN(@FinalCount))) + CAST(@FinalCount AS VARCHAR), 12) AS NVARCHAR)

	SELECT @ret = 'OC' + @Year + @FinalNumberWithPadding
    RETURN @ret;  
END; 


--SELECT CAST(LEFT(REPLICATE('0', (12-LEN(10001))) + CAST(10001 AS VARCHAR), 12) AS NVARCHAR)
GO
/****** Object:  UserDefinedFunction [dbo].[ufnGetRecieptNumber]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT dbo.ufnGetInvoiceNumber()
CREATE FUNCTION [dbo].[ufnGetRecieptNumber]()  
RETURNS NVARCHAR(1000) 
AS   
-- Returns the stock level for the product.  
BEGIN  
	

    DECLARE @ret NVARCHAR(1000); 
	DECLARE @CRStartNumber INT = 10000

	SELECT @CRStartNumber = ConfigValue FROM Config where ConfigKey = 'StartingRecieptNumber'

	DECLARE @Year NVARCHAR(2);
	SET @Year = CAST(RIGHT(Year(getDate()),2) AS NVARCHAR);
	DECLARE @OrderCount INT = 0
	DECLARE @FinalCount INT =0
		SELECT  @OrderCount = COUNT(1) 
	FROM CheckoutRequests WHERE YEAR(CreatedOn) = YEAR(GETDATE())
	


	SELECT @FinalCount = @OrderCount + @CRStartNumber
	
    
	DECLARE @FinalNumberWithPadding NVARCHAR(12)

	
	SELECT @FinalNumberWithPadding = CAST(LEFT(REPLICATE('0', (12-LEN(@FinalCount))) + CAST(@FinalCount AS VARCHAR), 12) AS NVARCHAR)

	SELECT @ret = 'RC' + @Year + @FinalNumberWithPadding
    RETURN @ret;  
END; 


--SELECT CAST(LEFT(REPLICATE('0', (12-LEN(10001))) + CAST(10001 AS VARCHAR), 12) AS NVARCHAR)
GO
/****** Object:  UserDefinedFunction [dbo].[ufnGetShippingNumber]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT dbo.ufnGetShippingNumber()
CREATE FUNCTION [dbo].[ufnGetShippingNumber]()  
RETURNS NVARCHAR(1000) 
AS   
-- Returns the stock level for the product.  
BEGIN  
	

    DECLARE @ret NVARCHAR(1000); 
	DECLARE @CRStartNumber INT = 10000

	SELECT @CRStartNumber = ConfigValue FROM Config where ConfigKey = 'StartingOrderNumber'

	DECLARE @Year NVARCHAR(2);
	SET @Year = CAST(RIGHT(Year(getDate()),2) AS NVARCHAR);
	DECLARE @OrderCount INT = 0
	DECLARE @FinalCount INT =0
		SELECT  @OrderCount = COUNT(1) 
	FROM DeliveryInformation WHERE YEAR(CreatedOn) = YEAR(GETDATE())	


	SELECT @FinalCount = @OrderCount + @CRStartNumber
	
    
	DECLARE @FinalNumberWithPadding NVARCHAR(12)

	
	SELECT @FinalNumberWithPadding = CAST(LEFT(REPLICATE('0', (12-LEN(@FinalCount))) + CAST(@FinalCount AS VARCHAR), 12) AS NVARCHAR)

	SELECT @ret = 'S' + @Year + @FinalNumberWithPadding
    RETURN @ret;  
END; 


GO
/****** Object:  StoredProcedure [dbo].[Admin_AssignChangeRequestOffsetVoucher]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_AssignChangeRequestOffsetVoucher]
	@SubscriberID int,
	@AdminUserID int
AS
BEGIN
	DECLARE @Duration INT;
	SELECT @Duration = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'VoucherValidityInDays'

	IF EXISTS(SELECT * FROM Subscribers WHERE SubscriberID = @SubscriberID)
	BEGIN
		INSERT INTO SubscriberVouchers
		(
			SubscriberID,
			VoucherID,
			ValidTo,
			AssignedBy
		)
		SELECT 
			@SubscriberID,
			VoucherID,
			CAST(DATEADD(DAY, @Duration, GETDATE()) AS date),
			@AdminUserID
		FROM Vouchers
		WHERE VoucherCode = 'DeliveryOffsetCR'

		INSERT INTO AdminUserLogTrail 
		(
			AdminUserID,
			ActionDescription,
			ActionSource,
			ActionOn
		)
		VALUES
		(
			@AdminUserID,
			'Assigned offset voucher for Sim Replacement Request for SubscriberID: - ' + CAST(@SubscriberID AS nvarchar(10)),
			'Admin Console',
			GETDATE()
		)
		Return 105;
	END
	ELSE
		RETURN 119;
END




GO
/****** Object:  StoredProcedure [dbo].[Admin_AssignOrderOffsetVoucher]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_AssignOrderOffsetVoucher]
	@OrderID int,
	@AdminUserID int
AS
BEGIN
	DECLARE @Duration INT;
	SELECT @Duration = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'VoucherValidityInDays'

	IF EXISTS(SELECT * FROM Orders WHERE OrderID = @OrderID AND (OrderStatus = 2 OR OrderStatus = 3 OR OrderStatus = 4 OR OrderStatus = 5 OR OrderStatus = 6))
	BEGIN
		INSERT INTO OrderVouchers
		(
			OrderID,
			VoucherID,
			ValidTo,
			AssignedBy
		)
		SELECT 
			@OrderID,
			VoucherID,
			CAST(DATEADD(DAY, @Duration, GETDATE()) AS date),
			@AdminUserID
		FROM Vouchers
		WHERE VoucherCode = 'DeliveryOffsetOrder'

		INSERT INTO AdminUserLogTrail 
		(
			AdminUserID,
			ActionDescription,
			ActionSource,
			ActionOn
		)
		VALUES
		(
			@AdminUserID,
			'Assigned offset voucher for order delivery Request for OrderID: - ' + CAST(@OrderID AS nvarchar(10)),
			'Admin Console',
			GETDATE()
		)
		Return 105;
	END
	ELSE
		RETURN 119;
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_AuthenticateAdminUser]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_AuthenticateAdminUser] 
	@Email NVARCHAR(255),
	@Password NVARCHAR(4000)	
AS
BEGIN

IF EXISTS (SELECT * FROM AdminUsers WHERE Email=@Email)
BEGIN	
	IF EXISTS (SELECT * FROM AdminUsers where Email=@Email AND Password=@Password)
	BEGIN
		SELECT	
			AdminUserID,
			Email,
			'Encrypted' as [Password],
			[Name],
			[Role]
		FROM AdminUsers INNER JOIN
			Roles ON AdminUsers.RoleID = Roles.RoleID 	
		WHERE Email=@Email AND Password=@Password

		--Register Customer Log
		INSERT INTO AdminUserLogTrail 
		(
			AdminUserID,
			ActionDescription,
			ActionOn
		)
		SELECT 
			AdminUserID,
			'Login to portal',
			GETDATE()
		FROM AdminUsers 
		WHERE Email=@Email

		RETURN 111 -- Auth success
	END

	ELSE

	BEGIN
	 
	 RETURN 110 -- Password unmatched
	
	END

END

ELSE

RETURN 109 -- email does not exists
		
END
	

GO
/****** Object:  StoredProcedure [dbo].[Admin_CreateAdminUser]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_CreateAdminUser] 
	@Email NVARCHAR(255),
	@Password NVARCHAR(4000),
	@RoleID int,
	@CreatedBy int
AS
BEGIN

IF NOT EXISTS (SELECT * FROM AdminUsers  WHERE Email=@Email)
BEGIN	
		INSERT INTO AdminUsers (Email, Password, RoleID, CreatedBy)
		VALUES (@Email, @Password,@RoleID, @CreatedBy)

		DECLARE @AdminUserID INT
		set @AdminUserID = SCOPE_IDENTITY(); 

		SELECT	
			AdminUserID,
			[Name],
			Email, 
			'Encrypted' as [Password],
			Department,
			Office,
			[Role]
		FROM AdminUsers	
		LEFT JOIN Departments ON AdminUsers.DepartmentID=Departments.DepartmentID
		LEFT JOIN Offices ON AdminUsers.OfficeID =Offices.OfficeID
		LEFT JOIN Roles ON AdminUsers.RoleID=Roles.RoleID
		where AdminUserID=@AdminUserID

		RETURN 100 -- creation success
END

ELSE
	BEGIN
		RETURN 108 -- EMAIL EXISTS
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_Data_CreatePlans]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_Data_CreatePlans]
	@PlanMarketingName NVARCHAR(255),
    @PortalSummaryDescription NVARCHAR(255),
    @PortalDescription NVARCHAR(255),
    @IsCustomerSelectable INT,
    @BSSPlanCode NVARCHAR(255),
    @BSSPlanName NVARCHAR(255),
    @BSSPlanDescription NVARCHAR(255),
    @PlanDescription NVARCHAR(255),
    @PlanType INT,
    @Data INT,
    @Voice INT,
    @SMS INT,
    @SubscriptionFee FLOAT,
    @IsGSTIncluded INT,
    @IsRecurring INT,
	@IsStandAlonePlan INT,
	@ValidFrom DATE,
	@ValidTo DATE, 
	@HasBuddyPromotion INT,
	@IsBuddyBundle INT
AS 
BEGIN
	DECLARE @PlanID INT
	DECLARE @BundleID INT
	IF EXISTS(SELECT PlanID FROM Plans WHERE BSSPlanCode = @BSSPlanCode)
	BEGIN
		SELECT -1;
	END
	ELSE 
	BEGIN
		INSERT INTO [dbo].[Plans]
			   ([PlanMarketingName]
			   ,[PortalSummaryDescription]
			   ,[PortalDescription]
			   ,[IsCustomerSelectable]
			   ,[BSSPlanCode]
			   ,[BSSPlanName]
			   ,[BSSPlanDescription]
			   ,[PlanDescription]
			   ,[PlanType]
			   ,[Data]
			   ,[Voice]
			   ,[SMS]
			   ,[SubscriptionFee]
			   ,[IsGSTIncluded]
			   ,[IsRecurring]
			   ,[CreatedOn]
			   ,[CreatedBy]
			   ,[LastUpdatedOn]
			   ,[LastUpdatedBy])
		 VALUES
			   (@PlanMarketingName,
			   @PortalSummaryDescription,
			   @PortalDescription,
			   @IsCustomerSelectable,
			   @BSSPlanCode,
			   @BSSPlanName,
			   @BSSPlanDescription,
			   @PlanDescription,
			   @PlanType,
			   @Data,
			   @Voice,
			   @SMS,
			   @SubscriptionFee,
			   @IsGSTIncluded,
			   @IsRecurring,
			   GETDATE(),
			   1,
			   GETDATE(),
			   1)
		SET @PlanID = SCOPE_IDENTITY();
		IF(@IsStandAlonePlan = 1)
		BEGIN
			INSERT INTO [dbo].[Bundles]
			   ([BundleName]
			   ,[PlanMarketingName]
			   ,[PortalSummaryDescription]
			   ,[PortalDescription]
			   ,[IsCustomerSelectable]
			   ,[ValidFrom]
			   ,[ValidTo]
			   ,[Status]
			   ,[HasBuddyPromotion]
			   ,[IsBuddyBundle])
			 VALUES
			   (@BSSPlanName,
			   @PlanMarketingName,
			   @PortalSummaryDescription,
			   @PortalDescription,
			   @IsCustomerSelectable,
			   @ValidFrom,
			   @ValidTo,
			   1,
			   @HasBuddyPromotion,
			   @IsBuddyBundle)
			SET @BundleID = SCOPE_IDENTITY();

			INSERT INTO BundlePlans (BundleID, PlanID)
			VALUES (
				@BundleID,
				@PlanID
			)
		END
		SELECT 1;
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetAccessToken]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_GetAccessToken]
	@AdminUserID INT,
	@CustomerID INT
AS
BEGIN
	IF EXISTS(SELECT * FROM AdminUsers WHERE AdminUserID = @AdminUserID) AND EXISTS(SELECT * FROM Customers WHERE CustomerID = @CustomerID)
	BEGIN
		DECLARE @Token NVARCHAR(255) = NEWID();
		INSERT INTO AdminAccessTokens
		(
			AdminUserID, 
			CustomerID,
			Token,
			CreatedOn
		)
		VALUES
		(
			@AdminUserID,
			@CustomerID,
			@Token,
			GETDATE()
		)

		SELECT @Token AS AccessToken;
		RETURN 105;
	END
	ELSE
	BEGIN
		RETURN 119;
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetAdminUserByID]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_GetAdminUserByID] 
	@AdminUserID int
AS
BEGIN	
		SELECT	
			AdminUserID,
			[Name],
			Email, 
			'Encrypted' as [Password],
			Department,
			Office,
			[Role]
		FROM AdminUsers	
		LEFT JOIN Departments ON AdminUsers.DepartmentID=Departments.DepartmentID
		LEFT JOIN Offices ON AdminUsers.OfficeID =Offices.OfficeID
		LEFT JOIN Roles ON AdminUsers.RoleID=Roles.RoleID
		where AdminUserID=@AdminUserID
		
	END
	

GO
/****** Object:  StoredProcedure [dbo].[Admin_GetAllAdminRoles]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_GetAllAdminRoles]
AS 
BEGIN
	SELECT RoleID, Role FROM Roles
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetAllAdminUsers]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_GetAllAdminUsers] 
AS
BEGIN
    SELECT AdminUserID,[Name],Email,'Encrypted' as [Password] ,[Role]
	FROM AdminUsers INNER JOIN
	Roles ON AdminUsers.RoleID =Roles.RoleID 
	ORDER BY AdminUsers.AdminUserID   
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetBannerDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Admin_GetBannerDetails] --'MainBanner'
	@LocationName NVARCHAR(50),
	@PageName NVARCHAR(50)
AS
BEGIN
	DECLARE @CurrentDate AS DATE
	SET @CurrentDate = CAST(GETDATE() AS DATE);
	SELECT Banners.BannerID,Locations.LocationID, Banners.BannerName, Banners.BannerImage, BannerUrl, UrlType, Banners.Status, Banners.ValidFrom,Banners.ValidTo
	FROM Banners INNER JOIN 
		Locations ON Banners.LocationID = Locations.LocationID INNER JOIN 
		Pages ON Locations.PageID = Pages.PageID
	WHERE LocationName = @LocationName
		AND Pages.PageName = @PageName
		AND Banners.Status = 1
		AND @CurrentDate BETWEEN ISNULL(Banners.ValidFrom, @CurrentDate) AND ISNULL(Banners.ValidTo, @CurrentDate)
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetBSSRequestIDAndSubscriberSession]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_GetBSSRequestIDAndSubscriberSession] --Admin_GetBSSRequestIDAndSubscriberSession 'Customers', 'GetAssets', 30
	@Source NVARCHAR(50),
	@APIName NVARCHAR(50),
	@CustomerID INT,	
	@MobileNumber NVARCHAR(50)
AS
BEGIN
	INSERT INTO BSSCallLogs 
	(
		CustomerID,
		Source,
		APIName, 
		CalledOn 
	)
	VALUES 
	(
		@CustomerID,
		@Source,
		@APIName,
		GETDATE()
	)

	DECLARE @BSSCallID INT

	SET @BSSCallID = SCOPE_IDENTITY();

	DECLARE @RequestID NVARCHAR(50)
	
	DECLARE @UserID NVARCHAR(50)

	SET @RequestID =  'GR2601' + CAST(@BSSCallID AS nvarchar(20))

	IF(LEN(@RequestID) < 20)
	
	BEGIN		
		
		SET @RequestID = 'GR2601' +RIGHT('00000000000000',14-LEN(@BSSCallID) )     + CAST(@BSSCallID AS varchar(14))		
	END
	
	UPDATE BSSCallLogs SET RequestID = @RequestID  WHERE BSSCallLogID = @BSSCallID
				
		IF  (@APIName ='GetAssets' OR @APIName ='UpdateAssetStatus')			
		BEGIN	

		DECLARE @OrderID INT

		select @OrderID=OrderId from Accounts  a INNER JOIN Orders o on o.AccountID=a.AccountID 
		where a.CustomerID =@CustomerID

		SELECT @UserID= UserSessionID  from OrderSubscribers  where 
					MobileNumber=@MobileNumber	and OrderID=@OrderID	
		END
	
	SELECT @RequestID AS RequestID, @UserID AS  UserID, @BSSCallID as BSSCallLogID
	
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetConfigurationByKey]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_GetConfigurationByKey]
	@ConfigKey NVARCHAR(50)
AS
BEGIN
	SELECT ConfigKey as [key], ConfigValue as value FROM Config WHERE ConfigKey = @ConfigKey
	
	IF @@ROWCOUNT>0

		RETURN 105 -- exists

	ELSE
	
		RETURN 102 -- record does not exists
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetConfigurations]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_GetConfigurations]
	@ConfigType NVARCHAR(50)
AS
BEGIN

	SELECT ConfigKey as [key], ConfigValue as value FROM Config WHERE ConfigType = @ConfigType
	
	IF @@ROWCOUNT>0

		RETURN 105 -- exists

	ELSE
	
		RETURN 102 -- record does not exists
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetConfigValue]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_GetConfigValue] 
--Admin_GetConfigValue 'AWSAccessKey', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjM4IiwibmJmIjoxNTU0NDQxNzcxLCJleHAiOjE1NTUwNDY1NzEsImlhdCI6MTU1NDQ0MTc3MX0.9jFc9Jf2ksEK0-56M3gLCXexWaP5kP23R9AXhY1IBBk'
	@ConfigKey NVARCHAR(50),
	@Token NVARCHAR(255)
AS 
BEGIN
	IF EXISTS(SELECT * FROM CustomerToken WHERE Token = @Token)
	BEGIN
		SELECT ConfigValue FROM Config 
		WHERE ConfigKey = @ConfigKey 
		RETURN 111 -- Auth success
	END
	ELSE
	BEGIN
		SELECT -1
		RETURN 113 --Token auth failed
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetCustomerChangeRequests]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Admin_GetCustomerChangeRequests] --[Admin_GetCustomerChangeRequests] 38
	@CustomerID INT
AS
BEGIN
	IF EXISTS(SELECT CustomerID FROM Customers WHERE CustomerID = @CustomerID)
	BEGIN
		DECLARE @AccountID INT
		SELECT @AccountID = AccountID FROM Accounts WHERE CustomerID = @CustomerID

		SELECT ChangeRequests.ChangeRequestID,
			Subscribers.SubscriberID,
			Subscribers.MobileNumber,
			OrderNumber, 
			RequestType,
			RequestOn,
			BillingUnit,
			BillingFloor,
			BillingBuildingNumber,
			BillingBuildingName,
			BillingStreetName,
			BillingPostCode,
			BillingContactNumber,
			DeliveryInformation.[Name],
			DeliveryInformation.Email,
			IDType,
			IDNumber,
			IsSameAsBilling,
			ShippingUnit,
			ShippingFloor,
			ShippingBuildingNumber,
			ShippingBuildingName,
			ShippingStreetName,
			ShippingPostCode,
			ShippingContactNumber,
			AlternateRecipientContact,
			AlternateRecipientName,
			AlternateRecipientEmail,
			DeliveryInformation.PortalSlotID,
			DeliverySlots.SlotDate,
			DeliverySlots.SlotFromTime,
			DeliverySlots.SlotToTime,
			ScheduledDate,
			ISNULL(CRCharges.ServiceFee, 0) AS ServiceFee,
			CASE OrderStatus 
				WHEN -1 THEN 'expired'--;0=Initiated;1=Paid;2=Processed;3=Shipped;4=Cancelled;5=PaymentDenied'
				WHEN 0 THEN 'initiated'
				WHEN 1 THEN 'paid and pending'
				WHEN 2 THEN 'processed'
				WHEN  3 THEN 'shipped'
				WHEN  4 THEN 'cancelled'
				ELSE 'payment denied'
			END AS OrderStatus,
			CASE  
				WHEN OrderStatus = 7 THEN 'completed'
				ELSE 'pending'
			END AS ListingStatus
		FROM ChangeRequests INNER JOIN
			RequestTypes ON ChangeRequests.RequestTypeID = RequestTypes.RequestTypeID INNER JOIN
			SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID INNER JOIN 
			Subscribers ON SubscriberRequests.SubscriberID = Subscribers.SubscriberID LEFT OUTER JOIN 
			DeliveryInformation ON ChangeRequests.DeliveryInformationID = DeliveryInformation.DeliveryInformationID LEFT OUTER JOIN 
			(	
				SELECT ChangeRequestID, SUM(ChangeRequestCharges.ServiceFee) AS ServiceFee
				FROM ChangeRequestCharges
				GROUP BY ChangeRequestID
			)CRCharges ON ChangeRequests.ChangeRequestID = CRCharges.ChangeRequestID LEFT OUTER JOIN 
			DeliverySlots ON DeliveryInformation.PortalSlotID = DeliverySlots.PortalSlotID 
		WHERE Subscribers.AccountID = @AccountID AND (OrderStatus = 1 OR OrderStatus = 2 OR OrderStatus = 3 OR OrderStatus = 4 OR
														OrderStatus = 5 OR OrderStatus = 6 OR OrderStatus = 7 OR OrderStatus = 8  OR OrderStatus = 9)
		---1=expired;0=initiated;1=paid and pending,2=sent to delivery vendor, 3=delivery order fulfilled, 4=out for delivery, 5=delivered, 6=delivery failed, 7=service activated, 8=cancelled;9=payment denied

		RETURN 105
	END
	ELSE
	BEGIN
		RETURN 102 -- does not exists
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetCustomerListing]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_GetCustomerListing] 
AS
BEGIN
	SELECT	
		CustomerID,
		Email,
		'Encrypted' as [Password],
		MobileNumber,
		ReferralCode,
		Nationality,
		Gender,
		DOB,
		CASE SMSSubscription WHEN 1 THEN 'Yes' ELSE 'No' END AS SMSSubscription,
		CASE EmailSubscription WHEN 1 THEN 'Yes' ELSE 'No' END AS EmailSubscription,
		CASE [Status] WHEN 1 THEN 'Active' ELSE 'InActive' END AS [Status],
		JoinedOn
	FROM Customers
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetCustomerOrders]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_GetCustomerOrders] --[Admin_GetCustomerOrders] 38
	@CustomerID INT
AS
BEGIN
	IF EXISTS(SELECT CustomerID FROM Customers WHERE CustomerID = @CustomerID)
	BEGIN
		DECLARE @AccountID INT
		SELECT @AccountID = AccountID FROM Accounts WHERE CustomerID = @CustomerID

		SELECT Orders.OrderID,
			OrderNumber, 
			OrderDate,
			Documents.IdentityCardNumber,
			Documents.IdentityCardType,
			BillingUnit,
			BillingFloor,
			BillingBuildingNumber,
			BillingBuildingName,
			BillingStreetName,
			BillingPostCode,
			BillingContactNumber,
			ReferralCode,
			PromotionCode,
			CAST(CASE OrderDocs.DocCount WHEN 0 THEN 0 ELSE 1 END AS bit) AS HaveDocuments,
			DeliveryInformation.[Name],
			DeliveryInformation.Email,
			IDType,
			IDNumber,
			IsSameAsBilling,
			ShippingUnit,
			ShippingFloor,
			ShippingBuildingNumber,
			ShippingBuildingName,
			ShippingStreetName,
			ShippingPostCode,
			ShippingContactNumber,
			AlternateRecipientContact,
			AlternateRecipientName,
			AlternateRecipientEmail,
			DeliveryInformation.PortalSlotID,
			DeliverySlots.SlotDate,
			DeliverySlots.SlotFromTime,
			DeliverySlots.SlotToTime,
			ScheduledDate,
			ISNULL(OrderCharges.ServiceFee, 0) AS ServiceFee,
			CASE OrderStatus 
				WHEN -1 THEN 'expired'
				WHEN 0 THEN 'initiated'
				WHEN 1 THEN 'paid and pending'
				WHEN 2 THEN 'sent to delivery vendor'
				WHEN  3 THEN 'delivery order fulfilled'
				WHEN  4 THEN 'out for delivery'
				WHEN  5 THEN 'delivered'
				WHEN  6 THEN 'delivery failed'
				WHEN  7 THEN 'service activated'
				WHEN  8 THEN 'cancelled'
				ELSE 'payment denied'
			END AS OrderStatus,
			CASE  
				WHEN OrderStatus = 7 THEN 'completed'
				ELSE 'pending'
			END AS ListingStatus
		FROM Orders LEFT OUTER JOIN
			OrderDocuments ON Orders.OrderID = OrderDocuments.OrderID LEFT OUTER JOIN
			Documents ON OrderDocuments.DocumentID = Documents.DocumentID LEFT OUTER JOIN 
			DeliveryInformation ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID LEFT OUTER JOIN 
			(	
				SELECT OrderID, SUM(OrderCharges.ServiceFee) AS ServiceFee
				FROM OrderCharges
				GROUP BY OrderID
			)OrderCharges ON Orders.OrderID = OrderCharges.OrderID LEFT OUTER JOIN 
			DeliverySlots ON DeliveryInformation.PortalSlotID = DeliverySlots.PortalSlotID LEFT OUTER JOIN 
			(	
				SELECT OrderID, COUNT(Documents.IdentityCardType) AS DocCount
				FROM OrderDocuments INNER JOIN 
					Documents ON OrderDocuments.DocumentID = Documents.DocumentID
				GROUP BY OrderID
			)OrderDocs ON Orders.OrderID = OrderDocs.OrderID
		WHERE Orders.AccountID = @AccountID AND (OrderStatus = 1 OR OrderStatus = 2 OR OrderStatus = 3 OR OrderStatus = 4 OR OrderStatus = 5 OR OrderStatus = 6 OR OrderStatus = 7)
		---1=expired;0=initiated;1=paid and pending,2=sent to delivery vendor, 3=delivery order fulfilled, 4=out for delivery, 5=delivered, 6=delivery failed, 7=service activated, 8=cancelled;9=payment denied

		SELECT
			OrderSubscribers.OrderID,
			OrderBundle.BundleID,
			MobileNumber,
			DisplayName,
			IsPrimaryNumber,
			Bundles.PlanMarketingName,
			Bundles.PortalDescription,
			Bundles.PortalSummaryDescription,
			TotalData,
			TotalSMS,
			TotalVoice,
			ActualSubscriptionFee,
			ApplicableSubscriptionFee,
			ServiceName,
			ActualServiceFee,
			ApplicableServiceFee,
			PremiumType,
			IsPorted,
			IsOwnNumber,
			DonorProvider,
			[PortedNumberTransferForm],
			[PortedNumberOwnedBy],
			[PortedNumberOwnerRegistrationID]		
		FROM Orders INNER JOIN 
			OrderSubscribers ON Orders.OrderID = OrderSubscribers.OrderID INNER JOIN 
			(
				SELECT OrderSubscriberID, BundleID 
				FROM OrderSubscriberChangeRequests INNER JOIN 
					ChangeRequests ON OrderSubscriberChangeRequests.ChangeRequestID = ChangeRequests.ChangeRequestID INNER JOIN 
					OrderSubscriptions ON ChangeRequests.ChangeRequestID = OrderSubscriptions.ChangeRequestID
				GROUP BY OrderSubscriberID, BundleID 
			) OrderBundle ON OrderSubscribers.OrderSubscriberID = OrderBundle.OrderSubscriberID INNER JOIN 
			Bundles ON Bundles.BundleID = OrderBundle.BundleID INNER JOIN 
			(
				SELECT SUM(ISNULL([Data], 0)) AS TotalData, 
					SUM(ISNULL(Voice, 0)) AS TotalVoice, 
					SUM(ISNULL([SMS], 0)) AS TotalSMS,
					SUM((ISNULL(SubscriptionFee, 0))) AS ActualSubscriptionFee, 
					SUM((ISNULL(SubscriptionFee, 0) * (1 - BundlePlans.DiscountPercentage/100))) AS ApplicableSubscriptionFee,
					Bundles.BundleID
				FROM Bundles INNER JOIN 
					BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN
					Plans ON BundlePlans.PlanID = Plans.PlanID
				GROUP BY Bundles.BundleID
			) AS BundleOffering ON BundleOffering.BundleID = Bundles.BundleID INNER JOIN 
			(
				SELECT Bundles.BundleID,
					SUM(ISNULL(AdminServices.ServiceFee, 0)) AS ActualServiceFee, 
					SUM(ISNULL(AdminServices.ServiceFee, 0) * (1 - ISNULL(PlanAdminServices.AdminServiceDiscountPercentage, 0)/100)) AS ApplicableServiceFee,
					STUFF(
						 (SELECT ',' + PortalServiceName 
						  FROM AdminServices INNER JOIN 
							PlanAdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID
						  WHERE PlanAdminServices.BundleID = Bundles.BundleID
						  FOR XML PATH (''))
						 , 1, 1, '') AS ServiceName
				FROM AdminServices INNER JOIN 
					PlanAdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID INNER JOIN
					Bundles ON Bundles.BundleID = PlanAdminServices.BundleID
				GROUP BY Bundles.BundleID
			) AdminFee ON AdminFee.BundleID = Bundles.BundleID 
		WHERE Orders.AccountID = @AccountID AND (OrderStatus = 1 OR OrderStatus = 2 OR OrderStatus = 3 OR OrderStatus = 4 OR OrderStatus = 5 OR OrderStatus = 6 OR OrderStatus = 7)

		SELECT Orders.OrderID,
			PortalServiceName,
			OrderCharges.ServiceFee,
			OrderCharges.IsRecurring,
			OrderCharges.IsGSTIncluded
		FROM Orders INNER JOIN 
			OrderCharges ON Orders.OrderID = OrderCharges.OrderID INNER JOIN 
			AdminServices ON OrderCharges.AdminServiceID = AdminServices.AdminServiceID
		WHERE Orders.AccountID = @AccountID AND (OrderStatus = 1 OR OrderStatus = 2 OR OrderStatus = 3 OR OrderStatus = 4 OR OrderStatus = 5 OR OrderStatus = 6 OR OrderStatus = 7)

		UNION ALL

		SELECT Orders.OrderID,
			PortalServiceName,
			SubscriberCharges.ServiceFee,
			SubscriberCharges.IsRecurring,
			SubscriberCharges.IsGSTIncluded
		FROM SubscriberCharges INNER JOIN
			OrderSubscribers ON  SubscriberCharges.OrderSubscriberID = OrderSubscribers.OrderSubscriberID INNER JOIN
			Orders ON  Orders.OrderID = OrderSubscribers.OrderID INNER JOIN
			AdminServices ON SubscriberCharges.AdminServiceID = AdminServices.AdminServiceID
		WHERE Orders.AccountID = @AccountID AND (OrderStatus = 1 OR OrderStatus = 2 OR OrderStatus = 3 OR OrderStatus = 4 OR OrderStatus = 5 OR OrderStatus = 6 OR OrderStatus = 7)
		
		RETURN 105
	END
	ELSE
	BEGIN
		RETURN 102 -- does not exists
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetCustomerSharedChangeRequests]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [dbo].[Admin_GetCustomerSharedChangeRequests] --[Admin_GetCustomerChangeRequests] 38
	@CustomerID INT
AS
BEGIN
	IF EXISTS(SELECT CustomerID FROM Customers WHERE CustomerID = @CustomerID)
	BEGIN
		DECLARE @AccountID INT
		SELECT @AccountID = AccountID FROM Accounts WHERE CustomerID = @CustomerID

		SELECT ChangeRequests.ChangeRequestID,
			OrderNumber, 
			RequestType,
			RequestOn,
			BillingUnit,
			BillingFloor,
			BillingBuildingNumber,
			BillingBuildingName,
			BillingStreetName,
			BillingPostCode,
			BillingContactNumber,
			DeliveryInformation.[Name],
			DeliveryInformation.Email,
			IDType,
			IDNumber,
			IsSameAsBilling,
			ShippingUnit,
			ShippingFloor,
			ShippingBuildingNumber,
			ShippingBuildingName,
			ShippingStreetName,
			ShippingPostCode,
			ShippingContactNumber,
			AlternateRecipientContact,
			AlternateRecipientName,
			AlternateRecipientEmail,
			DeliveryInformation.PortalSlotID,
			DeliverySlots.SlotDate,
			DeliverySlots.SlotFromTime,
			DeliverySlots.SlotToTime,
			ScheduledDate,
			CRCharges.ServiceFee,
			CASE OrderStatus 
				WHEN -1 THEN 'expired'--;0=Initiated;1=Paid;2=Processed;3=Shipped;4=Cancelled;5=PaymentDenied'
				WHEN 0 THEN 'initiated'
				WHEN 1 THEN 'paid and pending'
				WHEN 2 THEN 'processed'
				WHEN  3 THEN 'shipped'
				WHEN  4 THEN 'cancelled'
				ELSE 'payment denied'
			END AS OrderStatus,
			CASE  
				WHEN OrderStatus = 2 THEN 'completed'
				ELSE 'pending'
			END AS ListingStatus
		FROM ChangeRequests INNER JOIN
			RequestTypes ON ChangeRequests.RequestTypeID = RequestTypes.RequestTypeID INNER JOIN
			AccountChangeRequests ON ChangeRequests.ChangeRequestID = AccountChangeRequests.ChangeRequestID LEFT OUTER JOIN 
			DeliveryInformation ON ChangeRequests.DeliveryInformationID = DeliveryInformation.DeliveryInformationID LEFT OUTER JOIN 
			(	
				SELECT ChangeRequestID, SUM(ChangeRequestCharges.ServiceFee) AS ServiceFee
				FROM ChangeRequestCharges
				GROUP BY ChangeRequestID
			)CRCharges ON ChangeRequests.ChangeRequestID = CRCharges.ChangeRequestID LEFT OUTER JOIN 
			DeliverySlots ON DeliveryInformation.PortalSlotID = DeliverySlots.PortalSlotID 
		WHERE AccountChangeRequests.AccountID = @AccountID 
			AND (OrderStatus = 1 OR OrderStatus = 2 OR OrderStatus = 3 OR OrderStatus = 4)
		RETURN 105
	END
	ELSE
	BEGIN
		RETURN 102 -- does not exists
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetGenericConfigValue]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_GetGenericConfigValue]
	@ConfigKey NVARCHAR(50)
AS 
BEGIN
	SELECT ConfigValue FROM Config 
	WHERE ConfigKey = @ConfigKey 
		AND NeedValidation = 0
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetLookup]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Admin_GetLookup]
	@LookupType NVARCHAR(50)
AS
BEGIN
	SELECT Lookups.LookupID, Lookups.LookupText
	FROM Lookups INNER JOIN 
		LookupTypes ON Lookups.LookupTypeID = LookupTypes.LookupTypeID
	WHERE LookupTypes.LookupType = @LookupType
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetNumberTypeCodes]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_GetNumberTypeCodes]
	@ServiceType NVARCHAR(50)
AS
BEGIN
	SELECT PortalServiceName, 
		ServiceCode,
		ServiceFee
	FROM AdminServices
	WHERE ServiceType = @ServiceType

	IF @@ROWCOUNT>0

	RETURN 105 -- exists

	ELSE
		 RETURN 102 -- record does not exists
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetPageFAQ]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_GetPageFAQ] 
	@PageName NVARCHAR(50) 
AS
BEGIN
	SELECT FAQs.Title,
		FAQs.Description,
		FAQCategory,
		SortOrder
	FROM FAQs INNER JOIN 
		FAQCategories ON FAQCategories.FAQCategoryID = FAQs.FAQCategoryID INNER JOIN 
		PageFAQs ON FAQs.FAQID = PageFAQs.FAQID INNER JOIN 
		Pages ON PageFAQs.PageID = Pages.PageID
	WHERE PageName = @PageName
	ORDER BY PageFAQs.SortOrder
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetRequestIDForBSSAPI]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_GetRequestIDForBSSAPI] --Admin_GetRequestIDForBSSAPI 'Customers', 'GetAssets', 30
	@Source NVARCHAR(50),
	@APIName NVARCHAR(50),
	@CustomerID INT,
	@IsNewSession INT=null,
	@MobileNumber NVARCHAR(50)=NULL
AS
BEGIN
	INSERT INTO BSSCallLogs 
	(
		CustomerID,
		Source,
		APIName, 
		CalledOn 
	)
	VALUES 
	(
		@CustomerID,
		@Source,
		@APIName,
		GETDATE()
	)

	DECLARE @BSSCallID INT

	SET @BSSCallID = SCOPE_IDENTITY();

	DECLARE @RequestID NVARCHAR(50)
	
	DECLARE @UserID NVARCHAR(50)

	SET @RequestID =  'GR2601' + CAST(@BSSCallID AS nvarchar(20))

	IF(LEN(@RequestID) < 20)
	
	BEGIN		
		
		SET @RequestID = 'GR2601' +RIGHT('00000000000000',14-LEN(@BSSCallID) )     + CAST(@BSSCallID AS varchar(14))		
	END
	
	UPDATE BSSCallLogs SET RequestID = @RequestID  WHERE BSSCallLogID = @BSSCallID

	IF @IsNewSession =1
		
	  BEGIN		

			SET  @UserID= 'GR' +RIGHT('0000000000000000',16-LEN(@BSSCallID) )     + CAST(@BSSCallID AS varchar(16))	

			UPDATE BSSCallLogs SET UserID = @UserID  WHERE BSSCallLogID = @BSSCallID
		
	  END

	  ELSE
		BEGIN				
		IF  (@APIName ='GetAssets' OR @APIName ='UpdateAssetStatus')
			BEGIN		
			IF @MobileNumber is not null and @MobileNumber <> ''			
			BEGIN
			
				SELECT @UserID= UserID FROM BSSCallLogs  INNER JOIN BSSCallNumbers ON 
									BSSCallLogs.BSSCallLogID = BSSCallNumbers.BSSCallLogID   
									WHERE BSSCallNumbers.MobileNumber=@MobileNumber and BSSCallLogs.CustomerID=@CustomerID
			END
			ELSE
			  BEGIN
			    SELECT TOP 1 @UserID= UserID FROM BSSCallLogs WHERE CustomerID=@CustomerID AND UserId is Not Null order by BSSCallLogID desc


			  END
		   END
	
		END
	SELECT @RequestID AS RequestID, @UserID AS  UserID, @BSSCallID as BSSCallLogID
	
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetServiceFee]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_GetServiceFee]
	@ServiceCode INT		
AS
BEGIN
	SELECT ServiceCode, ServiceFee FROM  AdminServices WHERE ServiceCode=@ServiceCode AND 

	(CAST(GETDATE() AS date) BETWEEN CAST(ValidFrom AS date)  AND  CAST(ValidTo AS date))

	IF @@ROWCOUNT>0

	RETURN 105 -- exists

	ELSE
		 RETURN 102 -- record does not exists
		
END
	

	
GO
/****** Object:  StoredProcedure [dbo].[Admin_UpdateProfile]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
CREATE PROCEDURE [dbo].[Admin_UpdateProfile]
	@AdminID INT,
	@ExistingPassword NVARCHAR(4000),
	@NewPassword NVARCHAR(4000)
AS
BEGIN	
	IF EXISTS(SELECT * FROM AdminUsers WHERE AdminUserID = @AdminID)
	BEGIN	
		IF EXISTS(SELECT * FROM AdminUsers WHERE [Password] = @ExistingPassword AND AdminUserID = @AdminID)
		BEGIN
				UPDATE AdminUsers SET [Password] = @NewPassword WHERE AdminUserID = @AdminID
				RETURN 101 --Updated success
			END
		END 
	ELSE
	BEGIN		
		RETURN 102 --does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_ValidateAdminUserPassword]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Admin_ValidateAdminUserPassword]
	@AdminUserID INT,
	@Password NVARCHAR(255)
AS
BEGIN
	IF EXISTS(SELECT * FROM AdminUsers WHERE AdminUserID = @AdminUserID AND Password = @Password)
	BEGIN
		RETURN 105;
	END
	ELSE
	BEGIN
		RETURN 119;
	END
END
GO
/****** Object:  StoredProcedure [dbo].[AdminUser_AuthenticateToken]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AdminUser_AuthenticateToken]
	@Token NVARCHAR(1000)
AS 
BEGIN
	IF EXISTS(SELECT AdminUserID FROM AdminUserTokens WHERE Token = @Token)
	BEGIN
		SELECT AdminUserID,
			CreatedOn
		FROM AdminUserTokens
		WHERE Token = @Token
		RETURN 111 -- Auth success
	END
	ELSE
	BEGIN
		RETURN 113 --Token auth failed
	END
END
GO
/****** Object:  StoredProcedure [dbo].[AdminUser_CreateToken]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AdminUser_CreateToken]
	@AdminUserID INT,
	@Token nvarchar(1000)
AS 
BEGIN
	IF EXISTS(SELECT AdminUserID FROM AdminUserTokens WHERE Token = @Token)
	BEGIN
		SELECT AdminUserID,
			CreatedOn
		FROM AdminUserTokens
		WHERE Token = @Token
		RETURN 105 -- Already Exists
	END
	ELSE IF EXISTS(SELECT AdminUserID FROM AdminUserTokens WHERE AdminUserID = @AdminUserID)
	BEGIN
		UPDATE AdminUserTokens SET Token = @Token,CreatedOn=GETDATE() WHERE AdminUserID = @AdminUserID
		RETURN 202 -- Updated
	END
	ELSE
	BEGIN
		INSERT INTO AdminUserTokens
		(
			AdminUserID,
			Token,
			CreatedOn
		)
		VALUES
		(
			@AdminUserID,
			@Token,
			GETDATE()
		)
		RETURN 100 -- created
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Catelog_BundleExists]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Catelog_BundleExists]
	@BundleID INT		
AS
BEGIN	
	
	

	IF EXISTS(SELECT * FROM  Bundles WHERE BundleID=@BundleID) 
	
		return 105 -- exists

		else

		 return 102 -- record does not exists

		
END
	

	
GO
/****** Object:  StoredProcedure [dbo].[Catelog_CreateBundle]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Catelog_CreateBundle]
		   @BundleName nvarchar(255),
           @PlanMarketingName nvarchar(255),
           @PortalSummaryDescription nvarchar(max),
           @PortalDescription  nvarchar(max),
           @IsCustomerSelectable int,
           @ValidFrom datetime,
           @ValidTo datetime
          
AS
BEGIN	
	
	INSERT INTO [dbo].[Bundles]
           ([BundleName]
           ,[PlanMarketingName]
           ,[PortalSummaryDescription]
           ,[PortalDescription]
           ,[IsCustomerSelectable]
           ,[ValidFrom]
           ,[ValidTo]
           ,[Status])

		   VALUES (@BundleName,
           @PlanMarketingName,
           @PortalSummaryDescription,
           @PortalDescription,
           @IsCustomerSelectable,
           @ValidFrom,
           @ValidTo, 1) 

		   IF @@ERROR<>0

		   RETURN 107
	
	ELSE

	BEGIN
		   SELECT * FROM Bundles WHERE BundleID=@@IDENTITY		
		   
		   return 100  

	END
		
END
	

	
GO
/****** Object:  StoredProcedure [dbo].[Catelog_DeleteBundle]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Catelog_DeleteBundle]
	@BundleID INT		
AS
BEGIN		
	
	DECLARE @STATUS INT	

	IF EXISTS(SELECT * FROM  Bundles WHERE BundleID=@BundleID) 
	
	BEGIN

	SET @STATUS = (SELECT [STATUS] FROM Bundles WHERE BundleID=@BundleID)

	IF @STATUS=0
	
		BEGIN
	
			DELETE FROM Bundles where BundleID=@BundleID and Status=0	

			return 103 -- successfully deleted

		END

	ELSE  
		BEGIN  
			 return 104 -- active record can not be deleted
		END 

	
	END
	ELSE
		BEGIN

		 return 102 -- record does not exists

		END
	END
	

	
GO
/****** Object:  StoredProcedure [dbo].[Catelog_GetBundleById]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Catelog_GetBundleById]
	@BundleID INT
AS
BEGIN
	DECLARE @CurrentDate AS DATE
	SET @CurrentDate = CAST(GETDATE() AS DATE);
	SELECT 
		Bundles.BundleID,
		BundleName,
		PortalDescription,
		PortalSummaryDescription,
		Bundles.PlanMarketingName,
		TotalData,
		TotalSMS,
		TotalVoice,
		ActualSubscriptionFee,
		ApplicableSubscriptionFee,
		ServiceName,
		ActualServiceFee,
		ApplicableServiceFee,
		'' AS PromotionText
	FROM Bundles INNER JOIN 
		(
			SELECT SUM(ISNULL([Data], 0)) AS TotalData, 
				SUM(ISNULL(Voice, 0)) AS TotalVoice, 
				SUM(ISNULL([SMS], 0)) AS TotalSMS,
				SUM((ISNULL(SubscriptionFee, 0))) AS ActualSubscriptionFee, 
				SUM((ISNULL(SubscriptionFee, 0) * (1 - BundlePlans.DiscountPercentage/100))) AS ApplicableSubscriptionFee,
				Bundles.BundleID
			FROM Bundles INNER JOIN 
				BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN
				Plans ON BundlePlans.PlanID = Plans.PlanID
			GROUP BY Bundles.BundleID
		) AS BundleOffering ON BundleOffering.BundleID = Bundles.BundleID INNER JOIN 
		(
			SELECT Bundles.BundleID, Bundles.PlanMarketingName
			FROM Bundles INNER JOIN 
				BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN
				Plans ON BundlePlans.PlanID = Plans.PlanID
			WHERE Plans.PlanType = 0
		) AS BundleBasePlan ON BundleBasePlan.BundleID = Bundles.BundleID LEFT OUTER JOIN
		(
			SELECT Bundles.BundleID,
				SUM(ISNULL(AdminServices.ServiceFee, 0)) AS ActualServiceFee, 
				SUM(ISNULL(AdminServices.ServiceFee, 0) * (1 - ISNULL(PlanAdminServices.AdminServiceDiscountPercentage, 0)/100)) AS ApplicableServiceFee,
				STUFF(
					 (SELECT ',' + PortalServiceName 
					  FROM AdminServices INNER JOIN 
						PlanAdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID
					  WHERE PlanAdminServices.BundleID = Bundles.BundleID
					  FOR XML PATH (''))
					 , 1, 1, '') AS ServiceName
			FROM AdminServices INNER JOIN 
				PlanAdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID INNER JOIN
				Bundles ON Bundles.BundleID = PlanAdminServices.BundleID
			GROUP BY Bundles.BundleID
		) AdminFee ON AdminFee.BundleID = Bundles.BundleID 
	WHERE Bundles.IsCustomerSelectable = 1 
		AND Bundles.Status = 1
		AND @CurrentDate BETWEEN ISNULL(Bundles.ValidFrom, @CurrentDate) 
		AND ISNULL(Bundles.ValidTo, @CurrentDate) AND Bundles.BundleID=@BundleID
END
GO
/****** Object:  StoredProcedure [dbo].[Catelog_GetBundlesListing]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Catelog_GetBundlesListing]
	
AS
BEGIN
	DECLARE @CurrentDate AS DATE
	SET @CurrentDate = CAST(GETDATE() AS DATE);
	SELECT 
		Bundles.BundleID,
		BundleName,
		PortalDescription,
		PortalSummaryDescription,
		Bundles.PlanMarketingName,
		TotalData,
		TotalSMS,
		TotalVoice,
		ActualSubscriptionFee,
		ApplicableSubscriptionFee,
		ServiceName,
		ActualServiceFee,
		ApplicableServiceFee,
		'' AS PromotionText
	FROM Bundles INNER JOIN 
		(
			SELECT SUM(ISNULL([Data], 0)) AS TotalData, 
				SUM(ISNULL(Voice, 0)) AS TotalVoice, 
				SUM(ISNULL([SMS], 0)) AS TotalSMS,
				SUM((ISNULL(SubscriptionFee, 0))) AS ActualSubscriptionFee, 
				SUM((ISNULL(SubscriptionFee, 0) * (1 - BundlePlans.DiscountPercentage/100))) AS ApplicableSubscriptionFee,
				Bundles.BundleID
			FROM Bundles INNER JOIN 
				BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN
				Plans ON BundlePlans.PlanID = Plans.PlanID
			GROUP BY Bundles.BundleID
		) AS BundleOffering ON BundleOffering.BundleID = Bundles.BundleID INNER JOIN 
		(
			SELECT Bundles.BundleID, Bundles.PlanMarketingName
			FROM Bundles INNER JOIN 
				BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN
				Plans ON BundlePlans.PlanID = Plans.PlanID
			WHERE Plans.PlanType = 0
		) AS BundleBasePlan ON BundleBasePlan.BundleID = Bundles.BundleID LEFT OUTER JOIN
		(
			SELECT Bundles.BundleID,
				SUM(ISNULL(AdminServices.ServiceFee, 0)) AS ActualServiceFee, 
				SUM(ISNULL(AdminServices.ServiceFee, 0) * (1 - ISNULL(PlanAdminServices.AdminServiceDiscountPercentage, 0)/100)) AS ApplicableServiceFee,
				STUFF(
					 (SELECT ',' + PortalServiceName 
					  FROM AdminServices INNER JOIN 
						PlanAdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID
					  WHERE PlanAdminServices.BundleID = Bundles.BundleID
					  FOR XML PATH (''))
					 , 1, 1, '') AS ServiceName
			FROM AdminServices INNER JOIN 
				PlanAdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID INNER JOIN
				Bundles ON Bundles.BundleID = PlanAdminServices.BundleID
			GROUP BY Bundles.BundleID
		) AdminFee ON AdminFee.BundleID = Bundles.BundleID 
	WHERE Bundles.IsCustomerSelectable = 1 
		AND Bundles.Status = 1
		AND @CurrentDate BETWEEN ISNULL(Bundles.ValidFrom, @CurrentDate) AND ISNULL(Bundles.ValidTo, @CurrentDate)
END
GO
/****** Object:  StoredProcedure [dbo].[Catelog_GetPromotionalBundle]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Catelog_GetPromotionalBundle] --WebApp_GetPromotionalBundle 1, 'LAUNCH2019'
	@BundleID INT,
	@PromotionCode NVARCHAR(50)
AS
BEGIN
	DECLARE @BasePlanID AS INT
	SELECT @BasePlanID = Plans.PlanID 
	FROM Bundles INNER JOIN 
		BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN
		Plans ON BundlePlans.PlanID = Plans.PlanID
	WHERE Plans.PlanType = 0 AND Bundles.BundleID = @BundleID

	DECLARE @CurrentDate AS DATE
	SET @CurrentDate = CAST(GETDATE() AS DATE);

	SELECT 
		Bundles.BundleID,
		BundleName,
		PortalDescription,
		PortalSummaryDescription,
		Bundles.PlanMarketingName,
		TotalData,
		TotalSMS,
		TotalVoice,
		ActualSubscriptionFee,
		ApplicableSubscriptionFee,
		ServiceName,
		ActualServiceFee,
		ApplicableServiceFee,
		PromotionBundles.PromotionText AS PromotionText
	FROM Bundles INNER JOIN 
		PromotionBundles ON Bundles.BundleID = PromotionBundles.BundleID INNER JOIN 
		Promotions ON PromotionBundles.PromotionID = Promotions.PromotionID INNER JOIN 
		(
			SELECT 
				SUM(ISNULL([Data], 0)) AS TotalData, 
				SUM(ISNULL(Voice, 0)) AS TotalVoice, 
				SUM(ISNULL([SMS], 0)) AS TotalSMS, 
				SUM((ISNULL(SubscriptionFee, 0))) AS ActualSubscriptionFee, 
				SUM((ISNULL(SubscriptionFee, 0) * (1 - BundlePlans.DiscountPercentage/100))) AS ApplicableSubscriptionFee, 
				Bundles.BundleID
			FROM Bundles INNER JOIN 
				BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN
				Plans ON BundlePlans.PlanID = Plans.PlanID
			GROUP BY Bundles.BundleID
		) AS BundleOffering ON BundleOffering.BundleID = Bundles.BundleID INNER JOIN 
		(
			SELECT Bundles.BundleID, Plans.PlanMarketingName, Plans.PlanID
			FROM Bundles INNER JOIN 
				BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN
				Plans ON BundlePlans.PlanID = Plans.PlanID
			WHERE Plans.PlanType = 0
		) AS BundleBasePlan ON BundleBasePlan.BundleID = Bundles.BundleID LEFT OUTER JOIN
		(
			SELECT Bundles.BundleID,
				SUM(ISNULL(AdminServices.ServiceFee, 0)) AS ActualServiceFee, 
				SUM(ISNULL(AdminServices.ServiceFee, 0) * (1 - ISNULL(PlanAdminServices.AdminServiceDiscountPercentage, 0)/100)) AS ApplicableServiceFee,
				STUFF(
					 (SELECT ',' + PortalServiceName 
					  FROM AdminServices INNER JOIN 
						PlanAdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID
					  WHERE PlanAdminServices.BundleID = Bundles.BundleID
					  FOR XML PATH (''))
					 , 1, 1, '') AS ServiceName
			FROM AdminServices INNER JOIN 
				PlanAdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID INNER JOIN
				Bundles ON Bundles.BundleID = PlanAdminServices.BundleID
			GROUP BY Bundles.BundleID
		) AdminFee ON AdminFee.BundleID = Bundles.BundleID 
	WHERE  Bundles.Status = 1
		AND @CurrentDate BETWEEN ISNULL(Bundles.ValidFrom, @CurrentDate) AND ISNULL(Bundles.ValidTo, @CurrentDate)
		AND Promotions.PromotionCode = @PromotionCode
		AND @CurrentDate BETWEEN ISNULL(PromotionBundles.ValidFrom, @CurrentDate) AND ISNULL(PromotionBundles.ValidTo, @CurrentDate)
		AND BundleBasePlan.PlanID = @BasePlanID

	
END
GO
/****** Object:  StoredProcedure [dbo].[Catelog_GetPurchaseVASListing]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Catelog_GetPurchaseVASListing] --38
	@CustomerID INT,
	@MobileNumber NVARCHAR(50)
AS
BEGIN
	--Cannot bundle recurring and non recurring email together
	DECLARE @CurrentDate AS DATE
	SET @CurrentDate = CAST(GETDATE() AS DATE);
	SELECT 
		Bundles.BundleID AS VASID,
		BSSPlanCode AS BSSPlanCode,
		Bundles.PortalDescription,
		Bundles.PortalSummaryDescription,
		Bundles.PlanMarketingName,
		TotalData AS [Data],
		TotalSMS AS SMS,
		TotalVoice AS Voice,
		TotalSubscriptionFee AS SubscriptionFee,
		CASE IsRecurring WHEN 1 THEN 'monthly' WHEN 0 THEN 'one-time' ELSE 'pay-as-use' END  AS IsRecurring,
		ISNULL(SubscriptionCount, 0) AS SubscriptionCount
	FROM Bundles INNER JOIN 
		(
			SELECT SUM(Data) AS TotalData, SUM(SMS) AS TotalSMS, SUM(Voice) AS TotalVoice, SUM(SubscriptionFee) AS TotalSubscriptionFee, Bundles.BundleID, IsRecurring, BSSPlanCode
			FROM Bundles
				INNER JOIN BundlePlans ON Bundles.BundleID = BundlePlans.BundleID
				INNER JOIN Plans ON BundlePlans.PlanID = Plans.PlanID
			GROUP BY Bundles.BundleID, IsRecurring, BSSPlanCode
		) BundlePlans ON BundlePlans.BundleID = Bundles.BundleID LEFT OUTER JOIN 
		(
			SELECT BundleID, COUNT(*) AS SubscriptionCount
			FROM Subscriptions INNER JOIN	
				Subscribers ON  Subscriptions.SubscriberID = Subscribers.SubscriberID INNER JOIN 
				Accounts ON Subscribers.AccountID = Accounts.AccountID
			WHERE CustomerID = @CustomerID AND Subscribers.MobileNumber = @MobileNumber AND (Subscriptions.Status = 0 OR Subscriptions.Status = 1)
			GROUP BY BundleID
		)SCount ON SCount.BundleID = Bundles.BundleID
	WHERE Bundles.IsCustomerSelectable = 1
		AND Bundles.Status = 1
		AND BundleType = 1
		AND @CurrentDate BETWEEN ISNULL(Bundles.ValidFrom, @CurrentDate) AND ISNULL(Bundles.ValidTo, @CurrentDate)
END
GO
/****** Object:  StoredProcedure [dbo].[Catelog_GetSharedVASListing]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Catelog_GetSharedVASListing]
	@CustomerID INT
AS
BEGIN
	--Cannot bundle recurring and non recurring email together
	DECLARE @CurrentDate AS DATE
	SET @CurrentDate = CAST(GETDATE() AS DATE);
	SELECT 
		Bundles.BundleID AS VASID,
		Bundles.PortalDescription,
		Bundles.PortalSummaryDescription,
		Bundles.PlanMarketingName,
		TotalData AS [Data],
		TotalSMS AS SMS,
		TotalVoice AS Voice,
		TotalSubscriptionFee AS SubscriptionFee,
		CASE IsRecurring WHEN 1 THEN 'monthly' ELSE 'one-time' END  AS IsRecurring,
		ISNULL(SubscriptionCount, 0) AS SubscriptionCount
	FROM Bundles INNER JOIN 
		(
			SELECT SUM(Data) AS TotalData, SUM(SMS) AS TotalSMS, SUM(Voice) AS TotalVoice, SUM(SubscriptionFee) AS TotalSubscriptionFee, Bundles.BundleID, IsRecurring
			FROM Bundles
				INNER JOIN BundlePlans ON Bundles.BundleID = BundlePlans.BundleID
				INNER JOIN Plans ON BundlePlans.PlanID = Plans.PlanID
			GROUP BY Bundles.BundleID, IsRecurring
		) BundlePlans ON BundlePlans.BundleID = Bundles.BundleID LEFT OUTER JOIN 
		(
			SELECT BundleID, COUNT(*) AS SubscriptionCount
			FROM AccountSubscriptions INNER JOIN	
				Accounts ON AccountSubscriptions.AccountID = Accounts.AccountID
			WHERE CustomerID = @CustomerID AND (AccountSubscriptions.Status = 0 OR AccountSubscriptions.Status = 1)
			GROUP BY BundleID
		)SCount ON SCount.BundleID = Bundles.BundleID
	WHERE Bundles.IsCustomerSelectable = 1
		AND Bundles.Status = 1
		AND BundleType = 2
		AND @CurrentDate BETWEEN ISNULL(Bundles.ValidFrom, @CurrentDate) AND ISNULL(Bundles.ValidTo, @CurrentDate)
END
GO
/****** Object:  StoredProcedure [dbo].[Catelog_GetVASListing]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Catelog_GetVASListing] --[Catelog_GetVASListing] 38
	@CustomerID INT
AS
BEGIN
	--Cannot bundle recurring and non recurring email together
	DECLARE @CurrentDate AS DATE
	SET @CurrentDate = CAST(GETDATE() AS DATE);
	SELECT 
		Bundles.BundleID AS VASID,
		BSSPlanCode AS BSSPlanCode,
		Bundles.PortalDescription,
		Bundles.PortalSummaryDescription,
		Bundles.PlanMarketingName,
		TotalData AS [Data],
		TotalSMS AS SMS,
		TotalVoice AS Voice,
		TotalSubscriptionFee AS SubscriptionFee,
		CASE IsRecurring WHEN 1 THEN 'monthly' WHEN 0 THEN 'one-time' ELSE 'pay-as-use' END  AS IsRecurring,
		ISNULL(SubscriptionCount, 0) AS SubscriptionCount
	FROM Bundles INNER JOIN 
		(
			SELECT SUM(Data) AS TotalData, SUM(SMS) AS TotalSMS, SUM(Voice) AS TotalVoice, SUM(SubscriptionFee) AS TotalSubscriptionFee, Bundles.BundleID, BSSPlanCode, IsRecurring
			FROM Bundles
				INNER JOIN BundlePlans ON Bundles.BundleID = BundlePlans.BundleID
				INNER JOIN Plans ON BundlePlans.PlanID = Plans.PlanID
			GROUP BY Bundles.BundleID, BSSPlanCode, IsRecurring
		) TotalValue ON TotalValue.BundleID = Bundles.BundleID LEFT OUTER JOIN 
		(
			SELECT BundleID, COUNT(*) AS SubscriptionCount
			FROM Subscriptions INNER JOIN	
				Subscribers ON  Subscriptions.SubscriberID = Subscribers.SubscriberID INNER JOIN 
				Accounts ON Subscribers.AccountID = Accounts.AccountID
			WHERE CustomerID = @CustomerID AND (Subscriptions.Status = 0 OR Subscriptions.Status = 1)
			GROUP BY BundleID
		)SCount ON SCount.BundleID = Bundles.BundleID
	WHERE Bundles.IsCustomerSelectable = 1
		AND Bundles.Status = 1
		AND BundleType = 1
		AND @CurrentDate BETWEEN ISNULL(Bundles.ValidFrom, @CurrentDate) AND ISNULL(Bundles.ValidTo, @CurrentDate)
END
GO
/****** Object:  StoredProcedure [dbo].[Catelog_UpdateBundle]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Catelog_UpdateBundle]
		   @BundleID INT,
		   @BundleName nvarchar(255),
           @PlanMarketingName nvarchar(255),
           @PortalSummaryDescription nvarchar(max),
           @PortalDescription  nvarchar(max),
           @IsCustomerSelectable int,
           @ValidFrom date,
           @ValidTo date,
		   @status INT
          
AS
BEGIN	
	
	IF EXISTS (SELECT * FROM Bundles WHERE BundleID=@BundleID)

	BEGIN

	UPDATE [dbo].[Bundles]
           SET BundleName =@BundleName,
           planMarketingName= @PlanMarketingName,
           PortalSummaryDescription=@PortalSummaryDescription,
           PortalDescription=@PortalDescription,
           IsCustomerSelectable= @IsCustomerSelectable,
           ValidFrom=@ValidFrom,
           ValidTo=@ValidTo,
           [Status]=@status WHERE BundleID=@BundleID		  

		   IF @@ERROR <> 0
		   
		   RETURN 106

		  ELSE

		  -- Exec Catelog_GetBundleById @BundleID	

		  RETURN 101		

	END

	ELSE
		
		RETURN 102		
END
	

	
GO
/****** Object:  StoredProcedure [dbo].[Config_GetValue]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Config_GetValue]
@ConfigKey NVARCHAR(100)
AS
BEGIN
	SELECT  
		ConfigValue ,
		ConfigType,
		NeedValidation  
	FROM Config 
	WHERE ConfigKey=@ConfigKey
END
GO
/****** Object:  StoredProcedure [dbo].[CR_GetBuddyDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CR_GetBuddyDetails] @CustomerID INT  
,@MobileNumber NVARCHAR(50) 
AS  
BEGIN  
	  SELECT
	   --MobileNumber  
    --,DisplayName  
    --,SIMID  
    --,PremiumType  
    --,ActivatedOn  
    --,CASE Subscribers.IsPrimary  
    -- WHEN 0  
    --  THEN CAST(0 AS BIT)  
    -- ELSE CAST(1 AS BIT)  
    -- END AS IsPrimary  
    --,CASE IsBuddyLine  
    -- WHEN 0  
    --  THEN 'Main'  
    -- ELSE 'Buddy'  
    -- END AS AccountType  
    LinkedMobileNumber  
    ,LinkedDisplayName  
    --,SubscriberStatus.State  
   
   FROM Subscribers  
    INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID  
    LEFT OUTER JOIN  
    (  
     SELECT Subscribers.SubscriberID  
      ,LinkedSubscribers.DisplayName AS LinkedDisplayName  
      ,LinkedSubscribers.MobileNumber AS LinkedMobileNumber  
     FROM Subscribers  
     INNER JOIN Subscribers LinkedSubscribers ON Subscribers.LinkedSubscriberID = LinkedSubscribers.SubscriberID  
     LEFT OUTER JOIN  
     (  
      SELECT SubscriberID, State   
      FROM   
      (  
       SELECT SubscriberID  
        ,State, ROW_NUMBER() OVER(PARTITION BY SubscriberID ORDER BY StateDate DESC) AS RNum  
       FROM SubscriberStates  
      ) A   
      WHERE RNum = 1  
     ) SubscriberStatus ON SubscriberStatus.SubscriberID = Subscribers.SubscriberID  
     WHERE SubscriberStatus.State <> 'Terminated'  
    ) LinkedSubscribers ON LinkedSubscribers.SubscriberID = Subscribers.SubscriberID  
    LEFT OUTER JOIN  
    (  
     SELECT SubscriberID, State   
     FROM   
     (  
      SELECT SubscriberID  
       ,State, ROW_NUMBER() OVER(PARTITION BY SubscriberID ORDER BY StateDate DESC) AS RNum  
      FROM SubscriberStates  
     ) A   
     WHERE RNum = 1  
    ) SubscriberStatus ON SubscriberStatus.SubscriberID = Subscribers.SubscriberID 
	WHERE  MobileNumber = @MobileNumber
  
 IF @@ERROR <> 0  
  RETURN 107  
 ELSE  
  RETURN 100  
END  
GO
/****** Object:  StoredProcedure [dbo].[CR_GetMessageBody]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC CR_GetMessageBody 639
CREATE PROCEDURE [dbo].[CR_GetMessageBody] --[CR_GetMessageBody] 1129
	 @ChangeRequestID INT  
AS  
BEGIN  
 -- Main query 	
   DECLARE @CurrentDate DATE;            
   SET @CurrentDate = CAST(GETDATE() AS DATE)   
   
	 DECLARE @SubscriberID INT
	 SELECT @SubscriberID = SubscriberID 
	 FROM SubscriberRequests 
	 WHERE ChangeRequestID = @ChangeRequestID

	 DECLARE @OldBundleID INT
	 SELECT  @OldBundleID = Bundles.BundleID -- 0      
	 FROM Subscribers      
		INNER JOIN Subscriptions ON Subscriptions.SubscriberID = Subscribers.SubscriberID      
		INNER JOIN Plans ON Subscriptions.PlanID = Plans.PlanID      
		 AND Plans.PlanType = 0      
		INNER JOIN Bundles ON Subscriptions.BundleID = Bundles.BundleID       
	 WHERE Subscribers.SubscriberID = @SubscriberID       
		AND (Subscriptions.Status = 0 OR Subscriptions.Status = 1)       
		AND ISNULL(Subscriptions.ValidTo, @CurrentDate) >= @CurrentDate 

	 SELECT ChangeRequests.ChangeRequestID  
	  ,Accounts.AccountID  
	  ,Accounts.CustomerID  
	  ,Subscribers.SubscriberID  
	  ,ChangeRequests.OrderNumber  
	  ,ChangeRequests.RequestOn  
	  ,CASE   
	   WHEN ChangeRequests.RequestTypeID = (  
		 SELECT RequestTypeID  
		 FROM RequestTypes  
		 WHERE RequestType = 'Terminate'  
		 )  
		THEN dbo.GetTeminationDate()  
	   ELSE NULL  
	   END AS EffectiveDate  
	  ,ChangeRequests.BillingUnit  
	  ,ChangeRequests.BillingFloor  
	  ,ChangeRequests.BillingBuildingNumber  
	  ,ChangeRequests.BillingBuildingName  
	  ,ChangeRequests.BillingStreetName  
	  ,ChangeRequests.BillingPostCode  
	  ,ChangeRequests.BillingContactNumber  
	  ,Subscribers.MobileNumber  
	  ,Subscribers.PremiumType  
	  ,Subscribers.IsPorted  
	  ,1 AS IsOwnNumber  
	  ,Subscribers.DonorProviderName AS DonorProvider  
	  ,NULL AS PortedNumberTransferForm  
	  ,NULL AS PortedNumberOwnedBy  
	  ,NULL AS PortedNumberOwnerRegistrationID  
	  ,Customers.ReferralCode  
	  ,CASE Customers.Gender  
	   WHEN 'Male'  
		THEN 'Mr.'  
	   ELSE 'Ms.'  
	   END AS Title  
	  ,Accounts.AccountName AS Name  
	  ,Accounts.Email  
	  ,Customers.Nationality  
	  ,CUstID.IdentityCardType AS IDType  
	  ,CUstID.IdentityCardNumber AS IDNumber  
	  ,DeliveryInformation.IsSameAsBilling  
	  ,DeliveryInformation.ShippingUnit  
	  ,DeliveryInformation.ShippingFloor  
	  ,DeliveryInformation.ShippingBuildingNumber  
	  ,DeliveryInformation.ShippingBuildingName  
	  ,DeliveryInformation.ShippingStreetName  
	  ,DeliveryInformation.ShippingPostCode  
	  ,DeliveryInformation.ShippingContactNumber  
	  ,DeliveryInformation.AlternateRecipientContact  
	  ,DeliveryInformation.AlternateRecipientName  
	  ,DeliveryInformation.AlternateRecipientEmail  
	  ,DeliveryInformation.PortalSlotID  
	  ,DeliverySlots.SlotDate AS SlotDate  
	  ,DeliverySlots.SlotFromTime  
	  ,DeliverySlots.SlotToTime  
	  ,DeliveryInformation.ScheduledDate AS ScheduledDate  
	  ,Subscribers.MobileNumber AS OldMobileNumber  
	  ,NumberChangeRequests.NewMobileNumber AS NewMobileNumber  
	  ,Subscribers.SIMID AS OldSIM  
	 FROM ChangeRequests  
	 LEFT JOIN SubscriberRequests ON SubscriberRequests.ChangeRequestID = ChangeRequests.ChangeRequestID  
	 LEFT JOIN Subscribers ON Subscribers.SubscriberID = SubscriberRequests.SubscriberID  
	 LEFT JOIN AccountChangeRequests ON AccountChangeRequests.ChangeRequestID = ChangeRequests.ChangeRequestID  
	 INNER JOIN Accounts ON Accounts.AccountID = ISNULL(Subscribers.AccountID, AccountChangeRequests.AccountID)  
	 INNER JOIN Customers ON Customers.CustomerID = Accounts.CustomerID  
	 LEFT JOIN DeliveryInformation ON ChangeRequests.DeliveryInformationID = DeliveryInformation.DeliveryInformationID  
	 LEFT JOIN DeliverySlots ON DeliveryInformation.PortalSlotID = DeliverySlots.PortalSlotID  
	 LEFT JOIN NumberChangeRequests ON NumberChangeRequests.ChangeRequestID = ChangeRequests.ChangeRequestID  
	 LEFT JOIN (  
	  SELECT CustomerID  
	   ,IdentityCardType  
	   ,IdentityCardNumber  
	  FROM (  
	   SELECT CustomerID  
		,IdentityCardType  
		,IdentityCardNumber  
		,ROW_NUMBER() OVER (  
		 PARTITION BY CustomerID ORDER BY CreatedOn DESC  
		 ) AS RNum  
	   FROM Documents  
	   ) A  
	  WHERE RNum = 1  
	  ) CUstID ON Customers.CustomerID = CUstID.CustomerID  
	 WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID  
  
	 --New  Bundle  
	 SELECT OrderSubscriptions.BundleID AS BundleID  
	  ,OrderSubscriptions.BSSPlanCode AS BSSPlanCode  
	  ,Plans.BSSPlanName AS BSSPlanName  
	  ,Plans.PlanType  
	  ,OrderSubscriptions.OldBundleID  
	  ,Plans.PlanMarketingName  
	  ,Plans.PortalDescription  
	  ,Plans.Data AS TotalData  
	  ,Plans.SMS AS TotalSMS  
	  ,Plans.Voice AS TotalVoice  
	  ,Plans.SubscriptionFee AS ApplicableSubscriptionFee  
	  ,NULL AS ServiceName  
	  ,NULL AS ApplicableServiceFee  
	  ,OldPlan.PlanID AS OldPlanID  
	  ,OldPlan.BSSPlanCode AS OldBSSPlanId  
	  ,OldPlan.BSSPlanName AS OldBSSPlanName  
	 FROM ChangeRequests
		 LEFT JOIN OrderSubscriptions ON ChangeRequests.ChangeRequestID = OrderSubscriptions.ChangeRequestID  
		 LEFT JOIN Plans ON Plans.PlanID = OrderSubscriptions.PlanID   
		 LEFT JOIN (  
			  SELECT Bundles.BundleID  
			   ,Plans.PlanID  
			   ,BSSPlanCode  
			   ,BSSPlanName  
			  FROM Plans  
			  INNER JOIN BundlePlans ON Plans.PlanID = BundlePlans.PlanID  
			  INNER JOIN Bundles ON BundlePlans.BundleID = Bundles.BundleID  
			  WHERE PlanType = 0  
		  ) OldPlan ON OldPlan.BundleID = OrderSubscriptions.OldBundleID
	 WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID  
	  AND RequestTypeID IN (  
	   3,4,5 
	   )  
  
	 -- old Bundle 		     
	 SELECT Subscriptions.BundleID,  
		 Plans.BSSPlanCode,  
		 Plans.BSSPlanName,  
		 PlanType,  
		 Subscriptions.ValidFrom AS StartDate,  
		 Subscriptions.ValidTo AS ExpiryDate  
	 FROM Subscribers 
		 INNER JOIN Subscriptions ON Subscribers.SubscriberID = Subscriptions.SubscriberID  
		 INNER JOIN Plans ON Plans.PlanID = Subscriptions.PlanID  
			AND Subscriptions.Status = 1
	 WHERE Subscribers.SubscriberID = @SubscriberID  
	  AND Subscriptions.BundleID = @OldBundleID
  
	 -- Changes  
	 SELECT ChangeRequestCharges.ChangeRequestID  
	  ,Subscribers.SubscriberID AS SubscriberID  
	  ,AdminServices.PortalServiceName AS PortalServiceName  
	  ,AdminServices.ServiceFee AS ServiceFee  
	  ,AdminServices.IsRecurring AS IsRecurring  
	  ,AdminServices.IsGSTIncluded AS IsGSTIncluded  
	 FROM ChangeRequestCharges  
	 LEFT JOIN SubscriberRequests ON SubscriberRequests.ChangeRequestID = ChangeRequestCharges.ChangeRequestID  
	 LEFT JOIN Subscribers ON Subscribers.SubscriberID = SubscriberRequests.SubscriberID  
	 INNER JOIN AdminServices ON ChangeRequestCharges.AdminServiceID = AdminServices.AdminServiceID  
	 WHERE ChangeRequestCharges.ChangeRequestID = @ChangeRequestID  
  
	 IF @@ROWCOUNT = 0  
	 BEGIN  
	  RETURN 102  
	 END  
	 ELSE  
	  RETURN 105  
END  
GO
/****** Object:  StoredProcedure [dbo].[Customer_AuthenticateCustomer]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_AuthenticateCustomer] @Email NVARCHAR(255)
	,@Password NVARCHAR(4000)
AS
BEGIN
	DECLARE @Status INT = 0

	IF EXISTS (
			SELECT *
			FROM Customers
			WHERE Email = @Email
			)
	BEGIN
		IF EXISTS (
				SELECT *
				FROM Customers
				WHERE Email = @Email
					AND Password = @Password
				)
		BEGIN
			-- Get the status 
			SELECT @Status = STATUS
			FROM Customers
			WHERE Email = @Email
				AND Password = @Password
			-- If email and password is correct but account is already locked then return account locked error
			IF (@Status = 2)
			BEGIN
				RETURN 138
			END

			SELECT Customers.CustomerID
				,Email
				,'Encrypted' AS [Password]
				,MobileNumber
				,ReferralCode
				,Nationality
				,Gender
				,DOB
				,IdentityCardType
				,IdentityCardNumber
				,CASE SMSSubscription
					WHEN 1
						THEN 'Yes'
					ELSE 'No'
					END AS SMSSubscription
				,CASE EmailSubscription
					WHEN 1
						THEN 'Yes'
					ELSE 'No'
					END AS EmailSubscription
				,CASE [Status]
					WHEN 1
						THEN 'Active'
					WHEN 2
						THEN 'Locked'
					ELSE 'InActive'
					END AS [Status]
				,JoinedOn
				,ISNULL(OrderCount, 0) AS OrderCount
			FROM Customers
			LEFT OUTER JOIN (
				SELECT CustomerID
					,IdentityCardType
					,IdentityCardNumber
				FROM (
					SELECT CustomerID
						,IdentityCardType
						,IdentityCardNumber
						,ROW_NUMBER() OVER (
							PARTITION BY CustomerID ORDER BY CreatedOn DESC
							) AS RNUM
					FROM Documents
					) A
				WHERE RNUM = 1
				) CustomerDocuments ON Customers.CustomerID = CustomerDocuments.CustomerID
			LEFT OUTER JOIN (
				SELECT CustomerID
					,COUNT(*) AS OrderCount
				FROM Orders
				INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID
				WHERE OrderStatus = 1 -- Only fetch data if order status is active i.e. 1
				GROUP BY CustomerID
				) CustOrders ON Customers.CustomerID = CustOrders.CustomerID
			WHERE Email = @Email
				AND Password = @Password

			--Register Customer Log  
			INSERT INTO CustomerLogTrail (
				CustomerID
				,ActionDescription
				,ActionOn
				)
			SELECT CustomerID
				,'Login to portal'
				,GETDATE()
			FROM Customers
			WHERE Email = @Email

			-- After successful login set LoginAttempt counter to 0
			UPDATE Customers
			SET LoginAttemptCount = 0
			WHERE Email = @Email
				AND Password = @Password

			RETURN 111 -- Auth success  
		END
		ELSE
		BEGIN
			-- Get Max Retry count for login
			DECLARE @MaxLoginRetryCount INT = 0

			SELECT @MaxLoginRetryCount = ConfigValue
			FROM Config
			WHERE ConfigKey = 'LoginRetryCount'

			-- GEt statsu for Customer using email id only
			SELECT @Status = STATUS
			FROM Customers
			WHERE Email = @Email

			-- if account is locked then return account locked error
			IF (@Status = 2)
			BEGIN
				RETURN 138
			END

			-- Increase the login attempt by 1
			UPDATE Customers
			SET LoginAttemptCount = LoginAttemptCount + 1
			WHERE Email = @Email

			DECLARE @CurrentRetryCount INT = 0
			-- Get current login attempt count
			SELECT @CurrentRetryCount = LoginAttemptCount
			FROM Customers
			WHERE Email = @Email

			-- Compare Current Retry count vs Max retry count value from Config
			IF (@CurrentRetryCount >= @MaxLoginRetryCount)
			BEGIN
				-- Lock account if current counter is greater then max counter
				UPDATE Customers
				SET STATUS = 2
				WHERE Email = @Email

				RETURN 138 -- return account locked error
			END

			RETURN 110 -- Password unmatched  
		END
	END
	ELSE
		RETURN 109 -- email does not exists  
END
GO
/****** Object:  StoredProcedure [dbo].[Customer_AuthenticateToken]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_AuthenticateToken]
	@Token NVARCHAR(1000)
AS 
BEGIN
	IF EXISTS(SELECT CustomerID FROM CustomerToken WHERE Token = @Token)
	BEGIN
		SELECT CustomerID,
			CreatedOn
		FROM CustomerToken
		WHERE Token = @Token
		RETURN 111 -- Auth success
	END
	ELSE
	BEGIN
		RETURN 113 --Token auth failed
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Customer_AuthenticateTokenwithSource]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_AuthenticateTokenwithSource] 
--[Customer_AuthenticateTokenwithSource] 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjIxOSIsIm5iZiI6MTU1NjcwNDM1MiwiZXhwIjoxNTU3MzA5MTUyLCJpYXQiOjE1NTY3MDQzNTJ9.5jyQgKWxr0lcCp-7mD4Obbe0WktRTesZHAxKiYByf2s', 'Orders_CreateOrder'
	@Token NVARCHAR(4000),
	@Source NVARCHAR(255)
AS 
BEGIN
	DECLARE @CustomerID INT
	IF EXISTS(SELECT CustomerID FROM CustomerToken WHERE Token = @Token AND AdminUserID IS NULL)
	BEGIN
		SELECT @CustomerID = CustomerID FROM CustomerToken WHERE Token = @Token
		INSERT INTO CustomerLogTrail
		(
			CustomerID,
			ActionSource,
			ActionDescription,
			ActionOn
		)
		VALUES
		(
			@CustomerID,
			@Source,
			'Action performed by customer for - ' + @Source,
			GETDATE()
		)
		SELECT CustomerID,
			CreatedOn
		FROM CustomerToken
		WHERE Token = @Token
		RETURN 111 -- Auth success
	END
	ELSE IF EXISTS(SELECT CustomerID FROM CustomerToken WHERE Token = @Token AND AdminUserID IS NOT NULL)
	BEGIN
		DECLARE @AdminUserID INT
		SELECT @CustomerID = CustomerID, @AdminUserID = AdminUserID FROM CustomerToken WHERE Token = @Token
		DECLARE @HasPermission INT;
		SELECT @HasPermission = AdminAccessAllowed FROM APIReferences WHERE APISourceName = @Source
		IF(@HasPermission = 1)
		BEGIN
			INSERT INTO AdminUserLogTrail
			(
				AdminUserID,
				ActionSource,
				ActionDescription,
				ActionOn
			)
			VALUES
			(
				@AdminUserID,
				@Source,
				'Viewing customer access from - ' + @Source + ' for customer ID: - ' + CAST(@CustomerID AS nvarchar(50)),
				GETDATE()
			)

			SELECT CustomerID,
				CreatedOn
			FROM CustomerToken
			WHERE Token = @Token
			RETURN 111 -- Auth success
		END
		ELSE
		BEGIN
			INSERT INTO AdminUserLogTrail
			(
				AdminUserID,
				ActionSource,
				ActionDescription,
				ActionOn
			)
			VALUES
			(
				@AdminUserID,
				@Source,
				'ACCESS DENIED - Viewing customer access from - ' + @Source + ' for customer ID: - ' + CAST(@CustomerID AS nvarchar(50)),
				GETDATE()
			)
			RETURN 113 --Token auth failed
		END
	END
	ELSE
	BEGIN
		RETURN 113 --Token auth failed
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Customer_CR_ChangePhoneRequest]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_CR_ChangePhoneRequest] @CustomerID INT  
 ,@MobileNumber NVARCHAR(8)  
 ,@NewMobileNumber NVARCHAR(8)  
 ,@RequestTypeDescription NVARCHAR(50)  
 ,@PremiumType INT  
 --,@PortedNumberTransferForm NVARCHAR(1000)  
 --,@PortedNumberOwnedBy NVARCHAR(1000)  
 --,@PortedNumberOwnerRegistrationID NVARCHAR(1000)  
AS  
BEGIN  
 BEGIN TRY  
  BEGIN TRAN  
  
  DECLARE @CurrentDate DATE;  
  SET @CurrentDate = CAST(GETDATE() AS DATE)  
  
  DECLARE @RequestTypeID INT  
  
  SELECT @RequestTypeID = RequestTypeID  
  FROM RequestTypes  
  WHERE RequestType = @RequestTypeDescription  
  
  DECLARE @Remarks NVARCHAR(50)  
  
  SET @Remarks = 'CR-ChangeNumber'  
  
  DECLARE @AccountID INT  
  DECLARE @SubscriberID INT  
  
  SELECT @AccountID = Accounts.AccountID  
   ,@SubscriberID = SubscriberID  
  FROM Accounts  
  INNER JOIN Subscribers ON Accounts.AccountID = Subscribers.AccountID  
  WHERE CustomerID = @CustomerID  
   AND MobileNumber = @MobileNumber  
  
  IF (@SubscriberID IS NULL)  
  BEGIN  
   ROLLBACK TRAN  
   RETURN 102  
  END  
  
  IF EXISTS (  
    SELECT *  
    FROM ChangeRequests  
    INNER JOIN SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID  
    WHERE SubscriberID = @SubscriberID  
     AND RequestTypeID = @RequestTypeID  
     AND OrderStatus = 0  
    )  
  BEGIN  
   ROLLBACK TRAN  
   RETURN 105  
  END  
  
  INSERT INTO ChangeRequests (  
   RequestTypeID  
   ,RequestReason  
   )  
  SELECT @RequestTypeID  
   ,@Remarks  
  
  DECLARE @ChangeRequestID INT  
  
  SET @ChangeRequestID = SCOPE_IDENTITY();  
  
  UPDATE ChangeRequests  
  SET OrderNumber = dbo.ufnGetCRNumber()  
  WHERE ChangeRequestID = @ChangeRequestID  
  
  INSERT INTO SubscriberRequests (  
   ChangeRequestID  
   ,SubscriberID  
   )  
  VALUES (  
   @ChangeRequestID  
   ,@SubscriberID  
   )  
  
  IF EXISTS (  
    SELECT *  
    FROM RequestTypes  
    WHERE RequestTypeID = @RequestTypeID  
     AND IsChargable = 1  
    )  
  BEGIN  
   INSERT INTO ChangeRequestCharges (  
    ChangeRequestID  
    ,AdminServiceID  
    ,ServiceFee  
    ,IsGSTIncluded  
    ,IsRecurring  
    ,ReasonType  
    ,ReasonID  
    )  
   SELECT @ChangeRequestID  
    ,AdminServiceID  
    ,ServiceFee  
    ,IsGSTIncluded  
    ,IsRecurring  
    ,@Remarks  
    ,@RequestTypeID  
   FROM AdminServices  
   WHERE ServiceType = @RequestTypeDescription  
  END    
  
  INSERT INTO NumberChangeRequests (  
   ChangeRequestID  
   ,NewMobileNumber  
   ,PremiumType  
   ,DonorProvider  
   )  
  --,PortedNumberTransferForm  
  --,PortedNumberOwnedBy  
  --,PortedNumberOwnerRegistrationID  
  SELECT @ChangeRequestID  
   ,@NewMobileNumber  
   ,@PremiumType  
   ,DonorProviderName  
  --,@PortedNumberTransferForm  
  --,@PortedNumberOwnedBy  
  --,@PortedNumberOwnerRegistrationID  
  FROM Subscribers  
  WHERE SubscriberID = @SubscriberID  
  
  --SELECT @ChangeRequestID AS ChangeRequestID, OrderNumber,RequestOn, @RequestTypeDescription FROM ChangeRequests WHERE ChangeRequestID = @ChangeRequestID  
  SELECT PortalServiceName  
   ,SUM(ChangeRequestCharges.ServiceFee) AS ServiceFee  
  FROM ChangeRequestCharges  
  INNER JOIN AdminServices ON ChangeRequestCharges.AdminServiceID = AdminServices.AdminServiceID  
  GROUP BY PortalServiceName  
  
  COMMIT TRAN  
 END TRY  
  
 BEGIN CATCH  
  ROLLBACK TRAN  
 END CATCH  
  
 IF @@ERROR <> 0  
  RETURN 107  
 ELSE  
  RETURN 100  
END  
GO
/****** Object:  StoredProcedure [dbo].[Customer_CreateCustomer]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_CreateCustomer]   
 @Email NVARCHAR(255),  
 @Password NVARCHAR(4000),  
 @ReferralCode NVARCHAR(255)  
AS  
BEGIN  
  
IF NOT EXISTS (SELECT * FROM Customers WHERE Email=@Email)  
BEGIN   
  INSERT INTO Customers (Email, Password, ReferralCode)  
  VALUES (@Email, @Password, @ReferralCode)  
  
  DECLARE @CustomrID INT  
  set @CustomrID = SCOPE_IDENTITY();   
  INSERT INTO Accounts (CustomerID, Email, IsPrimary)  
  VALUES ( @CustomrID, @Email, 1)   
  
  SELECT   
   CustomerID,  
   Email,
   Name,
   'Encrypted' as [Password],  
   MobileNumber,  
   ReferralCode,  
   Nationality,  
   Gender,  
   CASE SMSSubscription WHEN 1 THEN 'Yes' ELSE 'No' END AS SMSSubscription,  
   CASE EmailSubscription WHEN 1 THEN 'Yes' ELSE 'No' END AS EmailSubscription,  
   CASE [Status] WHEN 1 THEN 'Active' ELSE 'InActive' END AS [Status]  
  FROM Customers where CustomerID=@CustomrID  
  
  RETURN 100 -- creation success  
END  
  
ELSE  
 BEGIN  
  RETURN 108 -- EMAIL EXISTS  
 END  
END  
GO
/****** Object:  StoredProcedure [dbo].[Customer_CreateToken]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_CreateToken]-- 139, 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjEzOSIsIm5iZiI6MTU1NjAxMTU2NCwiZXhwIjoxNTU2NjE2MzY0LCJpYXQiOjE1NTYwMTE1NjR9.umpOXHDbL-a3gZjOuX9rwm7SHFVqsTw2av80btP70sY'
	@CustomerID INT,
	@Token nvarchar(1000)
AS 
BEGIN
	IF EXISTS(SELECT CustomerID FROM CustomerToken WHERE Token = @Token AND AdminUserID IS NULL)
	BEGIN
		SELECT CustomerID,
			CreatedOn
		FROM CustomerToken
		WHERE Token = @Token AND AdminUserID IS NULL
		RETURN 105 -- Already Exists
	END
	ELSE IF EXISTS(SELECT CustomerID FROM CustomerToken WHERE CustomerID = @CustomerID AND AdminUserID IS NULL)
	BEGIN
		UPDATE CustomerToken SET Token = @Token,CreatedOn=GETDATE() WHERE CustomerID = @CustomerID AND AdminUserID IS NULL
		RETURN 202 -- Updated
	END
	ELSE
	BEGIN
		INSERT INTO CustomerToken
		(
			CustomerID,
			Token,
			CreatedOn
		)
		VALUES
		(
			@CustomerID,
			@Token,
			GETDATE()
		)
		RETURN 100 -- created
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Customer_ForgotPassword]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_ForgotPassword]
	@EmailAddress NVARCHAR(255)
AS
BEGIN
	DECLARE @CustomerID INT
	IF EXISTS(SELECT CustomerID FROM Customers WHERE Email = @EmailAddress)
	BEGIN
		SELECT @CustomerID = CustomerID		
		FROM Customers WHERE Email = @EmailAddress

		UPDATE ForgotPasswordTokens SET IsUsed = 1 
		WHERE CustomerID= @CustomerID AND DATEDIFF(second, GeneratedOn, GETDATE()) >0 
		
		INSERT INTO ForgotPasswordTokens
		(
			CustomerID,
			Token,			
			GeneratedOn
		)
		VALUES
		(
			@CustomerID,
			NEWID(),		
			GETDATE()
		)

		SELECT pt.CustomerID, pt.Token,c.[Name],c.Email FROM ForgotPasswordTokens pt inner join Customers c 
					on c.CustomerID=pt.CustomerID WHERE pt.CustomerID =@CustomerID  and IsUsed=0

		IF @@ERROR<>0

		RETURN 107 -- failed to generate token

		RETURN 100 -- token generated success
	END
	ELSE 
	BEGIN 
		RETURN 109 -- email does not exists
	END
END

SELECT DATEADD(d, -1, GETDATE())
GO
/****** Object:  StoredProcedure [dbo].[Customer_GetCustomerByEmail]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_GetCustomerByEmail] 
	@Email NVARCHAR(255)
AS
BEGIN	
		SELECT	
			CustomerID,
			Email,
			'Encrypted' as [Password],
			MobileNumber,
			ReferralCode,
			Nationality,
			Gender,
			CASE SMSSubscription WHEN 1 THEN 'Yes' ELSE 'No' END AS SMSSubscription,
			CASE EmailSubscription WHEN 1 THEN 'Yes' ELSE 'No' END AS EmailSubscription,
			CASE [Status] WHEN 1 THEN 'Active' ELSE 'InActive' END AS [Status]
			FROM Customers	where Email=@Email
		
	END
	

GO
/****** Object:  StoredProcedure [dbo].[Customer_GetDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_GetDetails] --38
	@CustomerID INT	
AS
BEGIN
	DECLARE @MaxSunscriberCount INT
	SELECT @MaxSunscriberCount = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'MaxSubscribersPerAccount';

	
	IF EXISTS (SELECT * FROM Customers WHERE CustomerID=@CustomerID)
	BEGIN	
		SELECT	
			Customers.CustomerID,
			Name,
			Email,
			'Encrypted' as [Password],
			MobileNumber,
			ReferralCode,
			Nationality,
			Gender,
			DOB,
			IdentityCardType,
			IdentityCardNumber,
			CASE SMSSubscription WHEN 1 THEN 'Yes' ELSE 'No' END AS SMSSubscription,
			CASE EmailSubscription WHEN 1 THEN 'Yes' ELSE 'No' END AS EmailSubscription,
			CASE [Status] WHEN 1 THEN 'Active' ELSE 'InActive' END AS [Status],
			JoinedOn,
			ISNULL(CustOrder.OrderCount, 0) AS OrderCount,
			CASE WHEN @MaxSunscriberCount < SubCount.SubscriberCount THEN 0 ELSE (@MaxSunscriberCount - SubCount.SubscriberCount) END AS PendingAllowedSubscribers
		FROM Customers LEFT OUTER JOIN 
			(
				SELECT CustomerID, IdentityCardType, IdentityCardNumber
				FROM
				(
					SELECT CustomerID, IdentityCardType, IdentityCardNumber, ROW_NUMBER() OVER(Partition BY CustomerID Order BY CreatedOn DESC) AS RNUM
					FROM Documents
				)A 
				WHERE RNUM = 1
			)CustomerDocuments ON Customers.CustomerID = CustomerDocuments.CustomerID LEFT OUTER JOIN 
			(
				SELECT CustomerID, COUNT(MobileNumber) AS SubscriberCount
				FROM Accounts LEFT OUTER JOIN
				(
					SELECT AccountID, MobileNumber FROM Subscribers WHERE IsBuddyLine = 0
				) A ON Accounts.AccountID = A.AccountID
				GROUP BY CustomerID
			)SubCount ON Customers.CustomerID = SubCount.CustomerID LEFT OUTER JOIN 
			(
				SELECT CustomerID, COUNT(OrderID) AS OrderCount
				FROM Orders INNER JOIN	
					Accounts ON Orders.AccountID = Accounts.AccountID 
				WHERE (OrderStatus = 1 OR OrderStatus = 2 OR OrderStatus = 3 OR OrderStatus = 4 OR OrderStatus = 5 OR OrderStatus = 6 OR OrderStatus = 7)
				GROUP BY CustomerID
			)CustOrder ON Customers.CustomerID = CustOrder.CustomerID
		where Customers.CustomerID = @CustomerID

		RETURN 111 -- Auth success

	END

	ELSE

	RETURN 109 -- email does not exists
		
END
	

GO
/****** Object:  StoredProcedure [dbo].[Customer_ResetPassword]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_ResetPassword]
	@Token NVARCHAR(255),
	@Password NVARCHAR(255)
AS
BEGIN
	DECLARE @CustID INT	
	IF EXISTS(SELECT CustomerID FROM ForgotPasswordTokens WHERE Token = @Token)
	
	BEGIN		
		SELECT @CustID = CustomerID	FROM ForgotPasswordTokens WHERE Token = @Token
						
			select * from ForgotPasswordTokens WHERE CustomerID = @CustID and Token=@Token 
					AND IsUsed = 0
					AND DATEDIFF(MINUTE, GeneratedOn, GETDATE()) <=30

			IF @@ROWCOUNT>0
			begin
			
			Update Customers set Password=@Password, Status = CASE WHEN c.Status = 2 THEN 1 ELSE c.Status END FROM Customers INNER JOIN Customers c ON Customers.CustomerID = c.CustomerID 
			where Customers.CustomerID=@CustID
			UPDATE ForgotPasswordTokens SET IsUsed = 1 WHERE Token = @Token
			IF @@ERROR<> 0

				RETURN  106-- Updation failed

				ELSE RETURN 101 -- Update Success
			end
			
			ELSE

			SELECT 125 --TOKEN EXPIRED

		END		
	
	ELSE 
	BEGIN 
		RETURN 102 -- Token does not exists
	END
END

GO
/****** Object:  StoredProcedure [dbo].[Customer_SearchCustomers]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[Customer_SearchCustomers] --[Customer_SearchCustomers] '%simon%'
	@SearchValue NVARCHAR(500)
AS
BEGIN
	SELECT 
		Customers.CustomerID,
		[Name],
		Customers.Email,
		PrimarySubscribers.MobileNumber,
		PrimarySubscribers.PlanMarketingName AS PlanName,
		ISNULL(AccountSubscribers.LineCount,0) AS [AdditionalLines],
		JoinedOn,
		IIF(Customers.[Status]=1,'Active','Inactive')[Status],
		ReferralCode,
		Nationality,
		Gender,
		Customers.DOB,
		CASE EmailSubscription WHEN 1 THEN 'Yes' ELSE 'No' END AS EmailSubscription
	FROM Customers 
		LEFT JOIN Accounts ON Customers.CustomerID=Accounts.CustomerID
		LEFT JOIN 
		(
			SELECT AccountID, PlanMarketingName, MobileNumber
			FROM 
			(
				SELECT AccountID, Plans.PlanMarketingName, MobileNumber, ROW_NUMBER() OVER(PARTITION BY MobileNumber, AccountID ORDER BY Subscribers.CreatedOn) AS RNum
				FROM Subscribers 
					LEFT JOIN Subscriptions ON Subscriptions.SubscriberID=Subscribers.SubscriberID
					LEFT JOIN Plans ON Plans.planID=Subscriptions.PlanID 
				WHERE IsPrimary = 1 AND Plans.PlanType = 0
			) A 
			WHERE RNum = 1
		)PrimarySubscribers ON PrimarySubscribers.AccountID =Accounts.AccountID
		LEFT JOIN 
		(
			SELECT AccountID, COUNT(*) AS LineCount
			FROM Subscribers 
			GROUP BY AccountID
		)AccountSubscribers ON AccountSubscribers.AccountID =Accounts.AccountID
	WHERE (Customers.Email LIKE '%' + @SearchValue + '%'
		OR Customers.MobileNumber LIKE '%' + @SearchValue + '%' 
		OR [Name] LIKE '%' + @SearchValue + '%' 
		OR BillingAccountNumber LIKE '%' + @SearchValue + '%')
		AND AccountSubscribers.LineCount > 0
END
GO
/****** Object:  StoredProcedure [dbo].[Customer_UpdateCustomerProfile]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_UpdateCustomerProfile] 
	@CustomerID INT,	
	@Password NVARCHAR(4000),
	@MobileNumber NVARCHAR(15) = NULL,
	@Email NVARCHAR(600) = NULL
AS
BEGIN

IF EXISTS (SELECT * FROM Customers WHERE CustomerID=@CustomerID)
BEGIN			
		IF(@Password IS NULL)
		BEGIN
			UPDATE Customers 
			SET MobileNumber = CASE WHEN @MobileNumber IS NULL THEN MobileNumber ELSE @MobileNumber END,
				Email = CASE WHEN @Email IS NULL THEN Email ELSE @Email END
			WHERE CustomerID = @CustomerID	
		END
		ELSE
		BEGIN
			UPDATE Customers 
			SET Password = @Password,
				MobileNumber = CASE WHEN @MobileNumber IS NULL THEN MobileNumber ELSE @MobileNumber END,
				Email = CASE WHEN @Email IS NULL THEN Email ELSE @Email END
			WHERE CustomerID = @CustomerID	
		END
		RETURN 101 --Updated success
END
ELSE
	BEGIN
		RETURN 102 -- recor does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Customer_ValidatePassword]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_ValidatePassword] @Email NVARCHAR(255)  
 ,@Password NVARCHAR(4000)  
AS  
BEGIN  
 DECLARE @Status INT = 0  
   IF EXISTS (  
    SELECT *  
    FROM Customers  
    WHERE Email = @Email  
     AND Password = @Password  
    )  
	BEGIN
		RETURN 111 -- Auth success
	END
	ELSE
	BEGIN
		RETURN 110 -- Password unmatched   
	END
 
END  
GO
/****** Object:  StoredProcedure [dbo].[Customer_ValidateResetPasswordToken]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_ValidateResetPasswordToken]
	@Token NVARCHAR(255)
	
AS
BEGIN
	DECLARE @CustID INT
	
	IF EXISTS(SELECT CustomerID FROM ForgotPasswordTokens WHERE Token = @Token)
	
	BEGIN		
		SELECT @CustID = CustomerID	FROM ForgotPasswordTokens WHERE Token = @Token

		
		
			select * from ForgotPasswordTokens WHERE CustomerID = @CustID and Token=@Token 
					
					AND DATEDIFF(MINUTE, GeneratedOn, GETDATE()) <=30

			IF @@ROWCOUNT>0

			RETURN 124 --  VALID TOKEN

			ELSE

				RETURN 125 --TOKEN EXPIRED

		END		
	
	ELSE 
	BEGIN 
		RETURN 102 -- Token does not exists
	END
END

GO
/****** Object:  StoredProcedure [dbo].[Customers_GetAccessNewTokenDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_GetAccessNewTokenDetails] --[Customers_GetAccessTokenDetails] '84128DAD-1B17-4BBC-80F5-DA4277A55921'
	@AccessToken NVARCHAR(255),
	@Token NVARCHAR(255)
AS
BEGIN
	IF EXISTS(SELECT * FROM AdminAccessTokens WHERE Token = @AccessToken AND IsUsed = 0 AND DATEDIFF(MINUTE, GETDATE(), CreatedOn) < 30)
	BEGIN
		DECLARE @CustomerID INT
		DECLARE @AdminUserID INT
		SELECT @CustomerID = CustomerID, @AdminUserID = AdminUserID FROM AdminAccessTokens WHERE Token = @AccessToken

		INSERT INTO AdminUserLogTrail
		(
			AdminUserID,
			ActionDescription,
			ActionOn
		)
		VALUES
		(
			@AdminUserID,
			'Viewing customer dashboard for customer ID: ' + CAST(@CustomerID AS nvarchar(50)),
			GETDATE()
		)
		UPDATE AdminAccessTokens SET IsUsed = 1 WHERE Token = @AccessToken

		INSERT INTO CustomerToken
		(
			CustomerID,
			AdminUserID,
			Token,
			CreatedOn
		)
		VALUES
		(
			@CustomerID,
			@AdminUserID,
			@Token,
			GETDATE()
		)
		
		SELECT @AdminUserID AS AdminUserID,
			CustomerID,
			Token
		FROM CustomerToken
		WHERE CustomerID = @CustomerID
		ORDER BY CreatedOn DESC
		RETURN 105;
	END
	ELSE
	BEGIN
		RETURN 119;
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetAccessTokenDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_GetAccessTokenDetails] --[Customers_GetAccessTokenDetails] '84128DAD-1B17-4BBC-80F5-DA4277A55921'
	@Token NVARCHAR(255)
AS
BEGIN
	IF EXISTS(SELECT * FROM AdminAccessTokens WHERE Token = @Token AND IsUsed = 0 AND DATEDIFF(MINUTE, GETDATE(), CreatedOn) < 30)
	BEGIN
		DECLARE @CustomerID INT
		DECLARE @AdminUserID INT
		SELECT @CustomerID = CustomerID, @AdminUserID = AdminUserID FROM AdminAccessTokens WHERE Token = @Token

		INSERT INTO AdminUserLogTrail
		(
			AdminUserID,
			ActionDescription,
			ActionOn
		)
		VALUES
		(
			@AdminUserID,
			'Viewing customer dashboard for customer ID: ' + CAST(@CustomerID AS nvarchar(50)),
			GETDATE()
		)
		IF EXISTS(SELECT Token FROM CustomerToken WHERE CustomerID = @CustomerID AND AdminUserID = @AdminUserID)
		BEGIN
			UPDATE AdminAccessTokens SET IsUsed = 1 WHERE Token = @Token
			IF EXISTS(SELECT * FROM CustomerToken WHERE  CustomerID = @CustomerID AND AdminUserID = @AdminUserID AND DATEDIFF(DAY, GETDATE(), CreatedOn) < 7)
			BEGIN
				SELECT @AdminUserID AS AdminUserID,
					CustomerID,
					Token
				FROM CustomerToken
				WHERE  CustomerID = @CustomerID AND AdminUserID = @AdminUserID
				ORDER BY CreatedOn DESC
			END
			ELSE
			BEGIN
				UPDATE CustomerToken SET CreatedOn=GETDATE() WHERE  CustomerID = @CustomerID AND AdminUserID = @AdminUserID
				
				SELECT @AdminUserID AS AdminUserID,
					CustomerID,
					Token
				FROM CustomerToken
				WHERE  CustomerID = @CustomerID AND AdminUserID = @AdminUserID
				ORDER BY CreatedOn DESC
			END
			RETURN 105;
		END 
		ELSE 
		BEGIN
			SELECT @CustomerID AS CustomerID;
			RETURN 113;
		END
	END
	ELSE
	BEGIN
		RETURN 119;
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetAccountID]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_GetAccountID] --[Customers_GetOrders] 38
	@CustomerID INT
AS
BEGIN
	IF EXISTS(SELECT CustomerID FROM Customers WHERE CustomerID = @CustomerID)
	BEGIN
		SELECT AccountID FROM Accounts WHERE CustomerID = @CustomerID

		RETURN 105
	END
	ELSE
	BEGIN
		RETURN 102 -- does not exists
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetBaseBundle]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Customers_GetBaseBundle] --Customers_GetBaseBundle 176, '88400140'
	@CustomerID INT,
	@MobileNumber NVARCHAR(8)
AS
BEGIN
	DECLARE @CurrentDate DATE = CAST(GETDATE() AS date);
	IF EXISTS (SELECT * FROM Subscribers INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID WHERE CustomerID=@CustomerID)
	BEGIN	
		SELECT 
			Bundles.BundleID,
			Bundles.PlanMarketingName,
			Bundles.PortalSummaryDescription,
			Bundles.PortalDescription,		
			Subscribers.MobileNumber,
			TotalData,
			TotalSMS,
			TotalVoice,
			ActualSubscriptionFee,
			ApplicableSubscriptionFee
		FROM Customers 
			INNER JOIN Accounts ON Customers.CustomerID = Accounts.CustomerID
			INNER JOIN Subscribers ON Subscribers.AccountID = Accounts.AccountID
			INNER JOIN Subscriptions ON Subscriptions.SubscriberID = Subscribers.SubscriberID
			INNER JOIN Plans ON Subscriptions.PlanID = Plans.PlanID
				AND Plans.PlanType = 0
			INNER JOIN Bundles ON Subscriptions.BundleID = Bundles.BundleID 
			INNER JOIN 
			(
				SELECT SUM(ISNULL([Data], 0)) AS TotalData, 
					SUM(ISNULL(Voice, 0)) AS TotalVoice, 
					SUM(ISNULL([SMS], 0)) AS TotalSMS,
					SUM((ISNULL(SubscriptionFee, 0))) AS ActualSubscriptionFee, 
					SUM((ISNULL(SubscriptionFee, 0) * (1 - BundlePlans.DiscountPercentage/100))) AS ApplicableSubscriptionFee,
					Bundles.BundleID
				FROM Bundles INNER JOIN 
					BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN
					Plans ON BundlePlans.PlanID = Plans.PlanID
				GROUP BY Bundles.BundleID
			) AS BundleOffering ON BundleOffering.BundleID = Bundles.BundleID
		WHERE Customers.CustomerID = @CustomerID 
			AND (Subscriptions.Status = 0 OR Subscriptions.Status = 1) 
			AND ISNULL(Subscriptions.ValidTo, @CurrentDate) <= @CurrentDate
			AND Subscribers.MobileNumber = @MobileNumber
	
		RETURN 105 -- success
	END
	ELSE
		RETURN 109 -- email does not exists
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetBillingDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_GetBillingDetails]
	@CustomerID INT
AS
BEGIN
	IF EXISTS (SELECT * FROM Customers WHERE CustomerID=@CustomerID)
	BEGIN	
		SELECT	
			Name,
			BillingUnit,
			BillingFloor,
			BillingStreetName,
			BillingBuildingNumber,
			BillingBuildingName,
			BillingContactNumber,
			BillingPostCode
		FROM Customers INNER JOIN
			Accounts ON Customers.CustomerID = Accounts.CustomerID
		WHERE Customers.CustomerID = @CustomerID
		RETURN 105 -- success

	END

	ELSE

	RETURN 109 -- email does not exists
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetBundlesListing]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_GetBundlesListing] --[Customers_GetBundlesListing] 176, '88400140'
	@CustomerID INT,
	@MobileNumber NVARCHAR(8)
AS
BEGIN
	DECLARE @CurrentDate AS DATE
	SET @CurrentDate = CAST(GETDATE() AS DATE);
	IF EXISTS (SELECT * FROM Subscribers INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID WHERE CustomerID=@CustomerID)
	BEGIN	

		DECLARE @OldPlanBundleID INT
	
		SELECT @OldPlanBundleID = Bundles.BundleID -- 0
		FROM Customers 
			INNER JOIN Accounts ON Customers.CustomerID = Accounts.CustomerID
			INNER JOIN Subscribers ON Subscribers.AccountID = Accounts.AccountID
			INNER JOIN Subscriptions ON Subscriptions.SubscriberID = Subscribers.SubscriberID
			INNER JOIN Plans ON Subscriptions.PlanID = Plans.PlanID
				AND Plans.PlanType = 0
			INNER JOIN Bundles ON Subscriptions.BundleID = Bundles.BundleID 
		WHERE Customers.CustomerID = @CustomerID 
			AND (Subscriptions.Status = 0 OR Subscriptions.Status = 1) 
			AND ISNULL(Subscriptions.ValidTo, @CurrentDate) <= @CurrentDate
			AND Subscribers.MobileNumber = @MobileNumber

		SELECT 
			Bundles.BundleID,
			BundleName,
			PortalDescription,
			PortalSummaryDescription,
			Bundles.PlanMarketingName,
			TotalData,
			TotalSMS,
			TotalVoice,
			ActualSubscriptionFee,
			ApplicableSubscriptionFee,
			ServiceName,
			ActualServiceFee,
			ApplicableServiceFee,
			'' AS PromotionText
		FROM Bundles INNER JOIN 
			(
				SELECT SUM(ISNULL([Data], 0)) AS TotalData, 
					SUM(ISNULL(Voice, 0)) AS TotalVoice, 
					SUM(ISNULL([SMS], 0)) AS TotalSMS,
					SUM((ISNULL(SubscriptionFee, 0))) AS ActualSubscriptionFee, 
					SUM((ISNULL(SubscriptionFee, 0) * (1 - BundlePlans.DiscountPercentage/100))) AS ApplicableSubscriptionFee,
					Bundles.BundleID
				FROM Bundles INNER JOIN 
					BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN
					Plans ON BundlePlans.PlanID = Plans.PlanID
				GROUP BY Bundles.BundleID
			) AS BundleOffering ON BundleOffering.BundleID = Bundles.BundleID INNER JOIN 
			(
				SELECT Bundles.BundleID, Bundles.PlanMarketingName
				FROM Bundles INNER JOIN 
					BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN
					Plans ON BundlePlans.PlanID = Plans.PlanID
				WHERE Plans.PlanType = 0
			) AS BundleBasePlan ON BundleBasePlan.BundleID = Bundles.BundleID LEFT OUTER JOIN
			(
				SELECT Bundles.BundleID,
					SUM(ISNULL(AdminServices.ServiceFee, 0)) AS ActualServiceFee, 
					SUM(ISNULL(AdminServices.ServiceFee, 0) * (1 - ISNULL(PlanAdminServices.AdminServiceDiscountPercentage, 0)/100)) AS ApplicableServiceFee,
					STUFF(
						 (SELECT ',' + PortalServiceName 
						  FROM AdminServices INNER JOIN 
							PlanAdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID
						  WHERE PlanAdminServices.BundleID = Bundles.BundleID
						  FOR XML PATH (''))
						 , 1, 1, '') AS ServiceName
				FROM AdminServices INNER JOIN 
					PlanAdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID INNER JOIN
					Bundles ON Bundles.BundleID = PlanAdminServices.BundleID
				GROUP BY Bundles.BundleID
			) AdminFee ON AdminFee.BundleID = Bundles.BundleID 
		WHERE Bundles.IsCustomerSelectable = 1 
			AND Bundles.Status = 1
			AND @CurrentDate BETWEEN ISNULL(Bundles.ValidFrom, @CurrentDate) AND ISNULL(Bundles.ValidTo, @CurrentDate)
			AND Bundles.BundleID <> @OldPlanBundleID
	RETURN 105 -- success
	END
	ELSE
		RETURN 109 -- email does not exists
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetCustomerChangeRequests]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Customers_GetCustomerChangeRequests] --[Admin_GetCustomerChangeRequests] 38
	@CustomerID INT
AS
BEGIN
	IF EXISTS(SELECT CustomerID FROM Customers WHERE CustomerID = @CustomerID)
	BEGIN
		DECLARE @AccountID INT
		SELECT @AccountID = AccountID FROM Accounts WHERE CustomerID = @CustomerID

		SELECT ChangeRequests.ChangeRequestID,
			Subscribers.SubscriberID,
			Subscribers.MobileNumber,
			OrderNumber, 
			RequestType,
			RequestOn,
			BillingUnit,
			BillingFloor,
			BillingBuildingNumber,
			BillingBuildingName,
			BillingStreetName,
			BillingPostCode,
			BillingContactNumber,
			DeliveryInformation.[Name],
			DeliveryInformation.Email,
			IDType,
			IDNumber,
			IsSameAsBilling,
			ShippingUnit,
			ShippingFloor,
			ShippingBuildingNumber,
			ShippingBuildingName,
			ShippingStreetName,
			ShippingPostCode,
			ShippingContactNumber,
			AlternateRecipientContact,
			AlternateRecipientName,
			AlternateRecipientEmail,
			DeliveryInformation.PortalSlotID,
			DeliverySlots.SlotDate,
			DeliverySlots.SlotFromTime,
			DeliverySlots.SlotToTime,
			ScheduledDate,
			ISNULL(CRCharges.ServiceFee, 0) AS ServiceFee,
			CASE OrderStatus 
				WHEN -1 THEN 'expired'--;0=Initiated;1=Paid;2=Processed;3=Shipped;4=Cancelled;5=PaymentDenied'
				WHEN 0 THEN 'initiated'
				WHEN 1 THEN 'paid and pending'
				WHEN 2 THEN 'sent to delivery vendor'
				WHEN  3 THEN 'delivery order fulfilled'
				WHEN  4 THEN 'out for delivery'
				WHEN  5 THEN 'delivered'
				WHEN  6 THEN 'delivery failed'
				WHEN  7 THEN 'service activated'
				WHEN  8 THEN 'cancelled'
				WHEN  9 THEN 'payment denied'
				ELSE 'not found'
			END AS OrderStatus,
			CASE  
				WHEN OrderStatus = 7 THEN 'completed'
				ELSE 'pending'
			END AS ListingStatus,
			CASE ChangeRequests.RequestTypeID 
			WHEN 9 THEN 1
			ELSE 0 
			END AS AllowRescheduling
		FROM ChangeRequests INNER JOIN
			RequestTypes ON ChangeRequests.RequestTypeID = RequestTypes.RequestTypeID INNER JOIN
			SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID INNER JOIN 
			Subscribers ON SubscriberRequests.SubscriberID = Subscribers.SubscriberID LEFT OUTER JOIN 
			DeliveryInformation ON ChangeRequests.DeliveryInformationID = DeliveryInformation.DeliveryInformationID LEFT OUTER JOIN 
			(	
				SELECT ChangeRequestID, SUM(ChangeRequestCharges.ServiceFee) AS ServiceFee
				FROM ChangeRequestCharges
				GROUP BY ChangeRequestID
			)CRCharges ON ChangeRequests.ChangeRequestID = CRCharges.ChangeRequestID LEFT OUTER JOIN 
			DeliverySlots ON DeliveryInformation.PortalSlotID = DeliverySlots.PortalSlotID 
		WHERE Subscribers.AccountID = @AccountID AND (OrderStatus = 1 OR OrderStatus = 2 OR OrderStatus = 3 OR OrderStatus = 4 OR
														OrderStatus = 5 OR OrderStatus = 6 OR OrderStatus = 7 OR OrderStatus = 8  OR OrderStatus = 9)
		AND (ChangeRequests.RequestTypeID = 5 OR ChangeRequests.RequestTypeID = 6 OR ChangeRequests.RequestTypeID = 7 OR ChangeRequests.RequestTypeID = 8 OR ChangeRequests.RequestTypeID = 9)
		---1=expired;0=initiated;1=paid and pending,2=sent to delivery vendor, 3=delivery order fulfilled, 4=out for delivery, 5=delivered, 6=delivery failed, 7=service activated, 8=cancelled;9=payment denied
		RETURN 100
	END
	ELSE
	BEGIN
		RETURN 119 -- does not exists
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetCustomerReferralCode]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_GetCustomerReferralCode] --Customers_GetCustomerReferralCode 'GRID'
	@ReferralCode NVARCHAR(50)
AS
BEGIN
	IF EXISTS(SELECT CustomerID FROM Customers WHERE ReferralCode = @ReferralCode)
	BEGIN
		SELECT CustomerID FROM Customers WHERE ReferralCode = @ReferralCode
	END
	ELSE
	BEGIN
		SELECT -1
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetOrders]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_GetOrders] --[Customers_GetOrders] 38
	@CustomerID INT
AS
BEGIN
	IF EXISTS(SELECT CustomerID FROM Customers WHERE CustomerID = @CustomerID)
	BEGIN
		DECLARE @AccountID INT
		SELECT @AccountID = AccountID FROM Accounts WHERE CustomerID = @CustomerID

		SELECT Orders.OrderID,
			OrderNumber, 
			OrderDate,
			Documents.IdentityCardNumber,
			Documents.IdentityCardType,
			BillingUnit,
			BillingFloor,
			BillingBuildingNumber,
			BillingBuildingName,
			BillingStreetName,
			BillingPostCode,
			BillingContactNumber,
			ReferralCode,
			PromotionCode,
			CAST(CASE OrderDocs.DocCount WHEN 0 THEN 0 ELSE 1 END AS bit) AS HaveDocuments,
			DeliveryInformation.[Name],
			DeliveryInformation.Email,
			IDType,
			IDNumber,
			IsSameAsBilling,
			ShippingUnit,
			ShippingFloor,
			ShippingBuildingNumber,
			ShippingBuildingName,
			ShippingStreetName,
			ShippingPostCode,
			ShippingContactNumber,
			AlternateRecipientContact,
			AlternateRecipientName,
			AlternateRecipientEmail,
			DeliveryInformation.PortalSlotID,
			DeliverySlots.SlotDate,
			DeliverySlots.SlotFromTime,
			DeliverySlots.SlotToTime,
			ScheduledDate,
			ISNULL(OrderCharges.ServiceFee, 0) AS ServiceFee,
			CASE OrderStatus 
				WHEN -1 THEN 'expired'
				WHEN 0 THEN 'initiated'
				WHEN 1 THEN 'paid and pending'
				WHEN 2 THEN 'sent to delivery vendor'
				WHEN  3 THEN 'delivery order fulfilled'
				WHEN  4 THEN 'out for delivery'
				WHEN  5 THEN 'delivered'
				WHEN  6 THEN 'delivery failed'
				WHEN  7 THEN 'service activated'
				WHEN  8 THEN 'cancelled'
				ELSE 'payment denied'
			END AS OrderStatus,
			CASE  
				WHEN OrderStatus = 7 THEN 'completed'
				ELSE 'pending'
			END AS ListingStatus,
			CASE WHEN OrderStatus = 1 OR OrderStatus = 6 THEN 1 ELSE 0 END AS AllowRescheduling,
			InvoiceNumber,
			MaskedCardNumber,
			CardBrand,
			Payments.ExpiryMonth,
			ExpiryYear,
			Payments.PaymentOn
		FROM Orders LEFT OUTER JOIN
			OrderDocuments ON Orders.OrderID = OrderDocuments.OrderID LEFT OUTER JOIN
			Documents ON OrderDocuments.DocumentID = Documents.DocumentID LEFT OUTER JOIN 
			DeliveryInformation ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID LEFT OUTER JOIN 
			(	SELECT OrderID, SUM(ISNULL(ServiceFee, 0)) AS ServiceFee
				FROM 
				(
					SELECT OrderID, SUM(OrderCharges.ServiceFee) AS ServiceFee
					FROM OrderCharges
					GROUP BY OrderID

					UNION ALL
				
					SELECT OrderID, SUM(SubscriberCharges.ServiceFee) AS ServiceFee
					FROM SubscriberCharges 
						INNER JOIN OrderSubscribers ON SubscriberCharges.OrderSubscriberID = OrderSubscribers.OrderSubscriberID
					GROUP BY OrderID
				)A
				GROUP BY OrderID
			)OrderCharges ON Orders.OrderID = OrderCharges.OrderID LEFT OUTER JOIN 
			DeliverySlots ON DeliveryInformation.PortalSlotID = DeliverySlots.PortalSlotID LEFT OUTER JOIN 
			(	
				SELECT OrderID, COUNT(Documents.IdentityCardType) AS DocCount
				FROM OrderDocuments INNER JOIN 
					Documents ON OrderDocuments.DocumentID = Documents.DocumentID
				GROUP BY OrderID
			)OrderDocs ON Orders.OrderID = OrderDocs.OrderID INNER JOIN 
			Payments ON Payments.PaymentID = Orders.PaymentID
		WHERE Orders.AccountID = @AccountID AND (OrderStatus = 1 OR OrderStatus = 2 OR OrderStatus = 3 OR OrderStatus = 4 OR OrderStatus = 5 OR OrderStatus = 6 OR OrderStatus = 7)
		---1=expired;0=initiated;1=paid and pending,2=sent to delivery vendor, 3=delivery order fulfilled, 4=out for delivery, 5=delivered, 6=delivery failed, 7=service activated, 8=cancelled;9=payment denied

		SELECT 
			OrderID,
			OrderSubscriberID,
			SubscriberID,
			Subscribers.MobileNumber,
			Subscribers.IsPrimary,
			Subscribers.DisplayName,
			ActivatedOn,
			Subscribers.DepositFee,
			Subscribers.IsBuddyLine,
			Subscribers.PremiumType,
			AdminServices.PortalServiceName AS PremiumName,
			Subscribers.IsPorted
		FROM OrderSubscribers 
			INNER JOIN Subscribers ON OrderSubscribers.OrderSubscriberID = Subscribers.RefOrderSubscriberID
			LEFT JOIN AdminServices ON Subscribers.PremiumType = AdminServices.ServiceCode
				AND (ServiceType = 'Premium' OR ServiceType = 'Free')
		WHERE AccountID = @AccountID

		SELECT
			OrderSubscribers.OrderID,
			OrderSubscribers.OrderSubscriberID,
			OrderBundle.BundleID,
			Bundles.PlanMarketingName,
			Bundles.PortalDescription,
			Bundles.PortalSummaryDescription,
			TotalData,
			TotalSMS,
			TotalVoice,
			ActualSubscriptionFee,
			ApplicableSubscriptionFee,
			ServiceName,
			ActualServiceFee,
			ApplicableServiceFee	
		FROM Orders INNER JOIN 
			OrderSubscribers ON Orders.OrderID = OrderSubscribers.OrderID INNER JOIN 
			(
				SELECT OrderSubscriberID, BundleID 
				FROM OrderSubscriberChangeRequests INNER JOIN 
					ChangeRequests ON OrderSubscriberChangeRequests.ChangeRequestID = ChangeRequests.ChangeRequestID INNER JOIN 
					OrderSubscriptions ON ChangeRequests.ChangeRequestID = OrderSubscriptions.ChangeRequestID
				GROUP BY OrderSubscriberID, BundleID 
			) OrderBundle ON OrderSubscribers.OrderSubscriberID = OrderBundle.OrderSubscriberID INNER JOIN 
			Bundles ON Bundles.BundleID = OrderBundle.BundleID INNER JOIN 
			(
				SELECT SUM(ISNULL([Data], 0)) AS TotalData, 
					SUM(ISNULL(Voice, 0)) AS TotalVoice, 
					SUM(ISNULL([SMS], 0)) AS TotalSMS,
					SUM((ISNULL(SubscriptionFee, 0))) AS ActualSubscriptionFee, 
					SUM((ISNULL(SubscriptionFee, 0) * (1 - BundlePlans.DiscountPercentage/100))) AS ApplicableSubscriptionFee,
					Bundles.BundleID
				FROM Bundles INNER JOIN 
					BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN
					Plans ON BundlePlans.PlanID = Plans.PlanID
				GROUP BY Bundles.BundleID
			) AS BundleOffering ON BundleOffering.BundleID = Bundles.BundleID LEFT JOIN 
			(
				SELECT Bundles.BundleID,
					SUM(ISNULL(AdminServices.ServiceFee, 0)) AS ActualServiceFee, 
					SUM(ISNULL(AdminServices.ServiceFee, 0) * (1 - ISNULL(PlanAdminServices.AdminServiceDiscountPercentage, 0)/100)) AS ApplicableServiceFee,
					STUFF(
						 (SELECT ',' + PortalServiceName 
						  FROM AdminServices INNER JOIN 
							PlanAdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID
						  WHERE PlanAdminServices.BundleID = Bundles.BundleID
						  FOR XML PATH (''))
						 , 1, 1, '') AS ServiceName
				FROM AdminServices INNER JOIN 
					PlanAdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID INNER JOIN
					Bundles ON Bundles.BundleID = PlanAdminServices.BundleID
				GROUP BY Bundles.BundleID
			) AdminFee ON AdminFee.BundleID = Bundles.BundleID 
		WHERE Orders.AccountID = @AccountID AND (OrderStatus = 1 OR OrderStatus = 2 OR OrderStatus = 3 OR OrderStatus = 4 OR OrderStatus = 5 OR OrderStatus = 6 OR OrderStatus = 7)

		SELECT Orders.OrderID,
			PortalServiceName,
			OrderCharges.ServiceFee,
			OrderCharges.IsRecurring,
			OrderCharges.IsGSTIncluded
		FROM Orders INNER JOIN 
			OrderCharges ON Orders.OrderID = OrderCharges.OrderID INNER JOIN 
			AdminServices ON OrderCharges.AdminServiceID = AdminServices.AdminServiceID
		WHERE Orders.AccountID = @AccountID AND (OrderStatus = 1 OR OrderStatus = 2 OR OrderStatus = 3 OR OrderStatus = 4 OR OrderStatus = 5 OR OrderStatus = 6 OR OrderStatus = 7)

		UNION ALL

		SELECT Orders.OrderID,
			PortalServiceName,
			SubscriberCharges.ServiceFee,
			SubscriberCharges.IsRecurring,
			SubscriberCharges.IsGSTIncluded
		FROM SubscriberCharges INNER JOIN
			OrderSubscribers ON  SubscriberCharges.OrderSubscriberID = OrderSubscribers.OrderSubscriberID INNER JOIN
			Orders ON  Orders.OrderID = OrderSubscribers.OrderID INNER JOIN
			AdminServices ON SubscriberCharges.AdminServiceID = AdminServices.AdminServiceID
		WHERE Orders.AccountID = @AccountID AND (OrderStatus = 1 OR OrderStatus = 2 OR OrderStatus = 3 OR OrderStatus = 4 OR OrderStatus = 5 OR OrderStatus = 6 OR OrderStatus = 7)
		
		SELECT Orders.OrderID,
			CASE OrderStatusLog.OrderStatus 
				WHEN -1 THEN 'expired'
				WHEN 0 THEN 'initiated'
				WHEN 1 THEN 'paid and pending'
				WHEN 2 THEN 'sent to delivery vendor'
				WHEN  3 THEN 'delivery order fulfilled'
				WHEN  4 THEN 'out for delivery'
				WHEN  5 THEN 'delivered'
				WHEN  6 THEN 'delivery failed'
				WHEN  7 THEN 'service activated'
				WHEN  8 THEN 'cancelled'
				ELSE 'payment denied'
			END AS OrderStatus,
			UpdatedOn
		FROM Orders 
			INNER JOIN OrderStatusLog ON Orders.OrderID = OrderStatusLog.OrderID
		WHERE AccountID = @AccountID
		ORDER BY UpdatedOn
		RETURN 105
	END
	ELSE
	BEGIN
		RETURN 102 -- does not exists
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetPaymentMethods]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_GetPaymentMethods]
	@CustomerID INT
AS
BEGIN
	IF EXISTS (SELECT * FROM Customers WHERE CustomerID=@CustomerID)
	BEGIN	
		SELECT	
			CardHolderName,
			MaskedCardNumer,
			CardType,
			IsDefault,
			ExpiryMonth,
			ExpiryYear,
			CardFundMethod,
			CardIssuer
		FROM PaymentMethods INNER JOIN
			Accounts ON PaymentMethods.AccountID = Accounts.AccountID
		WHERE CustomerID = @CustomerID AND IsDefault = 1
		RETURN 105 -- success

	END

	ELSE

	RETURN 109 -- email does not exists
		
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetPlans]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[Customers_GetPlans]
	@CustomerID INT,
	@MobileNumber NVARCHAR(8) = NULL,
	@PlanType INT = 1 
AS
BEGIN
	DECLARE @CurrentDate DATE = CAST(GETDATE() AS date);

	SELECT 
		SubscriptionID,
		Customers.CustomerID, 
		Plans.PlanID, 
		Plans.PlanMarketingName,
		Plans.PortalSummaryDescription,
		Plans.PortalDescription,		
		CASE IsRecurring WHEN 1 THEN 'monthly' WHEN 0 THEN 'one-time' ELSE 'pay-as-use' END AS SubscriptionType,
		Plans.IsRecurring,
		Subscribers.MobileNumber,
		Subscriptions.ValidTo AS ExpiryDate,
		Subscriptions.ActivatedOn AS SubscriptionDate,
		Subscriptions.SubscriptionFee,
		CASE Subscriptions.Status
			WHEN 0 THEN 'Purchased'
			WHEN 1 THEN 'Activated'
			WHEN 2 THEN 'OnHold'
			WHEN 3 THEN 'Terminated'
		END AS PlanStatus,
		CASE IsRecurring 
			WHEN 0 THEN 0 
			WHEN Plans.BundleRemovable THEN 1
			ELSE IsRemovable
		END AS Removable
	FROM Customers 
		INNER JOIN Accounts ON Customers.CustomerID = Accounts.CustomerID
		INNER JOIN Subscribers ON Subscribers.AccountID = Accounts.AccountID
		INNER JOIN Subscriptions ON Subscriptions.SubscriberID = Subscribers.SubscriberID
		INNER JOIN Plans ON Subscriptions.PlanID = Plans.PlanID
	WHERE Customers.CustomerID = @CustomerID AND PlanType = @PlanType AND Subscribers.MobileNumber = ISNULL(@MobileNumber,Subscribers.MobileNumber)
		AND (Subscriptions.Status = 0 OR Subscriptions.Status = 1) AND ISNULL(ValidTo, @CurrentDate) <= @CurrentDate
	IF @@ROWCOUNT=0
	BEGIN
	RETURN 102	
	END
	ELSE RETURN 105
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetProfileMessageQueue]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_GetProfileMessageQueue]
	@CustomerID INT
AS
BEGIN
	SELECT 
		AccountID AS accountID,
		Customers.CustomerID AS customerID,
		NULL AS subscriberID,
		Customers.MobileNumber AS mobilenumber,
		NULL AS MaskedCardNumber,
		NULL AS Token,
		NULL AS CardType,
		NULL AS IsDefault,
		NULL AS CardHolderName,
		NULL AS ExpiryMonth,
		NULL AS ExpiryYear,
		NULL AS CardFundMethod,
		NULL AS CardBrand,
		NULL AS CardIssuer,
	 	Accounts.BillingUnit AS billingUnit,
		Accounts.BillingFloor AS billingFloor,
		Accounts.BillingBuildingNumber AS billingBuildingNumber,
		Accounts.BillingBuildingName AS billingBuildingName,
		Accounts.BillingStreetName AS billingStreetName,
		Accounts.BillingPostCode AS billingPostCode,
		Accounts.BillingContactNumber AS billingContactNumber,
	 	Customers.Email AS email,
		NULL AS displayname,
		NULL AS paymentmode,              /*use for PayBill */
		NULL AS amountpaid,                   /*use for PayBill */
		NULL AS MPGSOrderID,                     /*use for PayBill */
		NULL AS invoicelist,
		NULL AS invoiceamounts
	FROM Customers INNER JOIN
		Accounts ON Customers.CustomerID = Accounts.CustomerID
	WHERE Customers.CustomerID = @CustomerID
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetSharedPlans]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_GetSharedPlans]
	@CustomerID INT 
AS
BEGIN
	DECLARE @CurrentDate DATE = CAST(GETDATE() AS date);
	SELECT 
		AccountSubscriptionID AS SubscriptionID,
		Customers.CustomerID, 
		Plans.PlanID, 
		Plans.PlanMarketingName,
		Plans.PortalSummaryDescription,
		Plans.PortalDescription,
		CASE WHEN Plans.IsRecurring = 0  
				   THEN  'One Time'
				   ELSE 'Monthly' 
		   END as 'SubscriptionType',
		Plans.IsRecurring,
		CASE AccountSubscriptions.Status
			WHEN 0 THEN 'Purchased'
			WHEN 1 THEN 'Activated'
			WHEN 2 THEN 'OnHold'
			WHEN 3 THEN 'Terminated'
		END AS PlanStatus,
		AccountSubscriptions.ValidTo AS ExpiryDate,
		AccountSubscriptions.ActivatedOn AS SubscriptionDate,
		AccountSubscriptions.SubscriptionFee,
		CASE IsRecurring 
			WHEN 0 THEN 0 
			ELSE 1
		END AS Removable
	FROM Customers 
		INNER JOIN Accounts ON Customers.CustomerID = Accounts.CustomerID
		INNER JOIN AccountSubscriptions ON AccountSubscriptions.AccountID = Accounts.AccountID
		INNER JOIN Plans ON AccountSubscriptions.PlanID = Plans.PlanID
	WHERE Customers.CustomerID = @CustomerID 
		AND PlanType = 2 
		AND (AccountSubscriptions.Status = 0 OR AccountSubscriptions.Status = 1) AND ISNULL(ValidTo, @CurrentDate) <= @CurrentDate
	IF @@ROWCOUNT=0
	BEGIN
	RETURN 102	
	END
	ELSE RETURN 105
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetShippingDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_GetShippingDetails]
	@CustomerID INT
AS
BEGIN
	IF EXISTS (SELECT * FROM Customers WHERE CustomerID=@CustomerID)
	BEGIN	
		SELECT	
			ShippingUnit,
			ShippingFloor,
			ShippingStreetName,
			ShippingBuildingNumber,
			ShippingBuildingName,
			ShippingContactNumber,
			ShippingPostCode
		FROM Customers INNER JOIN
			Accounts ON Customers.CustomerID = Accounts.CustomerID
		WHERE Customers.CustomerID = @CustomerID
		RETURN 105 -- success

	END

	ELSE

	RETURN 109 -- email does not exists
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetSubscribers]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Customers_GetSubscribers] --[Customers_GetSubscribers] 38
	@CustomerID INT
AS
BEGIN
	IF EXISTS (
			SELECT *
			FROM Customers
			WHERE CustomerID = @CustomerID
			)
	BEGIN
		IF EXISTS (
				SELECT MobileNumber
					,DisplayName
					,SIMID
					,PremiumType
					,ActivatedOn
					,Subscribers.IsPrimary
				FROM Subscribers
				INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID
				)
		BEGIN
			DECLARE @Msg NVARCHAR(255);
			select @Msg = ConfigValue FROM Config WHERE ConfigKey = 'PlanRequestMessage';
			
			SELECT MobileNumber
				,DisplayName
				,SIMID
				,PremiumType
				,ActivatedOn
				,CASE Subscribers.IsPrimary
					WHEN 0
						THEN CAST(0 AS BIT)
					ELSE CAST(1 AS BIT)
					END AS IsPrimary
				,CASE IsBuddyLine
					WHEN 0
						THEN 'Main'
					ELSE 'Buddy'
					END AS AccountType
				,LinkedMobileNumber
				,LinkedDisplayName
				,SubscriberStatus.State
				,ISNULL(SuspensionRaised, 0) AS SuspensionRaised
				,ISNULL(TerminationRaised, 0) AS TerminationRaised
				,ISNULL(PlanChangeRaised, 0) AS PlanChangeRaised
				,REPLACE(@Msg, '[PLAN]', PlanMarketingName) AS PlanChangeMessage
				,ISNULL(Subscribers.[SMSSubscription], 0) AS SMSSubscription
				,ISNULL(Subscribers.[VoiceSubscription], 0) AS VoiceSubscription
			FROM Subscribers
				INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID
				LEFT OUTER JOIN
				(
					SELECT Subscribers.SubscriberID
						,LinkedSubscribers.DisplayName AS LinkedDisplayName
						,LinkedSubscribers.MobileNumber AS LinkedMobileNumber
					FROM Subscribers
					INNER JOIN Subscribers LinkedSubscribers ON Subscribers.LinkedSubscriberID = LinkedSubscribers.SubscriberID
					LEFT OUTER JOIN
					(
						SELECT SubscriberID, State 
						FROM 
						(
							SELECT SubscriberID
								,State, ROW_NUMBER() OVER(PARTITION BY SubscriberID ORDER BY StateDate DESC) AS RNum
							FROM SubscriberStates
						) A 
						WHERE RNum = 1
					) SubscriberStatus ON SubscriberStatus.SubscriberID = Subscribers.SubscriberID
					WHERE SubscriberStatus.State <> 'Terminated'
				) LinkedSubscribers ON LinkedSubscribers.SubscriberID = Subscribers.SubscriberID
				LEFT OUTER JOIN
				(
					SELECT SubscriberID, State 
					FROM 
					(
						SELECT SubscriberID
							,State, ROW_NUMBER() OVER(PARTITION BY SubscriberID ORDER BY StateDate DESC) AS RNum
						FROM SubscriberStates
					) A 
					WHERE RNum = 1
				) SubscriberStatus ON SubscriberStatus.SubscriberID = Subscribers.SubscriberID
				LEFT OUTER JOIN
				(
					SELECT SubscriberID, COUNT(*) AS SuspensionRaised
					FROM SubscriberRequests
						INNER JOIN ChangeRequests ON SubscriberRequests.ChangeRequestID = ChangeRequests.ChangeRequestID
					WHERE ChangeRequests.OrderStatus = 1 AND RequestTypeID = 6
					GROUP BY SubscriberID
				) SubscriberSuspendReq ON SubscriberSuspendReq.SubscriberID = Subscribers.SubscriberID
				LEFT OUTER JOIN
				(
					SELECT SubscriberID, COUNT(*) AS TerminationRaised
					FROM SubscriberRequests
						INNER JOIN ChangeRequests ON SubscriberRequests.ChangeRequestID = ChangeRequests.ChangeRequestID
					WHERE ChangeRequests.OrderStatus = 1 AND RequestTypeID = 8
					GROUP BY SubscriberID
				) SubscriberTerminateReq ON SubscriberTerminateReq.SubscriberID = Subscribers.SubscriberID
				LEFT OUTER JOIN
				(
					SELECT SubscriberID, CRPlan.PlanMarketingName, COUNT(*) AS PlanChangeRaised
					FROM SubscriberRequests
						INNER JOIN ChangeRequests ON SubscriberRequests.ChangeRequestID = ChangeRequests.ChangeRequestID
						INNER JOIN
						(
							SELECT ChangeRequestID, Plans.PlanMarketingName
							FROM OrderSubscriptions INNER JOIN 
								Plans ON OrderSubscriptions.PlanID = Plans.PlanID
									AND Plans.PlanType = 0
						) CRPlan ON CRPlan.ChangeRequestID = ChangeRequests.ChangeRequestID
					WHERE ChangeRequests.OrderStatus = 1 AND RequestTypeID = 5
					GROUP BY SubscriberID, CRPlan.PlanMarketingName
				) SubscriberPlanChange ON SubscriberPlanChange.SubscriberID = Subscribers.SubscriberID
			WHERE CustomerID = @CustomerID
				AND SubscriberStatus.State <> 'Terminated'
			ORDER BY Subscribers.CreatedOn, IsPrimary DESC

			RETURN 105 --record exists
		END
		ELSE
		BEGIN
			RETURN 119 --does not exist
		END
	END
	ELSE
	BEGIN
		RETURN 119 --does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_IsAdditionalSubscriberAllowed]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_IsAdditionalSubscriberAllowed] --[Customers_IsAdditionalSubscriberAllowed] 38, 0
	@CustomerID INT,
	@Type INT --0-Actual;1-Order
AS
BEGIN
	DECLARE @MaxSunscriberCount INT
	SELECT @MaxSunscriberCount = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'MaxSubscribersPerAccount';

	DECLARE @CurrentSunscriberCount INT
	SELECT @CurrentSunscriberCount = COUNT(*) FROM
	(
		SELECT MobileNumber FROM Subscribers
			INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID 
		WHERE CustomerID = @CustomerID AND IsBuddyLine = 0

		UNION ALL

		SELECT MobileNumber FROM OrderSubscribers INNER JOIN 
			Orders ON OrderSubscribers.OrderID = Orders.OrderID INNER JOIN 
			Accounts ON Orders.AccountID = Accounts.AccountID 
		WHERE CustomerID = @CustomerID AND  OrderStatus = 0 AND @Type = 1
	) A
	
	IF(@MaxSunscriberCount >= @CurrentSunscriberCount)
	BEGIN	
		RETURN 140
	END
	ELSE 
	BEGIN
		RETURN 141
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_UpdateBillingDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Customers_UpdateBillingDetails]
	@CustomerID INT,
	@BillingBuildingNumber NVARCHAR(255),
	@BillingBuildingName NVARCHAR(255),
	@BillingUnit NVARCHAR(255),
	@BillingFloor NVARCHAR(255),
	@BillingStreetName NVARCHAR(255),
	@BillingPostCode NVARCHAR(255),
	@BillingContactNumber NVARCHAR(255)
AS
BEGIN
	IF EXISTS (SELECT * FROM Customers WHERE CustomerID=@CustomerID)
	BEGIN	
		UPDATE Accounts SET 
			BillingBuildingNumber = @BillingBuildingNumber,
			BillingBuildingName = @BillingBuildingName,
			BillingUnit = @BillingUnit,
			BillingFloor = @BillingFloor,
			BillingStreetName = @BillingStreetName,
			BillingPostCode = @BillingPostCode,
			BillingContactNumber = @BillingContactNumber
		WHERE CustomerID = @CustomerID
		RETURN 101 -- success
	END
	ELSE
		RETURN 109 -- email does not exists
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_UpdateDisplayDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Customers_UpdateDisplayDetails]
	@CustomerID INT,
	@MobileNumber NVARCHAR(255),
	@DisplayName NVARCHAR(255)
AS
BEGIN
	IF EXISTS (SELECT * FROM Customers WHERE CustomerID=@CustomerID)
	BEGIN	
		UPDATE Subscribers SET 
			DisplayName = @DisplayName
		FROM Subscribers
			INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID
		WHERE CustomerID = @CustomerID
			AND MobileNumber = @MobileNumber
		RETURN 101 -- success
	END
	ELSE
		RETURN 109 -- email does not exists
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_UpdateEmailSubscriptionDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_UpdateEmailSubscriptionDetails] 
	@CustomerID INT,
	@EmailSubscription INT = NULL
AS
BEGIN

IF EXISTS (SELECT * FROM Customers WHERE CustomerID=@CustomerID)
BEGIN	
	UPDATE Customers SET EmailSubscription = @EmailSubscription WHERE CustomerID = @CustomerID AND @EmailSubscription IS NOT NULL

	RETURN 101 -- update success

END

ELSE

RETURN 109 -- email does not exists
		
END
	

GO
/****** Object:  StoredProcedure [dbo].[Customers_UpdateReferralCode]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_UpdateReferralCode] --[Customers_UpdateReferralCode] 305, '2m522rUZ'
	@CustomerID INT,
	@ReferralCode NVARCHAR(255)
AS
BEGIN	
	IF EXISTS(SELECT * FROM Customers WHERE CustomerID = @CustomerID)
	BEGIN	
		DECLARE @OrderCount INT 
		SELECT @OrderCount = COUNT(*) FROM Orders INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID WHERE CustomerID = @CustomerID AND OrderStatus > 0;
		IF EXISTS(SELECT * FROM Customers WHERE ReferralCode = @ReferralCode AND CustomerID <> @CustomerID)
		BEGIN 
			SELECT CustomerID FROM Customers WHERE ReferralCode = @ReferralCode
			RETURN 105 --record already exists
		END
		ELSE 
		BEGIN
			IF EXISTS (SELECT * FROM Orders WHERE ReferralCode = @ReferralCode) OR @OrderCount > 1
			BEGIN
				RETURN 104
			END
			ELSE
			BEGIN
				UPDATE Customers SET ReferralCode = @ReferralCode WHERE CustomerID = @CustomerID
				SELECT CustomerID FROM Customers WHERE ReferralCode = @ReferralCode
				RETURN 101 --Updated success
			END
		END
	END
	ELSE
	BEGIN		
		RETURN 119 --does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_UpdateSMSSubscriptionDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_UpdateSMSSubscriptionDetails] 
	@CustomerID INT,
	@MobileNumber NVARCHAR(8),
	@SMSSubscription INT = NULL
AS
BEGIN

IF EXISTS (SELECT * FROM Subscribers INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID WHERE CustomerID=@CustomerID AND MobileNumber = @MobileNumber)
BEGIN	
	UPDATE Subscribers SET SMSSubscription = @SMSSubscription 
	FROM Subscribers INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID 
	WHERE CustomerID = @CustomerID 
		AND MobileNumber = @MobileNumber
		AND @SMSSubscription IS NOT NULL

	RETURN 101 -- Auth success

END

ELSE

RETURN 109 -- email does not exists
		
END
	

GO
/****** Object:  StoredProcedure [dbo].[Customers_UpdateSubscriptionDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_UpdateSubscriptionDetails] 
	@CustomerID INT,
	@EmailSubscription INT = NULL,
	@SMSSubscription INT = NULL
AS
BEGIN

IF EXISTS (SELECT * FROM Customers WHERE CustomerID=@CustomerID)
BEGIN	
	UPDATE Customers SET EmailSubscription = @EmailSubscription WHERE CustomerID = @CustomerID AND @EmailSubscription IS NOT NULL
	UPDATE Customers SET SMSSubscription = @SMSSubscription WHERE CustomerID = @CustomerID AND @SMSSubscription IS NOT NULL

	SELECT	
		Customers.CustomerID,
		Email,
		'Encrypted' as [Password],
		MobileNumber,
		ReferralCode,
		Nationality,
		Gender,
		DOB,
		IdentityCardType,
		IdentityCardNumber,
		CASE SMSSubscription WHEN 1 THEN 'Yes' ELSE 'No' END AS SMSSubscription,
		CASE EmailSubscription WHEN 1 THEN 'Yes' ELSE 'No' END AS EmailSubscription,
		CASE [Status] WHEN 1 THEN 'Active' ELSE 'InActive' END AS [Status],
		JoinedOn
	FROM Customers LEFT OUTER JOIN 
		(
			SELECT CustomerID, IdentityCardType, IdentityCardNumber
			FROM
			(
				SELECT CustomerID, IdentityCardType, IdentityCardNumber, ROW_NUMBER() OVER(Partition BY CustomerID Order BY CreatedOn DESC) AS RNUM
				FROM Documents
			)A 
			WHERE RNUM = 1
		)CustomerDocuments ON Customers.CustomerID = CustomerDocuments.CustomerID
	where Customers.CustomerID = @CustomerID

	RETURN 105 -- Auth success

END

ELSE

RETURN 109 -- email does not exists
		
END
	

GO
/****** Object:  StoredProcedure [dbo].[Customers_UpdateVoiceSubscriptionDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_UpdateVoiceSubscriptionDetails] 
	@CustomerID INT,
	@MobileNumber NVARCHAR(8),
	@VoiceSubscription INT = NULL
AS
BEGIN

IF EXISTS (SELECT * FROM Subscribers INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID WHERE CustomerID=@CustomerID AND MobileNumber = @MobileNumber)
BEGIN	
	UPDATE Subscribers SET VoiceSubscription = @VoiceSubscription 
	FROM Subscribers INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID 
	WHERE CustomerID = @CustomerID 
		AND MobileNumber = @MobileNumber
		AND @VoiceSubscription IS NOT NULL

	RETURN 101 -- update success

END

ELSE

RETURN 109 -- email does not exists
		
END
	

GO
/****** Object:  StoredProcedure [dbo].[Customers_ValidateReferralCode]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_ValidateReferralCode]
	@CustomerID INT,
	@ReferralCode NVARCHAR(255)
AS
BEGIN	
	IF EXISTS(SELECT * FROM Customers WHERE CustomerID = @CustomerID)
	BEGIN	
		IF EXISTS(SELECT * FROM Customers WHERE ReferralCode = @ReferralCode AND CustomerID <> @CustomerID)
		BEGIN 
			SELECT CustomerID FROM Customers WHERE ReferralCode = @ReferralCode
			RETURN 105 --record exists
		END
		ELSE 
		BEGIN
			RETURN 119 --does not exist
		END
	END
	ELSE
	BEGIN		
		RETURN 119 --does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Grid_ProcessChangePlan]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Grid_ProcessChangePlan]
	@ChangeRequestID INT,
	@Status INT --1=Success, 0=Failed
AS
BEGIN
	IF EXISTS(SELECT * FROM ChangeRequests WHERE ChangeRequestID = @ChangeRequestID AND OrderStatus = 1 AND RequestTypeID = 5)
	BEGIN
		IF(@Status = 1)
		BEGIN
			DECLARE @SubscriberID INT
			SELECT @SubscriberID = SubscriberID FROM SubscriberRequests WHERE ChangeRequestID = @ChangeRequestID
			
			DECLARE @CurrentDate DATE = CAST(GETDATE() AS date);

			DECLARE @OldBundleID INT
			SELECT 
				@OldBundleID = Bundles.BundleID
			FROM Subscribers 
				INNER JOIN Subscriptions ON Subscriptions.SubscriberID = Subscribers.SubscriberID
				INNER JOIN Plans ON Subscriptions.PlanID = Plans.PlanID
					AND Plans.PlanType = 0
				INNER JOIN Bundles ON Subscriptions.BundleID = Bundles.BundleID 
			WHERE Subscribers.SubscriberID = @SubscriberID 
				AND (Subscriptions.Status = 0 OR Subscriptions.Status = 1) 
				AND ISNULL(Subscriptions.ValidTo, @CurrentDate) <= @CurrentDate

			UPDATE Subscriptions
			SET Status = 3,
				LastUpdatedOn=GETDATE()
			FROM Subscriptions 
			WHERE SubscriberID = @SubscriberID	
				AND BundleID = @OldBundleID
				
			INSERT INTO SubscriptionLog
			(
				SubscriptionID,
				Status,
				Source,
				RefChangeRequestID,
				UpdatedOn
			)			
			SELECT 
				SubscriptionID,
				Status,
				1,
				@ChangeRequestID,
				GETDATE()
			FROM Subscriptions 
			WHERE SubscriberID = @SubscriberID	
				AND BundleID = @OldBundleID

			INSERT INTO Subscriptions
			(	
				SubscriberID,
				PlanID,
				ActivatedOn,
				BSSPlanCode,
				PlanMarketingName,
				SubscriptionFee,
				ValidFrom,
				ValidTo,
				Status,
				BundleID,
				CreatedOn,
				IsRemovable,
				RefOrderSubscriptionID,
				LastUpdatedOn
			)
			SELECT 
				SubscriberID,
				PlanID,
				GETDATE(),
				BSSPlanCode,
				PlanMarketingName,
				SubscriptionFee,
				ValidFrom,
				ValidTo,
				1,
				BundleID,
				GETDATE(),
				0,
				OrderSubscriptionID,
				GETDATE()
			FROM ChangeRequests INNER JOIN
				SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID INNER JOIN
				OrderSubscriptions ON ChangeRequests.ChangeRequestID = OrderSubscriptions.ChangeRequestID
			WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID

			INSERT INTO SubscriptionLog
			(
				SubscriptionID,
				Status,
				Source,
				RefChangeRequestID,
				UpdatedOn
			)			
			SELECT 
				SubscriptionID,
				Subscriptions.Status,
				1,
				@ChangeRequestID,
				GETDATE()
			FROM Subscriptions INNER JOIN 
				OrderSubscriptions ON Subscriptions.RefOrderSubscriptionID = OrderSubscriptions.OrderSubscriptionID				
			WHERE SubscriberID = @SubscriberID	
				AND ChangeRequestID = @ChangeRequestID
			
			IF EXISTS(SELECT * FROM Subscribers WHERE SubscriberID = @SubscriberID AND IsBuddyLine = 1)
			BEGIN
				UPDATE Subscribers SET LinkedSubscriberID = NULL WHERE LinkedSubscriberID = @SubscriberID;
				UPDATE Subscribers SET LinkedSubscriberID = NULL WHERE SubscriberID = @SubscriberID;
			END

			UPDATE ChangeRequests SET OrderStatus = 7, ProcessedOn = GETDATE() WHERE ChangeRequestID = @ChangeRequestID
		END
		ELSE 
		BEGIN
			UPDATE ChangeRequests SET OrderStatus = 8, ProcessedOn = GETDATE() WHERE ChangeRequestID = @ChangeRequestID
		END
		RETURN 1
	END
	ELSE
		RETURN -1
END
GO
/****** Object:  StoredProcedure [dbo].[Grid_ProcessChangeSim]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Grid_ProcessChangeSim]
	@ChangeRequestID INT,
	@Status INT, --1=Success, 0=Failed
	@SIMID NVARCHAR(255)
AS
BEGIN
	IF EXISTS(SELECT * FROM ChangeRequests WHERE ChangeRequestID = @ChangeRequestID AND OrderStatus = 1 AND RequestTypeID = 9)
	BEGIN
		IF(@Status = 1)
		BEGIN
			DECLARE @SubscriberID INT
			SELECT @SubscriberID = SubscriberID FROM SubscriberRequests WHERE ChangeRequestID = @ChangeRequestID
			
			UPDATE Subscribers SET SIMID = @SIMID
			WHERE SubscriberID = @SubscriberID

			UPDATE ChangeRequests SET OrderStatus = 7, ProcessedOn = GETDATE() WHERE ChangeRequestID = @ChangeRequestID
		END
		ELSE 
		BEGIN
			UPDATE ChangeRequests SET OrderStatus = 8, ProcessedOn = GETDATE() WHERE ChangeRequestID = @ChangeRequestID
		END
		RETURN 1
	END
	ELSE
		RETURN -1
END
GO
/****** Object:  StoredProcedure [dbo].[Grid_ProcessSuspension]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Grid_ProcessSuspension]
	@ChangeRequestID INT,
	@SuspensionType INT, --0=Partial; 1=Full
	@Remarks NVARCHAR(255)
AS
BEGIN
	IF EXISTS(SELECT * FROM ChangeRequests WHERE ChangeRequestID = @ChangeRequestID AND OrderStatus = 1 AND RequestTypeID = 6)
	BEGIN
		DECLARE @SubscriberID INT

		SELECT @SubscriberID = SubscriberID 
		FROM ChangeRequests INNER JOIN
			SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID
		WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID
			AND OrderStatus = 1

		INSERT INTO SubscriberStates
		(
			SubscriberID,
			State,
			StateDate,
			Remarks,
			StateSource
		)
		VALUES
		(
			@SubscriberID,
			CASE @SuspensionType WHEN 0 THEN 'PartialSuspension' ELSE 'Suspended' END,
			GETDATE(),
			@Remarks,
			1
		) 

		UPDATE ChangeRequests SET OrderStatus = 7 WHERE ChangeRequestID = @ChangeRequestID

		RETURN 1
	END
	ELSE
		RETURN -1
END
GO
/****** Object:  StoredProcedure [dbo].[Grid_ProcessTermination]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Grid_ProcessTermination]
	@ChangeRequestID INT,
	@Remarks NVARCHAR(255)
AS
BEGIN
	IF EXISTS(SELECT * FROM ChangeRequests WHERE ChangeRequestID = @ChangeRequestID AND OrderStatus = 1 AND RequestTypeID = 8)
	BEGIN
		DECLARE @SubscriberID INT

		SELECT @SubscriberID = SubscriberID 
		FROM ChangeRequests INNER JOIN
			SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID
		WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID
			AND OrderStatus = 1

		INSERT INTO SubscriberStates
		(
			SubscriberID,
			State,
			StateDate,
			Remarks,
			StateSource
		)
		VALUES
		(
			@SubscriberID,
			'Terminated',
			GETDATE(),
			@Remarks,
			1
		) 

		UPDATE ChangeRequests SET OrderStatus = 7 WHERE ChangeRequestID = @ChangeRequestID

		RETURN 1
	END
	ELSE
		RETURN -1
END
GO
/****** Object:  StoredProcedure [dbo].[Grid_ProcessUnSuspension]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Grid_ProcessUnSuspension]
	@ChangeRequestID INT,
	@Remarks NVARCHAR(255)
AS
BEGIN
	IF EXISTS(SELECT * FROM ChangeRequests WHERE ChangeRequestID = @ChangeRequestID AND OrderStatus = 1 AND RequestTypeID = 7)
	BEGIN
		DECLARE @SubscriberID INT

		SELECT @SubscriberID = SubscriberID 
		FROM ChangeRequests INNER JOIN
			SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID
		WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID
			AND OrderStatus = 1

		INSERT INTO SubscriberStates
		(
			SubscriberID,
			State,
			StateDate,
			Remarks,
			StateSource
		)
		VALUES
		(
			@SubscriberID,
			'Active',
			GETDATE(),
			@Remarks,
			1
		) 

		UPDATE ChangeRequests SET OrderStatus = 7 WHERE ChangeRequestID = @ChangeRequestID

		RETURN 1
	END
	ELSE
		RETURN -1
END
GO
/****** Object:  StoredProcedure [dbo].[Grid_ProcessVASAddition]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Grid_ProcessVASAddition]
	@ChangeRequestID INT,
	@Status INT, --1=Activated, 0=Failed
	@ValidFrom DATE,
	@ValidTo DATE
AS
BEGIN
	IF EXISTS(SELECT * FROM ChangeRequests WHERE ChangeRequestID = @ChangeRequestID AND OrderStatus = 1 AND RequestTypeID = 3)
	BEGIN
		DECLARE @OrderSubscriptionID INT
	
		IF EXISTS(SELECT * FROM ChangeRequests INNER JOIN
				SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID
			WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID
				AND OrderStatus = 1)
		BEGIN --Usual VAS Processing
			SELECT @OrderSubscriptionID = OrderSubscriptionID 
			FROM ChangeRequests INNER JOIN
				OrderSubscriptions ON ChangeRequests.ChangeRequestID = OrderSubscriptions.ChangeRequestID
			WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID

			UPDATE Subscriptions SET 
				Status = CASE @Status WHEN 1 THEN 1 ELSE 3 END, 
				ActivatedOn = GETDATE(), 
				ValidFrom = CASE @Status WHEN 1 THEN @ValidFrom ELSE NULL END,
				ValidTo = CASE @Status WHEN 1 THEN @ValidTo ELSE NULL END,
				LastUpdatedOn = GETDATE()
			WHERE RefOrderSubscriptionID = @OrderSubscriptionID

			INSERT INTO SubscriptionLog
			(
				SubscriptionID,
				Status,
				Source,
				UpdatedOn,
				RefChangeRequestID
			)
			SELECT
				SubscriptionID,
				CASE @Status WHEN 1 THEN 1 ELSE 3 END,
				1,
				GETDATE(),
				@ChangeRequestID
			FROM Subscriptions
			WHERE RefOrderSubscriptionID = @OrderSubscriptionID

			UPDATE ChangeRequests SET OrderStatus = CASE @Status WHEN 1 THEN 7 ELSE 8 END,
				ProcessedOn = GETDATE()
			WHERE ChangeRequestID = @ChangeRequestID

			RETURN 1
		END
		ELSE IF EXISTS(SELECT * FROM ChangeRequests INNER JOIN
				AccountChangeRequests ON ChangeRequests.ChangeRequestID = AccountChangeRequests.ChangeRequestID
			WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID
				AND OrderStatus = 1)
		BEGIN --Shared VAS Processing
			SELECT @OrderSubscriptionID = OrderSubscriptionID 
			FROM ChangeRequests INNER JOIN
				OrderSubscriptions ON ChangeRequests.ChangeRequestID = OrderSubscriptions.ChangeRequestID
			WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID

			UPDATE AccountSubscriptions SET 
				Status = CASE @Status WHEN 1 THEN 1 ELSE 3 END, 
				ActivatedOn = GETDATE(), 
				ValidFrom = CASE @Status WHEN 1 THEN @ValidFrom ELSE NULL END,
				ValidTo = CASE @Status WHEN 1 THEN @ValidTo ELSE NULL END,
				LastUpdatedOn = GETDATE()
			WHERE RefOrderSubscriptionID = @OrderSubscriptionID

			
			INSERT INTO AccountSubscriptionLog
			(
				AccountSubscriptionID,
				Status,
				Source,
				UpdatedOn,
				RefChangeRequestID
			)
			SELECT
				AccountSubscriptionID,
				CASE @Status WHEN 1 THEN 1 ELSE 3 END,
				1,
				GETDATE(),
				@ChangeRequestID
			FROM AccountSubscriptions
			WHERE RefOrderSubscriptionID = @OrderSubscriptionID

			UPDATE ChangeRequests SET OrderStatus = CASE @Status WHEN 1 THEN 7 ELSE 8 END,
				ProcessedOn = GETDATE()
			WHERE ChangeRequestID = @ChangeRequestID
			RETURN 1
		END
		ELSE
			RETURN -1;		
	END
	ELSE
		RETURN -1
END
GO
/****** Object:  StoredProcedure [dbo].[Grid_ProcessVASRemoval]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Grid_ProcessVASRemoval]
	@ChangeRequestID INT,
	@Status INT --1=Success, 0=Failed
AS
BEGIN
	IF EXISTS(SELECT * FROM ChangeRequests WHERE ChangeRequestID = @ChangeRequestID AND OrderStatus = 1 AND RequestTypeID = 3)
	BEGIN
		DECLARE @OrderSubscriptionID INT
	
		IF EXISTS(SELECT * FROM ChangeRequests INNER JOIN
				SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID
			WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID
				AND OrderStatus = 1)
		BEGIN --Usual VAS Processing
			SELECT @OrderSubscriptionID = OrderSubscriptionID 
			FROM ChangeRequests INNER JOIN
				OrderSubscriptions ON ChangeRequests.ChangeRequestID = OrderSubscriptions.ChangeRequestID
			WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID

			UPDATE Subscriptions SET 
				Status = CASE @Status WHEN 1 THEN 3 ELSE 1 END,
				LastUpdatedOn = GETDATE()
			WHERE RefOrderSubscriptionID = @OrderSubscriptionID

			INSERT INTO SubscriptionLog
			(
				SubscriptionID,
				Status,
				Source,
				UpdatedOn,
				RefChangeRequestID
			)
			SELECT
				SubscriptionID,
				CASE @Status WHEN 1 THEN 3 ELSE 1 END,
				1,
				GETDATE(),
				@ChangeRequestID
			FROM Subscriptions
			WHERE RefOrderSubscriptionID = @OrderSubscriptionID

			UPDATE ChangeRequests SET OrderStatus = CASE @Status WHEN 1 THEN 7 ELSE 8 END,
				ProcessedOn = GETDATE()
			WHERE ChangeRequestID = @ChangeRequestID

			RETURN 1
		END
		ELSE IF EXISTS(SELECT * FROM ChangeRequests INNER JOIN
				AccountChangeRequests ON ChangeRequests.ChangeRequestID = AccountChangeRequests.ChangeRequestID
			WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID
				AND OrderStatus = 1)
		BEGIN --Shared VAS Processing
			SELECT @OrderSubscriptionID = OrderSubscriptionID 
			FROM ChangeRequests INNER JOIN
				OrderSubscriptions ON ChangeRequests.ChangeRequestID = OrderSubscriptions.ChangeRequestID
			WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID

			UPDATE AccountSubscriptions SET 
				Status = CASE @Status WHEN 1 THEN 3 ELSE 1 END,
				LastUpdatedOn = GETDATE()
			WHERE RefOrderSubscriptionID = @OrderSubscriptionID
			
			INSERT INTO AccountSubscriptionLog
			(
				AccountSubscriptionID,
				Status,
				Source,
				UpdatedOn,
				RefChangeRequestID
			)
			SELECT
				AccountSubscriptionID,
				CASE @Status WHEN 1 THEN 3 ELSE 1 END,
				1,
				GETDATE(),
				@ChangeRequestID
			FROM AccountSubscriptions
			WHERE RefOrderSubscriptionID = @OrderSubscriptionID

			UPDATE ChangeRequests SET OrderStatus = CASE @Status WHEN 1 THEN 7 ELSE 8 END,
				ProcessedOn = GETDATE()
			WHERE ChangeRequestID = @ChangeRequestID
			RETURN 1
		END
		ELSE
			RETURN -1;		
	END
	ELSE
		RETURN -1
END
GO
/****** Object:  StoredProcedure [dbo].[Grid_UpdateBillingAccount]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Grid_UpdateBillingAccount] 
	@AccountID int, 
	@BillingAccountNumber nvarchar(255), 
	@BSSProfileid nvarchar(255)
AS
BEGIN
	UPDATE Accounts SET 
		BillingAccountNumber = @BillingAccountNumber,
		BSSProfileID = @BSSProfileid
	WHERE AccountID = @AccountID
END
GO
/****** Object:  StoredProcedure [dbo].[Grid_UpdateDeliveryStatus]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Grid_UpdateDeliveryStatus] 
	@DeliveryinformationID int,
	@deliverydatetime datetime,
	@deliveryStatus int  --5 or 6
AS
BEGIN
	IF EXISTS(SELECT * FROM DeliveryInformation WHERE DeliveryInformationID = @DeliveryinformationID)
	BEGIN
		INSERT INTO DeliveryInformationLog  
		(  
			DeliveryInformationID  
			,[ShippingNumber]  
			,[Name]  
			,[Email]  
			,[IDNumber]  
			,[IDType]  
			,OrderType
			,[IsSameAsBilling]  
			,[ShippingContactNumber]  
			,[ShippingFloor]  
			,[ShippingUnit]  
			,[ShippingBuildingName]  
			,[ShippingBuildingNumber]  
			,[ShippingStreetName]  
			,[ShippingPostCode]  
			,[AlternateRecipientName]  
			,[AlternateRecipientEmail]  
			,[AlternateRecipientContact]  
			,[AlternateRecioientIDNumber]  
			,[AlternateRecioientIDType]  
			,[PortalSlotID]  
			,[ScheduledDate]  
			,[DeliveryVendor]  
			,[DeliveryOn]  
			,[DeliveryTime]  
			,[TrackingCode]  
			,[TrackingUrl]  
			,[DeliveryFee]  
			,[VoucherID]  
			,[LoggedOn]  
		)  
		SELECT  
			DeliveryInformationID  
			,[ShippingNumber]  
			,[Name]  
			,[Email]  
			,[IDNumber]  
			,[IDType]  
			,OrderType
			,[IsSameAsBilling]  
			,[ShippingContactNumber]  
			,[ShippingFloor]  
			,[ShippingUnit]  
			,[ShippingBuildingName]  
			,[ShippingBuildingNumber]  
			,[ShippingStreetName]      
			,[ShippingPostCode]  
			,[AlternateRecipientName]  
			,[AlternateRecipientEmail]  
			,[AlternateRecipientContact]  
			,[AlternateRecioientIDNumber]  
			,[AlternateRecioientIDType]  
			,[PortalSlotID]  
			,[ScheduledDate]  
			,[DeliveryVendor]  
			,[DeliveryOn]  
			,[DeliveryTime]  
			,[VendorTrackingCode]  
			,[VendorTrackingUrl]  
			,[DeliveryFee]  
			,[VoucherID]  
			,GETDATE()  
		FROM DeliveryInformation   
		WHERE DeliveryInformationID = @DeliveryInformationID    

		UPDATE DeliveryInformation	SET 
			DeliveryTime =@deliverydatetime
		WHERE DeliveryInformationID = @DeliveryInformationID  

		IF EXISTS(SELECT * FROM Orders WHERE DeliveryInformationID = @DeliveryinformationID)
		BEGIN
			DECLARE @OrderID INT
			SELECT @OrderID = OrderID FROM Orders WHERE DeliveryInformationID = @DeliveryinformationID

			UPDATE Orders SET 
				OrderStatus = @deliveryStatus
			WHERE OrderID = @OrderID

			INSERT INTO OrderStatusLog 
			(
				OrderID,
				OrderStatus,
				UpdatedOn
			)
			VALUES
			(
				@OrderID,
				@deliveryStatus,
				GETDATE()
			)
		END
		ELSE
		BEGIN
			DECLARE @ChangeRequestID INT
			SELECT @ChangeRequestID = ChangeRequestID FROM ChangeRequests WHERE DeliveryInformationID = @DeliveryinformationID

			--We do not have intermittent status for CR so orderstatus remains at 3
			UPDATE ChangeRequests SET 
				OrderStatus = @deliveryStatus
			WHERE ChangeRequestID = @ChangeRequestID
		END
		RETURN 1
	END
	ELSE
		RETURN -1
END

GO
/****** Object:  StoredProcedure [dbo].[Grid_UpdateOrderStatus]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Grid_UpdateOrderStatus] 
	@OrderID int, 
	@OrderNumber nvarchar(50), 
	@Orderstatus NVARCHAR(1), 
	@error_reason nvarchar(50)
AS
BEGIN
	UPDATE Orders SET
		OrderStatus = CASE @Orderstatus WHEN 'C' THEN 7 WHEN 'F' THEN 8 END
	WHERE OrderID = @OrderID

	INSERT INTO OrderStatusLog
	(
		OrderID,
		OrderStatus,
		Remarks,
		UpdatedOn
	)
	VALUES
	(
		@OrderID,
		CASE @Orderstatus WHEN 'C' THEN 7 WHEN 'F' THEN 8 END,
		@error_reason,
		GETDATE()
	)
END
GO
/****** Object:  StoredProcedure [dbo].[Grid_UpdateSubscriberState]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Grid_UpdateSubscriberState] 
	@SubscriberID INT, 
	@state NVARCHAR(50), 
	@state_reason NVARCHAR(50)
AS
BEGIN
	INSERT INTO SubscriberStates
	(
		SubscriberID,
		State,
		StateDate,
		Remarks,
		StateSource		
	)
	VALUES
	(
		@SubscriberID,
		@state,
		GETDATE(),
		@state_reason,
		1
	)
END
GO
/****** Object:  StoredProcedure [dbo].[Grid_UpdateVendor]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Grid_UpdateVendor] 
	@DeliveryinformationID int, 
	@shipnumber nvarchar(255),
	@vendor nvarchar(255)
AS
BEGIN
	IF EXISTS(SELECT * FROM DeliveryInformation WHERE DeliveryInformationID = @DeliveryinformationID)
	BEGIN
		INSERT INTO DeliveryInformationLog  
		(  
			DeliveryInformationID  
			,[ShippingNumber]  
			,[Name]  
			,[Email]  
			,[IDNumber]  
			,[IDType]  
			,[IsSameAsBilling]  
			,[ShippingContactNumber]  
			,[ShippingFloor]  
			,[ShippingUnit]  
			,[ShippingBuildingName]  
			,[ShippingBuildingNumber]  
			,[ShippingStreetName]  
			,[ShippingPostCode]  
			,[AlternateRecipientName]  
			,[AlternateRecipientEmail]  
			,[AlternateRecipientContact]  
			,[AlternateRecioientIDNumber]  
			,[AlternateRecioientIDType]  
			,[PortalSlotID]  
			,[ScheduledDate]  
			,[DeliveryVendor]  
			,[DeliveryOn]  
			,[DeliveryTime]  
			,[TrackingCode]  
			,[TrackingUrl]  
			,[DeliveryFee]  
			,[VoucherID]  
			,[LoggedOn]  
		)  
		SELECT  
			DeliveryInformationID  
			,[ShippingNumber]  
			,[Name]  
			,[Email]  
			,[IDNumber]  
			,[IDType]  
			,[IsSameAsBilling]  
			,[ShippingContactNumber]  
			,[ShippingFloor]  
			,[ShippingUnit]  
			,[ShippingBuildingName]  
			,[ShippingBuildingNumber]  
			,[ShippingStreetName]      
			,[ShippingPostCode]  
			,[AlternateRecipientName]  
			,[AlternateRecipientEmail]  
			,[AlternateRecipientContact]  
			,[AlternateRecioientIDNumber]  
			,[AlternateRecioientIDType]  
			,[PortalSlotID]  
			,[ScheduledDate]  
			,[DeliveryVendor]  
			,[DeliveryOn]  
			,[DeliveryTime]  
			,[VendorTrackingCode]  
			,[VendorTrackingUrl]  
			,[DeliveryFee]  
			,[VoucherID]  
			,GETDATE()  
		FROM DeliveryInformation   
		WHERE DeliveryInformationID = @DeliveryInformationID  

		UPDATE DeliveryInformation SET 
			[ShippingNumber] =@shipnumber
			,[DeliveryVendor]=@vendor
		 WHERE DeliveryInformationID =  @DeliveryInformationID  
		RETURN 1
	END
	ELSE
		RETURN -1
END

GO
/****** Object:  StoredProcedure [dbo].[Grid_UpdateVendorTrackingCode]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Grid_UpdateVendorTrackingCode] 
	@DeliveryinformationID int,
	@TrackingCode nvarchar(255)
AS
BEGIN
	IF EXISTS(SELECT * FROM DeliveryInformation WHERE DeliveryInformationID = @DeliveryinformationID)
	BEGIN
		INSERT INTO DeliveryInformationLog  
		(  
			DeliveryInformationID  
			,[ShippingNumber]  
			,[Name]  
			,[Email]  
			,[IDNumber]  
			,[IDType]  
			,[IsSameAsBilling]  
			,[ShippingContactNumber]  
			,[ShippingFloor]  
			,[ShippingUnit]  
			,[ShippingBuildingName]  
			,[ShippingBuildingNumber]  
			,[ShippingStreetName]  
			,[ShippingPostCode]  
			,[AlternateRecipientName]  
			,[AlternateRecipientEmail]  
			,[AlternateRecipientContact]  
			,[AlternateRecioientIDNumber]  
			,[AlternateRecioientIDType]  
			,[PortalSlotID]  
			,[ScheduledDate]  
			,[DeliveryVendor]  
			,[DeliveryOn]  
			,[DeliveryTime]  
			,[TrackingCode]  
			,[TrackingUrl]  
			,[DeliveryFee]  
			,[VoucherID]  
			,[LoggedOn]  
		)  
		SELECT  
			DeliveryInformationID  
			,[ShippingNumber]  
			,[Name]  
			,[Email]  
			,[IDNumber]  
			,[IDType]  
			,[IsSameAsBilling]  
			,[ShippingContactNumber]  
			,[ShippingFloor]  
			,[ShippingUnit]  
			,[ShippingBuildingName]  
			,[ShippingBuildingNumber]  
			,[ShippingStreetName]      
			,[ShippingPostCode]  
			,[AlternateRecipientName]  
			,[AlternateRecipientEmail]  
			,[AlternateRecipientContact]  
			,[AlternateRecioientIDNumber]  
			,[AlternateRecioientIDType]  
			,[PortalSlotID]  
			,[ScheduledDate]  
			,[DeliveryVendor]  
			,[DeliveryOn]  
			,[DeliveryTime]  
			,[VendorTrackingCode]  
			,[VendorTrackingUrl]  
			,[DeliveryFee]  
			,[VoucherID]  
			,GETDATE()  
		FROM DeliveryInformation   
		WHERE DeliveryInformationID = @DeliveryInformationID  
  
		UPDATE DeliveryInformation SET 
			VendorTrackingCode =@TrackingCode
		WHERE DeliveryInformationID =  @DeliveryInformationID  

		IF EXISTS(SELECT * FROM Orders WHERE DeliveryInformationID = @DeliveryinformationID)
		BEGIN
			DECLARE @OrderID INT
			SELECT @OrderID = OrderID FROM Orders WHERE DeliveryInformationID = @DeliveryinformationID

			UPDATE Orders SET 
				OrderStatus = 3
			WHERE OrderID = @OrderID

			INSERT INTO OrderStatusLog 
			(
				OrderID,
				OrderStatus,
				UpdatedOn
			)
			VALUES
			(
				@OrderID,
				3,
				GETDATE()
			)
		END
		ELSE
		BEGIN
			DECLARE @ChangeRequestID INT
			SELECT @ChangeRequestID = ChangeRequestID FROM ChangeRequests WHERE DeliveryInformationID = @DeliveryinformationID

			UPDATE ChangeRequests SET 
				OrderStatus = 3
			WHERE ChangeRequestID = @ChangeRequestID
		END
		RETURN 1
	END
	ELSE
		RETURN -1
END

GO
/****** Object:  StoredProcedure [dbo].[Order_CR_SIMReplacementRequest]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_CR_SIMReplacementRequest] --[Order_CR_SIMReplacementRequest] 176, '88400140', 'ReplaceSIM'    
 @CustomerID INT    
 ,@MobileNumber NVARCHAR(8)    
 ,@RequestType NVARCHAR(50)    
AS    
BEGIN    
 BEGIN TRY    
  BEGIN TRAN    
    
  DECLARE @CurrentDate DATE;    
    
  SET @CurrentDate = CAST(GETDATE() AS DATE)    
    
  DECLARE @RequestTypeID INT    
    
  SELECT @RequestTypeID = RequestTypeID    
  FROM RequestTypes    
  WHERE RequestType = @RequestType    
    
  DECLARE @Remarks NVARCHAR(50)    
    
  SET @Remarks = 'CR-ChangeSim'    
  DECLARE @AccountID INT    
  DECLARE @SubscriberID INT    
    
  SELECT @AccountID = Accounts.AccountID    
   ,@SubscriberID = SubscriberID    
  FROM Accounts    
  INNER JOIN Subscribers ON Accounts.AccountID = Subscribers.AccountID    
  WHERE CustomerID = @CustomerID    
   AND MobileNumber = @MobileNumber    
       
  IF (@SubscriberID IS NULL)    
  BEGIN    
   ROLLBACK TRAN    
    
   RETURN 102    
  END    
  
  IF (  
    'Created' =  
     (SELECT TOP 1 State  
     FROM SubscriberStates WHERE SubscriberID = @SubscriberID ORDER BY StateDate DESC)        
     )  
  BEGIN
	 ROLLBACK TRAN  
    RETURN 139  
  END

  IF EXISTS (    
    SELECT *    
    FROM ChangeRequests    
    INNER JOIN SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID    
    WHERE SubscriberID = @SubscriberID    
     AND RequestTypeID = @RequestTypeID    
     AND OrderStatus = 1    
    )    
  BEGIN    
   ROLLBACK TRAN    
    
   RETURN 128    
  END    
    
  DECLARE @IsExistingRequest INT = 0;    
  IF EXISTS (SELECT * FROM Subscribers WHERE SubscriberID = @SubscriberID )    
  BEGIN    
   DECLARE @ChangeRequestID INT    
   IF EXISTS (SELECT * FROM ChangeRequests INNER JOIN SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID    
    WHERE SubscriberID = @SubscriberID    
     AND RequestTypeID = @RequestTypeID    
     AND OrderStatus = 0    
    )    
   BEGIN    
    SELECT @ChangeRequestID = ChangeRequests.ChangeRequestID    
    FROM ChangeRequests    
     INNER JOIN SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID    
    WHERE SubscriberID = @SubscriberID    
     AND RequestTypeID = @RequestTypeID    
     AND OrderStatus = 0    
    SET @IsExistingRequest = 1;    
   END    
   ELSE    
   BEGIN    
    INSERT INTO ChangeRequests (    
     RequestTypeID    
     ,RequestReason    
     )    
    VALUES (    
     @RequestTypeID    
     ,@Remarks    
     );    
    
    SET @ChangeRequestID = SCOPE_IDENTITY();    
   END    
   DECLARE @OrderNumber NVARCHAR(500)    
    
   SET @OrderNumber = dbo.ufnGetCRNumber()    
    
   UPDATE ChangeRequests    
   SET OrderNumber = @OrderNumber    
   WHERE ChangeRequestID = @ChangeRequestID    
    
   DELETE FROM SubscriberRequests WHERE ChangeRequestID = @ChangeRequestID AND SubscriberID = @SubscriberID    
   INSERT INTO SubscriberRequests (    
    ChangeRequestID    
    ,SubscriberID    
    )    
   VALUES (    
    @ChangeRequestID    
    ,@SubscriberID    
    )    
    
       
   DECLARE @SubscriberVoucherID INT    
   SELECT @SubscriberVoucherID = ReasonID FROM ChangeRequestCharges WHERE ChangeRequestID = @ChangeRequestID AND ReasonType = 'SubscriberVouchers';    
   UPDATE SubscriberVouchers SET IsUsed = 0 WHERE SubscriberVoucherID = @SubscriberVoucherID;    
    
   DELETE FROM ChangeRequestCharges WHERE ChangeRequestID = @ChangeRequestID     
       
   IF EXISTS (    
     SELECT *    
     FROM RequestTypes    
     WHERE RequestTypeID = @RequestTypeID    
      AND IsChargable = 1)    
   BEGIN    
    INSERT INTO ChangeRequestCharges (    
     ChangeRequestID    
     ,AdminServiceID    
     ,ServiceFee    
     ,IsGSTIncluded    
     ,IsRecurring    
     ,ReasonType    
     ,ReasonID    
     )    
    SELECT @ChangeRequestID    
     ,AdminServiceID    
     ,ServiceFee    
     ,IsGSTIncluded    
     ,IsRecurring    
     ,@Remarks    
     ,@RequestTypeID    
    FROM AdminServices    
    WHERE ServiceType = @RequestType    
        
    INSERT INTO ChangeRequestCharges (    
     ChangeRequestID    
     ,AdminServiceID    
     ,ServiceFee    
     ,IsGSTIncluded    
     ,IsRecurring    
     ,ReasonType    
     ,ReasonID    
     )    
    SELECT TOP(1) @ChangeRequestID    
     ,AdminServiceID    
     ,ServiceFee + (Vouchers.DiscountValue * -1)    
     ,IsGSTIncluded    
     ,IsRecurring    
     ,'SubscriberVouchers'    
     ,SubscriberVoucherID    
    FROM AdminServices CROSS JOIN    
     SubscriberVouchers INNER JOIN    
     Vouchers ON SubscriberVouchers.VoucherID = Vouchers.VoucherID    
    WHERE ServiceType = 'Voucher'    
     AND SubscriberVouchers.SubscriberID = @SubscriberID    
     AND VoucherCode = 'DeliveryOffsetCR' AND IsUsed = 0 AND SubscriberVouchers.ValidTo <= CAST(GETDATE() AS date)    
        
    UPDATE SubscriberVouchers SET IsUsed = 1 WHERE SubscriberID = @SubscriberID;    
   END    
       
   DECLARE @ServiceFee FLOAT = 0   
   SELECT @ServiceFee = SUM(ChangeRequestCharges.ServiceFee)    
   FROM ChangeRequestCharges    
   GROUP BY ChangeRequestID    
   HAVING ChangeRequestID = @ChangeRequestID    
    
   IF (ISNULL(@ServiceFee,0) = 0)    
   BEGIN    
    UPDATE ChangeRequests    
    SET OrderStatus = 1    
    WHERE ChangeRequestID = @ChangeRequestID    
   END    
    
   DECLARE @DeliveryInformationID INT    
    
   IF EXISTS(SELECT * FROM ChangeRequests WHERE ChangeRequestID = @ChangeRequestID AND DeliveryInformationID IS NULL)    
   BEGIN    
    INSERT INTO DeliveryInformation (    
     ShippingNumber    
     ,Name    
     ,Email    
  ,OrderType  
     ,ShippingContactNumber    
     ,ShippingFloor    
     ,ShippingUnit    
     ,ShippingBuildingName    
     ,ShippingBuildingNumber    
     ,ShippingStreetName    
     ,ShippingPostCode    
     ,IDNumber    
     ,IDType    
     )    
    SELECT dbo.ufnGetShippingNumber()    
     ,AccountName    
     ,Email    
  ,1  
     ,ShippingContactNumber    
     ,ShippingFloor    
     ,ShippingUnit    
     ,ShippingBuildingName    
     ,ShippingBuildingNumber    
     ,ShippingStreetName    
     ,ShippingPostCode    
     ,IdentityCardNumber    
     ,IdentityCardType    
    FROM Accounts    
    LEFT JOIN (    
     SELECT CustomerID    
      ,IdentityCardNumber    
      ,IdentityCardType    
     FROM (    
      SELECT CustomerID    
       ,IdentityCardNumber    
       ,IdentityCardType    
       ,ROW_NUMBER() OVER (    
        PARTITION BY CustomerID ORDER BY CreatedOn DESC    
        ) AS RNum    
      FROM Documents    
      ) A    
     WHERE Rnum = 1    
     ) Documents ON Documents.CustomerID = Accounts.CustomerID    
    WHERE AccountID = @AccountID    
    
    -- take the Delivery infomration id an update it in changerequest table      
    SET @DeliveryInformationID = SCOPE_IDENTITY();    
    
    UPDATE ChangeRequests    
    SET DeliveryInformationID = @DeliveryInformationID    
    WHERE ChangeRequestID = @ChangeRequestID    
   END    
   ELSE    
   BEGIN    
    SELECT @DeliveryInformationID = DeliveryInformationID FROM ChangeRequests WHERE ChangeRequestID = @ChangeRequestID    
    UPDATE DeliveryInformation SET    
     Name = Accounts.AccountName    
     ,Email = Accounts.Email    
     ,ShippingContactNumber = Accounts.ShippingContactNumber    
     ,ShippingFloor = Accounts.ShippingFloor    
     ,ShippingUnit = Accounts.ShippingUnit    
     ,ShippingBuildingName = Accounts.ShippingBuildingName    
     ,ShippingBuildingNumber = Accounts.ShippingBuildingNumber    
     ,ShippingStreetName = Accounts.ShippingContactNumber    
     ,ShippingPostCode = Accounts.ShippingPostCode    
     ,IDNumber = Documents.IdentityCardNumber    
     ,IDType = Documents.IdentityCardType    
    FROM DeliveryInformation     
     INNER JOIN ChangeRequests ON DeliveryInformation.DeliveryInformationID = ChangeRequests.DeliveryInformationID    
     INNER JOIN SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID    
     INNER JOIN Subscribers ON SubscriberRequests.SubscriberID = Subscribers.SubscriberID    
     INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID    
     LEFT JOIN (    
      SELECT CustomerID    
       ,IdentityCardNumber    
       ,IdentityCardType    
      FROM (    
       SELECT CustomerID    
        ,IdentityCardNumber    
        ,IdentityCardType    
        ,ROW_NUMBER() OVER (    
         PARTITION BY CustomerID ORDER BY CreatedOn DESC    
         ) AS RNum    
       FROM Documents    
       ) A    
      WHERE Rnum = 1    
      ) Documents ON Documents.CustomerID = Accounts.CustomerID    
    WHERE Accounts.AccountID = @AccountID AND DeliveryInformation.DeliveryInformationID = @DeliveryInformationID    
   END    
    
   SELECT @ChangeRequestID AS ChangeRequestID    
    ,@OrderNumber AS OrderNumber    
    ,ChangeRequests.RequestOn    
    ,Accounts.BillingUnit    
    ,Accounts.BillingFloor    
    ,Accounts.BillingBuildingName    
    ,Accounts.BillingBuildingNumber    
    ,Accounts.BillingStreetName    
    ,Accounts.BillingPostCode    
    ,Accounts.BillingContactNumber    
    ,Accounts.AccountName AS Name    
    ,Accounts.Email    
    ,@RequestType AS RequestTypeDescription    
    ,DeliveryInformation.IDType AS IdentityCardType    
    ,DeliveryInformation.IDNumber AS IdentityCardNumber    
    ,IsSameAsBilling AS IsSameAsBilling    
    ,DeliveryInformation.ShippingUnit    
    ,DeliveryInformation.ShippingFloor    
    ,DeliveryInformation.ShippingBuildingNumber    
    ,DeliveryInformation.ShippingStreetName    
    ,DeliveryInformation.ShippingPostCode    
    ,DeliveryInformation.ShippingContactNumber    
    ,DeliveryInformation.AlternateRecipientName    
    ,DeliveryInformation.AlternateRecipientEmail    
    ,DeliveryInformation.AlternateRecipientContact    
    ,DeliveryInformation.AlternateRecioientIDNumber AS AlternateRecipientIDNumber    
    ,DeliveryInformation.AlternateRecioientIDType AS AlternateRecipientIDType    
    ,DeliveryInformation.PortalSlotID    
    ,DeliveryInformation.ScheduledDate    
    ,DeliveryInformation.DeliveryVendor    
    ,DeliveryInformation.DeliveryOn    
    ,DeliveryInformation.DeliveryTime    
    ,DeliveryInformation.VendorTrackingCode    
    ,DeliveryInformation.VendorTrackingUrl    
    ,DeliveryInformation.DeliveryFee    
    ,ISNULL(CRCharge.ServiceFee, 0) AS PayableAmount    
   FROM ChangeRequests    
    INNER JOIN DeliveryInformation ON ChangeRequests.DeliveryInformationID = DeliveryInformation.DeliveryInformationID    
    INNER JOIN SubscriberRequests ON SubscriberRequests.ChangeRequestID = ChangeRequests.ChangeRequestID    
    INNER JOIN Subscribers ON Subscribers.SubscriberID = SubscriberRequests.SubscriberID    
    INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID    
    LEFT JOIN    
    (    
     SELECT ChangeRequestID, SUM(ChangeRequestCharges.ServiceFee) AS ServiceFee    
     FROM ChangeRequestCharges    
     GROUP BY ChangeRequestID    
    ) CRCharge ON CRCharge.ChangeRequestID = ChangeRequests.ChangeRequestID    
   WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID    
    
   SELECT PortalServiceName    
    ,SUM(ChangeRequestCharges.ServiceFee) AS ServiceFee    
    ,ChangeRequestCharges.IsRecurring AS IsRecurring    
    ,ChangeRequestCharges.IsGSTIncluded AS IsGSTIncluded    
   FROM ChangeRequestCharges    
   INNER JOIN AdminServices ON ChangeRequestCharges.AdminServiceID = AdminServices.AdminServiceID    
   GROUP BY PortalServiceName    
    ,ChangeRequestCharges.IsRecurring    
    ,ChangeRequestCharges.IsGSTIncluded    
    ,ChangeRequestCharges.ChangeRequestID    
   HAVING ChangeRequestCharges.ChangeRequestID = @ChangeRequestID    
  END    
  ELSE    
  BEGIN    
   ROLLBACK TRAN    
    
   RETURN 102    
  END    
    
  COMMIT TRAN    
 END TRY    
    
 BEGIN CATCH    
  ROLLBACK TRAN    
 END CATCH    
    
 IF @@ERROR <> 0    
  RETURN 107    
 ELSE    
  RETURN 100    
END 
GO
/****** Object:  StoredProcedure [dbo].[Order_CreateCustomerPaymentMethods]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_CreateCustomerPaymentMethods] 
	 (@CustomerID INT,
           @MaskedCardNumer NVARCHAR(255),
           @SourceType NVARCHAR(20),
           @Token NVARCHAR(255),
           @CardType NVARCHAR(255),
           @ExpiryMonth INT,
           @ExpiryYear INT,
           @CardFundMethod NVARCHAR(255),
           @CardBrand NVARCHAR(255))
AS
BEGIN

DECLARE @AccountID INT	

DECLARE @PaymentMethodID INT

SELECT @AccountID= AccountID FROM Customers C INNER JOIN Accounts A 
				ON A.CustomerID =C.CustomerID WHERE C.CustomerID=@CustomerID
		
		IF NOT EXISTS (SELECT * FROM PaymentMethods WHERE AccountID=@AccountID AND MaskedCardNumer=@MaskedCardNumer)
		BEGIN
			INSERT INTO PaymentMethods
           ( AccountID,
           MaskedCardNumer,
           SourceType,
           Token,
           CardType,
           ExpiryMonth,
           ExpiryYear,
           CardFundMethod,
           CardBrand,IsDefault )
     VALUES
           (@AccountID,
           @MaskedCardNumer,
           @SourceType,
           @Token,
           @CardType, 
           @ExpiryMonth,
           @ExpiryYear,
           @CardFundMethod,
           @CardBrand,1 )

		   set @PaymentMethodID=SCOPE_IDENTITY();

		  --CREATE success		

		UPDATE PaymentMethods SET IsDefault=0 WHERE AccountID=@AccountID and  PaymentMethodID <> @PaymentMethodID
		  RETURN 100
		END	
		ELSE

		RETURN 105	

END
GO
/****** Object:  StoredProcedure [dbo].[Order_CreateSubscriber]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_CreateSubscriber] --Order_CreateSubscriber 34, 4, '90259894', 'Launch2019'
	@OrderID INT,
	@BundleID INT,
	@UserId NVARCHAR(50),
	@MobileNumber NVARCHAR(8),
	@PromotionCode NVARCHAR(50)=null
AS
BEGIN
	DECLARE @CurrentDate DATE;
	SET @CurrentDate = CAST(GETDATE() AS date)
	BEGIN TRY
		BEGIN TRAN
			--insert subscribers
			DECLARE @SubscriberCount INT = 0;
			SELECT @SubscriberCount = COUNT(*) FROM OrderSubscribers WHERE OrderID = @OrderID

			INSERT INTO OrderSubscribers
			(
				OrderID,
				MobileNumber,
				UserSessionID,
				IsPorted,
				IsOwnNumber,
				IsPrimaryNumber,
				PremiumType
			)
			VALUES 
			(
				@OrderID,
				@MobileNumber,
				@UserId,
				0,
				0,
				CASE WHEN @SubscriberCount > 0 THEN 0 ELSE 1 END,
				0
			)

			DECLARE @OrderSubscriberID INT
			SET @OrderSubscriberID = SCOPE_IDENTITY();

			--insert required fee
			INSERT INTO SubscriberCharges
			(
				OrderSubscriberID,
				AdminServiceID,
				ServiceFee,
				IsGSTIncluded,
				IsRecurring,
				ReasonType,
				ReasonID
			)
			SELECT @OrderSubscriberID,
				AdminServices.AdminServiceID,
				ServiceFee,
				IsGSTIncluded,
				IsRecurring,
				'Orders',
				@OrderID
			FROM PlanAdminServices INNER JOIN 
				AdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID
			WHERE BundleID = @BundleID

			IF EXISTS(SELECT * FROM Bundles INNER JOIN 
					BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN 
					Plans ON BundlePlans.PlanID = Plans.PlanID LEFT OUTER JOIN 
					PromotionBundles ON Bundles.BundleID = PromotionBundles.BundleID  LEFT OUTER JOIN 
					Promotions ON  PromotionBundles.PromotionID = Promotions.PromotionID
						AND Promotions.PromotionCode = @PromotionCode
				WHERE Bundles.BundleID = @BundleID)
			BEGIN			
				DECLARE @Discount FLOAT = 0;
				SELECT @Discount = ISNULL(DiscountValue, 0) FROM PromotionBundles INNER JOIN 
						Promotions ON  PromotionBundles.PromotionID = Promotions.PromotionID
				WHERE PromotionCode = @PromotionCode AND BundleID = @BundleID
				DELETE FROM SubscriberCharges WHERE ReasonType = 'Orders - Promo' AND OrderSubscriberID = @OrderSubscriberID

				INSERT INTO SubscriberCharges
				(
					OrderSubscriberID,
					AdminServiceID,
					ServiceFee,
					IsGSTIncluded,
					IsRecurring,
					ReasonType,
					ReasonID
				)
				SELECT @OrderSubscriberID,
					AdminServices.AdminServiceID,
					(ServiceFee + @Discount) * -1,
					IsGSTIncluded,
					IsRecurring,
					'Orders - Promo',
					@OrderID
				FROM AdminServices
				WHERE ServiceType = 'Promo' AND @Discount <> 0
			END

			DELETE SubscriberCharges 
			FROM SubscriberCharges 
				INNER JOIN OrderSubscribers ON SubscriberCharges.OrderSubscriberID = OrderSubscribers.OrderSubscriberID 
			WHERE ReasonType = 'Orders - Delivery' AND OrderID = @OrderID
		
			INSERT INTO SubscriberCharges
			(
				OrderSubscriberID,
				AdminServiceID,
				ServiceFee,
				IsGSTIncluded,
				IsRecurring,
				ReasonType,
				ReasonID
			)
			SELECT 
				OrderSubscriberID,
				AdminServiceID,
				ISNULL(ServiceFee, 0),
				IsGSTIncluded,
				IsRecurring,
				'Orders - Delivery',
				@OrderID
			FROM OrderSubscribers CROSS JOIN 
				AdminServices 
			WHERE OrderID = @OrderID 
				AND AdminServices.ServiceType = 'Delivery'
				AND ISNULL(ServiceFee, 0) > 0
	
			--create default CR
			INSERT INTO [dbo].[ChangeRequests]
					([OrderNumber]
					,[RequestTypeID]
					,[RequestOn]
					,[OrderStatus]
					,[RequestReason])
			VALUES 
			(
				'GRID-' + CAST(@OrderSubscriberID AS nvarchar(50)) + CONVERT(NVARCHAR(10), @CurrentDate, 12),
				1,
				GETDATE(),
				0,
				'Created From Order'
			)
			DECLARE @CRID INT
			SET @CRID = SCOPE_IDENTITY();

			INSERT INTO OrderSubscriberChangeRequests
			(
				OrderSubscriberID,
				ChangeRequestID
			)
			VALUES 
			(
				@OrderSubscriberID,
				@CRID
			)
			--insert subscriptions
			INSERT INTO [dbo].[OrderSubscriptions]
				([ChangeRequestID]
				,[PlanID]
				,[PurchasedOn]
				,[BSSPlanCode]
				,[PlanMarketingName]
				,[SubscriptionFee]
				,[ValidFrom]
				,[ValidTo]
				,[Status]
				,[BundleID]
				,[PromotionID])
			SELECT 
				@CRID,
				Plans.PlanID,
				GETDATE(),
				BSSPlanCode,
				Plans.PlanMarketingName,
				SubscriptionFee,
				CASE ISNULL(BundlePlans.DurationValue, 0) 
					WHEN 0 THEN NULL
					ELSE @CurrentDate
				END,
				CASE ISNULL(BundlePlans.DurationValue, 0) 
					WHEN 0 THEN NULL
					ELSE DATEADD(month, DurationValue, @CurrentDate) --CASE DurationUOM WHEN 'month' THEN month WHEN 'year' THEN year ELSE day END
				END,
				0,
				@BundleID,
				PromotionBundles.PromotionID
			FROM Bundles INNER JOIN 
				BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN 
				Plans ON BundlePlans.PlanID = Plans.PlanID LEFT OUTER JOIN 
				PromotionBundles ON Bundles.BundleID = PromotionBundles.BundleID  LEFT OUTER JOIN 
				Promotions ON  PromotionBundles.PromotionID = Promotions.PromotionID
					AND Promotions.PromotionCode = @PromotionCode
			WHERE Bundles.BundleID = @BundleID		
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN
	END CATCH
	
	IF @@ERROR<>0

	RETURN 107

	ELSE RETURN 100
END
GO
/****** Object:  StoredProcedure [dbo].[Order_GetAccountIDFromCustomerID]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_GetAccountIDFromCustomerID]
	@CustomerID INT	
AS
BEGIN

IF EXISTS (SELECT * FROM Accounts WHERE CustomerID=@CustomerID)
BEGIN
	SELECT AccountID 
	FROM Accounts WHERE CustomerID=@CustomerID
	RETURN 105
END
ELSE
	RETURN 102
END
GO
/****** Object:  StoredProcedure [dbo].[Order_GetBSSAccountNumberByCustomerId]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_GetBSSAccountNumberByCustomerId]
	@CustomerID INT	
AS
BEGIN

SELECT BillingAccountNumber  AS AccountName FROM Accounts WHERE CustomerID=@CustomerID

IF @@rowcount>0
RETURN 105
ELSE RETURN 102
END
GO
/****** Object:  StoredProcedure [dbo].[Order_GetCustomerIDByAccountInvoiceID]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_GetCustomerIDByAccountInvoiceID]
	@AccountInvoiceID INT	
AS
BEGIN

IF EXISTS (SELECT * FROM AccountInvoices WHERE AccountInvoices.InvoiceID=@AccountInvoiceID)
BEGIN
	SELECT Accounts.CustomerID 
	FROM AccountInvoices INNER JOIN 
		Accounts ON AccountInvoices.AccountID = Accounts.AccountID
	WHERE AccountInvoices.InvoiceID=@AccountInvoiceID
	RETURN 105
END
ELSE
	RETURN 102
END
GO
/****** Object:  StoredProcedure [dbo].[Order_GetCustomerIDByChangeRequestID]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_GetCustomerIDByChangeRequestID]
	@ChangeRequestID INT	
AS
BEGIN

IF EXISTS (SELECT * FROM ChangeRequests INNER JOIN SubscriberRequests on ChangeRequests.ChangeRequestID=SubscriberRequests.ChangeRequestID WHERE ChangeRequests.ChangeRequestID=@ChangeRequestID)
BEGIN
	SELECT Accounts.CustomerID 
	FROM ChangeRequests INNER JOIN 
		SubscriberRequests on ChangeRequests.ChangeRequestID=SubscriberRequests.ChangeRequestID INNER JOIN 
		Subscribers ON SubscriberRequests.SubscriberID = Subscribers.SubscriberID INNER JOIN
		Accounts ON Subscribers.AccountID = Accounts.AccountID
	WHERE ChangeRequests.ChangeRequestID=@ChangeRequestID
	RETURN 105
END
ELSE IF EXISTS (SELECT * FROM ChangeRequests INNER JOIN AccountChangeRequests on ChangeRequests.ChangeRequestID=AccountChangeRequests.ChangeRequestID WHERE ChangeRequests.ChangeRequestID=@ChangeRequestID)
BEGIN 
	SELECT Accounts.CustomerID 
	FROM ChangeRequests INNER JOIN 
		AccountChangeRequests on ChangeRequests.ChangeRequestID=AccountChangeRequests.ChangeRequestID INNER JOIN 
		Accounts ON AccountChangeRequests.AccountID = Accounts.AccountID
	WHERE ChangeRequests.ChangeRequestID=@ChangeRequestID
	RETURN 105
END
ELSE
	RETURN 102
END
GO
/****** Object:  StoredProcedure [dbo].[Order_GetCustomerIDByOrderID]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_GetCustomerIDByOrderID]
	@OrderID INT	
AS
BEGIN

IF EXISTS (SELECT * FROM Orders WHERE OrderID=@OrderID)
BEGIN
SELECT a.CustomerID from Orders o inner join Accounts a on o.AccountID=a.AccountID
WHERE o.OrderID=@OrderID

RETURN 105
END
ELSE RETURN 102
END
GO
/****** Object:  StoredProcedure [dbo].[Order_GetCustomerNRICDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_GetCustomerNRICDetails]
	@OrderID INT
AS
BEGIN
	IF EXISTS(SELECT * FROM OrderDocuments WHERE OrderID = @OrderID)
	BEGIN
		SELECT Documents.DocumentID,CustomerID,DocumentURL, DocumentBackURL,IdentityCardNumber, IdentityCardType , Orders.Nationality
		FROM Documents INNER JOIN 
			OrderDocuments ON Documents.DocumentID = OrderDocuments.DocumentID 
			INNER JOIN Orders on Orders.OrderID=OrderDocuments.OrderID
		WHERE orders.OrderID = @OrderID

		RETURN 105 -- exists

	END
	ELSE
	BEGIN
		RETURN 102 -- does not exists
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Order_GetCustomerPaymentToken]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_GetCustomerPaymentToken] 
		 (@CustomerID INT		 
           )
AS
BEGIN

DECLARE @AccountID INT

SELECT @AccountID= AccountID FROM Customers C INNER JOIN Accounts A 
				
				ON A.CustomerID =C.CustomerID WHERE C.CustomerID=@CustomerID
		
		IF EXISTS (SELECT * FROM PaymentMethods WHERE AccountID=@AccountID AND IsDefault=1)	
		BEGIN
		
		SELECT PaymentMethodID,Token, SourceType, CardHolderName,CardType FROM PaymentMethods WHERE AccountID=@AccountID AND IsDefault=1

		RETURN 105
		
		END

		ELSE

		RETURN 102

END
GO
/****** Object:  StoredProcedure [dbo].[Order_GetOrderBasicDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_GetOrderBasicDetails] --16
	@OrderID INT
AS
BEGIN
	IF EXISTS(SELECT OrderID FROM Orders WHERE OrderID = @OrderID)
	BEGIN
		SELECT OrderID,
			OrderNumber, 
			OrderDate
		FROM Orders
		WHERE OrderID = @OrderID

		SELECT
			BundleID,
			MobileNumber,
			DisplayName 
		FROM OrderSubscribers INNER JOIN 
			(
				SELECT OrderSubscriberID, BundleID 
				FROM OrderSubscriberChangeRequests INNER JOIN 
					ChangeRequests ON OrderSubscriberChangeRequests.ChangeRequestID = ChangeRequests.ChangeRequestID INNER JOIN 
					OrderSubscriptions ON ChangeRequests.ChangeRequestID = OrderSubscriptions.ChangeRequestID
				GROUP BY OrderSubscriberID, BundleID 
			) OrderBundle ON OrderSubscribers.OrderSubscriberID = OrderBundle.OrderSubscriberID 
		WHERE OrderID = @OrderID
		RETURN 105
	END
	ELSE
	BEGIN
		RETURN 109 -- does not exists
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Order_GetOrderDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_GetOrderDetails] --[Order_GetOrderDetails] 34
	@OrderID INT
AS
BEGIN
	IF EXISTS(SELECT OrderID FROM Orders WHERE OrderID = @OrderID)
	BEGIN
		SELECT Orders.OrderID,
			OrderNumber, 
			OrderDate,
			Documents.IdentityCardNumber,
			Documents.IdentityCardType,
			BillingUnit,
			BillingFloor,
			BillingBuildingNumber,
			BillingBuildingName,
			BillingStreetName,
			BillingPostCode,
			BillingContactNumber,
			ReferralCode,
			PromotionCode,
			CAST(CASE OrderDocs.DocCount WHEN 0 THEN 0 ELSE 1 END AS bit) AS HaveDocuments,
			DeliveryInformation.[Name],
			DeliveryInformation.Email,
			IDType,
			IDNumber,
			IsSameAsBilling,
			ShippingUnit,
			ShippingFloor,
			ShippingBuildingNumber,
			ShippingBuildingName,
			ShippingStreetName,
			ShippingPostCode,
			ShippingContactNumber,
			AlternateRecipientContact,
			AlternateRecipientName,
			AlternateRecipientEmail,
			AlternateRecioientIDType,
			AlternateRecioientIDNumber,
			DeliveryInformation.PortalSlotID,
			DeliverySlots.SlotDate,
			DeliverySlots.SlotFromTime,
			DeliverySlots.SlotToTime,
			ScheduledDate,
			OrderCharges.ServiceFee
		FROM Orders LEFT OUTER JOIN
			OrderDocuments ON Orders.OrderID = OrderDocuments.OrderID LEFT OUTER JOIN
			Documents ON OrderDocuments.DocumentID = Documents.DocumentID LEFT OUTER JOIN 
			DeliveryInformation ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID LEFT OUTER JOIN 
			(	
				SELECT OrderID, SUM(OrderCharges.ServiceFee) AS ServiceFee
				FROM OrderCharges
				GROUP BY OrderID

				UNION ALL

				SELECT OrderID, SUM(ServiceFee) AS ServiceFee
				FROM SubscriberCharges INNER JOIN
					OrderSubscribers ON  SubscriberCharges.OrderSubscriberID = OrderSubscribers.OrderSubscriberID				
				GROUP BY OrderID
			)OrderCharges ON Orders.OrderID = OrderCharges.OrderID LEFT OUTER JOIN 
			DeliverySlots ON DeliveryInformation.PortalSlotID = DeliverySlots.PortalSlotID LEFT OUTER JOIN 
			(	
				SELECT OrderID, COUNT(Documents.IdentityCardType) AS DocCount
				FROM OrderDocuments INNER JOIN 
					Documents ON OrderDocuments.DocumentID = Documents.DocumentID
				GROUP BY OrderID
			)OrderDocs ON Orders.OrderID = OrderDocs.OrderID
		WHERE Orders.OrderID = @OrderID

		SELECT
			OrderSubscribers.OrderSubscriberID,
			OrderBundle.BundleID,
			MobileNumber,
			DisplayName,
			IsPrimaryNumber,
			Bundles.PlanMarketingName,
			Bundles.PortalDescription,
			Bundles.PortalSummaryDescription,
			TotalData,
			TotalSMS,
			TotalVoice,
			ActualSubscriptionFee,
			ApplicableSubscriptionFee,
			ServiceName,
			ActualServiceFee,
			ApplicableServiceFee,
			PremiumType,
			IsPorted,
			IsOwnNumber,
			DonorProvider,
			[PortedNumberTransferForm],
			[PortedNumberOwnedBy],
			[PortedNumberOwnerRegistrationID]		
		FROM OrderSubscribers INNER JOIN 
			(
				SELECT OrderSubscriberID, BundleID 
				FROM OrderSubscriberChangeRequests INNER JOIN 
					ChangeRequests ON OrderSubscriberChangeRequests.ChangeRequestID = ChangeRequests.ChangeRequestID INNER JOIN 
					OrderSubscriptions ON ChangeRequests.ChangeRequestID = OrderSubscriptions.ChangeRequestID
				GROUP BY OrderSubscriberID, BundleID 
			) OrderBundle ON OrderSubscribers.OrderSubscriberID = OrderBundle.OrderSubscriberID INNER JOIN 
			Bundles ON Bundles.BundleID = OrderBundle.BundleID INNER JOIN 
			(
				SELECT SUM(ISNULL([Data], 0)) AS TotalData, 
					SUM(ISNULL(Voice, 0)) AS TotalVoice, 
					SUM(ISNULL([SMS], 0)) AS TotalSMS,
					SUM((ISNULL(SubscriptionFee, 0))) AS ActualSubscriptionFee, 
					SUM((ISNULL(SubscriptionFee, 0) * (1 - BundlePlans.DiscountPercentage/100))) AS ApplicableSubscriptionFee,
					Bundles.BundleID
				FROM Bundles INNER JOIN 
					BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN
					Plans ON BundlePlans.PlanID = Plans.PlanID
				GROUP BY Bundles.BundleID
			) AS BundleOffering ON BundleOffering.BundleID = Bundles.BundleID INNER JOIN 
			(
				SELECT Bundles.BundleID,
					SUM(ISNULL(AdminServices.ServiceFee, 0)) AS ActualServiceFee, 
					SUM(ISNULL(AdminServices.ServiceFee, 0) * (1 - ISNULL(PlanAdminServices.AdminServiceDiscountPercentage, 0)/100)) AS ApplicableServiceFee,
					STUFF(
						 (SELECT ',' + PortalServiceName 
						  FROM AdminServices INNER JOIN 
							PlanAdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID
						  WHERE PlanAdminServices.BundleID = Bundles.BundleID
						  FOR XML PATH (''))
						 , 1, 1, '') AS ServiceName
				FROM AdminServices INNER JOIN 
					PlanAdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID INNER JOIN
					Bundles ON Bundles.BundleID = PlanAdminServices.BundleID
				GROUP BY Bundles.BundleID
			) AdminFee ON AdminFee.BundleID = Bundles.BundleID 
		WHERE OrderID = @OrderID
		ORDER BY OrderSubscribers.CreatedOn, IsPrimaryNumber DESC

		SELECT PortalServiceName,
			OrderCharges.ServiceFee,
			OrderCharges.IsRecurring,
			OrderCharges.IsGSTIncluded
		FROM OrderCharges INNER JOIN 
			AdminServices ON OrderCharges.AdminServiceID = AdminServices.AdminServiceID
		WHERE OrderID = @OrderID

		SELECT OrderSubscribers.OrderSubscriberID,
			PortalServiceName,
			SubscriberCharges.ServiceFee,
			SubscriberCharges.IsRecurring,
			SubscriberCharges.IsGSTIncluded
		FROM SubscriberCharges INNER JOIN
			OrderSubscribers ON  SubscriberCharges.OrderSubscriberID = OrderSubscribers.OrderSubscriberID INNER JOIN
			AdminServices ON SubscriberCharges.AdminServiceID = AdminServices.AdminServiceID
		WHERE OrderID = @OrderID
		RETURN 105
	END
	ELSE
	BEGIN
		RETURN 102 -- does not exists
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Order_GetPaymentTokenByID]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[Order_GetPaymentTokenByID] 
		 (@CustomerID INT,
		  @PaymentMethodID INT 
           )
AS
BEGIN

DECLARE @AccountID INT

SELECT @AccountID= AccountID FROM Customers C INNER JOIN Accounts A 
				
				ON A.CustomerID =C.CustomerID WHERE C.CustomerID=@CustomerID
		
		IF EXISTS (SELECT * FROM PaymentMethods WHERE @AccountID=@AccountID AND PaymentMethodID=@PaymentMethodID)	
		BEGIN
		
		SELECT PaymentMethodID,Token, SourceType FROM PaymentMethods WHERE @AccountID=@AccountID AND PaymentMethodID=@PaymentMethodID

		RETURN 105
		
		END

		ELSE

		RETURN 102

END
GO
/****** Object:  StoredProcedure [dbo].[Order_GetPortTypeByOrderID]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
CREATE PROCEDURE [dbo].[Order_GetPortTypeByOrderID] --152, '88400087'
	@OrderID int,
	@MobileNumber NVARCHAR(8)
AS
BEGIN
	IF EXISTS(SELECT * FROM OrderSubscribers WHERE OrderID = @OrderID AND MobileNumber = @MobileNumber)
	BEGIN
		SELECT IsPorted FROM OrderSubscribers WHERE OrderID = @OrderID AND MobileNumber = @MobileNumber
		RETURN 105
	END
	ELSE
		RETURN 119
END
GO
/****** Object:  StoredProcedure [dbo].[Order_RemovePaymentTokenByID]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_RemovePaymentTokenByID] 
		 (@CustomerID INT,
		  @PaymentMethodID INT 
           )
AS
BEGIN

DECLARE @AccountID INT

SELECT @AccountID= AccountID FROM Customers C INNER JOIN Accounts A 
				
				ON A.CustomerID =C.CustomerID WHERE C.CustomerID=@CustomerID
		
		IF EXISTS (SELECT * FROM PaymentMethods WHERE @AccountID=@AccountID AND PaymentMethodID=@PaymentMethodID)	
		BEGIN
		
		delete FROM PaymentMethods WHERE @AccountID=@AccountID AND PaymentMethodID=@PaymentMethodID

		RETURN 103
		
		END

		ELSE

		RETURN 102

END
GO
/****** Object:  StoredProcedure [dbo].[Order_SuspensionRequest]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Order_SuspensionRequest] @CustomerID INT
	,@MobileNumber NVARCHAR(8)
	
AS
BEGIN
	SELECT * FROM Subscriptions
END
GO
/****** Object:  StoredProcedure [dbo].[Order_TerminationRequest]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Order_TerminationRequest] @CustomerID INT
	,@MobileNumber NVARCHAR(8)
	
AS
BEGIN
	SELECT * FROM Subscriptions
END
GO
/****** Object:  StoredProcedure [dbo].[Order_UpdateCustomerPaymentMethodDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[Order_UpdateCustomerPaymentMethodDetails] 
		 (@CustomerID INT,
           @MaskedCardNumer NVARCHAR(255),          
           @Token NVARCHAR(255),
		   @CardHolderName NVARCHAR(255),
		   @CardIssuer NVARCHAR(255)

           )
AS
BEGIN

DECLARE @AccountID INT	

DECLARE @PaymentMethodID INT

SELECT @AccountID= AccountID FROM Customers C INNER JOIN Accounts A 
				
				ON A.CustomerID =C.CustomerID WHERE C.CustomerID=@CustomerID
		
		IF EXISTS (SELECT * FROM PaymentMethods WHERE @AccountID=@AccountID AND MaskedCardNumer=@MaskedCardNumer)	
		BEGIN
		UPDATE PaymentMethods SET  CardHolderName=@CardHolderName, 
		
		CardIssuer= @CardIssuer WHERE AccountID=@AccountID AND MaskedCardNumer=@MaskedCardNumer AND Token=@Token


		RETURN 101	
		END

		ELSE

		RETURN 102

END
GO
/****** Object:  StoredProcedure [dbo].[Order_UpdateSubscriberBasicDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_UpdateSubscriberBasicDetails] --Order_UpdateSubscriberBasicDetails 34, 4, 'Launch2019','90259894'
	@OrderID INT,
	@BundleID INT,	
	@DisplayName NVARCHAR(255),
	@MobileNumber NVARCHAR(8)
AS
BEGIN
	
	IF EXISTS (SELECT * FROM OrderSubscribers where OrderID=@OrderID and MobileNumber = @MobileNumber)
	BEGIN
		IF EXISTS(SELECT * FROM OrderSubscribers INNER JOIN Orders ON OrderSubscribers.OrderID = Orders.OrderID where OrderStatus = 0 AND Orders.OrderID = @OrderID)
		BEGIN 
			INSERT INTO OrderSubscriberLogs
			(
				OrderSubscriberID,
				OrderID,
				[LineStatus],
				[MobileNumber],
				[UserSessionID],
				[PremiumType],
				[IsPorted],
				[IsOwnNumber],
				[DonorProvider],
				[DisplayName],
				[IsPrimaryNumber]
			) 
			SELECT
				OrderSubscriberID,
				OrderID,
				IsActive,
				[MobileNumber],
				[UserSessionID],
				[PremiumType],
				[IsPorted],
				[IsOwnNumber],
				[DonorProvider],
				[DisplayName],
				[IsPrimaryNumber]
			FROM OrderSubscribers
			where OrderID=@OrderID and MobileNumber = @MobileNumber

			update OrderSubscribers set DisplayName =@DisplayName where OrderID=@OrderID and MobileNumber = @MobileNumber

			DECLARE @ChangeRequestID INT 

			SELECT @ChangeRequestID = ChangeRequestID 
			FROM OrderSubscribers os 
				INNER JOIN OrderSubscriberChangeRequests osc on os.OrderSubscriberID=osc.OrderSubscriberID 
			where OrderID=@OrderID and MobileNumber = @MobileNumber 

			INSERT INTO [OrderSubscriptionLogs]
				([ChangeRequestID]
				,[PlanID]
				,[PurchasedOn]
				,[BSSPlanCode]
				,[PlanName]
				,[SubscriptionFee]
				,[ValidFrom]
				,[ValidTo]
				,[Status]
				,[RefPlanID]
				,[PromotionID])
			SELECT [ChangeRequestID]
				,[PlanID]
				,[PurchasedOn]
				,[BSSPlanCode]
				,[PlanMarketingName]
				,[SubscriptionFee]
				,[ValidFrom]
				,[ValidTo]
				,[Status]
				,[BundleID]
				,[PromotionID]      
			FROM [OrderSubscriptions]
			WHERE ChangeRequestID=@ChangeRequestID

			DELETE FROM OrderSubscriptions WHERE ChangeRequestID=@ChangeRequestID

			 INSERT INTO OrderSubscriptions 
			 (
				ChangeRequestID,
				PlanID,
				BSSPlanCode,
				PlanMarketingName,
				SubscriptionFee,
				Status,
				BundleID,
				CreatedOn
			)
			SELECT 
				@ChangeRequestID,
				Plans.PlanID,
				BSSPlanCode,
				Plans.PlanMarketingName,
				SubscriptionFee,
				0,
				@BundleID,
				GETDATE()
			FROM Bundles INNER JOIN 
				BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN 
				Plans ON BundlePlans.PlanID = Plans.PlanID 
			WHERE Bundles.BundleID = @BundleID		

			IF @@ERROR<>0

			RETURN 106

			ELSE RETURN 101
		END
		ELSE
		RETURN 127
	END

	ELSE

	RETURN 102
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_CancelOrder]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[Orders_CancelOrder] @CustomerID INT	
	,@OrderID INT
	
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN

		DECLARE @CurrentDate DATE;

		SET @CurrentDate = CAST(GETDATE() AS DATE)

		-- update the Orders table OrderStatus = 8
		UPDATE Orders
		SET OrderStatus = 8
		WHERE OrderID = @OrderID
		-- Insert orderstatusLog table with 8 value
		INSERT OrderStatusLog
		(
			OrderID
			,OrderStatus
			,Remarks
			--,UpdatedOn
		)
		VALUES
		(
			@OrderID
			,8
			,'CancelByCustomer'
		)
		-- customer log trail table with customer id and in actiondescription = canceled by customer "Order id "
		INSERT INTO CustomerLogTrail (CustomerID, ActionDescription, ActionOn)   
  SELECT @CustomerID, 'Canceled By Customer for Order Id ' + CAST(@OrderID AS VARCHAR(20)), GETDATE() FROM Customers WHERE CustomerID = @CustomerID 
		
		COMMIT TRAN
	END TRY

	BEGIN CATCH
		ROLLBACK TRAN
	END CATCH

	IF @@ERROR <> 0
		RETURN 107
	ELSE
		RETURN 100
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_ConfirmedRescheduleDelivery]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_ConfirmedRescheduleDelivery] @CustomerID INT  

 ,@OrderID INT   
AS  
BEGIN  
 BEGIN TRY  
  BEGIN TRAN   
  
  DECLARE @CurrentDate DATE;  
  
  SET @CurrentDate = CAST(GETDATE() AS DATE)  
  
  DECLARE @OrderStatus INT;  
  DECLARE @DeliveryInformationID INT = 0  
  
  SELECT @OrderStatus = OrderStatus, @DeliveryInformationID = DeliveryInformationID  
  FROM Orders  
  WHERE OrderID = @OrderID  
  
  IF(@OrderStatus != 1 AND @OrderStatus != 6)  
  BEGIN  
  ROLLBACK TRAN  
   RETURN 135  
  END  
  
  DECLARE @AccountID INT  
  DECLARE @ServiceFee FLOAT = 0  
  
  SELECT @AccountID = Accounts.AccountID  
  FROM Accounts  
  WHERE CustomerID = @CustomerID  
  
  UPDATE DeliveryInformation SET  
  Name = R.Name  
  ,Email = R.Email  
  ,IDNumber = R.IDNumber  
  ,IDType = R.IDType    
  ,ShippingContactNumber = R.ShippingContactNumber  
  ,ShippingFloor = R.ShippingFloor  
  ,ShippingUnit = R.ShippingUnit  
  ,ShippingBuildingName = R.ShippingBuildingName  
  ,ShippingBuildingNumber = R.ShippingBuildingNumber  
  ,ShippingStreetName = R.ShippingStreetName  
  ,ShippingPostCode = R.ShippingPostCode  
  ,AlternateRecipientName = R.AlternateRecipientName  
  ,AlternateRecipientEmail = R.AlternateRecipientEmail  
  ,AlternateRecipientContact = R.AlternateRecipientContact  
  ,AlternateRecioientIDNumber = R.AlternateRecioientIDNumber  
  ,AlternateRecioientIDType = R.AlternateRecioientIDType  
  ,PortalSlotID = R.PortalSlotID  
  ,ScheduledDate  = R.ScheduledDate  
  FROM DeliveryInformation 
  INNER JOIN  Orders ON DeliveryInformation.DeliveryInformationID = Orders.DeliveryInformationID 
  INNER JOIN  
   (  
    SELECT Name  
     ,Email  
     ,IDNumber  
     ,IDType  
     ,ShippingContactNumber  
     ,ShippingFloor  
     ,ShippingUnit  
     ,ShippingBuildingName  
     ,ShippingBuildingNumber  
     ,ShippingStreetName  
     ,ShippingPostCode  
     ,AlternateRecipientName  
     ,AlternateRecipientEmail  
     ,AlternateRecipientContact  
     ,AlternateRecioientIDNumber  
     ,AlternateRecioientIDType  
     ,PortalSlotID  
     ,ScheduledDate  
     ,SourceID  
    FROM  
    (  
     SELECT Name  
      ,Email  
      ,IDNumber  
      ,IDType  
      ,ShippingContactNumber  
      ,ShippingFloor  
      ,ShippingUnit  
      ,ShippingBuildingName  
      ,ShippingBuildingNumber  
      ,ShippingStreetName  
      ,ShippingPostCode  
      ,AlternateRecipientName  
      ,AlternateRecipientEmail  
      ,AlternateRecipientContact  
      ,AlternateRecioientIDNumber  
      ,AlternateRecioientIDType  
      ,PortalSlotID  
      ,ScheduledDate  
      ,SourceID  
      ,ROW_NUMBER() OVER(PARTITION BY SourceID, SourceType ORDER BY CreatedOn DESC) AS RNum     
     FROM RescheduleDeliveryInformation   
     WHERE SourceID = @OrderID AND SourceType = 'Orders'  
    ) A   
    WHERE RNum = 1  
   ) R ON Orders.OrderID = R.SourceID     
  WHERE DeliveryInformation.DeliveryInformationID = @DeliveryInformationID     
  
  INSERT INTO DeliveryInformationLog(  
   DeliveryInformationID  
   ,ShippingNumber  
   ,Name  
   ,Email  
   ,IDNumber  
   ,IDType  
   ,IsSameAsBilling  
   ,ShippingContactNumber  
   ,ShippingFloor  
   ,ShippingUnit  
   ,ShippingBuildingName  
   ,ShippingBuildingNumber  
   ,ShippingStreetName  
   ,ShippingPostCode  
   ,AlternateRecipientName  
   ,AlternateRecipientEmail  
   ,AlternateRecipientContact  
   ,AlternateRecioientIDNumber  
   ,AlternateRecioientIDType  
   ,PortalSlotID  
   ,ScheduledDate  
   ,DeliveryVendor  
   ,DeliveryOn  
   ,DeliveryTime  
   ,TrackingCode  
   ,TrackingUrl  
   ,DeliveryFee  
   ,VoucherID  
   ,LoggedOn  
   ,RescheduleReasonID  
  )  
  SELECT DeliveryInformationID  
    ,ShippingNumber  
    ,Name  
    ,Email  
    ,IDNumber  
    ,IDType  
    ,IsSameAsBilling  
    ,ShippingContactNumber  
    ,ShippingFloor  
    ,ShippingUnit  
    ,ShippingBuildingName  
    ,ShippingBuildingNumber  
    ,ShippingStreetName  
    ,ShippingPostCode  
    ,AlternateRecipientName  
    ,AlternateRecipientEmail  
    ,AlternateRecipientContact  
    ,AlternateRecioientIDNumber  
    ,AlternateRecioientIDType  
    ,PortalSlotID  
    ,ScheduledDate  
    ,DeliveryVendor  
    ,DeliveryOn  
    ,DeliveryTime  
    ,VendorTrackingCode  
    ,VendorTrackingUrl  
    ,DeliveryFee  
    ,VoucherID  
    ,GETDATE()  
    ,CASE @OrderStatus WHEN 1 THEN 3 ELSE 2 END  
   FROM DeliveryInformation  
   WHERE DeliveryInformationID = @DeliveryInformationID  
     
  COMMIT TRAN  
 END TRY  
  
 BEGIN CATCH  
  ROLLBACK TRAN  
 END CATCH  
  
 IF @@ERROR <> 0  
  RETURN 107  
 ELSE  
  RETURN 100  
END  
GO
/****** Object:  StoredProcedure [dbo].[Orders_CR_BuySharedVAS]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Orders_CR_BuySharedVAS] @CustomerID INT    
 ,@BundleID INT    
 ,@RequestType NVARCHAR(50)    
AS    
BEGIN    
 DECLARE @CurrentDate DATE;    
    
 SET @CurrentDate = CAST(GETDATE() AS DATE)    
    
 BEGIN TRY    
  BEGIN TRAN    
    
  DECLARE @RequestTypeID INT    
    
  SELECT @RequestTypeID = RequestTypeID    
  FROM RequestTypes    
  WHERE RequestType = @RequestType    
    
  DECLARE @AccountID INT    
    
  SELECT @AccountID = Accounts.AccountID    
  FROM Accounts    
  WHERE CustomerID = @CustomerID    
    
  IF (@AccountID IS NULL)    
  BEGIN    
   ROLLBACK TRAN    
    
   RETURN 102    
  END    
  
  DECLARE @IsRecurring INT  
  
  SELECT TOP 1 @IsRecurring = Plans.IsRecurring    
  FROM Bundles      
  INNER JOIN BundlePlans ON Bundles.BundleID = BundlePlans.BundleID    
  INNER JOIN Plans ON BundlePlans.PlanID = Plans.PlanID  
 
  WHERE Bundles.BundleID = @BundleID 
    -- To Check Monthly and Pay as you use vases
  IF(@IsRecurring = 1 OR @IsRecurring = -1)
  BEGIN
	IF EXISTS (SELECT * FROM AccountSubscriptions where AccountID = @AccountID AND BundleID = @BundleID AND (AccountSubscriptions.Status = 0 OR AccountSubscriptions.Status = 1))
	BEGIN
		ROLLBACK TRAN  
		RETURN 128
	END
  END
  
  INSERT INTO ChangeRequests (    
   RequestTypeID    
   ,RequestReason    
   ,OrderStatus    
   )    
  VALUES (    
   @RequestTypeID    
   ,'CR-AddSharedVAS'    
   ,1    
   );    
    
  DECLARE @ChangeRequestID INT    
    
  SET @ChangeRequestID = SCOPE_IDENTITY();    
    
  DECLARE @OrderNumber NVARCHAR(500)    
    
  SET @OrderNumber = dbo.ufnGetCRNumber()    
    
  UPDATE ChangeRequests    
  SET OrderNumber = @OrderNumber    
  WHERE ChangeRequestID = @ChangeRequestID    
    
  INSERT INTO AccountChangeRequests (    
   ChangeRequestID    
   ,AccountID    
   )    
  VALUES (    
   @ChangeRequestID    
   ,@AccountID    
   )    
    
  INSERT INTO [dbo].[OrderSubscriptions] (    
   [ChangeRequestID]    
   ,[PlanID]    
   ,[PurchasedOn]    
   ,[BSSPlanCode]    
   ,[PlanMarketingName]    
   ,[SubscriptionFee]    
   ,[ValidFrom]    
   ,[ValidTo]    
   ,[Status]    
   ,[BundleID]    
   )    
  SELECT @ChangeRequestID    
   ,Plans.PlanID    
   ,@CurrentDate    
   ,BSSPlanCode    
   ,Plans.PlanMarketingName    
   ,SubscriptionFee    
   ,CASE ISNULL(BundlePlans.DurationValue, 0)    
    WHEN 0    
     THEN NULL    
    ELSE @CurrentDate    
    END    
   ,CASE ISNULL(BundlePlans.DurationValue, 0)    
    WHEN 0    
     THEN NULL    
    ELSE DATEADD(month, DurationValue, @CurrentDate) --CASE DurationUOM WHEN 'month' THEN month WHEN 'year' THEN year ELSE day END      
    END    
   ,0    
   ,@BundleID    
  FROM Bundles    
  INNER JOIN BundlePlans ON Bundles.BundleID = BundlePlans.BundleID    
  INNER JOIN Plans ON BundlePlans.PlanID = Plans.PlanID    
  WHERE Bundles.BundleID = @BundleID    
    
  DECLARE @OrderSubscriptionID INT  
  SET @OrderSubscriptionID = SCOPE_IDENTITY();  
  
  INSERT INTO AccountSubscriptions (    
   AccountID    
   ,PlanID    
   ,PurchasedOn    
   --,ActivatedOn      
   ,BSSPlanCode    
   ,PlanMarketingName    
   ,SubscriptionFee    
   ,Status    
   --,ValidFrom      
   --,ValidTo      
   ,BundleID    
   ,CreatedOn    
   ,RefOrderSubscriptionID  
   )    
  SELECT @AccountID    
   ,Plans.PlanID    
   ,@CurrentDate    
   --,NULL      
   ,BSSPlanCode    
   ,Plans.PlanMarketingName    
   ,SubscriptionFee    
   ,0  
   ,@BundleID    
   ,@CurrentDate  
   ,@OrderSubscriptionID    
  FROM Bundles    
  INNER JOIN BundlePlans ON Bundles.BundleID = BundlePlans.BundleID    
  INNER JOIN Plans ON BundlePlans.PlanID = Plans.PlanID    
  WHERE Bundles.BundleID = @BundleID    
     
   INSERT INTO AccountSubscriptionLog  
   (  
    AccountSubscriptionID,  
    Status,  
    Source,  
    UpdatedOn,  
    RefChangeRequestID  
   )  
   SELECT  
    AccountSubscriptionID,  
    0,  
    0,  
    GETDATE(),  
    @ChangeRequestID  
   FROM AccountSubscriptions  
   WHERE RefOrderSubscriptionID = @OrderSubscriptionID  
    
  SELECT @ChangeRequestID AS ChangeRequestID    
   ,BSSPlanCode    
   ,Plans.PlanMarketingName    
   ,SubscriptionFee    
  FROM Bundles    
  INNER JOIN BundlePlans ON Bundles.BundleID = BundlePlans.BundleID    
  INNER JOIN Plans ON BundlePlans.PlanID = Plans.PlanID    
  WHERE Bundles.BundleID = @BundleID    
    
  COMMIT TRAN    
 END TRY    
    
 BEGIN CATCH    
  ROLLBACK TRAN    
 END CATCH    
    
 IF @@ERROR <> 0    
  RETURN 107    
 ELSE    
  RETURN 100    
END 
GO
/****** Object:  StoredProcedure [dbo].[Orders_CR_BuyVAS]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Orders_CR_BuyVAS] @CustomerID INT    
 ,@Quantity INT    
 ,@BundleID INT    
 ,@MobileNumber VARCHAR(8)    
 ,@RequestType NVARCHAR(50)    
AS    
BEGIN    
 DECLARE @CurrentDate DATE;    
    
 SET @CurrentDate = CAST(GETDATE() AS DATE)    
    
 BEGIN TRY    
  BEGIN TRAN    
    
  DECLARE @RequestTypeID INT    
    
  SELECT @RequestTypeID = RequestTypeID    
  FROM RequestTypes    
  WHERE RequestType = @RequestType    
    
  DECLARE @AccountID INT    
  DECLARE @SubscriberID INT    
    
  SELECT @AccountID = Accounts.AccountID    
   ,@SubscriberID = SubscriberID    
  FROM Accounts    
  INNER JOIN Subscribers ON Accounts.AccountID = Subscribers.AccountID    
  WHERE CustomerID = @CustomerID    
   AND MobileNumber = @MobileNumber    
    
  IF (@SubscriberID IS NULL)    
  BEGIN    
   ROLLBACK TRAN    
   RETURN 102    
  END    
  
  DECLARE @Recurring INT = 0
  
  SELECT TOP 1 @Recurring = Plans.IsRecurring  FROM Bundles
  INNER JOIN BundlePlans ON BundlePlans.BundleID = Bundles.BundleID
  INNER JOIN Plans ON Plans.PlanID = BundlePlans.PlanID
  WHERE Bundles.BundleID = @BundleID
  -- To Check Monthly and Pay as you use vases
  IF(@Recurring = 1 OR @Recurring = -1)
  BEGIN
	IF EXISTS (SELECT * FROM Subscriptions where SubscriberID = @SubscriberID AND BundleID = @BundleID AND (Subscriptions.Status = 0 OR Subscriptions.Status = 1))
	BEGIN
		ROLLBACK TRAN  
		RETURN 128
	END
  END

  INSERT INTO ChangeRequests (    
   RequestTypeID    
   ,RequestReason    
   ,OrderStatus    
   )    
  VALUES (    
   @RequestTypeID    
   ,'CR-AddVAS'    
   ,1    
   );    
    
  DECLARE @ChangeRequestID INT    
    
  SET @ChangeRequestID = SCOPE_IDENTITY();    
    
  DECLARE @OrderNumber NVARCHAR(500)    
    
  SET @OrderNumber = dbo.ufnGetCRNumber()    
    
  UPDATE ChangeRequests    
  SET OrderNumber = @OrderNumber    
  WHERE ChangeRequestID = @ChangeRequestID    
    
  INSERT INTO SubscriberRequests (    
   ChangeRequestID    
   ,SubscriberID    
   )    
  VALUES (    
   @ChangeRequestID    
   ,@SubscriberID    
   )    
    
  --WHILE (@Quantity > 0)    
  --BEGIN    
   INSERT INTO [dbo].[OrderSubscriptions] (    
    [ChangeRequestID]    
    ,[PlanID]    
    ,[PurchasedOn]    
    ,[BSSPlanCode]    
    ,[PlanMarketingName]    
    ,[SubscriptionFee]    
    ,[ValidFrom]    
    ,[ValidTo]    
    ,[Status]    
    ,[BundleID]    
    )    
   SELECT @ChangeRequestID    
    ,Plans.PlanID    
    ,@CurrentDate    
    ,BSSPlanCode    
    ,Plans.PlanMarketingName    
    ,SubscriptionFee    
    ,CASE ISNULL(BundlePlans.DurationValue, 0)    
     WHEN 0    
      THEN NULL    
     ELSE @CurrentDate    
     END    
    ,CASE ISNULL(BundlePlans.DurationValue, 0)    
     WHEN 0    
      THEN NULL    
     ELSE DATEADD(month, DurationValue, @CurrentDate) --CASE DurationUOM WHEN 'month' THEN month WHEN 'year' THEN year ELSE day END      
     END    
    ,0    
    ,@BundleID    
   FROM Bundles    
   INNER JOIN BundlePlans ON Bundles.BundleID = BundlePlans.BundleID    
   INNER JOIN Plans ON BundlePlans.PlanID = Plans.PlanID    
   WHERE Bundles.BundleID = @BundleID    
   DECLARE @OrderSubscriptionID INT  
   SET @OrderSubscriptionID = SCOPE_IDENTITY();  
  -- SET @Quantity = @Quantity - 1;    
    
  -- IF @Quantity = 0    
  --  BREAK    
  -- ELSE    
  --  CONTINUE    
  --END    
    
  INSERT INTO ChangeRequestCharges (    
   ChangeRequestID    
   ,AdminServiceID    
   ,ServiceFee    
   ,IsGSTIncluded    
   ,IsRecurring    
   ,ReasonType    
   ,ReasonID    
   )    
  SELECT @ChangeRequestID    
   ,AdminServices.AdminServiceID    
   ,ServiceFee    
   ,IsGSTIncluded    
   ,IsRecurring    
   ,'CR-AddVAS'    
   ,@ChangeRequestID    
  FROM PlanAdminServices    
  INNER JOIN AdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID    
  WHERE BundleID = @BundleID    
    
  --Process the CR on subscriber profile level    
  INSERT INTO Subscriptions (    
   SubscriberID    
   ,PlanID    
   ,PurchasedOn    
   --,ActivatedOn    
   ,BSSPlanCode    
   ,PlanMarketingName    
   ,SubscriptionFee   
   ,Status   
   --,ValidFrom    
   --,ValidTo    
   ,BundleID    
   ,CreatedOn    
   ,RefOrderSubscriptionID  
   )    
  SELECT @SubscriberID    
   ,Plans.PlanID    
   ,@CurrentDate    
   --,NULL    
   ,BSSPlanCode    
   ,Plans.PlanMarketingName    
   ,SubscriptionFee    
   ,0  
   ,@BundleID    
   ,@CurrentDate    
   ,@OrderSubscriptionID  
  FROM Bundles    
  INNER JOIN BundlePlans ON Bundles.BundleID = BundlePlans.BundleID    
  INNER JOIN Plans ON BundlePlans.PlanID = Plans.PlanID    
  WHERE Bundles.BundleID = @BundleID    
    
  
 INSERT INTO SubscriptionLog  
 (  
  SubscriptionID,  
  Status,  
  Source,  
  UpdatedOn,  
  RefChangeRequestID  
 )  
 SELECT  
  SubscriptionID,  
  0,  
  0,  
  GETDATE(),  
  @ChangeRequestID  
 FROM Subscriptions  
 WHERE RefOrderSubscriptionID = @OrderSubscriptionID  
  
  SELECT @ChangeRequestID AS ChangeRequestID    
   ,BSSPlanCode    
   ,Plans.PlanMarketingName    
   ,SubscriptionFee    
  FROM Bundles    
  INNER JOIN BundlePlans ON Bundles.BundleID = BundlePlans.BundleID    
  INNER JOIN Plans ON BundlePlans.PlanID = Plans.PlanID    
  WHERE Bundles.BundleID = @BundleID    
    
  COMMIT TRAN    
 END TRY    
    
 BEGIN CATCH    
  ROLLBACK TRAN    
 END CATCH    
    
 IF @@ERROR <> 0    
  RETURN 107    
 ELSE    
  RETURN 100    
END 
GO
/****** Object:  StoredProcedure [dbo].[Orders_CR_ChangePlan]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_CR_ChangePlan] @CustomerID INT      
 ,@MobileNumber NVARCHAR(8)      
 ,@BundleID INT      
 ,@RequestType NVARCHAR(100)      
AS      
BEGIN      
 BEGIN TRY      
  BEGIN TRAN        
   DECLARE @CurrentDate DATE;      
      
   SET @CurrentDate = CAST(GETDATE() AS DATE)      
      
   DECLARE @RequestTypeID INT      
      
   SELECT @RequestTypeID = RequestTypeID      
   FROM RequestTypes      
   WHERE RequestType = @RequestType      
      
   DECLARE @AccountID INT      
   DECLARE @SubscriberID INT      
      
   SELECT @AccountID = Accounts.AccountID      
   ,@SubscriberID = SubscriberID      
   FROM Accounts      
   INNER JOIN Subscribers ON Accounts.AccountID = Subscribers.AccountID      
   WHERE CustomerID = @CustomerID      
   AND MobileNumber = @MobileNumber      
      
   IF (@SubscriberID IS NULL)      
   BEGIN      
    ROLLBACK TRAN      
    RETURN 102      
   END      
    
	IF (  
    'Created' =   
     (SELECT TOP 1 State  
     FROM SubscriberStates WHERE SubscriberID = @SubscriberID ORDER BY StateDate DESC)        
     )  
  BEGIN
	 ROLLBACK TRAN  
    RETURN 139  
  END
   
   IF EXISTS (      
    SELECT *      
    FROM ChangeRequests      
    INNER JOIN SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID      
    WHERE SubscriberID = @SubscriberID      
     AND RequestTypeID = @RequestTypeID      
     AND OrderStatus = 1      
   )      
   BEGIN      
    ROLLBACK TRAN      
    RETURN 128      
   END      
      
   DECLARE @ChangeRequestID INT      
      
   IF EXISTS (      
    SELECT *      
    FROM ChangeRequests      
    INNER JOIN SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID      
    WHERE SubscriberID = @SubscriberID      
     AND RequestTypeID = @RequestTypeID      
     AND OrderStatus = 0    
   )      
   BEGIN      
    SELECT @ChangeRequestID = ChangeRequests.ChangeRequestID    
    FROM ChangeRequests      
     INNER JOIN SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID      
    WHERE SubscriberID = @SubscriberID      
     AND RequestTypeID = @RequestTypeID      
     AND OrderStatus = 0    
   END      
   ELSE    
   BEGIN    
    INSERT INTO ChangeRequests     
    (      
     RequestTypeID      
     ,RequestReason      
     ,OrderStatus      
    )      
    VALUES (      
     @RequestTypeID      
     ,'CR-ChangePlan'      
     ,0      
    )      
      
    SET @ChangeRequestID = SCOPE_IDENTITY();      
   END    
   DECLARE @OldPlanPriorityOrder INT      
   DECLARE @OldPlanBundleID INT      
   DECLARE @NewPlanPriorityOrder INT      
      
   SELECT @NewPlanPriorityOrder = PlanPriorityOrder      
   FROM Bundles      
   WHERE BundleID = @BundleID -- 34 -- 1      
      
   SELECT @OldPlanPriorityOrder = PlanPriorityOrder, @OldPlanBundleID = Bundles.BundleID -- 0      
   FROM Customers       
    INNER JOIN Accounts ON Customers.CustomerID = Accounts.CustomerID      
    INNER JOIN Subscribers ON Subscribers.AccountID = Accounts.AccountID      
    INNER JOIN Subscriptions ON Subscriptions.SubscriberID = Subscribers.SubscriberID      
    INNER JOIN Plans ON Subscriptions.PlanID = Plans.PlanID      
     AND Plans.PlanType = 0      
    INNER JOIN Bundles ON Subscriptions.BundleID = Bundles.BundleID       
   WHERE Customers.CustomerID = @CustomerID       
    AND (Subscriptions.Status = 0 OR Subscriptions.Status = 1)       
    AND ISNULL(Subscriptions.ValidTo, @CurrentDate) >= @CurrentDate      
    AND Subscribers.MobileNumber = @MobileNumber      
      
   IF (@BundleID = @OldPlanBundleID)      
   BEGIN      
    ROLLBACK TRAN      
    RETURN 131      
   END      
      
   UPDATE ChangeRequests      
   SET OrderNumber = dbo.ufnGetCRNumber()      
   WHERE ChangeRequestID = @ChangeRequestID      
    
   DELETE FROM ChangeRequestCharges WHERE ChangeRequestID = @ChangeRequestID    
   IF EXISTS (      
    SELECT *      
    FROM RequestTypes      
    WHERE RequestTypeID = @RequestTypeID      
     AND IsChargable = 1      
     AND @OldPlanPriorityOrder > @NewPlanPriorityOrder -- downgrade      
   )      
   BEGIN      
    INSERT INTO ChangeRequestCharges   
    (      
     ChangeRequestID      
     ,AdminServiceID      
     ,ServiceFee      
     ,IsGSTIncluded      
     ,IsRecurring      
     ,ReasonType      
     ,ReasonID      
    )      
    SELECT     
     @ChangeRequestID      
     ,AdminServiceID      
     ,ServiceFee      
     ,IsGSTIncluded      
     ,IsRecurring      
     ,'CR-ChangePlan'      
     ,@RequestTypeID      
    FROM AdminServices      
    WHERE ServiceType = @RequestType      
   END      
      
   DECLARE @ServiceFee FLOAT = 0       
   SELECT @ServiceFee = SUM(ChangeRequestCharges.ServiceFee)        
   FROM ChangeRequestCharges        
   GROUP BY ChangeRequestID        
   HAVING ChangeRequestID = @ChangeRequestID        
        
   IF (ISNULL(@ServiceFee,0) = 0)  
   BEGIN        
    UPDATE ChangeRequests        
    SET OrderStatus = 1        
    WHERE ChangeRequestID = @ChangeRequestID        
   END        
      
   DELETE FROM SubscriberRequests WHERE ChangeRequestID = @ChangeRequestID AND SubscriberID = @SubscriberID    
   INSERT INTO SubscriberRequests (      
    ChangeRequestID      
    ,SubscriberID      
   )      
   VALUES (      
    @ChangeRequestID      
    ,@SubscriberID      
   )      
       
   DELETE FROM [OrderSubscriptions] WHERE ChangeRequestID = @ChangeRequestID    
   INSERT INTO [dbo].[OrderSubscriptions]     
   (      
    [ChangeRequestID]      
    ,[PlanID]      
    ,[PurchasedOn]      
    ,[BSSPlanCode]      
    ,[PlanMarketingName]      
    ,[Status]      
    ,[OldBundleID]    
    ,[BundleID]    
   )      
   SELECT     
    @ChangeRequestID AS ChangeRequestID      
    ,Plans.PlanID      
    ,@CurrentDate AS CurrentDate      
    ,BSSPlanCode      
    ,Plans.PlanMarketingName      
    ,0      
    ,@OldPlanBundleID    
    ,Bundles.BundleID    
   FROM Plans      
    INNER JOIN BundlePlans ON Plans.PlanID = BundlePlans.PlanID      
    INNER JOIN Bundles ON Bundles.BundleID = BundlePlans.BundleID      
   WHERE Bundles.BundleID = @BundleID      
      
   SELECT     
    ChangeRequests.ChangeRequestID      
    ,OrderNumber      
    ,RequestOn      
    ,Accounts.BillingUnit      
    ,Accounts.BillingFloor      
    ,Accounts.BillingBuildingNumber      
    ,Accounts.BillingBuildingName      
    ,Accounts.BillingStreetName      
    ,Accounts.BillingPostCode      
    ,Accounts.BillingContactNumber      
    ,AccountName AS Name      
    ,Accounts.Email      
    ,Documents.IdentityCardType AS IdentityCardType      
    ,Documents.IdentityCardNumber AS IdentityCardNumber      
    ,@OldPlanBundleID AS OldPlanBundleID      
    ,@BundleID AS NewBundleID      
   FROM ChangeRequests      
    INNER JOIN SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID      
    INNER JOIN Subscribers ON SubscriberRequests.SubscriberID = SubscriberRequests.SubscriberID      
    INNER JOIN Accounts ON Accounts.AccountID = Subscribers.AccountID      
    LEFT JOIN     
    (      
     SELECT CustomerID      
     ,IdentityCardNumber      
     ,IdentityCardType      
     FROM     
     (      
      SELECT CustomerID      
       ,IdentityCardNumber      
       ,IdentityCardType      
       ,ROW_NUMBER() OVER (PARTITION BY CustomerID ORDER BY CreatedOn DESC) AS RNum      
      FROM Documents      
     ) A      
     WHERE Rnum = 1      
    ) Documents ON Documents.CustomerID = Accounts.CustomerID      
   WHERE Accounts.AccountID = @AccountID      
    AND ChangeRequests.ChangeRequestID = @ChangeRequestID      
      
   SELECT     
    PortalServiceName      
    ,SUM(ChangeRequestCharges.ServiceFee) AS ServiceFee      
   FROM ChangeRequestCharges      
    INNER JOIN AdminServices ON ChangeRequestCharges.AdminServiceID = AdminServices.AdminServiceID      
   GROUP BY PortalServiceName      
    ,ChangeRequestCharges.IsRecurring      
    ,ChangeRequestCharges.IsGSTIncluded      
    ,ChangeRequestCharges.ChangeRequestID      
   HAVING ChangeRequestCharges.ChangeRequestID = @ChangeRequestID      
      
  COMMIT TRAN      
 END TRY      
      
 BEGIN CATCH      
  ROLLBACK TRAN      
 END CATCH      
      
 IF @@ERROR <> 0      
 RETURN 107      
 ELSE      
 RETURN 100      
END 
GO
/****** Object:  StoredProcedure [dbo].[Orders_CR_GetTerminationDate]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_CR_GetTerminationDate]-- 38
	@CustomerID INT
AS
BEGIN
	SELECT dbo.GetTeminationDate() AS TerminationDate
	return 105
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_CR_InsertRemoveSharedVAS]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_CR_InsertRemoveSharedVAS] @CustomerID INT  
 ,@AccountSubscriptionID INT  
 --,@PlanID INT  
 ,@RequestType NVARCHAR(100)  
AS  
BEGIN  
 BEGIN TRY  
  BEGIN TRAN  
  
  DECLARE @CurrentDate DATE;  
  
  SET @CurrentDate = CAST(GETDATE() AS DATE)  
  
  DECLARE @RequestTypeID INT  
  
  SELECT @RequestTypeID = RequestTypeID  
  FROM RequestTypes  
  WHERE RequestType = @RequestType  
  
  DECLARE @AccountID INT  
  
  SELECT @AccountID = Accounts.AccountID  
  FROM Accounts  
  WHERE CustomerID = @CustomerID  
  
  --AND MobileNumber = @MobileNumber    
  IF (@AccountID IS NULL)  
  BEGIN  
   ROLLBACK TRAN  
   RETURN 102  
  END  
  
  IF EXISTS (  
    SELECT *  
    FROM AccountSubscriptions  
    WHERE AccountSubscriptionID = @AccountSubscriptionID  
     AND Status = 2  
    )  
  BEGIN  
   ROLLBACK TRAN  
  
   RETURN 128  
  END  
  
  DECLARE @PlanID INT  
  DECLARE @BundleID INT  
  
  SELECT @PlanID = PlanID, @BundleID = BundleID 
  FROM AccountSubscriptions  
  WHERE AccountSubscriptionID = @AccountSubscriptionID  
  
  IF EXISTS (  
    SELECT *  
    FROM AccountSubscriptions  
    WHERE AccountID = @AccountID  
     AND PlanID = @PlanID  
    )  
  BEGIN  
   INSERT INTO ChangeRequests (  
    RequestTypeID  
    ,RequestReason  
    ,OrderStatus  
    )  
   VALUES (  
    @RequestTypeID  
    ,'CR-RemoveSharedVAS'  
    ,1  
    );  
  
   DECLARE @ChangeRequestID INT  
  
   SET @ChangeRequestID = SCOPE_IDENTITY();  
  
   UPDATE ChangeRequests  
   SET OrderNumber = dbo.ufnGetCRNumber()  
   WHERE ChangeRequestID = @ChangeRequestID  
  
   INSERT INTO AccountChangeRequests (  
    ChangeRequestID  
    ,AccountID  
    )  
   VALUES (  
    @ChangeRequestID  
    ,@AccountID  
    )  
  
   INSERT INTO [dbo].[OrderSubscriptions] (  
    [ChangeRequestID]  
    ,[PlanID]  
    ,[PurchasedOn]  
    ,[BSSPlanCode]  
    ,[PlanMarketingName]  
    ,[Status]  
	,BundleID
    )  
   SELECT @ChangeRequestID AS ChangeRequestID  
    ,Plans.PlanID  
    ,@CurrentDate AS CurrentDate  
    ,BSSPlanCode  
    ,Plans.PlanMarketingName  
    ,0  
	,@BundleID
   FROM Plans  
   WHERE PlanID = @PlanID  
  
   DECLARE @OrderSubscriptionID INT  
   SET @OrderSubscriptionID = SCOPE_IDENTITY();  
  
   UPDATE AccountSubscriptions  
   SET STATUS = 2,  
    RefOrderSubscriptionID = @OrderSubscriptionID  
   WHERE AccountSubscriptionID = @AccountSubscriptionID  
     
   INSERT INTO AccountSubscriptionLog  
   (  
    AccountSubscriptionID,  
    Status,  
    Source,  
    UpdatedOn,  
    RefChangeRequestID  
   )  
   SELECT  
    AccountSubscriptionID,  
    2,  
    0,  
    GETDATE(),  
    @ChangeRequestID  
   FROM AccountSubscriptions  
   WHERE AccountSubscriptionID = @AccountSubscriptionID  
  
   SELECT @ChangeRequestID AS ChangeRequestID  
    ,BSSPlanCode  
    ,Plans.PlanMarketingName  
    ,CreatedOn AS 'CurrentDate'  
    ,PlanID  
   FROM Plans  
   WHERE Plans.PlanID = @PlanID  
  END  
  ELSE  
  BEGIN  
   ROLLBACK TRAN  
  
   RETURN 102  
  END  
  
  COMMIT TRAN  
 END TRY  
  
 BEGIN CATCH  
  ROLLBACK TRAN  
 END CATCH  
  
 IF @@ERROR <> 0  
  RETURN 107  
 ELSE  
  RETURN 100  
END  
GO
/****** Object:  StoredProcedure [dbo].[Orders_CR_InsertRemoveVAS]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_CR_InsertRemoveVAS] @CustomerID INT  
 ,@SubscriptionID INT  
 ,@MobileNumber NVARCHAR(8)  
 --,@PlanID INT      
 ,@RequestType NVARCHAR(100)  
AS  
BEGIN  
 BEGIN TRY  
  BEGIN TRAN  
  
  DECLARE @CurrentDate DATE;  
  
  SET @CurrentDate = CAST(GETDATE() AS DATE)  
  
  DECLARE @RequestTypeID INT  
  
  SELECT @RequestTypeID = RequestTypeID  
  FROM RequestTypes  
  WHERE RequestType = @RequestType  
  
  DECLARE @AccountID INT  
  DECLARE @SubscriberID INT 
  DECLARE @PlanID INT   
  DECLARE @BundleID INT  
  
  SELECT @AccountID = Accounts.AccountID  
   ,@SubscriberID = SubscriberID  
  FROM Accounts  
  INNER JOIN Subscribers ON Accounts.AccountID = Subscribers.AccountID  
  WHERE CustomerID = @CustomerID  
   AND MobileNumber = @MobileNumber  
  
  IF (@SubscriberID IS NULL)  
  BEGIN  
   ROLLBACK TRAN  
  
   RETURN 102  
  END  
  
  IF EXISTS (  
    SELECT *  
    FROM Subscriptions  
    WHERE SubscriptionID = @SubscriptionID  
     AND STATUS = 2  
    )  
  BEGIN  
   ROLLBACK TRAN  
  
   RETURN 128  
  END  
  
  SELECT @PlanID = PlanID, @BundleID = BundleID
  FROM Subscriptions  
  WHERE SubscriptionID = @SubscriptionID  
  
  INSERT INTO ChangeRequests (  
   RequestTypeID  
   ,RequestReason  
   ,OrderStatus  
   )  
  VALUES (  
   @RequestTypeID  
   ,'CR-RemoveVAS'  
   ,1  
   );  
  
  DECLARE @ChangeRequestID INT  
  
  SET @ChangeRequestID = SCOPE_IDENTITY();  
  
  UPDATE ChangeRequests  
  SET OrderNumber = dbo.ufnGetCRNumber()  
  WHERE ChangeRequestID = @ChangeRequestID  
  
  INSERT INTO SubscriberRequests (  
   ChangeRequestID  
   ,SubscriberID  
   )  
  VALUES (  
   @ChangeRequestID  
   ,@SubscriberID  
   )  
  
  INSERT INTO [dbo].[OrderSubscriptions] (  
   [ChangeRequestID]  
   ,[PlanID]  
   ,[PurchasedOn]  
   ,[BSSPlanCode]  
   ,[PlanMarketingName]  
   ,[Status]  
   ,BundleID
   )  
  SELECT @ChangeRequestID AS ChangeRequestID  
   ,Plans.PlanID  
   ,@CurrentDate AS CurrentDate  
   ,BSSPlanCode  
   ,Plans.PlanMarketingName  
   ,0  
   ,@BundleID
  FROM Plans  
  WHERE PlanID = @PlanID  
  
  DECLARE @OrderSubscriptionID INT  
  SET @OrderSubscriptionID = SCOPE_IDENTITY();  
  
  UPDATE Subscriptions  
  SET STATUS = 2,  
   RefOrderSubscriptionID = @OrderSubscriptionID  
  WHERE SubscriptionID = @SubscriptionID  
  
  INSERT INTO SubscriptionLog  
  (  
   SubscriptionID,  
   Status,  
   Source,  
   UpdatedOn,  
   RefChangeRequestID  
  )  
  SELECT  
   SubscriptionID,  
   2,  
   0,  
   GETDATE(),  
   @ChangeRequestID  
  FROM Subscriptions  
  WHERE SubscriptionID = @SubscriptionID  
  
  SELECT @ChangeRequestID AS ChangeRequestID  
   ,BSSPlanCode  
   ,Plans.PlanMarketingName  
   ,CreatedOn AS 'CurrentDate'  
   ,PlanID  
  FROM Plans  
  WHERE Plans.PlanID = @PlanID  
  
  COMMIT TRAN  
 END TRY  
  
 BEGIN CATCH  
  ROLLBACK TRAN  
 END CATCH  
  
 IF @@ERROR <> 0  
  RETURN 107  
 ELSE  
  RETURN 100  
END  
GO
/****** Object:  StoredProcedure [dbo].[Orders_CR_PostPaymentProcessing]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_CR_PostPaymentProcessing] @CustomerID INT
	,@MobileNumber NVARCHAR(8)
	,@RequestTypeDescription NVARCHAR(50)
	,@Remarks NVARCHAR(50)
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN

		SELECT * FROM ChangeRequests

		COMMIT TRAN
	END TRY

	BEGIN CATCH
		ROLLBACK TRAN
	END CATCH

	IF @@ERROR <> 0
		RETURN 107
	ELSE
		RETURN 100
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_CR_RaiseRequest]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_CR_RaiseRequest] @CustomerID INT  
 ,@MobileNumber NVARCHAR(8)  
 ,@RequestTypeDescription NVARCHAR(50)  
 ,@Remarks NVARCHAR(50)  
AS  
BEGIN  
 BEGIN TRY  
  BEGIN TRAN  
  
  DECLARE @CurrentDate DATE;  
  
  SET @CurrentDate = CAST(GETDATE() AS DATE)  
  
  DECLARE @RequestTypeID INT  
  
  SELECT @RequestTypeID = RequestTypeID  
  FROM RequestTypes  
  WHERE RequestType = @RequestTypeDescription  
  
  DECLARE @AccountID INT  
  DECLARE @SubscriberID INT  
  
  SELECT @AccountID = Accounts.AccountID  
   ,@SubscriberID = SubscriberID  
  FROM Accounts  
  INNER JOIN Subscribers ON Accounts.AccountID = Subscribers.AccountID  
  WHERE CustomerID = @CustomerID  
   AND MobileNumber = @MobileNumber  
  
  IF (@SubscriberID IS NULL)  
  BEGIN  
   ROLLBACK TRAN  
  
   RETURN 102  
  END  
  
  -- Check for Unsuspension state i.e. value is 7  
  IF (@RequestTypeID = 7)  
  BEGIN  
   IF (  
    'Suspended' <>   
     (SELECT TOP 1 State  
     FROM SubscriberStates WHERE SubscriberID = @SubscriberID ORDER BY StateDate DESC)        
     )  
   BEGIN  
    ROLLBACK TRAN  
    RETURN 129  
   END  
  END  
  ELSE IF (  
    'Created' =   
     (SELECT TOP 1 State  
     FROM SubscriberStates WHERE SubscriberID = @SubscriberID ORDER BY StateDate DESC)        
     )  
  BEGIN
	 ROLLBACK TRAN  
    RETURN 139  
  END
  
  IF EXISTS (  
    SELECT *  
    FROM ChangeRequests  
    INNER JOIN SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID  
    WHERE SubscriberID = @SubscriberID  
     AND RequestTypeID = @RequestTypeID  
     AND OrderStatus = 1  
    )  
  BEGIN  
   ROLLBACK TRAN  
  
   RETURN 128  
  END  
  
  INSERT INTO ChangeRequests (  
   RequestTypeID  
   ,RequestReason  
   ,OrderStatus  
   )  
  VALUES (  
   @RequestTypeID  
   ,@Remarks  
   ,1  
   );  
  
  DECLARE @ChangeRequestID INT  
  
  SET @ChangeRequestID = SCOPE_IDENTITY();  
  
  UPDATE ChangeRequests  
  SET OrderNumber = dbo.ufnGetCRNumber()  
  WHERE ChangeRequestID = @ChangeRequestID  
  
  INSERT INTO SubscriberRequests (  
   ChangeRequestID  
   ,SubscriberID  
   )  
  VALUES (  
   @ChangeRequestID  
   ,@SubscriberID  
   )  
  
  IF EXISTS (  
    SELECT *  
    FROM RequestTypes  
    WHERE RequestTypeID = @RequestTypeID  
     AND IsChargable = 1  
    )  
  BEGIN  
   INSERT INTO ChangeRequestCharges (  
    ChangeRequestID  
    ,AdminServiceID  
    ,ServiceFee  
    ,IsGSTIncluded  
    ,IsRecurring  
    ,ReasonType  
    ,ReasonID  
    )  
   SELECT @ChangeRequestID  
    ,AdminServiceID  
    ,ServiceFee  
    ,IsGSTIncluded  
    ,IsRecurring  
    ,@Remarks  
    ,@RequestTypeID  
   FROM AdminServices  
   WHERE ServiceType = @RequestTypeDescription  
  END  
  
  SELECT @ChangeRequestID AS ChangeRequestID  
   ,OrderNumber  
   ,RequestOn  
   ,@RequestTypeDescription AS 'RequestTypeDescription'  
  FROM ChangeRequests  
  WHERE ChangeRequestID = @ChangeRequestID  
  
  SELECT PortalServiceName  
   ,SUM(ChangeRequestCharges.ServiceFee) AS ServiceFee  
  FROM ChangeRequestCharges  
  INNER JOIN AdminServices ON ChangeRequestCharges.AdminServiceID = AdminServices.AdminServiceID  
  GROUP BY PortalServiceName  
   ,ChangeRequestID  
  HAVING ChangeRequestID = @ChangeRequestID  
  
  COMMIT TRAN  
 END TRY  
  
 BEGIN CATCH  
  ROLLBACK TRAN  
 END CATCH  
  
 IF @@ERROR <> 0  
  RETURN 107  
 ELSE  
  RETURN 100  
END  
GO
/****** Object:  StoredProcedure [dbo].[Orders_CR_UpdateCRShippingDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_CR_UpdateCRShippingDetails]  
 @ChangeRequestID INT,  
 @Postcode NVARCHAR(50),  
 @BlockNumber NVARCHAR(50),  
 @Unit NVARCHAR(50) = null,  
 @Floor NVARCHAR(50) = null,  
 @BuildingName NVARCHAR(255) = null,  
 @StreetName NVARCHAR(255),  
 @ContactNumber NVARCHAR(255),  
 @IsBillingSame INT = null,  
 @PortalSlotID NVARCHAR(255),
 @CustomerID INT
AS  
BEGIN  

 IF EXISTS(SELECT * FROM ChangeRequests WHERE ChangeRequestID = @ChangeRequestID)  
 BEGIN   
  DECLARE @DeliveryInformationID INT  
  SELECT @DeliveryInformationID = DeliveryInformationID FROM ChangeRequests WHERE ChangeRequestID = @ChangeRequestID   
    
  DECLARE @DeliveryDate DATE  
  DECLARE @ShippingFee FLOAT  
  SELECT @DeliveryDate= SlotDate, @ShippingFee = ISNULL(AdditionalCharge, 0) FROM DeliverySlots WHERE PortalSlotID = @PortalSlotID  
  
  DECLARE @RequestTypeID INT, @Remarks NVARCHAR(50)
  SELECT @RequestTypeID = RequestTypeID, @Remarks = RequestReason
  FROM ChangeRequests
  WHERE ChangeRequestID = @ChangeRequestID  

  DECLARE @RequestType NVARCHAR(50)
  SELECT @RequestType = RequestType
  FROM RequestTypes
  WHERE RequestTypeID = @RequestTypeID 

  DELETE ChangeRequestCharges   
  FROM ChangeRequestCharges   
  WHERE /*ReasonType = 'Orders - AdditionalDeliveryFee' AND */ ChangeRequestID = @ChangeRequestID  
  
  IF EXISTS (
					SELECT *
					FROM RequestTypes
					WHERE RequestTypeID = @RequestTypeID
						AND IsChargable = 1
					)
			BEGIN
				INSERT INTO ChangeRequestCharges (
					ChangeRequestID
					,AdminServiceID
					,ServiceFee
					,IsGSTIncluded
					,IsRecurring
					,ReasonType
					,ReasonID
					)
				SELECT @ChangeRequestID
					,AdminServiceID
					,ServiceFee
					,IsGSTIncluded
					,IsRecurring
					,@Remarks
					,@RequestTypeID
				FROM AdminServices
				WHERE ServiceType = @RequestType
			END 
  
  IF(@DeliveryInformationID IS NOT NULL) 
  BEGIN  
     
   INSERT INTO DeliveryInformationLog  
   (  
    DeliveryInformationID  
     ,[ShippingNumber]  
     ,[Name]  
     ,[Email]  
     ,[IDNumber]  
     ,[IDType]  
     ,[IsSameAsBilling]  
     ,[ShippingContactNumber]  
     ,[ShippingFloor]  
     ,[ShippingUnit]  
     ,[ShippingBuildingName]  
     ,[ShippingBuildingNumber]  
     ,[ShippingStreetName]  
     ,[ShippingPostCode]  
     ,[AlternateRecipientName]  
     ,[AlternateRecipientEmail]  
     ,[AlternateRecipientContact]  
     ,[AlternateRecioientIDNumber]  
     ,[AlternateRecioientIDType]  
     ,[PortalSlotID]  
     ,[ScheduledDate]  
     ,[DeliveryVendor]  
     ,[DeliveryOn]  
     ,[DeliveryTime]  
     ,[TrackingCode]  
     ,[TrackingUrl]  
     ,[DeliveryFee]  
     ,[VoucherID]  
     ,[LoggedOn]  
   )  
   SELECT  
    DeliveryInformationID  
     ,[ShippingNumber]  
     ,[Name]  
     ,[Email]  
     ,[IDNumber]  
     ,[IDType]  
     ,[IsSameAsBilling]  
     ,[ShippingContactNumber]  
     ,[ShippingFloor]  
     ,[ShippingUnit]  
     ,[ShippingBuildingName]  
     ,[ShippingBuildingNumber]  
     ,[ShippingStreetName]       ,[ShippingPostCode]  
     ,[AlternateRecipientName]  
     ,[AlternateRecipientEmail]  
     ,[AlternateRecipientContact]  
     ,[AlternateRecioientIDNumber]  
     ,[AlternateRecioientIDType]  
     ,[PortalSlotID]  
     ,[ScheduledDate]  
     ,[DeliveryVendor]  
     ,[DeliveryOn]  
     ,[DeliveryTime]  
     ,[VendorTrackingCode]  
     ,[VendorTrackingUrl]  
     ,[DeliveryFee]  
     ,[VoucherID]  
     ,GETDATE()  
   FROM DeliveryInformation   
   WHERE DeliveryInformationID = @DeliveryInformationID  
  
   UPDATE DeliveryInformation SET  
     -- [Name] = NameInNRIC  
     --,[Email] = Customers.Email  
     --,[IDNumber] = IdentityCardNumber  
     --,[IDType] = IdentityCardType  
      [ShippingContactNumber] = @ContactNumber  
     ,[ShippingFloor] = @Floor  
     ,[ShippingUnit] = @Unit  
     ,[ShippingBuildingName] = @BuildingName  
     ,[ShippingBuildingNumber] = @BlockNumber  
     ,[ShippingStreetName] = @StreetName  
     ,[ShippingPostCode] = @Postcode  
     ,[PortalSlotID] = @PortalSlotID  
     ,[ScheduledDate] = @DeliveryDate  
     ,[DeliveryFee] = @ShippingFee  
   FROM DeliveryInformation 
    INNER JOIN ChangeRequests ON ChangeRequests.DeliveryInformationID = DeliveryInformation.DeliveryInformationID
	--INNER JOIN SubscriberRequests ON SubscriberRequests.ChangeRequestID = ChangeRequests.ChangeRequestID
	--INNER JOIN Subscribers ON Subscribers.SubscriberID = SubscriberRequests.SubscriberID
	--INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID
	--INNER JOIN Documents ON Documents.CustomerID = Accounts.CustomerID
	WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID  
  END 
    
  RETURN 101 --Updated success  
 END  
 ELSE  
 BEGIN    
  RETURN 102 --does not exist  
 END  
END  
GO
/****** Object:  StoredProcedure [dbo].[Orders_CR_VerifyDeliveryDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_CR_VerifyDeliveryDetails]
	@ChangeRequestID INT	
AS
BEGIN	
	DECLARE @Margin INT
	SELECT @Margin = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'DeliveryMarginInDays'
	DECLARE @Limit INT =10
	SELECT @Limit = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'DeliveryLimitInDays'

	DECLARE @PortalSlotID NVARCHAR(50);
	DECLARE @SlotAvailablity INT = 0;
	SELECT @PortalSlotID = PortalSlotID FROM DeliveryInformation INNER JOIN ChangeRequests ON ChangeRequests.DeliveryInformationID = DeliveryInformation.DeliveryInformationID WHERE ChangeRequestID = @ChangeRequestID

	SELECT @SlotAvailablity = COUNT(*) FROM DeliverySlots
	WHERE IsActive = 1 
		AND Capacity > UsedQuantity
		AND SlotDate > CAST(DATEADD(d, @Margin, GETDATE()) AS DATE) 
		AND SlotDate <= CAST(DATEADD(d, @Limit, GETDATE()) AS DATE)
		AND GETDATE() < NewOrderCutOffTime
		AND PortalSlotID = @PortalSlotID

	IF(@SlotAvailablity > 0 OR @PortalSlotID IS NULL)
	BEGIN
		RETURN 105
	END
	ELSE
		RETURN 130
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_CreateAccountInvoice]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_CreateAccountInvoice]
	       @AccountID INT,		  
           @InvoiceName NVARCHAR(255)=null,
           @InvoiceUrl NVARCHAR(255),
           @FinalAmount float,
           @Remarks NVARCHAR(255),
           @OrderStatus INT,
           @PaymentSourceID INT,       
           @CreatedBy INT,
		   @BSSBillId NVARCHAR(255)=null	
AS 
BEGIN
	
	INSERT INTO [dbo].[AccountInvoices]
           ([AccountID]
		   ,[BSSBillId]
           ,[InvoiceName]
           ,[InvoiceUrl]
           ,[FinalAmount]
           ,[Remarks]
           ,[OrderStatus]
           ,[PaymentSourceID]          
           ,[CreatedBy])
     VALUES
		 (@AccountID,
		  @BSSBillId,
           @InvoiceName,
           @InvoiceUrl,
           @FinalAmount,
           @Remarks,
           @OrderStatus,
           @PaymentSourceID,         
           @CreatedBy)	

	IF(@@IDENTITY >0)
	begin
	Select @@IDENTITY as OrderID 

	RETURN 100	
	end
	else return 107
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_CreateOrder]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_CreateOrder] --Orders_CreateOrder 30,1, '9H3H7fjC'
	@CustomerID INT,
	@BundleID INT,
	@ReferralCode NVARCHAR(50)=null,
	@PromotionCode NVARCHAR(50)=null
AS
BEGIN
	DECLARE @OrderID INT
	DECLARE @CurrentDate DATE;
	SET @CurrentDate = CAST(GETDATE() AS date)

	IF(@ReferralCode IS NOT NULL)
	BEGIN
		IF EXISTS(SELECT * FROM Customers WHERE ReferralCode = @ReferralCode AND CustomerID = @CustomerID)
		BEGIN 
			SET @ReferralCode = NULL;
		END
	END
	
	DECLARE @Discount FLOAT = 0;
	DECLARE @OrderSubscriberID INT
	IF EXISTS(SELECT OrderID FROM Orders INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID WHERE CustomerID = @CustomerID AND Orders.OrderStatus = 0)
	BEGIN
		SELECT @OrderID = OrderID FROM Orders INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID WHERE CustomerID = @CustomerID AND Orders.OrderStatus = 0

		IF EXISTS(SELECT * FROM [OrderSubscriptions] INNER JOIN 
				ChangeRequests ON [OrderSubscriptions].ChangeRequestID = ChangeRequests.ChangeRequestID INNER JOIN 
				OrderSubscriberChangeRequests ON ChangeRequests.ChangeRequestID = OrderSubscriberChangeRequests.ChangeRequestID INNER JOIN 
				OrderSubscribers ON OrderSubscriberChangeRequests.OrderSubscriberID = OrderSubscribers.OrderSubscriberID
			WHERE OrderID = @OrderID AND BundleID = @BundleID)
		BEGIN
			DELETE sc
			FROM SubscriberCharges sc
				INNER JOIN OrderSubscribers ON sc.OrderSubscriberID=OrderSubscribers.OrderSubscriberID
			WHERE OrderID = @OrderID;

			INSERT INTO SubscriberCharges
			(
				OrderSubscriberID,
				AdminServiceID,
				ServiceFee,
				IsGSTIncluded,
				IsRecurring,
				ReasonType,
				ReasonID
			)
			SELECT OrderSubscriberID,
				AdminServices.AdminServiceID,
				ServiceFee,
				IsGSTIncluded,
				IsRecurring,
				'Orders',
				@OrderID
			FROM PlanAdminServices INNER JOIN 
				AdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID CROSS JOIN
				OrderSubscribers
			WHERE BundleID = @BundleID
				AND OrderID = @OrderID

			IF EXISTS(SELECT * FROM Bundles INNER JOIN 
					BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN 
					Plans ON BundlePlans.PlanID = Plans.PlanID LEFT OUTER JOIN 
					PromotionBundles ON Bundles.BundleID = PromotionBundles.BundleID  LEFT OUTER JOIN 
					Promotions ON  PromotionBundles.PromotionID = Promotions.PromotionID
						AND Promotions.PromotionCode = @PromotionCode
				WHERE Bundles.BundleID = @BundleID)
			BEGIN			
				SELECT @Discount = ISNULL(DiscountValue, 0) FROM PromotionBundles INNER JOIN 
						Promotions ON  PromotionBundles.PromotionID = Promotions.PromotionID
				WHERE PromotionCode = @PromotionCode AND BundleID = @BundleID

				SELECT TOP(1) @OrderSubscriberID =OrderSubscriberID FROM OrderSubscribers WHERE OrderID = @OrderID
				INSERT INTO SubscriberCharges
				(
					OrderSubscriberID,
					AdminServiceID,
					ServiceFee,
					IsGSTIncluded,
					IsRecurring,
					ReasonType,
					ReasonID
				)
				SELECT @OrderSubscriberID,
					AdminServices.AdminServiceID,
					(ServiceFee + @Discount) * -1,
					IsGSTIncluded,
					IsRecurring,
					'Orders - Promo',
					@OrderID
				FROM AdminServices
				WHERE ServiceType = 'Promo' AND @Discount <> 0 
			END

			SELECT @OrderID AS OrderID, 'OldOrder' AS Status
		END
		ELSE
		BEGIN 
			UPDATE Orders SET PromotionCode = @PromotionCode WHERE OrderID = @OrderID

			DELETE sc
			FROM SubscriberCharges sc
				INNER JOIN OrderSubscribers ON sc.OrderSubscriberID=OrderSubscribers.OrderSubscriberID
			WHERE OrderID = @OrderID;

			INSERT INTO SubscriberCharges
			(
				OrderSubscriberID,
				AdminServiceID,
				ServiceFee,
				IsGSTIncluded,
				IsRecurring,
				ReasonType,
				ReasonID
			)
			SELECT OrderSubscriberID,
				AdminServices.AdminServiceID,
				ServiceFee,
				IsGSTIncluded,
				IsRecurring,
				'Orders',
				@OrderID
			FROM PlanAdminServices INNER JOIN 
				AdminServices ON PlanAdminServices.AdminServiceID = AdminServices.AdminServiceID CROSS JOIN
				OrderSubscribers
			WHERE BundleID = @BundleID
				AND OrderID = @OrderID

			IF EXISTS(SELECT * FROM Bundles INNER JOIN 
					BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN 
					Plans ON BundlePlans.PlanID = Plans.PlanID LEFT OUTER JOIN 
					PromotionBundles ON Bundles.BundleID = PromotionBundles.BundleID  LEFT OUTER JOIN 
					Promotions ON  PromotionBundles.PromotionID = Promotions.PromotionID
						AND Promotions.PromotionCode = @PromotionCode
				WHERE Bundles.BundleID = @BundleID)
			BEGIN			
				SELECT @Discount = ISNULL(DiscountValue, 0) FROM PromotionBundles INNER JOIN 
						Promotions ON  PromotionBundles.PromotionID = Promotions.PromotionID
				WHERE PromotionCode = @PromotionCode AND BundleID = @BundleID

				SELECT TOP(1) @OrderSubscriberID =OrderSubscriberID FROM OrderSubscribers WHERE OrderID = @OrderID
				INSERT INTO SubscriberCharges
				(
					OrderSubscriberID,
					AdminServiceID,
					ServiceFee,
					IsGSTIncluded,
					IsRecurring,
					ReasonType,
					ReasonID
				)
				SELECT @OrderSubscriberID,
					AdminServices.AdminServiceID,
					(ServiceFee + @Discount) * -1,
					IsGSTIncluded,
					IsRecurring,
					'Orders - Promo',
					@OrderID
				FROM AdminServices
				WHERE ServiceType = 'Promo' AND @Discount <> 0 
			END

			--Update Bundle Information
			DECLARE @ChangeRequestID INT 
			SELECT @ChangeRequestID = ChangeRequestID 
			FROM OrderSubscriberChangeRequests INNER JOIN 
				OrderSubscribers ON OrderSubscriberChangeRequests.OrderSubscriberID = OrderSubscribers.OrderSubscriberID
			WHERE OrderID = @OrderID

			DELETE FROM OrderSubscriptions WHERE ChangeRequestID = @ChangeRequestID

			INSERT INTO [dbo].[OrderSubscriptions]
				([ChangeRequestID]
				,[PlanID]
				,[PurchasedOn]
				,[BSSPlanCode]
				,[PlanMarketingName]
				,[SubscriptionFee]
				,[ValidFrom]
				,[ValidTo]
				,[Status]
				,[BundleID]
				,[PromotionID])
			SELECT 
				@ChangeRequestID,
				Plans.PlanID,
				GETDATE(),
				BSSPlanCode,
				Plans.PlanMarketingName,
				SubscriptionFee,
				CASE ISNULL(BundlePlans.DurationValue, 0) 
					WHEN 0 THEN NULL
					ELSE @CurrentDate
				END,
				CASE ISNULL(BundlePlans.DurationValue, 0) 
					WHEN 0 THEN NULL
					ELSE DATEADD(month, DurationValue, @CurrentDate) --CASE DurationUOM WHEN 'month' THEN month WHEN 'year' THEN year ELSE day END
				END,
				0,
				@BundleID,
				PromotionBundles.PromotionID
			FROM Bundles INNER JOIN 
				BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN 
				Plans ON BundlePlans.PlanID = Plans.PlanID LEFT OUTER JOIN 
				PromotionBundles ON Bundles.BundleID = PromotionBundles.BundleID LEFT OUTER JOIN 
				Promotions ON  PromotionBundles.PromotionID = Promotions.PromotionID
					AND Promotions.PromotionCode = @PromotionCode
			WHERE Bundles.BundleID = @BundleID		

			SELECT @OrderID AS OrderID, 'OldUpdatedOrder' AS Status
		END
	END
	ELSE
	BEGIN TRY
		BEGIN TRAN
			INSERT INTO [dbo].[Orders]
				([AccountID]
				,[ReferralCode]
				,PromotionCode
				,[OrderDate]
				,[ProcessedOn]
				,[OrderStatus]
				,Nationality
				,Gender
				,NameInNRIC
				,DOB
				,BillingContactNumber
				,BillingFloor
				,BillingUnit
				,BillingBuildingNumber
				,BillingBuildingName
				,BillingStreetName
				,BillingPostCode)
			 SELECT AccountID,
				@ReferralCode,
				@PromotionCode,
				@CurrentDate,
				GETDATE(),
				0,
				Nationality,
				Gender,
				Name,
				DOB,
				BillingContactNumber,
				BillingFloor,
				BillingUnit,
				BillingBuildingNumber,
				BillingBuildingName,
				BillingStreetName,
				BillingPostCode
			FROM Customers INNER JOIN 
				Accounts ON Customers.CustomerID = Accounts.CustomerID
					AND Accounts.IsPrimary = 1
			WHERE Customers.CustomerID = @CustomerID

			SET @OrderID = SCOPE_IDENTITY();

			--Status log	
			INSERT INTO OrderStatusLog
			(
				OrderID,
				OrderStatus,
				UpdatedOn
			)
			VALUES 
			(
				@OrderID,
				0,
				GETDATE()
			)	
			
			IF EXISTS(SELECT * FROM Documents WHERE CustomerID = @CustomerID AND IdentityCardNumber IS NOT NULL)
			BEGIN
				INSERT INTO OrderDocuments
				(
					OrderID,
					DocumentID					
				)
				SELECT 
					@OrderID,
					DocumentID
				FROM
				(
					SELECT DocumentID    
					 FROM 
					 (    
						  SELECT 
								DocumentID     
							   ,ROW_NUMBER() OVER (PARTITION BY CustomerID ORDER BY CreatedOn DESC) AS RNum    
						  FROM Documents 
						  WHERE CustomerID = @CustomerID
							AND IdentityCardNumber IS NOT NULL   
					) A    
					WHERE Rnum = 1    
				) A
			END

			IF EXISTS(SELECT Name FROM Customers WHERE CustomerID = @CustomerID AND Name IS NOT NULL)
			BEGIN
				DECLARE @DeliveryInformationID INT
				INSERT INTO DeliveryInformation
				(
					[ShippingNumber]
				   ,[Name]
				   ,[Email]
				   ,[IDNumber]
				   ,[IDType]
				   ,OrderType
				   ,[ShippingContactNumber]
				   ,[ShippingFloor]
				   ,[ShippingUnit]
				   ,[ShippingBuildingName]
				   ,[ShippingBuildingNumber]
				   ,[ShippingStreetName]
				   ,[ShippingPostCode]
				)
				SELECT
					dbo.ufnGetShippingNumber(),
					Name,
					Customers.Email,
					IdentityCardNumber, 
					IdentityCardType,
					0,
					ShippingContactNumber,
					ShippingFloor,
					ShippingUnit,
					ShippingBuildingName,
					ShippingBuildingNumber,
					ShippingStreetName,
					ShippingPostcode
				FROM Customers INNER JOIN 
					Accounts ON Customers.CustomerID = Accounts.CustomerID LEFT OUTER JOIN 
					(
						SELECT CustomerID, IdentityCardType, IdentityCardNumber
						FROM
						(
							SELECT CustomerID, IdentityCardType, IdentityCardNumber, ROW_NUMBER() OVER(Partition BY CustomerID Order BY CreatedOn DESC) AS RNUM
							FROM Documents
						)A 
						WHERE RNUM = 1
					)CustomerDocuments ON Customers.CustomerID = CustomerDocuments.CustomerID
				WHERE Customers.CustomerID = @CustomerID

				SET @DeliveryInformationID = SCOPE_IDENTITY();

				UPDATE Orders SET DeliveryInformationID = @DeliveryInformationID WHERE OrderID = @OrderID	
			END
		COMMIT TRAN
		SELECT @OrderID AS OrderID, 'NewOrder' AS Status
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN
	END CATCH
	
	select @@ERROR as error
	
	IF @@ERROR<>0

	return 107

	ELSE return 100

END
GO
/****** Object:  StoredProcedure [dbo].[Orders_CreatePendingBuddyList]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_CreatePendingBuddyList]
	@OrderID INT,	
	@OrderSubscriberID INT,	
	@NewMobileNumber NVARCHAR(50),
	@IsProcessed Bit
	
AS 
BEGIN
	
	DECLARE @PendingBuddyID INT

	INSERT INTO PendingBuddyOrders
           (OrderID)
     VALUES
           (@OrderID)

   IF(@@IDENTITY>0)
  
   BEGIN
   
   SET @PendingBuddyID= @@IDENTITY;

   INSERT INTO PendingBuddyOrderList
           ([PendingBuddyID]
           ,[OrderSubscriberID]
           ,[MobileNumber]
           ,[IsProcessed])
     VALUES
           (@PendingBuddyID
           ,@OrderSubscriberID
           ,@NewMobileNumber
           ,@IsProcessed)
	END

	IF(@@IDENTITY >0)

	RETURN 100

	ELSE RETURN  107

END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetAvailableSlots]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_GetAvailableSlots]
AS
BEGIN
	DECLARE @Margin INT
	SELECT @Margin = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'DeliveryMarginInDays'
	DECLARE @Limit INT =10
	SELECT @Limit = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'DeliveryLimitInDays'
	SELECT PortalSlotID,
		SlotDate,
		SlotFromTime,
		SlotToTime,
		CAST(SlotFromTime AS nvarchar(8)) + ' - ' +  CAST(SlotToTime AS nvarchar(8)) AS Slot,
		ISNULL(AdditionalCharge, 0) AS AdditionalCharge
	FROM DeliverySlots
	WHERE IsActive = 1 
		AND Capacity > UsedQuantity
		AND SlotDate > CAST(DATEADD(d, @Margin, GETDATE()) AS DATE) 
		AND SlotDate <= CAST(DATEADD(d, @Limit, GETDATE()) AS DATE)
		AND GETDATE() < NewOrderCutOffTime
	ORDER BY SlotDate, SlotFromTime
	IF @@ROWCOUNT=0
	BEGIN
		RETURN 102	
	END
	ELSE 
		RETURN 105
END


--DECLARE @Margin INT;	SELECT @Margin = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'DeliveryMarginInDays';	DECLARE @Limit INT =10;	SELECT @Margin = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'DeliveryLimitInDays'SELECT CAST(DATEADD(d, @Margin, GETDATE()) AS DATE), CAST(DATEADD(d, @Limit, GETDATE()) AS DATE)
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetChangeCardCheckoutRequestDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_GetChangeCardCheckoutRequestDetails]
	@Source NVARCHAR(50),
	@SourceID INT, 
	@MPGSOrderID NVARCHAR(255), 
	@CheckOutSessionID NVARCHAR(50),
	@SuccessIndicator NVARCHAR(20),
	@CheckoutVersion NVARCHAR(20),
	@TransactionID NVARCHAR(20)
AS
BEGIN	
		INSERT INTO CheckoutRequests 
		(
			SourceType,
			SourceID,
			MPGSOrderID,
			CheckOutSessionID,
			SuccessIndicator,
			CheckoutVersion,TransactionID,
			Amount,
			CreatedOn,
			Status
		)
		VALUES
		(
			@Source,
			@SourceID,
			@MPGSOrderID,
			@CheckOutSessionID,
			@SuccessIndicator,
			@CheckoutVersion,@TransactionID,
			0.01,
			GETDATE(),
			'Created'
		)

   DECLARE @Amount float
   set @Amount=0.01
	SELECT @Amount as Amount FROM CheckoutRequests WHERE SourceID = @SourceID AND SourceType = @Source and MPGSOrderID=@MPGSOrderID

	IF @@ROWCOUNT >0

	RETURN 105

	ELSE RETURN 102
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetCheckoutRequestDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_GetCheckoutRequestDetails]
	@Source NVARCHAR(50),
	@SourceID INT, -- GRID OrderID
	@MPGSOrderID nvarchar(255), -- MPGS OrderID
	@CheckOutSessionID NVARCHAR(50)=null,
	@SuccessIndicator NVARCHAR(20)=null,
	@CheckoutVersion NVARCHAR(20)=null,
	@TransactionID nvarchar(255)
	
AS
BEGIN

declare @IsProcessed Int

IF(@Source = 'Orders')
		SELECT @IsProcessed = IsPaid FROM Orders WHERE OrderID = @SourceID
	ELSE IF(@Source = 'ChangeRequest')
		SELECT @IsProcessed = IsPaid FROM  ChangeRequests WHERE ChangeRequestID = @SourceID
	ELSE IF(@Source = 'AccountInvoices')
		SELECT @IsProcessed = IsPaid FROM  AccountInvoices WHERE InvoiceID = @SourceID

	IF(@IsProcessed  = 0)

	BEGIN
	DECLARE @Amount FLOAT
	SELECT @Amount = SUM(ISNULL(ShipmentFee, 0))
	FROM
	(
		SELECT SUM(ISNULL(OrderCharges.ServiceFee, 0)) AS ShipmentFee
		FROM OrderCharges INNER JOIN 
			AdminServices ON OrderCharges.AdminServiceID = AdminServices.AdminServiceID
		WHERE OrderID = @SourceID AND @Source = 'Orders'

		UNION ALL
				
		SELECT SUM(ISNULL(SubscriberCharges.ServiceFee, 0)) AS ShipmentFee
		FROM SubscriberCharges INNER JOIN 
			OrderSubscribers ON SubscriberCharges.OrderSubscriberID = OrderSubscribers.OrderSubscriberID INNER JOIN
			AdminServices ON SubscriberCharges.AdminServiceID = AdminServices.AdminServiceID
		WHERE OrderID = @SourceID AND @Source = 'Orders'

		UNION ALL

		SELECT SUM(ISNULL(ChangeRequestCharges.ServiceFee, 0)) AS ShipmentFee
		FROM ChangeRequestCharges INNER JOIN 
			AdminServices ON ChangeRequestCharges.AdminServiceID = AdminServices.AdminServiceID
		WHERE ChangeRequestID = @SourceID AND @Source = 'ChangeRequest'

		UNION ALL
				
		SELECT SUM(ISNULL(AccountInvoices.FinalAmount, 0)) AS ShipmentFee
		FROM AccountInvoices 
		WHERE InvoiceID = @SourceID AND @Source = 'AccountInvoices'
	) Fee

	IF EXISTS(SELECT * FROM CheckoutRequests WHERE SourceID = @SourceID AND SourceType = @Source AND MPGSOrderID=@MPGSOrderID)
	BEGIN
		UPDATE CheckoutRequests SET Amount = @Amount, 
									CreatedOn = GETDATE(), 
									CheckOutSessionID=@CheckOutSessionID,
									CheckoutVersion=@CheckoutVersion,
									SuccessIndicator=@SuccessIndicator									
									WHERE SourceID = @SourceID AND SourceType = @Source AND MPGSOrderID=@MPGSOrderID 
	END
	ELSE
	BEGIN

	DECLARE @RecieptNumber NVARCHAR(50)	

	IF @Source = 'AccountInvoices' 
	BEGIN
	DECLARE @AccountID INT

	SELECT @AccountID=AccountID from AccountInvoices WHERE InvoiceID = @SourceID

	SELECT @RecieptNumber = BillingAccountNumber FROM Accounts WHERE AccountID=@AccountID
	END

	ELSE	

	SET @RecieptNumber=(SELECT dbo.ufnGetRecieptNumber())

		INSERT INTO CheckoutRequests 
		(
			SourceType,
			SourceID,
			MPGSOrderID,
			CheckOutSessionID,
			SuccessIndicator,
			CheckoutVersion,TransactionID,
			Amount,
			CreatedOn,
			Status,
			RecieptNumber
		)
		VALUES
		(
			@Source,
			@SourceID,
			@MPGSOrderID,
			@CheckOutSessionID,
			@SuccessIndicator,
			@CheckoutVersion,@TransactionID,
			@Amount,
			GETDATE(),
			'Created',
			@RecieptNumber
		)
	END
	SELECT  ROUND(Amount, 2) AS Amount, RecieptNumber FROM CheckoutRequests WHERE SourceID = @SourceID AND SourceType = @Source and MPGSOrderID=@MPGSOrderID

	IF @@ROWCOUNT >0

	RETURN 105

	ELSE RETURN 102

	END

	ELSE

	RETURN 126
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetCROrOrderDetailsForMessageQueue]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_GetCROrOrderDetailsForMessageQueue] @MPGSOrderID nvarchar(255)  
 
AS  
BEGIN  
 DECLARE @SourceID INT  
 DECLARE @Source NVARCHAR(50)  
  
 SELECT @SourceID = SourceID  
  ,@Source = SourceType  
 FROM CheckoutRequests  
 WHERE MPGSOrderID = @MPGSOrderID  
  
 IF (@Source = 'Orders')  
 BEGIN  
  SELECT 1  
 END  
 ELSE IF (@Source = 'ChangeRequest')  
 BEGIN  
  SELECT RequestTypeID,ChangeRequestID FROM ChangeRequests WHERE OrderStatus = 1 AND  ChangeRequestID = @SourceID  
 END  
 ELSE IF (@Source = 'AccountInvoices')  
 BEGIN  
  SELECT 1  
 END  
END  
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetCustomerDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[Orders_GetCustomerDetails] @OrderID INT,
	@CustomerID INT
AS
BEGIN
	IF EXISTS (
			SELECT *
			FROM Customers
			WHERE CustomerID = @CustomerID
			)
	BEGIN		
		DECLARE @CustomerName NVARCHAR(1000) = NULL
		DECLARE @CustomerEmail NVARCHAR(1000) = NULL
		DECLARE @DeliverInformationEmail NVARCHAR(1000) = NULL
		DECLARE @FinalEmail NVARCHAR(1000) = NULL

			SELECT @CustomerName = Customers.Name,
			@CustomerEmail = Customers.Email
			FROM Customers
			INNER JOIN Accounts ON Accounts.CustomerID = Customers.CustomerID			 
			WHERE Customers.CustomerID = @CustomerID

			SELECT @DeliverInformationEmail = DeliveryInformation.Email
			FROM DeliveryInformation
			INNER JOIN Orders ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID
			INNER JOIN Accounts ON Accounts.AccountID = Orders.AccountID
			INNER JOIN Customers ON Accounts.CustomerID = Customers.CustomerID	
			WHERE Customers.CustomerID = @CustomerID

			IF(@DeliverInformationEmail IS NOT NULL AND  @CustomerEmail IS NOT NULL)
			BEGIN
				IF(LTRIM(RTRIM(@CustomerEmail)) = LTRIM(RTRIM(@DeliverInformationEmail)))
				BEGIN
					SET @FinalEmail = @CustomerEmail
				END
				ELSE
				BEGIN
					SET @FinalEmail = @CustomerEmail + ';' + @DeliverInformationEmail
				END

			END
			ELSE
			BEGIN
				SET @FinalEmail = ISNULL(@CustomerEmail, @DeliverInformationEmail)
			END
				SELECT @CustomerName AS Name, @FinalEmail AS ToEmailList
			RETURN 105 --record exists
		END
		
	ELSE
	BEGIN
		RETURN 119 --does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetCustomerOrderCount]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_GetCustomerOrderCount]    
 @CustomerID int   
   
AS  
BEGIN    
  
  
SELECT count(*) as OrderCount FROM Orders  inner join Accounts on   
  
Orders.AccountID=Accounts.AccountID inner join Customers on Customers.CustomerID=Accounts.CustomerID  
  
WHERE Customers.CustomerID=@CustomerID and Orders.OrderStatus>0  
  
  
IF @@ROWCOUNT >0  
  
 RETURN 105  
  
  
 ELSE RETURN 102  
END  
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetInvoiceMessageQueueBody]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[Orders_GetInvoiceMessageQueueBody] 
	@InvoiceID INT
AS
BEGIN
	
	SELECT Accounts.AccountID AS accountID,
	    Accounts.CustomerID AS  customerID,
		Customers.mobilenumber AS mobilenumber,
		Payments.MaskedCardNumber,
		Payments.Token,
		Payments.CardType, 
		PaymentMethods.IsDefault AS IsDefault,
		Payments.CardHolderName, 
		Payments.ExpiryMonth, 
		Payments.ExpiryYear, 
		Payments.CardFundMethod, 
		Payments.CardBrand, 
		Payments.CardIssue AS CardIssuer,
		Accounts.email AS email,		
		--paymentmode              
		Payments.Amount AS amountPaid,                 
		Payments.MPGSOrderID AS MPGSOrderID,                   
		--invoicelist -- need to take from bss
		AccountInvoices.FinalAmount AS invoiceamounts
 FROM  Accounts INNER JOIN Customers ON Customers.CustomerID=Accounts.CustomerID
				INNER JOIN AccountInvoices ON AccountInvoices.AccountID=Accounts.AccountID
				INNER JOIN Payments ON Payments.PaymentID = AccountInvoices.PaymentID
				LEFT OUTER JOIN PaymentMethods ON PaymentMethods.MaskedCardNumer=Payments.MaskedCardNumber and PaymentMethods.Token=Payments.Token and PaymentMethods.AccountID=Accounts.AccountID
				WHERE AccountInvoices.InvoiceID=@InvoiceID

	IF @@ROWCOUNT>0 

	RETURN 105

	ELSE RETURN 102
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetMessageQueueBody]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[Orders_GetMessageQueueBody] --[Orders_GetMessageQueueBody] 76
	@OrderID INT
AS
BEGIN
	SELECT 
		Accounts.AccountID AS accountID,
		Customers.CustomerID AS customerID,    
		Orders.OrderID AS orderID,    
		Orders.OrderNumber AS orderNumber,    
		Orders.OrderDate AS orderDate,    
		Orders.BillingUnit AS billingUnit,    
		Orders.BillingFloor AS billingFloor,    
		Orders.BillingBuildingNumber AS billingBuildingNumber,    
		Orders.BillingBuildingName AS billingBuildingName,    
		Orders.BillingStreetName AS billingStreetName,    
		Orders.BillingPostCode AS billingPostCode,    
		Orders.BillingContactNumber AS billingContactNumber,    
		Orders.ReferralCode AS orderReferralCode,    
		CASE Orders.Gender WHEN 'Male' THEN 'Mr.' ELSE 'Ms.' END AS title,    
		Orders.NameInNRIC AS name,    
		Customers.Email AS email,    
		Orders.Nationality AS nationality,    
		Documents.IdentityCardType AS idType,    
		Documents.IdentityCardNumber AS idNumber,    
		DeliveryInformation.IsSameAsBilling AS isSameAsBilling,    
		DeliveryInformation.ShippingUnit AS shippingUnit,    
		DeliveryInformation.ShippingFloor AS shippingFloor,    
		DeliveryInformation.ShippingBuildingNumber AS shippingBuildingNumber,    
		DeliveryInformation.ShippingBuildingName AS shippingBuildingName,    
		DeliveryInformation.ShippingStreetName AS shippingStreetName,    
		DeliveryInformation.ShippingPostCode AS shippingPostCode,    
		DeliveryInformation.ShippingContactNumber AS shippingContactNumber,    
		DeliveryInformation.AlternateRecipientContact AS alternateRecipientContact,    
		DeliveryInformation.AlternateRecipientName AS alternateRecipientName,    
		DeliveryInformation.AlternateRecipientEmail AS alternateRecipientEmail,    
		DeliveryInformation.PortalSlotID AS portalSlotID,    
		DeliverySlots.SlotDate AS slotDate,    
		DeliverySlots.SlotFromTime AS slotFromTime,    
		DeliverySlots.SlotToTime AS slotToTime,    
		DeliveryInformation.ScheduledDate AS scheduledDate,    
		Orders.OrderDate AS submissionDate,   
		Charges.serviceFee,    
		Payments.Amount AS amountPaid,
		Payments.CardFundMethod AS paymentMode,
		Payments.MPGSOrderID,
		Payments.MaskedCardNumber,
		Payments.Token,
		Payments.CardType, 
		--Payments.IsDefault,
		Payments.CardHolderName, 
		Payments.ExpiryMonth, 
		Payments.ExpiryYear, 
		Payments.CardFundMethod, 
		Payments.CardBrand, 
		Payments.CardIssue AS CardIssuer, 
		Orders.DOB AS DateofBirth, 
		Customers.ReferralCode, 
		Payments.PaymentOn AS ProcessedOn,
		Orders.InvoiceNumber, 
		Orders.InvoiceUrl,
		Orders.CreatedOn
	FROM Orders 
		INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID 
		INNER JOIN Customers ON Accounts.CustomerID = Customers.CustomerID
		LEFT OUTER JOIN DeliveryInformation ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID
		LEFT OUTER JOIN OrderDocuments ON Orders.OrderID = OrderDocuments.OrderID
		LEFT OUTER JOIN Documents ON OrderDocuments.DocumentID = Documents.DocumentID
		LEFT OUTER JOIN DeliverySlots ON DeliveryInformation.PortalSlotID = DeliverySlots.PortalSlotID
		INNER JOIN Payments ON Orders.PaymentID = Payments.PaymentID 
		LEFT OUTER JOIN 
		(
			SELECT OrderID, SUM(ISNULL(serviceFee, 0)) AS serviceFee
			FROM 
			(
				SELECT 
					OrderCharges.OrderID,
					OrderCharges.ServiceFee AS serviceFee
				FROM OrderCharges INNER JOIN 
					AdminServices ON OrderCharges.AdminServiceID = AdminServices.AdminServiceID

				UNION ALL
	
				SELECT 
					OrderSubscribers.OrderID,
					SubscriberCharges.ServiceFee AS serviceFee
				FROM SubscriberCharges INNER JOIN 
					AdminServices ON SubscriberCharges.AdminServiceID = AdminServices.AdminServiceID INNER JOIN
					OrderSubscribers ON SubscriberCharges.OrderSubscriberID = OrderSubscribers.OrderSubscriberID
			) A
			GROUP BY OrderID
		) Charges ON Orders.OrderID = Charges.OrderID
	WHERE Orders.OrderID = @OrderID

	SELECT 
		OrderSubscribers.OrderID,
	    Subscribers.SubscriberID AS subscriberID,
		Subscribers.MobileNumber AS mobileNumber,
		Subscribers.DisplayName AS displayName,
		Subscribers.IsPrimary AS isPrimaryNumber,
		Subscribers.PremiumType AS premiumType,
		Subscribers.IsPorted AS isPorted,
		OrderSubscribers.IsOwnNumber AS isOwnNumber,
		Subscribers.DonorProviderName AS donorProvider,		
		Subscribers.DepositFee, 
	 	Subscribers.IsBuddyLine AS IsBuddyLine, 
		Subscribers.LinkedSubscriberID, 
		Subscribers.RefOrderSubscriberID, 
		OrderSubscribers.PortedNumberTransferForm AS portedNumberTransferForm,
		OrderSubscribers.PortedNumberOwnedBy AS portedNumberOwnedBy,
		OrderSubscribers.PortedNumberOwnerRegistrationID AS portedNumberOwnerRegistrationID
	FROM Subscribers
		INNER JOIN OrderSubscribers ON Subscribers.RefOrderSubscriberID = OrderSubscribers.OrderSubscriberID
	WHERE OrderSubscribers.OrderID = @OrderID
     
	SELECT 
		Subscribers.SubscriberID,
        OrderSubscriptions.BundleID AS bundleID,
		LTRIM(RTRIM(Plans.BSSPlanCode)) AS bssPlanCode,
		LTRIM(RTRIM(Plans.BSSPlanName)) AS bssPlanName,
		Plans.PlanType AS planType,
		Plans.PlanMarketingName AS planMarketingName,
		Plans.PortalDescription AS portalDescription,
		Plans.Data AS totalData,
		Plans.SMS AS totalSMS,
		Plans.Voice AS totalVoice,
		Plans.SubscriptionFee AS applicableSubscriptionFee
	FROM OrderSubscriptions 
		INNER JOIN ChangeRequests ON OrderSubscriptions.ChangeRequestID = ChangeRequests.ChangeRequestID
		INNER JOIN OrderSubscriberChangeRequests ON ChangeRequests.ChangeRequestID = OrderSubscriberChangeRequests.ChangeRequestID
		INNER JOIN OrderSubscribers ON OrderSubscriberChangeRequests.OrderSubscriberID = OrderSubscribers.OrderSubscriberID
		INNER JOIN Subscribers ON Subscribers.RefOrderSubscriberID = OrderSubscribers.OrderSubscriberID
		INNER JOIN Plans ON OrderSubscriptions.PlanID = Plans.PlanID
	WHERE OrderSubscribers.OrderID = @OrderID

	SELECT * FROM 
	(
		SELECT 
			OrderCharges.OrderID,
			-1 AS SubscriberID,
			AdminServices.PortalServiceName AS portalServiceName,
			OrderCharges.ServiceFee AS serviceFee,
			OrderCharges.IsRecurring AS isRecurring,
			OrderCharges.IsGSTIncluded AS isGSTIncluded
		FROM OrderCharges INNER JOIN 
			AdminServices ON OrderCharges.AdminServiceID = AdminServices.AdminServiceID
		WHERE OrderCharges.OrderID = @OrderID

		UNION ALL
	
		SELECT 
			OrderSubscribers.OrderID,
			Subscribers.SubscriberID,
			AdminServices.PortalServiceName AS portalServiceName,
			SubscriberCharges.ServiceFee AS serviceFee,
			SubscriberCharges.IsRecurring AS isRecurring,
			SubscriberCharges.IsGSTIncluded AS isGSTIncluded
		FROM SubscriberCharges 
			INNER JOIN AdminServices ON SubscriberCharges.AdminServiceID = AdminServices.AdminServiceID 
			INNER JOIN OrderSubscribers ON SubscriberCharges.OrderSubscriberID = OrderSubscribers.OrderSubscriberID
			INNER JOIN Subscribers ON Subscribers.RefOrderSubscriberID = OrderSubscribers.OrderSubscriberID
		WHERE OrderSubscribers.OrderID = @OrderID
	)Charges

	IF @@ROWCOUNT>0 

	RETURN 105

	ELSE RETURN 102
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetOrderShipmentDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--exec [Orders_GetOrderShipmentDetails] '2019-04-26'
CREATE PROCEDURE [dbo].[Orders_GetOrderShipmentDetails]
	@ScheduledDate date
AS
BEGIN
	SELECT 
		Orders.DeliveryInformationID
		,ShippingNumber
		,Name
		,Email
		,IDNumber
		,IDType
		,ShippingContactNumber
		,ShippingFloor
		,ShippingUnit
		,ShippingBuildingName
		,ShippingBuildingNumber
		,ShippingStreetName
		,ShippingPostCode
		,AlternateRecipientName
		,AlternateRecipientEmail
		,AlternateRecipientContact
		,PortalSlotID
		,ScheduledDate
		,OrderNumber AS OrderNo
		,Orders.OrderID
		,OrderStatus
		,OrderType --(0=New/1=Replacement/2=Redelivery)
		,CASE ISNULL(ShipmentFee, 0) WHEN 0 THEN 'true' ELSE 'false' END AS IsFreeDelivery
		,SIMQuantity
		,MobileNumbers
	FROM Orders INNER JOIN 
		DeliveryInformation ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID INNER JOIN 
		(
			SELECT OrderID, COUNT(*) AS SIMQuantity, STUFF(
					(SELECT ',' + MobileNumber 
					FROM OrderSubscribers OS1
					WHERE OS1.OrderID = OrderSubscribers.OrderID
					FOR XML PATH (''))
					, 1, 1, '') AS MobileNumbers
			FROM OrderSubscribers
			GROUP BY OrderID
		)NewSubscribers ON Orders.OrderID = NewSubscribers.OrderID LEFT OUTER JOIN 
		(
			SELECT OrderID, SUM(ISNULL(ShipmentFee, 0)) AS ShipmentFee
			FROM
			(
				SELECT OrderID, SUM(ISNULL(OrderCharges.ServiceFee, 0)) AS ShipmentFee
				FROM OrderCharges INNER JOIN 
					AdminServices ON OrderCharges.AdminServiceID = AdminServices.AdminServiceID
				WHERE ServiceType = 'Delivery'
				GROUP BY OrderID

				UNION ALL
				
				SELECT OrderID, SUM(ISNULL(SubscriberCharges.ServiceFee, 0)) AS ShipmentFee
				FROM SubscriberCharges INNER JOIN 
					OrderSubscribers ON SubscriberCharges.OrderSubscriberID = OrderSubscribers.OrderSubscriberID INNER JOIN
					AdminServices ON SubscriberCharges.AdminServiceID = AdminServices.AdminServiceID
				WHERE ServiceType = 'Delivery'
				GROUP BY OrderID
			) Fee 
			GROUP BY OrderID
		)ShipFee ON Orders.OrderID = ShipFee.OrderID LEFT OUTER JOIN 
		(
			SELECT OrderID, COUNT(DeliveryInformationLog.DeliveryInformationID) AS RescheduleCount
			FROM Orders INNER JOIN
				DeliveryInformation ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID LEFT OUTER JOIN
				DeliveryInformationLog ON DeliveryInformation.DeliveryInformationID = DeliveryInformationLog.DeliveryInformationID
					AND RescheduleReasonID IS NOT NULL
			GROUP BY OrderID				
		)ShipmentSchedule ON Orders.OrderID = ShipmentSchedule.OrderID
	WHERE ScheduledDate=@ScheduledDate and OrderStatus=1

	UNION ALL

	SELECT 
		ChangeRequests.DeliveryInformationID
		,ShippingNumber
		,Name
		,Email
		,IDNumber
		,IDType
		,ShippingContactNumber
		,ShippingFloor
		,ShippingUnit
		,ShippingBuildingName
		,ShippingBuildingNumber
		,ShippingStreetName
		,ShippingPostCode
		,AlternateRecipientName
		,AlternateRecipientEmail
		,AlternateRecipientContact
		,PortalSlotID
		,ScheduledDate
		,OrderNumber AS OrderNo
		,ChangeRequests.ChangeRequestID AS OrderID
		,OrderStatus
		,OrderType --(0=New/1=Replacement/2=Redelivery)
		,CASE ISNULL(ShipmentFee, 0) WHEN 0 THEN 'true' ELSE 'false' END AS IsFreeDelivery
		,1 AS SIMQuantity
		,Subscribers.MobileNumber AS MobileNumbers
	FROM ChangeRequests INNER JOIN 
		DeliveryInformation ON ChangeRequests.DeliveryInformationID = DeliveryInformation.DeliveryInformationID INNER JOIN 
		SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID INNER JOIN 
		Subscribers ON Subscribers.SubscriberID = SubscriberRequests.SubscriberID LEFT OUTER JOIN 
		(
			SELECT ChangeRequestID, SUM(ISNULL(ShipmentFee, 0)) AS ShipmentFee
			FROM
			(
				SELECT ChangeRequestID, SUM(ISNULL(ChangeRequestCharges.ServiceFee, 0)) AS ShipmentFee
				FROM ChangeRequestCharges INNER JOIN 
					AdminServices ON ChangeRequestCharges.AdminServiceID = AdminServices.AdminServiceID
				WHERE ServiceType = 'Delivery'
				GROUP BY ChangeRequestID
			) Fee 
			GROUP BY ChangeRequestID
		)ShipFee ON ChangeRequests.ChangeRequestID = ShipFee.ChangeRequestID LEFT OUTER JOIN 
		(
			SELECT ChangeRequestID, COUNT(DeliveryInformationLog.DeliveryInformationID) AS RescheduleCount
			FROM ChangeRequests INNER JOIN
				DeliveryInformation ON ChangeRequests.DeliveryInformationID = DeliveryInformation.DeliveryInformationID LEFT OUTER JOIN
				DeliveryInformationLog ON DeliveryInformation.DeliveryInformationID = DeliveryInformationLog.DeliveryInformationID
					AND RescheduleReasonID IS NOT NULL
			GROUP BY ChangeRequestID				
		)ShipmentSchedule ON ChangeRequests.ChangeRequestID = ShipmentSchedule.ChangeRequestID
	WHERE ScheduledDate=@ScheduledDate and OrderStatus=1
	
	INSERT INTO OrderStatusLog
	(
		OrderID,
		OrderStatus,
		UpdatedOn
	)
	SELECT 
		OrderID,
		2,
		GETDATE()
	FROM Orders
		 INNER JOIN DeliveryInformation ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID 
	WHERE ScheduledDate=@ScheduledDate and OrderStatus=1

	UPDATE Orders SET OrderStatus = 2 FROM Orders INNER JOIN DeliveryInformation ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID 
	WHERE ScheduledDate=@ScheduledDate and OrderStatus=1

	UPDATE ChangeRequests SET OrderStatus = 2 FROM ChangeRequests INNER JOIN DeliveryInformation ON ChangeRequests.DeliveryInformationID = DeliveryInformation.DeliveryInformationID 
	WHERE ScheduledDate=@ScheduledDate and OrderStatus=1
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetOrderShipmentDetails_test1]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [dbo].[Orders_GetOrderShipmentDetails_test1]
	--@ScheduledDate date
AS
BEGIN
	--UPDATE Orders SET OrderStatus = 3 WHERE OrderID = @OrderID

	--UPDATE Orders  SET OrderStatus = 2
	--FROM Orders a,DeliveryInformation b
	--where a.DeliveryInformationID=b.DeliveryInformationID
	--and b.ScheduledDate=@ScheduledDate and a.OrderStatus=1

	SELECT 
		Orders.DeliveryInformationID
		,ShippingNumber
		,Name
		,Email
		,IDNumber
		,IDType
		,ShippingContactNumber
		,ShippingFloor
		,ShippingUnit
		,ShippingBuildingName
		,ShippingBuildingNumber
		,ShippingStreetName
		,ShippingPostCode
		,AlternateRecipientName
		,AlternateRecipientEmail
		,AlternateRecipientContact
		,PortalSlotID
		,ScheduledDate
		,OrderNumber AS OrderNo
		,Orders.OrderID
		,OrderStatus
		--,RelatedOrderNo --OrderNumber will be same as initial order if its redelivery and replacement is a change request so separate order number no link
		,CASE WHEN RescheduleCount > 0 THEN 'Redelivery' ELSE 'New' END AS Ordertype --(New/Replacement/Redelivery)
		,CASE ISNULL(ShipmentFee, 0) WHEN 0 THEN 'true' ELSE 'false' END AS IsFreeDelivery
		,SIMQuantity
		,MobileNumbers
	FROM Orders INNER JOIN 
			DeliveryInformation ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID INNER JOIN 
			(
				SELECT OrderID, COUNT(*) AS SIMQuantity, STUFF(
					 (SELECT ',' + MobileNumber 
					  FROM OrderSubscribers OS1
					  WHERE OS1.OrderID = OrderSubscribers.OrderID
					  FOR XML PATH (''))
					 , 1, 1, '') AS MobileNumbers
				FROM OrderSubscribers
				GROUP BY OrderID
			)NewSubscribers ON Orders.OrderID = NewSubscribers.OrderID LEFT OUTER JOIN 
			(
				SELECT OrderID, SUM(ISNULL(ShipmentFee, 0)) AS ShipmentFee
				FROM
				(
					SELECT OrderID, SUM(ISNULL(OrderCharges.ServiceFee, 0)) AS ShipmentFee
					FROM OrderCharges INNER JOIN 
						AdminServices ON OrderCharges.AdminServiceID = AdminServices.AdminServiceID
					WHERE ServiceType = 'Delivery'
					GROUP BY OrderID

					UNION ALL
				
					SELECT OrderID, SUM(ISNULL(SubscriberCharges.ServiceFee, 0)) AS ShipmentFee
					FROM SubscriberCharges INNER JOIN 
						OrderSubscribers ON SubscriberCharges.OrderSubscriberID = OrderSubscribers.OrderSubscriberID INNER JOIN
						AdminServices ON SubscriberCharges.AdminServiceID = AdminServices.AdminServiceID
					WHERE ServiceType = 'Delivery'
					GROUP BY OrderID
				) Fee 
				GROUP BY OrderID
			)ShipFee ON Orders.OrderID = ShipFee.OrderID LEFT OUTER JOIN 
			(
				SELECT OrderID, COUNT(DeliveryInformationLog.DeliveryInformationID) AS RescheduleCount
				FROM Orders INNER JOIN
					DeliveryInformation ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID LEFT OUTER JOIN
					DeliveryInformationLog ON DeliveryInformation.DeliveryInformationID = DeliveryInformationLog.DeliveryInformationID
						AND RescheduleReasonID IS NOT NULL
				GROUP BY OrderID				
			)ShipmentSchedule ON Orders.OrderID = ShipmentSchedule.OrderID
		--	where ScheduledDate=@ScheduledDate
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetOrderShipmentDetailsbkp]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_GetOrderShipmentDetailsbkp]
	@OrderID INT
AS
BEGIN
	UPDATE Orders SET OrderStatus = 3 WHERE OrderID = @OrderID

	SELECT 
		Orders.DeliveryInformationID
		,Name
		,Email
		,IDNumber
		,IDType
		,ShippingContactNumber
		,ShippingFloor
		,ShippingUnit
		,ShippingBuildingName
		,ShippingBuildingNumber
		,ShippingStreetName
		,ShippingPostCode
		,AlternateRecipientName
		,AlternateRecipientEmail
		,AlternateRecipientContact
		,PortalSlotID
		,ScheduledDate
		,OrderNumber AS OrderNo
		,Orders.OrderID
		,OrderStatus
		--,RelatedOrderNo --OrderNumber will be same as initial order if its redelivery and replacement is a change request so separate order number no link
		,CASE WHEN RescheduleCount > 0 THEN 'Redelivery' ELSE 'New' END AS Ordertype --(New/Replacement/Redelivery)
		,CASE ISNULL(ShipmentFee, 0) WHEN 0 THEN 'true' ELSE 'false' END AS IsFreeDelivery
		,SIMQuantity
		,MobileNumbers
	FROM Orders INNER JOIN 
			DeliveryInformation ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID INNER JOIN 
			(
				SELECT OrderID, COUNT(*) AS SIMQuantity, STUFF(
					 (SELECT ',' + MobileNumber 
					  FROM OrderSubscribers OS1
					  WHERE OS1.OrderID = OrderSubscribers.OrderID
					  FOR XML PATH (''))
					 , 1, 1, '') AS MobileNumbers
				FROM OrderSubscribers
				GROUP BY OrderID
			)NewSubscribers ON Orders.OrderID = NewSubscribers.OrderID LEFT OUTER JOIN 
			(
				SELECT OrderID, SUM(ISNULL(ShipmentFee, 0)) AS ShipmentFee
				FROM
				(
					SELECT OrderID, SUM(ISNULL(OrderCharges.ServiceFee, 0)) AS ShipmentFee
					FROM OrderCharges INNER JOIN 
						AdminServices ON OrderCharges.AdminServiceID = AdminServices.AdminServiceID
					WHERE ServiceType = 'Delivery'
					GROUP BY OrderID

					UNION ALL
				
					SELECT OrderID, SUM(ISNULL(SubscriberCharges.ServiceFee, 0)) AS ShipmentFee
					FROM SubscriberCharges INNER JOIN 
						OrderSubscribers ON SubscriberCharges.OrderSubscriberID = OrderSubscribers.OrderSubscriberID INNER JOIN
						AdminServices ON SubscriberCharges.AdminServiceID = AdminServices.AdminServiceID
					WHERE ServiceType = 'Delivery'
					GROUP BY OrderID
				) Fee 
				GROUP BY OrderID
			)ShipFee ON Orders.OrderID = ShipFee.OrderID LEFT OUTER JOIN 
			(
				SELECT OrderID, COUNT(DeliveryInformationLog.DeliveryInformationID) AS RescheduleCount
				FROM Orders INNER JOIN
					DeliveryInformation ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID LEFT OUTER JOIN
					DeliveryInformationLog ON DeliveryInformation.DeliveryInformationID = DeliveryInformationLog.DeliveryInformationID
						AND RescheduleReasonID IS NOT NULL
				GROUP BY OrderID				
			)ShipmentSchedule ON Orders.OrderID = ShipmentSchedule.OrderID
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetOrderSubscribers]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_GetOrderSubscribers]
	@OrderID INT
AS
BEGIN	
	DECLARE @AccountID INT
	SELECT @AccountID = AccountID FROM Orders WHERE OrderID = @OrderID

	IF EXISTS(SELECT * FROM Orders WHERE OrderID = @OrderID)
	BEGIN
		SELECT MobileNumber, SUM(IsDefault) AS IsDefault FROM 
		(
			SELECT MobileNumber, 0 AS IsDefault FROM OrderSubscribers
			WHERE OrderID = @OrderID
			UNION ALL 
			SELECT Subscribers.MobileNumber, CASE WHEN Subscribers.MobileNumber = Customers.MobileNumber THEN 1 ELSE 0 END AS IsDefault FROM Subscribers 
				INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID
				INNER JOIN Customers ON Accounts.CustomerID = Customers.CustomerID
				LEFT OUTER JOIN
				(
					SELECT SubscriberID, State 
					FROM 
					(
						SELECT SubscriberID
							,State, ROW_NUMBER() OVER(PARTITION BY SubscriberID ORDER BY StateDate DESC) AS RNum
						FROM SubscriberStates
					) A 
					WHERE RNum = 1
				) SubscriberStatus ON SubscriberStatus.SubscriberID = Subscribers.SubscriberID
			WHERE Accounts.AccountID = @AccountID 
				AND SubscriberStatus.State <> 'Terminated'
		) A 
		GROUP BY MobileNumber
		RETURN 105 --record exists
	END
	ELSE
	BEGIN		
		RETURN 109 --does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetPendingOrderDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_GetPendingOrderDetails]
	@CustomerID INT
AS
BEGIN	
	IF EXISTS(SELECT * FROM Customers WHERE CustomerID = @CustomerID)
	BEGIN
		IF EXISTS(SELECT OrderID, OrderNumber FROM Orders INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID WHERE CustomerID = @CustomerID AND OrderStatus = 0)
		BEGIN
			SELECT OrderID, OrderNumber, OrderDate FROM Orders INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID WHERE CustomerID = @CustomerID AND OrderStatus = 0
			RETURN 105 --record exists
		END
		ELSE
		BEGIN
			RETURN 119 --does not exist
		END
	END
	ELSE
	BEGIN		
		RETURN 119 --does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetSourceTypeByMpgsOrderId]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_GetSourceTypeByMpgsOrderId]		
	@MPGSOrderID nvarchar(255) -- MPGS OrderID
	
AS
BEGIN		

IF EXISTS(SELECT * FROM  CheckoutRequests WHERE MPGSOrderID=@MPGSOrderID)
BEGIN
	SELECT SourceType, SourceID FROM CheckoutRequests WHERE  MPGSOrderID=@MPGSOrderID

	IF @@ROWCOUNT >0

	RETURN 105

END
	ELSE RETURN 102
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetTokenizationCheckoutRequestDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_GetTokenizationCheckoutRequestDetails]
	@Source NVARCHAR(50),
	@SourceID INT, -- GRID OrderID
	@MPGSOrderID nvarchar(255), -- MPGS OrderID
	@CheckOutSessionID NVARCHAR(50)=null,
	@SuccessIndicator NVARCHAR(20)=null,
	@CheckoutVersion NVARCHAR(20)=null,
	@TransactionID nvarchar(255)
AS
BEGIN	
	DECLARE @Amount FLOAT
	SELECT @Amount = SUM(ISNULL(ShipmentFee, 0))
	FROM
	(
		SELECT SUM(ISNULL(OrderCharges.ServiceFee, 0)) AS ShipmentFee
		FROM OrderCharges INNER JOIN 
			AdminServices ON OrderCharges.AdminServiceID = AdminServices.AdminServiceID
		WHERE OrderID = @SourceID AND @Source = 'Orders'

		UNION ALL
				
		SELECT SUM(ISNULL(SubscriberCharges.ServiceFee, 0)) AS ShipmentFee
		FROM SubscriberCharges INNER JOIN 
			OrderSubscribers ON SubscriberCharges.OrderSubscriberID = OrderSubscribers.OrderSubscriberID INNER JOIN
			AdminServices ON SubscriberCharges.AdminServiceID = AdminServices.AdminServiceID
		WHERE OrderID = @SourceID AND @Source = 'Orders'

		UNION ALL

		SELECT SUM(ISNULL(ChangeRequestCharges.ServiceFee, 0)) AS ShipmentFee
		FROM ChangeRequestCharges INNER JOIN 
			AdminServices ON ChangeRequestCharges.AdminServiceID = AdminServices.AdminServiceID
		WHERE ChangeRequestID = @SourceID AND @Source = 'ChangeRequest'

		UNION ALL
				
		SELECT SUM(ISNULL(AccountInvoices.FinalAmount, 0)) AS ShipmentFee
		FROM AccountInvoices 
		WHERE InvoiceID = @SourceID AND @Source = 'AccountInvoices'
	) Fee

	IF EXISTS(SELECT * FROM CheckoutRequests WHERE SourceID = @SourceID AND SourceType = @Source AND MPGSOrderID=@MPGSOrderID)
	BEGIN
		UPDATE CheckoutRequests SET Amount = @Amount, CreatedOn = GETDATE() WHERE SourceID = @SourceID AND SourceType = @Source 
		and MPGSOrderID=@MPGSOrderID
	END
	ELSE
	BEGIN
		INSERT INTO CheckoutRequests 
		(
			SourceType,
			SourceID,
			MPGSOrderID,
			CheckOutSessionID,
			SuccessIndicator,
			CheckoutVersion,
			TransactionID,
			Amount,
			CreatedOn,
			[Status]
		)
		VALUES
		(
			@Source,
			@SourceID,
			@MPGSOrderID,
			@CheckOutSessionID,
			@SuccessIndicator,
			@CheckoutVersion,
			@TransactionID,
			@Amount,
			GETDATE(),
			'Created'
		)
	END
	SELECT Amount FROM CheckoutRequests WHERE SourceID = @SourceID AND SourceType = @Source and MPGSOrderID=@MPGSOrderID 

	IF @@ROWCOUNT >0

	RETURN 105

	ELSE RETURN 102
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_IsAccountInvoicePaid]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create PROCEDURE [dbo].[Orders_IsAccountInvoicePaid] 
	@BSSBillId NVARCHAR(255)	
AS
BEGIN
	
IF EXISTS (SELECT * FROM AccountInvoices WHERE BSSBillId=@BSSBillId)

BEGIN
 
 SELECT IsPaid FROM AccountInvoices WHERE BSSBillId=@BSSBillId

 return 105

 END

 ELSE

 RETURN 102

END
GO
/****** Object:  StoredProcedure [dbo].[Orders_IsPaymentProcessed]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_IsPaymentProcessed] 
	@MPGSOrderID NVARCHAR(255)	
AS
BEGIN
	
	DECLARE @IsProcessed int

	SELECT @IsProcessed=IsProcessed
	from CheckoutRequests WHERE MPGSOrderID = @MPGSOrderID		
	
	IF @IsProcessed=1

	return 126

	else return 137

END
GO
/****** Object:  StoredProcedure [dbo].[Orders_ProcessBuddyPlan]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_ProcessBuddyPlan] -- [Orders_ProcessBuddyPlan] 320, 'GR00000dfdfdf00003469',  '114001954'
	@OrderSubscriberID INT,
	@UserId NVARCHAR(50),
	@NewMobileNumber NVARCHAR(50)
AS 
BEGIN
	DECLARE @CurrentDate DATE;
	SET @CurrentDate = CAST(GETDATE() AS date)
	IF EXISTS(SELECT * FROM OrderSubscribers WHERE OrderSubscriberID = @OrderSubscriberID)
	BEGIN
		DECLARE @OrderID INT;
		SELECT @OrderID = OrderID FROM OrderSubscribers WHERE OrderSubscriberID = @OrderSubscriberID
		--PRINT @OrderID;
		DECLARE @SubscriberID INT
		DECLARE @AccountID INT
		SELECT @SubscriberID = SubscriberID, @AccountID = AccountID FROM Subscribers WHERE RefOrderSubscriberID = @OrderSubscriberID
		IF EXISTS(SELECT * FROM Subscribers WHERE SubscriberID =@SubscriberID AND LinkedSubscriberID IS NULL)
		BEGIN		
			BEGIN TRY
				BEGIN TRAN
					--Order Processing
					INSERT INTO OrderSubscribers
					(
						OrderID,
						MobileNumber,
						UserSessionID,
						IsPorted,
						IsOwnNumber,
						IsPrimaryNumber,
						PremiumType,
						LinkedOrderSubscriberID,
						IsBuddyLine
					)
					VALUES 
					(
						@OrderID,
						@NewMobileNumber,
						@UserId,
						0,
						1,
						0,
						39,
						@OrderSubscriberID,
						1
					)

					DECLARE @NewOrderSubscriberID INT
					SET @NewOrderSubscriberID = SCOPE_IDENTITY();

					UPDATE OrderSubscribers SET BuddyProcessedOn = GETDATE(), LinkedOrderSubscriberID = @NewOrderSubscriberID WHERE OrderSubscriberID = @OrderSubscriberID 
					--No Charges for buddy
					DECLARE @BuddyBundleID INT
					SELECT @BuddyBundleID = Bundles.BundleID FROM Bundles 
					WHERE Bundles.IsBuddyBundle = 1 AND Status = 1
						AND ISNULL(ValidFrom, @CurrentDate) <= @CurrentDate
						AND ISNULL(ValidTo, @CurrentDate) >= @CurrentDate

	
					--create default CR
					INSERT INTO [dbo].[ChangeRequests]
							([OrderNumber]
							,[RequestTypeID]
							,[RequestOn]
							,[OrderStatus]
							,[RequestReason])
					VALUES 
					(
						'GRID-' + CAST(@NewOrderSubscriberID AS nvarchar(50)) + CONVERT(NVARCHAR(10), @CurrentDate, 12),
						17,
						GETDATE(),
						0,
						'Created From Order Buddy'
					)
					DECLARE @CRID INT
					SET @CRID = SCOPE_IDENTITY();
				
					INSERT INTO OrderSubscriberChangeRequests
					(
						OrderSubscriberID,
						ChangeRequestID
					)
					VALUES 
					(
						@NewOrderSubscriberID,
						@CRID
					)
					--insert subscriptions
					INSERT INTO [dbo].[OrderSubscriptions]
						([ChangeRequestID]
						,[PlanID]
						,[PurchasedOn]
						,[BSSPlanCode]
						,[PlanMarketingName]
						,[SubscriptionFee]
						,[ValidFrom]
						,[ValidTo]
						,[Status]
						,[BundleID])
					SELECT
						@CRID,
						Plans.PlanID,
						GETDATE(),
						BSSPlanCode,
						Plans.PlanMarketingName,
						SubscriptionFee,
						CASE ISNULL(BundlePlans.DurationValue, 0) 
							WHEN 0 THEN NULL
							ELSE @CurrentDate
						END,
						CASE ISNULL(BundlePlans.DurationValue, 0) 
							WHEN 0 THEN NULL
							ELSE DATEADD(month, DurationValue, @CurrentDate) --CASE DurationUOM WHEN 'month' THEN month WHEN 'year' THEN year ELSE day END
						END,
						0,
						Bundles.BundleID
					FROM Bundles INNER JOIN 
						BundlePlans ON Bundles.BundleID = BundlePlans.BundleID INNER JOIN 
						Plans ON BundlePlans.PlanID = Plans.PlanID
					WHERE Bundles.BundleID = @BuddyBundleID	
					--Order Processing	
				
					--Final Processing
					INSERT INTO Subscribers
					(
						[AccountID]
						,[MobileNumber]
						,[SIMID]
						,[PremiumType]
						,[DisplayName]
						,[IsPrimary]
						,[IsPorted]
						,[DonorProviderName]
						,[SMSSubscription]
						,[DepositFee]
						,[IsBuddyLine]
						,RefOrderSubscriberID
						,LinkedSubscriberID
					)
					SELECT @AccountID
						,[MobileNumber]
						,[SIMID]
						,[PremiumType]
						,[DisplayName]
						,[IsPrimaryNumber]
						,[IsPorted]
						,[DonorProvider]
						,1
						,[DepositFee]
						,[IsBuddyLine]
						,OrderSubscriberID
						,@SubscriberID
					FROM OrderSubscribers
					WHERE OrderSubscriberID = @NewOrderSubscriberID

					DECLARE @BuddySubscriberID INT
					SET @BuddySubscriberID = SCOPE_IDENTITY();
				
					UPDATE Subscribers SET LinkedSubscriberID = @BuddySubscriberID WHERE SubscriberID = @SubscriberID
		
					INSERT INTO SubscriberStates
						 ([SubscriberID]
						   ,[State]
						   ,[StateDate]
						   ,[StateSource])
					VALUES
					(
						@BuddySubscriberID,
						'Created',
						GETDATE(),
						0
					)

					INSERT INTO [dbo].[Subscriptions]
						([SubscriberID]
						,[PlanID]
						,[PurchasedOn]
						,[BSSPlanCode]
						,[PlanMarketingName]
						,[SubscriptionFee]
						,[ValidFrom]
						,[ValidTo]
						,[Status]
						,[BundleID]
						,IsRemovable)
					SELECT 
						@BuddySubscriberID
						,[PlanID]
						,[PurchasedOn]
						,[BSSPlanCode]
						,PlanMarketingName
						,[SubscriptionFee]
						,[ValidFrom]
						,[ValidTo]
						,0
						,[BundleID],
						0
					FROM OrderSubscriberChangeRequests INNER JOIN
						ChangeRequests ON OrderSubscriberChangeRequests.ChangeRequestID = ChangeRequests.ChangeRequestID INNER JOIN
						OrderSubscriptions ON ChangeRequests.ChangeRequestID = OrderSubscriptions.ChangeRequestID
					WHERE OrderSubscriberID = @NewOrderSubscriberID
				COMMIT TRAN
			END TRY
			BEGIN CATCH
				ROLLBACK TRAN
			END CATCH
	
			IF @@ERROR<>0

		RETURN 107

		ELSE RETURN 100
		END
		ELSE
			RETURN 136 --Buddy already assigned
	END
	ELSE
		RETURN 119
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_ProcessPayment]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_ProcessPayment] 
--[Orders_ProcessPayment] '62563ac347', '011A7Z', NULL, NULL, 40, '123123XXXX839247239', 'CREDIT', 'MASTERCARD', NULL, 'MASTERCARD', 'Stratagile', 2021, 12, 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjM4IiwibmJmIjoxNTU0MTg5MzE2LCJleHAiOjE1NTQ3OTQxMTYsImlhdCI6MTU1NDE4OTMxNn0.YyfU-7B_raIqEmFyVizpACUS-ORmnGkX3szBvsHPDgk', 'CAPTURED', 'SUCCESS', 'APPROVED', '0:0:0:1', 1
	@MPGSOrderID nvarchar(255),
	@TransactionID nvarchar(255)=null,
	@PaymentRequest NVARCHAR(MAX)=NULL,
    @PaymentResponse NVARCHAR(MAX)=NULL,
    @Amount FLOAT,
    @MaskedCardNumber NVARCHAR(255),
    @CardFundMethod NVARCHAR(255),
    @CardBrand NVARCHAR(255),
    @CardType NVARCHAR(255)=NULL,
    @CardIssuer NVARCHAR(255),
    @CardHolderName NVARCHAR(255)=null,
	@ExpiryYear INT,
	@ExpiryMonth INT,
    @Token NVARCHAR(255)=null,
    @PaymentStatus NVARCHAR(255),
	@ApiResult NVARCHAR(20),
	@GatewayCode NVARCHAR(20),
	@CustomerIP NVARCHAR(255)=null,
	@PaymentMethodSubscription INT
AS
BEGIN
	DECLARE @AccountID INT
	DECLARE @OrderID INT
	DECLARE @Source NVARCHAR(50)
	DECLARE @IsProcessed INT
	
	SELECT @OrderID = SourceId, @Source = SourceType
	from CheckoutRequests WHERE MPGSOrderID = @MPGSOrderID		

	IF(@Source = 'Orders')
		SELECT @IsProcessed = IsPaid FROM Orders WHERE OrderID = @OrderID
	ELSE IF(@Source = 'ChangeRequest')
		SELECT @IsProcessed = IsPaid FROM  ChangeRequests WHERE ChangeRequestID = @OrderID
	ELSE IF(@Source = 'AccountInvoices')
		SELECT @IsProcessed = IsPaid FROM  AccountInvoices WHERE InvoiceID = @OrderID

	IF(@IsProcessed  = 0)
	BEGIN
		UPDATE CheckoutRequests SET IsProcessed = 1 WHERE MPGSOrderID = @MPGSOrderID	

		IF(@Source = 'Orders')
			UPDATE Orders SET IsPaid = 1 WHERE OrderID = @OrderID
		ELSE IF(@Source = 'ChangeRequest')
			UPDATE ChangeRequests SET IsPaid = 1 WHERE ChangeRequestID = @OrderID
		ELSE IF(@Source = 'AccountInvoices')
			UPDATE AccountInvoices SET IsPaid = 1 WHERE InvoiceID = @OrderID

		DECLARE @PaymentID INT
		-- revice check against duplicate
		INSERT INTO [dbo].[Payments]
			([PaymentRequest]
			,[PaymentResponse]
			,[TransactionID]
			,[Amount]
			,[MaskedCardNumber]
			,[CardFundMethod]
			,[CardBrand]
			,[CardType]
			,[CardIssue]
			,[CardHolderName]
			,ExpiryMonth
			,ExpiryYear
			,[Token]
			,[PaymentStatus]
			,[ApiResult]
			,[GatewayCode]
			,[CustomerIP]
			,[PaymentOn]
			,[MPGSOrderID])
		 VALUES
			(@PaymentRequest,
			@PaymentResponse,
			@TransactionID,
			@Amount,
			@MaskedCardNumber,
			@CardFundMethod,
			@CardBrand,
			@CardType,
			@CardIssuer,
			@CardHolderName,
			@ExpiryMonth,
			@ExpiryYear,
			@Token,
			@PaymentStatus,
			@ApiResult,
			@GatewayCode,
			@CustomerIP,
			GETDATE()
			,@MPGSOrderID)
		SET @PaymentID = SCOPE_IDENTITY();
		DECLARE @PortalSlotID NVARCHAR(50)
	
		IF(@Source = 'Orders')
		BEGIN
			SELECT @AccountID = AccountID FROM Orders WHERE OrderID = @OrderID	

			UPDATE Orders SET PaymentID = @PaymentID, FinalPrice = @Amount, OrderStatus = 1, ProcessedOn = GETDATE()
			WHERE OrderID = @OrderID
			
			UPDATE Orders SET  OrderNumber = dbo.ufnGetOrderNumber(), InvoiceNumber = dbo.ufnGetInvoiceNumber()
			WHERE OrderID = @OrderID
	
			INSERT INTO OrderStatusLog (OrderID, OrderStatus, UpdatedOn) VALUES (@OrderID, 1, GETDATE())

			--Create subscriber details on the customer level
			INSERT INTO Subscribers
			(
				[AccountID]
				,[MobileNumber]
				,[SIMID]
				,[PremiumType]
				,[DisplayName]
				,[IsPrimary]
				,[IsPorted]
				,[DonorProviderName]
				,[SMSSubscription]
				,[DepositFee]
				,[IsBuddyLine]
				,RefOrderSubscriberID)
			SELECT [AccountID]
				,[MobileNumber]
				,[SIMID]
				,[PremiumType]
				,[DisplayName]
				,[IsPrimaryNumber]
				,[IsPorted]
				,[DonorProvider]
				,1
				,[DepositFee]
				,[IsBuddyLine]
				,OrderSubscriberID
			FROM Orders INNER JOIN 
				OrderSubscribers ON Orders.OrderID = OrderSubscribers.OrderID
			WHERE Orders.OrderID = @OrderID
		
			INSERT INTO SubscriberStates
				 ([SubscriberID]
				   ,[State]
				   ,[StateDate]
				   ,[StateSource])
			SELECT
				SubscriberID,
				'Created',
				GETDATE(),
				0
			FROM Subscribers INNER JOIN 
				OrderSubscribers ON Subscribers.RefOrderSubscriberID = OrderSubscribers.OrderSubscriberID
			WHERE OrderID = @OrderID
	
			INSERT INTO [dbo].[Subscriptions]
				([SubscriberID]
				,[PlanID]
				,[PurchasedOn]
				,[ActivatedOn]
				,[BSSPlanCode]
				,[PlanMarketingName]
				,[SubscriptionFee]
				,[ValidFrom]
				,[ValidTo]
				,[Status]
				,[BundleID]
				,IsRemovable)
			SELECT 
				[SubscriberID]
				,[PlanID]
				,[PurchasedOn]
				,[ActivatedOn]
				,[BSSPlanCode]
				,PlanMarketingName
				,[SubscriptionFee]
				,[ValidFrom]
				,[ValidTo]
				,0
				,[BundleID],
				0
			FROM Subscribers INNER JOIN 
				OrderSubscribers ON Subscribers.RefOrderSubscriberID = OrderSubscribers.OrderSubscriberID INNER JOIN
				OrderSubscriberChangeRequests ON OrderSubscriberChangeRequests.OrderSubscriberID = OrderSubscribers.OrderSubscriberID INNER JOIN
				ChangeRequests ON OrderSubscriberChangeRequests.ChangeRequestID = ChangeRequests.ChangeRequestID INNER JOIN
				OrderSubscriptions ON ChangeRequests.ChangeRequestID = OrderSubscriptions.ChangeRequestID
			WHERE OrderID = @OrderID

			SELECT @PortalSlotID = PortalSlotID FROM DeliveryInformation INNER JOIN Orders ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID WHERE OrderID = @OrderID

			UPDATE DeliverySlots SET UsedQuantity = UsedQuantity + 1 WHERE PortalSlotID = @PortalSlotID
		END
		ELSE IF(@Source = 'ChangeRequest')
		BEGIN
			IF EXISTS(SELECT AccountID FROM ChangeRequests INNER JOIN AccountChangeRequests ON ChangeRequests.ChangeRequestID = AccountChangeRequests.ChangeRequestID WHERE ChangeRequests.ChangeRequestID = @OrderID)
			BEGIN	
				SELECT @AccountID = AccountID FROM ChangeRequests INNER JOIN 
					AccountChangeRequests ON ChangeRequests.ChangeRequestID = AccountChangeRequests.ChangeRequestID 
				WHERE ChangeRequests.ChangeRequestID = @OrderID	
			END
			ELSE
			BEGIN	
				SELECT @AccountID = AccountID FROM ChangeRequests INNER JOIN 
					SubscriberRequests ON ChangeRequests.ChangeRequestID = SubscriberRequests.ChangeRequestID  INNER JOIN
					Subscribers ON SubscriberRequests.SubscriberID = Subscribers.SubscriberID
				WHERE ChangeRequests.ChangeRequestID = @OrderID	
			END
			UPDATE ChangeRequests SET PaymentID = @PaymentID, FinalPrice = @Amount, OrderStatus = 1
			WHERE ChangeRequestID = @OrderID	
			
			UPDATE ChangeRequests SET InvoiceNumber = dbo.ufnGetInvoiceNumber()
			WHERE ChangeRequestID = @OrderID

			SELECT @PortalSlotID = PortalSlotID FROM DeliveryInformation INNER JOIN ChangeRequests ON ChangeRequests.DeliveryInformationID = DeliveryInformation.DeliveryInformationID WHERE ChangeRequests.ChangeRequestID = @OrderID
			
			UPDATE DeliverySlots SET UsedQuantity = UsedQuantity + 1 WHERE PortalSlotID = @PortalSlotID
		END
		ELSE IF(@Source = 'AccountInvoices')
		BEGIN
			DECLARE @Remarks NVARCHAR(255)
			SELECT @AccountID = AccountID, @Remarks = Remarks FROM AccountInvoices 
			WHERE InvoiceID = @OrderID

			UPDATE AccountInvoices SET PaymentID = @PaymentID, OrderStatus = 1, PaidOn = GETDATE()
			WHERE InvoiceID = @OrderID		

			IF(@Remarks = 'RecheduleDeliveryInformation')
			BEGIN
				EXEC Orders_ProcessRescheduleDelivery @OrderID
			END
		END

		--Set default payment mode on customer level
		IF(@PaymentMethodSubscription = 1)
		BEGIN
			 IF EXISTS(SELECT * FROM PaymentMethods WHERE AccountID = @AccountID AND Token = @Token)
			 BEGIN 			
				UPDATE PaymentMethods SET IsDefault = 0 
				WHERE AccountID = @AccountID
				UPDATE PaymentMethods SET IsDefault = 1 
				WHERE AccountID = @AccountID AND Token = @Token
			 END
			 ELSE
			 BEGIN
				UPDATE PaymentMethods SET IsDefault = 0 WHERE AccountID = @AccountID
				INSERT INTO PaymentMethods 
					([AccountID]
				   ,[MaskedCardNumer]
				   ,[Token]
				   ,[CardType]
				   ,[IsDefault]
				   ,[CardHolderName]
				   ,[ExpiryMonth]
				   ,[ExpiryYear]
				   ,[CardFundMethod]
				   ,[CardBrand]
				   ,[CardIssuer])
				VALUES 
				(
					@AccountID,
					@MaskedCardNumber,
					@Token,
					@CardType,
					1,
					@CardHolderName,
					@ExpiryMonth,
					@ExpiryYear,
					@CardFundMethod,
					@CardBrand,
					@CardIssuer
				)
			 END
		END

		IF @@ERROR<>0
		RETURN 121 -- transaction updation failed
		ELSE
		RETURN 120 -- transaction updation success
	END
	ELSE
		RETURN 126--already processed response code
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_ProcessRescheduleDelivery]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_ProcessRescheduleDelivery]
	@AccountInvoiceID INT   
AS  
BEGIN  
	BEGIN TRY  
		BEGIN TRAN   
  
		DECLARE @CurrentDate DATE;    
		SET @CurrentDate = CAST(GETDATE() AS DATE)  
		DECLARE @SourceType NVARCHAR(50)
		DECLARE @OrderStatus INT, @SourceID INT, @RescheduleDeliveryInformationID INT;  
		DECLARE @DeliveryInformationID INT = 0  
		SELECT @SourceID = SourceID, @SourceType = SourceType, @RescheduleDeliveryInformationID = RescheduleDeliveryInformationID 
		FROM RescheduleDeliveryInformation 
			INNER JOIN AccountInvoices ON RescheduleDeliveryInformation.RescheduleDeliveryInformationID = AccountInvoices.PaymentSourceID  
		WHERE AccountInvoices.InvoiceID = @AccountInvoiceID		
	
		DECLARE @OldPortalSlotID NVARCHAR(50);
		DECLARE @PortalSlotID NVARCHAR(50);

		IF(@SourceType = 'Orders')
		BEGIN 
			SELECT @OrderStatus = OrderStatus, @DeliveryInformationID = DeliveryInformationID    
			FROM Orders  
			WHERE OrderID = @SourceID  
  
			IF(@OrderStatus != 1 AND @OrderStatus != 6)  
			BEGIN  
				ROLLBACK TRAN  
				RETURN 135  
			END  
			SELECT @OldPortalSlotID = PortalSlotID FROM DeliveryInformation INNER JOIN Orders ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID 
			WHERE OrderID = @SourceID	
		END
		ELSE 
		BEGIN   
			SELECT @OrderStatus = OrderStatus, @DeliveryInformationID = DeliveryInformationID   
			FROM ChangeRequests  
			WHERE ChangeRequestID = @SourceID  
  
			IF(@OrderStatus != 1 AND @OrderStatus != 3)  
			BEGIN  
				ROLLBACK TRAN  
				RETURN 135  
			END  
			SELECT @OldPortalSlotID = PortalSlotID FROM DeliveryInformation INNER JOIN ChangeRequests ON ChangeRequests.DeliveryInformationID = DeliveryInformation.DeliveryInformationID 
			WHERE ChangeRequestID = @SourceID	
		END
		DECLARE @ServiceFee FLOAT = 0  

		SELECT @PortalSlotID = PortalSlotID FROM RescheduleDeliveryInformation WHERE RescheduleDeliveryInformationID = @RescheduleDeliveryInformationID
		IF(@PortalSlotID <> @OldPortalSlotID)
		BEGIN
			UPDATE DeliverySlots SET UsedQuantity = UsedQuantity - 1
			WHERE PortalSlotID = @OldPortalSlotID

			UPDATE DeliverySlots SET UsedQuantity = ISNULL(UsedQuantity, 0) + 1
			WHERE PortalSlotID = @PortalSlotID
		END

		INSERT INTO DeliveryInformationLog
		(  
			DeliveryInformationID  
			,ShippingNumber  
			,Name  
			,Email  
			,IDNumber  
			,IDType  
			,OrderType
			,IsSameAsBilling  
			,ShippingContactNumber  
			,ShippingFloor  
			,ShippingUnit  
			,ShippingBuildingName  
			,ShippingBuildingNumber  
			,ShippingStreetName  
			,ShippingPostCode  
			,AlternateRecipientName  
			,AlternateRecipientEmail  
			,AlternateRecipientContact  
			,AlternateRecioientIDNumber  
			,AlternateRecioientIDType  
			,PortalSlotID  
			,ScheduledDate  
			,DeliveryVendor  
			,DeliveryOn  
			,DeliveryTime  
			,TrackingCode  
			,TrackingUrl  
			,DeliveryFee  
			,VoucherID  
			,LoggedOn  
			,RescheduleReasonID  
		)  
		SELECT 
			DeliveryInformationID  
			,ShippingNumber  
			,Name  
			,Email  
			,IDNumber  
			,IDType
			,OrderType  
			,IsSameAsBilling  
			,ShippingContactNumber  
			,ShippingFloor  
			,ShippingUnit  
			,ShippingBuildingName  
			,ShippingBuildingNumber  
			,ShippingStreetName  
			,ShippingPostCode  
			,AlternateRecipientName  
			,AlternateRecipientEmail  
			,AlternateRecipientContact  
			,AlternateRecioientIDNumber  
			,AlternateRecioientIDType  
			,PortalSlotID  
			,ScheduledDate  
			,DeliveryVendor  
			,DeliveryOn  
			,DeliveryTime  
			,VendorTrackingCode  
			,VendorTrackingUrl  
			,DeliveryFee  
			,VoucherID  
			,GETDATE()  
			,CASE @OrderStatus WHEN 1 THEN 3 ELSE 2 END  
		FROM DeliveryInformation  
		WHERE DeliveryInformationID = @DeliveryInformationID  

		UPDATE DeliveryInformation SET  
			OrderType = 3
			,ShippingContactNumber = R.ShippingContactNumber  
			,ShippingFloor = R.ShippingFloor  
			,ShippingUnit = R.ShippingUnit  
			,ShippingBuildingName = R.ShippingBuildingName  
			,ShippingBuildingNumber = R.ShippingBuildingNumber  
			,ShippingStreetName = R.ShippingStreetName  
			,ShippingPostCode = R.ShippingPostCode  
			,AlternateRecipientName = R.AlternateRecipientName  
			,AlternateRecipientEmail = R.AlternateRecipientEmail  
			,AlternateRecipientContact = R.AlternateRecipientContact  
			,AlternateRecioientIDNumber = R.AlternateRecioientIDNumber  
			,AlternateRecioientIDType = R.AlternateRecioientIDType  
			,PortalSlotID = R.PortalSlotID  
			,ScheduledDate  = R.ScheduledDate  
		FROM DeliveryInformation 
			INNER JOIN  Orders ON DeliveryInformation.DeliveryInformationID = Orders.DeliveryInformationID 
			INNER JOIN  
			(   
				SELECT
					ShippingContactNumber  
					,ShippingFloor  
					,ShippingUnit  
					,ShippingBuildingName  
					,ShippingBuildingNumber  
					,ShippingStreetName  
					,ShippingPostCode  
					,AlternateRecipientName  
					,AlternateRecipientEmail  
					,AlternateRecipientContact  
					,AlternateRecioientIDNumber  
					,AlternateRecioientIDType  
					,PortalSlotID  
					,ScheduledDate  
					,SourceID       
				FROM RescheduleDeliveryInformation   
				WHERE RescheduleDeliveryInformationID = @RescheduleDeliveryInformationID  
			) R ON Orders.OrderID = R.SourceID     
		WHERE DeliveryInformation.DeliveryInformationID = @DeliveryInformationID       
     
		COMMIT TRAN  
	END TRY  
  
	BEGIN CATCH  
		ROLLBACK TRAN  
	END CATCH  
  
	IF @@ERROR <> 0  
		RETURN 107  
	ELSE  
		RETURN 100  
END  
GO
/****** Object:  StoredProcedure [dbo].[Orders_RemoveAdditionalSubscriber]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_RemoveAdditionalSubscriber] --[Orders_RemoveAdditionalSubscriber] 270, '88400293'
	@OrderID INT,
	@MobileNumber NVARCHAR(50)
AS
BEGIN
	IF EXISTS(SELECT * FROM Orders WHERE OrderStatus = 0 AND OrderID = @OrderID)
	BEGIN
		IF EXISTS(SELECT * FROM OrderSubscribers WHERE OrderID = @OrderID AND MobileNumber = @MobileNumber)
		BEGIN
			DECLARE @IsPrimary INT
			declare @OrderSubscriberID Int
			SELECT @IsPrimary = OrderSubscribers.IsPrimaryNumber FROM OrderSubscribers 
			WHERE OrderID = @OrderID AND MobileNumber = @MobileNumber
			IF(@IsPrimary = 1)
			BEGIN
				RETURN 122 --primary number cannot be deleted
			END
			ELSE
			BEGIN		
				BEGIN TRY
					BEGIN TRAN
					select @OrderSubscriberID=OrderSubscribers.OrderSubscriberID  from OrderSubscribers 

					where OrderSubscribers.OrderID =@OrderID AND MobileNumber = @MobileNumber		
			
					delete from OrderSubscriberChangeRequests Where OrderSubscriberID= @OrderSubscriberID
						
					delete from subscriberCharges Where OrderSubscriberID= @OrderSubscriberID

					DELETE FROM OrderSubscriberLogs WHERE OrderSubscriberID = @OrderSubscriberID

					DELETE FROM OrderSubscribers 
					WHERE OrderSubscriberID = @OrderSubscriberID
					COMMIT TRAN
					RETURN 103 --deleted
				END TRY
				BEGIN CATCH
					ROLLBACK TRAN
					RETURN 109
				END CATCH
			END
		END
		ELSE
		BEGIN
			RETURN 109 --does not exist
		END
	END
	ELSE
	BEGIN
		RETURN 123 --cannot be deleted (reason)?
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_RemoveLOADetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_RemoveLOADetails]
	@OrderID INT
AS
BEGIN
	DECLARE @DeliveryInformationID INT
	SELECT @DeliveryInformationID = DeliveryInformationID FROM Orders WHERE OrderID = @OrderID	
	IF(@DeliveryInformationID IS NOT NULL)
	BEGIN
		INSERT INTO DeliveryInformationLog
		(
			DeliveryInformationID
			,[ShippingNumber]
			,[Name]
			,[Email]
			,[IDNumber]
			,[IDType]
			,[IsSameAsBilling]
			,[ShippingContactNumber]
			,[ShippingFloor]
			,[ShippingUnit]
			,[ShippingBuildingName]
			,[ShippingBuildingNumber]
			,[ShippingStreetName]
			,[ShippingPostCode]
			,[AlternateRecipientName]
			,[AlternateRecipientEmail]
			,[AlternateRecipientContact]
			,[AlternateRecioientIDNumber]
			,[AlternateRecioientIDType]
			,[PortalSlotID]
			,[ScheduledDate]
			,[DeliveryVendor]
			,[DeliveryOn]
			,[DeliveryTime]
			,[TrackingCode]
			,[TrackingUrl]
			,[DeliveryFee]
			,[VoucherID]
			,[LoggedOn]
		)
		SELECT
			DeliveryInformationID
			,[ShippingNumber]
			,[Name]
			,[Email]
			,[IDNumber]
			,[IDType]
			,[IsSameAsBilling]
			,[ShippingContactNumber]
			,[ShippingFloor]
			,[ShippingUnit]
			,[ShippingBuildingName]
			,[ShippingBuildingNumber]
			,[ShippingStreetName]
			,[ShippingPostCode]
			,[AlternateRecipientName]
			,[AlternateRecipientEmail]
			,[AlternateRecipientContact]
			,[AlternateRecioientIDNumber]
			,[AlternateRecioientIDType]
			,[PortalSlotID]
			,[ScheduledDate]
			,[DeliveryVendor]
			,[DeliveryOn]
			,[DeliveryTime]
			,[VendorTrackingCode]
			,[VendorTrackingUrl]
			,[DeliveryFee]
			,[VoucherID]
			,GETDATE()
		FROM DeliveryInformation 
		WHERE DeliveryInformationID = @DeliveryInformationID

		UPDATE DeliveryInformation SET
			[AlternateRecipientName] = NULL
			,[AlternateRecipientEmail] = NULL
			,[AlternateRecipientContact] = NULL
			,[AlternateRecioientIDNumber] = NULL
			,[AlternateRecioientIDType] = NULL
		FROM DeliveryInformation 
		WHERE DeliveryInformation.DeliveryInformationID = @DeliveryInformationID
		
		IF @@ERROR<>0
		
		RETURN 106

		ELSE RETURN 101
	END
	ELSE
		RETURN 102
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_RequireBuddy]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_RequireBuddy] --152
	@OrderID INT
AS
BEGIN

 DECLARE @CustomerID int

   Select @CustomerID = Accounts.CustomerID from orders inner join accounts on Orders.AccountID= Accounts.AccountID 
   
   where Orders.OrderID=@OrderID

	SELECT @CustomerID as CustomerID, OrderSubscribers.OrderSubscriberID, MobileNumber, BuddyBundles.HasBuddyPromotion 
	FROM Orders
		INNER JOIN OrderSubscribers ON Orders.OrderID = OrderSubscribers.OrderID
		INNER JOIN 
		(
			SELECT OrderSubscriberID, HasBuddyPromotion
			FROM OrderSubscriberChangeRequests 
				INNER JOIN ChangeRequests ON ChangeRequests.ChangeRequestID = OrderSubscriberChangeRequests.ChangeRequestID
				INNER JOIN OrderSubscriptions ON ChangeRequests.ChangeRequestID = OrderSubscriptions.ChangeRequestID
				INNER JOIN Bundles ON OrderSubscriptions.BundleID = Bundles.BundleID
			GROUP BY OrderSubscriberID, HasBuddyPromotion
		) BuddyBundles ON OrderSubscribers.OrderSubscriberID = BuddyBundles.OrderSubscriberID
	WHERE Orders.OrderID = @OrderID
		AND Orders.OrderStatus = 1 
		AND OrderSubscribers.BuddyProcessedOn IS NULL

		IF @@ROWCOUNT> 0

		RETURN 105

		ELSE RETURN 102
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_Reschedule_GetAvailableSlots]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_Reschedule_GetAvailableSlots]
AS
BEGIN
	DECLARE @Margin INT
	SELECT @Margin = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'DeliveryMarginInDays'
	DECLARE @Limit INT =10
	SELECT @Limit = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'DeliveryLimitInDays'
	SELECT PortalSlotID,
		SlotDate,
		SlotFromTime,
		SlotToTime,
		CAST(SlotFromTime AS nvarchar(8)) + ' - ' +  CAST(SlotToTime AS nvarchar(8)) AS Slot,
		ISNULL(AdditionalCharge, 0) AS AdditionalCharge
	FROM DeliverySlots
	WHERE IsActive = 1 
		AND Capacity > UsedQuantity
		AND SlotDate > CAST(DATEADD(d, @Margin, GETDATE()) AS DATE) 
		AND SlotDate <= CAST(DATEADD(d, @Limit, GETDATE()) AS DATE)
		AND GETDATE() < RescheduleCutOffTime
	ORDER BY SlotDate, SlotFromTime
	IF @@ROWCOUNT=0
	BEGIN
		RETURN 102	
	END
	ELSE 
		RETURN 105
END


--DECLARE @Margin INT;	SELECT @Margin = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'DeliveryMarginInDays';	DECLARE @Limit INT =10;	SELECT @Margin = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'DeliveryLimitInDays'SELECT CAST(DATEADD(d, @Margin, GETDATE()) AS DATE), CAST(DATEADD(d, @Limit, GETDATE()) AS DATE)
GO
/****** Object:  StoredProcedure [dbo].[Orders_RescheduleDelivery]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_RescheduleDelivery]     
 @CustomerID INT     
 ,@OrderType int --1=Orders;2=CHangeRequests     
 ,@OrderID INT      
 ,@ShippingContactNumber NVARCHAR(1000)   
 ,@ShippingFloor NVARCHAR(1000) = null     
 ,@ShippingUnit NVARCHAR(1000) = null      
 ,@ShippingBuildingName NVARCHAR(1000) = null      
 ,@ShippingBuildingNumber NVARCHAR(1000) = null      
 ,@ShippingStreetName NVARCHAR(1000)      
 ,@ShippingPostCode NVARCHAR(1000)  
 ,@AlternateRecipientName NVARCHAR(1000) = null     
 ,@AlternateRecipientEmail NVARCHAR(1000) = null     
 ,@AlternateRecipientContact NVARCHAR(1000) = null     
 ,@AlternateRecipientIDNumber NVARCHAR(1000) = null     
 ,@AlternateRecipientIDType NVARCHAR(1000) = null     
 ,@PortalSlotID NVARCHAR(1000)   
 ,@ScheduledDate DATE  
     
AS      
BEGIN      
 BEGIN TRY      
  BEGIN TRAN      
  -- Temporary code needs to be remove once proper sp starts working      
  --COMMIT TRAN      
  --RETURN 100      
      
  DECLARE @CurrentDate DATE;        
  SET @CurrentDate = CAST(GETDATE() AS DATE)      
      
  DECLARE @OrderStatus INT;       
  IF(@OrderType = 1)    
  BEGIN     
   SELECT @OrderStatus = OrderStatus      
   FROM Orders      
   WHERE OrderID = @OrderID      
      
   IF(@OrderStatus != 1 AND @OrderStatus != 6)      
   BEGIN      
    ROLLBACK TRAN      
    RETURN 135      
   END      
  END    
  ELSE     
  BEGIN       
   SELECT @OrderStatus = OrderStatus      
   FROM ChangeRequests      
   WHERE ChangeRequestID = @OrderID      
      
   IF(@OrderStatus != 1 AND @OrderStatus != 3)      
   BEGIN      
    ROLLBACK TRAN      
    RETURN 135      
   END      
  END    
  
 DECLARE @SlotAvailablity INT = 0;   
    
 DECLARE @Margin INT  
 SELECT @Margin = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'DeliveryMarginInDays'  
 DECLARE @Limit INT =10  
 SELECT @Limit = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'DeliveryLimitInDays'  
    
 SELECT @SlotAvailablity = COUNT(*) FROM DeliverySlots  
 WHERE IsActive = 1   
  AND Capacity > UsedQuantity  
  AND SlotDate > CAST(DATEADD(d, @Margin, GETDATE()) AS DATE)   
  AND SlotDate <= CAST(DATEADD(d, @Limit, GETDATE()) AS DATE)  
  AND GETDATE() < RescheduleCutOffTime  
  AND PortalSlotID = @PortalSlotID  
 IF(@SlotAvailablity <= 0)  
 BEGIN  
  ROLLBACK TRAN    
  RETURN 130;  
 END  
  
  DECLARE @AccountInvoiceID INT    
  DECLARE @AccountID INT      
  DECLARE @ServiceFee FLOAT = 0      
      
  SELECT @AccountID = Accounts.AccountID      
  FROM Accounts      
  WHERE CustomerID = @CustomerID      
      
  INSERT INTO RescheduleDeliveryInformation     
  (      
   SourceType      
   ,SourceID      
   ,RescheduleReasonID      
   ,Name      
   ,Email      
   ,IDNumber      
   ,IDType      
   ,ShippingContactNumber      
   ,ShippingFloor      
   ,ShippingUnit      
   ,ShippingBuildingName      
   ,ShippingBuildingNumber      
   ,ShippingStreetName      
   ,ShippingPostCode      
   ,AlternateRecipientName      
   ,AlternateRecipientEmail      
   ,AlternateRecipientContact      
   ,AlternateRecioientIDNumber      
   ,AlternateRecioientIDType      
   ,PortalSlotID      
   ,ScheduledDate      
   ,CreatedOn      
  )      
  SELECT     
   CASE @OrderType WHEN 1 THEN 'Orders' ELSE 'ChangeRequests' END    
   ,@OrderID      
   ,CASE @OrderStatus WHEN 1 THEN 3 ELSE 2 END      
   ,Customers.Name      
   ,Customers.Email      
   ,Documents.IdentityCardNumber      
   ,Documents.IdentityCardType      
   ,@ShippingContactNumber      
   ,@ShippingFloor      
   ,@ShippingUnit      
   ,@ShippingBuildingName      
   ,@ShippingBuildingNumber      
   ,@ShippingStreetName      
   ,@ShippingPostCode      
   ,@AlternateRecipientName      
   ,@AlternateRecipientEmail      
   ,@AlternateRecipientContact      
   ,@AlternateRecipientIDNumber      
   ,@AlternateRecipientIDType      
   ,@PortalSlotID      
   ,@ScheduledDate      
   ,GETDATE()      
  FROM Customers      
   INNER JOIN Accounts ON Accounts.CustomerID = Customers.CustomerID      
   LEFT JOIN     
   (      
    SELECT     
     CustomerID      
     ,IdentityCardNumber           ,IdentityCardType      
    FROM     
    (      
     SELECT     
      CustomerID      
      ,IdentityCardNumber      
      ,IdentityCardType      
      ,ROW_NUMBER() OVER (PARTITION BY CustomerID ORDER BY CreatedOn DESC) AS RNum      
     FROM Documents      
    ) A      
    WHERE Rnum = 1      
   ) Documents ON Documents.CustomerID = Accounts.CustomerID      
  WHERE Customers.CustomerID = @CustomerID      
      
  DECLARE @RescheduleDeliveryInformationID INT      
  SET @RescheduleDeliveryInformationID = SCOPE_IDENTITY();      
      
  IF (@OrderStatus = 1)      
  BEGIN      
   DECLARE @RetryCount INT      
   ,@ActualRetryCount INT      
      
   SELECT @RetryCount = CONVERT(INT, ConfigValue)      
   FROM Config      
   WHERE ConfigKey = 'DeliveryRetryCount'      
      
   -- Compare the retrycount with DeliveryInformationCount       
   SELECT @ActualRetryCount = COUNT(DeliveryInformationLog.DeliveryInformationID)      
   FROM Orders      
    INNER JOIN DeliveryInformation ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID      
    INNER JOIN DeliveryInformationLog ON DeliveryInformation.DeliveryInformationID = DeliveryInformationLog.DeliveryInformationID      
     AND RescheduleReasonID IS NOT NULL      
   GROUP BY OrderID      
   HAVING OrderID = @OrderID      
      
   IF (@ActualRetryCount > @RetryCount)      
   BEGIN      
    SELECT @ServiceFee = AdminServices.ServiceFee      
    FROM AdminServices      
    WHERE AdminServices.ServiceType = 'Reschedule'      
      
    INSERT INTO AccountInvoices (      
     AccountID      
     ,FinalAmount      
     ,Remarks      
     ,OrderStatus      
     ,PaymentSourceID      
     ,CreatedOn      
     )      
    VALUES    
    (    
      @AccountID      
     ,@ServiceFee      
     ,'RecheduleDeliveryInformation'      
     ,0      
     ,@RescheduleDeliveryInformationID      
     ,@CurrentDate        
    )    
    
    SET @AccountInvoiceID = SCOPE_IDENTITY();    
    
    -- OUTPUT FOR non zero Payable Amount       
    SELECT @ServiceFee AS PayableAmount      
     ,@AccountInvoiceID  AS AccountInvoiceID    
   END      
   ELSE      
    BEGIN      
     INSERT INTO AccountInvoices     
     (      
      AccountID      
      ,FinalAmount      
      ,Remarks      
      ,OrderStatus      
      ,PaymentSourceID      
      ,CreatedOn      
      )      
     VALUES    
     (    
       @AccountID      
      ,0      
      ,'RecheduleDeliveryInformation'      
      ,1     
      ,@RescheduleDeliveryInformationID      
      ,@CurrentDate        
     )    
  SET @AccountInvoiceID = SCOPE_IDENTITY();    
     -- OUTPUT FOR  zero Payable Amount       
     SELECT @ServiceFee AS PayableAmount      
      ,@AccountInvoiceID  AS AccountInvoiceID    
    END      
  END      
  ELSE IF (@OrderStatus = 6)      
  BEGIN      
   SELECT @ServiceFee = AdminServices.ServiceFee      
   FROM AdminServices      
   WHERE AdminServices.ServiceType = 'ReDelivery'      
      
   DECLARE @OrderVoucherID INT      
   DECLARE @VoucherAmount FLOAT = 0      
   SELECT @VoucherAmount = Vouchers.DiscountValue, @OrderVoucherID = OrderVoucherID      
   FROM OrderVouchers      
    INNER JOIN Vouchers ON Vouchers.VoucherID = OrderVouchers.VoucherID      
   WHERE OrderID = @OrderID      
    AND IsUsed = 0 AND ISNULL(OrderVouchers.ValidTo, @CurrentDate)>= @CurrentDate      
      
   UPDATE OrderVouchers SET IsUsed = 1 WHERE OrderVoucherID = @OrderVoucherID      
   UPDATE RescheduleDeliveryInformation SET RefOrderVoucherID = @OrderVoucherID WHERE RescheduleDeliveryInformationID = @RescheduleDeliveryInformationID      
      
   INSERT INTO AccountInvoices (      
    AccountID      
    ,FinalAmount      
    ,Remarks      
    ,OrderStatus      
    ,PaymentSourceID      
    ,CreatedOn      
   )      
   VALUES    
   (     
    @AccountID      
    ,(@ServiceFee + @VoucherAmount)      
    ,'RecheduleDeliveryInformation'      
    ,0      
    ,@RescheduleDeliveryInformationID      
    ,@CurrentDate      
   )    
    
   SET @AccountInvoiceID = SCOPE_IDENTITY();    
    
   -- OUTPUT FOR non zero Payable Amount       
   SELECT SUM(@ServiceFee + @VoucherAmount) AS PayableAmount      
    ,@AccountInvoiceID  AS AccountInvoiceID    
  END      
  ELSE      
  BEGIN      
   ROLLBACK TRAN      
   RETURN 106      
  END      
  COMMIT TRAN      
 END TRY      
      
 BEGIN CATCH      
  ROLLBACK TRAN      
 END CATCH      
      
 IF @@ERROR <> 0      
  RETURN 107      
 ELSE      
  RETURN 100      
END 
GO
/****** Object:  StoredProcedure [dbo].[Orders_RollBackOldUnfinishedOrder]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_RollBackOldUnfinishedOrder] --Orders_CreateOrder 30,1, '9H3H7fjC'  
 @OrderID INT  
AS  
BEGIN  
   
 IF NOT EXISTS (SELECT * FROM OrderSubscribers WHERE OrderID=@OrderID)  
 BEGIN  
  
  If exists (select * from Orders  WHERE OrderID=@OrderID AND OrderStatus=0)

  SET NOCOUNT OFF

  BEGIN
	
	 Delete FROM OrderDocuments WHERE OrderID=@OrderID  

	 DELETE FROM OrderStatusLog WHERE OrderID=@OrderID  
  
	 DELETE FROM Orders WHERE OrderID =@OrderID 
  
 IF @@ERROR<>0  
  
 RETURN 104   
  
 ELSE   
  
 RETURN 103  
  
  END

 END  
  
END  
GO
/****** Object:  StoredProcedure [dbo].[Orders_UpdateBSSCallNumbers]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateBSSCallNumbers] (
	@UserID NVARCHAR(50),
	@Json NVARCHAR(MAX),
	@BSSCallLogID int
	)
AS
BEGIN



IF EXISTS(SELECT * FROM BSSCallLogs WHERE BSSCallLogID = @BSSCallLogID)
BEGIN
		INSERT INTO BSSCallNumbers
    
		SELECT @BSSCallLogID,MobileNumber,GETDATE() FROM  		
		 
		 OPENJSON ( @json )  
		 WITH (   
		 MobileNumber   varchar(50) '$.MobileNumber'              
 ) 

 IF @@ROWCOUNT>0

 RETURN 100

END

ELSE
		
RETURN 102 -- No CallLogID exists 

END
GO
/****** Object:  StoredProcedure [dbo].[Orders_UpdateCheckoutResponse]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateCheckoutResponse]
	@MPGSOrderID nvarchar(255),	
	@Status NVARCHAR(50)	
AS
BEGIN

DECLARE @RESULT INT
	UPDATE CheckoutRequests SET Status = @Status, UpdatedOn = GETDATE()
	WHERE MPGSOrderID = @MPGSOrderID	

	IF @@ROWCOUNT> 0
	SET @RESULT=101
	ELSE SET @RESULT= 106
	IF(@Status <> 'Success')
	BEGIN
		DECLARE @SourceID INT
		DECLARE @Source NVARCHAR(50)
		SELECT @SourceID = SourceID, @Source = SourceType FROM CheckoutRequests WHERE MPGSOrderID = @MPGSOrderID
		IF(@Source = 'Orders')
		BEGIN
			UPDATE Orders SET OrderStatus = 1 WHERE OrderID = @SourceID
			INSERT INTO OrderStatusLog (OrderID, OrderStatus, UpdatedOn) VALUES (@SourceID, 5, GETDATE())
		END
		ELSE IF(@Source = 'ChangeRequest')
		BEGIN
			UPDATE ChangeRequests SET OrderStatus = 1 WHERE ChangeRequestID = @SourceID
		END
		ELSE IF(@Source = 'AccountInvoices')
		BEGIN
			UPDATE AccountInvoices SET OrderStatus = 1 WHERE InvoiceID = @SourceID
		END
	END 
	-- ELSE CALL Order/CR Process Procedure

	SELECT MPGSOrderID,CheckOutSessionID,Amount from CheckoutRequests where MPGSOrderID=  @MPGSOrderID

	RETURN @RESULT
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_UpdateCheckoutWebhookNotification]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateCheckoutWebhookNotification]
	@MPGSOrderID nvarchar(255),
	@TransactionID nvarchar(255),
	@Status NVARCHAR(50),
	@Amount FLOAT,
	@TimeStamp NVARCHAR(50)

AS
BEGIN   -- Remarks = @Response 

Declare @RESULT INT

	UPDATE CheckoutRequests SET Status = @Status, UpdatedOn = GETDATE()
	WHERE MPGSOrderID = @MPGSOrderID

	IF NOT EXISTS(SELECT * FROM CheckoutWebhookLogs WHERE MPGSOrderID=@MPGSOrderID AND TransactionID=@TransactionID)
	BEGIN
	INSERT INTO CheckoutWebhookLogs 
	(
		[Timestamp],
		TransactionID,
		MPGSOrderID,
		OrderStatus,
		Amount,
		LogOn
	)
	VALUES 
	(
		@TimeStamp,
		@TransactionID,
		@MPGSOrderID,
		@Status,
		@Amount,
		GETDATE()
	)
	IF @@IDENTITY>0

	SET @RESULT=105

	ELSE SET @RESULT=107
	END
	

	IF(@Status <> 'Success') -- need to revice this as 'Captured' is the status for success in webhook
	BEGIN
		DECLARE @SourceID INT
		DECLARE @Source NVARCHAR(50)
		SELECT @SourceID = SourceID, @Source = SourceType FROM CheckoutRequests WHERE MPGSOrderID = @MPGSOrderID
		IF(@Source = 'Orders')
		BEGIN
			UPDATE Orders SET OrderStatus = 1 WHERE OrderID = @SourceID
			INSERT INTO OrderStatusLog (OrderID, OrderStatus, UpdatedOn) VALUES (@SourceID, 5, GETDATE())
			-- check this orderStatus field or map correctly accordding to the @Status value
		END
		ELSE
		BEGIN
			UPDATE ChangeRequests SET OrderStatus = 5 WHERE ChangeRequestID = @SourceID
		END
	END 
	-- ELSE CALL Order/CR Process Procedure

	RETURN @RESULT
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_UpdateMPGSCreateTokenSessionDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateMPGSCreateTokenSessionDetails]
	@MPGSOrderID nvarchar(255), -- MPGS OrderID
	@CheckOutSessionID NVARCHAR(50)=null,
	@SuccessIndicator NVARCHAR(20)=null,
	@CheckoutVersion NVARCHAR(20)=null,
	@TransactionID nvarchar(255)
AS
BEGIN	

	DECLARE @SourceID INT

	IF EXISTS(SELECT SourceID FROM CheckoutRequests WHERE MPGSOrderID=@MPGSOrderID AND  TokenizeTransactionID=@TransactionID)

	BEGIN
	
	DECLARE @CustomerID INT

	SELECT @SourceID=SourceID FROM CheckoutRequests WHERE MPGSOrderID=@MPGSOrderID AND  TokenizeTransactionID=@TransactionID

	select @CustomerID=c.CustomerID from orders o inner join Accounts a on a.AccountID=o.AccountID inner join Customers c
	on c.CustomerID=a.CustomerID and o.OrderID=@SourceID
	select @CustomerID=@CustomerID from Orders

	UPDATE CheckoutRequests SET CheckOutSessionID=@CheckOutSessionID, SuccessIndicator=@SuccessIndicator, CheckoutVersion=@CheckoutVersion
		
	where MPGSOrderID=@MPGSOrderID and TokenizeTransactionID=@TransactionID

	SELECT MPGSOrderID,TokenizeTransactionID, Amount, CheckOutSessionID,SourceID as OrderID, @CustomerID as CustomerID FROM CheckoutRequests where 
	
	MPGSOrderID=@MPGSOrderID and @TransactionID=@TransactionID


	RETURN 105
	END

	ELSE

	RETURN 102	
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_UpdateOrderBasicDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateOrderBasicDetails]
	@OrderID INT,	
	@NameInNRIC NVARCHAR(255),
	@DisplayName NVARCHAR(255),
	@Gender NVARCHAR(255),
	@DOB DATE,
	@ContactNumber NVARCHAR(255)	
	AS
BEGIN
	DECLARE @DocumentID INT
	DECLARE @CustomerID INT
	SELECT @CustomerID = CustomerID FROM Orders INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID WHERE OrderID = @OrderID
	IF EXISTS(SELECT * FROM Orders WHERE OrderID = @OrderID)
	BEGIN		
		UPDATE Orders SET			
			Gender = @Gender,
			NameInNRIC = @NameInNRIC,
			DOB = @DOB,
			BillingContactNumber = BillingContactNumber
		WHERE OrderID = @OrderID

		UPDATE OrderSubscribers SET DisplayName = @DisplayName
		WHERE OrderID = @OrderID AND IsPrimaryNumber = 1

		UPDATE Customers SET
			Name = @NameInNRIC,
			Gender = @Gender,			
			DOB = @DOB,
			MobileNumber = @ContactNumber
		WHERE CustomerID = @CustomerID

		Update Accounts SET 
			AccountName = @NameInNRIC,
			ShippingContactNumber = @ContactNumber,
			BillingContactNumber = @ContactNumber
		WHERE CustomerID = @CustomerID
		RETURN 101 --Updated success
	END
	ELSE
	BEGIN		
		RETURN 102 --does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_UpdateOrderBillingDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateOrderBillingDetails]
	@OrderID INT,
	@Postcode NVARCHAR(50),
	@BlockNumber NVARCHAR(50),
	@Unit NVARCHAR(50) = null,
	@Floor NVARCHAR(50) = null,
	@BuildingName NVARCHAR(255) = null,
	@StreetName NVARCHAR(255),
	@ContactNumber NVARCHAR(255)
AS
BEGIN	
	DECLARE @CustomerID INT
	SELECT @CustomerID = CustomerID FROM Orders INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID WHERE OrderID = @OrderID
	IF EXISTS(SELECT * FROM Orders WHERE OrderID = @OrderID)
	BEGIN	
		UPDATE Orders SET 
			BillingFloor = @Floor,
			BillingUnit = @Unit,
			BillingBuildingNumber = @BlockNumber,
			BillingBuildingName = @BuildingName,
			BillingStreetName = @StreetName,
			BillingPostCode = @Postcode,
			BillingContactNumber = @ContactNumber
		WHERE OrderID = @OrderID

		UPDATE Accounts SET
			BillingFloor = @Floor,
			BillingUnit = @Unit,
			BillingBuildingNumber = @BlockNumber,
			BillingBuildingName = @BuildingName,
			BillingStreetName = @StreetName,
			BillingPostCode = @Postcode,
			BillingContactNumber = @ContactNumber
		WHERE CustomerID = @CustomerID 
		RETURN 101 --Updated success
	END
	ELSE
	BEGIN		
		RETURN 102 --does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_UpdateOrderLOADetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateOrderLOADetails]
	@OrderID INT,
	@RecipientName NVARCHAR(255),
	@IDType NVARCHAR(50),
	@IDNumber NVARCHAR(50),
	@ContactNumber NVARCHAR(255),
	@EmailAdddress NVARCHAR(255)
AS
BEGIN	
	IF EXISTS(SELECT * FROM Orders WHERE OrderID = @OrderID)
	BEGIN	
		DECLARE @DeliveryInformationID INT
		SELECT @DeliveryInformationID = DeliveryInformationID FROM Orders WHERE OrderID = @OrderID
		INSERT INTO DeliveryInformationLog
		(
			DeliveryInformationID
			,[ShippingNumber]
			,[Name]
			,[Email]
			,[IDNumber]
			,[IDType]
			,[IsSameAsBilling]
			,[ShippingContactNumber]
			,[ShippingFloor]
			,[ShippingUnit]
			,[ShippingBuildingName]
			,[ShippingBuildingNumber]
			,[ShippingStreetName]
			,[ShippingPostCode]
			,[AlternateRecipientName]
			,[AlternateRecipientEmail]
			,[AlternateRecipientContact]
			,[AlternateRecioientIDNumber]
			,[AlternateRecioientIDType]
			,[PortalSlotID]
			,[ScheduledDate]
			,[DeliveryVendor]
			,[DeliveryOn]
			,[DeliveryTime]
			,[TrackingCode]
			,[TrackingUrl]
			,[DeliveryFee]
			,[VoucherID]
			,[LoggedOn]
		)
		SELECT
			DeliveryInformationID
			,[ShippingNumber]
			,[Name]
			,[Email]
			,[IDNumber]
			,[IDType]
			,[IsSameAsBilling]
			,[ShippingContactNumber]
			,[ShippingFloor]
			,[ShippingUnit]
			,[ShippingBuildingName]
			,[ShippingBuildingNumber]
			,[ShippingStreetName]
			,[ShippingPostCode]
			,[AlternateRecipientName]
			,[AlternateRecipientEmail]
			,[AlternateRecipientContact]
			,[AlternateRecioientIDNumber]
			,[AlternateRecioientIDType]
			,[PortalSlotID]
			,[ScheduledDate]
			,[DeliveryVendor]
			,[DeliveryOn]
			,[DeliveryTime]
			,[VendorTrackingCode]
			,[VendorTrackingUrl]
			,[DeliveryFee]
			,[VoucherID]
			,GETDATE()
		FROM DeliveryInformation 
		WHERE DeliveryInformationID = @DeliveryInformationID

		UPDATE DeliveryInformation SET 
			AlternateRecipientName = @RecipientName,
			AlternateRecipientEmail = @EmailAdddress,
			AlternateRecipientContact = @ContactNumber,
			AlternateRecioientIDType = @IDType,
			AlternateRecioientIDNumber = @IDNumber
		WHERE DeliveryInformationID = @DeliveryInformationID
		RETURN 101 --Updated success
	END
	ELSE
	BEGIN		
		RETURN 102 --does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_UpdateOrderPersonalIDDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateOrderPersonalIDDetails]
	@OrderID INT,
	@Nationality NVARCHAR(255),
	@IDType NVARCHAR(50),
	@IDNumber NVARCHAR(50),
	@IDImageUrl NVARCHAR(255),	
	@IDImageUrlBack NVARCHAR(255)
AS
BEGIN
	DECLARE @DocumentID INT
	DECLARE @CustomerID INT
	SELECT @CustomerID = CustomerID FROM Orders INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID WHERE OrderID = @OrderID
	IF EXISTS(SELECT * FROM Orders WHERE OrderID = @OrderID)
	BEGIN
		IF EXISTS(SELECT * FROM Documents WHERE IdentityCardType = @IDType AND IdentityCardNumber = @IDNumber AND CustomerID = @CustomerID)
		BEGIN
			SELECT @DocumentID = DocumentID FROM Documents WHERE IdentityCardType = @IDType AND CustomerID = @CustomerID AND IdentityCardNumber = @IDNumber
			UPDATE Documents SET IdentityCardNumber = @IDNumber,
				DocumentURL = @IDImageUrl,DocumentBackURL=@IDImageUrlBack
			WHERE DocumentID = @DocumentID
		END
		ELSE 
		BEGIN
			INSERT INTO Documents
			(
				CustomerID,
				DocumentURL,
				DocumentBackURL,
				IdentityCardNumber,
				IdentityCardType
			)
			VALUES
			(
				@CustomerID,
				@IDImageUrl,
				@IDImageUrlBack,
				@IDNumber,
				@IDType
			)
			SET @DocumentID = SCOPE_IDENTITY();
		END
		DELETE FROM OrderDocuments WHERE OrderID = @OrderID
		INSERT INTO OrderDocuments 
		(
			OrderID,
			DocumentID
		)
		VALUES
		(
			@OrderID,
			@DocumentID
		)
		DELETE SC FROM SubscriberCharges SC INNER JOIN OrderSubscribers ON SC.OrderSubscriberID = OrderSubscribers.OrderSubscriberID WHERE OrderID = @OrderID AND ReasonType = 'Orders - Deposit';
		IF NOT(@IDType = 'NRIC' AND (LEFT(@IDNumber, 1) = 'S' OR LEFT(@IDNumber, 1) = 'T'))
		BEGIN
			INSERT INTO SubscriberCharges
			(
				OrderSubscriberID,
				AdminServiceID,
				ServiceFee,
				IsGSTIncluded,
				IsRecurring,
				ReasonType,
				ReasonID
			)
			SELECT OrderSubscriberID,
				AdminServices.AdminServiceID,
				ServiceFee,
				IsGSTIncluded,
				IsRecurring,
				'Orders - Deposit',
				@OrderID
			FROM AdminServices CROSS JOIN
				OrderSubscribers
			WHERE ServiceType = 'Deposit'
				AND OrderID = @OrderID 
		
		END		
		UPDATE Orders SET
			Nationality = @Nationality			
		WHERE OrderID = @OrderID

		UPDATE Customers SET			
			Nationality = @Nationality			
		WHERE CustomerID = @CustomerID
		RETURN 101 --Updated success
	END
	ELSE
	BEGIN		
		RETURN 102 --does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_UpdateOrderShippingDetails]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateOrderShippingDetails]  
 @OrderID INT,  
 @Postcode NVARCHAR(50),  
 @BlockNumber NVARCHAR(50),  
 @Unit NVARCHAR(50) = null,  
 @Floor NVARCHAR(50) = null,  
 @BuildingName NVARCHAR(255) = null,  
 @StreetName NVARCHAR(255),  
 @ContactNumber NVARCHAR(255),  
 @IsBillingSame INT,  
 @PortalSlotID NVARCHAR(255)  
AS  
BEGIN  
 DECLARE @CustomerID INT  
 SELECT @CustomerID = CustomerID FROM Orders INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID WHERE OrderID = @OrderID  
 IF EXISTS(SELECT * FROM Orders WHERE OrderID = @OrderID)  
 BEGIN   
  DECLARE @DeliveryInformationID INT  
  SELECT @DeliveryInformationID = DeliveryInformationID FROM Orders WHERE OrderID = @OrderID   
    
  DECLARE @DeliveryDate DATE  
  DECLARE @ShippingFee FLOAT  
  SELECT @DeliveryDate= SlotDate, @ShippingFee = ISNULL(AdditionalCharge, 0) FROM DeliverySlots WHERE PortalSlotID = @PortalSlotID  
  
  DELETE OrderCharges   
  FROM OrderCharges   
  WHERE ReasonType = 'Orders - AdditionalDeliveryFee' AND OrderID = @OrderID  
  
  INSERT INTO OrderCharges  
  (  
   OrderID,  
   AdminServiceID,  
   ServiceFee,  
   IsGSTIncluded,  
   IsRecurring,  
   ReasonType,  
   ReasonID  
  )  
  SELECT  
   @OrderID,  
   AdminServiceID,  
   ISNULL(ServiceFee, 0) + @ShippingFee,  
   IsGSTIncluded,  
   IsRecurring,  
   'Orders - AdditionalDeliveryFee',  
   @OrderID  
  FROM AdminServices   
  WHERE AdminServices.ServiceType = 'AdditionalDeliveryFee'  
   AND (ISNULL(ServiceFee, 0) + @ShippingFee) > 0  
  
  IF(@DeliveryInformationID IS NULL)  
  BEGIN  
   INSERT INTO DeliveryInformation  
   (  
    [ShippingNumber]  
      ,[Name]  
      ,[Email]  
      ,[IDNumber]  
      ,[IDType]  
      ,[OrderType]  
      ,[IsSameAsBilling]  
      ,[ShippingContactNumber]  
      ,[ShippingFloor]  
      ,[ShippingUnit]  
      ,[ShippingBuildingName]  
      ,[ShippingBuildingNumber]  
      ,[ShippingStreetName]  
      ,[ShippingPostCode]  
      ,[PortalSlotID]  
      ,[ScheduledDate]  
      ,[DeliveryFee]  
   )  
   SELECT  
    dbo.ufnGetShippingNumber(),  
    NameInNRIC,  
    Customers.Email,  
    IdentityCardNumber,   
    IdentityCardType,  
    0,  
    @IsBillingSame,  
    @ContactNumber,  
    @Floor,  
    @Unit,  
    @BuildingName,  
    @BlockNumber,  
    @StreetName,  
    @Postcode,  
    @PortalSlotID,  
    @DeliveryDate,  
    @ShippingFee  
   FROM Orders INNER JOIN   
    OrderDocuments ON Orders.OrderID = OrderDocuments.OrderID INNER JOIN   
    Documents ON OrderDocuments.DocumentID = Documents.DocumentID INNER JOIN   
    Accounts ON Orders.AccountID = Accounts.AccountID INNER JOIN   
    Customers ON Accounts.CustomerID = Customers.CustomerID  
   WHERE Orders.OrderID = @OrderID  
  
   SET @DeliveryInformationID = SCOPE_IDENTITY();  
  
   UPDATE Orders SET DeliveryInformationID = @DeliveryInformationID WHERE OrderID = @OrderID  
  END  
  ELSE   
  BEGIN  
   SELECT @DeliveryInformationID = DeliveryInformationID FROM Orders WHERE OrderID = @OrderID  
   INSERT INTO DeliveryInformationLog  
   (  
    DeliveryInformationID  
     ,[ShippingNumber]  
     ,[Name]  
     ,[Email]  
     ,[IDNumber]  
     ,[IDType]  
	 ,OrderType
     ,[IsSameAsBilling]  
     ,[ShippingContactNumber]  
     ,[ShippingFloor]  
     ,[ShippingUnit]  
     ,[ShippingBuildingName]  
     ,[ShippingBuildingNumber]  
     ,[ShippingStreetName]  
     ,[ShippingPostCode]  
     ,[AlternateRecipientName]  
     ,[AlternateRecipientEmail]  
     ,[AlternateRecipientContact]  
     ,[AlternateRecioientIDNumber]  
     ,[AlternateRecioientIDType]  
     ,[PortalSlotID]  
     ,[ScheduledDate]  
     ,[DeliveryVendor]  
     ,[DeliveryOn]  
     ,[DeliveryTime]  
     ,[TrackingCode]  
     ,[TrackingUrl]  
     ,[DeliveryFee]  
     ,[VoucherID]  
     ,[LoggedOn]  
   )  
   SELECT  
    DeliveryInformationID  
     ,[ShippingNumber]  
     ,[Name]  
     ,[Email]  
     ,[IDNumber]  
     ,[IDType]  
	 ,OrderType
     ,[IsSameAsBilling]  
     ,[ShippingContactNumber]  
     ,[ShippingFloor]  
     ,[ShippingUnit]  
     ,[ShippingBuildingName]  
     ,[ShippingBuildingNumber]  
     ,[ShippingStreetName]  
     ,[ShippingPostCode]  
     ,[AlternateRecipientName]  
     ,[AlternateRecipientEmail]  
     ,[AlternateRecipientContact]  
     ,[AlternateRecioientIDNumber]  
     ,[AlternateRecioientIDType]  
     ,[PortalSlotID]  
     ,[ScheduledDate]  
     ,[DeliveryVendor]  
     ,[DeliveryOn]  
     ,[DeliveryTime]  
     ,[VendorTrackingCode]  
     ,[VendorTrackingUrl]  
     ,[DeliveryFee]  
     ,[VoucherID]  
     ,GETDATE()  
   FROM DeliveryInformation   
   WHERE DeliveryInformationID = @DeliveryInformationID  
  
   UPDATE DeliveryInformation SET  
    [Name] = NameInNRIC  
     ,[Email] = Customers.Email  
     ,[IDNumber] = IdentityCardNumber  
     ,[IDType] = IdentityCardType  
     ,[IsSameAsBilling] = @IsBillingSame  
     ,[ShippingContactNumber] = @ContactNumber  
     ,[ShippingFloor] = @Floor  
     ,[ShippingUnit] = @Unit  
     ,[ShippingBuildingName] = @BuildingName  
     ,[ShippingBuildingNumber] = @BlockNumber  
     ,[ShippingStreetName] = @StreetName  
     ,[ShippingPostCode] = @Postcode  
     ,[PortalSlotID] = @PortalSlotID  
     ,[ScheduledDate] = @DeliveryDate  
     ,[DeliveryFee] = @ShippingFee  
   FROM DeliveryInformation INNER JOIN   
    Orders ON DeliveryInformation.DeliveryInformationID = Orders.DeliveryInformationID INNER JOIN   
    OrderDocuments ON Orders.OrderID = OrderDocuments.OrderID INNER JOIN   
    Documents ON OrderDocuments.DocumentID = Documents.DocumentID INNER JOIN   
    Accounts ON Orders.AccountID = Accounts.AccountID INNER JOIN   
    Customers ON Accounts.CustomerID = Customers.CustomerID  
   WHERE DeliveryInformation.DeliveryInformationID = @DeliveryInformationID  
  END  
  
  DELETE SubscriberCharges   
  FROM SubscriberCharges   
   INNER JOIN OrderSubscribers ON SubscriberCharges.OrderSubscriberID = OrderSubscribers.OrderSubscriberID   
  WHERE ReasonType = 'Orders - Delivery' AND OrderID = @OrderID  
    
  INSERT INTO SubscriberCharges  
  (  
   OrderSubscriberID,  
   AdminServiceID,  
   ServiceFee,  
   IsGSTIncluded,  
   IsRecurring,  
   ReasonType,  
   ReasonID  
  )  
  SELECT   
   OrderSubscriberID,  
   AdminServiceID,  
   ISNULL(ServiceFee, 0),  
   IsGSTIncluded,  
   IsRecurring,  
   'Orders - Delivery',  
   @OrderID  
  FROM OrderSubscribers CROSS JOIN   
   AdminServices   
  WHERE OrderID = @OrderID   
   AND AdminServices.ServiceType = 'Delivery'  
   AND ISNULL(ServiceFee, 0) > 0  
  
  UPDATE Accounts SET  
   ShippingFloor = @Floor,  
   ShippingUnit = @Unit,  
   ShippingBuildingNumber = @BlockNumber,  
   ShippingBuildingName = @BuildingName,  
   ShippingStreetName = @StreetName,  
   ShippingPostCode = @Postcode,  
   ShippingContactNumber = @ContactNumber  
  WHERE CustomerID = @CustomerID   
  RETURN 101 --Updated success  
 END  
 ELSE  
 BEGIN    
  RETURN 102 --does not exist  
 END  
END  
GO
/****** Object:  StoredProcedure [dbo].[Orders_UpdateOrderSubscriptions]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateOrderSubscriptions] 
	@OrderID INT,
	@ContactNumber NVARCHAR(255) = NULL,
	@Terms INT,
	@PaymentSubscription INT,
	@EmailMessage INT,
	@SMSMessage INT,
	@VoiceMessage INT
AS
BEGIN
	DECLARE @CustomerID INT
	SELECT @CustomerID = CustomerID FROM Orders INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID WHERE OrderID = @OrderID
	
	DECLARE @Margin INT
	SELECT @Margin = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'DeliveryMarginInDays'
	DECLARE @Limit INT =10
	SELECT @Limit = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'DeliveryLimitInDays'
	
	IF EXISTS(SELECT * FROM Orders WHERE OrderID = @OrderID)
	BEGIN
		
		DECLARE @PortalSlotID NVARCHAR(50);
		DECLARE @SlotAvailablity INT = 0;
		SELECT @PortalSlotID = PortalSlotID FROM DeliveryInformation INNER JOIN Orders ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID WHERE OrderID = @OrderID		
		
		
		SELECT @SlotAvailablity = COUNT(*) FROM DeliverySlots
		WHERE IsActive = 1 
			AND Capacity > UsedQuantity
			AND SlotDate > CAST(DATEADD(d, @Margin, GETDATE()) AS DATE) 
			AND SlotDate <= CAST(DATEADD(d, @Limit, GETDATE()) AS DATE)
			AND GETDATE() < NewOrderCutOffTime
			AND PortalSlotID = @PortalSlotID

		IF EXISTS(SELECT * FROM Orders WHERE OrderID = @OrderID AND DeliveryInformationID IS NULL)
		BEGIN
			RETURN 132;
		END
		ELSE IF NOT EXISTS(SELECT * FROM OrderDocuments WHERE OrderID = @OrderID)
		BEGIN
			RETURN 133;
		END
		ELSE IF EXISTS(SELECT * FROM Orders WHERE OrderID = @OrderID AND Nationality IS NULL)
		BEGIN
			RETURN 134;
		END
		ELSE IF(@SlotAvailablity <= 0)
		BEGIN
			RETURN 130;
		END
		ELSE
		BEGIN 
			UPDATE Orders SET
				Terms = @Terms,
				PaymentAcknowledgement = @PaymentSubscription,
				PromotionSubscription = @EmailMessage,
				SMSSubscription = @SMSMessage,
				VoiceSubscription = @VoiceMessage
			WHERE OrderID = @OrderID

			INSERT INTO CustomerLogTrail (CustomerID, ActionDescription, ActionOn) 
			SELECT @CustomerID, 'Updated the primary contact number from ' + MobileNumber  + ' to ' + @ContactNumber, GETDATE() FROM Customers WHERE CustomerID = @CustomerID

			UPDATE Customers SET MobileNumber = @ContactNumber, EmailSubscription = @EmailMessage, SMSSubscription = @SMSMessage, VoiceSubscription = @VoiceMessage WHERE CustomerID = @CustomerID
			RETURN 101 --Updated success
		END
	END
	ELSE
	BEGIN		
		RETURN 102 --does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_UpdatePendingBuddyList]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdatePendingBuddyList]
	@PendingBuddyOrderListID INT,		
	@IsProcessed Bit
	
AS 
BEGIN
	
	IF EXISTS(SELECT * FROM PendingBuddyOrderList WHERE PendingBuddyOrderListID=@PendingBuddyOrderListID)
	BEGIN
	Update PendingBuddyOrderList set IsProcessed=@IsProcessed where PendingBuddyOrderListID=@PendingBuddyOrderListID

	IF @@ERROR <>0

	RETURN 106

	ELSE RETURN  101
END

ELSE

RETURN 102

END
GO
/****** Object:  StoredProcedure [dbo].[Orders_UpdateSubscriberNumber]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateSubscriberNumber]
	@OrderID INT,
	@OldMobileNumber NVARCHAR(8),
	@NewMobileNumber NVARCHAR(8),
	@DisplayName NVARCHAR(50)=NULL,
	@PremiumTypeSericeCode INT=NULL
AS
BEGIN
	IF EXISTS(SELECT OrderID FROM OrderSubscribers WHERE OrderID = @OrderID AND MobileNumber = @OldMobileNumber)
	BEGIN
		DECLARE @OrderSubscriberID INT
		SELECT @OrderSubscriberID = OrderSubscriberID FROM OrderSubscribers
		WHERE OrderID = @OrderID AND MobileNumber = @OldMobileNumber
		--Create Log
		INSERT INTO OrderSubscriberLogs 
		(
			OrderSubscriberID,
			OrderID,
			MobileNumber,
			DisplayName,
			IsPrimaryNumber,
			IsOwnNumber,
			PremiumType,
			IsPorted,
			DonorProvider
		)
		SELECT 
			OrderSubscriberID,
			OrderID,
			MobileNumber,
			DisplayName,
			IsPrimaryNumber,
			IsOwnNumber,
			PremiumType,
			IsPorted,
			DonorProvider
		FROM OrderSubscribers
		WHERE OrderSubscriberID = @OrderSubscriberID

		UPDATE OrderSubscribers SET 
			MobileNumber = @NewMobileNumber,
			PremiumType = CASE WHEN @PremiumTypeSericeCode IS NULL OR @PremiumTypeSericeCode = 0 THEN 0 ELSE @PremiumTypeSericeCode END,
			DisplayName = @DisplayName,
			DonorProvider = NULL,
			PortedNumberOwnedBy = NULL,
			PortedNumberOwnerRegistrationID = NULL,
			PortedNumberTransferForm = NULL
		FROM OrderSubscribers 
		WHERE OrderSubscriberID = @OrderSubscriberID

		--Remove exisiting premium charge on same number
		DELETE FROM SubscriberCharges
		WHERE OrderSubscriberID = @OrderSubscriberID AND
			ReasonType = 'Orders - Premium Charge'
		
		--Charge for premium number			
		INSERT INTO SubscriberCharges
		(
			OrderSubscriberID,
			AdminServiceID,
			ServiceFee,
			IsGSTIncluded,
			IsRecurring,
			ReasonType,
			ReasonID
		)
		SELECT 
			@OrderSubscriberID,
			AdminServiceID,
			ServiceFee,
			IsGSTIncluded,
			IsRecurring,
			'Orders - Premium Charge',
			@OrderID
		FROM AdminServices 
		WHERE ServiceCode = @PremiumTypeSericeCode AND ServiceType = 'Premium'

		RETURN 101 --Updated
	END
	ELSE
	BEGIN
		RETURN 102 -- does not exists
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_UpdateSubscriberPortingNumber]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateSubscriberPortingNumber]
	@OrderID INT,
	@OldMobileNumber NVARCHAR(8),
	@NewMobileNumber NVARCHAR(8),
	@DisplayName NVARCHAR(50) = null,
	@IsOwnNumber INT,
    @DonorProvider NVARCHAR(255),
    @PortedNumberTransferForm NVARCHAR(255) = NULL,
    @PortedNumberOwnedBy NVARCHAR(255) = NULL,
    @PortedNumberOwnerRegistrationID NVARCHAR(255) = NULL
AS
BEGIN
	IF EXISTS(SELECT OrderID FROM OrderSubscribers WHERE OrderID = @OrderID AND MobileNumber = @OldMobileNumber)
	BEGIN
		--CReate Log
		INSERT INTO OrderSubscriberLogs 
		(
			OrderSubscriberID,
			OrderID,
			MobileNumber,
			DisplayName,
			IsPrimaryNumber,
			IsOwnNumber,
			PremiumType,
			IsPorted,
			DonorProvider
		)
		SELECT 
			OrderSubscriberID,
			OrderID,
			MobileNumber,
			DisplayName,
			IsPrimaryNumber,
			IsOwnNumber,
			PremiumType,
			IsPorted,
			DonorProvider
		FROM OrderSubscribers
		WHERE OrderID = @OrderID AND MobileNumber = @OldMobileNumber

		UPDATE OrderSubscribers SET 
			MobileNumber = @NewMobileNumber,
			PremiumType = 0,
			IsPorted = 1,
			DisplayName = @DisplayName,
			IsOwnNumber = @IsOwnNumber,
			DonorProvider = @DonorProvider,
			PortedNumberTransferForm = @PortedNumberTransferForm,
			PortedNumberOwnedBy = @PortedNumberOwnedBy,
			PortedNumberOwnerRegistrationID = @PortedNumberOwnerRegistrationID
		FROM OrderSubscribers 
		WHERE OrderID = @OrderID AND MobileNumber = @OldMobileNumber
		RETURN 101 --Updated
	END
	ELSE
	BEGIN
		RETURN 102 -- does not exists
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_ValidateReferralCode]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_ValidateReferralCode]
	@OrderID INT,
	@ReferralCode NVARCHAR(255)
AS
BEGIN	
	DECLARE @CustomerID INT
	SELECT @CustomerID = CustomerID FROM Orders INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID WHERE OrderID = @OrderID
	IF EXISTS(SELECT * FROM Orders WHERE OrderID = @OrderID)
	BEGIN
		IF EXISTS(SELECT * FROM Customers WHERE ReferralCode = @ReferralCode AND CustomerID <> @CustomerID)
		BEGIN 
			UPDATE Orders SET 
				ReferralCode = @ReferralCode
			WHERE OrderID = @OrderID
			RETURN 105 --record exists
		END
		ELSE 
		BEGIN
			RETURN 102 --does not exist
		END
	END
	ELSE
	BEGIN		
		RETURN 102 --does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[z_EmailNotificationsLogEntry]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[z_EmailNotificationsLogEntry] 
			@CustomerID int= null
           ,@Email nvarchar(255)
           ,@EmailSubject nvarchar(4000)
           ,@EmailBody nvarchar(max)
           ,@ScheduledOn datetime
           ,@EmailTemplateID int
           ,@SendOn datetime
           ,@Status int
AS
BEGIN	

declare @ID INT

select @id= customerID from Customers where Email=@Email

	INSERT INTO dbo.EmailNotification
           (CustomerID
           ,Email
           ,EmailSubject
           ,EmailBody
           ,ScheduledOn
           ,EmailTemplateID
           ,SendOn
           ,Status)
     VALUES
           (@id
           ,@Email
           ,@EmailSubject
           ,@EmailBody
           ,@ScheduledOn
           ,@EmailTemplateID
           ,@SendOn
           ,@Status)

			IF @@IDENTITY>0

			return 100

			else return 107
END
GO
/****** Object:  StoredProcedure [dbo].[z_EmailNotificationsLogEntryForDevPurpose]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
  
CREATE PROCEDURE [dbo].[z_EmailNotificationsLogEntryForDevPurpose]   
   @EventType nvarchar(255)  
           ,@Message nvarchar(max) 
AS  
BEGIN   
  
 INSERT INTO dbo.EmailNotificationLog 
           (EventType  
           ,Message  
           ,CreatedOn)  
     VALUES  
           (@EventType  
           ,@Message
		   ,GETDATE())  
  
   IF @@IDENTITY>0  
  
   return 100  
  
   else return 107  
END  
GO
/****** Object:  StoredProcedure [dbo].[z_GetDBD]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[z_GetDBD] AS
BEGIN
	SELECT 
		TableName, 
		ColName, 
		DataType, 
		[DataLength] / typestat AS [DataLength],
		ISNULL([Description], '') AS [Description]
	FROM 
	(	
		SELECT 
			sysobjects.id AS Major_id,
			syscolumns.colid AS Minor_id,
			sysobjects.name AS TableName, 
			syscolumns.name AS ColName,
			systypes.name AS DataType,
			syscolumns.length AS DataLength,
			CASE syscolumns.typestat WHEN 0 THEN 1 ELSE syscolumns.typestat END AS typestat,
			syscolumns.colorder AS ColOrder,
			sys.extended_properties.value AS [Description]
		FROM syscolumns INNER JOIN sysobjects
				ON syscolumns.id = sysobjects.id 
			INNER JOIN systypes
				ON syscolumns.xtype = systypes.xtype
			LEFT OUTER JOIN sys.extended_properties
				ON sysobjects.id = sys.extended_properties.major_id
					AND syscolumns.colid = sys.extended_properties.minor_id
		WHERE sysobjects.type = 'U'  
	) Table_Details
	WHERE DataType <> 'sysname'
	ORDER BY TableName, ColOrder
END
GO
/****** Object:  StoredProcedure [dbo].[z_GetEmailTemplateByName]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[z_GetEmailTemplateByName]  
 @TemplateName NVARCHAR(255)  
AS  
BEGIN  
 DECLARE @CustomerID INT  
 IF EXISTS(SELECT EmailTemplateID FROM EmailTemplates WHERE TemplateName = @TemplateName)  
 BEGIN  
 SELECT [EmailTemplateID]  
      ,[TemplateName]  
      ,[EmailSubject]  
      ,[EmailBody]  
  FROM [dbo].[EmailTemplates] 
  WHERE TemplateName = @TemplateName 
  
  RETURN 105  -- template exist  
 END  
 ELSE   
 BEGIN   
  RETURN 109 -- TEMPLATE does not exists  
 END  
END  
  
GO
/****** Object:  StoredProcedure [dbo].[z_GetSingleMessageQueueRecord]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[z_GetSingleMessageQueueRecord] 
AS
BEGIN
	
	SELECT
		MessageQueueRequestID 
		,Source
		,SNSTopic
		,MessageAttribute
		,MessageBody
		,STATUS
		,PublishedOn
		,CreatedOn
		,NumberOfRetries
		,LastTriedOn
		FROM MessageQueueRequests
		WHERE Status = 0
		AND NumberOfRetries < (SELECT ConfigValue FROM Config WHERE ConfigType = 'MessageQueuePublisher' AND ConfigKey = 'RetryDurationMins')
		ORDER BY LastTriedOn DESC

		IF @@ROWCOUNT=0
	BEGIN
	RETURN 102	
	END
	ELSE RETURN 105
		
END
GO
/****** Object:  StoredProcedure [dbo].[z_InsertIntoMessageQueueRequests]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[z_InsertIntoMessageQueueRequests] @Source NVARCHAR(1000)
	,@SNSTopic NVARCHAR(1000)
	,@MessageAttribute NVARCHAR(MAX)
	,@MessageBody NVARCHAR(MAX)
	,@Status INT = 0
	,@PublishedOn DATETIME = NULL
	,@CreatedOn DATETIME = NULL
	,@NumberOfRetries INT = 0
	,@LastTriedOn DATETIME = NULL
AS
BEGIN
	DECLARE @CurrentDate DATETIME;

	SET @CurrentDate = GETDATE()

	IF (@PublishedOn IS NULL)
	BEGIN
		SET @PublishedOn = @CurrentDate
	END

	IF (@CreatedOn IS NULL)
	BEGIN
		SET @CreatedOn = @CurrentDate
	END

	IF (@LastTriedOn IS NULL)
	BEGIN
		SET @LastTriedOn = @CurrentDate
	END

	INSERT INTO MessageQueueRequests (
		Source
		,SNSTopic
		,MessageAttribute
		,MessageBody
		,STATUS
		,PublishedOn
		,CreatedOn
		,NumberOfRetries
		,LastTriedOn
		)
	VALUES (
		@Source
		,@SNSTopic
		,@MessageAttribute
		,@MessageBody
		,@Status
		,@PublishedOn
		,@CreatedOn
		,@NumberOfRetries
		,@LastTriedOn
		)
END
GO
/****** Object:  StoredProcedure [dbo].[z_InsertIntoMessageQueueRequestsException]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[z_InsertIntoMessageQueueRequestsException] @Source NVARCHAR(1000)
	,@SNSTopic NVARCHAR(1000)
	,@MessageAttribute NVARCHAR(MAX)
	,@MessageBody NVARCHAR(MAX)
	,@Status INT = 0
	,@PublishedOn DATETIME = NULL
	,@CreatedOn DATETIME = NULL
	,@NumberOfRetries INT = 0
	,@LastTriedOn DATETIME = NULL
	,@Remark NVARCHAR(MAX)
	,@Exception NVARCHAR(MAX)
AS
BEGIN
	DECLARE @CurrentDate DATETIME;

	SET @CurrentDate = GETDATE()


	INSERT INTO MessageQueueRequestsException(
		Source
		,SNSTopic
		,MessageAttribute
		,MessageBody
		,STATUS
		,PublishedOn
		,CreatedOn
		,NumberOfRetries
		,LastTriedOn
		,Remark
		,Exception
		)
	VALUES (
		@Source
		,@SNSTopic
		,@MessageAttribute
		,@MessageBody
		,@Status
		,@PublishedOn
		,@CreatedOn
		,@NumberOfRetries
		,@LastTriedOn
		,@Remark
		,@Exception
		)
END
GO
/****** Object:  StoredProcedure [dbo].[z_UpdateStatusInMessageQueueRequests]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[z_UpdateStatusInMessageQueueRequests]
	@MessageQueueRequestID INT
	,@Status INT = 0
	,@PublishedOn DATETIME = NULL	
	,@NumberOfRetries INT = 0
	,@LastTriedOn DATETIME = NULL
AS
BEGIN
	DECLARE @CurrentDate DATETIME;

	SET @CurrentDate = GETDATE()	

	UPDATE MessageQueueRequests 
	SET Status = @Status,
		PublishedOn = CASE WHEN @PublishedOn IS NULL THEN PublishedOn ELSE @PublishedOn END,
		LastTriedOn = CASE WHEN @LastTriedOn IS NULL THEN LastTriedOn ELSE @LastTriedOn END,
		NumberOfRetries = @NumberOfRetries
	WHERE MessageQueueRequestID = @MessageQueueRequestID

	IF @@ERROR <> 0
		RETURN 101 
	ELSE
		RETURN 102 
END
GO
/****** Object:  StoredProcedure [dbo].[z_UpdateStatusInMessageQueueRequestsException]    Script Date: 5/14/2019 5:03:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[z_UpdateStatusInMessageQueueRequestsException] @Source NVARCHAR(1000)
	,@SNSTopic NVARCHAR(1000)
	,@MessageAttribute NVARCHAR(MAX)
	,@MessageBody NVARCHAR(MAX)
	,@Status INT = 0
	,@PublishedOn DATETIME = NULL
	,@CreatedOn DATETIME = NULL
	,@NumberOfRetries INT = 0
	,@LastTriedOn DATETIME = NULL
	,@Remark NVARCHAR(MAX)
	,@Exception NVARCHAR(MAX)
AS
BEGIN
	DECLARE @CurrentDate DATETIME;

	SET @CurrentDate = GETDATE()


	INSERT INTO MessageQueueRequestsException(
		Source
		,SNSTopic
		,MessageAttribute
		,MessageBody
		,STATUS
		,PublishedOn
		,CreatedOn
		,NumberOfRetries
		,LastTriedOn
		,Remark
		,Exception
		)
	VALUES (
		@Source
		,@SNSTopic
		,@MessageAttribute
		,@MessageBody
		,@Status
		,@PublishedOn
		,@CreatedOn
		,@NumberOfRetries
		,@LastTriedOn
		,@Remark
		,@Exception
		)
END
GO
