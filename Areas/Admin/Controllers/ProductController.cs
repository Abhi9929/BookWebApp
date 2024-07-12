using BookWebApp.DataAccess.Repository.IRepository;
using BookWebApp.Models;
using BookWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Protocol;

namespace BookWebApp.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ProductController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnviroment;
		public ProductController(IUnitOfWork db, IWebHostEnvironment webHostEnviroment)
		{
			_unitOfWork = db;
			_webHostEnviroment = webHostEnviroment;
		}
		public IActionResult Index()
		{
			List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

			return View(objProductList);
		}
		public IActionResult Upsert(int? id)
		{
			ProductVM productVM = new()
			{
				CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString()
				}),
				Product = new Product()
			};

			if (id == null || id == 0)
			{
				//create
				// Passing data using ViewModel
				return View(productVM);
			}
			else
			{
				//update
				productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
				return View(productVM);
			}
		}

		[HttpPost]
		public IActionResult Upsert(ProductVM productVM, IFormFile? file)
		{
			// Check if the model state is valid, meaning all validations passed.
			if (ModelState.IsValid)
			{
				string wwwRootPath = _webHostEnviroment.WebRootPath;
				if (file != null)
				{
					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
					string productPath = Path.Combine(wwwRootPath, @"images\product");

					if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
					{
						// delete the old image
						var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

						if(System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
					}

					// upload a new image
					using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
					{
						file.CopyTo(fileStream);
					}

					productVM.Product.ImageUrl = @"\images\product\" + fileName;
				}

				if(productVM.Product.Id == 0)
				{
					// Add the new Product object
					_unitOfWork.Product.Add(productVM.Product);
				}else
				{
					// Update the Change in Product object
					_unitOfWork.Product.Update(productVM.Product);
				}


				// Save the changes to the database.
				_unitOfWork.Save();

				TempData["success"] = "Product created successfully";

				// Redirect to the Index action of this controller to display the list of categories.
				return RedirectToAction("Index");
			}
			// Handling if the ModelState is not valid
			else
			{
				productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString()
				});
				return View(productVM);
			}

			// If the model state is invalid, return to the Create view to display validation errors.
		}

		public IActionResult Delete(int? id)
		{
			if (id == null || id == 0)
			{
				return NotFound();
			}
			Product? productFromDb = _unitOfWork.Product.Get(u => u.Id == id);

			if (productFromDb == null)
			{
				return NotFound();
			}
			return View(productFromDb);
		}

		[HttpPost, ActionName("Delete")]
		public IActionResult DeletePOST(int? id)
		{
			Product? obj = _unitOfWork.Product.Get(u => u.Id == id);
			if (obj == null)
			{
				return NotFound();
			}
			// Remove the product from the DbSet.
			_unitOfWork.Product.Remove(obj);

			// Save the changes to the database to persist the deletion.
			_unitOfWork.Save();

			TempData["success"] = "Product deleted successfully";
			return RedirectToAction("Index");
		}

	}
}
