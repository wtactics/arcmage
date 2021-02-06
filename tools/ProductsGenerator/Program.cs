using System.Threading.Tasks;

namespace ProductsGenerator
{
    /// <summary>
    /// Creates a Woo-Commerce compatible product list in csv format.
    ///  - exports cards and decks in release candidate states only
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                // change api url, login, password and output file 
                var apiUrl = "https://localhost:5000";
                var login = "";
                var password = "";
                var outputFile = @"c:\temp\products.csv";

                var productGenerator = new ProductGenerator(apiUrl, login, password, outputFile);
                await productGenerator.GenerateProductsCsv();


            }).GetAwaiter().GetResult();
        }
    }
}
