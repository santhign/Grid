USE [Grid]
GO
/****** Object:  UserDefinedFunction [dbo].[ufnGetOrderNumber]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufnGetOrderNumber](@ID int, @Date DATETIME = NULL, @Word NVARCHAR(100) = 'GRID-')  
RETURNS NVARCHAR(1000) 
AS   
-- Returns the stock level for the product.  
BEGIN  
	IF(@Date = NULL)
	BEGIN
		SELECT @Date = GETDATE()
	END

    DECLARE @ret NVARCHAR(1000);  
    SELECT @ret = 'GRID-' + CAST(@ID AS nvarchar(50)) + CONVERT(NVARCHAR(10), @Date, 12)
    RETURN @ret;  
END; 

GO
/****** Object:  UserDefinedFunction [dbo].[ufnGetShippingNumber]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[ufnGetShippingNumber](@ID int)  
RETURNS NVARCHAR(1000) 
AS   
-- Returns the stock level for the product.  
BEGIN  
    DECLARE @ret NVARCHAR(1000);  
    SELECT @ret = 'GRIDSHIPPING-' + CAST(@ID AS NVARCHAR(20))
    RETURN @ret;  
END; 

GO
/****** Object:  StoredProcedure [dbo].[Admin_AuthenticateAdminUser]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Admin_CreateAdminUser]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Admin_GetAdminUserByID]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Admin_GetAllAdminRoles]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Admin_GetAllAdminUsers]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Admin_GetBannerDetails]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Admin_GetBSSRequestIDAndSubscriberSession]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Admin_GetConfigurationByKey]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Admin_GetConfigurations]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Admin_GetConfigValue]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Admin_GetCustomerListing]    Script Date: 4/12/2019 11:51:12 AM ******/
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
		CASE SMSSubscription WHEN 1 THEN 'Yes' ELSE 'No' END AS SMSSubscription,
		CASE EmailSubscription WHEN 1 THEN 'Yes' ELSE 'No' END AS EmailSubscription,
		CASE [Status] WHEN 1 THEN 'Active' ELSE 'InActive' END AS [Status]
	FROM Customers
END
GO
/****** Object:  StoredProcedure [dbo].[Admin_GetGenericConfigValue]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Admin_GetLookup]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Admin_GetNumberTypeCodes]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Admin_GetPageFAQ]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Admin_GetRequestIDForBSSAPI]    Script Date: 4/12/2019 11:51:12 AM ******/
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
									WHERE BSSCallNumbers.MobileNumber=@MobileNumber
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
/****** Object:  StoredProcedure [dbo].[Admin_GetServiceFee]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Admin_UpdateProfile]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[AdminUser_AuthenticateToken]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[AdminUser_CreateToken]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Catelog_BundleExists]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Catelog_CreateBundle]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Catelog_DeleteBundle]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Catelog_GetBundleById]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Catelog_GetBundlesListing]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Catelog_GetPromotionalBundle]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Catelog_GetSharedVASListing]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Catelog_GetSharedVASListing]
	
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
		CASE IsRecurring WHEN 1 THEN 'monthly' ELSE 'one-time' END  AS IsRecurring
	FROM Bundles INNER JOIN 
		(
			SELECT SUM(Data) AS TotalData, SUM(SMS) AS TotalSMS, SUM(Voice) AS TotalVoice, SUM(SubscriptionFee) AS TotalSubscriptionFee, Bundles.BundleID, IsRecurring
			FROM Bundles
				INNER JOIN BundlePlans ON Bundles.BundleID = BundlePlans.BundleID
				INNER JOIN Plans ON BundlePlans.PlanID = Plans.PlanID
			WHERE Plans.PlanType = 2 
			GROUP BY Bundles.BundleID, IsRecurring
		) BundlePlans ON BundlePlans.BundleID = Bundles.BundleID
	WHERE Bundles.IsCustomerSelectable = 1
		AND Bundles.Status = 1
		AND @CurrentDate BETWEEN ISNULL(Bundles.ValidFrom, @CurrentDate) AND ISNULL(Bundles.ValidTo, @CurrentDate)
END
GO
/****** Object:  StoredProcedure [dbo].[Catelog_GetVASListing]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Catelog_GetVASListing]
	
AS
BEGIN
	--Cannot bundle recurring and non recurring email together
	DECLARE @CurrentDate AS DATE
	SET @CurrentDate = CAST(GETDATE() AS DATE);
	SELECT 
		Bundles.BundleID AS VASID,
		'' AS BSSPlanCode,
		Bundles.PortalDescription,
		Bundles.PortalSummaryDescription,
		Bundles.PlanMarketingName,
		TotalData AS [Data],
		TotalSMS AS SMS,
		TotalVoice AS Voice,
		TotalSubscriptionFee AS SubscriptionFee,
		CASE IsRecurring WHEN 1 THEN 'monthly' ELSE 'one-time' END  AS IsRecurring
	FROM Bundles INNER JOIN 
		(
			SELECT SUM(Data) AS TotalData, SUM(SMS) AS TotalSMS, SUM(Voice) AS TotalVoice, SUM(SubscriptionFee) AS TotalSubscriptionFee, Bundles.BundleID, IsRecurring
			FROM Bundles
				INNER JOIN BundlePlans ON Bundles.BundleID = BundlePlans.BundleID
				INNER JOIN Plans ON BundlePlans.PlanID = Plans.PlanID
			WHERE Plans.PlanType = 1 
			GROUP BY Bundles.BundleID, IsRecurring
		) BundlePlans ON BundlePlans.BundleID = Bundles.BundleID
	WHERE Bundles.IsCustomerSelectable = 1
		AND Bundles.Status = 1
		AND @CurrentDate BETWEEN ISNULL(Bundles.ValidFrom, @CurrentDate) AND ISNULL(Bundles.ValidTo, @CurrentDate)
END
GO
/****** Object:  StoredProcedure [dbo].[Catelog_UpdateBundle]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[ChangeRequests_BuyVAS]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ChangeRequests_BuyVAS] @CustomerID INT
	,@Quantity INT
	,@PlanId INT
	,@MobileNumber VARCHAR(8)
AS
BEGIN
	DECLARE @CurrentDate DATE;

	SET @CurrentDate = CAST(GETDATE() AS DATE)

	BEGIN TRY
		BEGIN TRAN

		DECLARE @RequestTypeID INT

		SELECT @RequestTypeID = RequestTypeID
		FROM RequestTypes
		WHERE RequestType = 'Removal'

		DECLARE @AccountID INT

		SELECT @AccountID = AccountID
		FROM Accounts
		WHERE CustomerID = @CustomerID

		DECLARE @SubscriberID INT

		
		SELECT @SubscriberID = SubscriberID
		FROM Subscribers
		WHERE MobileNumber = @MobileNumber
			AND AccountID = @AccountID

		DECLARE @SubscriptionID INT
		DECLARE @BundleID INT

		SELECT @SubscriptionID = SubscriptionID
			,@BundleID = BundleID
		FROM Subscriptions
		WHERE SubscriberID = @SubscriberID
			AND PlanID = @PlanId

		INSERT INTO ChangeRequests (
			RequestTypeID
			,RequestReason
			)
		VALUES (
			@RequestTypeID
			,'Created From Order'
			);

		DECLARE @ChangeRequestID INT

		SET @ChangeRequestID = SCOPE_IDENTITY();

		INSERT INTO SubscriberRequests (
			ChangeRequestID
			,SubscriberID
			)
		SELECT @ChangeRequestID
			,SubscriberID
		FROM Subscribers
		WHERE AccountID = @AccountID
			AND MobileNumber = @MobileNumber

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
			,[PromotionID]
			)
		SELECT @ChangeRequestID
			,Plans.PlanID
			,GETDATE()
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
			,PromotionBundles.PromotionID
		FROM Bundles
		INNER JOIN BundlePlans ON Bundles.BundleID = BundlePlans.BundleID
		INNER JOIN Plans ON BundlePlans.PlanID = Plans.PlanID
		LEFT OUTER JOIN PromotionBundles ON Bundles.BundleID = PromotionBundles.BundleID
		LEFT OUTER JOIN Promotions ON PromotionBundles.PromotionID = Promotions.PromotionID
		--AND Promotions.PromotionCode = @PromotionCode  
		WHERE Bundles.BundleID = @BundleID

		INSERT INTO ChangeRequestCharges (
			ChangeRequestID
			,IsGSTIncluded
			,IsRecurring
			)
		SELECT @ChangeRequestID
			,IsGSTIncluded
			,IsRecurring
		FROM Plans
		WHERE PlanID = @PlanId

		INSERT INTO Subscriptions (
			SubscriberID
			,PlanID
			,PurchasedOn
			--,ActivatedOn
			,BSSPlanCode
			,PlanMarketingName
			--,SubscriptionFee
			--,ValidFrom
			--,ValidTo
			--,STATUS
			,BundleID
			,CreatedOn
			)
		SELECT @SubscriberID
			,PlanID
			,@CurrentDate
			--,NULL
			,BSSPlanCode
			,PlanMarketingName
			,@BundleID
			,@CurrentDate
		FROM Plans
		WHERE PlanID = @PlanId

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
/****** Object:  StoredProcedure [dbo].[ChangeRequests_InsertRemoveVAS]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ChangeRequests_InsertRemoveVAS] @CustomerID INT
	,@MobileNumber NVARCHAR(8)
	,@PlanId INT
AS
BEGIN
	DECLARE @CurrentDate DATE;

	SET @CurrentDate = CAST(GETDATE() AS DATE)

	DECLARE @RequestTypeID INT

	SELECT @RequestTypeID = RequestTypeID
	FROM RequestTypes
	WHERE RequestType = 'Removal'

	DECLARE @AccountID INT

	SELECT @AccountID = AccountID
	FROM Accounts
	WHERE CustomerID = @CustomerID

	DECLARE @SubscriberID INT

	SELECT @SubscriberID = SubscriberID
	FROM Subscribers
	WHERE MobileNumber = @MobileNumber
		AND AccountID = @AccountID

	DECLARE @SubscriptionID INT
	DECLARE @BundleID INT

	SELECT @SubscriptionID = SubscriptionID
		,@BundleID = BundleID
	FROM Subscriptions
	WHERE SubscriberID = @SubscriberID
		AND PlanID = @PlanId

	INSERT INTO ChangeRequests (
		RequestTypeID
		,RequestReason
		)
	VALUES (
		@RequestTypeID
		,'Created From Order'
		);

	DECLARE @ChangeRequestID INT

	SET @ChangeRequestID = SCOPE_IDENTITY();

	INSERT INTO SubscriberRequests (
		ChangeRequestID
		,SubscriberID
		)
	SELECT @ChangeRequestID
		,SubscriberID
	FROM Subscribers
	WHERE AccountID = @AccountID
		AND MobileNumber = @MobileNumber

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
		,[PromotionID]
		)
	SELECT @ChangeRequestID
		,Plans.PlanID
		,GETDATE()
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
		,PromotionBundles.PromotionID
	FROM Bundles
	INNER JOIN BundlePlans ON Bundles.BundleID = BundlePlans.BundleID
	INNER JOIN Plans ON BundlePlans.PlanID = Plans.PlanID
	LEFT OUTER JOIN PromotionBundles ON Bundles.BundleID = PromotionBundles.BundleID
	LEFT OUTER JOIN Promotions ON PromotionBundles.PromotionID = Promotions.PromotionID
	--AND Promotions.PromotionCode = @PromotionCode  
	WHERE Bundles.BundleID = @BundleID

	INSERT INTO ChangeRequestCharges (
		ChangeRequestID
		,IsGSTIncluded
		,IsRecurring
		)
	SELECT @ChangeRequestID
		,IsGSTIncluded
		,IsRecurring
	FROM Plans
	WHERE PlanID = @PlanId
END
GO
/****** Object:  StoredProcedure [dbo].[Config_GetValue]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Customer_AuthenticateCustomer]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_AuthenticateCustomer] 
	@Email NVARCHAR(255),
	@Password NVARCHAR(4000)	
AS
BEGIN

IF EXISTS (SELECT * FROM Customers WHERE Email=@Email)
BEGIN	
	IF EXISTS (SELECT * FROM Customers where Email=@Email AND Password=@Password)
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
		FROM Customers	where Email=@Email AND Password=@Password

		--Register Customer Log
		INSERT INTO CustomerLogTrail 
		(
			CustomerID,
			ActionDescription,
			ActionOn
		)
		SELECT 
			CustomerID,
			'Login to portal',
			GETDATE()
		FROM Customers 
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
/****** Object:  StoredProcedure [dbo].[Customer_AuthenticateToken]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Customer_CR_ChangePhoneRequest]    Script Date: 4/12/2019 11:51:12 AM ******/
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
		SET OrderNumber = dbo.ufnGetOrderNumber(@ChangeRequestID, @CurrentDate, DEFAULT)
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
/****** Object:  StoredProcedure [dbo].[Customer_CreateCustomer]    Script Date: 4/12/2019 11:51:12 AM ******/
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
			'Encrypted' as [Password],
			MobileNumber,
			ReferralCode,
			Nationality,
			Gender,
			CASE SMSSubscription WHEN 1 THEN 'Yes' ELSE 'No' END AS SMSSubscription,
			CASE EmailSubscription WHEN 1 THEN 'Yes' ELSE 'No' END AS EmailSubscription,
			CASE [Status] WHEN 1 THEN 'Active' ELSE 'InActive' END AS [Status]
		FROM Customers	where CustomerID=@CustomrID

		RETURN 100 -- creation success
END

ELSE
	BEGIN
		RETURN 108 -- EMAIL EXISTS
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Customer_CreateToken]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_CreateToken]
	@CustomerID INT,
	@Token nvarchar(1000)
AS 
BEGIN
	IF EXISTS(SELECT CustomerID FROM CustomerToken WHERE Token = @Token)
	BEGIN
		SELECT CustomerID,
			CreatedOn
		FROM CustomerToken
		WHERE Token = @Token
		RETURN 105 -- Already Exists
	END
	ELSE IF EXISTS(SELECT CustomerID FROM CustomerToken WHERE CustomerID = @CustomerID)
	BEGIN
		UPDATE CustomerToken SET Token = @Token,CreatedOn=GETDATE() WHERE CustomerID = @CustomerID
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
/****** Object:  StoredProcedure [dbo].[Customer_ForgotPassword]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Customer_GetCustomerByEmail]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Customer_SearchCustomers]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[Customer_SearchCustomers]
	@SearchValue NVARCHAR(500)
AS
BEGIN
	SELECT 
		Customers.CustomerID,
		[Name],
		Customers.MobileNumber,
		PlanName,
		ISNULL(AccountSubscribers.LineCount,0) AS [AdditionalLines],
		JoinedOn,
		IIF(Customers.[Status]=1,'Active','Inactive')[Status]
	FROM Customers 
		LEFT JOIN Accounts ON Customers.CustomerID=Accounts.CustomerID
		LEFT JOIN 
		(
			SELECT AccountID, PlanName
			FROM Subscribers 
				LEFT JOIN Subscriptions ON Subscriptions.SubscriptionID=Subscribers.SubscriberID
				LEFT JOIN Plans ON Plans.planID=Subscriptions.PlanID 
			WHERE IsPrimary = 1 
		)PrimarySubscribers ON PrimarySubscribers.AccountID =Accounts.AccountID
		LEFT JOIN 
		(
			SELECT AccountID, COUNT(*) AS LineCount
			FROM Subscribers 
				LEFT JOIN Subscriptions ON Subscriptions.SubscriptionID=Subscribers.SubscriberID
				LEFT JOIN Plans ON Plans.planID=Subscriptions.PlanID 
			GROUP BY AccountID
		)AccountSubscribers ON AccountSubscribers.AccountID =Accounts.AccountID
	WHERE Customers.Email LIKE '%' + @SearchValue + '%'
		OR Customers.MobileNumber LIKE '%' + @SearchValue + '%' 
		OR [Name] LIKE '%' + @SearchValue + '%' 
		OR BillingAccountNumber LIKE '%' + @SearchValue + '%'
END
GO
/****** Object:  StoredProcedure [dbo].[Customer_UpdateCustomerProfile]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customer_UpdateCustomerProfile] 
	@CustomerID INT,
	
	@Password NVARCHAR(4000),
	@MobileNumber NVARCHAR(15)
AS
BEGIN

IF EXISTS (SELECT * FROM Customers WHERE CustomerID=@CustomerID)
BEGIN	
		
		UPDATE Customers 
		SET Password = @Password,
			MobileNumber = @MobileNumber
		WHERE CustomerID = @CustomerID	

		RETURN 101 --Updated success
END
ELSE
	BEGIN
		RETURN 102 -- recor does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetCustomerReferralCode]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Customers_GetPlans]    Script Date: 4/12/2019 11:51:12 AM ******/
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
	SELECT 
		Customers.CustomerID, 
		Plans.PlanID, 
		Plans.PlanMarketingName,
		CASE WHEN Plans.IsRecurring = 0  
				   THEN  'One Time'
				   ELSE 'Monthly' 
		   END as 'SubscriptionType',
		Plans.IsRecurring,
		Subscribers.MobileNumber,
		Subscriptions.ValidTo AS 'ExpiryDate'
	FROM Customers 
		INNER JOIN Accounts ON Customers.CustomerID = Accounts.CustomerID
		INNER JOIN Subscribers ON Subscribers.AccountID = Accounts.AccountID
		INNER JOIN Subscriptions ON Subscriptions.SubscriberID = Subscribers.SubscriberID
		INNER JOIN Plans ON Subscriptions.PlanID = Plans.PlanID
	WHERE Customers.CustomerID = @CustomerID AND PlanType = @PlanType AND Subscribers.MobileNumber = ISNULL(@MobileNumber,Subscribers.MobileNumber)
	IF @@ROWCOUNT=0
	BEGIN
	RETURN 102	
	END
	ELSE RETURN 105
END
GO
/****** Object:  StoredProcedure [dbo].[Customers_GetSubscribers]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Customers_GetSubscribers] @CustomerID INT
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
			FROM Subscribers
			INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID
			LEFT OUTER JOIN (
				SELECT Subscribers.SubscriberID
					,LinkedSubscribers.DisplayName AS LinkedDisplayName
					,LinkedSubscribers.MobileNumber AS LinkedMobileNumber
				FROM Subscribers
				INNER JOIN Subscribers LinkedSubscribers ON Subscribers.LinkedSubscriberID = LinkedSubscribers.SubscriberID
				) LinkedSubscribers ON LinkedSubscribers.SubscriberID = Subscribers.SubscriberID
			WHERE CustomerID = @CustomerID

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
/****** Object:  StoredProcedure [dbo].[Customers_UpdateReferralCode]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Customers_UpdateReferralCode]
	@CustomerID INT,
	@ReferralCode NVARCHAR(255)
AS
BEGIN	
	IF EXISTS(SELECT * FROM Customers WHERE CustomerID = @CustomerID)
	BEGIN	
		IF EXISTS(SELECT * FROM Customers WHERE ReferralCode = @ReferralCode AND CustomerID <> @CustomerID)
		BEGIN 
			SELECT CustomerID FROM Customers WHERE ReferralCode = @ReferralCode
			RETURN 105 --record already exists
		END
		ELSE 
		BEGIN
			IF EXISTS (SELECT * FROM Orders WHERE ReferralCode = @ReferralCode)
			BEGIN
				RETURN 104
			END
			ELSE
			BEGIN
				UPDATE Customers SET ReferralCode = @ReferralCode WHERE CustomerID = @CustomerID
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
/****** Object:  StoredProcedure [dbo].[Customers_ValidateReferralCode]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[InsertIntoMessageQueueRequests]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[InsertIntoMessageQueueRequests] @Source NVARCHAR(1000)
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
	BEGIN TRY
		BEGIN TRAN

		DECLARE @CurrentDate DATETIME;
		SET @CurrentDate = GETDATE()
		 
		
		IF(@PublishedOn IS NULL)
		BEGIN
			SET @PublishedOn = @CurrentDate
		END
		
		IF(@CreatedOn IS NULL)
		BEGIN
			SET @CreatedOn = @CurrentDate
		END
		
		IF(@LastTriedOn IS NULL)
		BEGIN
			SET @LastTriedOn = @CurrentDate
		END
		
		INSERT INTO MessageQueueRequests 
		(Source,SNSTopic,MessageAttribute,MessageBody,Status,PublishedOn,CreatedOn,NumberOfRetries,LastTriedOn)
		VALUES (@Source, @SNSTopic, @MessageAttribute, @MessageBody, @Status, @PublishedOn,@CreatedOn, @NumberOfRetries, @LastTriedOn)

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
/****** Object:  StoredProcedure [dbo].[Order_CR_SIMReplacementRequest]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Order_CR_SIMReplacementRequest] @CustomerID INT
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

		IF EXISTS (
				SELECT *
				FROM Subscriptions
				WHERE SubscriberID = @SubscriberID
				)
		BEGIN
			INSERT INTO ChangeRequests (
				RequestTypeID
				,RequestReason
				)
			VALUES (
				@RequestTypeID
				,@Remarks
				);

			DECLARE @ChangeRequestID INT

			SET @ChangeRequestID = SCOPE_IDENTITY();

			DECLARE @OrderNumber NVARCHAR(500)

			SET @OrderNumber = dbo.ufnGetOrderNumber(@ChangeRequestID, @CurrentDate, DEFAULT)

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

			INSERT INTO DeliveryInformation (
				ShippingNumber
				,Name
				,Email
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
			SELECT dbo.ufnGetShippingNumber(@ChangeRequestID) 
				,AccountName
				,Email
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
			INNER JOIN Documents ON Documents.CustomerID = Accounts.CustomerID
			WHERE AccountID = @AccountID

			-- take the Delivery infomration id an update it in changerequest table
			DECLARE @DeliveryInformationID INT

			SET @DeliveryInformationID = SCOPE_IDENTITY();

			UPDATE ChangeRequests
			SET DeliveryInformationID = @DeliveryInformationID
			WHERE ChangeRequestID = @ChangeRequestID

			SELECT @ChangeRequestID AS ChangeRequestId
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
				,Documents.IdentityCardType AS IdentityCardType
				,Documents.IdentityCardNumber AS IdentityCardNumber
				,IsSameAsBilling
				,DeliveryInformation.ShippingUnit
				,DeliveryInformation.ShippingFloor
				,DeliveryInformation.ShippingBuildingNumber
				,DeliveryInformation.ShippingStreetName
				,DeliveryInformation.ShippingPostCode
				,DeliveryInformation.ShippingContactNumber
				,DeliveryInformation.AlternateRecipientName
				,DeliveryInformation.AlternateRecipientEmail
				,DeliveryInformation.AlternateRecipientContact
				,DeliveryInformation.AlternateRecioientIDNumber
				,DeliveryInformation.AlternateRecioientIDType
				,DeliveryInformation.PortalSlotID
				,DeliveryInformation.ScheduledDate
				,DeliveryInformation.DeliveryVendor
				,DeliveryInformation.DeliveryOn
				,DeliveryInformation.DeliveryTime
				,DeliveryInformation.VendorTrackingCode
				,DeliveryInformation.VendorTrackingUrl
				,DeliveryInformation.DeliveryFee
			FROM DeliveryInformation
			INNER JOIN ChangeRequests ON ChangeRequests.DeliveryInformationID = DeliveryInformation.DeliveryInformationID
			INNER JOIN SubscriberRequests ON SubscriberRequests.ChangeRequestID = ChangeRequests.ChangeRequestID
			INNER JOIN Subscribers ON Subscribers.SubscriberID = SubscriberRequests.SubscriberID
			INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID
			INNER JOIN Documents ON Documents.CustomerID = Accounts.CustomerID
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
/****** Object:  StoredProcedure [dbo].[Order_CreateSubscriber]    Script Date: 4/12/2019 11:51:12 AM ******/
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
				WHERE ServiceType = 'Promo' 
			END
	
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
/****** Object:  StoredProcedure [dbo].[Order_GetBSSAccountNumberByCustomerId]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_GetBSSAccountNumberByCustomerId]
	@CustomerID INT	
AS
BEGIN

SELECT AccountName FROM Accounts WHERE CustomerID=@CustomerID

IF @@rowcount>0
RETURN 105
ELSE RETURN 102
END
GO
/****** Object:  StoredProcedure [dbo].[Order_GetCustomerIDByOrderID]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Order_GetOrderBasicDetails]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Order_GetOrderDetails]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Order_GetOrderDetails] --[Order_GetOrderDetails] 16
	@OrderID INT
AS
BEGIN
	IF EXISTS(SELECT OrderID FROM Orders WHERE OrderID = @OrderID)
	BEGIN
		SELECT Orders.OrderID,
			OrderNumber, 
			OrderDate,
			BillingUnit,
			BillingFloor,
			BillingBuildingNumber,
			BillingBuildingName,
			BillingStreetName,
			BillingPostCode,
			BillingContactNumber,
			ReferralCode,
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
			OrderCharges.ServiceFee
		FROM Orders LEFT OUTER JOIN
			DeliveryInformation ON Orders.DeliveryInformationID = DeliveryInformation.DeliveryInformationID LEFT OUTER JOIN 
			(	
				SELECT OrderID, SUM(OrderCharges.ServiceFee) AS ServiceFee
				FROM OrderCharges
				GROUP BY OrderID
			)OrderCharges ON Orders.OrderID = OrderCharges.OrderID LEFT OUTER JOIN 
			DeliverySlots ON DeliveryInformation.PortalSlotID = DeliverySlots.PortalSlotID
		WHERE Orders.OrderID = @OrderID

		SELECT
			OrderBundle.BundleID,
			MobileNumber,
			DisplayName,
			IsPrimaryNumber,
			Bundles.PlanMarketingName,
			Bundles.PortalDescription,
			Bundles.PortalSummaryDescription
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

		SELECT PortalServiceName,
			OrderCharges.ServiceFee,
			OrderCharges.IsRecurring,
			OrderCharges.IsGSTIncluded
		FROM OrderCharges INNER JOIN 
			AdminServices ON OrderCharges.AdminServiceID = AdminServices.AdminServiceID
		WHERE OrderID = @OrderID

		UNION ALL

		SELECT PortalServiceName,
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
/****** Object:  StoredProcedure [dbo].[Order_SIMReplacementRequest]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Order_SIMReplacementRequest] @CustomerID INT
	,@MobileNumber NVARCHAR(8)
	
AS
BEGIN
	SELECT * FROM Subscriptions
END
GO
/****** Object:  StoredProcedure [dbo].[Order_SuspensionRequest]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Order_TerminationRequest]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Orders_CR_BuyVAS]    Script Date: 4/12/2019 11:51:12 AM ******/
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
		VALUES (
			@RequestTypeID
			,'CR - VAS'
			);

		DECLARE @ChangeRequestID INT

		SET @ChangeRequestID = SCOPE_IDENTITY();

		DECLARE @OrderNumber NVARCHAR(500)

		SET @OrderNumber = dbo.ufnGetOrderNumber(@ChangeRequestID, @CurrentDate, DEFAULT)

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

		WHILE (@Quantity > 0)
		BEGIN
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

			SET @Quantity = @Quantity - 1;

			IF @Quantity = 0
				BREAK
			ELSE
				CONTINUE
		END

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
			,'CR - VAS'
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
			--,ValidFrom
			--,ValidTo
			,BundleID
			,CreatedOn
			)
		SELECT @SubscriberID
			,Plans.PlanID
			,@CurrentDate
			--,NULL
			,BSSPlanCode
			,Plans.PlanMarketingName
			,SubscriptionFee
			,@BundleID
			,@CurrentDate
		FROM Bundles
		INNER JOIN BundlePlans ON Bundles.BundleID = BundlePlans.BundleID
		INNER JOIN Plans ON BundlePlans.PlanID = Plans.PlanID
		WHERE Bundles.BundleID = @BundleID

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
/****** Object:  StoredProcedure [dbo].[Orders_CR_InsertRemoveVAS]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_CR_InsertRemoveVAS] @CustomerID INT
	,@MobileNumber NVARCHAR(8)
	,@PlanID INT
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

		IF EXISTS (
				SELECT *
				FROM Subscriptions
				WHERE SubscriberID = @SubscriberID
					AND PlanID = @PlanID
				)
		BEGIN
			INSERT INTO ChangeRequests (
				RequestTypeID
				,RequestReason
				)
			VALUES (
				@RequestTypeID
				,'CR-RemoveVAS'
				);

			DECLARE @ChangeRequestID INT

			SET @ChangeRequestID = SCOPE_IDENTITY();

			UPDATE ChangeRequests
			SET OrderNumber = dbo.ufnGetOrderNumber(@ChangeRequestID, @CurrentDate, DEFAULT)
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
				)
			SELECT @ChangeRequestID
				,Plans.PlanID
				,@CurrentDate
				,BSSPlanCode
				,Plans.PlanMarketingName
				,0
			FROM Plans
			WHERE PlanID = @PlanID
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
/****** Object:  StoredProcedure [dbo].[Orders_CR_RaiseRequest]    Script Date: 4/12/2019 11:51:12 AM ******/
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
		VALUES (
			@RequestTypeID
			,@Remarks
			);

		DECLARE @ChangeRequestID INT

		SET @ChangeRequestID = SCOPE_IDENTITY();

		UPDATE ChangeRequests
		SET OrderNumber = dbo.ufnGetOrderNumber(@ChangeRequestID, @CurrentDate, DEFAULT)
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
/****** Object:  StoredProcedure [dbo].[Orders_CR_TerminationRequest]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Orders_CR_TerminationRequest] @CustomerID INT
	,@MobileNumber NVARCHAR(8)
	
AS
BEGIN
	BEGIN TRY
		BEGIN TRAN
			DECLARE @CurrentDate DATE;
			SET @CurrentDate = CAST(GETDATE() AS DATE)

			DECLARE @RequestTypeID INT
			SELECT @RequestTypeID = RequestTypeID
			FROM RequestTypes
			WHERE RequestType = 'Temination'

			DECLARE @AccountID INT
			DECLARE @SubscriberID INT

			SELECT @AccountID = Accounts.AccountID,  @SubscriberID = SubscriberID
			FROM Accounts INNER JOIN
				Subscribers ON Accounts.AccountID = Subscribers.AccountID
			WHERE CustomerID = @CustomerID
				AND MobileNumber = @MobileNumber

			IF EXISTS(SELECT * FROM Subscriptions WHERE SubscriberID = @SubscriberID)
			BEGIN

				INSERT INTO ChangeRequests (
					RequestTypeID
					,RequestReason
					)
				VALUES (
					@RequestTypeID
					,'CR-Termination'
					);

				DECLARE @ChangeRequestID INT
				SET @ChangeRequestID = SCOPE_IDENTITY();

				INSERT INTO SubscriberRequests (
					ChangeRequestID
					,SubscriberID
					)
				VALUES
				(
					@ChangeRequestID
					,@SubscriberID)

				--INSERT INTO [dbo].[OrderSubscriptions] (
				--	[ChangeRequestID]
				--	,[PlanID]
				--	,[PurchasedOn]
				--	,[BSSPlanCode]
				--	,[PlanMarketingName]
				--	,[Status]
				--	)
				--SELECT @ChangeRequestID
				--	,Plans.PlanID
				--	,GETDATE()
				--	,BSSPlanCode
				--	,Plans.PlanMarketingName
				--	,0
				--FROM Plans 
				--WHERE PlanID = @PlanID
			END
			ELSE
			BEGIN
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
/****** Object:  StoredProcedure [dbo].[Orders_CR_UpdateCRShippingDetails]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_CR_UpdateCRShippingDetails]  
 @ChangeRequestID INT,  
 @Postcode NVARCHAR(50),  
 @BlockNumber NVARCHAR(50),  
 @Unit NVARCHAR(50),  
 @Floor NVARCHAR(50),  
 @BuildingName NVARCHAR(255),  
 @StreetName NVARCHAR(255),  
 @ContactNumber NVARCHAR(255),  
 @IsBillingSame INT,  
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
      [IsSameAsBilling] = @IsBillingSame  
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
   FROM DeliveryInformation 
    INNER JOIN ChangeRequests ON ChangeRequests.DeliveryInformationID = DeliveryInformation.DeliveryInformationID
	--INNER JOIN SubscriberRequests ON SubscriberRequests.ChangeRequestID = ChangeRequests.ChangeRequestID
	--INNER JOIN Subscribers ON Subscribers.SubscriberID = SubscriberRequests.SubscriberID
	--INNER JOIN Accounts ON Subscribers.AccountID = Accounts.AccountID
	--INNER JOIN Documents ON Documents.CustomerID = Accounts.CustomerID
	WHERE ChangeRequests.ChangeRequestID = @ChangeRequestID  
  END  
  
  --DELETE SubscriberCharges   
  --FROM SubscriberCharges   
  -- INNER JOIN OrderSubscribers ON SubscriberCharges.OrderSubscriberID = OrderSubscribers.OrderSubscriberID   
  --WHERE ReasonType = 'Orders - Delivery' AND OrderID = @OrderID  
    
  --INSERT INTO SubscriberCharges  
  --(  
  -- OrderSubscriberID,  
  -- AdminServiceID,  
  -- ServiceFee,  
  -- IsGSTIncluded,  
  -- IsRecurring,  
  -- ReasonType,  
  -- ReasonID  
  --)  
  --SELECT   
  -- OrderSubscriberID,  
  -- AdminServiceID,  
  -- ISNULL(ServiceFee, 0),  
  -- IsGSTIncluded,  
  -- IsRecurring,  
  -- 'Orders - Delivery',  
  -- @OrderID  
  --FROM OrderSubscribers CROSS JOIN   
  -- AdminServices   
  --WHERE OrderID = @OrderID   
  -- AND AdminServices.ServiceType = 'Delivery'  
  -- AND ISNULL(ServiceFee, 0) > 0  
  
  --UPDATE Accounts SET  
  -- ShippingFloor = @Floor,  
  -- ShippingUnit = @Unit,  
  -- ShippingBuildingNumber = @BlockNumber,  
  -- ShippingBuildingName = @BuildingName,  
  -- ShippingStreetName = @StreetName,  
  -- ShippingPostCode = @Postcode,  
  -- ShippingContactNumber = @ContactNumber  
  --WHERE CustomerID = @CustomerID   
  RETURN 101 --Updated success  
 END  
 ELSE  
 BEGIN    
  RETURN 102 --does not exist  
 END  
END  
GO
/****** Object:  StoredProcedure [dbo].[Orders_CreateOrder]    Script Date: 4/12/2019 11:51:12 AM ******/
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
	IF EXISTS(SELECT OrderID FROM Orders INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID WHERE CustomerID = @CustomerID AND Orders.OrderStatus = 0)
	BEGIN
		SELECT @OrderID = OrderID FROM Orders INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID WHERE CustomerID = @CustomerID AND Orders.OrderStatus = 0

		IF EXISTS(SELECT * FROM [OrderSubscriptions] INNER JOIN 
				ChangeRequests ON [OrderSubscriptions].ChangeRequestID = ChangeRequests.ChangeRequestID INNER JOIN 
				OrderSubscriberChangeRequests ON ChangeRequests.ChangeRequestID = OrderSubscriberChangeRequests.ChangeRequestID INNER JOIN 
				OrderSubscribers ON OrderSubscriberChangeRequests.OrderSubscriberID = OrderSubscribers.OrderSubscriberID
			WHERE OrderID = @OrderID AND BundleID = @BundleID)
		BEGIN
			SELECT @OrderID AS OrderID, 'OldOrder' AS Status
		END
		ELSE
		BEGIN 
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
				   ([OrderNumber]
				   ,[AccountID]
				   ,[ReferralCode]
				   ,[OrderDate]
				   ,[ProcessedOn]
				   ,[OrderStatus])
			 SELECT 'GRID-' + CAST(AccountID AS nvarchar(50)) + CONVERT(NVARCHAR(10), @CurrentDate, 12),
				AccountID,
				@ReferralCode,
				@CurrentDate,
				GETDATE(),
				0
			FROM Customers INNER JOIN 
				Accounts ON Customers.CustomerID = Accounts.CustomerID
					AND Accounts.IsPrimary = 1
			WHERE Customers.CustomerID = @CustomerID

			SET @OrderID = SCOPE_IDENTITY();
			Print @OrderID

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
		COMMIT TRAN
		SELECT @OrderID AS OrderID, 'NewOrder' AS Status
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN
	END CATCH
	select @@ERROR as error
	
	IF @@ERROR<>0

	select 107

	ELSE select 100

END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetAvailableSlots]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_GetAvailableSlots]
AS
BEGIN
	DECLARE @Margin INT
	SELECT @Margin = CAST(ConfigValue AS int) FROM Config WHERE ConfigKey = 'DeliveryMarginInDays'
	SELECT PortalSlotID,
		SlotDate,
		SlotFromTime,
		SlotToTime,
		CAST(SlotFromTime AS nvarchar(8)) + ' - ' +  CAST(SlotToTime AS nvarchar(8)) AS Slot,
		AdditionalCharge
	FROM DeliverySlots
	WHERE IsActive = 1 
		AND Capacity > UsedQuantity
		AND SlotDate > CAST(DATEADD(d, @Margin, GETDATE()) AS DATE)
	ORDER BY SlotDate, SlotFromTime
	IF @@ROWCOUNT=0
	BEGIN
	RETURN 102	
	END
	ELSE RETURN 105
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetCheckoutRequestDetails]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_GetCheckoutRequestDetails]
	@Source NVARCHAR(50),
	@SourceID INT, -- GRID OrderID
	@MPGSOrderID NVARCHAR(20), -- MPGS OrderID
	@CheckOutSessionID NVARCHAR(50),
	@SuccessIndicator NVARCHAR(20),
	@CheckoutVersion NVARCHAR(20)
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
	) Fee

	IF EXISTS(SELECT * FROM CheckoutRequests WHERE SourceID = @SourceID AND SourceType = @Source AND MPGSOrderID=@MPGSOrderID)
	BEGIN
		UPDATE CheckoutRequests SET Amount = @Amount, CreatedOn = GETDATE() WHERE SourceID = @SourceID AND SourceType = @Source
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
			@CheckoutVersion,
			@Amount,
			GETDATE(),
			'Created'
		)
	END
	SELECT Amount FROM CheckoutRequests WHERE SourceID = @SourceID AND SourceType = @Source

	IF @@ROWCOUNT >0

	RETURN 105

	ELSE RETURN 102
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetOrderShipmentDetails]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_GetOrderShipmentDetails]
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
/****** Object:  StoredProcedure [dbo].[Orders_GetOrderSubscribers]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_GetOrderSubscribers]
	@OrderID INT
AS
BEGIN	
	IF EXISTS(SELECT * FROM Orders WHERE OrderID = @OrderID)
	BEGIN
		SELECT MobileNumber FROM OrderSubscribers
		WHERE OrderID = @OrderID
		RETURN 105 --record exists
	END
	ELSE
	BEGIN		
		RETURN 109 --does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_GetPendingOrderDetails]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Orders_ProcessPayment]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_ProcessPayment] 
--[Orders_ProcessPayment] '62563ac347', '011A7Z', NULL, NULL, 40, '123123XXXX839247239', 'CREDIT', 'MASTERCARD', NULL, 'MASTERCARD', 'Stratagile', 2021, 12, 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjM4IiwibmJmIjoxNTU0MTg5MzE2LCJleHAiOjE1NTQ3OTQxMTYsImlhdCI6MTU1NDE4OTMxNn0.YyfU-7B_raIqEmFyVizpACUS-ORmnGkX3szBvsHPDgk', 'CAPTURED', 'SUCCESS', 'APPROVED', '0:0:0:1', 1
	@MPGSOrderID NVARCHAR(20),
	@TransactionID NVARCHAR(20),
	@PaymentRequest NVARCHAR(MAX)=NULL,
    @PaymentResponse NVARCHAR(MAX)=NULL,
    @Amount FLOAT,
    @MaskedCardNumber NVARCHAR(255),
    @CardFundMethod NVARCHAR(255),
    @CardBrand NVARCHAR(255),
    @CardType NVARCHAR(255)=NULL,
    @CardIssuer NVARCHAR(255),
    @CardHolderName NVARCHAR(255),
	@ExpiryYear INT,
	@ExpiryMonth INT,
    @Token NVARCHAR(255),
    @PaymentStatus NVARCHAR(255),
	@ApiResult NVARCHAR(20),
	@GatewayCode NVARCHAR(20),
	@CustomerIP NVARCHAR(255),
	@PaymentMethodSubscription INT
AS
BEGIN
	DECLARE @AccountID INT
	DECLARE @OrderID INT
	
	SELECT @OrderID = SourceId from CheckoutRequests WHERE MPGSOrderID = @MPGSOrderID	
	
	SELECT @AccountID = AccountID FROM Orders WHERE OrderID = @OrderID

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
        ,[PaymentOn])
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
		GETDATE())
	SET @PaymentID = SCOPE_IDENTITY();

	UPDATE Orders SET PaymentID = @PaymentID, FinalPrice = @Amount, OrderStatus = 1, ProcessedOn = GETDATE()
	WHERE OrderID = @OrderID
	INSERT INTO OrderStatusLog (OrderID, OrderStatus, UpdatedOn) VALUES (@OrderID, 1
	
	--'PaymentProcessed' -- verify this  type mismatch
	
	, GETDATE())

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
        ,[BundleID])
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
        ,[BundleID]
	FROM Subscribers INNER JOIN 
		OrderSubscribers ON Subscribers.RefOrderSubscriberID = OrderSubscribers.OrderSubscriberID INNER JOIN
		OrderSubscriberChangeRequests ON OrderSubscriberChangeRequests.OrderSubscriberID = OrderSubscribers.OrderSubscriberID INNER JOIN
		ChangeRequests ON OrderSubscriberChangeRequests.ChangeRequestID = ChangeRequests.ChangeRequestID INNER JOIN
		OrderSubscriptions ON ChangeRequests.ChangeRequestID = OrderSubscriptions.ChangeRequestID
	WHERE OrderID = @OrderID
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
	RETURN 120 -- transaction updation failed
	ELSE
	RETURN 121 -- transaction updation success
	
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_RemoveAdditionalSubscriber]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_RemoveAdditionalSubscriber]
	@OrderID INT,
	@MobileNumber NVARCHAR(50)
AS
BEGIN
	IF EXISTS(SELECT * FROM Orders WHERE OrderStatus = 0 AND OrderID = @OrderID)
	BEGIN
		IF EXISTS(SELECT * FROM OrderSubscribers WHERE OrderID = @OrderID AND MobileNumber = @MobileNumber)
		BEGIN
			DECLARE @IsPrimary INT
			SELECT @IsPrimary = OrderSubscribers.IsPrimaryNumber FROM OrderSubscribers 
			WHERE OrderID = @OrderID AND MobileNumber = @MobileNumber
			IF(@IsPrimary = 1)
			BEGIN
				RETURN 122 --primary number cannot be deleted
			END
			ELSE
			BEGIN
				DELETE FROM OrderSubscribers 
				WHERE OrderID = @OrderID AND MobileNumber = @MobileNumber
				RETURN 103 --deleted
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
/****** Object:  StoredProcedure [dbo].[Orders_RollBackOldUnfinishedOrder]    Script Date: 4/12/2019 11:51:12 AM ******/
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

	-- revice for audit entry  for the action
	DELETE FROM OrderSubscriptions where @OrderID=@OrderID 

	DELETE FROM OrderStatusLog WHERE OrderID=@OrderID

	DELETE FROM Orders WHERE OrderID =@OrderID	-- where OrderStatus=

	IF @@ERROR<>0

	RETURN 104 

	ELSE 

	RETURN 103

	END

END
 
GO
/****** Object:  StoredProcedure [dbo].[Orders_UpdateBSSCallNumbers]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Orders_UpdateCheckoutResponse]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateCheckoutResponse]
	@MPGSOrderID NVARCHAR(20),	
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
			UPDATE Orders SET OrderStatus = 5 WHERE OrderID = @SourceID
			INSERT INTO OrderStatusLog (OrderID, OrderStatus, UpdatedOn) VALUES (@SourceID, 5, GETDATE())
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
/****** Object:  StoredProcedure [dbo].[Orders_UpdateCheckoutWebhookNotification]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateCheckoutWebhookNotification]
	@MPGSOrderID NVARCHAR(20),
	@TransactionID NVARCHAR(20),
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
			UPDATE Orders SET OrderStatus = 5 WHERE OrderID = @SourceID
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
/****** Object:  StoredProcedure [dbo].[Orders_UpdateOrderBasicDetails]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateOrderBasicDetails]
	@OrderID INT,
	@IDType NVARCHAR(50),
	@IDNumber NVARCHAR(50),
	@IDImageUrl NVARCHAR(255),
	@NameInNRIC NVARCHAR(255),
	@Gender NVARCHAR(255),
	@DOB DATE,
	@ContactNumber NVARCHAR(255),
	@Nationality NVARCHAR(255),
	@IDImageUrlBack NVARCHAR(255)
AS
BEGIN
	DECLARE @DocumentID INT
	DECLARE @CustomerID INT
	SELECT @CustomerID = CustomerID FROM Orders INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID WHERE OrderID = @OrderID
	IF EXISTS(SELECT * FROM Orders WHERE OrderID = @OrderID)
	BEGIN
		IF EXISTS(SELECT * FROM Documents WHERE IdentityCardType = @IDType AND CustomerID = @CustomerID)
		BEGIN
			SELECT @DocumentID = DocumentID FROM Documents WHERE IdentityCardType = @IDType AND CustomerID = @CustomerID
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
			Nationality = @Nationality,
			Gender = @Gender,
			NameInNRIC = @NameInNRIC,
			DOB = @DOB,
			BillingContactNumber = BillingContactNumber
		WHERE OrderID = @OrderID

		UPDATE Customers SET
			Name = @NameInNRIC,
			Gender = @Gender,
			Nationality = @Nationality,
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
/****** Object:  StoredProcedure [dbo].[Orders_UpdateOrderBillingDetails]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateOrderBillingDetails]
	@OrderID INT,
	@Postcode NVARCHAR(50),
	@BlockNumber NVARCHAR(50),
	@Unit NVARCHAR(50),
	@Floor NVARCHAR(50),
	@BuildingName NVARCHAR(255),
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
/****** Object:  StoredProcedure [dbo].[Orders_UpdateOrderLOADetails]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Orders_UpdateOrderShippingDetails]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateOrderShippingDetails]
	@OrderID INT,
	@Postcode NVARCHAR(50),
	@BlockNumber NVARCHAR(50),
	@Unit NVARCHAR(50),
	@Floor NVARCHAR(50),
	@BuildingName NVARCHAR(255),
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
				'GRIDSHIPPING-' + CAST(@OrderID AS NVARCHAR(20)),
				NameInNRIC,
				Customers.Email,
				IdentityCardNumber, 
				IdentityCardType,
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
/****** Object:  StoredProcedure [dbo].[Orders_UpdateOrderSubscriptions]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateOrderSubscriptions]
	@OrderID INT,
	@ContactNumber NVARCHAR(255) = NULL,
	@Terms INT,
	@PaymentSubscription INT,
	@PromotionMessage INT
AS
BEGIN
	DECLARE @CustomerID INT
	SELECT @CustomerID = CustomerID FROM Orders INNER JOIN Accounts ON Orders.AccountID = Accounts.AccountID WHERE OrderID = @OrderID
	IF EXISTS(SELECT * FROM Orders WHERE OrderID = @OrderID)
	BEGIN
		UPDATE Orders SET
			Terms = @Terms,
			PaymentAcknowledgement = @PaymentSubscription,
			PromotionSubscription = @PromotionMessage
		WHERE OrderID = @OrderID

		INSERT INTO CustomerLogTrail (CustomerID, ActionDescription, ActionOn) 
		SELECT @CustomerID, 'Updated the primary contact number from ' + MobileNumber  + ' to ' + @ContactNumber, GETDATE() FROM Customers WHERE CustomerID = @CustomerID

		UPDATE Customers SET MobileNumber = @ContactNumber WHERE CustomerID = @CustomerID
		RETURN 101 --Updated success
	END
	ELSE
	BEGIN		
		RETURN 102 --does not exist
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Orders_UpdateSubscriberNumber]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[Orders_UpdateSubscriberPortingNumber]    Script Date: 4/12/2019 11:51:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Orders_UpdateSubscriberPortingNumber]
	@OrderID INT,
	@OldMobileNumber NVARCHAR(8),
	@NewMobileNumber NVARCHAR(8),
	@DisplayName NVARCHAR(50),
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
/****** Object:  StoredProcedure [dbo].[Orders_ValidateReferralCode]    Script Date: 4/12/2019 11:51:12 AM ******/
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
/****** Object:  StoredProcedure [dbo].[z_GetDBD]    Script Date: 4/12/2019 11:51:12 AM ******/
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
