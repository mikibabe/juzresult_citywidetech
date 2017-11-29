using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


public partial class ActiveProductReview : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {


        if (Session["employee_access_level"] == null)
        {
            Response.Write("<meta http-equiv=\"refresh\" content=\"0; URL=default.aspx\">");
            return;
        }

        UserName.Text = Session["name"].ToString();
        string userId = Session["login_card_id"] as string;

        
    }

    protected void ActiveButton_Click(object sender, EventArgs e)
    {
        if(!String.IsNullOrEmpty(Request["id"])){
            ActiveReview(Request["id"]);
        }
    }

    protected void CancelDelete_Click(object sender, EventArgs e)
    {
        Response.Write("<meta http-equiv=\"refresh\" content=\"0; URL=ProductReview.aspx\">");
        return;
    }

    private void ActiveReview(String id){
        //ErrorMessageLb.Text = "";
        string sqlConnString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["b2aSQLConnection"].ToString();
        string sc = " UPDATE product_review SET IsActive = 1 ";
        sc += " WHERE [index]=" + id;

        try
        {
            SqlCommand myCommand = new SqlCommand(sc);
            myCommand.Connection = new SqlConnection(sqlConnString);
            myCommand.Connection.Open();
            myCommand.ExecuteNonQuery();
            myCommand.Connection.Close();
        }
        catch (Exception ex)
        {
            //ErrorMessageLb.Text = ex.Message;
        }

        Response.Write("<meta http-equiv=\"refresh\" content=\"0; URL=ProductReview.aspx\">");
        return;
    }
}
