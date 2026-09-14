<%@ Application Language="C#" %>
<%@ Import Namespace="MasterAntiqueRepair" %>
<%@ Import Namespace="System.Web.Optimization" %>
<%@ Import Namespace="System.Web.Routing" %>

<script runat="server">

    void Application_Start(object sender, EventArgs e)
    {
        RouteConfig.RegisterRoutes(RouteTable.Routes);
        BundleConfig.RegisterBundles(BundleTable.Bundles);
    }

    void Application_Error(object sender, EventArgs e)
    {
        // Bad/unknown URLs (extensionless routes with no match, missing pages, etc.)
        // go Home instead of showing a 404 or an error page.
        var httpException = Server.GetLastError() as System.Web.HttpException;
        if (httpException != null && httpException.GetHttpCode() == 404)
        {
            Server.ClearError();
            Response.Redirect("~/");
        }
    }

</script>
