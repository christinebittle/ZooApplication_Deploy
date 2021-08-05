using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using System.Diagnostics;
using ZooApplication.Models;
using ZooApplication.Models.ViewModels;
using System.Web.Script.Serialization;


namespace ZooApplication.Controllers
{
    public class KeeperController : Controller
    {
        private static readonly HttpClient client;
        private JavaScriptSerializer jss = new JavaScriptSerializer();

        static KeeperController()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                //cookies are manually set in RequestHeader
                UseCookies = false
            };

            client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://localhost:44324/api/");
        }

        /// <summary>
        /// Grabs the authentication cookie sent to this controller.
        /// For proper WebAPI authentication, you can send a post request with login credentials to the WebAPI and log the access token from the response. The controller already knows this token, so we're just passing it up the chain.
        /// 
        /// Here is a descriptive article which walks through the process of setting up authorization/authentication directly.
        /// https://docs.microsoft.com/en-us/aspnet/web-api/overview/security/individual-accounts-in-web-api
        /// </summary>
        private void GetApplicationCookie()
        {
            string token = "";
            //HTTP client is set up to be reused, otherwise it will exhaust server resources.
            //This is a bit dangerous because a previously authenticated cookie could be cached for
            //a follow-up request from someone else. Reset cookies in HTTP client before grabbing a new one.
            client.DefaultRequestHeaders.Remove("Cookie");
            if (!User.Identity.IsAuthenticated) return;

            HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies.Get(".AspNet.ApplicationCookie");
            if (cookie != null) token = cookie.Value;

            //collect token as it is submitted to the controller
            //use it to pass along to the WebAPI.
            Debug.WriteLine("Token Submitted is : " + token);
            if (token != "") client.DefaultRequestHeaders.Add("Cookie", ".AspNet.ApplicationCookie=" + token);

            return;
        }

        // GET: Keeper/List
        public ActionResult List()
        {
            //objective: communicate with our Keeper data api to retrieve a list of Keepers
            //curl https://localhost:44324/api/Keeperdata/listkeepers


            string url = "keeperdata/listkeepers";
            HttpResponseMessage response = client.GetAsync(url).Result;

            //Debug.WriteLine("The response code is ");
            //Debug.WriteLine(response.StatusCode);

            IEnumerable<KeeperDto> Keepers = response.Content.ReadAsAsync<IEnumerable<KeeperDto>>().Result;
            //Debug.WriteLine("Number of Keepers received : ");
            //Debug.WriteLine(Keepers.Count());


            return View(Keepers);
        }

        // GET: Keeper/Details/5
        public ActionResult Details(int id)
        {
            DetailsKeeper ViewModel = new DetailsKeeper();

            //objective: communicate with our Keeper data api to retrieve one Keeper
            //curl https://localhost:44324/api/Keeperdata/findkeeper/{id}

            string url = "keeperdata/findKeeper/" + id;
            HttpResponseMessage response = client.GetAsync(url).Result;

            Debug.WriteLine("The response code is ");
            Debug.WriteLine(response.StatusCode);

            KeeperDto SelectedKeeper = response.Content.ReadAsAsync<KeeperDto>().Result;
            Debug.WriteLine("Keeper received : ");
            Debug.WriteLine(SelectedKeeper.KeeperFirstName);

            ViewModel.SelectedKeeper = SelectedKeeper;

            //show all animals under the care of this keeper
            url = "animaldata/listanimalsforkeeper/" + id;
            response = client.GetAsync(url).Result;
            IEnumerable<AnimalDto> KeptAnimals = response.Content.ReadAsAsync<IEnumerable<AnimalDto>>().Result;

            ViewModel.KeptAnimals = KeptAnimals;


            return View(ViewModel);
        }

        public ActionResult Error()
        {

            return View();
        }

        // GET: Keeper/New
        [Authorize(Roles = "Admin")]
        public ActionResult New()
        {
            return View();
        }

        // POST: Keeper/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(Keeper Keeper)
        {
            GetApplicationCookie();
            Debug.WriteLine("the json payload is :");
            //Debug.WriteLine(Keeper.KeeperName);
            //objective: add a new Keeper into our system using the API
            //curl -H "Content-Type:application/json" -d @Keeper.json https://localhost:44324/api/Keeperdata/addKeeper 
            string url = "keeperdata/addkeeper";


            string jsonpayload = jss.Serialize(Keeper);
            Debug.WriteLine(jsonpayload);

            HttpContent content = new StringContent(jsonpayload);
            content.Headers.ContentType.MediaType = "application/json";

            HttpResponseMessage response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("List");
            }
            else
            {
                return RedirectToAction("Error");
            }


        }

        // GET: Keeper/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            GetApplicationCookie();
            string url = "keeperdata/findkeeper/" + id;
            HttpResponseMessage response = client.GetAsync(url).Result;
            KeeperDto selectedKeeper = response.Content.ReadAsAsync<KeeperDto>().Result;
            return View(selectedKeeper);
        }

        // POST: Keeper/Update/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Update(int id, Keeper Keeper)
        {
            GetApplicationCookie();
            string url = "keeperdata/updatekeeper/" + id;
            string jsonpayload = jss.Serialize(Keeper);
            HttpContent content = new StringContent(jsonpayload);
            content.Headers.ContentType.MediaType = "application/json";
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            Debug.WriteLine(content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("List");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // GET: Keeper/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirm(int id)
        {

            string url = "keeperdata/findkeeper/" + id;
            HttpResponseMessage response = client.GetAsync(url).Result;
            KeeperDto selectedKeeper = response.Content.ReadAsAsync<KeeperDto>().Result;
            return View(selectedKeeper);
        }

        // POST: Keeper/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            GetApplicationCookie();
            string url = "keeperdata/deletekeeper/" + id;
            HttpContent content = new StringContent("");
            content.Headers.ContentType.MediaType = "application/json";
            HttpResponseMessage response = client.PostAsync(url, content).Result;

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("List");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }
    }
}
