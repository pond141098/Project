$(function () {
    $("#JsonData").html('<img src="/img/loading.gif" width="50">');
    $.get("/Home/Job", function (JsonResult) {
        setTimeout(function () {

            $("#JsonData").html(JsonResult);
            Datatable();

            $("#JsonData").on("click", ".delete", function (e) {
                var Id = $(this).val();
                $.SmartMessageBox({
                    title: "คำเตือน!",
                    content: "ต้องการลบรายการนี้หรือไม่?",
                    buttons: '[ไม่][ใช่]'
                }, function (ButtonPressed) {
                    if (ButtonPressed == "ใช่") {
                        $.get("/HomeController/DeleteVehicle", { "VehicleId": Id }, function (response) {
                            if (response.valid == true) {
                                $.smallBox({
                                    title: response.message,
                                    content: "<i class='fa fa-clock-o'></i> <i>2 seconds ago...</i>",
                                    color: "#296191",
                                    iconSmall: "fa fa-thumbs-up bounce animated",
                                    timeout: 1000
                                });
                                setTimeout(function () {
                                    window.location.href = "/HomeController/VehicleIndex";
                                }, 1000)
                            } else {
                                $.smallBox({
                                    title: response.message,
                                    content: "<i class='fa fa-clock-o'></i> <i>2 seconds ago...</i>",
                                    color: "#FB0404",
                                    iconSmall: "fa fa-thumbs-up bounce animated",
                                    timeout: 1000
                                });
                            }
                        })
                    }
                    if (ButtonPressed == "ไม่") {

                    }
                });
                e.preventDefault();
            });

            $("#JsonData").on("click", ".edit", function () {
                window.location.href = "/HomeController/FormEditVehicle?VehicleId=" + $(this).val();
            });

        }, 200);
    });


    // add data
    $("#Add").click(function () {
        window.location.href = "/HomeController/FormAddJob";
    });
});


function Datatable() {
    /* User List using datatable ;*/
    var responsiveHelper_dt_basic = undefined;
    var responsiveHelper_datatable_fixed_column = undefined;
    var responsiveHelper_datatable_col_reorder = undefined;
    var responsiveHelper_datatable_tabletools = undefined;

    var breakpointDefinition = {
        tablet: 1024,
        phone: 480
    };

    $('#JsonTable').dataTable({
        "pageLength": 20,
        lengthMenu: [
            [20, 25, 50, -1],
            [20, 25, 50, 'All'],
        ],
        "sDom": "<'dt-toolbar'<'col-xs-12 col-sm-6'f><'col-sm-6 col-xs-12 hidden-xs'l>r>" +
            "t" +
            "<'dt-toolbar-footer'<'col-sm-6 col-xs-12 hidden-xs'i><'col-xs-12 col-sm-6'p>>",
        "autoWidth": true,
        "oLanguage": {
            "sSearch": '<span class="input-group-addon"><i class="glyphicon glyphicon-search"></i></span>'
        },
        "preDrawCallback": function () {
            // Initialize the responsive datatables helper once.
            if (!responsiveHelper_dt_basic) {
                responsiveHelper_dt_basic = new ResponsiveDatatablesHelper($('#JsonTable'), breakpointDefinition);
            }
        },
        "rowCallback": function (nRow) {
            responsiveHelper_dt_basic.createExpandIcon(nRow);
        },
        "drawCallback": function (oSettings) {
            responsiveHelper_dt_basic.respond();
        }
    });
    /* End User List */
}