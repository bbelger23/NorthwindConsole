using System;
using NLog.Web;
using System.IO;
using System.Linq;
using NorthwindConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NorthwindConsole
{
    class Program
    {
        // create static instance of Logger
        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();
        static void Main(string[] args)
        {
            var db = new NWConsole_48_BMBContext();

            logger.Info("Program started");

            try
            {
                string choice;
                do
                {
                    // ask user for choice 
                    Console.WriteLine("1) Display Categories");
                    Console.WriteLine("2) Add Category");
                    Console.WriteLine("3) Edit Category");
                    Console.WriteLine("4) Display Category and related products");
                    Console.WriteLine("5) Display all Categories and their related products");
                    Console.WriteLine("6) Delete a Category");
                    Console.WriteLine("7) Add Product");
                    Console.WriteLine("8) Edit Product");
                    Console.WriteLine("9) Display all Products (active, discontinued, or both)");
                    Console.WriteLine("10) Display Product");
                    Console.WriteLine("11) Delete a Product");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"Option {choice} selected");
                    if (choice == "1")
                    {
                        //Displays all categories from the database
                        
                        var query = db.Categories.OrderBy(p => p.CategoryName);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{query.Count()} records returned");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName} - {item.Description}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (choice == "2")
                    {
                        //Adds a category to the database
                        Category category = InputCategory(db);
                        
                        if (category != null) {
                            db.Categories.Add(category);
                            db.SaveChanges();
                            logger.Info("Category added - {name}", category.CategoryName);
                        }
                    }
                    else if (choice == "3")
                    {
                        // Edit a category in the database
                        Console.WriteLine("Choose the Category to edit:");
                        var category = GetCategory(db);
                        if (category != null)
                        {
                            // input blog
                            Category UpdatedCategory = InputCategory(db);
                            if (UpdatedCategory != null)
                            {
                                UpdatedCategory.CategoryId = category.CategoryId;
                                db.EditCategory(UpdatedCategory);
                                logger.Info($"Category (id: {category.CategoryId}) updated");
                            }
                        }
                    }
                    else if (choice == "4")
                    {
                        //Displays a category and all related products
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Select the category whose products you want to display:");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");
                        Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"{category.CategoryName} - {category.Description}");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        foreach (Product p in category.Products)
                        {
                            Console.WriteLine(p.ProductName);
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (choice == "5")
                    {
                        //Displays all categories and their products
                        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
                        foreach (var item in query)
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine($"{item.CategoryName}");
                            foreach (Product p in item.Products)
                            {
                                if (p.Discontinued == false) 
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Console.WriteLine($"\t{p.ProductName}");
                                }
                            }
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine();
                    }
                    else if (choice == "6")
                    {
                        // delete category
                        Console.WriteLine("Choose the category to delete:");
                         var category = GetCategory(db);
                        if (category != null)
                        {
                            // delete category
                            db.DeleteCategory(category);
                            logger.Info($"Blog (id: {category.CategoryId}) deleted");
                        }
                    }
                    else if (choice == "7")
                    {
                        //Adds a product to the database
                        Product product = InputProduct(db);
                        if (product != null) {
                            db.AddProduct(product);
                            logger.Info("Product added - {name}", product.ProductName);
                        }
                    }
                    else if (choice == "8") 
                    {
                        //Edits a product in the database
                        Console.WriteLine("Choose a Product to edit");
                        var product = GetProduct(db);
                        if (product !=null) 
                        {
                            Product updatedProduct = InputProduct(db);
                            if (updatedProduct != null) 
                            {
                                updatedProduct.ProductId = product.ProductId;
                                db.EditProduct(updatedProduct);
                                logger.Info($"Product (id: {product.ProductId}) updated");
                            }
                        }

                    }
                    else if (choice == "9")
                    {
                        //Displays product from the database per the users choice
                        string option;
                        
                        do{
                            Console.WriteLine("1) Active products");
                            Console.WriteLine("2) Discontinued products");
                            Console.WriteLine("3) All products");
                            Console.WriteLine("\"q\" to quit");

                            option = Console.ReadLine();
                            logger.Info($"Option {option} selected");

                            if (option == "1") {
                                //Displays all active products
                                var query = db.Products
                                    .Where(p => p.Discontinued == false)
                                    .OrderBy(p => p.ProductId);
                            
                                foreach (var item in query) {
                                    Console.ForegroundColor = ConsoleColor.Magenta;
                                    Console.WriteLine($"{item.ProductId}: {item.ProductName}");
                                }
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine();
                            }
                            else if (option == "2") {
                                //Displays all discontinued products
                                var query = db.Products
                                    .Where(p => p.Discontinued == true)
                                    .OrderBy(p => p.ProductId);
                            
                                    foreach (var item in query) {
                                    Console.ForegroundColor = ConsoleColor.Magenta;
                                    Console.WriteLine($"{item.ProductId}: {item.ProductName}");
                                }
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine();
                            }
                            else if (option == "3") 
                            {
                                //Displays all products
                                var query = db.Products.OrderBy(p => p.ProductId);
                                foreach (var item in query)
                            {
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                Console.WriteLine($"{item.ProductId}: {item.ProductName}");
                            }
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine();
                            }
                        } while (option.ToLower() != "q");
                        
                    }
                    else if (choice == "10")
                    {
                        //Displays a product
                        var query = db.Products.OrderBy(p => p.ProductId);

                        Console.WriteLine("Select the Product you want to display:");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.ProductId}) {item.ProductName}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"ProductId {id} selected");
                        Product product = db.Products.FirstOrDefault(p => p.ProductId == id);
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"Product Id: {product.ProductId} \n Product Name: {product.ProductName} \n Supplier Id: {product.SupplierId} \n Category Id: {product.CategoryId} \n Quantity per Unit: {product.QuantityPerUnit} \n Unit Price: {product.UnitPrice:C2} \n Units in Stock: {product.UnitsInStock} \n Units on Order: {product.UnitsOnOrder} \n Reorder Level: {product.ReorderLevel} \n Discontinued: {product.Discontinued}");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (choice == "11")
                    {
                        // delete product
                        Console.WriteLine("Choose the Product to delete:");
                         var product = GetProduct(db);
                        if (product != null)
                        {
                            // delete product
                            db.DeleteProduct(product);
                            logger.Info($"Blog (id: {product.ProductId}) deleted");
                        }
                    }
                } while (choice.ToLower() != "q");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }

        public static Product InputProduct(NWConsole_48_BMBContext db)
        {
            Product product = new Product();
            Console.WriteLine("Enter Product name");
            product.ProductName = Console.ReadLine();
            Console.WriteLine("Enter Supplier ID"); 
            product.SupplierId = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Enter Category ID");
            product.CategoryId = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Enter Quantity per unit");
            product.QuantityPerUnit = Console.ReadLine();
            Console.WriteLine("Enter price");
            product.UnitPrice = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Enter number of units in stock");
            product.UnitsInStock = Convert.ToInt16(Console.ReadLine());
            Console.WriteLine("Etner number of units on order");
            product.UnitsOnOrder = Convert.ToInt16(Console.ReadLine());
            Console.WriteLine("Enter at what level to reorder");
            product.ReorderLevel = Convert.ToInt16(Console.ReadLine());
            Console.WriteLine("Is the product discontinued? (Enter false for no or true for yes)");
            product.Discontinued = Convert.ToBoolean(Console.ReadLine());

            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(product, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Products.Any(p => p.ProductName == product.ProductName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Product name exists", new string[] { "ProductName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
                return null;
            }
            return product;
        }

        public static Category InputCategory(NWConsole_48_BMBContext db)
        {
            Category category = new Category();
            Console.WriteLine("Enter Category Name:");
            category.CategoryName = Console.ReadLine();
            Console.WriteLine("Enter the Category Description:");
            category.Description = Console.ReadLine();
                        
            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                                
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
                return null;
            }
            return category;
        }

        public static Product GetProduct(NWConsole_48_BMBContext db) {
            var products = db.Products.OrderBy(P => P.ProductId);
            foreach (Product p in products)
            {
                Console.WriteLine($"{p.ProductId}: {p.ProductName}");
            }
            if (int.TryParse(Console.ReadLine(), out int ProductId))
            {
                Product product = db.Products.FirstOrDefault(p => p.ProductId == ProductId);
                if (product != null)
                {
                    return product;
                }
            }
            logger.Error("Invalid Product Id");
            return null;
        }

        public static Category GetCategory(NWConsole_48_BMBContext db) {
            var categories = db.Categories.OrderBy(c => c.CategoryId);
            foreach (Category c in categories)
            {
                Console.WriteLine($"{c.CategoryId}: {c.CategoryName}");
            }
            if (int.TryParse(Console.ReadLine(), out int CategoryId))
            {
                Category category = db.Categories.FirstOrDefault(c => c.CategoryId == CategoryId);
                if (category != null)
                {
                    return category;
                }
            }
            logger.Error("Invalid Category Id");
            return null;
        }
    }
}