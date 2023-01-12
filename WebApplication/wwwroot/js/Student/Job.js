$(function () {
    $.get("/Student/getJob", function (JsonResult) {
        setTimeout(function () {

            $("#JsonData").html(JsonResult);

        }, 200);
    });


    // add data
    $("#Add").click(function () {
        window.location.href = "/Student/FormRegisterJob";
    });

});
