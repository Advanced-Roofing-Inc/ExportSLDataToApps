using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ExportSLDataToApps
{
   class Project
   {
      public string Number { get; set; }
      public string Description { get; set; }
      public string Manufacture { get; set; }
      public string RoofType { get; set; }
      public List<CostCode> CostCodes { get; set; }

      public static List<Project> FetchActiveProjects(DataSource source)
      {
         var projectList = new List<Project>();

         using (var connection = source.CreateConnection())
         {
            var sqlText = @"
               select p.project, p.project_desc, p.pm_id02 as roof_type, pex.pm_id11 as manufacture
               from PJPROJ p
               left join pjprojex pex on p.project = pex.project
               where p.status_pa = 'A'
               and p.project <> '00Z000'";
            var sqlCommand = new SqlCommand(sqlText, connection);
            sqlCommand.Connection = connection;

            SqlDataReader reader = null;
            reader = sqlCommand.ExecuteReader();
            
            while (reader.Read())
            {
               var project = new Project
               {
                  Number = reader["project"].ToString().Trim(),
                  Description = reader["project_desc"].ToString().Trim(),
                  Manufacture = reader["manufacture"].ToString().Trim(),
                  RoofType = reader["roof_type"].ToString().Trim(),
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
