IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = '{0}'))
BEGIN
	DROP TABLE [dbo].[{0}]
END

CREATE TABLE [dbo].[{0}] (
	[Id] [int] NOT NULL IDENTITY(1,1),
	[Country] [varchar](100) NOT NULL,
	[FromIp] [bigint] NOT NULL,
	[ToIp] [bigint] NOT NULL,
	[Count] [int] NOT NULL,
	[Assigned] [datetime] NOT NULL,
	[Inserted] [datetime] NOT NULL
) ON [PRIMARY]

ALTER TABLE [dbo].[{0}] WITH NOCHECK ADD 
	CONSTRAINT [PK_{0}] PRIMARY KEY  CLUSTERED 
	(
		[Id]
	)  ON [PRIMARY] 

ALTER TABLE [dbo].[{0}] ADD 
	CONSTRAINT [DF_{0}_Inserted] DEFAULT (getdate()) FOR [Inserted]


CREATE NONCLUSTERED INDEX [IX_{0}_Country] 
	ON [dbo].[{0}]([Country])


CREATE NONCLUSTERED INDEX [IX_country_by_ip_FromIp_ToIp] 
	ON [dbo].[yv_country_by_ip]([FromIp], [ToIp]) INCLUDE ([Country])

