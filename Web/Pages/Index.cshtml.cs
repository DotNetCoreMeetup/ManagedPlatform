using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Web.Pages
{
    public class IndexModel : PageModel
    {
        private HttpClient _httpClient;
        private Options _options;

        public IndexModel(HttpClient httpClient, Options options)
        {
            _httpClient = httpClient;
            _options = options;
            FileName = "Please choose image";
        }

        [BindProperty]
        public List<string> ImageList { get; private set; }

        [BindProperty]
        public IFormFile Upload { get; set; }

        [BindProperty]
        public string Delete { get; set; }

        [BindProperty]
        public string FileName { get; set; }

        public async Task OnGetAsync()
        {
            var imagesUrl = _options.ApiUrl;

            string imagesJson = await _httpClient.GetStringAsync(imagesUrl);

            IEnumerable<string> imagesList = JsonConvert.DeserializeObject<IEnumerable<string>>(imagesJson);


            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var t = Task.Run(() => DownloadImagesSynchrnously(imagesList));
            t.Wait();

            stopWatch.Stop();
            double timeSync = stopWatch.Elapsed.TotalMilliseconds;


            stopWatch.Reset();
            stopWatch.Start();

            t = Task.Run(() => DownloadImagesInParallel(imagesList));
            t.Wait();

            stopWatch.Stop();
            double timeInParallel = stopWatch.Elapsed.TotalMilliseconds;


            this.ImageList = imagesList.ToList<string>();
        }

        public async Task DownloadImagesSynchrnously(IEnumerable<string> imagesList)
        {
            foreach(string url in imagesList)
            {
                await _httpClient.GetAsync(url);
            }
        }

        public async Task DownloadImagesInParallel(IEnumerable<string> imagesList)
        {

            var tasks = imagesList.Select(url => _httpClient.GetAsync(url));

            List<Task> taskList = tasks.ToList<Task>();

            while (taskList.Any())
            {
                var finishedTask = await Task.WhenAny(taskList);
                taskList.Remove(finishedTask);
                Console.WriteLine("I'm finished! " + finishedTask.Status.ToString());
            }
        }


        public async Task<IActionResult> OnPostAsync(string Delete)
        {
            if (Upload != null && Upload.Length > 0)
            {
                var imagesUrl = _options.ApiUrl;
                FileName = Upload.Name;

                using (var image = new StreamContent(Upload.OpenReadStream()))
                {
                    image.Headers.ContentType = new MediaTypeHeaderValue(Upload.ContentType);
                    var response = await _httpClient.PostAsync(imagesUrl, image);
                }
            }
            
            if (!String.IsNullOrEmpty(Delete))
            {
                foreach(string imageUrl in ImageList)
                {
                }
            }
            
            return RedirectToPage("/Index");
        }
    }
}