using System;
using System.Linq;

public partial class TestEF : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        using (var db = new MasterAntiqueRepair.TestDbContext())
        {
            var item = new MasterAntiqueRepair.TestItem
            {
                Name = "Hello EF",
                CreatedAt = DateTime.Now
            };
            db.TestItems.Add(item);
            db.SaveChanges();

            var allItems = db.TestItems.ToList();
            ResultLabel.Text = "Count: " + allItems.Count;


            var manager = new MasterAntiqueRepair.UserRole
            {
                UserRoleValue = MasterAntiqueRepair.UserRole.Role.MANAGER,
               
            };

            var user = new MasterAntiqueRepair.User
            {
                Name = "Joe",
                UserRole = manager,
               CreatedAt = DateTime.Now
           };

            db.Users.Add(user);
            db.SaveChanges();

            var allUsers = db.Users.ToList();
            UserLabel.Text = "Count " + allUsers.Count;

            UserName.Text = allUsers[0].Name;

            MasterAntiqueRepair.State s;
        }
    }
}
