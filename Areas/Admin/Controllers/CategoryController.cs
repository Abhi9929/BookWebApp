using Microsoft.AspNetCore.Mvc;
using BookWebApp.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using BookWebApp.Models;
using BookWebApp.DataAccess.Repository.IRepository;

namespace BookWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        // The constructor injects the IUnitOfWork instance, allowing for database operations.
        public CategoryController(IUnitOfWork db)
        {
            _unitOfWork = db;
        }
        public IActionResult Index()
        {
            // Retrieve the list of categories from the database
            List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList();

            // Pass the list to the view
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            // Validate if the Name and DisplayOrder are the same.
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display Order cannot match the Name");
            }

            // Check if the model state is valid, meaning all validations passed.
            if (ModelState.IsValid)
            {
                // Add the new Category object to the Categories DbSet.
                _unitOfWork.Category.Add(obj);

                // Save the changes to the database.
                _unitOfWork.Save();

                TempData["success"] = "Category created successfully";

                // Redirect to the Index action of this controller to display the list of categories.
                return RedirectToAction("Index");
            }

            // If the model state is invalid, return to the Create view to display validation errors.
            return View();
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            // Try to find the category by its ID using different methods for illustration purposes.
            // Use the Find method to retrieve the category. It is optimized for primary key lookups.
            Category? categoryFromDb = _unitOfWork.Category.Get(u => u.Id == id);
            /*

			// Alternatively, use FirstOrDefault with a lambda expression to find the category.
			// This is a LINQ query that searches for the category by ID.
			Category? categoryFromDb1 = _unitOfWork.Category.Categories.FirstOrDefault(u => u.Id == id);

			// Another way to find the category is using Where and FirstOrDefault methods together.
			// The Where method filters the results, and FirstOrDefault retrieves the first matching category.
			Category? categoryFromDb2 = _unitOfWork.Category.Categories.Where(u => u.Id == id).FirstOrDefault();
			*/

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryFromDb = _unitOfWork.Category.Get(u => u.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Category? obj = _unitOfWork.Category.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            // Remove the category from the DbSet.
            _unitOfWork.Category.Remove(obj);

            // Save the changes to the database to persist the deletion.
            _unitOfWork.Save();

            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }

    }
}
