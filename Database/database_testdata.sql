-- Rooms
INSERT INTO rooms (roomName) VALUES ("Room 1");
INSERT INTO rooms (roomName) VALUES ("Room 2");
INSERT INTO rooms (roomName) VALUES ("Room 3");
INSERT INTO rooms (roomName) VALUES ("Room 4");
INSERT INTO rooms (roomName) VALUES ("Room 5");
INSERT INTO rooms (roomName) VALUES ("Room 6");
INSERT INTO rooms (roomName) VALUES ("Room 7");

-- Users
-- password for johan_the_man is 'johanersej'
INSERT INTO users (userName, password, realName, admin, phoneNum, email, active, needsApproval)
	VALUES ("johan_the_man","1000:40fMt+KjU9BuxnAzB30T7iD7AxGRn08Z:Iumz5ozS0aTrq+OzbkugeXsl1pNIWoiT","Johan", 1, "012345678","johan@stengade.dk",1,0);
-- password for andreas_PKSU is 'andreasercool'
INSERT INTO users (userName, password, realName, admin, phoneNum, email, active, needsApproval)
	VALUES ("andreas_PKSU","1000:WJ5Ayds/Y/ppFJQUabbWiBZqxF+EDQg4:+8y/SwqlBVsAww1mvN2wC9a5JIKxhBP7","Andreas", 1, "012345678","andreas@stengade.dk",1,0);
-- password for stephan_kerbal is 'kerbaltillinux'
INSERT INTO users (userName, password, realName, admin, phoneNum, email, active, needsApproval)
	VALUES ("stephan_kerbal","1000:9Fmy1xBY2YVGGNcNcSVuN9SZoukPehdL:0hVUB/TjHtnGIqpl1CXggrkcDkciHwpW","Stephan", 1, "012345678","stephan@stengade.dk",1,0);
-- password for bill123 is 'kerbal'
INSERT INTO users (userName, password, realName, admin, phoneNum, email, active, needsApproval)
	VALUES ("bill123","1000:isl+vj+KMCRz1AB3fWiB5g1z0ESC/JKe:IJ0ibpJ8nm5tlxtZhkwN/fCTsORR+vM1","Bill", 0, "012345678","bill@kerbal.kb",1,0);

-- Groups
INSERT INTO groups (groupName) VALUES ("Poker players");
INSERT INTO groups (groupName) VALUES ("Bartenders");
INSERT INTO groups (groupName) VALUES ("Minecraft Gurus");
INSERT INTO groups (groupName) VALUES ("L04");

-- Groupmembers
INSERT INTO groupmembers(groupId,userId,groupLeader,canCreate) VALUES (4,1,1,1);
INSERT INTO groupmembers(groupId,userId,groupLeader,canCreate) VALUES (4,2,1,1);
INSERT INTO groupmembers(groupId,userId,groupLeader,canCreate) VALUES (4,3,1,1);
INSERT INTO groupmembers(groupId,userId,groupLeader,canCreate) VALUES (3,2,1,1);
INSERT INTO groupmembers(groupId,userId,groupLeader,canCreate) VALUES (2,1,1,1);
INSERT INTO groupmembers(groupId,userId,groupLeader,canCreate) VALUES (1,3,1,1);
INSERT INTO groupmembers(groupId,userId,groupLeader,canCreate) VALUES (1,1,1,1);
INSERT INTO groupmembers(groupId,userId,groupLeader,canCreate) VALUES (4,4,0,0);

-- Event types
	-- Basic event (no fields)
INSERT INTO eventtypes (eventTypeName) VALUES ("Basic Event");

DROP TABLE IF EXISTS table_1;

CREATE TABLE table_1 (
	eventId int NOT NULL,
	PRIMARY KEY (eventId),
    CONSTRAINT table_1eventid
    FOREIGN KEY (eventId)
    REFERENCES events (eventId)
    ON DELETE CASCADE
    ON UPDATE NO ACTION
);

	-- Concert event
INSERT INTO eventtypes (eventTypeName) VALUES ("Concert");
INSERT INTO eventtypefields (eventTypeId, fieldName, fieldDescription, requiredCreation, requiredApproval, fieldType, varCharLength, fieldOrder)
VALUES (2, "Band-name","Name of the band that's playing",1,1,1,50,1);

DROP TABLE IF EXISTS table_2;

CREATE TABLE table_2 (
	eventId int NOT NULL,
	field_1 VARCHAR(50),
	PRIMARY KEY (eventId),
    CONSTRAINT table_2eventid
    FOREIGN KEY (eventId)
    REFERENCES events (eventId)
    ON DELETE CASCADE
    ON UPDATE NO ACTION
);

-- Events
	-- Muse
INSERT INTO events (userId, eventTypeId, eventName, eventStart, eventEnd, visible, state)
	VALUES (3,2,"Muse","2013-03-19 22:00:00","2013-03-20 02:30:00",true,0);
INSERT INTO table_2 (eventId,field_1) VALUES (1,"muse");
INSERT INTO eventroomsused (eventId, roomId) VALUES (1,2);
INSERT INTO eventroomsused (eventId, roomId) VALUES (1,3);

	-- Poker
INSERT INTO events (userId, eventTypeId, eventName, eventStart, eventEnd, visible, state)
	VALUES (1,1,"Poker","2013-03-22 22:00:00","2013-03-24 10:00:00",true,1);
INSERT INTO table_1 (eventId) VALUES (2);
INSERT INTO eventroomsused (eventId, roomId) VALUES (2,4);

	-- Kerbal
INSERT INTO events (userId, eventTypeId, eventName, eventStart, eventEnd, visible, state)
	VALUES (2,1,"Kerbal","2013-03-02 22:00:00","2013-04-20 02:30:00",true,2);
INSERT INTO table_1 (eventId) VALUES (3);
INSERT INTO eventroomsused (eventId, roomId) VALUES (3,1);
INSERT INTO eventroomsused (eventId, roomId) VALUES (3,6);

	-- MineCon
INSERT INTO events (userId, eventTypeId, eventName, eventStart, eventEnd, visible, state)
	VALUES (2,1,"MineCon 2013","2013-03-02 10:00:00","2013-03-02 18:30:00",true,1);
INSERT INTO table_1 (eventId) VALUES (4);
INSERT INTO eventroomsused (eventId, roomId) VALUES (4,5);
   
    -- Visible event
INSERT INTO events (userId, eventTypeId, eventName, eventStart, eventEnd, visible, state)
	VALUES (1,1,"Visible","2013-05-10 10:00:00","2013-05-12 10:00:00",true,1);
INSERT INTO table_1 (eventId) VALUES (5);
INSERT INTO eventroomsused (eventId, roomId) VALUES (5,1);
INSERT INTO eventroomsused (eventId, roomId) VALUES (5,2);

    -- VisibleToL04 event
INSERT INTO events (userId, eventTypeId, eventName, eventStart, eventEnd, visible, state)
	VALUES (1,1,"L04 event","2013-05-12 10:00:00","2013-05-13 10:00:00",false,2);
INSERT INTO table_1 (eventId) VALUES (6);
INSERT INTO eventroomsused (eventId, roomId) VALUES (6,1);
INSERT INTO eventroomsused (eventId, roomId) VALUES (6,3);
INSERT INTO eventvisibility (eventId, groupId) VALUES (6,4);

    -- VisibleToMinecraftGurus event
INSERT INTO events (userId, eventTypeId, eventName, eventStart, eventEnd, visible, state)
	VALUES (3,1,"Minecraft1337","2013-05-13 10:00:00","2013-05-16 10:00:00",false,0);
INSERT INTO table_1 (eventId) VALUES (7);
INSERT INTO eventroomsused (eventId, roomId) VALUES (7,4);
INSERT INTO eventroomsused (eventId, roomId) VALUES (7,5);
INSERT INTO eventvisibility (eventId, groupId) VALUES (7,3);


COMMIT;