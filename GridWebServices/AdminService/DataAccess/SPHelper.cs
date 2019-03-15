﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using System.Diagnostics;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;


namespace AdminService.DataAccess
{
    /// <summary>
    /// Helper class to execute stored procedures
    /// </summary>
    public class SPHelper : IDisposable    {
     
        private SqlCommand command;       

        /// <summary>
        /// Returns connneciton string form configuration 
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public string GetConnection(IConfiguration configuration)
        {           
            return configuration.GetConnectionString("DefaultConnection");
        }

      
        /// <summary>
        /// Run SP without parameters
        /// </summary>
        /// <param name="sprocName"></param>
        /// <param name="configuration"></param>
        public SPHelper(string sprocName, IConfiguration configuration)
        {
            //creating command object with connection name and proc name, and open connection for the command
            command = new SqlCommand(sprocName, new SqlConnection(GetConnection(configuration)));
            command.CommandType = CommandType.StoredProcedure;
            command.CommandTimeout = 0;
            command.Connection.Open();
        }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sprocName"></param>
      /// <param name="parameters"></param>
      /// <param name="configuration"></param>
        public SPHelper(string sprocName, SqlParameter[] parameters, IConfiguration configuration)
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
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public int Run(DataTable dataTable)
        {
            //	Fill a DataTable with the result of executing this stored procedure.
            if (command == null)
                throw new ObjectDisposedException(GetType().FullName);

            SqlDataAdapter dataAdapter = new SqlDataAdapter();

            dataAdapter.SelectCommand = command;
            dataAdapter.Fill(dataTable);

            return (dataTable.Rows.Count);
        }
       

        /// <summary>
        /// Run command  with data adapter: fill dataset
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
     
        public int Run(DataSet dataSet)
        {
            //	Fill a DataSet with the result of executing this stored procedure.
            if (command == null)
                throw new ObjectDisposedException(GetType().FullName);

            SqlDataAdapter dataAdapter = new SqlDataAdapter();

            dataAdapter.SelectCommand = command;
            dataAdapter.Fill(dataSet);
            return (dataSet.Tables.Count);
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
