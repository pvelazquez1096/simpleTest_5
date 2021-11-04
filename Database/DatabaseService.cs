using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace simpleTest_5.Database
{
    class DatabaseService
    {
        private string connectionString = @"Data Source=10.10.100.187;Initial Catalog=DBA;Persist Security Info=True;User ID=pvelazquez;Password='in5igh75734mr34d3R'";
        private string query = "SELECT TOP (10) [FirstName],[LastName],[MiddleName],[DateOfBirth],[AddressLine1],[AddressLine2],[City],[State],[ZipCode],[Country],[PrimaryPhoneNum],[EmergencyContactPhoneNum],[EmergencyContactFullName],[PrimaryEmailAddress],[HireDate],[ReportsTo],[EmployeeID],[EmployeeStatus],[PracticeArea],[CompanyDepartment],[Role],[ReportsToEmailAddress],[Discipline],[Vertical] FROM[DBA].[edw].[EmployeesDim]";
        private SqlConnection sqlCon = null;


        public void inizialliceDatabaseService()
        {
            if(sqlCon == null)
                sqlCon = new SqlConnection(connectionString);
        }

        public SqlDataReader? getUsers()
        {
            if (sqlCon == null)
                inizialliceDatabaseService();

            SqlDataReader dataReader = null;

            sqlCon.Open();

            if(sqlCon.State == System.Data.ConnectionState.Open)
            {
                using(SqlCommand cmd = sqlCon.CreateCommand())
                {
                    cmd.Connection = sqlCon;
                    cmd.CommandText = query;

                    dataReader = cmd.ExecuteReader();
                }
            }

            sqlCon.Close();
            return dataReader;
        }
    }
}
