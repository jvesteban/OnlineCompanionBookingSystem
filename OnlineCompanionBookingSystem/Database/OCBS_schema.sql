USE [master]
GO
/****** Object:  Database [OnlineCompanionBookingManagementSystem_Database]    Script Date: 02/10/2026 11:04:40 am ******/
CREATE DATABASE [OnlineCompanionBookingManagementSystem_Database]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'OnlineCompanionBookingManagementSystem_Database', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQL\DATA\OnlineCompanionBookingManagementSystem_Database.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'OnlineCompanionBookingManagementSystem_Database_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQL\DATA\OnlineCompanionBookingManagementSystem_Database_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET COMPATIBILITY_LEVEL = 170
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [OnlineCompanionBookingManagementSystem_Database].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET ARITHABORT OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET  DISABLE_BROKER 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET  MULTI_USER 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET DB_CHAINING OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET OPTIMIZED_LOCKING = OFF 
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET QUERY_STORE = ON
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [OnlineCompanionBookingManagementSystem_Database]
GO
/****** Object:  Table [dbo].[Activities]    Script Date: 02/10/2026 11:04:40 am ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Activities](
	[ActivityID] [int] IDENTITY(1,1) NOT NULL,
	[CompanionID] [int] NOT NULL,
	[ActivityName] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ActivityID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Availability]    Script Date: 02/10/2026 11:04:40 am ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Availability](
	[AvailabilityID] [int] IDENTITY(1,1) NOT NULL,
	[CompanionID] [int] NOT NULL,
	[DayOfWeek] [nvarchar](20) NOT NULL,
	[StartTime] [time](7) NOT NULL,
	[EndTime] [time](7) NOT NULL,
	[IsAvailable] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AvailabilityID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BookingActivities]    Script Date: 02/10/2026 11:04:40 am ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BookingActivities](
	[BookingActivityID] [int] IDENTITY(1,1) NOT NULL,
	[BookingID] [int] NOT NULL,
	[ActivityName] [nvarchar](150) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[BookingActivityID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bookings]    Script Date: 02/10/2026 11:04:40 am ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bookings](
	[BookingID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NOT NULL,
	[CompanionID] [int] NOT NULL,
	[PackageID] [int] NOT NULL,
	[BookingDate] [date] NOT NULL,
	[BookingTime] [time](7) NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[ContactInfoShared] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[BookingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CompanionProfiles]    Script Date: 02/10/2026 11:04:40 am ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CompanionProfiles](
	[CompanionID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[Bio] [nvarchar](max) NULL,
	[ProfilePhoto] [nvarchar](255) NULL,
	[VerificationStatus] [nvarchar](20) NOT NULL,
	[VerificationDocPath] [nvarchar](255) NULL,
	[DateVerified] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[CompanionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NavSeen]    Script Date: 02/10/2026 11:04:40 am ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NavSeen](
	[UserID] [int] NOT NULL,
	[Section] [varchar](30) NOT NULL,
	[ItemID] [int] NOT NULL,
	[Marker] [varchar](30) NOT NULL,
 CONSTRAINT [PK_NavSeen] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC,
	[Section] ASC,
	[ItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notifications]    Script Date: 02/10/2026 11:04:40 am ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notifications](
	[NotificationID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[Message] [nvarchar](255) NOT NULL,
	[IsRead] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[RelatedBookingID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[NotificationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Packages]    Script Date: 02/10/2026 11:04:40 am ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Packages](
	[PackageID] [int] IDENTITY(1,1) NOT NULL,
	[CompanionID] [int] NOT NULL,
	[PackageName] [nvarchar](100) NOT NULL,
	[Duration] [nvarchar](50) NULL,
	[Rate] [decimal](10, 2) NOT NULL,
	[Description] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[PackageID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payments]    Script Date: 02/10/2026 11:04:40 am ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payments](
	[PaymentID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[ReferenceNo] [varchar](40) NOT NULL,
	[Purpose] [varchar](60) NOT NULL,
	[Amount] [decimal](10, 2) NOT NULL,
	[Method] [varchar](30) NOT NULL,
	[Status] [varchar](20) NOT NULL,
	[DatePaid] [datetime] NOT NULL,
	[IsSimulated] [bit] NOT NULL,
	[RefundedAt] [datetime] NULL,
	[RefundReference] [varchar](40) NULL,
PRIMARY KEY CLUSTERED 
(
	[PaymentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Payments_ReferenceNo] UNIQUE NONCLUSTERED 
(
	[ReferenceNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Ratings]    Script Date: 02/10/2026 11:04:40 am ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ratings](
	[RatingID] [int] IDENTITY(1,1) NOT NULL,
	[BookingID] [int] NOT NULL,
	[CustomerID] [int] NOT NULL,
	[CompanionID] [int] NOT NULL,
	[Score] [int] NOT NULL,
	[Comment] [nvarchar](max) NULL,
	[DateCreated] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RatingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[BookingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 02/10/2026 11:04:40 am ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[PasswordHash] [nvarchar](255) NOT NULL,
	[ContactNumber] [nvarchar](20) NULL,
	[Role] [nvarchar](20) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[ProfilePicture] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_BookingActivities_BookingID]    Script Date: 02/10/2026 11:04:40 am ******/
CREATE NONCLUSTERED INDEX [IX_BookingActivities_BookingID] ON [dbo].[BookingActivities]
(
	[BookingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UX_Ratings_BookingID]    Script Date: 02/10/2026 11:04:40 am ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Ratings_BookingID] ON [dbo].[Ratings]
(
	[BookingID] ASC
)
WHERE ([BookingID] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Availability] ADD  DEFAULT ((1)) FOR [IsAvailable]
GO
ALTER TABLE [dbo].[Bookings] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[Bookings] ADD  DEFAULT (getdate()) FOR [DateCreated]
GO
ALTER TABLE [dbo].[Bookings] ADD  DEFAULT ((0)) FOR [ContactInfoShared]
GO
ALTER TABLE [dbo].[CompanionProfiles] ADD  DEFAULT ('Pending') FOR [VerificationStatus]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT ((0)) FOR [IsRead]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT (getdate()) FOR [DateCreated]
GO
ALTER TABLE [dbo].[Payments] ADD  CONSTRAINT [DF_Payments_DatePaid]  DEFAULT (getdate()) FOR [DatePaid]
GO
ALTER TABLE [dbo].[Payments] ADD  CONSTRAINT [DF_Payments_IsSimulated]  DEFAULT ((1)) FOR [IsSimulated]
GO
ALTER TABLE [dbo].[Ratings] ADD  DEFAULT (getdate()) FOR [DateCreated]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [DateCreated]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Activities]  WITH CHECK ADD  CONSTRAINT [FK_Activities_CompanionProfiles] FOREIGN KEY([CompanionID])
REFERENCES [dbo].[CompanionProfiles] ([CompanionID])
GO
ALTER TABLE [dbo].[Activities] CHECK CONSTRAINT [FK_Activities_CompanionProfiles]
GO
ALTER TABLE [dbo].[Availability]  WITH CHECK ADD  CONSTRAINT [FK_Availability_CompanionProfiles] FOREIGN KEY([CompanionID])
REFERENCES [dbo].[CompanionProfiles] ([CompanionID])
GO
ALTER TABLE [dbo].[Availability] CHECK CONSTRAINT [FK_Availability_CompanionProfiles]
GO
ALTER TABLE [dbo].[BookingActivities]  WITH CHECK ADD  CONSTRAINT [FK_BookingActivities_Bookings] FOREIGN KEY([BookingID])
REFERENCES [dbo].[Bookings] ([BookingID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BookingActivities] CHECK CONSTRAINT [FK_BookingActivities_Bookings]
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD  CONSTRAINT [FK_Bookings_CompanionProfiles] FOREIGN KEY([CompanionID])
REFERENCES [dbo].[CompanionProfiles] ([CompanionID])
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK_Bookings_CompanionProfiles]
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD  CONSTRAINT [FK_Bookings_Customer] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK_Bookings_Customer]
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD  CONSTRAINT [FK_Bookings_Packages] FOREIGN KEY([PackageID])
REFERENCES [dbo].[Packages] ([PackageID])
GO
ALTER TABLE [dbo].[Bookings] CHECK CONSTRAINT [FK_Bookings_Packages]
GO
ALTER TABLE [dbo].[CompanionProfiles]  WITH CHECK ADD  CONSTRAINT [FK_CompanionProfiles_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[CompanionProfiles] CHECK CONSTRAINT [FK_CompanionProfiles_Users]
GO
ALTER TABLE [dbo].[NavSeen]  WITH CHECK ADD  CONSTRAINT [FK_NavSeen_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NavSeen] CHECK CONSTRAINT [FK_NavSeen_Users]
GO
ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [FK_Notifications_Bookings] FOREIGN KEY([RelatedBookingID])
REFERENCES [dbo].[Bookings] ([BookingID])
GO
ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_Bookings]
GO
ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [FK_Notifications_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_Users]
GO
ALTER TABLE [dbo].[Packages]  WITH CHECK ADD  CONSTRAINT [FK_Packages_CompanionProfiles] FOREIGN KEY([CompanionID])
REFERENCES [dbo].[CompanionProfiles] ([CompanionID])
GO
ALTER TABLE [dbo].[Packages] CHECK CONSTRAINT [FK_Packages_CompanionProfiles]
GO
ALTER TABLE [dbo].[Payments]  WITH CHECK ADD  CONSTRAINT [FK_Payments_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Payments] CHECK CONSTRAINT [FK_Payments_Users]
GO
ALTER TABLE [dbo].[Ratings]  WITH CHECK ADD  CONSTRAINT [FK_Ratings_Bookings] FOREIGN KEY([BookingID])
REFERENCES [dbo].[Bookings] ([BookingID])
GO
ALTER TABLE [dbo].[Ratings] CHECK CONSTRAINT [FK_Ratings_Bookings]
GO
ALTER TABLE [dbo].[Ratings]  WITH CHECK ADD  CONSTRAINT [FK_Ratings_CompanionProfiles] FOREIGN KEY([CompanionID])
REFERENCES [dbo].[CompanionProfiles] ([CompanionID])
GO
ALTER TABLE [dbo].[Ratings] CHECK CONSTRAINT [FK_Ratings_CompanionProfiles]
GO
ALTER TABLE [dbo].[Ratings]  WITH CHECK ADD  CONSTRAINT [FK_Ratings_Customer] FOREIGN KEY([CustomerID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Ratings] CHECK CONSTRAINT [FK_Ratings_Customer]
GO
ALTER TABLE [dbo].[Bookings]  WITH CHECK ADD CHECK  (([Status]='Cancelled' OR [Status]='Completed' OR [Status]='Declined' OR [Status]='Confirmed' OR [Status]='Pending'))
GO
ALTER TABLE [dbo].[CompanionProfiles]  WITH CHECK ADD CHECK  (([VerificationStatus]='Rejected' OR [VerificationStatus]='Verified' OR [VerificationStatus]='Pending'))
GO
ALTER TABLE [dbo].[Ratings]  WITH CHECK ADD CHECK  (([Score]>=(1) AND [Score]<=(5)))
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD CHECK  (([Role]='Admin' OR [Role]='Companion' OR [Role]='Customer'))
GO
USE [master]
GO
ALTER DATABASE [OnlineCompanionBookingManagementSystem_Database] SET  READ_WRITE 
GO
