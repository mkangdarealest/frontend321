using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Web.Mvc;

// Assumes you have an "Admin" area
[Area("Admin")]
// [Authorize(Roles = "Admin")] // Recommended: Secure your admin controller
public class AdminCustomersController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminCustomersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Admin/AdminCustomers
    // Lists all customers
    public async Task<IActionResult> Index()
    {
        var customers = await _context.Customers.ToListAsync();
        return View(customers);
    }

    // GET: /Admin/AdminCustomers/OrderHistory/5
    // Shows order history for a single customer
    public async Task<IActionResult> OrderHistory(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // Find the customer and eagerly load their 'Orders' collection
        var customer = await _context.Customers
            .Include(c => c.Orders) // This is the key part
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            return NotFound();
        }

        // Pass the single customer (with their orders) to the view
        return View(customer);
    }
}