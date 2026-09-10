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

            var user = new MasterAntiqueRepair.User
            {
                Name = "Joe",
                Role = 2,
               CreatedAt = DateTime.Now
           };

            db.Users.Add(user);
            db.SaveChanges();

            var allUsers = db.Users.ToList();
            UserLabel.Text = "Count " + allUsers.Count;

            UserName.Text = allUsers[0].Name;

        }
    }
}
