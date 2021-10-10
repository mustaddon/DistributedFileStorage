CREATE TABLE [dbo].[DfsFileInfo]
(
	[Id] VARCHAR(32) NOT NULL PRIMARY KEY,
	[Name] NVARCHAR(128) NOT NULL,
	[Metadata] NVARCHAR(MAX) NULL,
	[ContentId] VARCHAR(56) NOT NULL,

    CONSTRAINT [FK_DfsFileInfo_Content] FOREIGN KEY ([ContentId]) REFERENCES [dbo].[DfsContentInfo] ([Id]),
)
