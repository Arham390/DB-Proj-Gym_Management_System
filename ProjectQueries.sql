CREATE TABLE Audit_Trail (
    audit_ID INT Identity(1,1) PRIMARY KEY,
    TablePerformedon varchar(50),
    action VARCHAR(255),
);
CREATE TABLE Membership (
    mID INT IDENTITY(1,1) PRIMARY KEY,
    type VARCHAR(50),
    duration VARCHAR(15),
    cost DECIMAL(10, 2)
);
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    PasswordHash VARCHAR(255),
    Email VARCHAR(255),
    FullName VARCHAR(100),
    Gender VARCHAR(20),
    UserType VARCHAR(10),
	Contact VARCHAR(15)
);
CREATE TABLE Owners (
    OwnerID INT PRIMARY KEY,
	GymName VARCHAR(100),
	Approved INT,
    FOREIGN KEY (OwnerID) REFERENCES Users(UserID)
);

CREATE TABLE WorkoutPlans (
    PlanID INT IDENTITY(1,1) PRIMARY KEY,
    PlanName VARCHAR(100),
    CreatorID INT,
    FOREIGN KEY (CreatorID) REFERENCES Users(UserID),
);
CREATE TABLE Exercise (
    ExerciseID INT IDENTITY(1,1) PRIMARY KEY,
    TargetMuscle VARCHAR(20),
    ExerciseName VARCHAR(100),
    Machine VARCHAR(50)
);
CREATE TABLE ROUTINE (
    PlanID INT,
    FOREIGN KEY (PlanID) REFERENCES WorkoutPlans(PlanID),
    rID INT IDENTITY(1,1) PRIMARY KEY,
    Day1 VARCHAR(10),
    sets1 INT,
    Reps INT,
    Interval INT,
	ExerciseID INT,
	FOREIGN KEY (ExerciseID) REFERENCES Exercise(ExerciseID)
);
CREATE TABLE diet (
    dietID INT IDENTITY(1,1) PRIMARY KEY,
    diet_plan_name VARCHAR(50),
    CreatorID INT,
	FOREIGN KEY (CreatorID) REFERENCES Users(UserID)
);
CREATE TABLE meal (
   mealID INT IDENTITY(1,1) PRIMARY KEY,
    dietplanID INT,
	FOREIGN KEY (dietplanID) REFERENCES diet(dietID),
	Day1 VARCHAR(30),
	meal_name VARCHAR(20),
    Protein DECIMAL(8,2),
    Carbs DECIMAL(8,2),
    Fiber DECIMAL(8,2),
    Fat DECIMAL(8,2),
    calories DECIMAL(8,2),
	allergens VARCHAR(30)
);
CREATE TABLE Members (
    MemberID INT PRIMARY KEY,
	GymID INT,
	workoutID INT,
    mID INT,
	dietID INT,
	JoinDate DATE,
    Height DECIMAL(5, 2),
    Weight DECIMAL(5, 2),
	FOREIGN KEY (GymID) REFERENCES Owners(OwnerID),
	FOREIGN KEY (dietID) REFERENCES diet(dietID),
	FOREIGN KEY (workoutID) REFERENCES WorkoutPlans(PlanID),
    FOREIGN KEY (MemberID) REFERENCES Users(UserID),
    FOREIGN KEY (mID) REFERENCES Membership(mID)
);

CREATE TABLE Trainers (
    TrainerID INT PRIMARY KEY,
	Height DECIMAL(5, 2),
	Weight DECIMAL(5, 2),
	Spec Varchar(40),
	Experience VARCHAR(15),
    FOREIGN KEY (TrainerID) REFERENCES Users(UserID),
);

CREATE TABLE TrainerRatings (
	RatingID INT identity(1,1) Primary Key,
    TrainerID INT,
    MemberID INT,
    GymID INT,
    Rating INT,
    Feedback TEXT,
    FOREIGN KEY (TrainerID) REFERENCES Trainers(TrainerID),
    FOREIGN KEY (MemberID) REFERENCES Members(MemberID),
);

CREATE TABLE worksat (
    trainer INT ,
    gymid INT ,
	PRIMARY KEY(TRAINER,GYMID),
    FOREIGN KEY (trainer) REFERENCES Trainers(TrainerID),
    FOREIGN KEY (gymid) REFERENCES Owners(OwnerID)
);
CREATE TABLE Admins (
    AdminID INT PRIMARY KEY,
    FOREIGN KEY (AdminID) REFERENCES Users(UserID)
);
CREATE TABLE PersonalTrainingSessions (
    SessionID INT IDENTITY(1,1)PRIMARY KEY,
    TrainerID INT,
    MemberID INT,
	Status1 INT,
    SessionDate DATE,
    SessionTime TIME,
    FOREIGN KEY (TrainerID) REFERENCES Trainers(TrainerID),
    FOREIGN KEY (MemberID) REFERENCES Members(MemberID)
);
-- Start a transaction
BEGIN TRANSACTION;

-- Trigger 1: TR_WorkoutPlans_CreatorCheck
GO
CREATE TRIGGER TR_WorkoutPlans_CreatorCheck
ON WorkoutPlans
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS(
        SELECT 1 
        FROM inserted i 
        LEFT JOIN Users u ON i.CreatorID = u.UserID
        WHERE u.UserID IS NULL OR u.UserType NOT IN ('Trainer', 'Member')
    )
    BEGIN
        RAISERROR ('CreatorID must exist in Users table and be a Trainer or Member.', 16, 1)
        ROLLBACK TRANSACTION
    END
END;
GO

-- Trigger 2: TR_Routine_ExerciseCheck
GO
CREATE TRIGGER TR_Routine_ExerciseCheck
ON Routine
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS(SELECT 1 FROM inserted i LEFT JOIN Exercise e ON i.ExerciseID = e.ExerciseID WHERE e.ExerciseID IS NULL)
    BEGIN
        RAISERROR ('ExerciseID must exist in Exercise table.', 16, 1)
        ROLLBACK TRANSACTION
    END
END;
GO

-- Trigger 3: TR_Members_ReferentialIntegrityCheck
GO
CREATE TRIGGER TR_Members_ReferentialIntegrityCheck
ON Members
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS(SELECT 1 FROM inserted i 
              LEFT JOIN Users u ON i.MemberID = u.UserID
              LEFT JOIN Owners o ON i.GymID = o.OwnerID
              LEFT JOIN WorkoutPlans w ON i.workoutID = w.PlanID
              LEFT JOIN diet d ON i.dietID = d.dietID
              LEFT JOIN Membership m ON i.mID = m.mID
              WHERE u.UserID IS NULL  OR (o.Approved = 0))
    BEGIN
        RAISERROR ('Referential integrity check failed or Gym not approved.', 16, 1)
        ROLLBACK TRANSACTION
    END
END;
GO

-- Trigger 4: PersonalTrainingSessionsReferentialIntegrityCheck
GO
CREATE TRIGGER PersonalTrainingSessionsReferentialIntegrityCheck
ON PersonalTrainingSessions
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS(
        SELECT 1 FROM inserted i 
        LEFT JOIN Trainers t ON i.TrainerID = t.TrainerID
        LEFT JOIN Members m ON i.MemberID = m.MemberID
        LEFT JOIN worksat w ON t.TrainerID = w.trainer
        WHERE t.TrainerID IS NULL OR m.MemberID IS NULL OR (w.gymid IS NOT NULL AND w.gymid != m.GymID)
    )
    BEGIN
        RAISERROR ('Referential integrity check failed.', 16, 1)
        ROLLBACK TRANSACTION
    END
END;
GO

-- Trigger 5: worksAtGymApprovalCheck
GO
CREATE TRIGGER worksAtGymApprovalCheck
ON worksat
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS(
        SELECT 1 FROM inserted i
        LEFT JOIN Owners o ON i.gymid = o.OwnerID
        WHERE o.Approved = 0
    )
    BEGIN
        RAISERROR ('Trainer cannot work at unapproved gyms.', 16, 1)
        ROLLBACK TRANSACTION
    END
END;
GO

-- Trigger 6: TR_diet_CreatorCheck
GO
CREATE TRIGGER TR_diet_CreatorCheck
ON diet
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS(
        SELECT 1 FROM inserted i
        LEFT JOIN Users u ON i.CreatorID = u.UserID
        WHERE u.UserID IS NULL OR u.UserType NOT IN ('Member', 'Trainer')
    )
    BEGIN
        RAISERROR ('CreatorID must exist in Users table and be a member or trainer.', 16, 1)
        ROLLBACK TRANSACTION
    END
END;
GO

-- Trigger 7: TR_meal_DietPlanCheck
GO
CREATE TRIGGER TR_meal_DietPlanCheck
ON meal
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS(
        SELECT 1 FROM inserted i
        LEFT JOIN diet d ON i.dietplanID = d.dietID
        WHERE d.dietID IS NULL
    )
    BEGIN
        RAISERROR ('dietplanID must exist in diet table.', 16, 1)
        ROLLBACK TRANSACTION
    END
END;
GO

-- Trigger 8: TR_Trainers_UserTypeCheck
GO
CREATE TRIGGER TR_Trainers_UserTypeCheck
ON Trainers
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS(
        SELECT 1 FROM inserted i
        LEFT JOIN Users u ON i.TrainerID = u.UserID
        WHERE u.UserType <> 'Trainer'
    )
    BEGIN
        RAISERROR ('User must have Usertype as trainer.', 16, 1)
        ROLLBACK TRANSACTION
    END
END;
GO

-- Trigger 9: TR_Owner_UserTypeCheck
GO
CREATE TRIGGER TR_Owner_UserTypeCheck
ON Owners
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS(
        SELECT 1 FROM inserted i
        LEFT JOIN Users u ON i.OwnerID = u.UserID
        WHERE u.UserType <> 'Owner'
    )
    BEGIN
        RAISERROR ('User must have Usertype as trainer.', 16, 1)
        ROLLBACK TRANSACTION
    END
END;
GO

-- Trigger 10: TR_Admin_UserTypeCheck
GO
CREATE TRIGGER TR_Admin_UserTypeCheck
ON Admins
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS(
        SELECT 1 FROM inserted i
        LEFT JOIN Users u ON i.AdminID = u.UserID
        WHERE u.UserType <> 'Admin'
    )
    BEGIN
        RAISERROR ('User must have Usertype as Admin.', 16, 1)
        ROLLBACK TRANSACTION
    END
END;
GO

-- Trigger 11: TR_TrainerRatings_WorkAtGymCheck
GO
CREATE TRIGGER TR_TrainerRatings_WorkAtGymCheck
ON TrainerRatings
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS(
        SELECT 1 FROM inserted i
        LEFT JOIN Trainers t ON i.TrainerID = t.TrainerID
        LEFT JOIN worksat w ON t.TrainerID = w.trainer
        WHERE w.gymid <> i.GymID OR w.gymid IS NULL
    )
    BEGIN
        RAISERROR ('Trainer does not work at the specified gym.', 16, 1)
        ROLLBACK TRANSACTION
    END
END;
GO

-- Trigger 12: Owner_InsertDeleteTrigger
GO
CREATE OR ALTER TRIGGER Owner_InsertDeleteTrigger
ON owners
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OperationType VARCHAR(20);
    DECLARE @TableName VARCHAR(255) = 'Owners';
    DECLARE @TableID INT;

    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @OperationType = 'INSERT';
        SELECT @TableID = OwnerID FROM inserted;
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @OperationType = 'DELETE';
        SELECT @TableID = OwnerID FROM deleted;
    END

    INSERT INTO Audit_Trail (TablePerformedon, action) 
    VALUES (@TableName, @OperationType);
END;
GO

-- Trigger 13: Members_InsertDeleteTrigger
GO
CREATE OR ALTER TRIGGER Members_InsertDeleteTrigger
ON Members
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OperationType VARCHAR(20);
    DECLARE @TableName VARCHAR(255) = 'Members';
    DECLARE @TableID INT;

    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @OperationType = 'INSERT';
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @OperationType = 'DELETE';
    END

    INSERT INTO Audit_Trail (TablePerformedon, action) 
    VALUES (@TableName, @OperationType);
END;
GO

-- Trigger 14: diet_InsertDeleteTrigger
GO
CREATE OR ALTER TRIGGER diet_InsertDeleteTrigger
ON diet
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OperationType VARCHAR(20);
    DECLARE @TableName VARCHAR(255) = 'Diet';
    DECLARE @TableID INT;

    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @OperationType = 'INSERT';
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @OperationType = 'DELETE';
    END

    INSERT INTO Audit_Trail (TablePerformedon, action) 
    VALUES (@TableName, @OperationType);
END;
GO

-- Trigger 15: Exersice_InsertDeleteTrigger
GO
CREATE OR ALTER TRIGGER Exersice_InsertDeleteTrigger
ON Exercise
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OperationType VARCHAR(20);
    DECLARE @TableName VARCHAR(255) = 'Exersice';
    DECLARE @TableID INT;

    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @OperationType = 'INSERT';
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @OperationType = 'DELETE';
    END

    INSERT INTO Audit_Trail (TablePerformedon, action) 
    VALUES (@TableName, @OperationType);
END;
GO

-- Trigger 16: Meal_InsertDeleteTrigger
GO
CREATE OR ALTER TRIGGER Meal_InsertDeleteTrigger
ON Meal
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OperationType VARCHAR(20);
    DECLARE @TableName VARCHAR(255) = 'Meal';
    DECLARE @TableID INT;

    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @OperationType = 'INSERT';
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @OperationType = 'DELETE';
    END

    INSERT INTO Audit_Trail (TablePerformedon, action) 
    VALUES (@TableName, @OperationType);
END;
GO

-- Trigger 17: PTS_InsertDeleteTrigger
GO
CREATE OR ALTER TRIGGER PTS_InsertDeleteTrigger
ON PersonalTrainingSessions
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OperationType VARCHAR(20);
    DECLARE @TableName VARCHAR(255) = 'Personal Training Session';

    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @OperationType = 'INSERT';
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @OperationType = 'DELETE';
    END

    INSERT INTO Audit_Trail (TablePerformedon, action) 
    VALUES (@TableName, @OperationType);
END;
GO

-- Trigger 18: Routine_InsertDeleteTrigger
GO
CREATE OR ALTER TRIGGER Routine_InsertDeleteTrigger
ON Routine
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OperationType VARCHAR(20);
    DECLARE @TableName VARCHAR(255) = 'Routine';
    DECLARE @TableID INT;

    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @OperationType = 'INSERT';
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @OperationType = 'DELETE';
    END

    INSERT INTO Audit_Trail (TablePerformedon, action) 
    VALUES (@TableName, @OperationType);
END;
GO

-- Trigger 19: Trainer_InsertDeleteTrigger
GO
CREATE OR ALTER TRIGGER Trainer_InsertDeleteTrigger
ON Trainers
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OperationType VARCHAR(20);
    DECLARE @TableName VARCHAR(255) = 'Trainers';
    DECLARE @TableID INT;

    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @OperationType = 'INSERT';
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @OperationType = 'DELETE';
    END

    INSERT INTO Audit_Trail (TablePerformedon, action) 
    VALUES (@TableName, @OperationType);
END;
GO

-- Trigger 20: TrainerRatings_InsertDeleteTrigger
GO
CREATE OR ALTER TRIGGER TrainerRatings_InsertDeleteTrigger
ON TrainerRatings
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OperationType VARCHAR(20);
    DECLARE @TableName VARCHAR(255) = 'TrainerRatings';
    DECLARE @TableID INT;

    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @OperationType = 'INSERT';
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @OperationType = 'DELETE';
    END

    INSERT INTO Audit_Trail (TablePerformedon, action) 
    VALUES (@TableName, @OperationType);
END;
GO

-- Trigger 21: User_InsertDeleteTrigger
GO
CREATE OR ALTER TRIGGER User_InsertDeleteTrigger
ON Users
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OperationType VARCHAR(20);
    DECLARE @TableName VARCHAR(255) = 'Users';
    DECLARE @TableID INT;

    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @OperationType = 'INSERT';
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @OperationType = 'DELETE';
    END

    INSERT INTO Audit_Trail (TablePerformedon, action) 
    VALUES (@TableName, @OperationType);
END;
GO

-- Trigger 22: WorkoutPlans_InsertDeleteTrigger
GO
CREATE OR ALTER TRIGGER WorkoutPlans_InsertDeleteTrigger
ON WorkoutPlans
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OperationType VARCHAR(20);
    DECLARE @TableName VARCHAR(255) = 'WokoutPlans';
    DECLARE @TableID INT;

    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @OperationType = 'INSERT';
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @OperationType = 'DELETE';
    END

    INSERT INTO Audit_Trail (TablePerformedon, action) 
    VALUES (@TableName, @OperationType);
END;
GO

-- Trigger 23: WorksAt_InsertDeleteTrigger
GO
CREATE OR ALTER TRIGGER WorksAt_InsertDeleteTrigger
ON WorksAt
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OperationType VARCHAR(20);
    DECLARE @TableName VARCHAR(255) = 'WorksAt';
    DECLARE @TableID INT;

    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @OperationType = 'INSERT';
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @OperationType = 'DELETE';
    END

    INSERT INTO Audit_Trail (TablePerformedon, action) 
    VALUES (@TableName, @OperationType);
END;
GO

-- Trigger 24: Admin_InsertDeleteTrigger
GO
CREATE OR ALTER TRIGGER Admin_InsertDeleteTrigger
ON Admins
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OperationType VARCHAR(20);
    DECLARE @TableName VARCHAR(255) = 'Admins';
    DECLARE @TableID INT;

    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        SET @OperationType = 'INSERT';
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        SET @OperationType = 'DELETE';
    END

    INSERT INTO Audit_Trail (TablePerformedon, action) 
    VALUES (@TableName, @OperationType);
END;
GO

-- Commit the transaction
COMMIT TRANSACTION;





 INSERT INTO Users (PasswordHash, Email, FullName, Gender, UserType, Contact)
VALUES
-- Members (35 users)
('pass123', 'member1@example.com', 'John Doe', 'Male', 'Member', '1234567890'),
('pass456', 'member2@example.com', 'Jane Smith', 'Female', 'Member', '9876543210'),
('pass789', 'member3@example.com', 'Alice Johnson', 'Female', 'Member', '1231231234'),
('pass987', 'member4@example.com', 'Mike Brown', 'Male', 'Member', '4567891230'),
('pass654', 'member5@example.com', 'Emily Wilson', 'Female', 'Member', '3216549870'),
('pass321', 'member6@example.com', 'David Lee', 'Male', 'Member', '7894563210'),
('pass951', 'member7@example.com', 'Anna Taylor', 'Female', 'Member', '6547893210'),
('pass753', 'member8@example.com', 'Chris Evans', 'Male', 'Member', '9876541320'),
('pass258', 'member9@example.com', 'Sophia Clark', 'Female', 'Member', '2589631470'),
('pass369', 'member10@example.com', 'Kevin White', 'Male', 'Member', '9638527410'),
('pass741', 'member11@example.com', 'Olivia Harris', 'Female', 'Member', '1472583690'),
('pass852', 'member12@example.com', 'Daniel Martinez', 'Male', 'Member', '3691472580'),
('pass963', 'member13@example.com', 'Emma Thomas', 'Female', 'Member', '7418529630'),
('pass147', 'member14@example.com', 'William Robinson', 'Male', 'Member', '8529631470'),
('pass369', 'member15@example.com', 'Sophie Garcia', 'Female', 'Member', '1592634780'),
('pass852', 'member16@example.com', 'Jackson Lewis', 'Male', 'Member', '7539518520'),
('pass963', 'member17@example.com', 'Ava Perez', 'Female', 'Member', '1592634780'),
('pass147', 'member18@example.com', 'Matthew Hall', 'Male', 'Member', '7539518520'),
('pass369', 'member19@example.com', 'Chloe Young', 'Female', 'Member', '1592634780'),
('pass852', 'member20@example.com', 'Ethan Martinez', 'Male', 'Member', '7539518520'),
('pass963', 'member21@example.com', 'Mia Adams', 'Female', 'Member', '1592634780'),
('pass147', 'member22@example.com', 'Alexander Lopez', 'Male', 'Member', '7539518520'),
('pass369', 'member23@example.com', 'Emily Rodriguez', 'Female', 'Member', '1592634780'),
('pass852', 'member24@example.com', 'James Nelson', 'Male', 'Member', '7539518520'),
('pass963', 'member25@example.com', 'Madison Carter', 'Female', 'Member', '1592634780'),
('pass147', 'member26@example.com', 'Michael Allen', 'Male', 'Member', '7539518520'),
('pass369', 'member27@example.com', 'Abigail Baker', 'Female', 'Member', '1592634780'),
('pass852', 'member28@example.com', 'Daniel Gonzalez', 'Male', 'Member', '7539518520'),
('pass963', 'member29@example.com', 'Elizabeth Garcia', 'Female', 'Member', '1592634780'),
('pass147', 'member30@example.com', 'David Davis', 'Male', 'Member', '7539518520'),
('pass369', 'member31@example.com', 'Grace Wright', 'Female', 'Member', '1592634780'),
('pass852', 'member32@example.com', 'John Hernandez', 'Male', 'Member', '7539518520'),
('pass963', 'member33@example.com', 'Victoria Hill', 'Female', 'Member', '1592634780'),
('pass147', 'member34@example.com', 'Matthew Scott', 'Male', 'Member', '7539518520'),
('pass369', 'member35@example.com', 'Charlotte Green', 'Female', 'Member', '1592634780'),
-- Trainers (10 users)
('pass123', 'trainer1@example.com', 'Mark Jones', 'Male', 'Trainer', '5678901234'),
('pass456', 'trainer2@example.com', 'Sarah Lee', 'Female', 'Trainer', '2468135790'),
('pass789', 'trainer3@example.com', 'Chris Johnson', 'Male', 'Trainer', '1234567890'),
('pass987', 'trainer4@example.com', 'Emily Moore', 'Female', 'Trainer', '9876543210'),
('pass654', 'trainer5@example.com', 'David Brown', 'Male', 'Trainer', '6541239870'),
('pass321', 'trainer6@example.com', 'Jessica Wilson', 'Female', 'Trainer', '3216549870'),
('pass951', 'trainer7@example.com', 'Michael Taylor', 'Male', 'Trainer', '9517538246'),
('pass753', 'trainer8@example.com', 'Laura Garcia', 'Female', 'Trainer', '7539518520'),
('pass258', 'trainer9@example.com', 'Brian Martinez', 'Male', 'Trainer', '2583691470'),
('pass369', 'trainer10@example.com', 'Alexis Allen', 'Female', 'Trainer', '3698521470'),
-- Owners (4 users)
('pass123', 'owner1@example.com', 'David Smith', 'Male', 'Owner', '1234567890'),
('pass456', 'owner2@example.com', 'Jennifer Brown', 'Female', 'Owner', '9876543210'),
('pass789', 'owner3@example.com', 'Daniel Johnson', 'Male', 'Owner', '1234567890'),
('pass987', 'owner4@example.com', 'Jessica Lee', 'Female', 'Owner', '9876543210'),
-- Admin (1 user)
('adminpass', 'admin@example.com', 'Admin User', 'Male', 'Admin', '9998887776');




-- Inserting sample data into Trainers table
INSERT INTO Trainers (TrainerID, Height, Weight, Spec, Experience)
VALUES
(36,  180.3, 82.5, 'Strength & Conditioning', '5 Years'),
(37,  170.0, 65.0, 'Yoga & Flexibility', '3 Years'),
(38,  175.0, 75.0, 'HIIT & Cardio', '4 Years'),
(39,  185.0, 80.0, 'CrossFit', '6 Years'),
(40,  172.0, 70.0, 'Weightlifting', '2 Years'),
(41,  168.0, 62.0, 'Pilates', '3 Years'),
(42,  178.0, 77.0, 'Functional Training', '4 Years'),
(43,  180.0, 85.0, 'Bodybuilding', '7 Years'),
(44,  175.0, 72.0, 'Boxing', '5 Years'),
(45,  170.0, 68.0, 'Circuit Training', '3 Years');

-- Inserting data into Owners table based on existing Users
INSERT INTO Owners (OwnerID, GymName, Approved)
VALUES
-- Owner 1
(46, 'Fitness Junction', 1),
-- Owner 2
(47, 'GymXtreme', 1),
-- Owner 3
(48, 'FitZone', 1),
-- Owner 4
(49, 'Fitness World', 1);

-- Inserting data into Admins table
INSERT INTO Admins (AdminID)
VALUES
(50);  



-- Inserting data into worksAt table for trainers
INSERT INTO worksAt (trainer, gymid)
VALUES
(36, 46),  -- Trainer 1 at Gym 1
(37, 47),  -- Trainer 2 at Gym 2
(38, 46),  -- Trainer 3 at Gym 1
(39, 47),  -- Trainer 4 at Gym 2
(40, 46),  -- Trainer 5 at Gym 1
(41, 48),  -- Trainer 6 at Gym 3
(42, 49),  -- Trainer 7 at Gym 4
(43, 47),  -- Trainer 8 at Gym 2
(44, 49),  -- Trainer 9 at Gym 4
(45, 48);  -- Trainer 10 at Gym 3



-- Inserting data into Membership table
INSERT INTO Membership (type, duration, cost)
VALUES
('Platinum', '2 Years', 1800.00),
('Gold', '1 Year', 1000.00),
('Silver', '6 Months', 600.00),
('Bronze', '3 Months', 300.00),
('Basic', '1 Month', 100.00);


-- Inserting data into diet table for plans created by users
INSERT INTO diet (diet_plan_name, CreatorID)
VALUES
('Weight Loss Plan', 1),    -- User 1
('Muscle Gain Plan', 2),     -- User 2
('Low Carb Diet', 3),        -- User 3
('Vegetarian Meal Plan', 4), -- User 4
('Keto Diet', 5),            -- User 5
('Paleo Diet', 6),           -- User 6
('Intermittent Fasting', 7), -- User 7
('Gluten-Free Plan', 8),     -- User 8
('Detox Cleanse', 9),        -- User 9
('Balanced Diet', 10),       -- User 10
('Vegan Lifestyle Plan', 11),      -- User 11
('Mediterranean Diet', 12),        -- User 12
('High-Fiber Meal Plan', 13),      -- User 13
('Low-Calorie Diet', 14),          -- User 14
('Whole30 Challenge', 15);         -- User 15


-- Inserting data into diet table for plans created by trainers
INSERT INTO diet (diet_plan_name, CreatorID)
VALUES
('High Protein Diet', 36),   -- Trainer 1
('Flexibility Diet', 37),    -- Trainer 2
('Cardio Boost Plan', 38),   -- Trainer 3
('CrossFit Fuel', 39),       -- Trainer 4
('Strength Building Meal Plan', 40),  -- Trainer 5
('Pilates Nutrition', 41),   -- Trainer 6
('Functional Nutrition Guide', 42),   -- Trainer 7
('Bodybuilding Meal Plan', 43),  -- Trainer 8
('Boxing Diet', 44),         -- Trainer 9
('Circuit Training Nutrition', 45); -- Trainer 10

-- Inserting data into meal table for diet plans
INSERT INTO meal (dietplanID, Day1, meal_name, Protein, Carbs, Fiber, Fat, calories, allergens)
VALUES
(1, 'Monday', 'Breakfast', 25.5, 15.7, 5.2, 10.3, 320, 'Nuts'),
(1, 'Monday', 'Lunch', 30.8, 40.5, 8.9, 12.1, 450, 'Dairy'),
(2, 'Monday', 'Dinner', 35.4, 20.6, 6.3, 14.5, 380, 'Gluten'),
(2, 'Tuesday', 'Breakfast', 22.1, 35.2, 7.8, 8.7, 300, 'Soy'),
(3, 'Tuesday', 'Lunch', 28.9, 45.6, 9.5, 16.2, 500, 'Nuts'),
(3, 'Monday', 'Breakfast', 30.2, 18.9, 6.5, 12.4, 340, 'Dairy'),
(3, 'Monday', 'Lunch', 35.6, 42.1, 10.3, 18.9, 480, 'Gluten'),
(4, 'Monday', 'Dinner', 40.3, 25.8, 8.2, 20.5, 420, 'Nuts'),
(5, 'Tuesday', 'Breakfast', 28.5, 38.7, 9.2, 14.6, 380, 'Soy'),
(5, 'Tuesday', 'Lunch', 33.9, 47.2, 11.5, 22.1, 520, 'Dairy'),
(5, 'Monday', 'Breakfast', 27.6, 20.3, 6.8, 13.7, 360, 'Nuts'),
(6, 'Monday', 'Lunch', 32.8, 44.5, 10.9, 17.5, 490, 'Gluten'),
(7, 'Monday', 'Dinner', 37.4, 28.9, 8.5, 19.2, 430, 'Dairy'),
(8, 'Tuesday', 'Breakfast', 25.1, 32.4, 8.1, 11.9, 340, 'Soy'),
(9, 'Tuesday', 'Lunch', 30.6, 50.2, 12.3, 23.8, 540, 'Nuts'),
(10, 'Monday', 'Breakfast', 25.5, 15.7, 5.2, 10.3, 320, 'Nuts'),
(10, 'Monday', 'Lunch', 30.8, 40.5, 8.9, 12.1, 450, 'Dairy'),
(11, 'Monday', 'Dinner', 35.4, 20.6, 6.3, 14.5, 380, 'Gluten'),
(11, 'Tuesday', 'Breakfast', 22.1, 35.2, 7.8, 8.7, 300, 'Soy'),
(12, 'Tuesday', 'Lunch', 28.9, 45.6, 9.5, 16.2, 500, 'Nuts'),
(12, 'Monday', 'Breakfast', 30.2, 18.9, 6.5, 12.4, 340, 'Dairy'),
(12, 'Monday', 'Lunch', 35.6, 42.1, 10.3, 18.9, 480, 'Gluten'),
(13, 'Monday', 'Dinner', 40.3, 25.8, 8.2, 20.5, 420, 'Nuts'),
(14, 'Tuesday', 'Breakfast', 28.5, 38.7, 9.2, 14.6, 380, 'Soy'),
(14, 'Tuesday', 'Lunch', 33.9, 47.2, 11.5, 22.1, 520, 'Dairy');

INSERT INTO meal (dietplanID, Day1, meal_name, Protein, Carbs, Fiber, Fat, calories, allergens)
VALUES
(15, 'Monday', 'Breakfast', 25.5, 15.7, 5.2, 10.3, 320, 'Nuts'),
(15, 'Monday', 'Lunch', 30.8, 40.5, 8.9, 12.1, 450, 'Dairy'),
(16, 'Monday', 'Dinner', 35.4, 20.6, 6.3, 14.5, 380, 'Gluten'),
(16, 'Tuesday', 'Breakfast', 22.1, 35.2, 7.8, 8.7, 300, 'Soy'),
(17, 'Tuesday', 'Lunch', 28.9, 45.6, 9.5, 16.2, 500, 'Nuts'),
(17, 'Monday', 'Breakfast', 30.2, 18.9, 6.5, 12.4, 340, 'Dairy'),
(17, 'Monday', 'Lunch', 35.6, 42.1, 10.3, 18.9, 480, 'Gluten'),
(18, 'Monday', 'Dinner', 40.3, 25.8, 8.2, 20.5, 420, 'Nuts'),
(19, 'Tuesday', 'Breakfast', 28.5, 38.7, 9.2, 14.6, 380, 'Soy'),
(19, 'Tuesday', 'Lunch', 33.9, 47.2, 11.5, 22.1, 520, 'Dairy'),
(19, 'Monday', 'Breakfast', 27.6, 20.3, 6.8, 13.7, 360, 'Nuts'),
(20, 'Monday', 'Lunch', 32.8, 44.5, 10.9, 17.5, 490, 'Gluten'),
(21, 'Monday', 'Dinner', 37.4, 28.9, 8.5, 19.2, 430, 'Dairy'),
(22, 'Tuesday', 'Breakfast', 25.1, 32.4, 8.1, 11.9, 340, 'Soy'),
(22, 'Tuesday', 'Lunch', 30.6, 50.2, 12.3, 23.8, 540, 'Nuts'),
(23, 'Monday', 'Breakfast', 25.5, 15.7, 5.2, 10.3, 320, 'Nuts'),
(23, 'Monday', 'Lunch', 30.8, 40.5, 8.9, 12.1, 450, 'Dairy'),
(24, 'Monday', 'Dinner', 35.4, 20.6, 6.3, 14.5, 380, 'Gluten'),
(24, 'Tuesday', 'Breakfast', 22.1, 35.2, 7.8, 8.7, 300, 'Soy'),
(25, 'Tuesday', 'Lunch', 28.9, 45.6, 9.5, 16.2, 500, 'Nuts');

INSERT INTO Exercise (TargetMuscle, ExerciseName, Machine)
VALUES
--no machine type
('Legs', 'Box Jumps', null),
('Back', 'Renegade Rows', null),
('Legs', 'Step-Ups', null),
('Full Body', 'Bodyweight Squats', NULL),
('Cardio', 'Jumping Jacks', NULL),
('Legs', 'Dumbbell Lunges', 'Dumbbell'),
('Cardio', 'Burpees', NULL),
('Upper Body', 'Push-Ups', NULL),
('Legs', 'Deadlifts', 'Barbell'),
('Core', 'Plank', NULL),

('Chest', 'Bench Press', 'Barbell'),
('Legs', 'Squats', 'Dumbbell'),
('Back', 'Deadlifts', 'Barbell'),
('Shoulders', 'Military Press', 'Barbell'),
('Arms', 'Bicep Curls', 'Dumbbell'),
('Chest', 'Push-Ups', 'Bodyweight'),
('Legs', 'Lunges', 'Bodyweight'),
('Back', 'Pull-Ups', 'Pull-Up Bar'),
('Shoulders', 'Lateral Raises', 'Dumbbell'),
('Arms', 'Tricep Dips', 'Bodyweight'),
('Chest', 'Incline Bench Press', 'Barbell'),
('Legs', 'Leg Press', 'Leg Press Machine'),
('Back', 'Lat Pulldowns', 'Lat Pulldown Machine'),
('Shoulders', 'Front Raises', 'Dumbbell'),
('Arms', 'Hammer Curls', 'Dumbbell'),
('Chest', 'Chest Flys', 'Cable Machine'),
('Legs', 'Deadlifts', 'Trap Bar'),
('Back', 'Bent Over Rows', 'Barbell'),
('Shoulders', 'Arnold Press', 'Dumbbell'),
('Arms', 'Skull Crushers', 'EZ Bar'),
('Chest', 'Dumbbell Pullover', 'Dumbbell'),
('Legs', 'Leg Curls', 'Leg Curl Machine'),
('Back', 'Seated Rows', 'Cable Machine'),
('Shoulders', 'Reverse Flys', 'Dumbbell'),
('Arms', 'Preacher Curls', 'Preacher Curl Bench'),
('Chest', 'Chest Press Machine', 'Chest Press Machine'),
('Legs', 'Calf Raises', 'Smith Machine'),
('Back', 'Hyperextensions', 'Hyperextension Bench'),
('Shoulders', 'Shrugs', 'Dumbbell'),
('Arms', 'Tricep Pushdowns', 'Cable Machine');



INSERT INTO WorkoutPlans (PlanName, CreatorID)
VALUES
('Beginner Full Body', 1),
('Weight Loss Circuit', 2),
('Strength Training for Women', 3),
('High Intensity Interval Training (HIIT)', 4),
('Upper Body Strength', 5),
('Lower Body Blast', 6),
('Core Strengthening', 7),
('Flexibility and Mobility', 8),
('Cardio Kickboxing', 9),
('Functional Fitness', 10),

-- Unique workout plans for IDs 11-15
('Advanced Strength Training', 11),
('Endurance Cycling Workout', 12),
('Flexibility and Balance Routine', 13),
('MMA Conditioning Circuit', 14),
('Outdoor Bootcamp Challenge', 15),



-- Plans created by trainers (UserIDs 36-45)

('Bodybuilding Hypertrophy Program', 36),
('Yoga for Strength and Flexibility', 37),
('Functional Core Training', 38),
('Advanced Cardiovascular Endurance', 39),
('Olympic Weightlifting Mastery', 40),
('Pilates for Posture and Alignment', 41),
('Dynamic Warm-Up and Cool Down', 42),
('Speed and Agility for Sports', 43),
('Powerlifting Strength Program', 44),
('Metabolic Conditioning Circuit', 45);


INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
-- Plan 1: Beginner Full Body
(1, 'Monday', 3, 10, 60, 1),
(1, 'Monday', 3, 12, 60, 2),
(1, 'Monday', 3, 10, 60, 3),
(1, 'Wednesday', 3, 12, 60, 4),
(1, 'Wednesday', 3, 10, 60, 5),
(1, 'Wednesday', 3, 12, 60, 6),
(1, 'Friday', 3, 10, 60, 7),
(1, 'Friday', 3, 12, 60, 8),
(1, 'Friday', 3, 10, 60, 9),
-- Plan 2: Weight Loss Circuit
(2, 'Tuesday', 4, 15, 45, 10),
(2, 'Tuesday', 4, 20, 45, 11),
(2, 'Tuesday', 4, 15, 45, 12),
(2, 'Thursday', 4, 20, 45, 13),
(2, 'Thursday', 4, 15, 45, 14),
(2, 'Thursday', 4, 20, 45, 15),
-- Plan 3: Strength Training for Women
(3, 'Monday', 3, 10, 60, 16),
(3, 'Monday', 3, 12, 60, 17),
(3, 'Monday', 3, 10, 60, 18),
(3, 'Wednesday', 3, 12, 60, 19),
(3, 'Wednesday', 3, 10, 60, 20),
(3, 'Wednesday', 3, 12, 60, 21),
-- Plan 4: High Intensity Interval Training (HIIT)
(4, 'Tuesday', 5, 30, 30, 22),
(4, 'Tuesday', 5, 25, 30, 23),
(4, 'Tuesday', 5, 30, 30, 24),
(4, 'Thursday', 5, 25, 30, 25),
(4, 'Thursday', 5, 30, 30, 26),
(4, 'Thursday', 5, 25, 30, 27),
-- Plan 5: Upper Body Strength
(5, 'Monday', 4, 8, 90, 28),
(5, 'Monday', 4, 10, 90, 29),
(5, 'Wednesday', 4, 8, 90, 30),


-- Plan 6: Core Strengthening
(6, 'Wednesday', 3, 15, 45, 16),
(6, 'Wednesday', 3, 20, 45, 17),
(6, 'Wednesday', 3, 15, 45, 18),
(6, 'Friday', 3, 20, 45, 19),
(6, 'Friday', 3, 15, 45, 20),
(6, 'Friday', 3, 20, 45, 21),
-- Plan 7: Flexibility and Mobility
(7, 'Monday', 2, 8, 60, 22),
(7, 'Monday', 2, 10, 60, 23),
(7, 'Wednesday', 2, 8, 60, 24),
(7, 'Wednesday', 2, 10, 60, 25),
(7, 'Friday', 2, 8, 60, 26),
(7, 'Friday', 2, 10, 60, 27),
-- Plan 8: Cardio Kickboxing
(8, 'Tuesday', 4, 12, 45, 28),
(8, 'Tuesday', 4, 15, 45, 29),
(8, 'Thursday', 4, 12, 45, 30),
(8, 'Thursday', 4, 15, 45, 31),
(8, 'Saturday', 4, 12, 45, 32),
(8, 'Saturday', 4, 15, 45, 33),
-- Plan 9: Functional Fitness
(9, 'Monday', 3, 10, 60, 34),
(9, 'Monday', 3, 12, 60, 35),
(9, 'Wednesday', 3, 10, 60, 36),
(9, 'Wednesday', 3, 12, 60, 37),
(9, 'Friday', 3, 10, 60, 38),
(9, 'Friday', 3, 12, 60, 39),
-- Plan 10: Muscle Building Power Workout
(10, 'Monday', 4, 8, 90, 40),
(10, 'Monday', 4, 10, 90, 31),
(10, 'Wednesday', 4, 8, 90, 32),
(10, 'Wednesday', 4, 10, 90, 33),
(10, 'Friday', 4, 8, 90, 34),
(10, 'Friday', 4, 10, 90, 35);

-- Plan 11: Advanced Strength Training
INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
(11, 'Monday', 4, 8, 60, 1),
(11, 'Monday', 4, 10, 60, 2),
(11, 'Monday', 4, 8, 60, 3),
(11, 'Wednesday', 4, 10, 60, 4),
(11, 'Wednesday', 4, 8, 60, 5),
(11, 'Wednesday', 4, 10, 60, 6);

-- Plan 12: Endurance Cycling Workout
INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
(12, 'Tuesday', 1, 60, 0, 7),
(12, 'Thursday', 1, 60, 0, 8),
(12, 'Saturday', 1, 60, 0, 9);

-- Plan 13: Flexibility and Balance Routine
INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
(13, 'Monday', 1, 30, 0, 10),
(13, 'Wednesday', 1, 30, 0, 11),
(13, 'Friday', 1, 30, 0, 12);

-- Plan 14: MMA Conditioning Circuit
INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
(14, 'Tuesday', 5, 15, 30, 13),
(14, 'Thursday', 5, 15, 30, 14),
(14, 'Saturday', 5, 15, 30, 15);

-- Plan 15: Outdoor Bootcamp Challenge
INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
(15, 'Monday', 4, 12, 45, 16),
(15, 'Wednesday', 4, 12, 45, 17),
(15, 'Friday', 4, 12, 45, 18);

-- Plan 16: Plyometric Power Session
INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
(16, 'Tuesday', 4, 10, 45, 19),
(16, 'Thursday', 4, 10, 45, 20),
(16, 'Saturday', 4, 10, 45, 21);

-- Plan 17: Total Body Barre Workout
INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
(17, 'Monday', 3, 12, 60, 22),
(17, 'Wednesday', 3, 12, 60, 23),
(17, 'Friday', 3, 12, 60, 24);

-- Plan 18: TRX Suspension Training
INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
(18, 'Tuesday', 4, 12, 45, 25),
(18, 'Thursday', 4, 12, 45, 26),
(18, 'Saturday', 4, 12, 45, 27);

-- Plan 19: Urban Dance Fitness Class
INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
(19, 'Monday', 1, 60, 0, 28),
(19, 'Wednesday', 1, 60, 0, 29),
(19, 'Friday', 1, 60, 0, 30);

-- Plan 20: Vinyasa Yoga Flow
INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
(20, 'Tuesday', 1, 60, 0, 31),
(20, 'Thursday', 1, 60, 0, 32),
(20, 'Saturday', 1, 60, 0, 33);

-- Plan 21: Muscle Building Power Workout
INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
(21, 'Monday', 4, 8, 60, 34),
(21, 'Monday', 4, 10, 60, 35),
(21, 'Wednesday', 4, 8, 60, 1),
(21, 'Wednesday', 4, 10, 60, 2),
(21, 'Friday', 4, 8, 60, 3),
(21, 'Friday', 4, 10, 60, 4);

-- Plan 22: Advanced Plyometric Circuit
INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
(22, 'Tuesday', 5, 12, 45, 5),
(22, 'Tuesday', 5, 15, 45, 6),
(22, 'Thursday', 5, 12, 45, 7),
(22, 'Thursday', 5, 15, 45, 8),
(22, 'Saturday', 5, 12, 45, 9),
(22, 'Saturday', 5, 15, 45, 10);

-- Plan 23: Endurance and Stamina Training
INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
(23, 'Monday', 3, 12, 60, 11),
(23, 'Monday', 3, 15, 60, 12),
(23, 'Wednesday', 3, 12, 60, 13),
(23, 'Wednesday', 3, 15, 60, 14),
(23, 'Friday', 3, 12, 60, 15),
(23, 'Friday', 3, 15, 60, 16);

-- Plan 24: Agility and Speed Drills
INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
(24, 'Tuesday', 4, 10, 45, 17),
(24, 'Tuesday', 4, 12, 45, 18),
(24, 'Thursday', 4, 10, 45, 19),
(24, 'Thursday', 4, 12, 45, 20),
(24, 'Saturday', 4, 10, 45, 21),
(24, 'Saturday', 4, 12, 45, 22);

-- Plan 25: Athletic Performance Enhancement
INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID)
VALUES
(25, 'Monday', 3, 10, 60, 23),
(25, 'Monday', 3, 12, 60, 24),
(25, 'Wednesday', 3, 10, 60, 25),
(25, 'Wednesday', 3, 12, 60, 26),
(25, 'Friday', 3, 10, 60, 27),
(25, 'Friday', 3, 12, 60, 28);



-- MemberID, GymID, workoutID, mID, dietID, JoinDate, Height, Weight
-- Member 1-5 to Fitness Junction (GymID 46)
INSERT INTO Members (MemberID, GymID, workoutID, mID, dietID, JoinDate, Height, Weight)
VALUES
(1, 46, NULL, 1, 1, '2024-05-01', 170.0, 70.0),
(2, 46, NULL, 2, 2, '2024-05-02', 165.0, 68.0),
(3, 46, NULL, 3, 3, '2024-05-03', 175.0, 75.0),
(4, 46, NULL, 4, 4, '2024-05-04', 180.0, 80.0),
(5, 46, NULL, 5, 5, '2024-05-05', 160.0, 65.0);

-- Member 6-10 to GymXtreme (GymID 47)
INSERT INTO Members (MemberID, GymID, workoutID, mID, dietID, JoinDate, Height, Weight)
VALUES
(6, 47, NULL, 1, 6, '2024-05-06', 175.0, 72.0),
(7, 47, NULL, 2, 7, '2024-05-07', 165.0, 68.0),
(8, 47, NULL, 3, 8, '2024-05-08', 170.0, 70.0),
(9, 47, NULL, 4, 9, '2024-05-09', 172.0, 69.0),
(10, 47, NULL, 5, 10, '2024-05-10', 180.0, 75.0);

-- Member 11-15 to FitZone (GymID 48)
INSERT INTO Members (MemberID, GymID, workoutID, mID, dietID, JoinDate, Height, Weight)
VALUES
(11, 48, NULL, 1, 11, '2024-05-11', 160.0, 65.0),
(12, 48, NULL, 2, 12, '2024-05-12', 155.0, 60.0),
(13, 48, NULL, 3, 13, '2024-05-13', 170.0, 68.0),
(14, 48, NULL, 4, 14, '2024-05-14', 168.0, 67.0),
(15, 48, NULL, 5, 15, '2024-05-15', 175.0, 70.0);

-- Member 16-20 to Fitness World (GymID 49)
INSERT INTO Members (MemberID, GymID, workoutID, mID, dietID, JoinDate, Height, Weight)
VALUES
(16, 49, NULL, 1, 16, '2024-05-16', 185.0, 80.0),
(17, 49, NULL, 2, 17, '2024-05-17', 170.0, 70.0),
(18, 49, NULL, 3, 18, '2024-05-18', 175.0, 75.0),
(19, 49, NULL, 4, 19, '2024-05-19', 172.0, 72.0),
(20, 49, NULL, 5, 20, '2024-05-20', 180.0, 78.0);

-- Member 21-25 to Fitness Junction (GymID 46)
INSERT INTO Members (MemberID, GymID, workoutID, mID, dietID, JoinDate, Height, Weight)
VALUES
(21, 46, NULL, 1, 21, '2024-05-21', 170.0, 70.0),
(22, 46, NULL, 2, 22, '2024-05-22', 165.0, 68.0),
(23, 46, NULL, 3, 23, '2024-05-23', 175.0, 75.0),
(24, 46, NULL, 4, 24, '2024-05-24', 180.0, 80.0),
(25, 46, NULL, 5, 25, '2024-05-25', 160.0, 65.0);

-- Member 26-30 to GymXtreme (GymID 47)
INSERT INTO Members (MemberID, GymID, workoutID, mID, dietID, JoinDate, Height, Weight)
VALUES
(26, 47, NULL, 1, 1, '2024-05-26', 175.0, 72.0),
(27, 47, NULL, 2, 2, '2024-05-27', 165.0, 68.0),
(28, 47, NULL, 3, 3, '2024-05-28', 170.0, 70.0),
(29, 47, NULL, 4, 4, '2024-05-29', 172.0, 69.0),
(30, 47, NULL, 5, 5, '2024-05-30', 180.0, 75.0);

-- Member 31-35 to FitZone (GymID 48)
INSERT INTO Members (MemberID, GymID, workoutID, mID, dietID, JoinDate, Height, Weight)
VALUES
(31, 48, NULL, 1, 6, '2024-05-31', 160.0, 65.0),
(32, 48, NULL, 2, 7, '2024-06-01', 155.0, 60.0),
(33, 48, NULL, 3, 8, '2024-06-02', 170.0, 68.0),
(34, 48, NULL, 4, 9, '2024-06-03', 168.0, 67.0),
(35, 48, NULL, 5, 10, '2024-06-04', 175.0, 70.0);


INSERT INTO TrainerRatings (TrainerID, MemberID, Rating, Feedback)
VALUES
-- Trainer 1 ratings
(36, 1, 4, 'Great trainer, very knowledgeable.'),
(36, 2, 5, 'Excellent sessions, saw good results.'),
(36, 3, 3, 'Could improve on explaining exercises.'),

-- Trainer 2 ratings
(37, 6, 4, 'Always pushes me to do my best.'),

-- Trainer 3 ratings
(38, 11, 5, 'Very motivating, helps me stay focused.'),
(38, 12, 4, 'Clear instructions and helpful tips.'),
(38, 13, 3, 'Sometimes runs late for sessions.'),

-- Trainer 4 ratings
(39, 16, 5, 'Great expertise in designing workout plans.'),

-- Trainer 5 ratings
(40, 20, 4, 'Good at adapting exercises for different fitness levels.'),
(40, 21, 5, 'Always encouraging and supportive.'),

-- Trainer 6 ratings
(41, 23, 3, 'Could provide more variety in workouts.'),

-- Trainer 7 ratings
(42, 24, 4, 'Responsive to my goals and needs.'),
(42, 25, 5, 'Highly recommend, best trainer I''ve had.'),
(42, 26, 4, 'Highly professional and dedicated.'),

-- Trainer 8 ratings
(43, 29, 5, 'Very knowledgeable and experienced.'),

-- Trainer 9 ratings
(44, 30, 4, 'Excellent communication and guidance.'),

-- Trainer 10 ratings
(45, 34, 3, 'Could be more punctual for sessions.');



-- Inserting data into PersonalTrainingSessions table
INSERT INTO PersonalTrainingSessions (TrainerID, MemberID, SessionDate, SessionTime,Status1)
VALUES
-- Session 
-- Session 1 (Trainer 1 with Member 1 at Gym 1)
(36, 1, '2024-05-01', '09:00:00',1),

-- Session 2 (Trainer 1 with Member 2 at Gym 1)
(36, 2, '2024-05-02', '10:00:00',1),

-- Session 3 (Trainer 1 with Member 3 at Gym 1)
(36, 3, '2024-05-03', '11:00:00',1),

-- Session 4 (Trainer 1 with Member 4 at Gym 1)
(36, 4, '2024-05-04', '12:00:00',0),

-- Session 5 (Trainer 1 with Member 5 at Gym 1)
(36, 5, '2024-05-05', '13:00:00',0),

-- Session 6 (Trainer 2 with Member 6 at Gym 2)
(37, 6, '2024-05-06', '09:00:00',0),

-- Session 7 (Trainer 2 with Member 7 at Gym 2)
(37, 7, '2024-05-07', '10:00:00',1),

-- Session 8 (Trainer 2 with Member 8 at Gym 2)
(37, 8, '2024-05-08', '11:00:00',0),

-- Session 9 (Trainer 2 with Member 9 at Gym 2)
(37, 9, '2024-05-09', '12:00:00',1),

-- Session 10 (Trainer 2 with Member 10 at Gym 2)
(37, 10, '2024-05-10', '13:00:00',0),
-- Additional sessions for Trainer 6 (TrainerID 41)
-- Gym 3 (FitZone)
(41, 11, '2024-05-06', '09:00:00',0), -- Member 11
(41, 12, '2024-05-07', '10:00:00',0), -- Member 12
(41, 13, '2024-05-08', '11:00:00',1), -- Member 13
(41, 14, '2024-05-09', '12:00:00',1), -- Member 14
(41, 15, '2024-05-10', '13:00:00',1), -- Member 15

-- Additional sessions for Trainer 7 (TrainerID 42)
-- Gym 4 (Fitness World)
(42, 16, '2024-05-11', '09:00:00',0), -- Member 16
(42, 17, '2024-05-12', '10:00:00',1), -- Member 17
(42, 18, '2024-05-13', '11:00:00',1), -- Member 18
(42, 19, '2024-05-14', '12:00:00',0), -- Member 19
(42, 20, '2024-05-15', '13:00:00',0), -- Member 20

-- Additional sessions for Trainer 8 (TrainerID 43)
-- Gym 2 (GymXtreme)
(43, 6, '2024-05-16', '09:00:00',0), -- Member 6
(43, 7, '2024-05-17', '10:00:00',1), -- Member 7
(43, 8, '2024-05-18', '11:00:00',0), -- Member 8
(43, 9, '2024-05-19', '12:00:00',1), -- Member 9
(43, 10, '2024-05-20', '13:00:00',0), -- Member 10

-- Additional sessions for Trainer 9 (TrainerID 44)
-- Gym 4 (Fitness World)
(44, 16, '2024-05-21', '09:00:00',1), -- Member 16
(44, 17, '2024-05-22', '10:00:00',1), -- Member 17
(44, 18, '2024-05-23', '11:00:00',0), -- Member 18
(44, 19, '2024-05-24', '12:00:00',0), -- Member 19
(44, 20, '2024-05-25', '13:00:00',0), -- Member 20

-- Additional sessions for Trainer 10 (TrainerID 45)
-- Gym 3 (FitZone)
(45, 11, '2024-05-26', '09:00:00',0), -- Member 11
(45, 12, '2024-05-27', '10:00:00',0), -- Member 12
(45, 13, '2024-05-28', '11:00:00',1), -- Member 13
(45, 14, '2024-05-29', '12:00:00',1), -- Member 14
(45, 15, '2024-05-30', '13:00:00',1); -- Member 15

--query1:
--query1:
/*SELECT FullName, Email, Contact
FROM Users
inner join Members m on m.MemberID = Users.UserID
INNER JOIN worksat w ON m.GymID = w.gymid
INNER JOIN Trainers t ON w.trainer = t.TrainerID
WHERE t.TrainerID = 38 AND m.GymID = 46;


--query2
SELECT FullName, Email, Contact
FROM Users
inner join Members m on m.MemberID = Users.UserID
INNER JOIN Owners o ON m.GymID = o.OwnerID
INNER JOIN diet d ON m.dietID = d.dietID
WHERE o.GymName = 'Fitness Junction' AND d.diet_plan_name = 'Weight Loss Plan';

--query3
SELECT u.FullName, u.Email, u.Contact, o.GymName
FROM Users u
inner join Members m on m.MemberID = u.UserID
INNER JOIN Owners o ON m.GymID = o.OwnerID
INNER JOIN Trainers t ON EXISTS (
    SELECT 1
    FROM worksat w
    WHERE w.trainer = t.TrainerID AND w.gymid = m.GymID
)
INNER JOIN diet d ON m.dietID = d.dietID
WHERE t.TrainerID = 37 AND d.diet_plan_name = 'Weight Loss Plan';
select * from Members
select * from WorkoutPlans
select * from Exercise
select * from ROUTINE

--query4
SELECT count(*) AS MEMBERCOUNT
from members m
JOIN WorkoutPlans wp ON m.MemberID = wp.CreatorID
JOIN ROUTINE r ON wp.PlanID = r.PlanID
JOIN Exercise e ON r.ExerciseID = e.ExerciseID
WHERE m.GymID = 46 -- Specify the GymID
 AND r.Day1 = 'Wednesday' -- Specify the given date
 AND e.Machine = 'Dumbbell'
 group by e.Machine

--query5
SELECT d.diet_plan_name
FROM diet d
INNER JOIN meal m ON d.dietID = m.dietplanID
WHERE m.meal_name = 'Breakfast'
GROUP BY d.diet_plan_name
HAVING SUM(calories) < 500;

--query6
SELECT d.diet_plan_name
FROM diet d
INNER JOIN meal m ON d.dietID = m.dietplanID
GROUP BY d.diet_plan_name
HAVING SUM(Carbs) < 300;

--QUERY7
SELECT p.PlanName FROM WorkoutPlans p WHERE NOT EXISTS (SELECT 1 FROM ROUTINE r JOIN Exercise e ON r.ExerciseID = e.ExerciseID WHERE p.PlanID = r.PlanID AND e.Machine = 'Dumbbell') GROUP BY p.PlanName--query7


--query8
SELECT d.diet_plan_name
FROM diet d
WHERE d.dietID NOT IN (
  SELECT dietplanID
  FROM meal m
  WHERE m.allergens LIKE '%Nuts%'
);

--query9
SELECT FullName, Email, Contact, m.JoinDate, m.Weight, m.Height,ms.type as Type,ms.duration as Duration,ms.cost as Cost
FROM users
INNER JOIN Members m ON m.MemberID = users.UserID
INNER JOIN Owners o ON m.GymID = o.OwnerID
INNER JOIN Membership ms ON m.mID=ms.mID
WHERE m.JoinDate >= DATEADD(MONTH, -3, GETDATE()) and o.OwnerID=46;

--QUERY10
SELECT o.GymName, COUNT(m.MemberID) AS TotalMembers
FROM Owners o
INNER JOIN Members m ON m.GymID = o.OwnerID
WHERE m.JoinDate >= DATEADD(MONTH, -6, GETDATE())
GROUP BY o.GymName
ORDER BY TotalMembers DESC

---Details of members who have had session with specific trainer
--query11
SELECT u.FullName, u.Email, u.Contact
FROM Users u
JOIN Members m on m.MemberID = u.UserID
JOIN PersonalTrainingSessions pts ON m.MemberID = pts.MemberID
JOIN Trainers t ON pts.TrainerID = t.TrainerID
WHERE t.TrainerID = 36
--List of diet plans with meals that contain at least 20 grams of protein per serving.
--query12
SELECT Distinct d.diet_plan_name, d.CreatorID
FROM diet d
JOIN meal m ON d.dietID = m.dietplanID
WHERE m.Protein >= 20;

--List of workout plans that involve exercises targeting Chest back or Legs, sorted by plan name.
--query13
SELECT Distinct wp.PlanName, wp.CreatorID
FROM WorkoutPlans wp
JOIN ROUTINE r ON wp.PlanID = r.PlanID
JOIN Exercise e ON r.ExerciseID = e.ExerciseID
WHERE e.TargetMuscle IN ('Chest', 'Back', 'Legs') -- Specify muscle groups
ORDER BY wp.PlanName;

--List of diet plans that have meals with a specific allergen, sorted by diet plan name.
--query14
SELECT Distinct d.diet_plan_name, d.CreatorID
FROM diet d
JOIN meal m ON d.dietID = m.dietplanID
WHERE m.allergens NOT LIKE '%Nuts%' -- Specify allergen
ORDER BY d.diet_plan_name;

--List of workout plans that involve exercises with a specific machine, sorted by plan name.
--query15
SELECT Distinct wp.PlanName, wp.CreatorID
FROM WorkoutPlans wp
JOIN ROUTINE r ON wp.PlanID = r.PlanID
JOIN Exercise e ON r.ExerciseID = e.ExerciseID
WHERE e.Machine = 'Dumbbell'
ORDER BY wp.PlanName;

--Details of members who have followed a specific diet plan in the last 3 months, sorted by join date.
--query16
SELECT u.FullName, u.Email, u.Contact
FROM Users u
JOIN Members m on m.MemberID = u.UserID
JOIN diet d ON m.dietID = d.dietID
WHERE d.diet_plan_name = 'Weight Loss Plan'
  AND m.JoinDate >= DATEADD(MONTH, -3, GETDATE())
ORDER BY m.JoinDate;


--Find the total number of a specific membership type aqquired by members of a specific gym
--Query17
SELECT m.mID, COUNT(*) AS Membership
FROM Members m
JOIN Owners on m.GymID = Owners.OwnerID
where m.mID = 1 AND GymID = 46
group by m.mID
--Query to find routines with plan on weekends
--query18
select Distinct WorkoutPlans.PlanID,WorkoutPlans.PlanName,WorkoutPlans.CreatorID from WorkoutPlans
JOIN ROUTINE on  WorkoutPlans.PlanID = ROUTINE.PlanID
where ROUTINE.Day1 in ('Saturday','Sunday')

--query19
select SUM(m.cost), Owners.GymName
from Owners
join Members on Members.GymID = Owners.OwnerID
Join Membership m on m.mID = Members.mID
group by Owners.GymName

--List of diet plans that have meals with the highest calorie count, sorted by plan name.
--query20
SELECT d.diet_plan_name, d.CreatorID, MAX(m.calories) AS MaxCalories
FROM diet d
JOIN meal m ON d.dietID = m.dietplanID
GROUP BY d.diet_plan_name, d.CreatorID
ORDER BY MaxCalories DESC;

     SELECT m.mID,ms.type as Type, COUNT(*) AS Membership FROM Members m JOIN Owners on m.GymID = Owners.OwnerID Join Membership ms on ms.mID=m.mID WHERE ms.type Like '{membershipType}' AND GymID = {GetGymID(gymName)} GROUP BY m.mID,ms.type
	 */