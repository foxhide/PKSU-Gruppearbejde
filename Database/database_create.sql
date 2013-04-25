SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='TRADITIONAL,ALLOW_INVALID_DATES';

DROP SCHEMA IF EXISTS `pksudb` ;
CREATE SCHEMA IF NOT EXISTS `pksudb` DEFAULT CHARACTER SET utf8 ;
USE `pksudb` ;

-- -----------------------------------------------------
-- Table `pksudb`.`users`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `pksudb`.`users` ;

CREATE  TABLE IF NOT EXISTS `pksudb`.`users` (
  `userId` INT NOT NULL AUTO_INCREMENT ,
  `userName` VARCHAR(45) NOT NULL ,
  `password` VARCHAR(45) NOT NULL ,
  `realName` VARCHAR(45) NOT NULL ,
  `admin` TINYINT(1) NULL DEFAULT 0 ,
  `email` VARCHAR(45) NULL DEFAULT NULL ,
  `active` TINYINT(1) NULL DEFAULT 0 ,
  `needsApproval` TINYINT(1) NULL DEFAULT 1 ,
  PRIMARY KEY (`userId`) ,
  UNIQUE INDEX `userName_UNIQUE` (`userName` ASC) )
AUTO_INCREMENT = 1
COMMENT = 'Table for user logins, passwords and information.\n\nuserId: U /* comment truncated */ /*nique userid for each user (primary key). 
userName: Unique login/screen name tied to userId. Required, 45 chars.
password: Password tied to a userId. Required, 45 chars.
realname: real name of the user. Required, 45 chars.
admin: whether or not this user has admin status.
email: user email address. Not required, 45 chars.
active: whether the user is active, inactive users cannot login.
needsApproval: whether the user needs initial approval from an administrator.*/';


-- -----------------------------------------------------
-- Table `pksudb`.`eventtypes`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `pksudb`.`eventtypes` ;

CREATE  TABLE IF NOT EXISTS `pksudb`.`eventtypes` (
  `eventTypeId` INT NOT NULL AUTO_INCREMENT ,
  `eventTypeName` VARCHAR(45) NOT NULL ,
  PRIMARY KEY (`eventTypeId`) )
AUTO_INCREMENT = 1
COMMENT = 'Table for event types with names.\n\neventTypeId: Primary key. /* comment truncated */ /*
eventTypeName: name of a event type*/';


-- -----------------------------------------------------
-- Table `pksudb`.`events`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `pksudb`.`events` ;

CREATE  TABLE IF NOT EXISTS `pksudb`.`events` (
  `eventId` INT NOT NULL AUTO_INCREMENT ,
  `userId` INT NOT NULL ,
  `eventTypeId` INT NOT NULL ,
  `eventName` VARCHAR(45) NOT NULL ,
  `eventStart` DATETIME NOT NULL ,
  `eventEnd` DATETIME NOT NULL ,
  `visible` TINYINT(1) NULL DEFAULT 1 ,
  `state` INT NULL DEFAULT 0 ,
  PRIMARY KEY (`eventId`) ,
  INDEX `userId_idx` (`userId` ASC) ,
  INDEX `eventTypeId_idx` (`eventTypeId` ASC) ,
  CONSTRAINT `eventCreator`
    FOREIGN KEY (`userId` )
    REFERENCES `pksudb`.`users` (`userId` )
    ON DELETE RESTRICT
    ON UPDATE NO ACTION,
  CONSTRAINT `eventTypeId`
    FOREIGN KEY (`eventTypeId` )
    REFERENCES `pksudb`.`eventtypes` (`eventTypeId` )
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
AUTO_INCREMENT = 1
COMMENT = 'Table for events.\n\neventId: Integer as primary key to identi /* comment truncated */ /*fy each event uniquely
userId: userId of the creator of the event, references table users (foreign key).
eventTypeId: id of the eventtype, references table eventtypes (foreign key).
eventName: Name of the event (max 45 chars)
eventStart: Time at which the event starts.(format year-month-day hour:min:sec, 0000-00-00 00:00:00)
eventEnd: Time at which the event ends. (format year-month-day hour:min:sec, 0000-00-00 00:00:00)
visible: whether the event is visible to all.
state: current state of the event (not defined in database).*/';


-- -----------------------------------------------------
-- Table `pksudb`.`files`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `pksudb`.`files` ;

CREATE  TABLE IF NOT EXISTS `pksudb`.`files` (
  `fileId` INT NOT NULL AUTO_INCREMENT ,
  `fileName` VARCHAR(45) NOT NULL ,
  `pathToFile` VARCHAR(150) NOT NULL ,
  PRIMARY KEY (`fileId`) )
AUTO_INCREMENT = 1
COMMENT = 'Table for files.\n\nfileId: unique primary key for files.\nfile /* comment truncated */ /*Name: name of the file, e.g. mytextfile.txt.
pathToFile: path to file (duh), e.g. /root/myfolder/mytextfile.txt.
eventId: event that the file is associated with.*/';


-- -----------------------------------------------------
-- Table `pksudb`.`groups`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `pksudb`.`groups` ;

CREATE  TABLE IF NOT EXISTS `pksudb`.`groups` (
  `groupId` INT NOT NULL AUTO_INCREMENT ,
  `groupName` VARCHAR(45) NOT NULL ,
  PRIMARY KEY (`groupId`) )
AUTO_INCREMENT = 1
COMMENT = 'Table for groups.\n\ngroupId: Primary key identifying the grou /* comment truncated */ /*p.
groupName: the name of the group.*/';


-- -----------------------------------------------------
-- Table `pksudb`.`eventvisibility`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `pksudb`.`eventvisibility` ;

CREATE  TABLE IF NOT EXISTS `pksudb`.`eventvisibility` (
  `eventId` INT NOT NULL ,
  `groupId` INT NOT NULL ,
  PRIMARY KEY (`eventId`, `groupId`) ,
  INDEX `visigroup_idx` (`groupId` ASC) ,
  CONSTRAINT `visievent`
    FOREIGN KEY (`eventId` )
    REFERENCES `pksudb`.`events` (`eventId` )
    ON DELETE CASCADE
    ON UPDATE NO ACTION,
  CONSTRAINT `visigroup`
    FOREIGN KEY (`groupId` )
    REFERENCES `pksudb`.`groups` (`groupId` )
    ON DELETE CASCADE
    ON UPDATE NO ACTION)
ENGINE = InnoDB
COMMENT = 'A table describing which usergroups can see specific events. /* comment truncated */ /*

eventId: foreign key referencing events table.
groupId: foreign key referencing groups table.
primary key is both together.*/';


-- -----------------------------------------------------
-- Table `pksudb`.`eventeditorsusers`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `pksudb`.`eventeditorsusers` ;

CREATE  TABLE IF NOT EXISTS `pksudb`.`eventeditorsusers` (
  `eventId` INT NOT NULL ,
  `userId` INT NOT NULL ,
  PRIMARY KEY (`eventId`, `userId`) ,
  INDEX `editevent_idx` (`eventId` ASC) ,
  INDEX `edituser_idx` (`userId` ASC) ,
  CONSTRAINT `editeventusers`
    FOREIGN KEY (`eventId` )
    REFERENCES `pksudb`.`events` (`eventId` )
    ON DELETE CASCADE
    ON UPDATE NO ACTION,
  CONSTRAINT `edituserid`
    FOREIGN KEY (`userId` )
    REFERENCES `pksudb`.`users` (`userId` )
    ON DELETE CASCADE
    ON UPDATE NO ACTION)
ENGINE = InnoDB
COMMENT = 'A table describing which users can edit specific events.\nTo  /* comment truncated */ /*be used sparsely, in conjunction with eventeditorsgroups.

eventId: foreign key referencing events table.
userId: foreign key referencing users table.
primary key is both together.*/';


-- -----------------------------------------------------
-- Table `pksudb`.`groupmembers`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `pksudb`.`groupmembers` ;

CREATE  TABLE IF NOT EXISTS `pksudb`.`groupmembers` (
  `groupId` INT NOT NULL ,
  `userId` INT NOT NULL ,
  `groupLeader` TINYINT(1) NULL DEFAULT 0 ,
  `approved` TINYINT(1) NULL DEFAULT 0 ,
  PRIMARY KEY (`groupId`, `userId`) ,
  INDEX `membergroupid_idx` (`groupId` ASC) ,
  INDEX `memberuserid_idx` (`userId` ASC) ,
  CONSTRAINT `membergroupid`
    FOREIGN KEY (`groupId` )
    REFERENCES `pksudb`.`groups` (`groupId` )
    ON DELETE CASCADE
    ON UPDATE NO ACTION,
  CONSTRAINT `memberuserid`
    FOREIGN KEY (`userId` )
    REFERENCES `pksudb`.`users` (`userId` )
    ON DELETE RESTRICT
    ON UPDATE NO ACTION)
ENGINE = InnoDB
COMMENT = 'A table describing which users belong to which groups.\n\ngrou /* comment truncated */ /*pId: foreign key referencing groups table.
userId: foreign key referencing users table.
groupLeader: whether this user is one of the group leaders.
primary key is both together.*/';


-- -----------------------------------------------------
-- Table `pksudb`.`rooms`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `pksudb`.`rooms` ;

CREATE  TABLE IF NOT EXISTS `pksudb`.`rooms` (
  `roomId` INT NOT NULL AUTO_INCREMENT ,
  `roomName` VARCHAR(45) NOT NULL ,
  PRIMARY KEY (`roomId`) )
AUTO_INCREMENT = 1
COMMENT = 'Table for rooms.\n\nroomId: Primary key identifying the room.\n /* comment truncated */ /* /* comment truncated */ /*roomName: the name of the room.*/*/';


-- -----------------------------------------------------
-- Table `pksudb`.`eventeditorsgroups`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `pksudb`.`eventeditorsgroups` ;

CREATE  TABLE IF NOT EXISTS `pksudb`.`eventeditorsgroups` (
  `eventId` INT NOT NULL ,
  `groupId` INT NOT NULL ,
  PRIMARY KEY (`eventId`, `groupId`) ,
  INDEX `editevent_idx` (`eventId` ASC) ,
  INDEX `editgroupid_idx` (`groupId` ASC) ,
  CONSTRAINT `editgroupevent`
    FOREIGN KEY (`eventId` )
    REFERENCES `pksudb`.`events` (`eventId` )
    ON DELETE CASCADE
    ON UPDATE NO ACTION,
  CONSTRAINT `editgroupid`
    FOREIGN KEY (`groupId` )
    REFERENCES `pksudb`.`groups` (`groupId` )
    ON DELETE CASCADE
    ON UPDATE NO ACTION)
ENGINE = InnoDB
COMMENT = 'A table describing which usergroups can edit specific events /* comment truncated */ /*.

eventId: foreign key referencing events table.
groupId: foreign key referencing groups table.
primary key is both together.*/';


-- -----------------------------------------------------
-- Table `pksudb`.`eventtypefields`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `pksudb`.`eventtypefields` ;

CREATE  TABLE IF NOT EXISTS `pksudb`.`eventtypefields` (
  `fieldId` INT NOT NULL AUTO_INCREMENT ,
  `eventTypeId` INT NOT NULL ,
  `fieldName` VARCHAR(45) NOT NULL ,
  `fieldDescription` VARCHAR(100) NOT NULL ,
  `requiredField` TINYINT(1) NULL DEFAULT 0 ,
  `fieldType` INT NULL ,
  `varCharLength` INT NULL DEFAULT 0 ,
  PRIMARY KEY (`fieldId`) ,
  INDEX `eventtypenameid_idx` (`eventTypeId` ASC) ,
  CONSTRAINT `eventtypenameid`
    FOREIGN KEY (`eventTypeId` )
    REFERENCES `pksudb`.`eventtypes` (`eventTypeId` )
    ON DELETE CASCADE
    ON UPDATE NO ACTION)
AUTO_INCREMENT = 1
COMMENT = 'Table for which fields events use and whether they are requi /* comment truncated */ /*red fields.

fieldId: Primary key. Auto incremented.
eventTypeId: foreign key referencing eventtypenames. cannot be null.
fieldName: name of the field in the database. cannot be null.
fieldDescription: description of a field. cannot be null.
requiredField: whether or not filling this field is required.
fieldType: 0 for int, 1 for text, 2 for date, 3 for user, 4 for group, 5 for file, 6 for boolean.
varCharLength: length of string if type is string, 0 if not string.*/';


-- -----------------------------------------------------
-- Table `pksudb`.`eventroomsused`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `pksudb`.`eventroomsused` ;

CREATE  TABLE IF NOT EXISTS `pksudb`.`eventroomsused` (
  `eventId` INT NOT NULL ,
  `roomId` INT NOT NULL ,
  PRIMARY KEY (`eventId`, `roomId`) ,
  INDEX `roomsusedevid_idx` (`eventId` ASC) ,
  INDEX `roomsusedroomid_idx` (`roomId` ASC) ,
  CONSTRAINT `roomsusedevid`
    FOREIGN KEY (`eventId` )
    REFERENCES `pksudb`.`events` (`eventId` )
    ON DELETE CASCADE
    ON UPDATE NO ACTION,
  CONSTRAINT `roomsusedroomid`
    FOREIGN KEY (`roomId` )
    REFERENCES `pksudb`.`rooms` (`roomId` )
    ON DELETE CASCADE
    ON UPDATE NO ACTION)
ENGINE = InnoDB
COMMENT = 'Table describing which rooms are used for an event.\n\neventId /* comment truncated */ /*: foreign key referencing events table
roomId: foreign key referencing rooms table
primary key is both together.*/';


-- -----------------------------------------------------
-- Table `pksudb`.`eventcreationgroups`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `pksudb`.`eventcreationgroups` ;

CREATE  TABLE IF NOT EXISTS `pksudb`.`eventcreationgroups` (
  `eventTypeId` INT NOT NULL ,
  `groupId` INT NOT NULL ,
  PRIMARY KEY (`eventTypeId`, `groupId`) ,
  INDEX `creationeventtypeid_idx` (`eventTypeId` ASC) ,
  INDEX `creationgroupid_idx` (`groupId` ASC) ,
  CONSTRAINT `creationeventtypeid`
    FOREIGN KEY (`eventTypeId` )
    REFERENCES `pksudb`.`eventtypes` (`eventTypeId` )
    ON DELETE CASCADE
    ON UPDATE NO ACTION,
  CONSTRAINT `creationgroupid`
    FOREIGN KEY (`groupId` )
    REFERENCES `pksudb`.`groups` (`groupId` )
    ON DELETE CASCADE
    ON UPDATE NO ACTION)
ENGINE = InnoDB
COMMENT = 'Table describing which groups can create which event types.\n /* comment truncated */ /*
eventTypeId: Foreign and primary key, not null.
groupId: Foreign and primary key, not null.*/';

USE `pksudb` ;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;

-- -----------------------------------------------------
-- Data for table `pksudb`.`eventtypes`
-- -----------------------------------------------------
START TRANSACTION;
USE `pksudb`;
INSERT INTO `pksudb`.`eventtypes` (`eventTypeId`, `eventTypeName`) VALUES (1, 'Basic event');

COMMIT;
