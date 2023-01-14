$(function () {

    // go to FormRegisterJob
    $("#register").click(function () {
        window.location.href = "/Student/FormRegisterJob?transaction_job_id=" + $(this).val("id");
    });

});