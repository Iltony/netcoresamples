using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nelibur.ObjectMapper;
using Newtonsoft.Json;
using OpenGameListWebApp.Data;
using OpenGameListWebApp.Data.Items;
using OpenGameListWebApp.Data.Users;
using OpenGameListWebApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OpenGameListWebApp.Controllers
{
    [Route("api/[controller]")]
    public class ItemsController : BaseController
    {
        #region Private Fields
        private ApplicationDbContext DbContext;
        #endregion Private Fields

        #region Constructor
        public ItemsController(
        ApplicationDbContext context,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager) : base(
        context,
        signInManager,
        userManager)
        { }
        #endregion Constructor

        #region RESTful Conventions

        /// <summary>
        /// GET: api/items
        /// </summary>
        /// <returns>Nothing: this method will raise a HttpNotFound HTTP exception, since we're not supporting this API call.</returns>
        [HttpGet()]
        public IActionResult Get()
        {
            return NotFound(new { Error = "not found" });
        }

        /// <summary>
        /// GET: api/items/{id}
        /// ROUTING TYPE: attribute-based
        /// </summary>
        /// <returns>A Json-serialized object representing a single item.</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            //return new JsonResult(GetSampleItems()
            //.Where(i => i.Id == id)
            //.FirstOrDefault(),
            //DefaultJsonSettings);

            var item = DbContext.Items.Where(i => i.Id == id).FirstOrDefault();

            if (item != null)
                return new JsonResult(TinyMapper.Map<ItemViewModel>(item), DefaultJsonSettings);
            else
                return NotFound(new { Error = String.Format("Item ID {0} has not been found", id) });
        }

        /// <summary>
        /// POST: api/items
        /// </summary>
        /// <returns>Creates a new Item and return it accordingly.</returns> 
        [HttpPost()]
        [Authorize]
        public async Task<IActionResult> Add([FromBody]ItemViewModel ivm)
        {
            if (ivm != null)
            {
                // create a new Item with the client-sent json data
                var item = TinyMapper.Map<Item>(ivm);
                // override any property that could be wise to set from server - side only
                item.CreatedDate = item.LastModifiedDate = DateTime.Now;
                item.UserId = await GetCurrentUserId();
                // add the new item     
                DbContext.Items.Add(item);
                // persist the changes into the Database.
                DbContext.SaveChanges();
                // return the newly-created Item to the client.
                return new JsonResult(TinyMapper.Map<ItemViewModel>(item),
                DefaultJsonSettings);
            }
            // return a generic HTTP Status 500 (Not Found) if the client payload is invalid.
            return new StatusCodeResult(500);
        }

        /// <summary>
        /// PUT: api/items/{id}
        /// </summary>
        /// <returns>Updates an existing Item and return it accordingly.</returns>
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(int id, [FromBody]ItemViewModel ivm)
        {
            if (ivm != null)
            {
                var item = DbContext.Items.Where(i => i.Id ==
                id).FirstOrDefault();
                if (item != null)
                {
                    // handle the update (on per-property basis)
                    item.UserId = ivm.UserId;
                    item.Description = ivm.Description;
                    item.Flags = ivm.Flags;
                    item.Notes = ivm.Notes;
                    item.Text = ivm.Text;
                    item.Title = ivm.Title;
                    item.Type = ivm.Type;
                    // override any property that could be wise to set from server - side only
                    item.LastModifiedDate = DateTime.Now;
                    // persist the changes into the Database.
                    DbContext.SaveChanges();
                    // return the updated Item to the client.
                    return new
                    JsonResult(TinyMapper.Map<ItemViewModel>(item), DefaultJsonSettings);
                }
            }
            // return a HTTP Status 404 (Not Found) if we couldn't find a suitable item.
            return NotFound(new
            {
                Error = String.Format("Item ID {0} has not been found", id)
            });
        }

        /// <summary>
        /// DELETE: api/items/{id}
        /// </summary>
        /// <returns>Deletes an Item, returning a HTTP status 200 (ok) when done.</ returns >
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var item = DbContext.Items.Where(i => i.Id == id).FirstOrDefault();
            if (item != null)
            {
                // remove the item to delete from the DbContext.
                DbContext.Items.Remove(item);
                // persist the changes into the Database.
                DbContext.SaveChanges();
                // return an HTTP Status 200 (OK).
                return new OkResult();
            }
            // return a HTTP Status 404 (Not Found) if we couldn't find a suitable item.
            return NotFound(new
            {
                Error = String.Format("Item ID {0} has not been found", id)
            });
        }
        #endregion

        #region Attribute-based Routing
        /// <summary>
        /// GET: api/items/GetLatest
        /// ROUTING TYPE: attribute-based
        /// </summary>
        /// <returns>An array of a default number of Json-serialized objects representing the last inserted items.</returns>
        [HttpGet("GetLatest")]
        public IActionResult GetLatest()
        {
            return GetLatest(DefaultNumberOfItems);
        }

        /// <summary>
        /// GET: api/items/GetLatest/{n}
        /// ROUTING TYPE: attribute-based
        /// </summary>
        /// <returns>An array of {n} Json-serialized objects representing the last inserted items.</returns>
        [HttpGet("GetLatest/{n}")]
        public IActionResult GetLatest(int n)
        {
            if (n > MaxNumberOfItems) n = MaxNumberOfItems;

            ////var items = GetSampleItems().OrderByDescending(i =>
            ////i.CreatedDate).Take(n);
            ////return new JsonResult(items, DefaultJsonSettings);

            var items = DbContext.Items.OrderByDescending(i => i.CreatedDate).Take(n).ToArray();
            return new JsonResult(ToItemViewModelList(items), DefaultJsonSettings);
        }

        /// <summary>
        /// GET: api/items/GetMostViewed
        /// ROUTING TYPE: attribute-based
        /// </summary>
        /// <returns>An array of a default number of Json-serialized objects representing the items with most user views.</returns>
        [HttpGet("GetMostViewed")]
        public IActionResult GetMostViewed()
        {
            return GetMostViewed(DefaultNumberOfItems);
        }

        /// <summary>
        /// GET: api/items/GetMostViewed/{n}
        /// ROUTING TYPE: attribute-based
        /// </summary>
        /// <returns>An array of {n} Json-serialized objects representing the items with most user views.</returns>
        [HttpGet("GetMostViewed/{n}")]
        public IActionResult GetMostViewed(int n)
        {
            if (n > MaxNumberOfItems) n = MaxNumberOfItems;
            //var items = GetSampleItems().OrderByDescending(i =>
            //i.ViewCount).Take(n);
            //return new JsonResult(items, DefaultJsonSettings);

            var items = DbContext.Items.OrderByDescending(i => i.ViewCount).Take(n).ToArray();
            return new JsonResult(ToItemViewModelList(items), DefaultJsonSettings);
        }

        /// <summary>
        /// GET: api/items/GetRandom
        /// ROUTING TYPE: attribute-based
        /// </summary>
        /// <returns>An array of a default number of Json-serialized objects representing some randomly-picked items. </returns>
        [HttpGet("GetRandom")]
        public IActionResult GetRandom()
        {
            return GetRandom(DefaultNumberOfItems);
        }

        /// <summary>
        /// GET: api/items/GetRandom/{n}
        /// ROUTING TYPE: attribute-based
        /// </summary>
        /// <returns>An array of {n} Json-serialized objects representing some randomly-picked items.</returns>
        [HttpGet("GetRandom/{n}")]
        public IActionResult GetRandom(int n)
        {
            if (n > MaxNumberOfItems) n = MaxNumberOfItems;
            //var items = GetSampleItems().OrderBy(i =>
            //Guid.NewGuid()).Take(n);
            //return new JsonResult(items, DefaultJsonSettings);
            var items = DbContext.Items.OrderBy(i => Guid.NewGuid()).Take(n).ToArray();
            return new JsonResult(ToItemViewModelList(items), DefaultJsonSettings);
        }

        #endregion

        #region Private Members
        /// <summary>
        /// Maps a collection of Item entities into a list of ItemViewModel objects.
        /// </summary>
        /// <param name="items">An IEnumerable collection of item entities</param>
        /// <returns>a mapped list of ItemViewModel objects</returns>
        private List<ItemViewModel> ToItemViewModelList(IEnumerable<Item> items)
        {
            var lst = new List<ItemViewModel>();
            foreach (var i in items)
                lst.Add(TinyMapper.Map<ItemViewModel>(i));
            return lst;
        }

        /// <summary>
        /// Generate a sample array of source Items to emulate a database (for testing purposes only).
        /// </summary>
        /// <param name="num">The number of items to generate: default is 999</param>
        /// <returns>a defined number of mock items (for testing purpose only)</returns>
        private List<ItemViewModel> GetSampleItems(int num = 999)
        {
            List<ItemViewModel> lst = new List<ItemViewModel>();
            DateTime date = new DateTime(2015, 12, 31).AddDays(-num);
            for (int id = 1; id <= num; id++)
            {
                date = date.AddDays(1);
                lst.Add(new ItemViewModel()
                {
                    Id = id,
                    Title = String.Format("Item {0} Title", id),
                    Description = String.Format("This is a sample description for item {0}: Lorem ipsum dolor sit amet.", id),
                    CreatedDate = date,
                    LastModifiedDate = date,
                    ViewCount = num - id
                });
            }
            return lst;
        }

        /// <summary>
        /// Returns a suitable JsonSerializerSettings object that can be used to generate the JsonResult return value for this Controller's methods.
        /// </summary>
        private JsonSerializerSettings DefaultJsonSettings
        {
            get
            {
                return new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented
                };
            }
        }

        /// <summary>
        /// Returns the default number of items to retrieve when using the parameterless overloads of the API methods retrieving item lists.
        /// </summary>
        private int DefaultNumberOfItems
        {
            get
            {
                return 5;
            }
        }

        /// <summary>
        /// Returns the maximum number of items to retrieve when using the API methods retrieving item lists.
        /// </summary>
        private int MaxNumberOfItems
        {
            get
            {
                return 100;
            }
        }

        #endregion
    }
}