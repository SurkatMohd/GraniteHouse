using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraniteHouse.Data;
using GraniteHouse.Models.ViewModel;
using GraniteHouse.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;

namespace GraniteHouse.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostingEnvironment;

        [BindProperty]
        public ProductsViewModel ProductsVM { get; set; }
        public ProductsController(ApplicationDbContext db,IWebHostEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            ProductsVM = new ProductsViewModel()
            {
                ProductTypes = _db.ProductTypes.ToList(),
                SpecialTags = _db.SpecialTags.ToList(),
                Products = new Models.Products()
            };
        }
        public async Task<IActionResult> Index()
        {
            var products = _db.Products.Include(m => m.ProductTypes).Include(m => m.SpecialTags);
            return View(await products.ToListAsync());
        }

        //Get Products Create
        public IActionResult Create()
        {
            return View(ProductsVM);
        }

        //Post Products Create
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            //add oour Product

            if (!ModelState.IsValid)
            {
                return View(ProductsVM);
            }

            _db.Products.Add(ProductsVM.Products);
            await _db.SaveChangesAsync();

            //Image being saved

            //then retrived From the Database
            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var productFromDb = _db.Products.Find(ProductsVM.Products.Id);


            if (files.Count != 0)
            {
                //Image has been Uploded
                var uploads = Path.Combine(webRootPath, SD.ImageFolder);
                var extention = Path.GetExtension(files[0].FileName);

                //and change the file name to the product id
                using (var filestream = new FileStream(Path.Combine(uploads, ProductsVM.Products.Id + extention), FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }

                productFromDb.Image = @"\" + SD.ImageFolder + @"\" + ProductsVM.Products.Id + extention;

            }
            else
            {
                //when use does not upload the image
                var uploads = Path.Combine(webRootPath, SD.ImageFolder + @"\" + SD.DefaultProductImage);
                System.IO.File.Copy(uploads, webRootPath + @"\" + SD.ImageFolder + @"\" + ProductsVM.Products.Id + ".png");
                productFromDb.Image = @"\" + SD.ImageFolder + @"\" + ProductsVM.Products.Id + ".png";
            }
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
    }
}