<%@ Page Language="C#" AutoEventWireup="true" CodeFile="AddCoupon.aspx.cs" Inherits="AddCoupon" Debug="true" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/css/bootstrap.min.css">
        <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/css/bootstrap.min.css">
        <link href="cssjs/style.css" rel="stylesheet" />
    <link href="cssjs/layout.css" rel="stylesheet" />
    <style>
        h3 {
            font-family: Verdana;
            font-size: larger;
            color: #A8A8A8;
            font-weight: bold;
        }

        .linkButton {
            font-size: 12px;
            font-family: Verdana,sans-serif;
            font-weight: bold;
            text-decoration: underline;
            color: #35b5E9;
            background-color: #FFFFFF;
            border-style: none;
            border-color: #FFFFFF;
            text-align: right;
            width: auto;
            background-color: transparent;
        }

        .moveButton {
            position: relative;
            top: 7px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
         <table class=showUserName>
                <tr>
                    <td>
                        <font>Hi, </font><asp:Label ID="UserName" runat="server" Text=""></asp:Label><font>!</font>
                    </td>
                </tr>
            </table>
            <hr />
            <asp:Label ID="ErrorMessageLb" runat="server" Text="" ForeColor="red"></asp:Label>
            <div class="form-group">
                <label for="CouponNameTxt">Coupon CODE</label>
                <asp:TextBox ID="CouponNameTxt" runat="server" class="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label for="CouponValueTxt">Coupon Value % OFF</label>
                <asp:TextBox ID="CouponValueTxt" runat="server" class="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <label for="exampleInputEmail1">Coupon Type</label>
                <asp:DropDownList ID="CouponTypeDp" runat="server">
                    <asp:ListItem Text="Once Per User" Value="1" Selected="True" />
                    <asp:ListItem Text="Unlimited" Value="2" />
                </asp:DropDownList>
            </div>
            <div class="form-group">
                <label for="exampleInputEmail1">Coupon Start Date</label>
                <asp:Calendar ID="StartDateCal" runat="server"></asp:Calendar>
            </div>
            <div class="form-group">
                <label for="exampleInputEmail1">Coupon End Date</label>
                <asp:Calendar ID="EndDateCal" runat="server"></asp:Calendar>
            </div>
            <div class="form-group">
                <asp:Button ID="AddButton" runat="server" Text="Add" OnClick="AddButton_Click" class="btn btn-primary " />
            </div>
        </div>
    </form>
</body>
</html>
