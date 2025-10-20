using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Web.Mvc;
using frontend.Models;

namespace frontend.Controllers
{
    public class SearchController : Controller
    {
        // GET: /Search?q=term
        public ActionResult Index(string q)
        {
            var results = new List<ProductSearchResult>();

            if (string.IsNullOrWhiteSpace(q))
            {
                return View(results);
            }

            // Read EF connection string from Web.config (EntityConnection)
            var entityConnString = ConfigurationManager.ConnectionStrings["LongChauDbEntities"]?.ConnectionString;
            if (string.IsNullOrEmpty(entityConnString))
            {
                // no connection string configured
                return View(results);
            }

            var ecb = new EntityConnectionStringBuilder(entityConnString);
            var providerConnString = ecb.ProviderConnectionString;

            // Search term for LIKE
            var term = "%" + q.Trim() + "%";

            // Query: search name/brand/shortdescription/description and get primary image if exists
            var sql = @"
SELECT p.Id, p.Name, p.Brand, p.Price, p.ShortDescription,
       pi.Url AS ImageUrl
FROM Products p
LEFT JOIN ProductImages pi ON pi.ProductId = p.Id AND ISNULL(pi.IsPrimary, 0) = 1
WHERE (p.Name LIKE @term OR p.Brand LIKE @term OR p.ShortDescription LIKE @term OR p.Description LIKE @term)
ORDER BY CASE WHEN p.Name LIKE @term THEN 0 ELSE 1 END, p.Name
";

            using (var conn = new SqlConnection(providerConnString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@term", term);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var r = new ProductSearchResult
                        {
                            Id = rdr.GetInt32(rdr.GetOrdinal("Id")),
                            Name = rdr["Name"] as string,
                            Brand = rdr["Brand"] as string,
                            ShortDescription = rdr["ShortDescription"] as string,
                            ImageUrl = rdr["ImageUrl"] as string
                        };
                        var priceIdx = rdr.GetOrdinal("Price");
                        if (!rdr.IsDBNull(priceIdx))
                            r.Price = rdr.GetDecimal(priceIdx);

                        results.Add(r);
                    }
                }
            }

            ViewBag.Query = q;
            return View(results);
        }
    }
}