<%@ Application Language="C#" %>
<%@ Import Namespace="MasterAntiqueRepair" %>
<%@ Import Namespace="System.Web.Optimization" %>
<%@ Import Namespace="System.Web.Routing" %>
<%@ Import Namespace="System.Web.Configuration" %>

<script runat="server">

    void Application_Start(object sender, EventArgs e)
    {
        RouteConfig.RegisterRoutes(RouteTable.Routes);
        BundleConfig.RegisterBundles(BundleTable.Bundles);
    }

    void Application_BeginRequest(object sender, EventArgs e)
    {
        // Opt-in HTTPS enforcement - see the RequireHttps comment in Web.config.
        // Skipped for local requests so IIS Express's plain-HTTP dev workflow
        // (no HTTPS binding configured) never breaks, even if this were left on.
        var requireHttps = WebConfigurationManager.AppSettings["RequireHttps"];
        if (string.Equals(requireHttps, "true", StringComparison.OrdinalIgnoreCase)
            && !Request.IsSecureConnection
            && !Request.IsLocal)
        {
            Response.Redirect("https://" + Request.Url.Host + Request.RawUrl, true);
        }
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
