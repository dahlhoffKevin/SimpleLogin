using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace SimpleLogin
{
    public class SimpleLogin
    {
        public static readonly string _encryptKey = "b14ca5898a4e4133bbce2ea2315a1916";
        private static bool _konami = false;

        private enum MenuOptions
        {
            Exit = 0,
            Login = 1,
            Register = 2,
            ShowAllUsers = 3,
            SearchUser = 4,
            ResetPassword = 5,
            ChangeUsername = 6,
            DeleteUser = 7,
            KonamiCode = 3254785
        }

        public static void Main()
        {
            using var database = new UserContext();

            while (true)
            {
                PrintMenu();

                bool _ = int.TryParse(Console.ReadLine() ?? "1", out int start);

                if (!Enum.IsDefined(typeof(MenuOptions), start))
                {
                    Console.WriteLine("Invalid Input!\n");
                    continue;
                }

                switch ((MenuOptions)start)
                {
                    case MenuOptions.Exit:
                        Environment.Exit(1);
                        break;
                    case MenuOptions.Login:
                        ProcessLogin(database);
                        break;
                    case MenuOptions.Register:
                        ProcessRegistration(database);
                        break;
                    case MenuOptions.ShowAllUsers:
                        ShowAllUsers(database);
                        Console.WriteLine("");
                        break;
                    case MenuOptions.SearchUser:
                        SearchUser(database);
                        Console.WriteLine("");
                        break;
                    case MenuOptions.ResetPassword:
                        ResetUserPassword(database);
                        Console.WriteLine("");
                        break;
                    case MenuOptions.ChangeUsername:
                        ChangeUsername(database);
                        Console.WriteLine("");
                        break;
                    case MenuOptions.DeleteUser:
                        DeleteUser(database);
                        Console.WriteLine("");
                        break;
                    case MenuOptions.KonamiCode:
                        ToggleKonamiCode();
                        break;
                    default:
                        break;
                }
            }
        }

        private static void PrintMenu()
        {
            Console.WriteLine("Input Exit : 0 | " +
                              "Login : 1 | " +
                              "Register : 2 | " +
                              "Show All User: 3 | " +
                              "Search User: 4 | " +
                              "Reset Password: 5 | " +
                              "Change Username: 6 | " +
                              "Delete User 7");
        }

        /// <summary>
        /// Returns the username entered by the user
        /// </summary>
        /// <returns></returns>
        private static string GetUsername()
        {
            Console.WriteLine("Input Username:");
            return Console.ReadLine() ?? "";
        }

        /// <summary>
        /// Returns the password entered by the user
        /// </summary>
        /// <returns></returns>
        private static string GetUsersecret()
        {
            Console.WriteLine("Input Password:");
            return SimpleEncrypter.EncryptString(_encryptKey, Console.ReadLine() ?? "");
        }

        /// <summary>
        /// Returns the recovery key entered by the user
        /// </summary>
        /// <returns></returns>
        private static string GetUserRecoveryKey()
        {
            Console.WriteLine("Input your recovery key:");
            return SimpleEncrypter.EncryptString(_encryptKey, Console.ReadLine() ?? "");
        }

        /// <summary>
        /// Returns the new userscret entered by the user
        /// </summary>
        /// <returns></returns>
        private static string GetNewUsersecret()
        {
            Console.WriteLine("Input new Password:");
            string newUsersecret = Console.ReadLine() ?? "";

            while (string.IsNullOrEmpty(newUsersecret))
            {
                Console.WriteLine("Your new Password should not be blanck!");
                newUsersecret = Console.ReadLine() ?? "";
            }

            return SimpleEncrypter.EncryptString(_encryptKey, newUsersecret);
        }

        /// <summary>
        /// Checks if the entered password is correct
        /// </summary>
        /// <param name="database"></param>
        /// <param name="username"></param>
        /// <param name="usersecret"></param>
        /// <returns></returns>
        private static bool CheckUserCredential(UserContext database, string username, string usersecret)
        {
            if (database.Users.Any(u => u.Username == username && u.Usersecret == usersecret)) return true;
            return false;
        }

        /// <summary>
        /// Checks if the entered username is already in use
        /// </summary>
        /// <param name="database"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        private static bool CheckIfUserExists(UserContext database, string username)
        {
            if (database.Users.Any(u => u.Username == username)) return true;
            return false;
        }

        /// <summary>
        /// Adds a new user to the database
        /// </summary>
        /// <param name="database"></param>
        /// <param name="username"></param>
        /// <param name="usersecret"></param>
        /// <returns></returns>
        private static bool AddNewUser(UserContext database, string username, string usersecret)
        {
            if (string.IsNullOrEmpty(username)) return false;
            if (usersecret == SimpleEncrypter.EncryptString(_encryptKey, string.Empty)) return false;
            if (CheckIfUserExists(database, username)) return false;

            string recoveryKey = RecoveryKeyGenerator.GenerateRecoveryKey();
            string encryptedRecoveryKey = SimpleEncrypter.EncryptString(_encryptKey, recoveryKey);

            Console.WriteLine($"This is your recovery key, please keep this key safe and dont lose it, you will need this key to reset your password if neccessary:\n{recoveryKey}");

            database.Add(new User() { Id = new Guid(), Username = username, Usersecret = usersecret, RecoveryKey = encryptedRecoveryKey });
            database.SaveChanges();

            return true;
        }

        /// <summary>
        /// Checks if the entered username and password are correct
        /// </summary>
        /// <param name="database"></param>
        private static void ProcessLogin(UserContext database)
        {
            if (CheckUserCredential(database, GetUsername(), GetUsersecret()))
                Console.WriteLine("Login successful!\n");
            else
                Console.WriteLine("Login failed! Username or Password incorrect\n");
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="database"></param>
        private static void ProcessRegistration(UserContext database)
        {
            if (AddNewUser(database, GetUsername(), GetUsersecret()))
                Console.WriteLine("Successfully registered!\n");
            else
                Console.WriteLine("Registration failed: User already exists!\n");
        }

        /// <summary>
        /// Prints a list of all registered users in the database
        /// </summary>
        /// <param name="database"></param>
        private static void ShowAllUsers(UserContext database)
        {
            List<User> user = database.Users.ToList();
            Console.WriteLine($"Users found: {user.Count}");
            foreach (User u in user)
            {
                if (!_konami)
                    Console.WriteLine($"Username: {u.Username}");
                else
                    Console.WriteLine(
                        $"Username: {u.Username} | User Infos:" +
                        $"\n\tGUID: {u.Id}" +
                        $"\n\tPasswort: {SimpleEncrypter.DecryptString(_encryptKey, u.Usersecret ?? "") ?? "<No password submitted>"}" +
                        $"\n\tRecoveryKey: {SimpleEncrypter.DecryptString(_encryptKey, u.RecoveryKey ?? "") ?? "<No recovery key submitted>"}"
                    );
            }
        }

        /// <summary>
        /// Prints a searched user if found
        /// </summary>
        /// <param name="database"></param>
        private static void SearchUser(UserContext database)
        {
            string username = GetUsername();
            Guid newGuid = new();
            User user = database.Users
                .Where(u => u.Username == username)
                .FirstOrDefault() ?? new User() { Id = newGuid, Username = $"{newGuid}-null", Usersecret = $"{newGuid}-null" };

            if (user.Username == $"{newGuid}-null")
            {
                Console.WriteLine("User not found");
                return;
            }

            Console.WriteLine($"1 User found:\n{user.Username}");
        }

        /// <summary>
        /// Resets the password of a user
        /// </summary>
        /// <param name="database"></param>
        private static void ResetUserPassword(UserContext database)
        {
            string username = GetUsername();
            string encryptedRecoveryKey = GetUserRecoveryKey();

            var user = database.Users
                .Where(u => u.Username == username)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(encryptedRecoveryKey) || user == null)
            {
                Console.WriteLine("Invalid input or user not found!");
                return;
            }

            if (user.RecoveryKey != encryptedRecoveryKey)
            {
                Console.WriteLine("Your recovery key is incorrect!");
                return;
            }

            string newUsersecret = GetNewUsersecret();
            if (string.IsNullOrEmpty(newUsersecret))
                return;

            user.Usersecret = newUsersecret;
            database.SaveChanges();

            Console.WriteLine("Password updated!\n");

            string newRecoveryKey = RecoveryKeyGenerator.GenerateRecoveryKey();
            string newEncryptedRecoveryKey = SimpleEncrypter.EncryptString(_encryptKey, newRecoveryKey);
            user.RecoveryKey = newEncryptedRecoveryKey;
            database.SaveChanges();
            Console.WriteLine("Your recovery key was updated. New recovery key: " + newRecoveryKey);
        }

        /// <summary>
        /// Changes the username of a user
        /// </summary>
        /// <param name="database"></param>
        private static void ChangeUsername(UserContext database)
        {
            string username = GetUsername();
            string usersecret = GetUsersecret();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(usersecret))
            {
                Console.WriteLine("Username or Password is empty!");
                return;
            }

            if (!CheckIfUserExists(database, username) || !CheckUserCredential(database, username, usersecret))
            {
                Console.WriteLine("Invalid username or password!");
                return;
            }

            Console.WriteLine("Input new Username:");
            string newUsername = Console.ReadLine() ?? "";
            if (string.IsNullOrEmpty(newUsername))
            {
                Console.WriteLine("Username is empty!");
                return;
            }

            var user = database.Users
                .Where(u => u.Username == username)
                .FirstOrDefault();

            if (user == null)
            {
                Console.WriteLine("User not found!");
                return;
            }

            user.Username = newUsername;
            database.SaveChanges();
            Console.WriteLine("Username updated!");
        }

        /// <summary>
        /// Deletes a user from the database
        /// </summary>
        /// <param name="database"></param>
        private static void DeleteUser(UserContext database)
        {
            string username = GetUsername();
            string userSecret = GetUsersecret();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userSecret))
            {
                Console.WriteLine("Username or Password is empty!");
                return;
            }

            if (!CheckIfUserExists(database, username) || !CheckUserCredential(database, username, userSecret))
            {
                Console.WriteLine("Invalid username or password!");
                return;
            }

            var user = database.Users
                .Where(u => u.Username == username)
                .FirstOrDefault();

            if (user == null)
            {
                Console.WriteLine("User not found!");
                return;
            }

            database.Users.Remove(user);
            database.SaveChanges();
            Console.WriteLine("User deleted!");
        }

        /// <summary>
        /// Toggle the Konami Code
        /// </summary>
        private static void ToggleKonamiCode()
        {
            _konami = !_konami;
        }
    }
}
