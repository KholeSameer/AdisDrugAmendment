
$(window).load(function () {
        GetClient();
    });


    $(window).bind("pageshow", function () {
        $('#criteriaType').val('');
    $('#criteriaType option').text("Select Criteria Type");
        $('#criteriaType option').attr('selected', 'selected');
    });

    $(document).ready(function () {

        $('#client').change(function () {
            var clientName = $('#client').val();
            if (clientName == '') {
                $('#criteriaType').val('');
                $('#criteriaType option').text("Select Criteria Type");
                $('#criteriaType option').attr('selected', 'selected');
            }
            else {
                GetCriteria(clientName);
            }

        });

        $('#activeDrugsBtn').click(function () {
            var clientName = $('#client').val();
            var criteriaType = $('#criteriaType').val();
            if (clientName != '' && criteriaType != '') {
                GetDrugList(clientName, criteriaType);
    }
            else {
        alert("All fields are mandatory.");
    }
        });

        $('#exportToExcel').click(function () {
            var clientName = $('#client').val();
            var criteriaType = $('#criteriaType').val();
            if (clientName != '' && criteriaType != '') {
                //ExportFunction_Server(clientName, criteriaType);
                exportFunction(clientName, criteriaType);
            }
            else {
                alert("All fields are mandatory.");
            }
        });

        $('#deleteDrugBtn').click(function () {
            var clientName = $('#client').val();
            var criteriaType = $('#criteriaType').val();
            if (clientName != '' && criteriaType != '') {
                var url = "AdisDrugAmendment/Home/DeleteDrugView?clientName=" + clientName + "&criteriaType=" + criteriaType;
                window.location.href = url;
            }
            else {
        alert("All fields are manadatory...!");
    }

        });
    });

    function GetClient() {
        $.ajax({
            type: 'GET',
            contentType: 'application/json; charset=utf-8',
            cache: false,
            url: 'AdisDrugAmendment/Home/PopulateClients',
            dataType: 'json',
            success: function (data) {
                var clientOptions;
                clientOptions = "<option value=''>Select Customer</option>";
                var json = JSON.stringify(data);
                $.each(data, function (i, json) {
                    var ClientNames = json.Text.substring(json.Text.indexOf(".") + 1);
                    ClientNames = ClientNames.charAt(0).toUpperCase() + ClientNames.slice(1);
                    if (ClientNames == 'Ranbaxy') {
                        clientOptions += "<option value='" + json.Text + "'>" + 'Sun Pharma' + "</option>";
                    }
                    else if (ClientNames == 'Mylan_psur') {
                        clientOptions += "<option value='" + json.Text + "'>" + 'Mylan Non-ICSR' + "</option>";
                    }
                    else if (ClientNames == 'Mylan') {
                        clientOptions += "<option value='" + json.Text + "'>" + 'Mylan ICSR' + "</option>";
                    }
                    else {
                        clientOptions += "<option value='" + json.Text + "'>" + ClientNames + "</option>";
                    }
                });
                $('#client').html(clientOptions);
            },
            error: function (err) {
                alert('Ajax call went wrong while fetching the clients list...!');
            }
        });
    }

    function GetCriteria(clientName) {
        $.ajax({
            type: 'GET',
            contentType: 'application/json; charset=utf-8',
            url: 'AdisDrugAmendment/Home/PopulateCriteriaType',
            data: { ClientName: clientName },
            dataType: 'json',
            success: function (data) {
                var criteriaTypeOptions;
                var json = JSON.stringify(data);
                $.each(data, function (i, json) {
                    criteriaTypeOptions += "<option value='" + json.Text + "'>" + json.Text + "</option>";
                });

                $('#criteriaType').html(criteriaTypeOptions);
            },
            error: function (err) {
                alert('Ajax call went wrong while auto-populating the drug list.');
            }
        });
    }

    function GetDrugList(clientName, criteriaType) {
        $.ajax({
            type: 'GET',
            contentType: 'application/json; charset=utf-8',
            url: 'AdisDrugAmendment/Home/GetDrugList',
            data: { ClientName: clientName, CriteriaType: criteriaType },
            dataType: 'json',
            success: function (data) {

                var json = JSON.stringify(data);
                var tableRow = "";
                var dynamicID = clientName.substring(clientName.indexOf(".") + 1);
                var table = "<table id='DrugDetailsTable" + dynamicID + "'>";
                var tableHeader = "<thead><tr><th>Criteria</th><th>Modification Date</th><th>Creation Date</th><th>IsActive</th></tr ></thead >";
                $.each(data, function (i, json) {
                    var date = null;
                    var CreationDate = null;
                    var ModificationDate = null;

                    if (json.CreationDate != null) {
                        date = new Date(parseInt(json.CreationDate.substr(6)));
                        CreationDate = date.getFullYear() + "-" +
                            ("0" + (date.getMonth() + 1)).slice(-2) + "-" +
                            ("0" + date.getDate()).slice(-2) + " " + (date.getHours() < 10 ? '0' : '') + date.getHours() + ":" +
                            (date.getMinutes() < 10 ? '0' : '') + date.getMinutes();
                        
                    }
                    else {
                        CreationDate = "Not Available";
                    }
                    if (json.ModificationDate != null) {
                        date = new Date(parseInt(json.ModificationDate.substr(6)));
                        ModificationDate = date.getFullYear() + "-" +
                            ("0" + (date.getMonth() + 1)).slice(-2) + "-" +
                            ("0" + date.getDate()).slice(-2) + " " + (date.getHours() < 10 ? '0' : '') + date.getHours() + ":" +
                            (date.getMinutes() < 10 ? '0' : '') + date.getMinutes();
                    }
                    else {
                        ModificationDate = "Not Available";
                    }
                    if (json.IsActive == 1) {
                        json.IsActive = "Yes";
                    }
                    else {
                        json.IsActive = "No";
                    }
                    tableRow += '<tr><td>' + json.Criteria + '</td><td>' + ModificationDate + '</td><td>' + CreationDate + '</td><td>' + json.IsActive + '</td></tr>';
                });
                var appendData = table + tableHeader + tableRow;
                appendData += "</table >";
                $('#DrugDetailsDiv').html(appendData);
                //Pagination
                $("#DrugDetailsTable" + dynamicID +"").DataTable();
            },
            error: function (err) {
                alert('Ajax call went wrong while getting the drug list.');
            }
        });
    }

    function exportFunction(clientName, criteriaType) {
        $.ajax({
            type: 'GET',
            contentType: 'application/json; charset=utf-8',
            //url: 'Home/GetDatatable',
            url: 'AdisDrugAmendment/Home/GetActiveDrugList',
            data: { Delivery: clientName, CriteriaType: criteriaType },
            dataType: 'json',
            success: function (data) {
                var json = JSON.stringify(data);
                var dynamicFileName = clientName.substring(clientName.indexOf(".") + 1);
                dynamicFileName = dynamicFileName.charAt(0).toUpperCase() + dynamicFileName.slice(1);
                if (dynamicFileName == "Ranbaxy") {
                    dynamicFileName = "Sunpharma";
                }
                var sheetName = dynamicFileName + "_" + criteriaType;
                dynamicFileName += "_" + criteriaType + "_" + (new Date($.now())).toString().slice(0, -31).slice(4);
                dynamicFileName = dynamicFileName.replace(/ /g,"_");
                $("#exportToExcel").excelexportjs({
                    datatype: 'json',
                    dataset: data,
                    columns: getColumns(data),
                    worksheetName: sheetName,
                    fileName: dynamicFileName
                });
            },
            error: function (err) {
                alert('Ajax call went wrong while exporting the drug to excel sheet.');
            }
        });
    }

    //Latest Code
    /*
* jQuery Client Side Excel Export Plugin Library
* http://techbytarun.com/
*
* Copyright (c) 2013 Batta Tech Private Limited
* https://github.com/tarunbatta/ExcelExportJs/blob/master/LICENSE.txt
*
* March 22, 2017 - Update by Maynard for IE 11.09 up compatability
* 
*/

    (function ($) {
        var $defaults = {
            containerid: null
            , datatype: 'table'
            , dataset: null
            , columns: null
            , returnUri: false
            , worksheetName: "Drug List"
            , encoding: "utf-8"
            , fileName: "fileName"

        };

        var $settings = $defaults;

        $.fn.excelexportjs = function (options) {

            $settings = $.extend({}, $defaults, options);

            var gridData = [];
            var excelData;

            return Initialize();

            function Initialize() {
                var type = $settings.datatype.toLowerCase();

                BuildDataStructure(type);


                switch (type) {
                    case 'table':
                        excelData = Export(ConvertFromTable());
                        break;
                    case 'json':
                        excelData = Export(ConvertDataStructureToTable());
                        break;
                    case 'xml':
                        excelData = Export(ConvertDataStructureToTable());
                        break;
                    case 'jqgrid':
                        excelData = Export(ConvertDataStructureToTable());
                        break;
                }


                if ($settings.returnUri) {
                    return excelData;
                }
                else {

                    if (!isBrowserIE()) {
                        var link = document.createElement('a');
                        link.href = excelData;
                        link.setAttribute('download', $settings.fileName + ".xls");
                        document.body.appendChild(link);
                        link.click();
                        document.body.removeChild(link);
                    }
                    
                }
            }

            function BuildDataStructure(type) {
                switch (type) {
                    case 'table':
                        break;
                    case 'json':
                        gridData = $settings.dataset;
                        break;
                    case 'xml':
                        $($settings.dataset).find("row").each(function (key, value) {
                            var item = {};

                            if (this.attributes != null && this.attributes.length > 0) {
                                $(this.attributes).each(function () {
                                    item[this.name] = this.value;
                                });

                                gridData.push(item);
                            }
                        });
                        break;
                    case 'jqgrid':
                        $($settings.dataset).find("rows > row").each(function (key, value) {
                            var item = {};

                            if (this.children != null && this.children.length > 0) {
                                $(this.children).each(function () {
                                    item[this.tagName] = $(this).text();
                                });

                                gridData.push(item);
                            }
                        });
                        break;
                }
            }

            function ConvertFromTable() {
                var result = $('<div>').append($('#' + $settings.containerid).clone()).html();
                return result;
            }

            function ConvertDataStructureToTable() {
                var result = "<table id='tabledata'>";

                result += "<thead><tr>";
                $($settings.columns).each(function (key, value) {
                    if (this.ishidden != true) {
                        result += "<th";
                        if (this.width != null) {
                            result += " style='width: " + this.width + "'";
                        }
                        result += ">";
                        result += this.headertext;
                        result += "</th>";
                    }
                });
                result += "</tr></thead>";

                result += "<tbody>";
                $(gridData).each(function (key, value) {
                    result += "<tr>";
                    $($settings.columns).each(function (k, v) {
                        if (value.hasOwnProperty(this.datafield)) {
                            if (this.ishidden != true) {
                                result += "<td";
                                if (this.width != null) {
                                    result += " style='width: " + this.width + "'";
                                }
                                result += ">";
                                result += value[this.datafield];
                                result += "</td>";
                            }
                        }
                    });
                    result += "</tr>";
                });
                result += "</tbody>";

                result += "</table>";

                return result;
            }

            function Export(htmltable) {

                if (isBrowserIE()) {

                    exportToExcelIE(htmltable);
                }
                else {
                    var excelFile = "<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:x='urn:schemas-microsoft-com:office:excel' xmlns='http://www.w3.org/TR/REC-html40'>";
                    excelFile += "<head>";
                    excelFile += '<meta http-equiv="Content-type" content="text/html;charset=' + $defaults.encoding + '" />';
                    excelFile += "<!--[if gte mso 9]>";
                    excelFile += "<xml>";
                    excelFile += "<x:ExcelWorkbook>";
                    excelFile += "<x:ExcelWorksheets>";
                    excelFile += "<x:ExcelWorksheet>";
                    excelFile += "<x:Name>";
                    excelFile += "{worksheet}";
                    excelFile += "</x:Name>";
                    excelFile += "<x:WorksheetOptions>";
                    excelFile += "<x:DisplayGridlines/>";
                    excelFile += "</x:WorksheetOptions>";
                    excelFile += "</x:ExcelWorksheet>";
                    excelFile += "</x:ExcelWorksheets>";
                    excelFile += "</x:ExcelWorkbook>";
                    excelFile += "</xml>";
                    excelFile += "<![endif]-->";
                    excelFile += "</head>";
                    excelFile += "<body>";
                    excelFile += htmltable.replace(/"/g, '\'');
                    excelFile += "</body>";
                    excelFile += "</html>";

                    var uri = "data:application/vnd.ms-excel;base64,";
                    var ctx = { worksheet: $settings.worksheetName, table: htmltable };

                    return (uri + base64(format(excelFile, ctx)));
                }
            }

            function base64(s) {
                return window.btoa(unescape(encodeURIComponent(s)));
            }

            function format(s, c) {
                return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; });
            }

            function isBrowserIE() {
                var msie = !!navigator.userAgent.match(/Trident/g) || !!navigator.userAgent.match(/MSIE/g);
                if (msie > 0) {  // If Internet Explorer, return true
                    return true;
                }
                else {  // If another browser, return false
                    return false;
                }
            }

            function exportToExcelIE(table) {


                var el = document.createElement('div');
                el.innerHTML = table;

                var tab_text = "<table border='2px'><tr bgcolor='#87AFC6'>";
                var textRange; var j = 0;
                var tab;


                if ($settings.datatype.toLowerCase() == 'table') {
                    tab = document.getElementById($settings.containerid);  // get table              
                }
                else {
                    tab = el.children[0]; // get table
                }



                for (j = 0; j < tab.rows.length; j++) {
                    tab_text = tab_text + tab.rows[j].innerHTML + "</tr>";
                    //tab_text=tab_text+"</tr>";
                }

                tab_text = tab_text + "</table>";
                tab_text = tab_text.replace(/<A[^>]*>|<\/A>/g, "");//remove if u want links in your table
                tab_text = tab_text.replace(/<img[^>]*>/gi, ""); // remove if u want images in your table
                tab_text = tab_text.replace(/<input[^>]*>|<\/input>/gi, ""); // reomves input params

                var ua = window.navigator.userAgent;
                var msie = ua.indexOf("MSIE ");
                var sa;

                if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./))      // If Internet Explorer
                {
                    name = $settings.fileName;
                    name += ".xls";
                    //console.log(name);
                    winObj = window.open("txt/html", "replace");
                    winObj.document.write(tab_text);
                    winObj.document.close();
                    winObj.document.execCommand("SaveAs", true, "DrugList.xls");
                }
                else {
                    sa = window.open('data:application/vnd.ms-excel,' + encodeURIComponent(tab_text));
                }


                return (sa);


            }

        };
    })(jQuery);


    //get columns
    function getColumns(paramData) {

        var header = [];
        $.each(paramData[0], function (key, value) {
            //console.log(key + '==' + value);
            var obj = {}
            obj["headertext"] = key;
            obj["datatype"] = "string";
            obj["datafield"] = key;
            header.push(obj);
        });
        return header;

    }
//End of Export to Excel



    function ExportFunction_Server(clientName, criteriaType) {
        $.ajax({
            type: 'GET',
            contentType: 'application/json; charset=utf-8',
            url: 'AdisDrugAmendment/Home/ExportUsingDatatable',
            data: { Delivery: clientName, CriteriaType: criteriaType },
            dataType: 'json',
            success: function (data) {
                if (data == "Error") {
                    alert("Error while exporting drug list in excel format.");
                }
                else {
                    alert("Exported file has been stored at location : " + data);
                    var a = new ActiveXObject("Shell.Application");
                    a.open(data);
                }
                
            },
            error: function (err) {
                alert('Ajax call went wrong while exporting the drug to excel sheet.');
            }
        });
    }