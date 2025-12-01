using GamingThroughVoiceRecognitionSystem.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace GamingThroughVoiceRecognitionSystem.Database
{
    public class DbConn
    {
        private readonly string connectionString;
        private SqlConnection conn;

        public DbConn()
        {
            connectionString = ConfigurationManager.ConnectionStrings["GamingDB"].ConnectionString;
            conn = new SqlConnection(connectionString);
        }

        #region Connection Handling
        public SqlConnection Conn => conn;

        public void OpenConnection()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
        }

        public void CloseConnection()
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
        #endregion

        #region Helper → GetData
        public DataTable GetData(string query, SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();
            try
            {
                OpenConnection();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            finally
            {
                CloseConnection();
            }
            return dt;
        }
        #endregion

        #region Password Hashing
        public string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (var b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
        #endregion

        #region User Management

        // Add new user (signup)
        public bool AddUser(UserModel user)
        {
            try
            {
                OpenConnection();
                string query = @"INSERT INTO user_info (FullName, Age, Email, PasswordHash)
                                 VALUES (@FullName, @Age, @Email, @PasswordHash)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@Age", user.Age);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(user.PasswordHash));

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        // Add user with face data
        public bool AddUserWithFace(UserModel user, byte[] faceData)
        {
            try
            {
                OpenConnection();
                string query = @"INSERT INTO user_info (FullName, Age, Email, PasswordHash, FaceData)
                                 VALUES (@FullName, @Age, @Email, @PasswordHash, @FaceData)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@Age", user.Age);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(user.PasswordHash));
                    cmd.Parameters.AddWithValue("@FaceData", faceData ?? (object)DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        // Add user with voice data
        public bool AddUserWithVoice(UserModel user, byte[] voiceData)
        {
            try
            {
                OpenConnection();
                string query = @"INSERT INTO user_info (FullName, Age, Email, PasswordHash, VoiceData)
                                 VALUES (@FullName, @Age, @Email, @PasswordHash, @VoiceData)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@Age", user.Age);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(user.PasswordHash));
                    cmd.Parameters.AddWithValue("@VoiceData", voiceData ?? (object)DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        // Update user face data
        public bool UpdateUserFaceData(int userId, byte[] faceData)
        {
            try
            {
                OpenConnection();
                string query = @"UPDATE user_info SET FaceData = @FaceData WHERE UserID = @UserID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@FaceData", faceData);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        // Update user voice data
        public bool UpdateUserVoiceData(int userId, byte[] voiceData)
        {
            try
            {
                OpenConnection();
                string query = @"UPDATE user_info SET VoiceData = @VoiceData WHERE UserID = @UserID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@VoiceData", voiceData);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        // Get user face data
        public byte[] GetUserFaceData(int userId)
        {
            try
            {
                OpenConnection();
                string query = @"SELECT FaceData FROM user_info WHERE UserID = @UserID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    object result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? (byte[])result : null;
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        // Store face data for existing user
        public bool StoreFaceData(int userId, byte[] faceData)
        {
            return UpdateUserFaceData(userId, faceData);
        }

        // Store voice data for existing user
        public bool StoreVoiceData(int userId, byte[] voiceData)
        {
            return UpdateUserVoiceData(userId, voiceData);
        }

        // Authenticate with face recognition
        public bool AuthenticateWithFace(byte[] capturedFaceData, out int userId)
        {
            userId = -1;
            try
            {
                OpenConnection();
                // Get all users with face data
                string query = @"SELECT UserID, FaceData FROM user_info WHERE FaceData IS NOT NULL";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int currentUserId = dr.GetInt32(0);
                            byte[] storedFaceData = (byte[])dr["FaceData"];

                            // Simple comparison - in production, use ML face recognition
                            if (CompareBiometricData(capturedFaceData, storedFaceData))
                            {
                                userId = currentUserId;
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

        // Authenticate with voice recognition
        public bool AuthenticateWithVoice(byte[] capturedVoiceData, out int userId)
        {
            userId = -1;
            try
            {
                OpenConnection();
                // Get all users with voice data
                string query = @"SELECT UserID, VoiceData FROM user_info WHERE VoiceData IS NOT NULL";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int currentUserId = dr.GetInt32(0);
                            byte[] storedVoiceData = (byte[])dr["VoiceData"];

                            // Simple comparison - in production, use ML voice recognition
                            if (CompareBiometricData(capturedVoiceData, storedVoiceData))
                            {
                                userId = currentUserId;
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

        // Simple biometric data comparison (placeholder for ML-based comparison)
        private bool CompareBiometricData(byte[] data1, byte[] data2)
        {
            if (data1 == null || data2 == null || data1.Length != data2.Length)
                return false;

            // Simple byte-by-byte comparison
            // In production, this should use ML models for face/voice recognition
            // For now, we use exact match with some tolerance
            int matchingBytes = 0;
            int totalBytes = data1.Length;

            for (int i = 0; i < totalBytes; i++)
            {
                if (data1[i] == data2[i])
                    matchingBytes++;
            }

            // Allow 95% similarity threshold
            double similarity = (double)matchingBytes / totalBytes;
            return similarity >= 0.95;
        }

        // SignUp method (wrapper for AddUser)
        public bool SignUp(string fullName, int age, string email, string password)
        {
            UserModel user = new UserModel
            {
                FullName = fullName,
                Age = age,
                Email = email,
                PasswordHash = password
            };
            return AddUser(user);
        }

        // Login method
        public bool Login(string email, string password, out int userId)
        {
            return ValidateLogin(email, password, out userId);
        }

        // Validate login credentials
        public bool ValidateLogin(string email, string password, out int userId)
        {
            userId = -1;
            try
            {
                OpenConnection();
                string query = @"SELECT UserID FROM user_info 
                                 WHERE Email=@Email AND PasswordHash=@PasswordHash";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(password));

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        userId = Convert.ToInt32(result);
                        return true;
                    }
                    return false;
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        // Get user by ID
        public UserModel GetUserById(int userId)
        {
            UserModel user = null;
            try
            {
                OpenConnection();
                string query = @"SELECT UserID, FullName, Age, Email 
                                 FROM user_info WHERE UserID=@UserID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            user = new UserModel
                            {
                                UserId = dr.GetInt32(0),
                                FullName = dr.GetString(1),
                                Age = dr.GetInt32(2),
                                Email = dr.GetString(3)
                            };
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }
            return user;
        }

        // Get user by Email (for profile)
        public UserModel GetUserByEmail(string email)
        {
            UserModel user = null;
            try
            {
                OpenConnection();
                string query = @"SELECT UserID, FullName, Age, Email 
                                 FROM user_info WHERE Email=@Email";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            user = new UserModel
                            {
                                UserId = dr.GetInt32(0),
                                FullName = dr.GetString(1),
                                Age = dr.GetInt32(2),
                                Email = dr.GetString(3)
                            };
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }
            return user;
        }

        // Update user profile
        public bool UpdateUserProfile(UserModel user)
        {
            try
            {
                OpenConnection();
                string query = @"UPDATE user_info 
                                 SET FullName=@FullName, Age=@Age, Email=@Email
                                 WHERE UserID=@UserID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@Age", user.Age);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@UserID", user.UserId);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        #endregion

        #region Games Management
        public List<GameModel> GetAvailableGames()
        {
            var games = new List<GameModel>();
            try
            {
                OpenConnection();
                string query = "SELECT GameID, GameName, FilePath FROM games";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            games.Add(new GameModel
                            {
                                GameId = dr.GetInt32(0),
                                GameName = dr.GetString(1),
                                FilePath = dr.GetString(2)
                            });
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }
            return games;
        }
        #endregion

        #region Voice Data Management (Optional)
        public bool AddVoiceSample(int userId, byte[] voiceData)
        {
            try
            {
                OpenConnection();
                string query = "INSERT INTO user_voice_data (UserID, VoiceSample) VALUES (@UserID, @VoiceSample)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@VoiceSample", voiceData);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        public List<byte[]> GetVoiceSamples(int userId)
        {
            List<byte[]> samples = new List<byte[]>();
            try
            {
                OpenConnection();
                string query = "SELECT VoiceSample FROM user_voice_data WHERE UserID=@UserID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            samples.Add((byte[])dr["VoiceSample"]);
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }
            return samples;
        }
        #endregion
    }
}
