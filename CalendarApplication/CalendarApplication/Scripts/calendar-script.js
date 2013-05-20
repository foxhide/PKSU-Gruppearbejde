function goto(date) {
    var data = date.split("-");
    if (data.length < 2 || data.length > 3) { return; }
    else if (data.length == 2) {
        $("#DateFrom").val(date + "-01 00:00:00");
        $("#Mode").val("MONTH");
    }
    else {
        $("#DateFrom").val(date + " 00:00:00");
        $("#Mode").val("DAY");
    }
    $("#cal_form").submit();
}