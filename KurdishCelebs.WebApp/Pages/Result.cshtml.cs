using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KurdishCelebs.WebApp.Pages
{
    public class ResultModel : PageModel
    {
        public string Id { get; set; }

        public void OnGet(string id)
        {
            Id = id;
        }
    }
}
