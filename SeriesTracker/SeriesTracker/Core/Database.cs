using SeriesTracker.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SeriesTracker.Core
{
	public class Database
	{
		private SqlConnection connection;
		private SqlCommand cmd;
		private SqlDataReader reader;

		private bool useOnlineDatabase = true;

		public Database()
		{
			if (useOnlineDatabase)
			{
				// Online
				//connection = new SqlConnection("Server=seriestracker.database.windows.net;Initial Catalog=SeriesTracker;Persist Security Info=True;User ID=seriestrackeruser;Password=STUser!@#;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=15;");
				connection = new SqlConnection("Server=seriestracker.cw1deesdx7ti.eu-central-1.rds.amazonaws.com,1433;Initial Catalog=SeriesTracker;User ID=seriestrackeruser;Password=STUser123!;Connection Timeout=15;");
			}
			else
			{
				// Local
				connection = new SqlConnection("Server=localhost;Database=SeriesTracker;Integrated Security=True");
			}
		}

		#region User
		public LoginResult UserLogin(string UsernameOrEmail, string Password)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserLogin");
				cmd.Parameters.AddWithValue("Email", UsernameOrEmail);
				cmd.Parameters.AddWithValue("Password", sha256(Password));
				SqlDataReader reader = cmd.ExecuteReader();

				if (reader.HasRows)
				{
					reader.Read();

					DateTime? DoB = null;
					if (!string.IsNullOrEmpty(reader["DateOfBirth"].ToString())) DoB = DateTime.Parse(reader["DateOfBirth"].ToString());

					User data = new User
					{
						UserID = int.Parse(reader["UserID"].ToString()),
						Username = reader["Username"].ToString(),
						FirstName = reader["Name"].ToString(),
						LastName = reader["Surname"].ToString(),
						Email = reader["Email"].ToString(),
						Password = Password,
						DateOfBirth = DoB
					};
					reader.Close();

					return new LoginResult { Result = SQLResult.LoginSuccessful, UserData = data };
				}
				else
				{
					return new LoginResult { Result = SQLResult.BadLogin };
				}
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
				return new LoginResult { Result = SQLResult.ErrorHasOccured, Message = ex.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<LoginResult> UserLoginAsync(string UsernameOrEmail, string Password)
		{
			return await Task.Run(() => UserLogin(UsernameOrEmail, Password));
		}

		public SeriesResult<User> UserRegister(string Username, string Email, string Name, string Password)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserRegister");
				cmd.Parameters.AddWithValue("Username", Username);
				cmd.Parameters.AddWithValue("Email", Email);
				cmd.Parameters.AddWithValue("Name", Name);
				cmd.Parameters.AddWithValue("Password", sha256(Password));
				cmd.Parameters.Add("Result", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;
				cmd.ExecuteNonQuery();

				if (cmd.Parameters["Result"].Value.ToString() == "Registered")
				{
					return new SeriesResult<User> { Result = SQLResult.RegistrationSuccessful, Message = "Registered" };
				}
				else if (cmd.Parameters["Result"].Value.ToString() == "UsernameTaken")
				{
					return new SeriesResult<User> { Result = SQLResult.UsernameAlreadyRegistered, Message = "Username already in use" };
				}
				else if (cmd.Parameters["Result"].Value.ToString() == "EmailTaken")
				{
					return new SeriesResult<User> { Result = SQLResult.EmailAlreadyRegistered, Message = "Email already registered" };
				}
				else
				{
					return new SeriesResult<User> { Result = SQLResult.ErrorHasOccured, Message = "No Message" };
				}
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
				return new SeriesResult<User> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<User>> UserRegisterAsync(string Username, string Email, string Name, string Password)
		{
			return await Task.Run(() => UserRegister(Username, Email, Name, Password));
		}

		public SeriesResult<User> UserUpdate(User UserData)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserUpdate");
				cmd.Parameters.AddWithValue("UserID", UserData.UserID);
				cmd.Parameters.AddWithValue("Name", UserData.FirstName);
				cmd.Parameters.AddWithValue("Surname", UserData.LastName);
				cmd.Parameters.AddWithValue("Password", sha256(UserData.Password));
				cmd.Parameters.AddWithValue("DoB", UserData.DateOfBirth);
				int r = cmd.ExecuteNonQuery();

				return new SeriesResult<User> { Result = SQLResult.Success };
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
				return new SeriesResult<User> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<User>> UserUpdateAsync(User UserData)
		{
			return await Task.Run(() => UserUpdate(UserData));
		}

		public SeriesResult<User> UserDelete()
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserDelete");
				cmd.Parameters.AddWithValue("UserID", AppGlobal.User.UserID.ToString());
				cmd.ExecuteNonQuery();

				return new SeriesResult<User> { Result = SQLResult.Success };
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);

				return new SeriesResult<User> { Result = SQLResult.ErrorHasOccured, Message = ex.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<User>> UserDeleteAsync()
		{
			return await Task.Run(() => UserDelete());
		}
		#endregion

		#region UserShow
		public SeriesResult<Show> UserShowList()
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserShowList");
				cmd.Parameters.AddWithValue("UserID", AppGlobal.User.UserID);
				reader = cmd.ExecuteReader();

				// Check to see if the user has data online
				List<Show> data = new List<Show>();
				if (reader.HasRows)
				{
					Show current = null;
					while (reader.Read())
					{
						int UserShowID = int.Parse(reader["UserShowID"].ToString());

						// New show so add to data
						if (current == null || current.UserShowID != UserShowID)
						{
							if (current != null)
								data.Add(current);

							current = new Show(UserShowID, int.Parse(reader["TvdbID"].ToString()), reader["SeriesName"].ToString());
						}

						//current.UserShowID = UserShowID;
						//current.Id = int.Parse(reader["TvdbID"].ToString());
						//current.SeriesName = reader["SeriesName"].ToString();

						if (!string.IsNullOrEmpty(reader["UserShowCategoryID"].ToString()))
						{
							current.Categories.Add(new Category
							{
								UserShowCategoryID = int.Parse(reader["UserShowCategoryID"].ToString()),
								CategoryID = int.Parse(reader["CategoryID"].ToString()),
								Name = reader["CategoryName"].ToString()
							});
						}
					}

					if (data.Count == 0 || data.Last().UserShowID != current.UserShowID)
					{
						data.Add(current);
					}
				}
				reader.Close();

				return new SeriesResult<Show> { Result = SQLResult.Success, ListData = data };
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
				return new SeriesResult<Show> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<Show>> UserShowListAsync()
		{
			return await Task.Run(() => UserShowList());
		}

		public SeriesResult<Show> UserShowAdd(Show record)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserShowAdd");
				cmd.Parameters.AddWithValue("UserID", AppGlobal.User.UserID);
				cmd.Parameters.AddWithValue("TvdbID", record.Id);
				cmd.Parameters.AddWithValue("TvdbName", record.SeriesName);
				cmd.Parameters.Add("ID", SqlDbType.Int).Direction = ParameterDirection.Output;
				cmd.ExecuteNonQuery();

				record.UserShowID = int.Parse(cmd.Parameters["ID"].Value.ToString());

				return new SeriesResult<Show> { Result = SQLResult.Success, Data = record };
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
				return new SeriesResult<Show> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<Show>> UserShowAddAsync(Show record)
		{
			return await Task.Run(() => UserShowAdd(record));
		}

		public SeriesResult<Show> UserShowAddMultiple(List<Show> records)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserShowAdd");
				foreach (Show s in records)
				{
					cmd.Parameters.Clear();

					cmd.Parameters.AddWithValue("UserID", AppGlobal.User.UserID);
					cmd.Parameters.AddWithValue("TvdbID", s.Id);
					cmd.Parameters.AddWithValue("TvdbName", s.SeriesName);
					cmd.Parameters.Add("ID", SqlDbType.Int).Direction = ParameterDirection.Output;
					cmd.ExecuteNonQuery();

					s.UserShowID = int.Parse(cmd.Parameters["ID"].Value.ToString());
				}

				return new SeriesResult<Show> { Result = SQLResult.Success, ListData = records };
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
				return new SeriesResult<Show> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<Show>> UserShowAddMultipleAsync(List<Show> records)
		{
			return await Task.Run(() => UserShowAddMultiple(records));
		}

		public SeriesResult<Show> UserShowUpdate(Show record)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserShowUpdate");
				cmd.Parameters.AddWithValue("UserID", AppGlobal.User.UserID);
				cmd.Parameters.AddWithValue("TvdbID", record.Id);
				int r = cmd.ExecuteNonQuery();

				return new SeriesResult<Show> { Result = SQLResult.Success };
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
				return new SeriesResult<Show> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<Show>> UserShowUpdateAsync(Show record)
		{
			return await Task.Run(() => UserShowUpdate(record));
		}

		public SeriesResult<Show> UserShowDelete(int UserShowID)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserShowDelete");
				cmd.Parameters.AddWithValue("UserID", AppGlobal.User.UserID);
				cmd.Parameters.AddWithValue("UserShowID", UserShowID);
				cmd.ExecuteNonQuery();

				return new SeriesResult<Show> { Result = SQLResult.Success };
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
				return new SeriesResult<Show> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<Show>> UserShowDeleteAsync(int UserShowID)
		{
			return await Task.Run(() => UserShowDelete(UserShowID));
		}
		#endregion

		#region UserShowWatch
		public SeriesResult<UserShowWatch> UserShowWatchList(Show record)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserShowWatchList");
				cmd.Parameters.AddWithValue("UserShowID", record.UserShowID);
				reader = cmd.ExecuteReader();

				List<UserShowWatch> data = new List<UserShowWatch>();
				if (reader.HasRows)
				{
					while (reader.Read())
					{
						data.Add(new UserShowWatch
						{
							ID = int.Parse(reader["UserShowWatchID"].ToString()),
							UserShowID = int.Parse(reader["UserShowID"].ToString()),
							SeasonNo = int.Parse(reader["SeasonNo"].ToString()),
							EpisodeNo = int.Parse(reader["EpisodeNo"].ToString())
						});
					}
				}
				reader.Close();

				return new SeriesResult<UserShowWatch> { Result = SQLResult.Success, ListData = data };
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
				return new SeriesResult<UserShowWatch> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<UserShowWatch>> UserShowWatchListAsync(Show record)
		{
			return await Task.Run(() => UserShowWatchList(record));
		}

		public SeriesResult<UserShowWatch> UserShowWatchAdd(UserShowWatch record)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserShowWatchAdd");
				cmd.Parameters.AddWithValue("UserShowID", record.UserShowID);
				cmd.Parameters.AddWithValue("SeasonNo", record.SeasonNo);
				cmd.Parameters.AddWithValue("EpisodeNo", record.EpisodeNo);
				cmd.Parameters.Add("ID", SqlDbType.Int).Direction = ParameterDirection.Output;
				cmd.ExecuteNonQuery();

				record.ID = int.Parse(cmd.Parameters["ID"].Value.ToString());

				return new SeriesResult<UserShowWatch> { Result = SQLResult.Success, Data = record };
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);

				return new SeriesResult<UserShowWatch> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<UserShowWatch>> UserShowWatchAddAsync(UserShowWatch record)
		{
			return await Task.Run(() => UserShowWatchAdd(record));
		}

		public SeriesResult<UserShowWatch> UserShowWatchDelete(UserShowWatch record)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserShowWatchDelete");
				cmd.Parameters.AddWithValue("UserShowWatchID", record.ID);
				cmd.ExecuteNonQuery();

				return new SeriesResult<UserShowWatch> { Result = SQLResult.Success, Data = record };
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);

				return new SeriesResult<UserShowWatch> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<UserShowWatch>> UserShowWatchDeleteAsync(UserShowWatch record)
		{
			return await Task.Run(() => UserShowWatchDelete(record));
		}
		#endregion

		#region UserCategory
		public SeriesResult<Category> UserCategoryList()
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserCategoryList");
				cmd.Parameters.AddWithValue("UserID", AppGlobal.User.UserID);
				reader = cmd.ExecuteReader();

				List<Category> data = new List<Category>();
				if (reader.HasRows)
				{
					while (reader.Read())
					{
						data.Add(new Category()
						{
							//UserShowCategoryID = int.Parse(reader["UserCategoryID"].ToString()),
							CategoryID = int.Parse(reader["CategoryID"].ToString()),
							Name = reader["Name"].ToString()
						});
					}
				}
				reader.Close();

				return new SeriesResult<Category> { Result = SQLResult.Success, ListData = data };
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
				return new SeriesResult<Category> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<Category>> UserCategoryListAsync()
		{
			return await Task.Run(() => UserCategoryList());
		}

		public SeriesResult<Category> UserCategoryAdd(Category record)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserCategoryAdd");
				cmd.Parameters.AddWithValue("UserID", AppGlobal.User.UserID);
				cmd.Parameters.AddWithValue("Category", record.Name);
				cmd.Parameters.Add("ID", SqlDbType.Int).Direction = ParameterDirection.Output;
				cmd.ExecuteNonQuery();

				record.CategoryID = int.Parse(cmd.Parameters["ID"].Value.ToString());

				return new SeriesResult<Category> { Result = SQLResult.Success, Data = record };
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
				return new SeriesResult<Category> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<Category>> UserCategoryAddAsync(Category record)
		{
			return await Task.Run(() => UserCategoryAdd(record));
		}

		public SeriesResult<Category> UserCategoryAddMultiple(List<Category> records)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserCategoryAdd");
				foreach (Category record in records)
				{
					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("UserID", AppGlobal.User.UserID);
					cmd.Parameters.AddWithValue("Category", record.Name);
					cmd.Parameters.Add("ID", SqlDbType.Int).Direction = ParameterDirection.Output;
					cmd.ExecuteNonQuery();

					record.CategoryID = int.Parse(cmd.Parameters["ID"].Value.ToString());
				}

				return new SeriesResult<Category> { Result = SQLResult.Success, ListData = records };
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
				return new SeriesResult<Category> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<Category>> UserCategoryAddMultipleAsync(List<Category> records)
		{
			return await Task.Run(() => UserCategoryAddMultiple(records));
		}

		public SeriesResult<Category> UserCategoryDelete(Category record)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserCategoryDelete");
				cmd.Parameters.AddWithValue("UserID", AppGlobal.User.UserID);
				cmd.Parameters.AddWithValue("CategoryID", record.CategoryID);
				cmd.ExecuteNonQuery();

				return new SeriesResult<Category> { Result = SQLResult.Success };
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
				return new SeriesResult<Category> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<Category>> UserCategoryDeleteAsync(Category records)
		{
			return await Task.Run(() => UserCategoryDelete(records));
		}

		public SeriesResult<Category> UserCategoryDeleteMutliple(List<Category> records)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserCategoryDelete");
				foreach (Category record in records)
				{
					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("UserID", AppGlobal.User.UserID);
					cmd.Parameters.AddWithValue("CategoryID", record.CategoryID);
					cmd.ExecuteNonQuery();
				}

				return new SeriesResult<Category> { Result = SQLResult.Success };
			}
			catch (Exception ex)
			{
				ErrorMethods.LogError(ex.Message);
				return new SeriesResult<Category> { Result = SQLResult.ErrorHasOccured, Message = ex.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<Category>> UserCategoryDeleteMutlipleAsync(List<Category> records)
		{
			return await Task.Run(() => UserCategoryDeleteMutliple(records));
		}

		public SeriesResult<Category> UserShowCategoryAdd(int UserShowID, Category record)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserShowCategoryAdd");
				cmd.Parameters.AddWithValue("UserShowID", UserShowID);
				cmd.Parameters.AddWithValue("CategoryID", record.CategoryID);
				cmd.Parameters.Add("ID", SqlDbType.Int).Direction = ParameterDirection.Output;
				cmd.ExecuteNonQuery();

				record.UserShowCategoryID = int.Parse(cmd.Parameters["ID"].Value.ToString());

				return new SeriesResult<Category> { Result = SQLResult.Success, Data = record };
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
				return new SeriesResult<Category> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<Category>> UserShowCategoryAddAsync(int UserShowID, Category record)
		{
			return await Task.Run(() => UserShowCategoryAdd(UserShowID, record));
		}

		public SeriesResult<Category> UserShowCategoryDelete(int UserShowCategoryID)
		{
			try
			{
				connection.Open();

				cmd = CreateSqlCommandForSP("UserShowCategoryDelete");
				cmd.Parameters.AddWithValue("UserShowCategoryID", UserShowCategoryID);
				cmd.ExecuteNonQuery();

				return new SeriesResult<Category> { Result = SQLResult.Success };
			}
			catch (Exception e)
			{
				ErrorMethods.LogError(e.Message);
				return new SeriesResult<Category> { Result = SQLResult.ErrorHasOccured, Message = e.Message };
			}
			finally
			{
				connection.Close();
			}
		}

		public async Task<SeriesResult<Category>> UserShowCategoryDeleteAsync(int UserShowCategoryID)
		{
			return await Task.Run(() => UserShowCategoryDelete(UserShowCategoryID));
		}
		#endregion

		#region Error logging
		public void LogError(string methodName, int lineNumber, string errorMessage)
		{
			try
			{
				connection.Open();

				cmd = new SqlCommand("LogError", connection);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("UserID",AppGlobal.User.UserID);
				cmd.Parameters.AddWithValue("MethodName", methodName);
				cmd.Parameters.AddWithValue("LineNumber", lineNumber);
				cmd.Parameters.AddWithValue("Message", errorMessage);
				cmd.ExecuteNonQuery();
			}
			finally
			{
				connection.Close();
			}
		}
		#endregion

		#region Misc
		private SqlCommand CreateSqlCommandForSP(string storedProcedure)
		{
			return new SqlCommand(storedProcedure, connection)
			{
				CommandType = CommandType.StoredProcedure
			};
		}

		private string sha256(string password)
		{
			SHA256Managed crypt = new SHA256Managed();
			string hash = String.Empty;
			byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
			foreach (byte bit in crypto)
				hash += bit.ToString("x2");
			return hash;
		}
		#endregion
	}

	public class LoginResult
	{
		public SQLResult Result;
		public string Message;

		public User UserData;
	}

	public class SeriesResult<T>
	{
		public SQLResult Result { get; set; }
		public string Message { get; set; }

		public T Data { get; set; }
		public List<T> ListData { get; set; }
	}

	public enum SQLResult
	{
		Success,
		Fail,
		LoginSuccessful,
		RegistrationSuccessful,
		ProfileUpdated,
		BadLogin,
		UsernameAlreadyRegistered,
		EmailAlreadyRegistered,
		ErrorHasOccured,
		NoChanges
	}
}
