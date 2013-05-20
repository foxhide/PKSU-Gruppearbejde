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

function showPopup(name) {
    // Show full name
    $('#' + name + '_small').show();
    // Set time
    setTimeout(function () {
        // Show big if small is still visible == cursor is still on event
        if ($('#' + name + '_small').is(":visible")) {
            $('#' + name + '_big').fadeIn();
        }
    }, 700);
}

function hidePopup(name) {
    // Hide both big and small on cursor exit
    $('#' + name + '_big').hide();
    $('#' + name + '_small').hide();
}