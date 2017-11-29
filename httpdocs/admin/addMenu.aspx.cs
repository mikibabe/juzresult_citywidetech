using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminMenu;

public partial class admin_addMenu : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            string userId = Session["login_card_id"] as string;
            XmlOperator xo = new XmlOperator();
            if (String.IsNullOrEmpty(userId))
            {
                Response.Write("<meta http-equiv=\"refresh\" content=\"0; URL=default.aspx\">");
                return;
            }
            String userMenuPath = Server.MapPath("~/admin/img/userMenu/");
            userMenuPath = userMenuPath + userId + "\\";
            if (!System.IO.Directory.Exists(userMenuPath))
            {
                //create folder for the user.
                Directory.CreateDirectory(userMenuPath);
            }

            if(!System.IO.File.Exists(userMenuPath + "p.xml"))
            {
                //create xml file
                
                xo.CreateXmlFile(userMenuPath);
            }

            string menuId = Request.Form["menuId"];
            if(menuId != "-1")
            {
                //copy default image to here
                string sourceFile = Server.MapPath("~/admin/img/menu/menuItem/4.png");
                string targetFile = userMenuPath + menuId + ".png";
                File.Copy(sourceFile, targetFile, true);
                xo.AddNewMenu(userMenuPath, menuId, userMenuPath + menuId + ".png");
            }

        }catch(Exception ex)
        {
            
        }
        Response.Redirect("Shortcut.aspx");
    }
}