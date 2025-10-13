using Microsoft.AspNetCore.Components.Forms;

namespace BlazorApp.Model
{
    public class ExampleFormModel
    {
        public string? ExampleString { get; set; }
        public string? ExampleString2 { get; set; }
        public List<IBrowserFile>? ExampleFile { get; set; } = new List<IBrowserFile>();
    }
}
