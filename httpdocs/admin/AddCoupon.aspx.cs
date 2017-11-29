using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


public partial class AddCoupon : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {


        if (Session["login_card_id"] == null)
        {
            Response.Write("<meta http-equiv=\"refresh\" content=\"0; URL=default.aspx\">");
            return;
        }

        UserName.Text = Session["name"].ToString();
        string userId = Session["login_card_id"] as string;
    }

    protected void AddButton_Click(object sender, EventArgs e)
    {
        ErrorMessageLb.Text = "";
        //check input 
        if (String.IsNullOrEmpty(CouponNameTxt.Text))
        {
            ErrorMessageLb.Text = "Please Enter CODE";
            return;
        }
        if (String.IsNullOrEmpty(CouponValueTxt.Text))
        {
            ErrorMessageLb.Text = "Please Enter Value";
            return;
        }
        double couponValue = 100;
        if (!double.TryParse(CouponValueTxt.Text, out couponValue))
        {
            ErrorMessageLb.Text = "Please Enter a right value 1-999";
            return;
        }
        DateTime startDate = StartDateCal.SelectedDate;
        DateTime endDate = EndDateCal.SelectedDate;

        if (startDate > endDate)
        {
            ErrorMessageLb.Text = "Start date must early then end date";
            return;
        }

        string sqlConnString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["b2aSQLConnection"].ToString();

        //check same name.
        string sc = " SELECT CouponId FROM Coupons where CouponName = '"+ CouponNameTxt.Text.ToUpper() +"'";
        try
        {
        	DataSet dstcom = new DataSet();
            SqlDataAdapter myCommand = new SqlDataAdapter(sc, sqlConnString);
            myCommand.Fill(dstcom, "coupon");
            myCommand.Dispose();
            if(dstcom.Tables[0].Rows.Count > 0){
            	ErrorMessageLb.Text = CouponNameTxt.Text.ToUpper() + " already exist.";
            	return;
            }
        }
        catch (Exception ex)
        {
            //ErrorMessageLb.Text = ex.Message;
        }


        sc = " INSERT INTO COUPONS (CouponName, CouponValue, CouponType, StartDate, EndDate, IsClose) ";
        sc += " VALUES";
        sc += " ('" + CouponNameTxt.Text.ToUpper() + "', " + couponValue + " , " + CouponTypeDp.SelectedValue + " , '" + startDate.ToString("yyyy-MM-dd") + "', '" + endDate.ToString("yyyy-MM-dd") + "', 0) ";
        
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
            //Response.Write(sc + "--" + e);
        }

        Response.Write("<meta http-equiv=\"refresh\" content=\"0; URL=CouponList.aspx\">");
        return;

    }
}
