using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;


public partial class AutoComplete : System.Web.UI.Page
{

    DataSet dstcom = new DataSet();
    string sqlConnString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["b2aSQLConnection"].ToString();

    protected void Page_Load(object sender, EventArgs e)
    {
        const string m_sCompanyName = "cyetekcity";
        int m_nDealerLevel = -1;
        if(Session[m_sCompanyName + "dealer_level"] != null)
            int.TryParse(Session[m_sCompanyName + "dealer_level"].ToString(), out m_nDealerLevel);



        string value = "";//"{suggestions: [{ value: 'United Arab Emirates', data: 'AE' },{ value: 'United Kingdom',  data: 'UK' },{ value: 'United States',        data: 'US' }]}";
        TheSuggestions s = new TheSuggestions();
        s.suggestions = new List<ValueData>();

        
        string sc = " SELECT TOP 10 name,cat ,s_cat, ss_cat";
        sc += "  FROM product";
        string queryString = "a";
        if(!String.IsNullOrEmpty(Request["query"])){
        	queryString = Request["query"];
            // if(queryString.Length <= 2){
            //     return;
            // }
        }
        sc += " WHERE name like '%"+ queryString +"%'";
        try
        {
            SqlDataAdapter myCommand = new SqlDataAdapter(sc, sqlConnString);
            myCommand.Fill(dstcom, "review");
            
            if(dstcom.Tables["review"].Rows.Count > 0){
            	for(int i = 0; i < dstcom.Tables["review"].Rows.Count; i++){
            		ValueData vd = new ValueData();
		        	vd.value = dstcom.Tables["review"].Rows[i]["name"].ToString();
		        	vd.data = dstcom.Tables["review"].Rows[i]["name"].ToString();
                    if(ShowMenu(dstcom.Tables["review"].Rows[i]["cat"].ToString(),
                        dstcom.Tables["review"].Rows[i]["s_cat"].ToString(),
                        dstcom.Tables["review"].Rows[i]["ss_cat"].ToString()))
                    {
                        s.suggestions.Add(vd);
                    }
		        	
            	}
            }

            
            myCommand.Dispose();

        }
        catch (Exception ex)
        {
            Response.Write(ex.Message);
        }


        value = JsonConvert.SerializeObject(s);
        Response.Clear();
        Response.ContentType = "application/json; charset=utf-8";
        Response.Write(value);
        Response.End();
    }

    bool ShowMenu(string c, string s, string ss){
        bool returnValue = false;
        if(s == ""){
            s = "zzzOthers";
        }
        if(ss == ""){
            ss = "zzzOthers";
        }

        int show_level = GetCatagoryShowLevel(c, s, ss);
        int userLevel = 0;
        if(Session["card_type"] != null){
            //if logined
            int.TryParse(Session["card_type"].ToString(), out userLevel);

            if(userLevel == 0){ //person.
                if(show_level == 1 || show_level == 3){
                    returnValue = true;
                }
            }else if(userLevel == 1){ //customer
                if(show_level == 1 || show_level == 3){
                    returnValue = true;
                }
            }else if(userLevel == 2){ //dealer
                if(show_level == 2 || show_level == 3){
                    returnValue = true;
                }
            }else{// if other type of user show all like admin.
                returnValue = true;
            }
        }else{ //if nobody login
             if(show_level == 1 || show_level == 3){
                 returnValue = true;
             }
        }
        return returnValue;
    }

    int GetCatagoryShowLevel(string c, string s, string ss){
        int showLevel = 4;
        string sc = "select show_level from main_catalog where ";
        sc += " cat = '"+c+"'";
        // if(s != ""){
        //     sc += " and s_cat = '"+s+"'";
        // }else{
        //     sc += " and s_cat = 'zzzOthers'";
        // }
        // if(ss != ""){
        //      sc += " and ss_cat = '"+ss+"'";
        // }else{
        //     sc += " and ss_cat = 'zzzOthers'";
        // }
        DataSet ddss = new DataSet();

        try
        {
            SqlDataAdapter myCommand = new SqlDataAdapter(sc, sqlConnString);
            int rows =  myCommand.Fill(ddss, "ddss");
            if(rows > 0)
            {
                string show_level_string = ddss.Tables["ddss"].Rows[0]["show_level"].ToString();
                showLevel = int.Parse(show_level_string);
            }
        }
        catch(Exception ex) 
        {
            showLevel = 4;
        }

        return showLevel;
    }
}

public class TheSuggestions{
	List<ValueData> _suggestions;
	public List<ValueData> suggestions{
		get { return _suggestions; }
    	set { _suggestions = value; }
    }
}

public class ValueData{
	string _value="";
	public string value{
		get { return _value; }
    	set { _value = value; }
    }

	string _data="";
	public string data{
		get { return _data; }
    	set { _data = value; }
	}
}