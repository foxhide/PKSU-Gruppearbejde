using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CalendarApplication.Database;

namespace CalendarApplication.Tests
{
    [TestClass]
    public class PasswordHashingTest
    {
        private readonly int NUMBER_OF_TESTS = 400;
        private readonly int PASSWORD_MIN = 6;
        private readonly int PASSWORD_MAX = 12;
        private readonly Random random = new Random();
        private readonly string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXZabcdefghijklmnopqrstuvwxz1234567890_";

        // Tests password hashing on random strings
        [TestMethod]
        public void TestPasswordHashing()
        {
            for (int i = 0; i < NUMBER_OF_TESTS; i++)
            {
                int length = this.random.Next(PASSWORD_MAX-PASSWORD_MIN) + PASSWORD_MIN;
                string password = this.GetRandomString(length);
                string hash = PasswordHashing.CreateHash(password);

                Assert.AreEqual(hash.Length, 70);

                Assert.IsTrue(PasswordHashing.ValidatePassword(password, hash));
            }
        }

        // Create a random string
        private string GetRandomString(int length)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                Char c = this.CHARS[this.random.Next(this.CHARS.Length)];
                builder.Append(c);
            }
            return builder.ToString();
        }
    }
}