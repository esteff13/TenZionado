-- Sample campus events; run once after schema.sql. StartsAt values use UTC.
INSERT INTO dbo.Events(Title,Description,Venue,StartsAt,Capacity) VALUES
(N'Campus Tech Forum',N'Explore student projects and discuss practical uses of technology.',N'Innovation Hall',DATEADD(day,7,SYSUTCDATETIME()),40),
(N'Creative Coding Workshop',N'A guided session on building your first interactive web experience.',N'Computer Lab 2',DATEADD(day,10,SYSUTCDATETIME()),25),
(N'Community Volunteer Briefing',N'Meet the campus volunteer team and learn about upcoming projects.',N'Student Center',DATEADD(day,14,SYSUTCDATETIME()),60);
