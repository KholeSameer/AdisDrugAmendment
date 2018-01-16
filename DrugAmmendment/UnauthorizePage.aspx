<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UnauthorizePage.aspx.cs" Inherits="DrugAmmendment.UnauthorizePage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Unauthorized Access</title>
    <link rel="shortcut icon" type="image/png" sizes="96x96" href="/AdisDrugAmendment/Images/adis_logo_webshop.png">
    <style>
        body {
            background-image: url('/AdisDrugAmendment/Images/DrugImg.jpg');
            background-size: cover;
            background-repeat: no-repeat;
        }
        .warningWrapper{
            height:20%;
            padding:1% 25% 5% 29%;
            text-transform:uppercase;
            font-size:10px;
        }
        .warningHolder{
            border:2px solid black;
            display:inline-block;
            padding: 5%;
        }
    </style>
    
</head>
<body>
    <form id="form1" runat="server">
    <div class="warningWrapper">
        <div class="warningHolder">
            <h1>You are not authorized to perform this task.</h1>
            <h2>Please contact your admin.</h2>
        </div>
    </div>
    </form>
</body>
</html>
