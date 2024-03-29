USE [Grid]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 5/14/2019 5:04:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AccountChangeRequests]    Script Date: 5/14/2019 5:04:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccountChangeRequests](
	[OrderChangeRequestID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NULL,
	[ChangeRequestID] [int] NULL,
 CONSTRAINT [PK_OrderChangeRequests] PRIMARY KEY CLUSTERED 
(
	[OrderChangeRequestID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AccountInvoices]    Script Date: 5/14/2019 5:04:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccountInvoices](
	[InvoiceID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NULL,
	[BSSBillId] [nvarchar](50) NULL,
	[InvoiceName] [nvarchar](255) NULL,
	[InvoiceUrl] [nvarchar](255) NULL,
	[FinalAmount] [float] NULL,
	[Remarks] [nvarchar](255) NULL,
	[OrderStatus] [int] NOT NULL,
	[PaymentSourceID] [int] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [int] NULL,
	[PaymentID] [int] NULL,
	[PaidOn] [datetime] NULL,
	[IsPaid] [int] NOT NULL,
 CONSTRAINT [PK_AccountInvoices] PRIMARY KEY CLUSTERED 
(
	[InvoiceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Accounts]    Script Date: 5/14/2019 5:04:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Accounts](
	[AccountID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NULL,
	[BillingAccountNumber] [nvarchar](255) NULL,
	[BSSProfileID] [nvarchar](255) NULL,
	[AccountName] [nvarchar](255) NULL,
	[Email] [nvarchar](255) NULL,
	[Status] [int] NOT NULL,
	[IsFinancialHold] [int] NOT NULL,
	[BillingFloor] [nvarchar](255) NULL,
	[BillingUnit] [nvarchar](255) NULL,
	[BillingBuildingName] [nvarchar](255) NULL,
	[BillingBuildingNumber] [nvarchar](255) NULL,
	[BillingStreetName] [nvarchar](255) NULL,
	[BillingPostCode] [nvarchar](255) NULL,
	[BillingContactNumber] [nvarchar](255) NULL,
	[ShippingFloor] [nvarchar](255) NULL,
	[ShippingUnit] [nvarchar](255) NULL,
	[ShippingBuildingName] [nvarchar](255) NULL,
	[ShippingBuildingNumber] [nvarchar](255) NULL,
	[ShippingStreetName] [nvarchar](255) NULL,
	[ShippingPostCode] [nvarchar](255) NULL,
	[ShippingContactNumber] [nvarchar](255) NULL,
	[IsPrimary] [int] NOT NULL,
 CONSTRAINT [PK_Accounts] PRIMARY KEY CLUSTERED 
(
	[AccountID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AccountSubscriptionLog]    Script Date: 5/14/2019 5:04:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccountSubscriptionLog](
	[AccountSubscriptionLogID] [int] IDENTITY(1,1) NOT NULL,
	[AccountSubscriptionID] [int] NULL,
	[Status] [int] NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Source] [int] NOT NULL,
	[RefChangeRequestID] [int] NULL,
 CONSTRAINT [PK_AccountSubscriptionLog] PRIMARY KEY CLUSTERED 
(
	[AccountSubscriptionLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AccountSubscriptions]    Script Date: 5/14/2019 5:04:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccountSubscriptions](
	[AccountSubscriptionID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NULL,
	[PlanID] [int] NULL,
	[PurchasedOn] [datetime] NULL,
	[ActivatedOn] [datetime] NULL,
	[BSSPlanCode] [nvarchar](255) NULL,
	[PlanMarketingName] [nvarchar](255) NULL,
	[SubscriptionFee] [float] NULL,
	[ValidFrom] [date] NULL,
	[ValidTo] [date] NULL,
	[Status] [int] NOT NULL,
	[BundleID] [int] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[RefOrderSubscriptionID] [int] NULL,
	[LastUpdatedOn] [datetime] NULL,
 CONSTRAINT [PK_AccountSubscriptions] PRIMARY KEY CLUSTERED 
(
	[AccountSubscriptionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AdHocPayments]    Script Date: 5/14/2019 5:04:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdHocPayments](
	[AdHocPaymentID] [int] IDENTITY(1,1) NOT NULL,
	[SourceType] [nvarchar](50) NULL,
	[SourceID] [int] NULL,
	[AdminServiceID] [int] NULL,
	[ServiceFee] [float] NULL,
	[IsGSTIncluded] [int] NULL,
	[IsRecurring] [int] NULL,
	[ReasonType] [nvarchar](255) NULL,
	[AccountInvoiceID] [int] NULL,
	[UniqueIdentifier] [nvarchar](255) NULL,
 CONSTRAINT [PK_AdHocPayments] PRIMARY KEY CLUSTERED 
(
	[AdHocPaymentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AdminAccessTokens]    Script Date: 5/14/2019 5:04:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdminAccessTokens](
	[AdminAccessTokenID] [int] IDENTITY(1,1) NOT NULL,
	[AdminUserID] [int] NULL,
	[CustomerID] [int] NULL,
	[Token] [nvarchar](255) NOT NULL,
	[IsUsed] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_AdminAccessTokens] PRIMARY KEY CLUSTERED 
(
	[AdminAccessTokenID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AdminServices]    Script Date: 5/14/2019 5:04:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdminServices](
	[AdminServiceID] [int] IDENTITY(1,1) NOT NULL,
	[PortalServiceName] [nvarchar](255) NULL,
	[ServiceCode] [int] NULL,
	[ServiceType] [nvarchar](50) NULL,
	[ServiceFee] [float] NULL,
	[IsGSTIncluded] [int] NOT NULL,
	[IsRecurring] [int] NOT NULL,
	[ValidFrom] [date] NULL,
	[ValidTo] [date] NULL,
	[ChargeAt] [int] NOT NULL,
	[CreatedBy] [int] NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_AdminServices] PRIMARY KEY CLUSTERED 
(
	[AdminServiceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AdminUserLogTrail]    Script Date: 5/14/2019 5:04:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdminUserLogTrail](
	[AdminUserLogID] [int] IDENTITY(1,1) NOT NULL,
	[AdminUserID] [int] NULL,
	[ActionSource] [nvarchar](255) NULL,
	[ActionDescription] [nvarchar](255) NULL,
	[ActionOn] [datetime] NOT NULL,
 CONSTRAINT [PK_AdminUserLogTrail] PRIMARY KEY CLUSTERED 
(
	[AdminUserLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AdminUsers]    Script Date: 5/14/2019 5:04:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdminUsers](
	[AdminUserID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NULL,
	[Email] [nvarchar](255) NULL,
	[Password] [nvarchar](4000) NULL,
	[DepartmentID] [int] NULL,
	[OfficeID] [int] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[RoleID] [int] NULL,
	[CreatedBy] [int] NULL,
 CONSTRAINT [PK_AdminUsers] PRIMARY KEY CLUSTERED 
(
	[AdminUserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AdminUserTokens]    Script Date: 5/14/2019 5:04:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdminUserTokens](
	[AdminUserTokenID] [int] IDENTITY(1,1) NOT NULL,
	[AdminUserID] [int] NULL,
	[Token] [nvarchar](1000) NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_AdminUserTokens] PRIMARY KEY CLUSTERED 
(
	[AdminUserTokenID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[APIReferences]    Script Date: 5/14/2019 5:04:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[APIReferences](
	[APIReferenceID] [int] IDENTITY(1,1) NOT NULL,
	[APISourceName] [nvarchar](255) NULL,
	[AdminAccessAllowed] [int] NOT NULL,
 CONSTRAINT [PK_APIReferences] PRIMARY KEY CLUSTERED 
(
	[APIReferenceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Banners]    Script Date: 5/14/2019 5:04:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Banners](
	[BannerID] [int] IDENTITY(1,1) NOT NULL,
	[LocationID] [int] NULL,
	[BannerName] [nvarchar](50) NULL,
	[BannerUrl] [nvarchar](255) NULL,
	[UrlType] [int] NOT NULL,
	[BannerImage] [nvarchar](255) NULL,
	[ValidFrom] [date] NULL,
	[ValidTo] [date] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_Banners] PRIMARY KEY CLUSTERED 
(
	[BannerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BSSCallLogs]    Script Date: 5/14/2019 5:04:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BSSCallLogs](
	[BSSCallLogID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NULL,
	[Source] [nvarchar](50) NULL,
	[APIName] [nvarchar](50) NULL,
	[RequestID] [nvarchar](50) NULL,
	[UserID] [nvarchar](50) NULL,
	[CalledOn] [datetime] NOT NULL,
 CONSTRAINT [PK_BSSCallLogs] PRIMARY KEY CLUSTERED 
(
	[BSSCallLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BSSCallNumbers]    Script Date: 5/14/2019 5:04:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BSSCallNumbers](
	[BSSCallNumberID] [int] IDENTITY(1,1) NOT NULL,
	[BSSCallLogID] [int] NULL,
	[MobileNumber] [nvarchar](50) NULL,
	[RetrievedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_BSSCallNumbers] PRIMARY KEY CLUSTERED 
(
	[BSSCallNumberID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BundlePlans]    Script Date: 5/14/2019 5:04:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BundlePlans](
	[BundlePlanID] [int] IDENTITY(1,1) NOT NULL,
	[BundleID] [int] NULL,
	[PlanID] [int] NULL,
	[DiscountPercentage] [float] NULL,
	[DurationUOM] [nvarchar](255) NULL,
	[DurationValue] [int] NULL,
 CONSTRAINT [PK_BundlePlans] PRIMARY KEY CLUSTERED 
(
	[BundlePlanID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bundles]    Script Date: 5/14/2019 5:04:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bundles](
	[BundleID] [int] IDENTITY(1,1) NOT NULL,
	[BundleName] [nvarchar](255) NULL,
	[PlanMarketingName] [nvarchar](255) NULL,
	[PortalSummaryDescription] [nvarchar](max) NULL,
	[PortalDescription] [nvarchar](max) NULL,
	[IsCustomerSelectable] [int] NOT NULL,
	[ValidFrom] [date] NULL,
	[ValidTo] [date] NULL,
	[Status] [int] NOT NULL,
	[HasBuddyPromotion] [int] NOT NULL,
	[IsBuddyBundle] [int] NOT NULL,
	[PlanPriorityOrder] [int] NOT NULL,
	[BundleType] [int] NOT NULL,
 CONSTRAINT [PK_Bundles] PRIMARY KEY CLUSTERED 
(
	[BundleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ChangeRequestCharges]    Script Date: 5/14/2019 5:04:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChangeRequestCharges](
	[ChangeRequestChargeID] [int] IDENTITY(1,1) NOT NULL,
	[ChangeRequestID] [int] NULL,
	[AdminServiceID] [int] NULL,
	[ServiceFee] [float] NULL,
	[IsGSTIncluded] [int] NOT NULL,
	[IsRecurring] [int] NOT NULL,
	[ReasonType] [nvarchar](50) NULL,
	[ReasonID] [int] NULL,
 CONSTRAINT [PK_ChangeRequestCharges] PRIMARY KEY CLUSTERED 
(
	[ChangeRequestChargeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ChangeRequests]    Script Date: 5/14/2019 5:04:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChangeRequests](
	[ChangeRequestID] [int] IDENTITY(1,1) NOT NULL,
	[OrderNumber] [nvarchar](50) NULL,
	[RequestTypeID] [int] NOT NULL,
	[RequestOn] [datetime] NOT NULL,
	[PaymentID] [int] NULL,
	[FinalPrice] [float] NULL,
	[DeliveryInformationID] [int] NULL,
	[BillingFloor] [nvarchar](255) NULL,
	[BillingUnit] [nvarchar](255) NULL,
	[BillingBuildingName] [nvarchar](255) NULL,
	[BillingBuildingNumber] [nvarchar](255) NULL,
	[BillingStreetName] [nvarchar](255) NULL,
	[BillingPostCode] [nvarchar](255) NULL,
	[BillingContactNumber] [nvarchar](255) NULL,
	[OrderStatus] [int] NOT NULL,
	[RefAdminUserID] [int] NULL,
	[FromDate] [date] NULL,
	[ToDate] [date] NULL,
	[RequestReason] [nvarchar](255) NULL,
	[InvoiceNumber] [nvarchar](50) NULL,
	[InvoiceUrl] [nvarchar](255) NULL,
	[ProcessedOn] [datetime] NULL,
	[IsPaid] [int] NOT NULL,
 CONSTRAINT [PK_ChangeRequests] PRIMARY KEY CLUSTERED 
(
	[ChangeRequestID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ChangeRequestVouchers]    Script Date: 5/14/2019 5:04:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChangeRequestVouchers](
	[ChangeRequestVoucherID] [int] IDENTITY(1,1) NOT NULL,
	[ChangeRequestID] [int] NULL,
	[VoucherID] [int] NULL,
 CONSTRAINT [PK_CHangeRequestVouchers] PRIMARY KEY CLUSTERED 
(
	[ChangeRequestVoucherID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CheckoutRequests]    Script Date: 5/14/2019 5:04:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CheckoutRequests](
	[CheckoutRequestID] [int] IDENTITY(1,1) NOT NULL,
	[SourceType] [nvarchar](50) NULL,
	[SourceID] [int] NULL,
	[MPGSOrderID] [nvarchar](255) NULL,
	[CheckOutSessionID] [nvarchar](50) NULL,
	[SuccessIndicator] [nvarchar](20) NULL,
	[CheckoutVersion] [nvarchar](20) NULL,
	[TransactionID] [nvarchar](255) NULL,
	[TokenizeTransactionID] [nvarchar](255) NULL,
	[Amount] [float] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[Status] [nvarchar](50) NULL,
	[UpdatedOn] [datetime] NULL,
	[Remarks] [nvarchar](max) NULL,
	[IsProcessed] [int] NOT NULL,
	[RecieptNumber] [nvarchar](50) NULL,
 CONSTRAINT [PK_CheckoutRequests] PRIMARY KEY CLUSTERED 
(
	[CheckoutRequestID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CheckoutWebhookLogs]    Script Date: 5/14/2019 5:04:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CheckoutWebhookLogs](
	[CheckoutWebhookLogID] [int] IDENTITY(1,1) NOT NULL,
	[Timestamp] [nvarchar](50) NULL,
	[TransactionID] [nvarchar](255) NULL,
	[MPGSOrderID] [nvarchar](255) NULL,
	[OrderStatus] [nvarchar](50) NULL,
	[Amount] [float] NULL,
	[LogOn] [datetime] NOT NULL,
 CONSTRAINT [PK_CheckoutWebhookLogs] PRIMARY KEY CLUSTERED 
(
	[CheckoutWebhookLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Config]    Script Date: 5/14/2019 5:04:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Config](
	[ConfigKey] [nvarchar](50) NOT NULL,
	[ConfigValue] [nvarchar](max) NULL,
	[ConfigType] [nvarchar](50) NULL,
	[NeedValidation] [int] NOT NULL,
 CONSTRAINT [PK_Config] PRIMARY KEY CLUSTERED 
(
	[ConfigKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomerLogTrail]    Script Date: 5/14/2019 5:04:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerLogTrail](
	[CustomerLogTrailID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NULL,
	[ActionSource] [nvarchar](255) NULL,
	[ActionDescription] [nvarchar](255) NULL,
	[ActionOn] [datetime] NOT NULL,
	[IPAddress] [nvarchar](255) NULL,
 CONSTRAINT [PK_CustomerLogTrail] PRIMARY KEY CLUSTERED 
(
	[CustomerLogTrailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Customers]    Script Date: 5/14/2019 5:04:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customers](
	[CustomerID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NULL,
	[Email] [nvarchar](255) NULL,
	[MobileNumber] [nvarchar](255) NULL,
	[Password] [nvarchar](4000) NULL,
	[ReferralCode] [nvarchar](255) NULL,
	[Nationality] [nvarchar](255) NULL,
	[Gender] [nvarchar](255) NULL,
	[DOB] [date] NULL,
	[EmailSubscription] [int] NOT NULL,
	[SMSSubscription] [int] NOT NULL,
	[VoiceSubscription] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[JoinedOn] [datetime] NOT NULL,
	[LoginAttemptCount] [int] NOT NULL,
 CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED 
(
	[CustomerID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomerToken]    Script Date: 5/14/2019 5:04:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerToken](
	[TokenID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NULL,
	[Token] [nvarchar](1000) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[AdminUserID] [int] NULL,
 CONSTRAINT [PK_CustomerToken] PRIMARY KEY CLUSTERED 
(
	[TokenID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DeliveryInformation]    Script Date: 5/14/2019 5:04:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeliveryInformation](
	[DeliveryInformationID] [int] IDENTITY(1,1) NOT NULL,
	[ShippingNumber] [nvarchar](255) NULL,
	[Name] [nvarchar](255) NULL,
	[Email] [nvarchar](255) NULL,
	[OrderType] [int] NULL,
	[IDNumber] [nvarchar](255) NULL,
	[IDType] [nvarchar](255) NULL,
	[IsSameAsBilling] [int] NOT NULL,
	[ShippingContactNumber] [nvarchar](255) NULL,
	[ShippingFloor] [nvarchar](255) NULL,
	[ShippingUnit] [nvarchar](255) NULL,
	[ShippingBuildingName] [nvarchar](255) NULL,
	[ShippingBuildingNumber] [nvarchar](255) NULL,
	[ShippingStreetName] [nvarchar](255) NULL,
	[ShippingPostCode] [nvarchar](255) NULL,
	[AlternateRecipientName] [nvarchar](255) NULL,
	[AlternateRecipientEmail] [nvarchar](255) NULL,
	[AlternateRecipientContact] [nvarchar](255) NULL,
	[AlternateRecioientIDNumber] [nvarchar](255) NULL,
	[AlternateRecioientIDType] [nvarchar](255) NULL,
	[PortalSlotID] [nvarchar](50) NULL,
	[ScheduledDate] [date] NULL,
	[DeliveryVendor] [nvarchar](255) NULL,
	[DeliveryOn] [date] NULL,
	[DeliveryTime] [datetime] NULL,
	[VendorTrackingCode] [nvarchar](255) NULL,
	[VendorTrackingUrl] [nvarchar](255) NULL,
	[DeliveryFee] [float] NULL,
	[VoucherID] [int] NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_DeliveryInformation] PRIMARY KEY CLUSTERED 
(
	[DeliveryInformationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DeliveryInformationLog]    Script Date: 5/14/2019 5:04:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeliveryInformationLog](
	[DeliveryInformationLogID] [int] IDENTITY(1,1) NOT NULL,
	[DeliveryInformationID] [int] NOT NULL,
	[ShippingNumber] [nvarchar](255) NULL,
	[Name] [nvarchar](255) NULL,
	[Email] [nvarchar](255) NULL,
	[IDNumber] [nvarchar](255) NULL,
	[IDType] [nvarchar](255) NULL,
	[OrderType] [int] NULL,
	[IsSameAsBilling] [int] NOT NULL,
	[ShippingContactNumber] [nvarchar](255) NULL,
	[ShippingFloor] [nvarchar](255) NULL,
	[ShippingUnit] [nvarchar](255) NULL,
	[ShippingBuildingName] [nvarchar](255) NULL,
	[ShippingBuildingNumber] [nvarchar](255) NULL,
	[ShippingStreetName] [nvarchar](255) NULL,
	[ShippingPostCode] [nvarchar](255) NULL,
	[AlternateRecipientName] [nvarchar](255) NULL,
	[AlternateRecipientEmail] [nvarchar](255) NULL,
	[AlternateRecipientContact] [nvarchar](255) NULL,
	[AlternateRecioientIDNumber] [nvarchar](255) NULL,
	[AlternateRecioientIDType] [nvarchar](255) NULL,
	[PortalSlotID] [nvarchar](50) NULL,
	[ScheduledDate] [date] NULL,
	[DeliveryVendor] [nvarchar](255) NULL,
	[DeliveryOn] [date] NULL,
	[DeliveryTime] [datetime] NULL,
	[TrackingCode] [nvarchar](255) NULL,
	[TrackingUrl] [nvarchar](255) NULL,
	[DeliveryFee] [float] NULL,
	[VoucherID] [int] NULL,
	[LoggedOn] [datetime] NOT NULL,
	[RescheduleReasonID] [int] NULL,
 CONSTRAINT [PK_DeliveryInformationLog] PRIMARY KEY CLUSTERED 
(
	[DeliveryInformationLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DeliverySlots]    Script Date: 5/14/2019 5:04:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeliverySlots](
	[DeliverySlotID] [int] IDENTITY(1,1) NOT NULL,
	[PortalSlotID] [nvarchar](50) NULL,
	[SlotDate] [date] NULL,
	[SlotFromTime] [time](7) NULL,
	[SlotToTime] [time](7) NULL,
	[NewOrderCutOffTime] [datetime] NULL,
	[RescheduleCutOffTime] [datetime] NULL,
	[AdditionalCharge] [float] NULL,
	[Capacity] [int] NULL,
	[UsedQuantity] [int] NULL,
	[IsActive] [int] NOT NULL,
 CONSTRAINT [PK_DeliverySlots] PRIMARY KEY CLUSTERED 
(
	[DeliverySlotID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Departments]    Script Date: 5/14/2019 5:04:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Departments](
	[DepartmentID] [int] IDENTITY(1,1) NOT NULL,
	[Department] [nvarchar](50) NULL,
 CONSTRAINT [PK_Departments] PRIMARY KEY CLUSTERED 
(
	[DepartmentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Documents]    Script Date: 5/14/2019 5:04:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Documents](
	[DocumentID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NULL,
	[DocumentURL] [nvarchar](4000) NULL,
	[DocumentBackURL] [nvarchar](4000) NULL,
	[IdentityCardNumber] [nvarchar](255) NULL,
	[IdentityCardType] [nvarchar](255) NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_Documents] PRIMARY KEY CLUSTERED 
(
	[DocumentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmailNotification]    Script Date: 5/14/2019 5:04:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmailNotification](
	[EmailID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NULL,
	[Email] [nvarchar](255) NULL,
	[EmailSubject] [nvarchar](4000) NULL,
	[EmailBody] [nvarchar](max) NULL,
	[ScheduledOn] [datetime] NULL,
	[EmailTemplateID] [int] NULL,
	[SendOn] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_EmailNotification] PRIMARY KEY CLUSTERED 
(
	[EmailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmailNotificationLog]    Script Date: 5/14/2019 5:04:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmailNotificationLog](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NULL,
	[EventType] [nvarchar](50) NULL,
	[Message] [nvarchar](max) NULL,
	[CreatedOn] [datetime] NULL,
 CONSTRAINT [PK_EmailNotificationLog] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmailTemplates]    Script Date: 5/14/2019 5:04:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmailTemplates](
	[EmailTemplateID] [int] IDENTITY(1,1) NOT NULL,
	[TemplateName] [nvarchar](255) NULL,
	[EmailSubject] [nvarchar](4000) NULL,
	[EmailBody] [nvarchar](max) NULL,
 CONSTRAINT [PK_EmailTemplates] PRIMARY KEY CLUSTERED 
(
	[EmailTemplateID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Errors]    Script Date: 5/14/2019 5:04:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Errors](
	[ErrorID] [int] IDENTITY(1,1) NOT NULL,
	[ErrorSource] [nvarchar](50) NULL,
	[MessageType] [nvarchar](50) NULL,
	[ErrorText] [nvarchar](255) NULL,
 CONSTRAINT [PK_Errors] PRIMARY KEY CLUSTERED 
(
	[ErrorID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Events]    Script Date: 5/14/2019 5:04:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Events](
	[EventID] [int] IDENTITY(1,1) NOT NULL,
	[EventName] [nvarchar](255) NULL,
	[EventCode] [nvarchar](255) NULL,
	[EventDate] [datetime] NULL,
	[EventLeadID] [int] NULL,
	[EventInchargeID] [int] NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED 
(
	[EventID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventSalesRepresentatives]    Script Date: 5/14/2019 5:04:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventSalesRepresentatives](
	[EventSalesRepresentativeID] [int] IDENTITY(1,1) NOT NULL,
	[EventID] [int] NULL,
	[SalesRepresentativeID] [int] NULL,
	[UserCode] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_EventSalesRepresentatives] PRIMARY KEY CLUSTERED 
(
	[EventSalesRepresentativeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FAQCategories]    Script Date: 5/14/2019 5:04:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FAQCategories](
	[FAQCategoryID] [int] IDENTITY(1,1) NOT NULL,
	[FAQCategory] [nvarchar](50) NULL,
	[Description] [nvarchar](255) NULL,
 CONSTRAINT [PK_FAQCategories] PRIMARY KEY CLUSTERED 
(
	[FAQCategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FAQs]    Script Date: 5/14/2019 5:04:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FAQs](
	[FAQID] [int] IDENTITY(1,1) NOT NULL,
	[FAQCategoryID] [int] NULL,
	[Title] [nvarchar](255) NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_FAQs] PRIMARY KEY CLUSTERED 
(
	[FAQID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ForgotPasswordTokens]    Script Date: 5/14/2019 5:04:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ForgotPasswordTokens](
	[CustomerPasswordTokenID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NULL,
	[Token] [nvarchar](4000) NULL,
	[IsUsed] [bit] NOT NULL,
	[GeneratedOn] [datetime] NOT NULL,
	[UsedOn] [datetime] NULL,
 CONSTRAINT [PK_CustomerPasswordTokens] PRIMARY KEY CLUSTERED 
(
	[CustomerPasswordTokenID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Locations]    Script Date: 5/14/2019 5:04:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Locations](
	[LocationID] [int] IDENTITY(1,1) NOT NULL,
	[LocationName] [nvarchar](50) NULL,
	[PageID] [int] NULL,
 CONSTRAINT [PK_Locations] PRIMARY KEY CLUSTERED 
(
	[LocationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Lookups]    Script Date: 5/14/2019 5:04:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Lookups](
	[LookupID] [int] IDENTITY(1,1) NOT NULL,
	[LookupText] [nvarchar](255) NULL,
	[LookupTypeID] [int] NULL,
 CONSTRAINT [PK_Lookups] PRIMARY KEY CLUSTERED 
(
	[LookupID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LookupTypes]    Script Date: 5/14/2019 5:04:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LookupTypes](
	[LookupTypeID] [int] IDENTITY(1,1) NOT NULL,
	[LookupType] [nvarchar](50) NULL,
 CONSTRAINT [PK_LookupTypes] PRIMARY KEY CLUSTERED 
(
	[LookupTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MessageQueueRequests]    Script Date: 5/14/2019 5:04:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MessageQueueRequests](
	[MessageQueueRequestID] [int] IDENTITY(1,1) NOT NULL,
	[Source] [nvarchar](50) NULL,
	[SNSTopic] [nvarchar](255) NULL,
	[MessageAttribute] [nvarchar](max) NULL,
	[MessageBody] [nvarchar](max) NULL,
	[Status] [int] NOT NULL,
	[PublishedOn] [datetime] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[NumberOfRetries] [int] NOT NULL,
	[LastTriedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_MessageQueueRequests] PRIMARY KEY CLUSTERED 
(
	[MessageQueueRequestID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MessageQueueRequestsException]    Script Date: 5/14/2019 5:04:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MessageQueueRequestsException](
	[MessageQueueRequestID] [int] IDENTITY(1,1) NOT NULL,
	[Source] [nvarchar](50) NULL,
	[SNSTopic] [nvarchar](255) NULL,
	[MessageAttribute] [nvarchar](max) NULL,
	[MessageBody] [nvarchar](max) NULL,
	[Status] [int] NULL,
	[PublishedOn] [datetime] NULL,
	[CreatedOn] [datetime] NULL,
	[NumberOfRetries] [int] NULL,
	[LastTriedOn] [datetime] NULL,
	[Remark] [nvarchar](max) NULL,
	[Exception] [nvarchar](max) NULL,
 CONSTRAINT [PK_MessageQueueRequestsException] PRIMARY KEY CLUSTERED 
(
	[MessageQueueRequestID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NumberChangeRequests]    Script Date: 5/14/2019 5:04:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NumberChangeRequests](
	[NumberChangeRequestID] [int] IDENTITY(1,1) NOT NULL,
	[ChangeRequestID] [int] NULL,
	[NewMobileNumber] [nvarchar](8) NOT NULL,
	[PremiumType] [int] NOT NULL,
	[PortingType] [int] NOT NULL,
	[DonorProvider] [nvarchar](255) NULL,
	[IsOwnNumber] [int] NOT NULL,
	[PortedNumberTransferForm] [nvarchar](255) NULL,
	[PortedNumberOwnedBy] [nvarchar](255) NULL,
	[PortedNumberOwnerRegistrationID] [nvarchar](255) NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_NumberChangeRequests] PRIMARY KEY CLUSTERED 
(
	[NumberChangeRequestID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Offices]    Script Date: 5/14/2019 5:04:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Offices](
	[OfficeID] [int] IDENTITY(1,1) NOT NULL,
	[Office] [nvarchar](50) NULL,
 CONSTRAINT [PK_Offices] PRIMARY KEY CLUSTERED 
(
	[OfficeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderCharges]    Script Date: 5/14/2019 5:04:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderCharges](
	[OrderChargeID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NULL,
	[AdminServiceID] [int] NULL,
	[ServiceFee] [float] NULL,
	[IsGSTIncluded] [int] NOT NULL,
	[IsRecurring] [int] NOT NULL,
	[ReasonType] [nvarchar](50) NULL,
	[ReasonID] [int] NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_OrderCharges] PRIMARY KEY CLUSTERED 
(
	[OrderChargeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderDocuments]    Script Date: 5/14/2019 5:04:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderDocuments](
	[OrderDocumentID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NULL,
	[DocumentID] [int] NULL,
 CONSTRAINT [PK_OrderDocuments] PRIMARY KEY CLUSTERED 
(
	[OrderDocumentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Orders]    Script Date: 5/14/2019 5:04:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Orders](
	[OrderID] [int] IDENTITY(1,1) NOT NULL,
	[OrderNumber] [nvarchar](50) NULL,
	[AccountID] [int] NULL,
	[PaymentID] [int] NULL,
	[DeliveryInformationID] [int] NULL,
	[NameInNRIC] [nvarchar](255) NULL,
	[Gender] [nvarchar](255) NULL,
	[DOB] [date] NULL,
	[Nationality] [nvarchar](255) NULL,
	[BillingFloor] [nvarchar](255) NULL,
	[BillingUnit] [nvarchar](255) NULL,
	[BillingBuildingName] [nvarchar](255) NULL,
	[BillingBuildingNumber] [nvarchar](255) NULL,
	[BillingStreetName] [nvarchar](255) NULL,
	[BillingPostCode] [nvarchar](255) NULL,
	[BillingContactNumber] [nvarchar](255) NULL,
	[ReferralCode] [nvarchar](50) NULL,
	[PromotionCode] [nvarchar](50) NULL,
	[FinalPrice] [float] NULL,
	[RefAdminUserID] [int] NULL,
	[EventSalesRepresentativeID] [int] NULL,
	[OrderDate] [datetime] NOT NULL,
	[ProcessedOn] [datetime] NULL,
	[OrderStatus] [int] NOT NULL,
	[InvoiceNumber] [nvarchar](50) NULL,
	[InvoiceUrl] [nvarchar](255) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[Terms] [int] NULL,
	[PaymentAcknowledgement] [int] NULL,
	[PromotionSubscription] [int] NULL,
	[SMSSubscription] [int] NOT NULL,
	[VoiceSubscription] [int] NOT NULL,
	[IsPaid] [int] NULL,
 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
(
	[OrderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderStatusLog]    Script Date: 5/14/2019 5:04:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderStatusLog](
	[OrderStatusLogID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NULL,
	[OrderStatus] [int] NOT NULL,
	[Remarks] [nvarchar](255) NULL,
	[UpdatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_OrderStatusLog] PRIMARY KEY CLUSTERED 
(
	[OrderStatusLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderSubscriberChangeRequests]    Script Date: 5/14/2019 5:04:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderSubscriberChangeRequests](
	[OrderSubscriberChangeRequestID] [int] IDENTITY(1,1) NOT NULL,
	[OrderSubscriberID] [int] NULL,
	[ChangeRequestID] [int] NULL,
 CONSTRAINT [PK_OrderLineChangeRequests] PRIMARY KEY CLUSTERED 
(
	[OrderSubscriberChangeRequestID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderSubscriberLogs]    Script Date: 5/14/2019 5:04:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderSubscriberLogs](
	[OrderSubscriberLogID] [int] IDENTITY(1,1) NOT NULL,
	[OrderSubscriberID] [int] NULL,
	[OrderID] [int] NULL,
	[LineStatus] [int] NOT NULL,
	[MobileNumber] [nvarchar](8) NOT NULL,
	[UserSessionID] [nvarchar](50) NULL,
	[PremiumType] [int] NOT NULL,
	[IsPorted] [int] NOT NULL,
	[IsOwnNumber] [int] NOT NULL,
	[DonorProvider] [nvarchar](255) NULL,
	[DisplayName] [nvarchar](255) NULL,
	[IsPrimaryNumber] [int] NOT NULL,
 CONSTRAINT [PK_SubscriberLogs] PRIMARY KEY CLUSTERED 
(
	[OrderSubscriberLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderSubscribers]    Script Date: 5/14/2019 5:04:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderSubscribers](
	[OrderSubscriberID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NULL,
	[IsActive] [int] NOT NULL,
	[MobileNumber] [nvarchar](8) NULL,
	[UserSessionID] [nvarchar](50) NULL,
	[SIMID] [nvarchar](255) NULL,
	[PremiumType] [int] NOT NULL,
	[IsPorted] [int] NOT NULL,
	[IsOwnNumber] [int] NOT NULL,
	[DonorProvider] [nvarchar](255) NULL,
	[DisplayName] [nvarchar](255) NULL,
	[IsPrimaryNumber] [int] NOT NULL,
	[PortedNumberTransferForm] [nvarchar](255) NULL,
	[PortedNumberOwnedBy] [nvarchar](255) NULL,
	[PortedNumberOwnerRegistrationID] [nvarchar](255) NULL,
	[IsBuddyLine] [int] NOT NULL,
	[LinkedOrderSubscriberID] [int] NULL,
	[DepositFee] [float] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[BuddyProcessedOn] [datetime] NULL,
 CONSTRAINT [PK_OrderAdditionalLines] PRIMARY KEY CLUSTERED 
(
	[OrderSubscriberID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderSubscriptionLogs]    Script Date: 5/14/2019 5:04:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderSubscriptionLogs](
	[OrderSubscriptionLogID] [int] IDENTITY(1,1) NOT NULL,
	[OrderSubscriptionID] [int] NULL,
	[ChangeRequestID] [int] NULL,
	[PlanID] [int] NULL,
	[PurchasedOn] [datetime] NULL,
	[BSSPlanCode] [nvarchar](255) NULL,
	[PlanName] [nvarchar](255) NULL,
	[SubscriptionFee] [float] NULL,
	[ValidFrom] [date] NULL,
	[ValidTo] [date] NULL,
	[Status] [int] NOT NULL,
	[RefPlanID] [int] NULL,
	[PromotionID] [int] NULL,
 CONSTRAINT [PK_SubscriptionLogs] PRIMARY KEY CLUSTERED 
(
	[OrderSubscriptionLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderSubscriptions]    Script Date: 5/14/2019 5:04:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderSubscriptions](
	[OrderSubscriptionID] [int] IDENTITY(1,1) NOT NULL,
	[ChangeRequestID] [int] NULL,
	[PlanID] [int] NULL,
	[PurchasedOn] [datetime] NULL,
	[BSSPlanCode] [nvarchar](255) NULL,
	[PlanMarketingName] [nvarchar](255) NULL,
	[SubscriptionFee] [float] NULL,
	[ValidFrom] [date] NULL,
	[ValidTo] [date] NULL,
	[Status] [int] NOT NULL,
	[BundleID] [int] NULL,
	[OldBundleID] [int] NULL,
	[PromotionID] [int] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[RefSubscriptionID] [int] NULL,
 CONSTRAINT [PK_Subscriptions] PRIMARY KEY CLUSTERED 
(
	[OrderSubscriptionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderVouchers]    Script Date: 5/14/2019 5:04:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderVouchers](
	[OrderVoucherID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NULL,
	[VoucherID] [int] NULL,
	[ValidTo] [date] NULL,
	[IsUsed] [int] NOT NULL,
	[AssignedBy] [int] NULL,
 CONSTRAINT [PK_OrderVouchers] PRIMARY KEY CLUSTERED 
(
	[OrderVoucherID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PageFAQs]    Script Date: 5/14/2019 5:04:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PageFAQs](
	[PageFAQID] [int] IDENTITY(1,1) NOT NULL,
	[PageID] [int] NULL,
	[FAQID] [int] NULL,
	[SortOrder] [int] NULL,
 CONSTRAINT [PK_PageFAQs] PRIMARY KEY CLUSTERED 
(
	[PageFAQID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Pages]    Script Date: 5/14/2019 5:04:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Pages](
	[PageID] [int] IDENTITY(1,1) NOT NULL,
	[PageName] [nvarchar](255) NULL,
	[PageContent] [nvarchar](max) NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_Pages] PRIMARY KEY CLUSTERED 
(
	[PageID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentMethods]    Script Date: 5/14/2019 5:04:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentMethods](
	[PaymentMethodID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NULL,
	[MaskedCardNumer] [nvarchar](255) NULL,
	[SourceType] [nvarchar](20) NULL,
	[Token] [nvarchar](255) NULL,
	[CardType] [nvarchar](255) NULL,
	[IsDefault] [int] NOT NULL,
	[CardHolderName] [nvarchar](255) NULL,
	[ExpiryMonth] [int] NULL,
	[ExpiryYear] [int] NULL,
	[CardFundMethod] [nvarchar](255) NULL,
	[CardBrand] [nvarchar](255) NULL,
	[CardIssuer] [nvarchar](255) NULL,
 CONSTRAINT [PK_PaymentMethods] PRIMARY KEY CLUSTERED 
(
	[PaymentMethodID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payments]    Script Date: 5/14/2019 5:04:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payments](
	[PaymentID] [int] IDENTITY(1,1) NOT NULL,
	[TransactionID] [nvarchar](50) NULL,
	[PaymentRequest] [nvarchar](max) NULL,
	[PaymentResponse] [nvarchar](max) NULL,
	[Amount] [float] NULL,
	[MaskedCardNumber] [nvarchar](255) NULL,
	[CardFundMethod] [nvarchar](255) NULL,
	[CardBrand] [nvarchar](255) NULL,
	[CardType] [nvarchar](255) NULL,
	[CardIssue] [nvarchar](255) NULL,
	[CardHolderName] [nvarchar](255) NULL,
	[ExpiryMonth] [int] NULL,
	[ExpiryYear] [int] NULL,
	[Token] [nvarchar](255) NULL,
	[PaymentStatus] [nvarchar](255) NULL,
	[GatewayCode] [nvarchar](50) NULL,
	[ApiResult] [nvarchar](50) NULL,
	[CustomerIP] [nvarchar](50) NULL,
	[PaymentOn] [datetime] NULL,
	[MPGSOrderID] [nvarchar](255) NULL,
 CONSTRAINT [PK_Payments] PRIMARY KEY CLUSTERED 
(
	[PaymentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentSources]    Script Date: 5/14/2019 5:04:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentSources](
	[PaymentSourceID] [int] IDENTITY(1,1) NOT NULL,
	[PaymentSource] [nvarchar](255) NULL,
 CONSTRAINT [PK_PaymentSources] PRIMARY KEY CLUSTERED 
(
	[PaymentSourceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PendingBuddyOrderList]    Script Date: 5/14/2019 5:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PendingBuddyOrderList](
	[PendingBuddyOrderListID] [int] IDENTITY(1,1) NOT NULL,
	[PendingBuddyID] [int] NOT NULL,
	[OrderSubscriberID] [int] NOT NULL,
	[MobileNumber] [nvarchar](50) NOT NULL,
	[IsProcessed] [bit] NOT NULL,
 CONSTRAINT [PK_PendingBuddyOrderList] PRIMARY KEY CLUSTERED 
(
	[PendingBuddyOrderListID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PendingBuddyOrders]    Script Date: 5/14/2019 5:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PendingBuddyOrders](
	[PendingBuddyID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
 CONSTRAINT [PK_PendingBuddyOrders] PRIMARY KEY CLUSTERED 
(
	[PendingBuddyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Permissions]    Script Date: 5/14/2019 5:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Permissions](
	[PermissionID] [int] IDENTITY(1,1) NOT NULL,
	[Permission] [nvarchar](255) NULL,
 CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED 
(
	[PermissionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PlanAdminServices]    Script Date: 5/14/2019 5:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlanAdminServices](
	[PlanAdminServiceID] [int] IDENTITY(1,1) NOT NULL,
	[AdminServiceID] [int] NULL,
	[BundleID] [int] NULL,
	[AdminServiceDiscountPercentage] [float] NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_PlanAdminServices] PRIMARY KEY CLUSTERED 
(
	[PlanAdminServiceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Plans]    Script Date: 5/14/2019 5:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Plans](
	[PlanID] [int] IDENTITY(1,1) NOT NULL,
	[PlanMarketingName] [nvarchar](255) NULL,
	[PortalSummaryDescription] [nvarchar](max) NULL,
	[PortalDescription] [nvarchar](max) NULL,
	[IsCustomerSelectable] [int] NOT NULL,
	[BSSPlanCode] [nvarchar](255) NULL,
	[BSSPlanName] [nvarchar](255) NULL,
	[BSSPlanDescription] [nvarchar](255) NULL,
	[IsBSSOCSBundle] [int] NULL,
	[PlanDescription] [nvarchar](max) NULL,
	[PlanType] [int] NULL,
	[Data] [float] NULL,
	[Voice] [float] NULL,
	[SMS] [float] NULL,
	[SortOrder] [int] NULL,
	[SubscriptionFee] [float] NULL,
	[IsGSTIncluded] [int] NOT NULL,
	[IsRecurring] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [int] NULL,
	[LastUpdatedOn] [datetime] NULL,
	[LastUpdatedBy] [int] NULL,
	[BundleRemovable] [int] NOT NULL,
 CONSTRAINT [PK_Plans] PRIMARY KEY CLUSTERED 
(
	[PlanID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PortalSlotsforstratigile]    Script Date: 5/14/2019 5:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PortalSlotsforstratigile](
	[PortalSlotID] [bigint] IDENTITY(1,1) NOT NULL,
	[SlotDate] [date] NOT NULL,
	[SlotFromTime] [time](7) NOT NULL,
	[SlotToTime] [time](7) NOT NULL,
	[ReSchduleCutOffTime] [datetime] NOT NULL,
	[NewOrderCutOffTime] [datetime] NULL,
	[AdditionalCharge] [money] NULL,
	[Capacity] [int] NOT NULL,
	[IsActive] [int] NOT NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PromotionBundles]    Script Date: 5/14/2019 5:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PromotionBundles](
	[PromotionPlanID] [int] IDENTITY(1,1) NOT NULL,
	[PromotionID] [int] NULL,
	[BundleID] [int] NULL,
	[RedemptionLimit] [int] NULL,
	[ValidFrom] [date] NULL,
	[ValidTo] [date] NULL,
	[DiscountValue] [float] NULL,
	[ApplicableTo] [int] NOT NULL,
	[PromotionText] [nvarchar](max) NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_PromotionBundles] PRIMARY KEY CLUSTERED 
(
	[PromotionPlanID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Promotions]    Script Date: 5/14/2019 5:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Promotions](
	[PromotionID] [int] IDENTITY(1,1) NOT NULL,
	[PromotionCode] [nvarchar](50) NULL,
	[IsAutomated] [int] NOT NULL,
	[RedemptionLimit] [int] NULL,
	[Remarks] [nvarchar](255) NULL,
	[PromotionCategory] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_Promotion] PRIMARY KEY CLUSTERED 
(
	[PromotionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RequestTypes]    Script Date: 5/14/2019 5:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RequestTypes](
	[RequestTypeID] [int] IDENTITY(1,1) NOT NULL,
	[RequestType] [nvarchar](255) NULL,
	[IsChargable] [int] NOT NULL,
	[RequestFee] [float] NULL,
 CONSTRAINT [PK_RequestTypes] PRIMARY KEY CLUSTERED 
(
	[RequestTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RescheduleDeliveryInformation]    Script Date: 5/14/2019 5:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RescheduleDeliveryInformation](
	[RescheduleDeliveryInformationID] [int] IDENTITY(1,1) NOT NULL,
	[SourceType] [nvarchar](50) NULL,
	[SourceID] [int] NULL,
	[RescheduleReasonID] [int] NULL,
	[RefOrderVoucherID] [int] NULL,
	[Name] [nvarchar](255) NULL,
	[Email] [nvarchar](255) NULL,
	[IDNumber] [nvarchar](255) NULL,
	[IDType] [nvarchar](255) NULL,
	[ShippingContactNumber] [nvarchar](255) NULL,
	[ShippingFloor] [nvarchar](255) NULL,
	[ShippingUnit] [nvarchar](255) NULL,
	[ShippingBuildingName] [nvarchar](255) NULL,
	[ShippingBuildingNumber] [nvarchar](255) NULL,
	[ShippingStreetName] [nvarchar](255) NULL,
	[ShippingPostCode] [nvarchar](255) NULL,
	[AlternateRecipientName] [nvarchar](255) NULL,
	[AlternateRecipientEmail] [nvarchar](255) NULL,
	[AlternateRecipientContact] [nvarchar](255) NULL,
	[AlternateRecioientIDNumber] [nvarchar](255) NULL,
	[AlternateRecioientIDType] [nvarchar](255) NULL,
	[PortalSlotID] [nvarchar](50) NULL,
	[ScheduledDate] [date] NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_RescheduleDeliveryInformation] PRIMARY KEY CLUSTERED 
(
	[RescheduleDeliveryInformationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RescheduleReasons]    Script Date: 5/14/2019 5:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RescheduleReasons](
	[RescheduleReasonID] [int] IDENTITY(1,1) NOT NULL,
	[Reason] [nvarchar](255) NULL,
 CONSTRAINT [PK_RescheduleReasons] PRIMARY KEY CLUSTERED 
(
	[RescheduleReasonID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RolePermissions]    Script Date: 5/14/2019 5:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RolePermissions](
	[RolePermissionID] [int] IDENTITY(1,1) NOT NULL,
	[RoleID] [int] NULL,
	[PermissionID] [int] NULL,
 CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED 
(
	[RolePermissionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Roles]    Script Date: 5/14/2019 5:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[RoleID] [int] IDENTITY(1,1) NOT NULL,
	[Role] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED 
(
	[RoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SalesRepresentatives]    Script Date: 5/14/2019 5:04:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SalesRepresentatives](
	[SalesRepresentativeID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NULL,
	[Email] [nvarchar](255) NULL,
	[Mobile] [nvarchar](255) NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_SalesRepresentatives] PRIMARY KEY CLUSTERED 
(
	[SalesRepresentativeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SMSNotifications]    Script Date: 5/14/2019 5:04:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SMSNotifications](
	[SMSID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NULL,
	[SMSText] [nvarchar](4000) NULL,
	[Mobile] [nvarchar](255) NULL,
	[SchduledOn] [datetime] NULL,
	[SMSTemplateID] [int] NULL,
	[SendOn] [datetime] NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT [PK_SMSNotifications] PRIMARY KEY CLUSTERED 
(
	[SMSID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SMSTemplates]    Script Date: 5/14/2019 5:04:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SMSTemplates](
	[SMSTemplateID] [int] IDENTITY(1,1) NOT NULL,
	[TemplateName] [nvarchar](255) NULL,
	[SMSTemplate] [nvarchar](4000) NULL,
 CONSTRAINT [PK_SMSTemplates] PRIMARY KEY CLUSTERED 
(
	[SMSTemplateID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SubscriberCharges]    Script Date: 5/14/2019 5:04:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubscriberCharges](
	[SubscriberChargeID] [int] IDENTITY(1,1) NOT NULL,
	[OrderSubscriberID] [int] NULL,
	[AdminServiceID] [int] NULL,
	[ServiceFee] [float] NULL,
	[IsGSTIncluded] [int] NOT NULL,
	[IsRecurring] [int] NOT NULL,
	[ReasonType] [nvarchar](50) NULL,
	[ReasonID] [int] NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_AdditionalFees] PRIMARY KEY CLUSTERED 
(
	[SubscriberChargeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SubscriberRequests]    Script Date: 5/14/2019 5:04:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubscriberRequests](
	[SubscriberRequestID] [int] IDENTITY(1,1) NOT NULL,
	[ChangeRequestID] [int] NULL,
	[SubscriberID] [int] NULL,
 CONSTRAINT [PK_SubscriberRequests] PRIMARY KEY CLUSTERED 
(
	[SubscriberRequestID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Subscribers]    Script Date: 5/14/2019 5:04:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Subscribers](
	[SubscriberID] [int] IDENTITY(1,1) NOT NULL,
	[AccountID] [int] NULL,
	[MobileNumber] [nvarchar](8) NULL,
	[SIMID] [nvarchar](255) NULL,
	[PremiumType] [int] NOT NULL,
	[DisplayName] [nvarchar](255) NULL,
	[ActivatedOn] [datetime] NULL,
	[IsPrimary] [int] NOT NULL,
	[IsPorted] [int] NOT NULL,
	[DonorProviderName] [nvarchar](255) NULL,
	[PortedNumberTransferForm] [nvarchar](255) NULL,
	[PortedNumberOwnedBy] [nvarchar](255) NULL,
	[PortedNumberOwnerRegistrationID] [nvarchar](255) NULL,
	[SMSSubscription] [int] NOT NULL,
	[VoiceSubscription] [int] NOT NULL,
	[DepositFee] [float] NULL,
	[IsBuddyLine] [int] NOT NULL,
	[LinkedSubscriberID] [int] NULL,
	[RefOrderSubscriberID] [int] NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_Subscriber] PRIMARY KEY CLUSTERED 
(
	[SubscriberID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SubscriberStates]    Script Date: 5/14/2019 5:04:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubscriberStates](
	[SubscriberStateID] [int] IDENTITY(1,1) NOT NULL,
	[SubscriberID] [int] NULL,
	[State] [nvarchar](255) NULL,
	[StateDate] [datetime] NOT NULL,
	[Remarks] [nvarchar](255) NULL,
	[StateSource] [int] NOT NULL,
 CONSTRAINT [PK_SubscriberStates] PRIMARY KEY CLUSTERED 
(
	[SubscriberStateID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SubscriberVouchers]    Script Date: 5/14/2019 5:04:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubscriberVouchers](
	[SubscriberVoucherID] [int] IDENTITY(1,1) NOT NULL,
	[SubscriberID] [int] NULL,
	[VoucherID] [int] NULL,
	[ValidTo] [date] NULL,
	[AssignedBy] [int] NULL,
	[IsUsed] [int] NOT NULL,
 CONSTRAINT [PK_SubscriberVouchers] PRIMARY KEY CLUSTERED 
(
	[SubscriberVoucherID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SubscriptionLog]    Script Date: 5/14/2019 5:04:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SubscriptionLog](
	[SubscriptionLogID] [int] IDENTITY(1,1) NOT NULL,
	[SubscriptionID] [int] NULL,
	[Status] [int] NULL,
	[UpdatedOn] [datetime] NOT NULL,
	[Source] [int] NOT NULL,
	[RefChangeRequestID] [int] NULL,
 CONSTRAINT [PK_SubscriptionLog] PRIMARY KEY CLUSTERED 
(
	[SubscriptionLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Subscriptions]    Script Date: 5/14/2019 5:04:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Subscriptions](
	[SubscriptionID] [int] IDENTITY(1,1) NOT NULL,
	[SubscriberID] [int] NULL,
	[PlanID] [int] NULL,
	[PurchasedOn] [datetime] NULL,
	[ActivatedOn] [datetime] NULL,
	[BSSPlanCode] [nvarchar](255) NULL,
	[PlanMarketingName] [nvarchar](255) NULL,
	[SubscriptionFee] [float] NULL,
	[ValidFrom] [date] NULL,
	[ValidTo] [date] NULL,
	[Status] [int] NOT NULL,
	[BundleID] [int] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[IsRemovable] [int] NOT NULL,
	[RefOrderSubscriptionID] [int] NULL,
	[LastUpdatedOn] [datetime] NULL,
 CONSTRAINT [PK_Subscription] PRIMARY KEY CLUSTERED 
(
	[SubscriptionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Vouchers]    Script Date: 5/14/2019 5:04:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Vouchers](
	[VoucherID] [int] IDENTITY(1,1) NOT NULL,
	[VoucherCode] [nvarchar](50) NULL,
	[DiscountValue] [float] NULL,
	[Remarks] [nvarchar](255) NULL,
	[ValidFrom] [date] NULL,
	[ValidTo] [date] NULL,
	[IsActive] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [int] NULL,
 CONSTRAINT [PK_Promotions] PRIMARY KEY CLUSTERED 
(
	[VoucherID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AccountInvoices] ADD  CONSTRAINT [DF_AccountInvoices_OrderStatus]  DEFAULT ((0)) FOR [OrderStatus]
GO
ALTER TABLE [dbo].[AccountInvoices] ADD  CONSTRAINT [DF_AccountInvoices_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[AccountInvoices] ADD  CONSTRAINT [DF_AccountInvoices_IsPaid]  DEFAULT ((0)) FOR [IsPaid]
GO
ALTER TABLE [dbo].[Accounts] ADD  CONSTRAINT [DF_Accounts_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[Accounts] ADD  CONSTRAINT [DF_Accounts_IsFinancialHold]  DEFAULT ((0)) FOR [IsFinancialHold]
GO
ALTER TABLE [dbo].[Accounts] ADD  CONSTRAINT [DF_Accounts_IsPrimary]  DEFAULT ((0)) FOR [IsPrimary]
GO
ALTER TABLE [dbo].[AccountSubscriptionLog] ADD  CONSTRAINT [DF_AccountSubscriptionLog_Source]  DEFAULT ((0)) FOR [Source]
GO
ALTER TABLE [dbo].[AccountSubscriptions] ADD  CONSTRAINT [DF_AccountSubscriptions_Status]  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[AccountSubscriptions] ADD  CONSTRAINT [DF_AccountSubscriptions_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[AdminAccessTokens] ADD  CONSTRAINT [DF_AdminAccessTokens_Token]  DEFAULT (newid()) FOR [Token]
GO
ALTER TABLE [dbo].[AdminAccessTokens] ADD  CONSTRAINT [DF_AdminAccessTokens_IsUsed]  DEFAULT ((0)) FOR [IsUsed]
GO
ALTER TABLE [dbo].[AdminAccessTokens] ADD  CONSTRAINT [DF_AdminAccessTokens_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[AdminServices] ADD  CONSTRAINT [DF_AdminServices_IsGSTIncluded]  DEFAULT ((1)) FOR [IsGSTIncluded]
GO
ALTER TABLE [dbo].[AdminServices] ADD  CONSTRAINT [DF_AdminServices_IsRecurring]  DEFAULT ((0)) FOR [IsRecurring]
GO
ALTER TABLE [dbo].[AdminServices] ADD  CONSTRAINT [DF_AdminServices_ChargeAt]  DEFAULT ((0)) FOR [ChargeAt]
GO
ALTER TABLE [dbo].[AdminServices] ADD  CONSTRAINT [DF_AdminServices_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[AdminUserLogTrail] ADD  CONSTRAINT [DF_AdminUserLogTrail_ActionOn]  DEFAULT (getdate()) FOR [ActionOn]
GO
ALTER TABLE [dbo].[AdminUsers] ADD  CONSTRAINT [DF_AdminUsers_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[AdminUserTokens] ADD  CONSTRAINT [DF_AdminUserTokens_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[APIReferences] ADD  CONSTRAINT [DF_APIReferences_AdminAccessAllowed]  DEFAULT ((1)) FOR [AdminAccessAllowed]
GO
ALTER TABLE [dbo].[Banners] ADD  CONSTRAINT [DF_Banners_UrlType]  DEFAULT ((0)) FOR [UrlType]
GO
ALTER TABLE [dbo].[Banners] ADD  CONSTRAINT [DF_Banners_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[BSSCallLogs] ADD  CONSTRAINT [DF_BSSCallLogs_CalledOn]  DEFAULT (getdate()) FOR [CalledOn]
GO
ALTER TABLE [dbo].[BSSCallNumbers] ADD  CONSTRAINT [DF_BSSCallNumbers_RetrievedOn]  DEFAULT (getdate()) FOR [RetrievedOn]
GO
ALTER TABLE [dbo].[Bundles] ADD  CONSTRAINT [DF_Bundles_IsCustomerSelectable]  DEFAULT ((0)) FOR [IsCustomerSelectable]
GO
ALTER TABLE [dbo].[Bundles] ADD  CONSTRAINT [DF_Bundles_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[Bundles] ADD  CONSTRAINT [DF_Bundles_HasBuddyPromotion]  DEFAULT ((1)) FOR [HasBuddyPromotion]
GO
ALTER TABLE [dbo].[Bundles] ADD  CONSTRAINT [DF_Bundles_IsBuddyBundle]  DEFAULT ((0)) FOR [IsBuddyBundle]
GO
ALTER TABLE [dbo].[Bundles] ADD  CONSTRAINT [DF_Bundles_PlanPriorityOrder]  DEFAULT ((0)) FOR [PlanPriorityOrder]
GO
ALTER TABLE [dbo].[Bundles] ADD  CONSTRAINT [DF_Bundles_BundleType]  DEFAULT ((0)) FOR [BundleType]
GO
ALTER TABLE [dbo].[ChangeRequestCharges] ADD  CONSTRAINT [DF_ChangeRequestCharges_IsGSTIncluded]  DEFAULT ((1)) FOR [IsGSTIncluded]
GO
ALTER TABLE [dbo].[ChangeRequestCharges] ADD  CONSTRAINT [DF_ChangeRequestCharges_IsRecurring]  DEFAULT ((0)) FOR [IsRecurring]
GO
ALTER TABLE [dbo].[ChangeRequests] ADD  CONSTRAINT [DF_ChangeRequests_RequestType]  DEFAULT ((0)) FOR [RequestTypeID]
GO
ALTER TABLE [dbo].[ChangeRequests] ADD  CONSTRAINT [DF_ChangeRequests_RequestOn]  DEFAULT (getdate()) FOR [RequestOn]
GO
ALTER TABLE [dbo].[ChangeRequests] ADD  CONSTRAINT [DF_ChangeRequests_Status]  DEFAULT ((0)) FOR [OrderStatus]
GO
ALTER TABLE [dbo].[ChangeRequests] ADD  CONSTRAINT [DF_ChangeRequests_IsPaid]  DEFAULT ((0)) FOR [IsPaid]
GO
ALTER TABLE [dbo].[CheckoutRequests] ADD  CONSTRAINT [DF_CheckoutRequests_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[CheckoutRequests] ADD  CONSTRAINT [DF_CheckoutRequests_IsProcessed]  DEFAULT ((0)) FOR [IsProcessed]
GO
ALTER TABLE [dbo].[CheckoutWebhookLogs] ADD  CONSTRAINT [DF_CheckoutWebhookLogs_LogOn]  DEFAULT (getdate()) FOR [LogOn]
GO
ALTER TABLE [dbo].[Config] ADD  CONSTRAINT [DF_Config_NeedValidation]  DEFAULT ((0)) FOR [NeedValidation]
GO
ALTER TABLE [dbo].[CustomerLogTrail] ADD  CONSTRAINT [DF_CustomerLogTrail_ActionOn]  DEFAULT (getdate()) FOR [ActionOn]
GO
ALTER TABLE [dbo].[Customers] ADD  CONSTRAINT [DF_Customers_MarketingSubscription]  DEFAULT ((0)) FOR [EmailSubscription]
GO
ALTER TABLE [dbo].[Customers] ADD  CONSTRAINT [DF_Customers_SMSSubscription]  DEFAULT ((0)) FOR [SMSSubscription]
GO
ALTER TABLE [dbo].[Customers] ADD  CONSTRAINT [DF_Customers_VoiceSubscription]  DEFAULT ((0)) FOR [VoiceSubscription]
GO
ALTER TABLE [dbo].[Customers] ADD  CONSTRAINT [DF_Customers_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[Customers] ADD  CONSTRAINT [DF_Customers_JoinedOn]  DEFAULT (getdate()) FOR [JoinedOn]
GO
ALTER TABLE [dbo].[Customers] ADD  CONSTRAINT [DF_Customers_LoginAttemptCount]  DEFAULT ((0)) FOR [LoginAttemptCount]
GO
ALTER TABLE [dbo].[CustomerToken] ADD  CONSTRAINT [DF_CustomerToken_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[DeliveryInformation] ADD  CONSTRAINT [DF_DeliveryInformation_IsSameAsBilling]  DEFAULT ((0)) FOR [IsSameAsBilling]
GO
ALTER TABLE [dbo].[DeliveryInformation] ADD  CONSTRAINT [DF_DeliveryInformation_DeliveryFee]  DEFAULT ((0)) FOR [DeliveryFee]
GO
ALTER TABLE [dbo].[DeliveryInformation] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[DeliveryInformationLog] ADD  CONSTRAINT [DF_DeliveryInformationLog_IsSameAsBilling]  DEFAULT ((0)) FOR [IsSameAsBilling]
GO
ALTER TABLE [dbo].[DeliveryInformationLog] ADD  CONSTRAINT [DF_DeliveryInformationLog_DeliveryFee]  DEFAULT ((0)) FOR [DeliveryFee]
GO
ALTER TABLE [dbo].[DeliveryInformationLog] ADD  CONSTRAINT [DF_DeliveryInformationLog_LoggedOn]  DEFAULT (getdate()) FOR [LoggedOn]
GO
ALTER TABLE [dbo].[DeliverySlots] ADD  CONSTRAINT [DF_DeliverySlots_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Documents] ADD  CONSTRAINT [DF_Documents_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[EmailNotification] ADD  CONSTRAINT [DF_EmailNotification_Status]  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[Events] ADD  CONSTRAINT [DF_Events_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[ForgotPasswordTokens] ADD  CONSTRAINT [DF_CustomerPasswordTokens_IsUsed]  DEFAULT ((0)) FOR [IsUsed]
GO
ALTER TABLE [dbo].[ForgotPasswordTokens] ADD  CONSTRAINT [DF_CustomerPasswordTokens_GeneratedOn]  DEFAULT (getdate()) FOR [GeneratedOn]
GO
ALTER TABLE [dbo].[MessageQueueRequests] ADD  CONSTRAINT [DF_MessageQueueRequests_Status]  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[MessageQueueRequests] ADD  CONSTRAINT [DF_MessageQueueRequests_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[MessageQueueRequests] ADD  CONSTRAINT [DF_MessageQueueRequests_NumberOfRetries]  DEFAULT ((0)) FOR [NumberOfRetries]
GO
ALTER TABLE [dbo].[MessageQueueRequests] ADD  CONSTRAINT [DF_MessageQueueRequests_LastTriedOn]  DEFAULT (getdate()) FOR [LastTriedOn]
GO
ALTER TABLE [dbo].[NumberChangeRequests] ADD  CONSTRAINT [DF_Table_1_ChangeNumber_PremiumType]  DEFAULT ((0)) FOR [PremiumType]
GO
ALTER TABLE [dbo].[NumberChangeRequests] ADD  CONSTRAINT [DF_Table_1_ChangeNumber_PortingType]  DEFAULT ((0)) FOR [PortingType]
GO
ALTER TABLE [dbo].[NumberChangeRequests] ADD  CONSTRAINT [DF_NumberChangeRequests_IsOwnNumber]  DEFAULT ((1)) FOR [IsOwnNumber]
GO
ALTER TABLE [dbo].[NumberChangeRequests] ADD  CONSTRAINT [DF_NumberChangeRequests_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[OrderCharges] ADD  CONSTRAINT [DF_OrderCharges_IsGSTIncluded]  DEFAULT ((1)) FOR [IsGSTIncluded]
GO
ALTER TABLE [dbo].[OrderCharges] ADD  CONSTRAINT [DF_OrderCharges_IsRecurring]  DEFAULT ((0)) FOR [IsRecurring]
GO
ALTER TABLE [dbo].[OrderCharges] ADD  CONSTRAINT [DF_OrderCharges_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_OrderDate]  DEFAULT (getdate()) FOR [OrderDate]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_OrderStatus]  DEFAULT ((0)) FOR [OrderStatus]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_PromotionSubscription]  DEFAULT ((0)) FOR [PromotionSubscription]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_SMSSubscription]  DEFAULT ((0)) FOR [SMSSubscription]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_VoiceSubscription]  DEFAULT ((0)) FOR [VoiceSubscription]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_IsPaid]  DEFAULT ((0)) FOR [IsPaid]
GO
ALTER TABLE [dbo].[OrderStatusLog] ADD  CONSTRAINT [DF_OrderStatusLog_OrderStatus]  DEFAULT ((0)) FOR [OrderStatus]
GO
ALTER TABLE [dbo].[OrderStatusLog] ADD  CONSTRAINT [DF_OrderStatusLog_UpdatedOn]  DEFAULT (getdate()) FOR [UpdatedOn]
GO
ALTER TABLE [dbo].[OrderSubscriberLogs] ADD  CONSTRAINT [DF_SubscriberLogs_LineStatus]  DEFAULT ((0)) FOR [LineStatus]
GO
ALTER TABLE [dbo].[OrderSubscriberLogs] ADD  CONSTRAINT [DF_SubscriberLogs_PremiumType]  DEFAULT ((0)) FOR [PremiumType]
GO
ALTER TABLE [dbo].[OrderSubscriberLogs] ADD  CONSTRAINT [DF_SubscriberLogs_PortingType]  DEFAULT ((0)) FOR [IsPorted]
GO
ALTER TABLE [dbo].[OrderSubscriberLogs] ADD  CONSTRAINT [DF_SubscriberLogs_IsOwnNumber]  DEFAULT ((1)) FOR [IsOwnNumber]
GO
ALTER TABLE [dbo].[OrderSubscriberLogs] ADD  CONSTRAINT [DF_SubscriberLogs_IsPrimaryNumber]  DEFAULT ((0)) FOR [IsPrimaryNumber]
GO
ALTER TABLE [dbo].[OrderSubscribers] ADD  CONSTRAINT [DF_OrderLines_LineStatus]  DEFAULT ((0)) FOR [IsActive]
GO
ALTER TABLE [dbo].[OrderSubscribers] ADD  CONSTRAINT [DF_OrderAdditionalLines_IsPremium]  DEFAULT ((0)) FOR [PremiumType]
GO
ALTER TABLE [dbo].[OrderSubscribers] ADD  CONSTRAINT [DF_OrderAdditionalLines_PortingType]  DEFAULT ((0)) FOR [IsPorted]
GO
ALTER TABLE [dbo].[OrderSubscribers] ADD  CONSTRAINT [DF_OrderAdditionalLines_IsOwnNumber]  DEFAULT ((1)) FOR [IsOwnNumber]
GO
ALTER TABLE [dbo].[OrderSubscribers] ADD  CONSTRAINT [DF_OrderDetails_IsPrimaryNumber]  DEFAULT ((0)) FOR [IsPrimaryNumber]
GO
ALTER TABLE [dbo].[OrderSubscribers] ADD  CONSTRAINT [DF_OrderSubscribers_IsBuddyLine]  DEFAULT ((0)) FOR [IsBuddyLine]
GO
ALTER TABLE [dbo].[OrderSubscribers] ADD  CONSTRAINT [DF_OrderSubscribers_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[OrderSubscriptionLogs] ADD  CONSTRAINT [DF_SubscriptionLogs_Status]  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[OrderSubscriptions] ADD  CONSTRAINT [DF_Subscriptions_Status]  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[OrderSubscriptions] ADD  CONSTRAINT [DF_OrderSubscriptions_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[OrderVouchers] ADD  CONSTRAINT [DF_OrderVouchers_IsUsed]  DEFAULT ((0)) FOR [IsUsed]
GO
ALTER TABLE [dbo].[Pages] ADD  CONSTRAINT [DF_Pages_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[PaymentMethods] ADD  CONSTRAINT [DF_PaymentMethods_IsDefault]  DEFAULT ((0)) FOR [IsDefault]
GO
ALTER TABLE [dbo].[PendingBuddyOrderList] ADD  CONSTRAINT [DF_PendingBuddyOrderList_IsProcessed]  DEFAULT ((0)) FOR [IsProcessed]
GO
ALTER TABLE [dbo].[PendingBuddyOrders] ADD  CONSTRAINT [DF_PendingBuddyOrders_DateCreated]  DEFAULT (getdate()) FOR [DateCreated]
GO
ALTER TABLE [dbo].[PlanAdminServices] ADD  CONSTRAINT [DF_PlanAdminServices_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Plans] ADD  CONSTRAINT [DF_Plans_IsCustomerSelectable]  DEFAULT ((0)) FOR [IsCustomerSelectable]
GO
ALTER TABLE [dbo].[Plans] ADD  CONSTRAINT [DF_Plans_PlanType]  DEFAULT ((1)) FOR [PlanType]
GO
ALTER TABLE [dbo].[Plans] ADD  CONSTRAINT [DF_Plans_IsGSTIncluded]  DEFAULT ((0)) FOR [IsGSTIncluded]
GO
ALTER TABLE [dbo].[Plans] ADD  CONSTRAINT [DF_Plans_IsRecurring]  DEFAULT ((1)) FOR [IsRecurring]
GO
ALTER TABLE [dbo].[Plans] ADD  CONSTRAINT [DF_Plans_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Plans] ADD  CONSTRAINT [DF_Plans_BundleRemovable]  DEFAULT ((0)) FOR [BundleRemovable]
GO
ALTER TABLE [dbo].[PromotionBundles] ADD  CONSTRAINT [DF_PromotionPlans_ApplicableTo]  DEFAULT ((0)) FOR [ApplicableTo]
GO
ALTER TABLE [dbo].[PromotionBundles] ADD  CONSTRAINT [DF_PromotionBundles_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Promotions] ADD  CONSTRAINT [DF_Promotions_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[RequestTypes] ADD  CONSTRAINT [DF_RequestTypes_IsChargable]  DEFAULT ((0)) FOR [IsChargable]
GO
ALTER TABLE [dbo].[RescheduleDeliveryInformation] ADD  CONSTRAINT [DF_RescheduleDeliveryInformation_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[SalesRepresentatives] ADD  CONSTRAINT [DF_SalesRepresentatives_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[SMSNotifications] ADD  CONSTRAINT [DF_SMSNotifications_Status]  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[SubscriberCharges] ADD  CONSTRAINT [DF_AdditionalFees_IsGSTIncluded]  DEFAULT ((1)) FOR [IsGSTIncluded]
GO
ALTER TABLE [dbo].[SubscriberCharges] ADD  CONSTRAINT [DF_AdditionalFees_IsRecurring]  DEFAULT ((0)) FOR [IsRecurring]
GO
ALTER TABLE [dbo].[SubscriberCharges] ADD  CONSTRAINT [DF_SubscriberCharges_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Subscribers] ADD  CONSTRAINT [DF_Subscribers_PremiumType1]  DEFAULT ((0)) FOR [PremiumType]
GO
ALTER TABLE [dbo].[Subscribers] ADD  CONSTRAINT [DF_Subscribers_IsPrimary]  DEFAULT ((0)) FOR [IsPrimary]
GO
ALTER TABLE [dbo].[Subscribers] ADD  CONSTRAINT [DF_Subscribers_PortingType]  DEFAULT ((0)) FOR [IsPorted]
GO
ALTER TABLE [dbo].[Subscribers] ADD  CONSTRAINT [DF_Subscribers_SMSSubscription]  DEFAULT ((0)) FOR [SMSSubscription]
GO
ALTER TABLE [dbo].[Subscribers] ADD  CONSTRAINT [DF_Subscribers_VoiceSubscription]  DEFAULT ((0)) FOR [VoiceSubscription]
GO
ALTER TABLE [dbo].[Subscribers] ADD  CONSTRAINT [DF_Subscribers_IsBuddyLine]  DEFAULT ((0)) FOR [IsBuddyLine]
GO
ALTER TABLE [dbo].[Subscribers] ADD  CONSTRAINT [DF_Subscribers_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[SubscriberStates] ADD  CONSTRAINT [DF_SubscriberStates_StateDate]  DEFAULT (getdate()) FOR [StateDate]
GO
ALTER TABLE [dbo].[SubscriberStates] ADD  CONSTRAINT [DF_SubscriberStates_StateSource]  DEFAULT ((0)) FOR [StateSource]
GO
ALTER TABLE [dbo].[SubscriberVouchers] ADD  CONSTRAINT [DF_SubscriberVouchers_IsUsed]  DEFAULT ((0)) FOR [IsUsed]
GO
ALTER TABLE [dbo].[SubscriptionLog] ADD  CONSTRAINT [DF_SubscriptionLog_Source]  DEFAULT ((0)) FOR [Source]
GO
ALTER TABLE [dbo].[Subscriptions] ADD  CONSTRAINT [DF_Subscriptions_Status_1]  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[Subscriptions] ADD  CONSTRAINT [DF_Subscriptions_CreatedOn_1]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Subscriptions] ADD  CONSTRAINT [DF_Subscriptions_IsRemovable]  DEFAULT ((1)) FOR [IsRemovable]
GO
ALTER TABLE [dbo].[Vouchers] ADD  CONSTRAINT [DF_Vouchers_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Vouchers] ADD  CONSTRAINT [DF_Vouchers_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[AccountChangeRequests]  WITH CHECK ADD  CONSTRAINT [FK_AccountChangeRequests_Accounts] FOREIGN KEY([AccountID])
REFERENCES [dbo].[Accounts] ([AccountID])
GO
ALTER TABLE [dbo].[AccountChangeRequests] CHECK CONSTRAINT [FK_AccountChangeRequests_Accounts]
GO
ALTER TABLE [dbo].[AccountChangeRequests]  WITH CHECK ADD  CONSTRAINT [FK_OrderChangeRequests_ChangeRequests] FOREIGN KEY([ChangeRequestID])
REFERENCES [dbo].[ChangeRequests] ([ChangeRequestID])
GO
ALTER TABLE [dbo].[AccountChangeRequests] CHECK CONSTRAINT [FK_OrderChangeRequests_ChangeRequests]
GO
ALTER TABLE [dbo].[AccountInvoices]  WITH CHECK ADD  CONSTRAINT [FK_AccountInvoices_Payments] FOREIGN KEY([PaymentID])
REFERENCES [dbo].[Payments] ([PaymentID])
GO
ALTER TABLE [dbo].[AccountInvoices] CHECK CONSTRAINT [FK_AccountInvoices_Payments]
GO
ALTER TABLE [dbo].[Accounts]  WITH CHECK ADD  CONSTRAINT [FK_Accounts_Customers] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
GO
ALTER TABLE [dbo].[Accounts] CHECK CONSTRAINT [FK_Accounts_Customers]
GO
ALTER TABLE [dbo].[AccountSubscriptions]  WITH CHECK ADD  CONSTRAINT [FK_AccountSubscriptions_Accounts] FOREIGN KEY([AccountID])
REFERENCES [dbo].[Accounts] ([AccountID])
GO
ALTER TABLE [dbo].[AccountSubscriptions] CHECK CONSTRAINT [FK_AccountSubscriptions_Accounts]
GO
ALTER TABLE [dbo].[AccountSubscriptions]  WITH CHECK ADD  CONSTRAINT [FK_AccountSubscriptions_Plans] FOREIGN KEY([PlanID])
REFERENCES [dbo].[Plans] ([PlanID])
GO
ALTER TABLE [dbo].[AccountSubscriptions] CHECK CONSTRAINT [FK_AccountSubscriptions_Plans]
GO
ALTER TABLE [dbo].[AdminUsers]  WITH CHECK ADD  CONSTRAINT [FK_AdminUsers_Departments] FOREIGN KEY([DepartmentID])
REFERENCES [dbo].[Departments] ([DepartmentID])
GO
ALTER TABLE [dbo].[AdminUsers] CHECK CONSTRAINT [FK_AdminUsers_Departments]
GO
ALTER TABLE [dbo].[AdminUsers]  WITH CHECK ADD  CONSTRAINT [FK_AdminUsers_Offices] FOREIGN KEY([OfficeID])
REFERENCES [dbo].[Offices] ([OfficeID])
GO
ALTER TABLE [dbo].[AdminUsers] CHECK CONSTRAINT [FK_AdminUsers_Offices]
GO
ALTER TABLE [dbo].[AdminUsers]  WITH CHECK ADD  CONSTRAINT [FK_AdminUsers_Roles] FOREIGN KEY([RoleID])
REFERENCES [dbo].[Roles] ([RoleID])
GO
ALTER TABLE [dbo].[AdminUsers] CHECK CONSTRAINT [FK_AdminUsers_Roles]
GO
ALTER TABLE [dbo].[Banners]  WITH CHECK ADD  CONSTRAINT [FK_Banners_Locations] FOREIGN KEY([LocationID])
REFERENCES [dbo].[Locations] ([LocationID])
GO
ALTER TABLE [dbo].[Banners] CHECK CONSTRAINT [FK_Banners_Locations]
GO
ALTER TABLE [dbo].[BSSCallNumbers]  WITH CHECK ADD  CONSTRAINT [FK_BSSCallNumbers_BSSCallLogs] FOREIGN KEY([BSSCallLogID])
REFERENCES [dbo].[BSSCallLogs] ([BSSCallLogID])
GO
ALTER TABLE [dbo].[BSSCallNumbers] CHECK CONSTRAINT [FK_BSSCallNumbers_BSSCallLogs]
GO
ALTER TABLE [dbo].[BundlePlans]  WITH CHECK ADD  CONSTRAINT [FK_BundlePlans_Bundles] FOREIGN KEY([BundleID])
REFERENCES [dbo].[Bundles] ([BundleID])
GO
ALTER TABLE [dbo].[BundlePlans] CHECK CONSTRAINT [FK_BundlePlans_Bundles]
GO
ALTER TABLE [dbo].[BundlePlans]  WITH CHECK ADD  CONSTRAINT [FK_BundlePlans_Plans] FOREIGN KEY([PlanID])
REFERENCES [dbo].[Plans] ([PlanID])
GO
ALTER TABLE [dbo].[BundlePlans] CHECK CONSTRAINT [FK_BundlePlans_Plans]
GO
ALTER TABLE [dbo].[ChangeRequestCharges]  WITH CHECK ADD  CONSTRAINT [FK_ChangeRequestCharges_ChangeRequests] FOREIGN KEY([ChangeRequestID])
REFERENCES [dbo].[ChangeRequests] ([ChangeRequestID])
GO
ALTER TABLE [dbo].[ChangeRequestCharges] CHECK CONSTRAINT [FK_ChangeRequestCharges_ChangeRequests]
GO
ALTER TABLE [dbo].[ChangeRequests]  WITH CHECK ADD  CONSTRAINT [FK_ChangeRequests_DeliveryInformation] FOREIGN KEY([DeliveryInformationID])
REFERENCES [dbo].[DeliveryInformation] ([DeliveryInformationID])
GO
ALTER TABLE [dbo].[ChangeRequests] CHECK CONSTRAINT [FK_ChangeRequests_DeliveryInformation]
GO
ALTER TABLE [dbo].[ChangeRequests]  WITH CHECK ADD  CONSTRAINT [FK_ChangeRequests_Payments] FOREIGN KEY([PaymentID])
REFERENCES [dbo].[Payments] ([PaymentID])
GO
ALTER TABLE [dbo].[ChangeRequests] CHECK CONSTRAINT [FK_ChangeRequests_Payments]
GO
ALTER TABLE [dbo].[ChangeRequests]  WITH CHECK ADD  CONSTRAINT [FK_ChangeRequests_RequestTypes] FOREIGN KEY([RequestTypeID])
REFERENCES [dbo].[RequestTypes] ([RequestTypeID])
GO
ALTER TABLE [dbo].[ChangeRequests] CHECK CONSTRAINT [FK_ChangeRequests_RequestTypes]
GO
ALTER TABLE [dbo].[ChangeRequestVouchers]  WITH CHECK ADD  CONSTRAINT [FK_CHangeRequestVouchers_ChangeRequests] FOREIGN KEY([ChangeRequestID])
REFERENCES [dbo].[ChangeRequests] ([ChangeRequestID])
GO
ALTER TABLE [dbo].[ChangeRequestVouchers] CHECK CONSTRAINT [FK_CHangeRequestVouchers_ChangeRequests]
GO
ALTER TABLE [dbo].[ChangeRequestVouchers]  WITH CHECK ADD  CONSTRAINT [FK_CHangeRequestVouchers_Vouchers] FOREIGN KEY([VoucherID])
REFERENCES [dbo].[Vouchers] ([VoucherID])
GO
ALTER TABLE [dbo].[ChangeRequestVouchers] CHECK CONSTRAINT [FK_CHangeRequestVouchers_Vouchers]
GO
ALTER TABLE [dbo].[CustomerLogTrail]  WITH CHECK ADD  CONSTRAINT [FK_CustomerLogTrail_Customers] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
GO
ALTER TABLE [dbo].[CustomerLogTrail] CHECK CONSTRAINT [FK_CustomerLogTrail_Customers]
GO
ALTER TABLE [dbo].[CustomerToken]  WITH CHECK ADD  CONSTRAINT [FK_CustomerToken_Customers] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
GO
ALTER TABLE [dbo].[CustomerToken] CHECK CONSTRAINT [FK_CustomerToken_Customers]
GO
ALTER TABLE [dbo].[DeliveryInformation]  WITH CHECK ADD  CONSTRAINT [FK_DeliveryInformation_Vouchers] FOREIGN KEY([VoucherID])
REFERENCES [dbo].[Vouchers] ([VoucherID])
GO
ALTER TABLE [dbo].[DeliveryInformation] CHECK CONSTRAINT [FK_DeliveryInformation_Vouchers]
GO
ALTER TABLE [dbo].[DeliveryInformationLog]  WITH CHECK ADD  CONSTRAINT [FK_DeliveryInformationLog_DeliveryInformation] FOREIGN KEY([DeliveryInformationID])
REFERENCES [dbo].[DeliveryInformation] ([DeliveryInformationID])
GO
ALTER TABLE [dbo].[DeliveryInformationLog] CHECK CONSTRAINT [FK_DeliveryInformationLog_DeliveryInformation]
GO
ALTER TABLE [dbo].[DeliveryInformationLog]  WITH CHECK ADD  CONSTRAINT [FK_DeliveryInformationLog_RescheduleReasons] FOREIGN KEY([RescheduleReasonID])
REFERENCES [dbo].[RescheduleReasons] ([RescheduleReasonID])
GO
ALTER TABLE [dbo].[DeliveryInformationLog] CHECK CONSTRAINT [FK_DeliveryInformationLog_RescheduleReasons]
GO
ALTER TABLE [dbo].[Documents]  WITH CHECK ADD  CONSTRAINT [FK_Documents_Customers] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
GO
ALTER TABLE [dbo].[Documents] CHECK CONSTRAINT [FK_Documents_Customers]
GO
ALTER TABLE [dbo].[EmailNotification]  WITH CHECK ADD  CONSTRAINT [FK_EmailNotification_Customers] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
GO
ALTER TABLE [dbo].[EmailNotification] CHECK CONSTRAINT [FK_EmailNotification_Customers]
GO
ALTER TABLE [dbo].[EmailNotification]  WITH CHECK ADD  CONSTRAINT [FK_EmailNotification_EmailTemplates] FOREIGN KEY([EmailTemplateID])
REFERENCES [dbo].[EmailTemplates] ([EmailTemplateID])
GO
ALTER TABLE [dbo].[EmailNotification] CHECK CONSTRAINT [FK_EmailNotification_EmailTemplates]
GO
ALTER TABLE [dbo].[Events]  WITH CHECK ADD  CONSTRAINT [FK_Events_AdminUsers] FOREIGN KEY([EventLeadID])
REFERENCES [dbo].[AdminUsers] ([AdminUserID])
GO
ALTER TABLE [dbo].[Events] CHECK CONSTRAINT [FK_Events_AdminUsers]
GO
ALTER TABLE [dbo].[EventSalesRepresentatives]  WITH CHECK ADD  CONSTRAINT [FK_EventSalesRepresentatives_Events] FOREIGN KEY([EventID])
REFERENCES [dbo].[Events] ([EventID])
GO
ALTER TABLE [dbo].[EventSalesRepresentatives] CHECK CONSTRAINT [FK_EventSalesRepresentatives_Events]
GO
ALTER TABLE [dbo].[EventSalesRepresentatives]  WITH CHECK ADD  CONSTRAINT [FK_EventSalesRepresentatives_SalesRepresentatives] FOREIGN KEY([SalesRepresentativeID])
REFERENCES [dbo].[SalesRepresentatives] ([SalesRepresentativeID])
GO
ALTER TABLE [dbo].[EventSalesRepresentatives] CHECK CONSTRAINT [FK_EventSalesRepresentatives_SalesRepresentatives]
GO
ALTER TABLE [dbo].[FAQs]  WITH CHECK ADD  CONSTRAINT [FK_FAQs_FAQCategories] FOREIGN KEY([FAQCategoryID])
REFERENCES [dbo].[FAQCategories] ([FAQCategoryID])
GO
ALTER TABLE [dbo].[FAQs] CHECK CONSTRAINT [FK_FAQs_FAQCategories]
GO
ALTER TABLE [dbo].[Locations]  WITH CHECK ADD  CONSTRAINT [FK_Locations_Pages] FOREIGN KEY([PageID])
REFERENCES [dbo].[Pages] ([PageID])
GO
ALTER TABLE [dbo].[Locations] CHECK CONSTRAINT [FK_Locations_Pages]
GO
ALTER TABLE [dbo].[Lookups]  WITH CHECK ADD  CONSTRAINT [FK_Lookups_LookupTypes] FOREIGN KEY([LookupTypeID])
REFERENCES [dbo].[LookupTypes] ([LookupTypeID])
GO
ALTER TABLE [dbo].[Lookups] CHECK CONSTRAINT [FK_Lookups_LookupTypes]
GO
ALTER TABLE [dbo].[NumberChangeRequests]  WITH CHECK ADD  CONSTRAINT [FK_NumberChangeRequests_ChangeRequests] FOREIGN KEY([ChangeRequestID])
REFERENCES [dbo].[ChangeRequests] ([ChangeRequestID])
GO
ALTER TABLE [dbo].[NumberChangeRequests] CHECK CONSTRAINT [FK_NumberChangeRequests_ChangeRequests]
GO
ALTER TABLE [dbo].[OrderCharges]  WITH CHECK ADD  CONSTRAINT [FK_OrderCharges_AdminServices] FOREIGN KEY([AdminServiceID])
REFERENCES [dbo].[AdminServices] ([AdminServiceID])
GO
ALTER TABLE [dbo].[OrderCharges] CHECK CONSTRAINT [FK_OrderCharges_AdminServices]
GO
ALTER TABLE [dbo].[OrderCharges]  WITH CHECK ADD  CONSTRAINT [FK_OrderCharges_Orders] FOREIGN KEY([OrderID])
REFERENCES [dbo].[Orders] ([OrderID])
GO
ALTER TABLE [dbo].[OrderCharges] CHECK CONSTRAINT [FK_OrderCharges_Orders]
GO
ALTER TABLE [dbo].[OrderDocuments]  WITH CHECK ADD  CONSTRAINT [FK_OrderDocuments_Documents] FOREIGN KEY([DocumentID])
REFERENCES [dbo].[Documents] ([DocumentID])
GO
ALTER TABLE [dbo].[OrderDocuments] CHECK CONSTRAINT [FK_OrderDocuments_Documents]
GO
ALTER TABLE [dbo].[OrderDocuments]  WITH CHECK ADD  CONSTRAINT [FK_OrderDocuments_Orders] FOREIGN KEY([OrderID])
REFERENCES [dbo].[Orders] ([OrderID])
GO
ALTER TABLE [dbo].[OrderDocuments] CHECK CONSTRAINT [FK_OrderDocuments_Orders]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_Accounts] FOREIGN KEY([AccountID])
REFERENCES [dbo].[Accounts] ([AccountID])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_Accounts]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_DeliveryInformation] FOREIGN KEY([DeliveryInformationID])
REFERENCES [dbo].[DeliveryInformation] ([DeliveryInformationID])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_DeliveryInformation]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_Payments] FOREIGN KEY([PaymentID])
REFERENCES [dbo].[Payments] ([PaymentID])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_Payments]
GO
ALTER TABLE [dbo].[OrderStatusLog]  WITH CHECK ADD  CONSTRAINT [FK_OrderStatusLog_Orders] FOREIGN KEY([OrderID])
REFERENCES [dbo].[Orders] ([OrderID])
GO
ALTER TABLE [dbo].[OrderStatusLog] CHECK CONSTRAINT [FK_OrderStatusLog_Orders]
GO
ALTER TABLE [dbo].[OrderSubscriberChangeRequests]  WITH CHECK ADD  CONSTRAINT [FK_OrderLineChangeRequests_ChangeRequests] FOREIGN KEY([ChangeRequestID])
REFERENCES [dbo].[ChangeRequests] ([ChangeRequestID])
GO
ALTER TABLE [dbo].[OrderSubscriberChangeRequests] CHECK CONSTRAINT [FK_OrderLineChangeRequests_ChangeRequests]
GO
ALTER TABLE [dbo].[OrderSubscriberChangeRequests]  WITH CHECK ADD  CONSTRAINT [FK_OrderLineChangeRequests_OrderLines] FOREIGN KEY([OrderSubscriberID])
REFERENCES [dbo].[OrderSubscribers] ([OrderSubscriberID])
GO
ALTER TABLE [dbo].[OrderSubscriberChangeRequests] CHECK CONSTRAINT [FK_OrderLineChangeRequests_OrderLines]
GO
ALTER TABLE [dbo].[OrderSubscriberLogs]  WITH CHECK ADD  CONSTRAINT [FK_SubscriberLogs_Subscribers] FOREIGN KEY([OrderSubscriberID])
REFERENCES [dbo].[OrderSubscribers] ([OrderSubscriberID])
GO
ALTER TABLE [dbo].[OrderSubscriberLogs] CHECK CONSTRAINT [FK_SubscriberLogs_Subscribers]
GO
ALTER TABLE [dbo].[OrderSubscribers]  WITH CHECK ADD  CONSTRAINT [FK_OrderAdditionalLines_Orders] FOREIGN KEY([OrderID])
REFERENCES [dbo].[Orders] ([OrderID])
GO
ALTER TABLE [dbo].[OrderSubscribers] CHECK CONSTRAINT [FK_OrderAdditionalLines_Orders]
GO
ALTER TABLE [dbo].[OrderSubscriptionLogs]  WITH CHECK ADD  CONSTRAINT [FK_SubscriptionLogs_Subscriptions] FOREIGN KEY([OrderSubscriptionID])
REFERENCES [dbo].[OrderSubscriptions] ([OrderSubscriptionID])
GO
ALTER TABLE [dbo].[OrderSubscriptionLogs] CHECK CONSTRAINT [FK_SubscriptionLogs_Subscriptions]
GO
ALTER TABLE [dbo].[OrderSubscriptions]  WITH CHECK ADD  CONSTRAINT [FK_OrderSubscriptions_Plans] FOREIGN KEY([PlanID])
REFERENCES [dbo].[Plans] ([PlanID])
GO
ALTER TABLE [dbo].[OrderSubscriptions] CHECK CONSTRAINT [FK_OrderSubscriptions_Plans]
GO
ALTER TABLE [dbo].[OrderSubscriptions]  WITH CHECK ADD  CONSTRAINT [FK_Subscriptions_ChangeRequests] FOREIGN KEY([ChangeRequestID])
REFERENCES [dbo].[ChangeRequests] ([ChangeRequestID])
GO
ALTER TABLE [dbo].[OrderSubscriptions] CHECK CONSTRAINT [FK_Subscriptions_ChangeRequests]
GO
ALTER TABLE [dbo].[OrderVouchers]  WITH CHECK ADD  CONSTRAINT [FK_OrderVouchers_Orders] FOREIGN KEY([OrderID])
REFERENCES [dbo].[Orders] ([OrderID])
GO
ALTER TABLE [dbo].[OrderVouchers] CHECK CONSTRAINT [FK_OrderVouchers_Orders]
GO
ALTER TABLE [dbo].[OrderVouchers]  WITH CHECK ADD  CONSTRAINT [FK_OrderVouchers_Vouchers] FOREIGN KEY([VoucherID])
REFERENCES [dbo].[Vouchers] ([VoucherID])
GO
ALTER TABLE [dbo].[OrderVouchers] CHECK CONSTRAINT [FK_OrderVouchers_Vouchers]
GO
ALTER TABLE [dbo].[PageFAQs]  WITH CHECK ADD  CONSTRAINT [FK_PageFAQs_FAQs] FOREIGN KEY([FAQID])
REFERENCES [dbo].[FAQs] ([FAQID])
GO
ALTER TABLE [dbo].[PageFAQs] CHECK CONSTRAINT [FK_PageFAQs_FAQs]
GO
ALTER TABLE [dbo].[PageFAQs]  WITH CHECK ADD  CONSTRAINT [FK_PageFAQs_Pages] FOREIGN KEY([PageID])
REFERENCES [dbo].[Pages] ([PageID])
GO
ALTER TABLE [dbo].[PageFAQs] CHECK CONSTRAINT [FK_PageFAQs_Pages]
GO
ALTER TABLE [dbo].[PaymentMethods]  WITH CHECK ADD  CONSTRAINT [FK_PaymentMethods_Accounts] FOREIGN KEY([AccountID])
REFERENCES [dbo].[Accounts] ([AccountID])
GO
ALTER TABLE [dbo].[PaymentMethods] CHECK CONSTRAINT [FK_PaymentMethods_Accounts]
GO
ALTER TABLE [dbo].[PlanAdminServices]  WITH CHECK ADD  CONSTRAINT [FK_PlanAdminServices_AdminServices] FOREIGN KEY([AdminServiceID])
REFERENCES [dbo].[AdminServices] ([AdminServiceID])
GO
ALTER TABLE [dbo].[PlanAdminServices] CHECK CONSTRAINT [FK_PlanAdminServices_AdminServices]
GO
ALTER TABLE [dbo].[PlanAdminServices]  WITH CHECK ADD  CONSTRAINT [FK_PlanAdminServices_Bundles] FOREIGN KEY([BundleID])
REFERENCES [dbo].[Bundles] ([BundleID])
GO
ALTER TABLE [dbo].[PlanAdminServices] CHECK CONSTRAINT [FK_PlanAdminServices_Bundles]
GO
ALTER TABLE [dbo].[PromotionBundles]  WITH CHECK ADD  CONSTRAINT [FK_PromotionBundles_Bundles] FOREIGN KEY([BundleID])
REFERENCES [dbo].[Bundles] ([BundleID])
GO
ALTER TABLE [dbo].[PromotionBundles] CHECK CONSTRAINT [FK_PromotionBundles_Bundles]
GO
ALTER TABLE [dbo].[PromotionBundles]  WITH CHECK ADD  CONSTRAINT [FK_PromotionBundles_Promotions] FOREIGN KEY([PromotionID])
REFERENCES [dbo].[Promotions] ([PromotionID])
GO
ALTER TABLE [dbo].[PromotionBundles] CHECK CONSTRAINT [FK_PromotionBundles_Promotions]
GO
ALTER TABLE [dbo].[RolePermissions]  WITH CHECK ADD  CONSTRAINT [FK_RolePermissions_Permissions] FOREIGN KEY([PermissionID])
REFERENCES [dbo].[Permissions] ([PermissionID])
GO
ALTER TABLE [dbo].[RolePermissions] CHECK CONSTRAINT [FK_RolePermissions_Permissions]
GO
ALTER TABLE [dbo].[RolePermissions]  WITH CHECK ADD  CONSTRAINT [FK_RolePermissions_Roles] FOREIGN KEY([RoleID])
REFERENCES [dbo].[Roles] ([RoleID])
GO
ALTER TABLE [dbo].[RolePermissions] CHECK CONSTRAINT [FK_RolePermissions_Roles]
GO
ALTER TABLE [dbo].[SMSNotifications]  WITH CHECK ADD  CONSTRAINT [FK_SMSNotifications_Customers] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Customers] ([CustomerID])
GO
ALTER TABLE [dbo].[SMSNotifications] CHECK CONSTRAINT [FK_SMSNotifications_Customers]
GO
ALTER TABLE [dbo].[SMSNotifications]  WITH CHECK ADD  CONSTRAINT [FK_SMSNotifications_SMSTemplates] FOREIGN KEY([SMSTemplateID])
REFERENCES [dbo].[SMSTemplates] ([SMSTemplateID])
GO
ALTER TABLE [dbo].[SMSNotifications] CHECK CONSTRAINT [FK_SMSNotifications_SMSTemplates]
GO
ALTER TABLE [dbo].[SubscriberCharges]  WITH CHECK ADD  CONSTRAINT [FK_AdditionalFees_AdminServices] FOREIGN KEY([AdminServiceID])
REFERENCES [dbo].[AdminServices] ([AdminServiceID])
GO
ALTER TABLE [dbo].[SubscriberCharges] CHECK CONSTRAINT [FK_AdditionalFees_AdminServices]
GO
ALTER TABLE [dbo].[SubscriberCharges]  WITH CHECK ADD  CONSTRAINT [FK_AdditionalFees_Subscribers] FOREIGN KEY([OrderSubscriberID])
REFERENCES [dbo].[OrderSubscribers] ([OrderSubscriberID])
GO
ALTER TABLE [dbo].[SubscriberCharges] CHECK CONSTRAINT [FK_AdditionalFees_Subscribers]
GO
ALTER TABLE [dbo].[SubscriberRequests]  WITH CHECK ADD  CONSTRAINT [FK_SubscriberRequests_ChangeRequests] FOREIGN KEY([ChangeRequestID])
REFERENCES [dbo].[ChangeRequests] ([ChangeRequestID])
GO
ALTER TABLE [dbo].[SubscriberRequests] CHECK CONSTRAINT [FK_SubscriberRequests_ChangeRequests]
GO
ALTER TABLE [dbo].[SubscriberRequests]  WITH CHECK ADD  CONSTRAINT [FK_SubscriberRequests_Subscribers] FOREIGN KEY([SubscriberID])
REFERENCES [dbo].[Subscribers] ([SubscriberID])
GO
ALTER TABLE [dbo].[SubscriberRequests] CHECK CONSTRAINT [FK_SubscriberRequests_Subscribers]
GO
ALTER TABLE [dbo].[Subscribers]  WITH CHECK ADD  CONSTRAINT [FK_Subscribers_Accounts] FOREIGN KEY([AccountID])
REFERENCES [dbo].[Accounts] ([AccountID])
GO
ALTER TABLE [dbo].[Subscribers] CHECK CONSTRAINT [FK_Subscribers_Accounts]
GO
ALTER TABLE [dbo].[SubscriberStates]  WITH CHECK ADD  CONSTRAINT [FK_SubscriberStates_Subscribers] FOREIGN KEY([SubscriberID])
REFERENCES [dbo].[Subscribers] ([SubscriberID])
GO
ALTER TABLE [dbo].[SubscriberStates] CHECK CONSTRAINT [FK_SubscriberStates_Subscribers]
GO
ALTER TABLE [dbo].[Subscriptions]  WITH CHECK ADD  CONSTRAINT [FK_Subscriptions_Plans] FOREIGN KEY([PlanID])
REFERENCES [dbo].[Plans] ([PlanID])
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [FK_Subscriptions_Plans]
GO
ALTER TABLE [dbo].[Subscriptions]  WITH CHECK ADD  CONSTRAINT [FK_Subscriptions_Subscribers] FOREIGN KEY([SubscriberID])
REFERENCES [dbo].[Subscribers] ([SubscriberID])
GO
ALTER TABLE [dbo].[Subscriptions] CHECK CONSTRAINT [FK_Subscriptions_Subscribers]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'-1=Expired;0=Initiated;1=Paid;2=Processed;3=Shipped;4=Cancelled;5=PaymentDenied' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountInvoices', @level2type=N'COLUMN',@level2name=N'OrderStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountInvoices', @level2type=N'COLUMN',@level2name=N'IsPaid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Active;2=InActive;3=Closed' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Accounts', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Accounts', @level2type=N'COLUMN',@level2name=N'IsFinancialHold'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Portal;1=BSS' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountSubscriptionLog', @level2type=N'COLUMN',@level2name=N'Source'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Purchased; 1=Activated; 2=OnHold; 3=Terminated' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AccountSubscriptions', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Portal;1=BSS' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AdminServices', @level2type=N'COLUMN',@level2name=N'ChargeAt'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Internal;1=External' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Banners', @level2type=N'COLUMN',@level2name=N'UrlType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Active;0=Draft' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Banners', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Bundles', @level2type=N'COLUMN',@level2name=N'IsCustomerSelectable'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Active;0=InActive' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Bundles', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Bundles', @level2type=N'COLUMN',@level2name=N'HasBuddyPromotion'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Bundles', @level2type=N'COLUMN',@level2name=N'IsBuddyBundle'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=New Purchase; 1=Change SIM; 2= Change Number; 3=Termination' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ChangeRequests', @level2type=N'COLUMN',@level2name=N'RequestTypeID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'-1=expired;0=initiated;1=paid and pending,2=sent to delivery vendor, 3=delivery order fulfilled, 4=out for delivery, 5=delivered, 6=delivery failed, 7=service activated, 8=cancelled;9=payment denied' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ChangeRequests', @level2type=N'COLUMN',@level2name=N'OrderStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ChangeRequests', @level2type=N'COLUMN',@level2name=N'IsPaid'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Config', @level2type=N'COLUMN',@level2name=N'NeedValidation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customers', @level2type=N'COLUMN',@level2name=N'EmailSubscription'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customers', @level2type=N'COLUMN',@level2name=N'SMSSubscription'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Active;0=InActive' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Customers', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Orders;1=CR;2=Rechedule' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'DeliveryInformation', @level2type=N'COLUMN',@level2name=N'OrderType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'DeliveryInformation', @level2type=N'COLUMN',@level2name=N'IsSameAsBilling'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'DeliveryInformationLog', @level2type=N'COLUMN',@level2name=N'IsSameAsBilling'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'DeliverySlots', @level2type=N'COLUMN',@level2name=N'IsActive'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Pending;1=Sent;2=Failed' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmailNotification', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=No, 1=Yes' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ForgotPasswordTokens', @level2type=N'COLUMN',@level2name=N'IsUsed'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Pending; 1=Sent' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'MessageQueueRequests', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'MessageQueueRequests', @level2type=N'COLUMN',@level2name=N'NumberOfRetries'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=No;1=Yes' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'NumberChangeRequests', @level2type=N'COLUMN',@level2name=N'PremiumType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=New Number; 1=Port Existing Number' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'NumberChangeRequests', @level2type=N'COLUMN',@level2name=N'PortingType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'NumberChangeRequests', @level2type=N'COLUMN',@level2name=N'IsOwnNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'-1=expired;0=initiated;1=paid and pending,2=sent to delivery vendor, 3=delivery order fulfilled, 4=out for delivery, 5=delivered, 6=delivery failed, 7=service activated, 8=cancelled;9=payment denied' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Orders', @level2type=N'COLUMN',@level2name=N'OrderStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'-1=expired;0=initiated;1=paid and pending,2=sent to delivery vendor, 3=delivery order fulfilled, 4=out for delivery, 5=delivered, 6=delivery failed, 7=service activated, 8=cancelled;9=payment denied' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrderStatusLog', @level2type=N'COLUMN',@level2name=N'OrderStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Assigned;1=Active;2=Terminated' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrderSubscriberLogs', @level2type=N'COLUMN',@level2name=N'LineStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=No;1=Yes' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrderSubscriberLogs', @level2type=N'COLUMN',@level2name=N'PremiumType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=New Number; 1=Port Existing Number' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrderSubscriberLogs', @level2type=N'COLUMN',@level2name=N'IsPorted'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrderSubscriberLogs', @level2type=N'COLUMN',@level2name=N'IsOwnNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No;' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrderSubscriberLogs', @level2type=N'COLUMN',@level2name=N'IsPrimaryNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=No;1=Yes' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrderSubscribers', @level2type=N'COLUMN',@level2name=N'IsActive'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=No;1=Bronze;2=Silver;3=Platinum;4=Gold' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrderSubscribers', @level2type=N'COLUMN',@level2name=N'PremiumType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=New Number; 1=Port Existing Number' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrderSubscribers', @level2type=N'COLUMN',@level2name=N'IsPorted'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrderSubscribers', @level2type=N'COLUMN',@level2name=N'IsOwnNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No;' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrderSubscribers', @level2type=N'COLUMN',@level2name=N'IsPrimaryNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes; 0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrderSubscribers', @level2type=N'COLUMN',@level2name=N'IsBuddyLine'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Purchased; 1=Activated; 2=OnHold; 3=Terminated' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrderSubscriptionLogs', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Purchased; 1=Activated; 2=OnHold; 3=Terminated' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'OrderSubscriptions', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Active;0=Draft' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Pages', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plans', @level2type=N'COLUMN',@level2name=N'IsCustomerSelectable'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=BasePlan;1=VAS;2=SharingVAS;3=Provisioning' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plans', @level2type=N'COLUMN',@level2name=N'PlanType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plans', @level2type=N'COLUMN',@level2name=N'IsGSTIncluded'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plans', @level2type=N'COLUMN',@level2name=N'IsRecurring'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Plans', @level2type=N'COLUMN',@level2name=N'BundleRemovable'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=CartLevel;1=LineLevel' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PromotionBundles', @level2type=N'COLUMN',@level2name=N'ApplicableTo'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Pending;1=Sent;2=Failed' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SMSNotifications', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=No;1=Yes' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Subscribers', @level2type=N'COLUMN',@level2name=N'PremiumType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=New Number; 1=Port Existing Number' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Subscribers', @level2type=N'COLUMN',@level2name=N'IsPorted'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Subscribers', @level2type=N'COLUMN',@level2name=N'SMSSubscription'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes; 0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Subscribers', @level2type=N'COLUMN',@level2name=N'IsBuddyLine'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Created;1=Active;2=PartialSuspension;3=Suspended;4=Terminated;5=TOS' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SubscriberStates', @level2type=N'COLUMN',@level2name=N'State'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Portal;1=BSS' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SubscriberStates', @level2type=N'COLUMN',@level2name=N'StateSource'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Portal;1=BSS' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SubscriptionLog', @level2type=N'COLUMN',@level2name=N'Source'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0=Purchased; 1=Activated; 2=OnHold; 3=Terminated' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Subscriptions', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=yes;0=no' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Subscriptions', @level2type=N'COLUMN',@level2name=N'IsRemovable'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1=Yes;0=No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Vouchers', @level2type=N'COLUMN',@level2name=N'IsActive'
GO
