using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Wikiled.Twitter.Monitor.Service.Requests
{
    public class HistoryRequest
    {
        [Required]
        [FromRoute]
        public string Keyword { get; set; }

        [Required]
        [FromRoute]
        public  int Hours { get; set; }
    }
}
