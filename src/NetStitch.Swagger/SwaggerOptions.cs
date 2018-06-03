using Microsoft.AspNetCore.Http;
using System;

namespace NetStitch.Swagger
{
    public class SwaggerOptions
    {
        public string ApiBasePath { get; private set; }
        public Info Info { get; set; }

        public Func<HttpContext, string> CustomHost { get; set; }
        public string XmlDocumentPath { get; set; }
        public string JsonName { get; set; } = "swagger.json";
        public string[] ForceSchemas { get; set; } = Array.Empty<string>();
        public string ServerID { get; set; } = "default";

        public SwaggerOptions(string title, string description, string apiBasePath)
        {
            ApiBasePath = apiBasePath;
            Info = new Info { description = description, title = title };
        }
    }
}