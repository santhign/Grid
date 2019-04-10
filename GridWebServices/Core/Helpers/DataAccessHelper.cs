using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Core.Helpers
{
    /// <summary>
    /// Data Access Helper class
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class DataAccessHelper : IDisposable
    {
        /// <summary>
        /// The command
        /// </summary>
        private SqlCommand command;

        /// <summary>
        /// Returns connneciton string form configuration
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public string GetConnection(IConfiguration configuration)
        {
            return configuration.GetConnectionString("DefaultConnection");
        }

        /// <summary>
        /// Run SP without parameters
        /// </summary>
        /// <param name="sprocName">Name of the sproc.</param>
        /// <param name="configuration">The configuration.</param>
        public DataAccessHelper(string sprocName, IConfiguration configuration)
        {
            //creating command object with connection name and proc name, and open connection for the command
            command = new SqlCommand(sprocName, new SqlConnection(GetConnection(configuration)));
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(
                new SqlParameter("ReturnValue",
                    SqlDbType.Int,
                /* int size */ 4,
                    ParameterDirection.ReturnValue,
                /* bool isNullable */ false,
                /* byte precision */ 0,
                /* byte scale */ 0,
                /* string srcColumn */ string.Empty,
                    DataRowVersion.Default,
                /* value */ null
                )
            );
            command.CommandTimeout = 0;
            command.Connection.Open();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessHelper"/> class.
        /// </summary>
        /// <param name="sprocName">Name of the sproc.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="configuration">The configuration.</param>
        public DataAccessHelper(string sprocName, SqlParameter[] parameters, IConfiguration configuration)
        {
            // prepare a command object with procedure and parameters
            command = new SqlCommand(sprocName, new SqlConnection(GetConnection(configuration)));
            command.CommandType = CommandType.StoredProcedure;

            foreach (SqlParameter parameter in parameters)
                command.Parameters.Add(parameter);

            command.Parameters.Add(
                new SqlParameter("ReturnValue",
                    SqlDbType.Int,
                /* int size */ 4,
                    ParameterDirection.ReturnValue,
                /* bool isNullable */ false,
                /* byte precision */ 0,
                /* byte scale */ 0,
                /* string srcColumn */ string.Empty,
                    DataRowVersion.Default,
                /* value */ null
                )
            );
            command.CommandTimeout = 0;
            command.Connection.Open();
        }


        /// <summary>
        /// Run command with executeNonQuery
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public int Run()
        {
            // Execute this stored procedure.  Int32 value returned by the stored procedure
            if (command == null)
                throw new ObjectDisposedException(GetType().FullName);
            command.ExecuteNonQuery();
            return (int)command.Parameters["ReturnValue"].Value;
        }

        /// <summary>
        /// Run command  with data adapter: fill datatable
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public int Run(DataTable dataTable)
        {
            //	Fill a DataTable with the result of executing this stored procedure.
            if (command == null)
                throw new ObjectDisposedException(GetType().FullName);

            SqlDataAdapter dataAdapter = new SqlDataAdapter();

            dataAdapter.SelectCommand = command;
            dataAdapter.Fill(dataTable);

            return (int)command.Parameters["ReturnValue"].Value; 
        }

        /// <summary>
        /// Run command  with data adapter: fill dataset
        /// </summary>
        /// <param name="dataSet">The data set.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public int Run(DataSet dataSet)
        {
            //	Fill a DataSet with the result of executing this stored procedure.
            if (command == null)
                throw new ObjectDisposedException(GetType().FullName);

            SqlDataAdapter dataAdapter = new SqlDataAdapter();

            dataAdapter.SelectCommand = command;
            dataAdapter.Fill(dataSet);
            return (int)command.Parameters["ReturnValue"].Value;
        }

        /// <summary>
        /// Runs the asynchronous.
        /// </summary>
        /// <returns>interger value to check if database call executed successfully or not</returns>
        public async Task<int> RunAsync()
        {
            return await Task.Run(() =>
            {
                // Execute this stored procedure.  Int32 value returned by the stored procedure
                if (command == null)
                    throw new ObjectDisposedException(GetType().FullName);
                command.ExecuteNonQuery();
                return (int)command.Parameters["ReturnValue"].Value;
            });

        }

        /// <summary>
        /// Runs the asynchronous.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <returns>interger value to check if database call executed successfully or not</returns>
        public async Task<int> RunAsync(DataTable dataTable)
        {

            return await Task.Run(() =>
             {
                //	Fill a DataTable with the result of executing this stored procedure.
                if (command == null)
                     throw new ObjectDisposedException(GetType().FullName);

                 SqlDataAdapter dataAdapter = new SqlDataAdapter
                 {
                     SelectCommand = command
                 };
                 dataAdapter.Fill(dataTable);

                 return (int)command.Parameters["ReturnValue"].Value;
             });

                // It should never come to this line as it should get return from above
            
        }

        /// <summary>
        /// Runs the asynchronous to fetch the Data from DB.
        /// </summary>
        /// <param name="dataSet">The data set.</param>
        /// <returns>interger value to check if database call executed successfully or not</returns>
        public async Task<int> RunAsync(DataSet dataSet)
        {
            return await Task.Run(() =>
            {
                //	Fill a DataTable with the result of executing this stored procedure.
                if (command == null)
                    throw new ObjectDisposedException(GetType().FullName);

                var dataAdapter = new SqlDataAdapter
                {
                    SelectCommand = command
                };
                dataAdapter.Fill(dataSet);

                return (int)command.Parameters["ReturnValue"].Value;
            });
        }

        /// <summary>
        /// Dispose connection and command objects
        /// </summary>
        public void Dispose()
        {
            //	Dispose of this StoredProcedure.
            if (command != null)
            {
                SqlConnection connection = command.Connection;
                Debug.Assert(connection != null);
                connection.Close();
                command.Dispose();
                command = null;
                connection.Dispose();
            }
        }

    }
}
