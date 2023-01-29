$(function () {
    // Delete
    $(".btn-danger").on("click", function (e) {
        var Id = $(this).val();
        $.SmartMessageBox({
            title: "คำเตือน!",
            content: "ต้องการลบรายการนี้หรือไม่?",
            buttons: '[ไม่][ใช่]'
        }, function (ButtonPressed) {
            if (ButtonPressed == "ใช่") {
                $.get("/Student/DeleteRegisterJob", { "transaction_register_id": Id }, function (response) {
                    if (response.valid == true) {
                        $.smallBox({
                            title: response.message,
                            content: "<i class='fa fa-clock-o'></i> <i>2 seconds ago...</i>",
                            color: "#296191",
                            iconSmall: "fa fa-thumbs-up bounce animated",
                            timeout: 1000
                        });
                        setTimeout(function () {
                            window.location.href = "/Student/HistoryRegister";
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

    // go to FormRegisterJob
    $("#register").click(function () {
        window.location.href = "/Student/FormRegisterJob?transaction_job_id=" + $(this).val("id");
    });

});