--- Файл schema.sql должен находиться в папках с самим кодом (Program.cs) и с исполняемым файлом (Program.exe).

CREATE TABLE Roles (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Users (
    Id SERIAL PRIMARY KEY,
    UserName VARCHAR(100) NOT NULL,
	UserSurname VARCHAR(100) NOT NULL,
    Email VARCHAR(255) NOT NULL
);

CREATE TABLE UserRoles (
    UserId INT REFERENCES Users(Id) ON DELETE CASCADE,
    RoleId INT REFERENCES Roles(Id) ON DELETE CASCADE,
    PRIMARY KEY (UserId, RoleId)
);


INSERT INTO Roles (Name) VALUES ('Администратор'), ('Модератор'), ('Пользователь'), ('Менеджер'), ('Тестировщик');

INSERT INTO Users (UserName, UserSurname, Email) VALUES 
('Антон', 'Иванов', 'ant_ivanov@example.com'),
('Мария', 'Маркова', 'marymark@example.com'),
('Сергей', 'Петров', 'sergrey@example.com'),
('Андрей', 'Козлов', 'kozlov@example.com'),
('Ольга', 'Новикова', 'olganov@example.com'),
('Алексей', 'Морозов', 'alex.moroz@example.com'),
('Илья', 'Бледный', 'palemoon@example.com');


INSERT INTO UserRoles (UserId, RoleId) VALUES 
(1, 1),
(2, 2),
(3, 3), 
(2, 3),
(4, 4),
(4, 3),
(5, 4),
(6, 3),
(6, 5),
(7, 2),
(7, 3),
(7, 5);