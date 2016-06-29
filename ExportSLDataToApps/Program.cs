﻿using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net;

namespace ExportSLDataToApps
{
   class Program
   {
      static void Main(string[] args)
      {
         var companies = new List<string>();
         companies.Add("ari");
         companies.Add("agt");

         foreach (var company in companies)
         {
            // Connect to SL database defined in App.config
            Console.WriteLine("Connecting to {0} database...", company);
            var connectionString = ConfigurationManager.ConnectionStrings[company].ConnectionString;
            var source = new DataSource(connectionString);

            if (source == null)
            {
               Console.WriteLine("Error establishing database connection. Exiting.");
               return;
            }

            // Get list of ARI projects and cost codes
            Console.WriteLine("Getting list of projects from SL...");
            var projects = Project.FetchActiveProjects(source);

            Console.WriteLine("Getting list of cost codes from SL...");
            var costCodes = CostCode.FetchCostCodes(source);
            Project.MapCostCodesToProjectList(projects, costCodes);

            // Send mapped project list to API
            Console.WriteLine("Sending projects to web API...");
            var apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
            var apiUsername = ConfigurationManager.AppSettings["ApiUsername"];
            var apiPassword = ConfigurationManager.AppSettings["ApiPassword"];

            var client = new RestClient(apiBaseUrl);
            client.Authenticator = new HttpBasicAuthenticator(apiUsername, apiPassword);
            var request = new RestRequest(company, Method.POST);
            var requestBody = JsonConvert.SerializeObject(projects);
            request.AddParameter("data", requestBody);

            try
            {
               var response = client.Execute(request);
               Console.WriteLine(response.Content);
            }
            catch (Exception e)
            {
               Console.WriteLine("Server error:");
               Console.WriteLine(e);
            }
         }
                  
         Console.WriteLine("Press any key to exit...");
         Console.ReadKey();
      }
   }
}
