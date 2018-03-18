using System;
using System.Collections.Generic;

namespace SeriesTracker.Models
{
	public class User
	{
		private int userId;
		private string username;
		private string email;
		private string firstName;
		private string lastName;
		private DateTime? dateOfBirth;
		private string password;

		public int UserID
		{
			get { return userId; }
			set { userId = value; }
		}
		public string Username
		{
			get { return username; }
			set { username = value; }
		}
		public string Email
		{
			get { return email; }
			set { email = value; }
		}
		public string FirstName
		{
			get { return firstName; }
			set { firstName = value; }
		}
		public string LastName
		{
			get { return lastName; }
			set { lastName = value; }
		}
		public DateTime? DateOfBirth
		{
			get { return dateOfBirth; }
			set { dateOfBirth = value; }
		}
		public string Password
		{
			get { return password; }
			set { password = value; }
		}

		public List<Show> Shows { get; set; }
		public List<Category> Categories { get; set; }

		public User()
		{
			//UserID = -1;

			//DateOfBirth = null;

			//Shows = new List<Show>();
			//Categories = new List<Category>();
		}
	}
}
