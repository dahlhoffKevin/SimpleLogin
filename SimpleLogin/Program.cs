using Microsoft.Data.Sqlite;
using System;
using System.IO.Pipes;
using System.Net;
using System.Net.NetworkInformation;

namespace SimpleLogin
{
    public class SimpleLogin
    {
        public static readonly string _encryptKey = "b14ca5898a4e4133bbce2ea2315a1916";

        public static void Main()
        {
            while (true)
            {
                Console.WriteLine("Input Exit : 0 | Login : 1 | Register : 2");

                bool result = int.TryParse(Console.ReadLine() ?? "1", out int start);

                if (result == false || !new List<int>() { 0,1,2 }.Contains(start))
                {
                    Console.WriteLine("Wrong input\n");
                    continue;
                }

                if (start == 0) Environment.Exit(1);

                string username = GetUsername();
                string password = GetPassword();

                switch (start)
                {
                    case 1:
                        if (CheckUserCredential(username, password)) Console.WriteLine("Login successfull!\n");
                        else Console.WriteLine("Login failed! Username or Password incorrect\n");
                        break;

                    case 2:
                        if (AddNewUser(username, password)) Console.WriteLine("Successfully registered!\n");
                        else Console.WriteLine("Registration failed!\n");
                        break;

                    default:
                        break;
                }
            }
        }

        private static string GetUsername()
        {
            Console.WriteLine("Input Username:");
            return Console.ReadLine() ?? "";
        }

        private static string GetPassword()
        {
            Console.WriteLine("Input Password:");
            return SimpleEncrypter.EncryptString(_encryptKey, Console.ReadLine() ?? "");
        }

        private static bool AddNewUser(string username, string password)
        {
            if (CheckIfUserExists(username)) return false;

            using var database = new UserContext();
            database.Add(new User() { Id = new Guid(), Username = username, Usersecret = password });
            database.SaveChanges();

            return true;
        }

        private static bool CheckUserCredential(string username, string password)
        {
            using var database = new UserContext();
            if (database.Users.Any(u => u.Username == username && u.Usersecret == password)) return true;
            return false;
        }

        private static bool CheckIfUserExists(string username)
        {
            using var database = new UserContext();
            if (database.Users.Any(u => u.Username == username)) return true;
            return false;
        }
    }
}