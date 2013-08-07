/* Function for validating a date input */
function validateDate(name, compare) {
    var yearInput = $("#" + name + "_Year");
    var year = yearInput.val();
    var monthInput = $("#" + name + "_Month");
    var month = monthInput.val();
    var dayInput = $("#" + name + "_Day");
    var day = dayInput.val();
    var hourInput = $("#" + name + "_Hour");
    var hour = hourInput.val();
    var minuteInput = $("#" + name + "_Minute");
    var minute = minuteInput.val();

    // Minutes
    if (minute < 0) {
        hour--;
        minute = 59;
    }
    else if (minute > 59) {
        hour++;
        minute = 0;
    }

    // Hours
    if (hour < 0) {
        day--;
        hour = 23;
    }
    else if (hour > 23) {
        day++;
        hour = 0;
    }

    // Days
    if (month > 0 && month <= 12) {
        if (day < 1) {
            month--;
            day = daysInMonth(month, year);
        }
        else if (day > daysInMonth(month,year)) {
            month++;
            day = 1;
        }
    }

    // Months
    if (month <= 0) {
        month = 12;
        year--;
    }
    else if (month > 12) {
        month = 1;
        year++;
    }

    // Years
    if (year < 1) {
        year = 1;
        month = 1;
        day = 1;
        hour = 0;
        minute = 0;
    }
    else if (year > 9999) {
        year = 9999;
        month = 12;
        day = 31;
        hour = 0;
        minute = 0;
    }

    var newDate = new Date(year, month - 1, day, hour, minute);

    // Apply compare validation.
    var compareArr = compare.split(",");
    for (var i = 0; i < compareArr.length; i++) {
        var otherId = compareArr[i].substring(2);
        if (compareArr[i].charAt(0) == 'g') {
            if (otherId == "today") {
                newDate = newDate > new Date() ? newDate : new Date();
            }
            else {
                var otherDate = getDate(otherId);
                otherDate = newDate > otherDate ? otherDate : newDate;
                setDate(otherId,otherDate);
            }
        }
        else if (compareArr[i].charAt(0) == 'l') {
            if (otherId == "today") {
                newDate = newDate < new Date() ? newDate : new Date();
            }
            else {
                var otherDate = getDate(otherId);
                otherDate = newDate < otherDate ? otherDate : newDate;
                setDate(otherId, otherDate);
            }
        }
    }

    // Writeback to editor and hidden fields
    setDate(name, newDate);
}

/* Padding function: pads a single digit with a leading zero */
function pad(val) {
    if (val < 10) {
        return "0" + val;
    }
    else {
        return "" + val;
    }
}

/* Returns number of days in a given month */
function daysInMonth(month,year) {
    if (month < 1 || month > 12) { return 31; }
    else {
        return (new Date(year, month, 0)).getDate();
    }
}

/* Sets the date of a datetime editor */
function setDate(id, date) {
    $("#" + id + "_Year").val(date.getFullYear());
    $("#" + id + "_Month").val(date.getMonth() + 1);
    $("#" + id + "_Day").val(date.getDate());
    $("#" + id + "_Hour").val(date.getHours());
    $("#" + id + "_Minute").val(date.getMinutes());

    var date_hidden = $("#" + id);
    date_hidden.val(pad(date.getDate()) + "-" + pad(date.getMonth()) + "-" + date.getFullYear() + " "
                        + pad(date.getHours()) + ":" + pad(date.getMinutes()) + ":00");
}

/* Use only if date is valid!!! */
function getDate(id) {
    var year = $("#" + id + "_Year").val();
    var month = $("#" + id + "_Month").val();
    var day = $("#" + id + "_Day").val();
    var hour = $("#" + id + "_Hour").val();
    var minute = $("#" + id + "_Minute").val();
    return new Date(year, month - 1, day, hour, minute);
}

/* Function for creating datepicker */
function createDatePicker(name, compare, onchange, dateString) {
    // Parse selected date, given as argument
    var dateArr = dateString.split("-");
    var date = new Date(parseInt(dateArr[0], 10), parseInt(dateArr[1], 10) - 1, parseInt(dateArr[2], 10));

    // Create jQuery datepicker
    $(function () {
        $("#" + name + "_picker").datepicker({
            beforeShow: function (input, inst) {
                return getRange(compare);
            },
            onSelect: function (dateText, inst) {
                var date = new Date(dateText);
                $("#" + name + "_Year").val(date.getFullYear());
                $("#" + name + "_Month").val(date.getMonth() + 1);
                $("#" + name + "_Day").val(date.getDate());
                validateDate(name, compare);
                if (onchange) { onchange(); } // Call the onchange function
            }
        }).datepicker("setDate",date); // Set the date.
    });
}

/* Function for updating times in datepickers (only .today will work now, as other dates are updated!) */
function getRange(cmpStr) {
    if (cmpStr == "") { return {}; }
    var compareArr = cmpStr.split(",");
    var maxDate = null;
    var minDate = null;
    for (var i = 0; i < compareArr.length; i++) {
        if (compareArr[i] == 'g.today') {
            minDate = new Date();
        }
        else if (compareArr[i] == 'l.today') {
            maxDate = new Date();
        }
    }
    return { minDate: minDate, maxDate: maxDate };
}

/* Function for showing date picker */
function showDatePicker(name) {
    $("#" + name).datepicker("show")
}

/* Function for disabling/enabling date picker */
function setDateTimeEditorEnabled(id, enabled) {
    if (enabled) {
        $("#" + id + "_Year").removeAttr('disabled');
        $("#" + id + "_Month").removeAttr('disabled');
        $("#" + id + "_Day").removeAttr('disabled');
        $("#" + id + "_Hour").removeAttr('disabled');
        $("#" + id + "_Minute").removeAttr('disabled');
        $("#" + id + "_picker_button").show();
    }
    else {
        $("#" + id + "_Year").attr('disabled', 'disabled');
        $("#" + id + "_Month").attr('disabled', 'disabled');
        $("#" + id + "_Day").attr('disabled', 'disabled');
        $("#" + id + "_Hour").attr('disabled', 'disabled');
        $("#" + id + "_Minute").attr('disabled', 'disabled');
        $("#" + id + "_picker_button").hide();
    }
}