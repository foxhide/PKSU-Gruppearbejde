-- Rooms
INSERT INTO rooms (roomName) VALUES ("Room 1");
INSERT INTO rooms (roomName) VALUES ("Room 2");
INSERT INTO rooms (roomName) VALUES ("Room 3");
INSERT INTO rooms (roomName) VALUES ("Room 4");
INSERT INTO rooms (roomName) VALUES ("Room 5");
INSERT INTO rooms (roomName) VALUES ("Room 6");
INSERT INTO rooms (roomName) VALUES ("Room 7");

-- Users
INSERT INTO users (userName, password, realName, admin, email, active, needsApproval) VALUES ("johan_the_man","johanersej","Johan", 1,"johan@stengade.dk",1,0);
INSERT INTO users (userName, password, realName, admin, email, active, needsApproval) VALUES ("andreas_PKSU","andreasercool","Andreas", 1,"andreas@stengade.dk",1,0);
INSERT INTO users (userName, password, realName, admin, email, active, needsApproval) VALUES ("stephan_kerbal","kerbaltillinux","Stephan", 1,"stephan@stengade.dk",1,0);

-- Event types
	-- Basic event (no fields)
INSERT INTO eventtypes (eventTypeName) VALUES ("Basic Event");

CREATE TABLE pksudb.table_1 (
	eventId int NOT NULL,
	PRIMARY KEY (eventId),
    CONSTRAINT table_1eventid
    FOREIGN KEY (eventId)
    REFERENCES pksudb.events (eventId)
    ON DELETE CASCADE
    ON UPDATE NO ACTION
);

	-- Concert event
INSERT INTO eventtypes (eventTypeName) VALUES ("Concert");
INSERT INTO eventtypefields (eventTypeId, fieldName, fieldDescription, requiredField, fieldType, varCharLength) VALUES (2, "Band-name","Name of the band that's playing",1,1,50);

CREATE TABLE pksudb.table_2 (
	eventId int NOT NULL,
	field_1 VARCHAR(50) NOT NULL,
	PRIMARY KEY (eventId),
    CONSTRAINT table_2eventid
    FOREIGN KEY (eventId)
    REFERENCES pksudb.events (eventId)
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
INSERT INTO eventroomsused (eventId, roomId) VALUES (2,4);

	-- Kerbal
INSERT INTO events (userId, eventTypeId, eventName, eventStart, eventEnd, visible, state)
	VALUES (2,1,"Kerbal","2013-03-02 22:00:00","2013-04-20 02:30:00",true,2);
INSERT INTO eventroomsused (eventId, roomId) VALUES (3,1);
INSERT INTO eventroomsused (eventId, roomId) VALUES (3,6);

	-- MineCon
INSERT INTO events (userId, eventTypeId, eventName, eventStart, eventEnd, visible, state)
	VALUES (2,1,"MineCon 2013","2013-03-02 10:00:00","2013-03-02 18:30:00",true,1);
INSERT INTO eventroomsused (eventId, roomId) VALUES (4,5);

COMMIT;