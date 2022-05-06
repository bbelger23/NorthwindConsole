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
            logger.Info("Program started");

            try
            {
                string choice;
                do
                {
                    // ask user for choice 
                    Console.WriteLine("1) Display Categories");
                    Console.WriteLine("2) Add Category");
                    Console.WriteLine("3) Display Category and related products");
                    Console.WriteLine("4) Display all Categories and their related products");
                    Console.WriteLine("5) Add Product");
                    Console.WriteLine("6) Edit Product");
                    Console.WriteLine("7) Display all Products (active, discontinued, or both)");
                    Console.WriteLine("8) Display Product");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"Option {choice} selected");
                    if (choice == "1")
                    {
                        //Displays all categories from the database
                        var db = new NWConsole_48_BMBContext();
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
                             var db = new NWConsole_48_BMBContext();
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
                                db.Categories.Add(category);
                                db.SaveChanges();

                                logger.Info("Category added - {name}", category.CategoryName);
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    else if (choice == "3")
                    {
                        //Displays a category and all related products
                        var db = new NWConsole_48_BMBContext();
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
                    else if (choice == "4")
                    {
                        //Displays all categories and their products
                        var db = new NWConsole_48_BMBContext();
                        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
                        foreach (var item in query)
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine($"{item.CategoryName}");
                            foreach (Product p in item.Products)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine($"\t{p.ProductName}");
                            }
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine();
                    }
                    else if (choice == "5")
                    {
                        //Adds a product to the database
                        var db = new NWConsole_48_BMBContext();
                        Product product = new Product();
                        Console.WriteLine("Enter Product name");
                        product.ProductName = Console.ReadLine();
                        Console.WriteLine("Enter Supplier ID");
                        var supplier = GetSupplier(db);
                        if (supplier != null) {
                            product.SupplierId = Convert.ToInt32(Console.ReadLine());
                        }
                        Console.WriteLine("Enter Category ID");
                        var category = GetCategory(db);
                        if (category != null) {
                            product.CategoryId = Convert.ToInt32(Console.ReadLine());
                        }
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

                        db.Products.Add(product);
                        db.SaveChanges();

                        logger.Info("Product added - {name}", product.ProductName);

                    }
                    else if (choice == "6") 
                    {
                        //Edits a product in the database

                    }
                    else if (choice == "7")
                    {
                        //Displays product from the database per the users choice
                        string option;
                        
                        do{
                            var db = new NWConsole_48_BMBContext();
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
                                    Console.WriteLine($"{item.ProductName}");
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
                                    Console.WriteLine($"{item.ProductName}");
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
                                Console.WriteLine($"{item.ProductName}");
                            }
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine();
                            }
                        } while (option.ToLower() != "q");
                        
                    }
                    else if (choice == "8")
                    {
                        //Displays a product

                    }

                } while (choice.ToLower() != "q");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }
        public static Supplier GetSupplier(NWConsole_48_BMBContext db)
        {
            // display all suppliers
            var suppliers = db.Suppliers.OrderBy(s => s.SupplierId);
            foreach (Supplier s in suppliers)
            {
                Console.WriteLine($"{s.SupplierId}: {s.CompanyName}");
            }
            if (int.TryParse(Console.ReadLine(), out int SupplierId))
            {
                Supplier supplier = db.Suppliers.FirstOrDefault(s => s.SupplierId == SupplierId);
                if (supplier != null)
                {
                    return supplier;
                }
            }
            logger.Error("Invalid Supplier Id");
            return null;
        }
        public static Category GetCategory(NWConsole_48_BMBContext db)
        {
            // display all categories
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