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
    public class SpeciesController : Controller
    {
        private static readonly HttpClient client;
        private JavaScriptSerializer jss = new JavaScriptSerializer();

        static SpeciesController()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                //cookies are manually set in RequestHeader
                UseCookies = false
            };

            client = new HttpClient(handler);
            client.BaseAddress = new Uri("http://zooapp.us-east-2.elasticbeanstalk.com/api/");
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

        // GET: Species/List
        public ActionResult List()
        {
            //objective: communicate with our Species data api to retrieve a list of Speciess
            //curl https://localhost:44324/api/Speciesdata/listSpeciess


            string url = "speciesdata/listspecies";
            HttpResponseMessage response = client.GetAsync(url).Result;

            //Debug.WriteLine("The response code is ");
            //Debug.WriteLine(response.StatusCode);

            IEnumerable<SpeciesDto> Species = response.Content.ReadAsAsync<IEnumerable<SpeciesDto>>().Result;
            //Debug.WriteLine("Number of Speciess received : ");
            //Debug.WriteLine(Speciess.Count());


            return View(Species);
        }

        // GET: Species/Details/5
        public ActionResult Details(int id)
        {
            //objective: communicate with our Species data api to retrieve one Species
            //curl https://localhost:44324/api/Speciesdata/findspecies/{id}

            DetailsSpecies ViewModel = new DetailsSpecies();

            string url = "speciesdata/findspecies/" + id;
            HttpResponseMessage response = client.GetAsync(url).Result;

            Debug.WriteLine("The response code is ");
            Debug.WriteLine(response.StatusCode);

            SpeciesDto SelectedSpecies = response.Content.ReadAsAsync<SpeciesDto>().Result;
            Debug.WriteLine("Species received : ");
            Debug.WriteLine(SelectedSpecies.SpeciesName);

            ViewModel.SelectedSpecies = SelectedSpecies;

            //showcase information about animals related to this species
            //send a request to gather information about animals related to a particular species ID
            url = "animaldata/listanimalsforspecies/" + id;
            response = client.GetAsync(url).Result;
            IEnumerable<AnimalDto> RelatedAnimals = response.Content.ReadAsAsync<IEnumerable<AnimalDto>>().Result;

            ViewModel.RelatedAnimals = RelatedAnimals;

            //show all trivia about this species
            url = "TriviaData/ListTriviasForSpecies/" + id;
            response = client.GetAsync(url).Result;
            IEnumerable<TriviaDto> RelatedTrivias = response.Content.ReadAsAsync<IEnumerable<TriviaDto>>().Result;

            ViewModel.RelatedTrivias = RelatedTrivias;


            return View(ViewModel);
        }

        public ActionResult Error()
        {

            return View();
        }

        // GET: Species/New
        [Authorize(Roles ="Admin")]
        public ActionResult New()
        {
            return View();
        }

        // POST: Species/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(Species Species)
        {
            GetApplicationCookie();
            Debug.WriteLine("the json payload is :");
            //Debug.WriteLine(Species.SpeciesName);
            //objective: add a new Species into our system using the API
            //curl -H "Content-Type:application/json" -d @Species.json https://localhost:44324/api/Speciesdata/addSpecies 
            string url = "speciesdata/addspecies";


            string jsonpayload = jss.Serialize(Species);
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

        // GET: Species/Edit/5

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            GetApplicationCookie();
            string url = "speciesdata/findspecies/" + id;
            HttpResponseMessage response = client.GetAsync(url).Result;
            SpeciesDto selectedSpecies = response.Content.ReadAsAsync<SpeciesDto>().Result;
            return View(selectedSpecies);
        }

        // POST: Species/Update/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Update(int id, Species Species)
        {
            GetApplicationCookie();
            string url = "speciesdata/updatespecies/" + id;
            string jsonpayload = jss.Serialize(Species);
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

        // GET: Species/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirm(int id)
        {
           
            string url = "speciesdata/findspecies/" + id;
            HttpResponseMessage response = client.GetAsync(url).Result;
            SpeciesDto selectedSpecies = response.Content.ReadAsAsync<SpeciesDto>().Result;
            return View(selectedSpecies);
        }

        // POST: Species/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            GetApplicationCookie();
            string url = "speciesdata/deletespecies/" + id;
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
