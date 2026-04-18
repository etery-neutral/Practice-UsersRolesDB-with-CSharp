using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

//Подключение к базе данных с помощью файла appsettings.json, в котором храняться входные данные.
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Ошибка: не найдена строка подключения в appsettings.json");
    return;
}

using var connection = new NpgsqlConnection(connectionString);
connection.Open();

//Проверка наличия таблиц в базе данных.
//Если они отсутствуют, программа самостоятельно их создаст, подгрузив инструкцию из schema.sql.
var checkTableQuery = @"
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_name = 'users'
    );";

var tableExists = connection.ExecuteScalar<bool>(checkTableQuery);

if (!tableExists)
{
    Console.WriteLine("Таблицы не найдены. Выполняю скрипт schema.sql...");

    string schemaPath = Path.Combine(AppContext.BaseDirectory, "schema.sql");
    if (!File.Exists(schemaPath))
    {
        Console.WriteLine($"Ошибка: файл schema.sql не найден по пути: {schemaPath}");
        Console.WriteLine("Проверьте, что файл скопирован в выходной каталог.");
        return;
    }

    string sqlScript = File.ReadAllText(schemaPath);

    var commands = sqlScript.Split(';', StringSplitOptions.RemoveEmptyEntries);

    foreach (var cmd in commands)
    {
        var trimmedCmd = cmd.Trim();
        if (string.IsNullOrWhiteSpace(trimmedCmd))
            continue;

        try
        {
            connection.Execute(trimmedCmd);
            Console.WriteLine($"Выполнено: {trimmedCmd.Substring(0, Math.Min(50, trimmedCmd.Length))}...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при выполнении: {trimmedCmd}");
            Console.WriteLine($"    {ex.Message}");
            return;
        }
    }

    Console.WriteLine("\nСхема базы данных успешно создана и заполнена тестовыми данными.\n");
}


//Выполнение первой задачи.
Console.WriteLine("Задача 1: Все пользователи и их роли\n");

var queryUsersWithRoles = @"
    SELECT u.Id, u.UserName, u.UserSurname, u.Email, r.Name as RoleName
    FROM Users u
    INNER JOIN UserRoles ur ON u.Id = ur.UserId
    INNER JOIN Roles r ON ur.RoleId = r.Id
    ORDER BY u.UserName, r.Name;";

var userDictionary = new Dictionary<int, User>();

var users = connection.Query<User, string, User>(
    queryUsersWithRoles,
    (user, roleName) =>
    {

        if (!userDictionary.TryGetValue(user.Id, out var currentUser))
        {
            currentUser = user;
            currentUser.Roles = new List<string>();
            userDictionary.Add(currentUser.Id, currentUser);
        }

        currentUser.Roles.Add(roleName);
        return currentUser;
    },
    splitOn: "RoleName"
).Distinct().ToList();


foreach (var user in users)
{
    Console.WriteLine($"Пользователь: {user.UserName} {user.UserSurname} ({user.Email})");
    Console.WriteLine($"  Роли: {string.Join(", ", user.Roles)}");
    Console.WriteLine();
}


//Выполнение второй задачи.
Console.WriteLine("\nЗадача 2: Количество пользователей в каждой роли");

var queryRolesCount = @"
    SELECT r.Name, COUNT(ur.UserId) as UserCount
    FROM Roles r
    LEFT JOIN UserRoles ur ON r.Id = ur.RoleId
    GROUP BY r.Id, r.Name
    ORDER BY r.Name;";

var roleCounts = connection.Query(queryRolesCount);

foreach (var row in roleCounts)
{
    Console.WriteLine($"Роль: {row.name}, Количество пользователей: {row.usercount}");
}

Console.WriteLine("\nНажмите любую клавишу для выхода...");
Console.ReadKey();


//Класс, описывающий сущность "Пользователь".
public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string UserSurname { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }

}