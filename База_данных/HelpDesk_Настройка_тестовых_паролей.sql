USE HelpDesk_Aytalieva;
GO
-- Учебные хеши PBKDF2 для локальной учебной базы
UPDATE dbo.Users SET PasswordHash = N'100000.HnKR+/YpZNzcm3j2DssKGg==.4p6rt5jrPB1K69f/gYfYWZ4My21pmnbjFoRrgD10xck=' WHERE Login = N'admin';
UPDATE dbo.Users SET PasswordHash = N'100000.0hOW8riqKhHUTtvNinA/uw==.lp3EAmrX85n1g0rcawajT8RFpOZqO54qw0+JGw5oFRE=' WHERE Login = N'support1';
UPDATE dbo.Users SET PasswordHash = N'100000.LdmDcIkOp4YEPYK3kCu0jg==.URfvZKhBFSKpLgV7pR2WsD9pyPXv566eLgpiVICeoeA=' WHERE Login = N'user1';
GO
