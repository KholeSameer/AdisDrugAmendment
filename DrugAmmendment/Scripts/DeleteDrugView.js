﻿$(document).ready(function () {
    var data = $('#mappedData').html();
    $("#hiddenMappedClient").val(data);
    $(function () {
        $("#criteria").autocomplete({
            source: function (request, response) {
                var param = { criteria: $('#criteria').val(), delivery: $('#client').val(), criteriaType: $('#criteriaType').val() };
                $.ajax({
                    url: "GetAutoCriteria",
                    data: JSON.stringify(param),
                    dataType: "json",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    dataFilter: function (data) { return data; },
                    success: function (data) {
                        response($.map(data, function (item) {
                            return {
                                label: item,
                                value: item
                            };
                        }));

                    },
                    error: function (err) {
                        alert("Ajax call went wrong while auto-populating the criteria.");
                    }
                });
            },
            minLength: 2
        });
    });
})