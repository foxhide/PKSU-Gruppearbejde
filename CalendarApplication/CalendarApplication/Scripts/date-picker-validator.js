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

    // Writeback to editor fields
    yearInput.val(year);
    monthInput.val(month);
    dayInput.val(day);
    hourInput.val(hour);
    minuteInput.val(minute);

    // Apply compare validation.
    var compareArr = compare.split(",");
    for (var i = 0; i < compareArr.length; i++) {
        if (compareArr[i].charAt(0) == 'g') {
            validateCompare(name, compareArr[i].substring(2), false);
        }
        else if (compareArr[i].charAt(0) == 'l') {
            validateCompare(name, compareArr[i].substring(2), true);
        }
    }

    //Writeback to the hidden field:
    var date_hidden = $("#" + name);
    date_hidden.val(pad(dayInput.val()) + "-" + pad(monthInput.val()) + "-" + yearInput.val() + " "
                        + pad(hourInput.val()) + ":" + pad(minuteInput.val()) + ":00");
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

/* Compares to another DateTimeEditor or to todays date */
function validateCompare(name,other,compGreater) {
    var year = $("#" + name + "_Year");
    var month = $("#" + name + "_Month");
    var day = $("#" + name + "_Day");
    var hour = $("#" + name + "_Hour");
    var minute = $("#" + name + "_Minute");
    var date = new Date();

    // Check if we should compare by today or other date
    if (other != "today") {
        // Parse date from hidden field, this is safe since the value is written by C#
        var arr = $("#" + other).val().split(/\s|:|-/);
        date = new Date(arr[2], arr[1]-1, arr[0], arr[3], arr[4]);
    }

    // Compare greater or less
    if (compGreater) {
        if (year.val() >= date.getFullYear()) {
            year.val(date.getFullYear());
            if (month.val() >= date.getMonth()+1) {
                month.val(date.getMonth()+1);
                if (day.val() >= date.getDate()) {
                    day.val(date.getDate());
                    if (hour.val() >= date.getHours()) {
                        hour.val(date.getHours());
                        if (minute.val() > date.getMinutes()) {
                            minute.val(date.getMinutes());
                        }
                    }
                }
            }
        }
    }
    else {
        if (year.val() <= date.getFullYear()) {
            year.val(date.getFullYear());
            if (month.val() <= date.getMonth()+1) {
                month.val(date.getMonth()+1);
                if (day.val() <= date.getDate()) {
                    day.val(date.getDate());
                    if (hour.val() <= date.getHours()) {
                        hour.val(date.getHours());
                        if (minute.val() < date.getMinutes()) {
                            minute.val(date.getMinutes());
                        }
                    }
                }
            }
        }
    }
}

/* Function for creating datepicker */
function createDatePicker(name, compare, onchange) {
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
        });
    });
}

/* Function for updating times in datepickers */
function getRange(cmpStr) {
    if (cmpStr == "") { return {}; }
    var compareArr = cmpStr.split(",");
    var maxDate = null;
    var minDate = null;
    for (var i = 0; i < compareArr.length; i++) {
        var name = compareArr[i].substring(2);
        if (compareArr[i].charAt(0) == 'g') {
            if (name == "today") {
                minDate = new Date();
            }
            else {
                minDate = $.datepicker.parseDate("dd-mm-yy", $("#" + name).val().substring(0, 10))
            }
        }
        else {
            if (name == "today") {
                maxDate = new Date();
            }
            else {
                maxDate = $.datepicker.parseDate("dd-mm-yy", $("#" + name).val().substring(0, 10))
            }
        }
    }
    return { minDate: minDate, maxDate: maxDate };
}

/* Function for showing date picker */
function showDatePicker(name) {
    $("#" + name).datepicker("show")
}