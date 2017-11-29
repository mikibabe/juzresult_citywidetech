<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ActiveProductReview.aspx.cs" Inherits="ActiveProductReview" Debug="true" %>


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
            
            <div class="form-group">
                <label >Are you sure to activat</label>
                <asp:Button ID="ActiveButton" runat="server" Text="Active" OnClick="ActiveButton_Click" class="btn btn-primary" />
                <asp:Button ID="CancelDelete" runat="server" Text="Cancel" OnClick="CancelDelete_Click" class="btn btn-primary" />

            </div>
            
        </div>
    </form>
</body>
</html>
