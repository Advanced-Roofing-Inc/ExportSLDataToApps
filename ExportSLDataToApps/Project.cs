﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ExportSLDataToApps
{
   class Project
   {
      public string Number { get; set; }
      public string Description { get; set; }
      public List<CostCode> CostCodes { get; set; }

      public static List<Project> FetchActiveProjects(DataSource source)
      {
         var projectList = new List<Project>();

         using (var connection = source.CreateConnection())
         {
            var sqlText = "SELECT project, project_desc FROM PJPROJ WHERE status_pa = 'A'";
            var sqlCommand = new SqlCommand(sqlText, connection);
            sqlCommand.Connection = connection;

            SqlDataReader reader = null;
            reader = sqlCommand.ExecuteReader();
            
            while (reader.Read())
            {
               var project = new Project
               {
                  Number = reader["project"].ToString().Trim(),
                  Description = reader["project_desc"].ToString(),
                  CostCodes = new List<CostCode>()
               };

               projectList.Add(project);
            }

            connection.Close();
         }

         return projectList;
      }

      public static void MapCostCodesToProjectList(List<Project> activeProjectList, List<CostCode> costCodeList)
      {
         foreach (var cc in costCodeList)
         {
            foreach (var p in activeProjectList)
            {
               if (cc.ProjectNumber.Equals(p.Number))
               {
                  p.CostCodes.Add(cc);
               }
            }
         }
      }
      
   }
}
