using frontend.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace frontend.Controllers
{
    // Secure this entire controller for Admins
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private LongChauDbEntities db = new LongChauDbEntities();

        // GET: AdminUsers
        public ActionResult Index()
        {
            return View(db.Admins.ToList());
        }

        // GET: AdminUsers/Create
        public ActionResult Create()
        {
            // define roles
            var roles = new List<string> { "Admin", "Accounting", "Sales", "Manager", "Staff" };
            ViewBag.RoleList = new SelectList(roles); // <--- Loaded here

            return View();
        }

        // POST: AdminUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Username,PasswordUser,Role")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                db.Admins.Add(admin);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // --- THE FIX: RELOAD THE LIST IF VALIDATION FAILS ---
            var roles = new List<string> { "Admin", "Accounting", "Sales", "Manager", "Staff" };
            ViewBag.RoleList = new SelectList(roles, admin.Role); // Keep selected value
                                                                  // ---------------------------------------------------

            return View(admin);
        }

        // GET: AdminUsers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Admin admin = db.Admins.Find(id);
            if (admin == null) return HttpNotFound();

            var roles = new List<string> { "Admin", "Accounting", "Sales", "Manager", "Staff" };
            ViewBag.RoleList = new SelectList(roles, admin.Role); // <--- Loaded here

            return View(admin);
        }

        // POST: AdminUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Username,PasswordUser,Role")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                db.Entry(admin).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // --- THE FIX: RELOAD THE LIST IF VALIDATION FAILS ---
            var roles = new List<string> { "Admin", "Accounting", "Sales", "Manager", "Staff" };
            ViewBag.RoleList = new SelectList(roles, admin.Role);
            // ---------------------------------------------------

            return View(admin);
        }

        // GET: AdminUsers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin);
        }

        // POST: AdminUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Admin admin = db.Admins.Find(id);

            // Extra safety: prevent the user from deleting their own account
            if (admin.Username == User.Identity.Name)
            {
                TempData["Error"] = "Bạn không thể xóa tài khoản của chính mình.";
                return RedirectToAction("Index");
            }

            db.Admins.Remove(admin);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}