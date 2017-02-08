using System;
using System.Collections.Generic;
using System.Text;

namespace NetStitch.Server
{
    public static class HttpStatus
    {
        public static int OK => (int)System.Net.HttpStatusCode.OK;
        public static int NoContent => (int)System.Net.HttpStatusCode.NoContent;
        public static int BadRequest => (int)System.Net.HttpStatusCode.BadRequest;
        public static int NotFound => (int)System.Net.HttpStatusCode.NotFound;
        public static int InternalServerError => (int)System.Net.HttpStatusCode.InternalServerError;
    }
}
