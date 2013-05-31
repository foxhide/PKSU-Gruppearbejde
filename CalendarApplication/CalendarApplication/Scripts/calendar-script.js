/* Function for switching between pages in lists */
function gotoOtherPage(from) {
    $('#EventFrom').val(from);
    $('#list_form').submit();
}

/* Function for sorting */
function sortBy(eventOrder) {
    var old = $('#Order').val();
    if (old == eventOrder) {
        var desc = $('#Descending').val().toLowerCase() == "true";
        $('#Descending').val(!desc);
    }
    else {
        $('#Descending').val(false);
    }
    $('#Order').val(eventOrder);
    $('#EventFrom').val(0);
    $('#list_form').submit();
}

/* Function for navigating calendar (month and days).
   If date is of format yyyy-MM, the calendar month is
   shown. If it has format yyyy-MM-dd, the calendar day
   is shown. */
function goto(date) {
    var data = date.split("-");
    if (data.length < 2 || data.length > 3) { return; }
    else if (data.length == 2) {
        $("#Date").val(date + "-01 00:00:00");
        $("#Mode").val("MONTH");
    }
    else {
        $("#Date").val(date + " 00:00:00");
        $("#Mode").val("DAY");
    }
    $("#cal_form").submit();
}

/* Functions for showing/hideing event pop-ups in the calendar. */
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
    $('#' + name + '_small').hide();
}

function hidePopupBig(name) {
    $('#' + name + '_big').hide();
}

function changeCheckboxes(element) {
    for (var i = 0; $("#Filter_Eventtypes_" + i + "__Selected").length > 0 && $("#Filter_Eventtypes_" + i + "__Selected") != null; i++) {
        $("#Filter_Eventtypes_"+i+"__Selected").attr("checked", element.checked);
    }
}

function checkBoxes() {
    var checked = 0;
    var unchecked = 0;
    for (var i = 0; $("#Filter_Eventtypes_" + i + "__Selected").length > 0 && $("#Filter_Eventtypes_" + i + "__Selected") != null; i++) {
        if ($("#Filter_Eventtypes_" + i + "__Selected").attr("checked")) {
            checked++;
        }
        else {
            unchecked++;
        }
    }
    $("#checkAll").attr("checked", checked > unchecked);
}